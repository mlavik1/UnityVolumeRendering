using UnityEngine;

namespace UnityVolumeRendering
{
    public enum CutoutType
    {
        Inclusive, Exclusive
    }

    /// <summary>
    /// Cutout box.
    /// Used for cutting a model (cutout view).
    /// </summary>
    [ExecuteInEditMode]
    public class CutoutBox : MonoBehaviour
    {
        /// <summary>
        /// Volume dataset to cut.
        /// </summary>
        public VolumeRenderedObject targetObject;

        public CutoutType cutoutType = CutoutType.Exclusive;

        private void OnDisable()
        {
            if (targetObject != null)
            {
                targetObject.meshRenderer.sharedMaterial.DisableKeyword("CUTOUT_BOX_INCL");
                targetObject.meshRenderer.sharedMaterial.DisableKeyword("CUTOUT_BOX_EXCL");
            }
        }

        private void Update()
        {
            if (targetObject == null)
                return;

            Material mat = targetObject.meshRenderer.sharedMaterial;

            mat.DisableKeyword(cutoutType == CutoutType.Inclusive ? "CUTOUT_BOX_EXCL" : "CUTOUT_BOX_INCL");
            mat.EnableKeyword(cutoutType == CutoutType.Exclusive ? "CUTOUT_BOX_EXCL" : "CUTOUT_BOX_INCL");
            mat.SetMatrix("_CrossSectionMatrix", transform.worldToLocalMatrix * targetObject.transform.localToWorldMatrix);
        }
    }
}
