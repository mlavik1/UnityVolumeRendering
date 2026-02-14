using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Rendering;
using LightType = UnityEngine.LightType;

namespace UnityVolumeRendering
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(VolumeRenderedObject))]
    public class ShadowVolumeManager : MonoBehaviour
    {
        private const int NUM_DISPATCH_CHUNKS = 5;
        private const int dispatchCount = NUM_DISPATCH_CHUNKS * NUM_DISPATCH_CHUNKS * NUM_DISPATCH_CHUNKS;

        private VolumeRenderedObject volumeRenderedObject = null;
        private RenderTexture shadowVolumeTexture = null;
        private Vector3 lightDirection;
        private bool initialised = false;
        private ComputeShader shadowVolumeShader;
        private int handleMain;
        private int currentDispatchIndex = 0;
        private float cooldown = 1.0f;
        private double lastUpdateTimeEditor = 0.0f;
        private bool isDirty = true;

        private void Awake()
        {
            if (!SystemInfo.supportsComputeShaders)
            {
                Debug.LogError("Shadow volumes not supported on this platform (SystemInfo.supportsComputeShaders == false)");
                DestroyImmediate(this);
            }
        }

        private void Start()
        {
            if (!initialised)
                Initialise();
        }
        
        private void OnValidate()
        {
            if (!initialised)
                Initialise();
        }

        private void Update()
        {
            HandleUpdate();
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.update += OnEditorUpdate;
#endif
            if (volumeRenderedObject != null)
            {
                volumeRenderedObject.meshRenderer.sharedMaterial.EnableKeyword("SHADOWS_ON");
            }
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.update -= OnEditorUpdate;
#endif
            currentDispatchIndex = 0;
            if (volumeRenderedObject != null)
            {
                volumeRenderedObject.meshRenderer.sharedMaterial.DisableKeyword("SHADOWS_ON");
            }
        }

        private void OnEditorUpdate()
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                if (isDirty || (UnityEditor.EditorApplication.timeSinceStartup - lastUpdateTimeEditor > 0.02f))
                {
                    HandleUpdate();
                    UnityEditor.EditorUtility.SetDirty(UnityEditor.SceneView.lastActiveSceneView);
                }
            }
#endif
        }

        private void Initialise()
        {
            Debug.Log("Initialising shadow volume buffers");
            volumeRenderedObject = GetComponent<VolumeRenderedObject>();
            Debug.Assert(volumeRenderedObject != null);

            Vector3Int shadowVolumeDimensions = new Vector3Int(512, 512, 512);
            
            shadowVolumeTexture = new RenderTexture(shadowVolumeDimensions.x, shadowVolumeDimensions.y, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
            shadowVolumeTexture.dimension = TextureDimension.Tex3D;
            shadowVolumeTexture.volumeDepth = shadowVolumeDimensions.z;
            shadowVolumeTexture.enableRandomWrite = true;
            shadowVolumeTexture.wrapMode = TextureWrapMode.Clamp;
            shadowVolumeTexture.Create();

            volumeRenderedObject.meshRenderer.sharedMaterial.SetTexture("_ShadowVolume", shadowVolumeTexture);
            volumeRenderedObject.meshRenderer.sharedMaterial.SetVector("_ShadowVolumeTextureSize", new Vector3(shadowVolumeDimensions.x, shadowVolumeDimensions.y, shadowVolumeDimensions.z));

            shadowVolumeShader = Resources.Load("ShadowVolume") as ComputeShader;
            handleMain = shadowVolumeShader.FindKernel("ShadowVolumeMain");
            if (handleMain < 0)
            {
                Debug.LogError("Shadow volume compute shader initialization failed.");
            }
            initialised = true;
        }

        private void HandleUpdate()
        {
#if UNITY_EDITOR
            lastUpdateTimeEditor = UnityEditor.EditorApplication.timeSinceStartup;
#endif
            // Dirty hack for broken data texture
            // TODO: Investigate issue with calling VolumeDataset.GetDataTexture from first update in editor after leaving play mode
            if (cooldown > 0.0f)
            {
                cooldown -= Time.deltaTime;
                return;
            }

            if (volumeRenderedObject.GetRenderMode() != RenderMode.DirectVolumeRendering)
            {
                return;
            }

            lightDirection = -GetLightDirection(volumeRenderedObject);

            if (currentDispatchIndex == 0)
            {
                ConfigureCompute();
            }
            if (currentDispatchIndex < dispatchCount)
            {
                DispatchComputeChunk();
                currentDispatchIndex++;
            }
            if (currentDispatchIndex == dispatchCount)
            {
                currentDispatchIndex = 0;
            }
            isDirty = false;
        }

        private void ConfigureCompute()
        {
            VolumeDataset dataset = volumeRenderedObject.dataset;
            
            Texture3D dataTexture = dataset.GetDataTexture();
            
#if UNITY_2020_2_OR_NEWER
            if (volumeRenderedObject.GetCubicInterpolationEnabled())
                shadowVolumeShader.EnableKeyword("CUBIC_INTERPOLATION_ON");
            else
                shadowVolumeShader.DisableKeyword("CUBIC_INTERPOLATION_ON");
#endif

            shadowVolumeShader.SetVector("_TextureSize", new Vector3(dataset.dimX, dataset.dimY, dataset.dimZ));
            shadowVolumeShader.SetInts("_Dimension", new int[] { shadowVolumeTexture.width, shadowVolumeTexture.height, shadowVolumeTexture.volumeDepth });
            shadowVolumeShader.SetTexture(handleMain, "_VolumeTexture", dataTexture);
            shadowVolumeShader.SetTexture(handleMain, "_TFTex", volumeRenderedObject.transferFunction.GetTexture());
            shadowVolumeShader.SetTexture(handleMain, "_ShadowVolume", shadowVolumeTexture);
            shadowVolumeShader.SetVector("_LightDirection", lightDirection);

            Material volRendMaterial = volumeRenderedObject.meshRenderer.sharedMaterial;
            shadowVolumeShader.SetFloat("_MinVal", volRendMaterial.GetFloat("_MinVal"));
            shadowVolumeShader.SetFloat("_MaxVal", volRendMaterial.GetFloat("_MaxVal"));

#if UNITY_2020_2_OR_NEWER
            if (volRendMaterial.IsKeywordEnabled("CROSS_SECTION_ON"))
            {
                shadowVolumeShader.EnableKeyword("CROSS_SECTION_ON");
                shadowVolumeShader.SetMatrixArray("_CrossSectionMatrices", volRendMaterial.GetMatrixArray("_CrossSectionMatrices"));
                shadowVolumeShader.SetFloats("_CrossSectionTypes", volRendMaterial.GetFloatArray("_CrossSectionTypes"));
                shadowVolumeShader.SetInt("_NumCrossSections", 1);
            }
            else
            {
                shadowVolumeShader.DisableKeyword("CROSS_SECTION_ON");
            }
#endif
            if (volumeRenderedObject != null)
            {
                volumeRenderedObject.meshRenderer.sharedMaterial.EnableKeyword("SHADOWS_ON");
            }
        }

        private void DispatchComputeChunk()
        {
            int threadGroupsX = (shadowVolumeTexture.width / NUM_DISPATCH_CHUNKS + 7) / 8;
            int threadGroupsY = (shadowVolumeTexture.height / NUM_DISPATCH_CHUNKS + 7) / 8;
            int threadGroupsZ = (shadowVolumeTexture.volumeDepth / NUM_DISPATCH_CHUNKS + 7) / 8;
            int dispatchChunkWidth = shadowVolumeTexture.width / NUM_DISPATCH_CHUNKS;
            int dispatchChunkHeight = shadowVolumeTexture.height / NUM_DISPATCH_CHUNKS;
            int dispatchChunkDepth = shadowVolumeTexture.volumeDepth / NUM_DISPATCH_CHUNKS;

            int ix = currentDispatchIndex % NUM_DISPATCH_CHUNKS;
            int iy = (currentDispatchIndex / NUM_DISPATCH_CHUNKS) % NUM_DISPATCH_CHUNKS;
            int iz = currentDispatchIndex / (NUM_DISPATCH_CHUNKS * NUM_DISPATCH_CHUNKS);
            shadowVolumeShader.SetInts("_DispatchOffsets", new int[] { dispatchChunkWidth * ix, dispatchChunkHeight * iy, dispatchChunkDepth * iz });
            shadowVolumeShader.Dispatch(handleMain, threadGroupsX, threadGroupsY, threadGroupsZ);
        }

        private Vector3 GetLightDirection(VolumeRenderedObject targetObject)
        {
            Transform targetTransform = targetObject.volumeContainerObject.transform;
            if (targetObject.GetLightSource() == LightSource.SceneMainLight)
            {
                Light[] lights = GameObject.FindObjectsOfType(typeof(Light)) as Light[];
                Light directionalLight = lights.FirstOrDefault(l => l.type == LightType.Directional);
                if ( directionalLight != null)
                {
                    return targetTransform.InverseTransformDirection(directionalLight.transform.forward);
                }

                if (lights.Length > 0)
                {
                    return targetTransform.InverseTransformDirection(lights[0].transform.forward); // TODO
                }
            }
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return targetTransform.InverseTransformDirection(UnityEditor.SceneView.lastActiveSceneView.camera.transform.forward);
            }
#endif
            return targetTransform.InverseTransformDirection(Camera.main.transform.forward);
        }
    }
}
