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
        private const int NUM_DISPATCH_CHUNKS = 3;
        private const int dispatchCount = NUM_DISPATCH_CHUNKS * NUM_DISPATCH_CHUNKS * NUM_DISPATCH_CHUNKS;

        private VolumeRenderedObject volumeRenderedObject = null;
        private Texture3D targetTexture = null;
        private RenderTexture shadowVolumeTexture = null;
        private Vector3 lightDirection;
        private bool needsUpdate = true;
        private bool initialised = false;
        private ComputeShader shadowVolumeShader;
        private int handleMain;
        int currentDispatchIndex = 0;

        private void Start()
        {
            if (!initialised)
                Initialise();
            needsUpdate = true;
        }
        
        private void OnValidate()
        {
            if (!initialised)
                Initialise();
            needsUpdate = true;
        }

        private void Initialise()
        {
            Debug.Log("Initialising shadow volume buffers");
            volumeRenderedObject = GetComponent<VolumeRenderedObject>();
            Debug.Assert(volumeRenderedObject != null);
            
            Texture3D dataTexture = volumeRenderedObject.dataset.GetDataTexture();
            targetTexture = new Texture3D(512,512,512,
                TextureFormat.RFloat, false);
            Debug.Log(targetTexture.width);
            
            shadowVolumeTexture = new RenderTexture(targetTexture.width, targetTexture.height, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
            shadowVolumeTexture.dimension = TextureDimension.Tex3D;
            shadowVolumeTexture.volumeDepth = targetTexture.depth;
            shadowVolumeTexture.enableRandomWrite = true;
            shadowVolumeTexture.wrapMode = TextureWrapMode.Clamp;
            shadowVolumeTexture.Create();

            volumeRenderedObject.meshRenderer.sharedMaterial.SetTexture("_ShadowVolume", targetTexture);
            volumeRenderedObject.meshRenderer.sharedMaterial.SetVector("_ShadowVolumeTextureSize", new Vector3(targetTexture.width, targetTexture.height, targetTexture.depth));

            shadowVolumeShader = Resources.Load("ShadowVolume") as ComputeShader;
            handleMain = shadowVolumeShader.FindKernel("ShadowVolumeMain");
            if (handleMain < 0)
            {
                Debug.LogError("Shadow volume compute shader initialization failed.");
            }

            needsUpdate = true;
        }

        private void Update()
        {
            Vector3 oldLightDirection = lightDirection;
            lightDirection = -GetLightDirection(volumeRenderedObject);
            needsUpdate |= lightDirection != oldLightDirection;

            if (needsUpdate)
            {
                if (currentDispatchIndex == 0)
                {
                    ConfigureCompute();
                    needsUpdate = false;
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
                Graphics.CopyTexture(shadowVolumeTexture, targetTexture);
            }
        }

        private void ConfigureCompute()
        {
            Debug.Log("Dispatch ShadowVolume compute shader");
            VolumeDataset dataset = volumeRenderedObject.dataset;
            
            Texture3D dataTexture = dataset.GetDataTexture();
            
            if (volumeRenderedObject.GetCubicInterpolationEnabled())
                shadowVolumeShader.EnableKeyword("CUBIC_INTERPOLATION_ON");
            else
                shadowVolumeShader.DisableKeyword("CUBIC_INTERPOLATION_ON");

            shadowVolumeShader.SetVector("TextureSize", new Vector3(dataset.dimX, dataset.dimY, dataset.dimZ));
            shadowVolumeShader.SetInts("Dimension", new int[] { shadowVolumeTexture.width, shadowVolumeTexture.height, shadowVolumeTexture.volumeDepth });
            shadowVolumeShader.SetTexture(handleMain, "VolumeTexture", dataTexture);
            shadowVolumeShader.SetTexture(handleMain, "TFTex", volumeRenderedObject.transferFunction.GetTexture());
            shadowVolumeShader.SetTexture(handleMain, "ShadowVolume", shadowVolumeTexture);
            shadowVolumeShader.SetVector("LightDirection", lightDirection);

            Material volRendMaterial = volumeRenderedObject.meshRenderer.sharedMaterial;
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

        }

        private void DispatchComputeChunk()
        {
            int threadGroupsX = (shadowVolumeTexture.width + 7) / (8 * NUM_DISPATCH_CHUNKS);
            int threadGroupsY = (shadowVolumeTexture.height + 7) / (8 * NUM_DISPATCH_CHUNKS);
            int threadGroupsZ = (shadowVolumeTexture.volumeDepth + 7) / (8 * NUM_DISPATCH_CHUNKS);
            int dispatchChunkWidth = shadowVolumeTexture.width / NUM_DISPATCH_CHUNKS;
            int dispatchChunkHeight = shadowVolumeTexture.height / NUM_DISPATCH_CHUNKS;
            int dispatchChunkDepth = shadowVolumeTexture.volumeDepth / NUM_DISPATCH_CHUNKS;

            int ix = currentDispatchIndex % NUM_DISPATCH_CHUNKS;
            int iy = (currentDispatchIndex / NUM_DISPATCH_CHUNKS) % NUM_DISPATCH_CHUNKS;
            int iz = currentDispatchIndex / (NUM_DISPATCH_CHUNKS * NUM_DISPATCH_CHUNKS);
            shadowVolumeShader.SetInts("DispatchOffsets", new int[] { dispatchChunkWidth * ix, dispatchChunkHeight * iy, dispatchChunkDepth * iz });
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
