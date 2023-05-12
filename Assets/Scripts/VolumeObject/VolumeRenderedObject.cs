using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace UnityVolumeRendering
{
    [ExecuteInEditMode]
    public class VolumeRenderedObject : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        public TransferFunction transferFunction;

        [SerializeField, HideInInspector]
        public TransferFunction2D transferFunction2D;

        [SerializeField, HideInInspector]
        public VolumeDataset dataset;

        [SerializeField, HideInInspector]
        public MeshRenderer meshRenderer;

        [SerializeField, HideInInspector]
        public GameObject volumeContainerObject;

        [SerializeField, HideInInspector]
        private RenderMode renderMode;
        [SerializeField, HideInInspector]
        private TFRenderMode tfRenderMode;
        [SerializeField, HideInInspector]
        private bool lightingEnabled;
        [SerializeField, HideInInspector]
        private LightSource lightSource;

        [SerializeField, HideInInspector]
        private Vector2 visibilityWindow = new Vector2(0.0f, 1.0f);
        [SerializeField, HideInInspector]
        private bool rayTerminationEnabled = true;
        [SerializeField, HideInInspector]
        private bool cubicInterpolationEnabled = false;

        private CrossSectionManager crossSectionManager;

        private SemaphoreSlim updateMatLock = new SemaphoreSlim(1, 1);

        public SlicingPlane CreateSlicingPlane()
        {
            GameObject sliceRenderingPlane = GameObject.Instantiate(Resources.Load<GameObject>("SlicingPlane"));
            sliceRenderingPlane.transform.parent = this.volumeContainerObject.transform;
            sliceRenderingPlane.transform.localPosition = Vector3.zero;
            sliceRenderingPlane.transform.localRotation = Quaternion.identity;
            sliceRenderingPlane.transform.localScale = Vector3.one * 0.1f; // TODO: Change the plane mesh instead and use Vector3.one
            MeshRenderer sliceMeshRend = sliceRenderingPlane.GetComponent<MeshRenderer>();
            sliceMeshRend.material = new Material(sliceMeshRend.sharedMaterial);
            Material sliceMat = sliceRenderingPlane.GetComponent<MeshRenderer>().sharedMaterial;
            sliceMat.SetTexture("_DataTex", dataset.GetDataTexture());
            sliceMat.SetTexture("_TFTex", transferFunction.GetTexture());
            sliceMat.SetMatrix("_parentInverseMat", transform.worldToLocalMatrix);
            sliceMat.SetMatrix("_planeMat", Matrix4x4.TRS(sliceRenderingPlane.transform.position, sliceRenderingPlane.transform.rotation, transform.lossyScale)); // TODO: allow changing scale

            SlicingPlane slicingPlaneComp = sliceRenderingPlane.GetComponent<SlicingPlane>();
            slicingPlaneComp.targetObject = this;
            return slicingPlaneComp;
        }

        public void SetRenderMode(RenderMode mode)
        {
            Task task = SetRenderModeAsync(mode);
        }

        public async Task SetRenderModeAsync(RenderMode mode, IProgressHandler progressHandler = null)
        {
            if (renderMode != mode)
            {
                renderMode = mode;
                SetVisibilityWindow(0.0f, 1.0f); // reset visibility window
            }
            await UpdateMaterialPropertiesAsync(progressHandler);
        }

        public void SetTransferFunctionMode(TFRenderMode mode)
        {
            Task task = SetTransferFunctionModeAsync(mode);
        }

        public async Task SetTransferFunctionModeAsync(TFRenderMode mode, IProgressHandler progressHandler = null)
        {
            if (progressHandler == null)
                progressHandler = NullProgressHandler.instance;

            progressHandler.StartStage(0.3f, "Generating transfer function texture");
            tfRenderMode = mode;
            if (tfRenderMode == TFRenderMode.TF1D && transferFunction != null)
                transferFunction.GenerateTexture();
            else if (transferFunction2D != null)
                transferFunction2D.GenerateTexture();
            progressHandler.EndStage();
            
            progressHandler.StartStage(0.7f, "Updating material properties");
            await UpdateMaterialPropertiesAsync(progressHandler);
            progressHandler.EndStage();
        }

        public TFRenderMode GetTransferFunctionMode()
        {
            return tfRenderMode;
        }

        public RenderMode GetRenderMode()
        {
            return renderMode;
        }

        public bool GetLightingEnabled()
        {
            return lightingEnabled;
        }

        public LightSource GetLightSource()
        {
            return lightSource;
        }

        public CrossSectionManager GetCrossSectionManager()
        {
            if (crossSectionManager == null)
                crossSectionManager = GetComponent<CrossSectionManager>();
            if (crossSectionManager == null)
                crossSectionManager = gameObject.AddComponent<CrossSectionManager>();
            return crossSectionManager;
        }

        public void SetLightingEnabled(bool enable)
        {
            if (enable != lightingEnabled)
            {
                lightingEnabled = enable;
                UpdateMaterialProperties();
            }
        }

        public async Task SetLightingEnabledAsync(bool enable, IProgressHandler progressHandler = null)
        {
            if (enable != lightingEnabled)
            {
                lightingEnabled = enable;
                await UpdateMaterialPropertiesAsync(progressHandler);
            }
        }

        public void SetLightSource(LightSource source)
        {
            lightSource = source;
            UpdateMaterialProperties();
        }

        public void SetVisibilityWindow(float min, float max)
        {
            SetVisibilityWindow(new Vector2(min, max));
        }

        public void SetVisibilityWindow(Vector2 window)
        {
            if (window != visibilityWindow)
            {
                visibilityWindow = window;
                UpdateMaterialProperties();
            }
        }

        public Vector2 GetVisibilityWindow()
        {
            return visibilityWindow;
        }

        public bool GetRayTerminationEnabled()
        {
            return rayTerminationEnabled;
        }

        public void SetRayTerminationEnabled(bool enable)
        {
            if (enable != rayTerminationEnabled)
            {
                rayTerminationEnabled = enable;
                UpdateMaterialProperties();
            }
        }

        [System.Obsolete("Back-to-front rendering no longer supported")]
        public bool GetDVRBackwardEnabled()
        {
            return false;
        }

        [System.Obsolete("Back-to-front rendering no longer supported")]
        public void SetDVRBackwardEnabled(bool enable)
        {
            Debug.LogWarning("Back-to-front rendering no longer supported");
        }

        public bool GetCubicInterpolationEnabled()
        {
            return cubicInterpolationEnabled;
        }

        public void SetCubicInterpolationEnabled(bool enable)
        {
            if (enable != cubicInterpolationEnabled)
            {
                cubicInterpolationEnabled = enable;
                UpdateMaterialProperties();
            }
        }

        public void SetTransferFunction(TransferFunction tf)
        {
            this.transferFunction = tf;
            UpdateMaterialProperties();
        }

        public async Task SetTransferFunctionAsync(TransferFunction tf, IProgressHandler progressHandler = null)
        {
            if (meshRenderer.sharedMaterial == null)
            {
                meshRenderer.sharedMaterial = new Material(Shader.Find("VolumeRendering/DirectVolumeRenderingShader"));
                meshRenderer.sharedMaterial.SetTexture("_DataTex", dataset.GetDataTexture());
            }
            if (transferFunction == null)
            {
                transferFunction = TransferFunctionDatabase.CreateTransferFunction();
            }

            this.transferFunction = tf;
            await UpdateMaterialPropertiesAsync(progressHandler);
        }

        private void UpdateMaterialProperties(IProgressHandler progressHandler = null)
        {
            Task task = UpdateMaterialPropertiesAsync(progressHandler);
        }

        private async Task UpdateMaterialPropertiesAsync(IProgressHandler progressHandler = null)
        {
            await updateMatLock.WaitAsync();

            try
            {
                bool useGradientTexture = tfRenderMode == TFRenderMode.TF2D || renderMode == RenderMode.IsosurfaceRendering || lightingEnabled;
                Texture3D texture = useGradientTexture ? await dataset.GetGradientTextureAsync(progressHandler) : null;
                meshRenderer.sharedMaterial.SetTexture("_GradientTex", texture);
                UpdateMatInternal();
            }
            finally
            {
                updateMatLock.Release();
            }
        }

        private void UpdateMatInternal()
        {
            if (tfRenderMode == TFRenderMode.TF2D)
            {
                meshRenderer.sharedMaterial.SetTexture("_TFTex", transferFunction2D.GetTexture());
                meshRenderer.sharedMaterial.EnableKeyword("TF2D_ON");
            }
            else
            {
                meshRenderer.sharedMaterial.SetTexture("_TFTex", transferFunction.GetTexture());
                meshRenderer.sharedMaterial.DisableKeyword("TF2D_ON");
            }

            if (lightingEnabled)
                meshRenderer.sharedMaterial.EnableKeyword("LIGHTING_ON");
            else
                meshRenderer.sharedMaterial.DisableKeyword("LIGHTING_ON");

            if (lightSource == LightSource.SceneMainLight)
                meshRenderer.sharedMaterial.EnableKeyword("USE_MAIN_LIGHT");
            else
                meshRenderer.sharedMaterial.DisableKeyword("USE_MAIN_LIGHT");

            switch (renderMode)
            {
                case RenderMode.DirectVolumeRendering:
                    {
                        meshRenderer.sharedMaterial.EnableKeyword("MODE_DVR");
                        meshRenderer.sharedMaterial.DisableKeyword("MODE_MIP");
                        meshRenderer.sharedMaterial.DisableKeyword("MODE_SURF");
                        break;
                    }
                case RenderMode.MaximumIntensityProjectipon:
                    {
                        meshRenderer.sharedMaterial.DisableKeyword("MODE_DVR");
                        meshRenderer.sharedMaterial.EnableKeyword("MODE_MIP");
                        meshRenderer.sharedMaterial.DisableKeyword("MODE_SURF");
                        break;
                    }
                case RenderMode.IsosurfaceRendering:
                    {
                        meshRenderer.sharedMaterial.DisableKeyword("MODE_DVR");
                        meshRenderer.sharedMaterial.DisableKeyword("MODE_MIP");
                        meshRenderer.sharedMaterial.EnableKeyword("MODE_SURF");
                        break;
                    }
            }

            meshRenderer.sharedMaterial.SetFloat("_MinVal", visibilityWindow.x);
            meshRenderer.sharedMaterial.SetFloat("_MaxVal", visibilityWindow.y);
            meshRenderer.sharedMaterial.SetVector("_TextureSize", new Vector3(dataset.dimX, dataset.dimY, dataset.dimZ));

            if (rayTerminationEnabled)
                meshRenderer.sharedMaterial.EnableKeyword("RAY_TERMINATE_ON");
            else
                meshRenderer.sharedMaterial.DisableKeyword("RAY_TERMINATE_ON");

            if (cubicInterpolationEnabled)
                meshRenderer.sharedMaterial.EnableKeyword("CUBIC_INTERPOLATION_ON");
            else
                meshRenderer.sharedMaterial.DisableKeyword("CUBIC_INTERPOLATION_ON");
        }

        private void Awake()
        {
            // TODO: Remove this after some time. This is to avoid breaking old serialised objects from before volumeContainerObject was added.
            EnsureVolumeContainerRef();
        }

        private void Start()
        {
            UpdateMaterialProperties();
        }

        public void OnValidate()
        {
            // TODO: Remove this after some time. This is to avoid breaking old serialised objects from before volumeContainerObject was added.
            EnsureVolumeContainerRef();
        }

        private void EnsureVolumeContainerRef()
        {
            if (volumeContainerObject == null)
            {
                Debug.LogWarning("VolumeContainer missing. This is expected if the object was saved with an old version of the plugin. Please re-save it.");
                Transform trans = this.transform.Find("VolumeContainer");
                if (trans == null)
                    trans = this.transform.GetComponentInChildren<MeshRenderer>(true)?.transform;
                if (trans)
                    volumeContainerObject = trans.gameObject;
            }
        }
    }
}
