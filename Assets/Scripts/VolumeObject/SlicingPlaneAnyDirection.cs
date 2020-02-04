using UnityEngine;

namespace UnityVolumeRendering
{
    [ExecuteInEditMode]
    public class SlicingPlaneAnyDirection : MonoBehaviour
    {
        public Material mat;
        public Transform volumeTransform;

        private void OnDisable()
        {
            if (mat != null)
                mat.DisableKeyword("SLICEPLANE_ON");
        }

        private void Update()
        {
            if (mat == null || volumeTransform == null)
                return;

            mat.EnableKeyword("SLICEPLANE_ON");
            mat.SetVector("_PlanePos", volumeTransform.position - transform.position);
            mat.SetVector("_PlaneNormal", transform.forward);
        }
    }
}
