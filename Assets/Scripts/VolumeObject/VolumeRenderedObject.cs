using UnityEngine;

namespace UnityVolumeRendering
{
    public class VolumeRenderedObject : MonoBehaviour
    {
        [HideInInspector]
        public TransferFunction transferFunction;

        [HideInInspector]
        public TransferFunction2D transferFunction2D;

        [HideInInspector]
        public VolumeDataset dataset;

        private RenderMode renderMode;
        private TFRenderMode tfRenderMode;

        public SlicingPlane CreateSlicingPlane()
        {
            GameObject sliceRenderingPlane = GameObject.Instantiate(Resources.Load<GameObject>("SlicingPlane"));
            sliceRenderingPlane.transform.parent = transform;
            sliceRenderingPlane.transform.localPosition = Vector3.zero;
            sliceRenderingPlane.transform.localRotation = Quaternion.identity;
            MeshRenderer sliceMeshRend = sliceRenderingPlane.GetComponent<MeshRenderer>();
            sliceMeshRend.material = new Material(sliceMeshRend.sharedMaterial);
            Material sliceMat = sliceRenderingPlane.GetComponent<MeshRenderer>().sharedMaterial;
            sliceMat.SetTexture("_DataTex", dataset.GetDataTexture());
            sliceMat.SetTexture("_TFTex", transferFunction.GetTexture());
            sliceMat.SetMatrix("_parentInverseMat", transform.worldToLocalMatrix);
            sliceMat.SetMatrix("_planeMat", Matrix4x4.TRS(sliceRenderingPlane.transform.position, sliceRenderingPlane.transform.rotation, Vector3.one)); // TODO: allow changing scale

            return sliceRenderingPlane.GetComponent<SlicingPlane>();
        }

        public void SetRenderMode(RenderMode mode)
        {
            renderMode = mode;
            UpdateMaaterialProperties();
        }

        public void SetTransferFunctionMode(TFRenderMode mode)
        {
            tfRenderMode = mode;
            UpdateMaaterialProperties();
        }

        public RenderMode GetRenderMode()
        {
            return renderMode;
        }

        private void UpdateMaaterialProperties()
        {
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

            bool useGradientTexture = tfRenderMode == TFRenderMode.TF2D || renderMode == RenderMode.IsosurfaceRendering;
            meshRenderer.sharedMaterial.SetTexture("_GradientTex", useGradientTexture ? dataset.GetGradientTexture() : null);

            if(tfRenderMode == TFRenderMode.TF2D)
            {
                meshRenderer.sharedMaterial.SetTexture("_TFTex", transferFunction2D.GetTexture());
                meshRenderer.sharedMaterial.EnableKeyword("TF2D_ON");
            }
            else
            {
                meshRenderer.sharedMaterial.SetTexture("_TFTex", transferFunction.GetTexture());
                meshRenderer.sharedMaterial.DisableKeyword("TF2D_ON");
            }

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
        }
    }
}
