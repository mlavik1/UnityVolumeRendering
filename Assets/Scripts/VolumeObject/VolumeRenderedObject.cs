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

        private RenderMode remderMode;

        public SlicingPlane CreateSlicingPlane()
        {
            GameObject slicingPlaneObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
            slicingPlaneObj.transform.parent = transform;
            slicingPlaneObj.transform.localPosition = Vector3.zero;
            slicingPlaneObj.transform.localRotation = Quaternion.Euler(270.0f, 0.0f, 0.0f);
            SlicingPlane slicingPlane = slicingPlaneObj.AddComponent<SlicingPlane>();
            slicingPlane.voldRendObj = this;

            SlicingPlaneAnyDirection csplane = slicingPlaneObj.AddComponent<SlicingPlaneAnyDirection>();
            csplane.mat = this.GetComponent<MeshRenderer>().sharedMaterial;
            csplane.volumeTransform = this.transform;
            csplane.enabled = false;

            return slicingPlane;
        }

        public void SetRenderMode(RenderMode mode)
        {
            remderMode = mode;

            switch (mode)
            {
                case RenderMode.DirectVolumeRendering:
                    {
                        GetComponent<MeshRenderer>().sharedMaterial.EnableKeyword("MODE_DVR");
                        GetComponent<MeshRenderer>().sharedMaterial.DisableKeyword("MODE_MIP");
                        GetComponent<MeshRenderer>().sharedMaterial.DisableKeyword("MODE_SURF");
                        break;
                    }
                case RenderMode.MaximumIntensityProjectipon:
                    {
                        GetComponent<MeshRenderer>().sharedMaterial.DisableKeyword("MODE_DVR");
                        GetComponent<MeshRenderer>().sharedMaterial.EnableKeyword("MODE_MIP");
                        GetComponent<MeshRenderer>().sharedMaterial.DisableKeyword("MODE_SURF");
                        break;
                    }
                case RenderMode.IsosurfaceRendering:
                    {
                        GetComponent<MeshRenderer>().sharedMaterial.DisableKeyword("MODE_DVR");
                        GetComponent<MeshRenderer>().sharedMaterial.DisableKeyword("MODE_MIP");
                        GetComponent<MeshRenderer>().sharedMaterial.EnableKeyword("MODE_SURF");
                        break;
                    }
            }
        }

        public RenderMode GetRemderMode()
        {
            return remderMode;
        }
    }
}
