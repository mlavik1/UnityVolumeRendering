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
    public class CutoutBox : MonoBehaviour, CrossSectionObject
    {
        /// <summary>
        /// Volume dataset to cross section.
        /// </summary>
        [SerializeField]
        private VolumeRenderedObject targetObject;

        public CutoutType cutoutType = CutoutType.Exclusive;


        private void OnEnable()
        {
            if (targetObject != null)
                targetObject.GetCrossSectionManager().AddCrossSectionObject(this);
        }

        private void OnDisable()
        {
            if (targetObject != null)
                targetObject.GetCrossSectionManager().RemoveCrossSectionObject(this);
        }

        public void SetTargetObject(VolumeRenderedObject target)
        {
            if (this.enabled && targetObject != null)
                targetObject.GetCrossSectionManager().RemoveCrossSectionObject(this);
            
            targetObject = target;

            if (this.enabled && targetObject != null)
                targetObject.GetCrossSectionManager().AddCrossSectionObject(this);
        }

        public CrossSectionType GetCrossSectionType()
        {
            switch (cutoutType)
            {
                case CutoutType.Inclusive:
                    return CrossSectionType.BoxInclusive;
                case CutoutType.Exclusive:
                    return CrossSectionType.BoxExclusive;
                default:
                    throw new System.NotImplementedException();
            }
        }

        public Matrix4x4 GetMatrix()
        {
            return transform.worldToLocalMatrix * targetObject.volumeContainerObject.transform.localToWorldMatrix;
        }
    }
}
