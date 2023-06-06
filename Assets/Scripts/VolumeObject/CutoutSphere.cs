using UnityEngine;

namespace UnityVolumeRendering
{
    /// <summary>
    /// Cutout sphere.
    /// Used for cutting a model (cutout view).
    /// </summary>
    [ExecuteInEditMode]
    public class CutoutSphere : MonoBehaviour, CrossSectionObject
    {
        [SerializeField]
        private VolumeRenderedObject targetObject;
        [field: SerializeField] public CutoutType CutoutType { get; set; } = CutoutType.Exclusive;

        public CrossSectionType GetCrossSectionType()
        {
            switch (CutoutType)
            {
                case CutoutType.Inclusive:
                    return CrossSectionType.SphereInclusive;
                case CutoutType.Exclusive:
                    return CrossSectionType.SphereExclusive;
                default:
                    throw new System.NotImplementedException();
            }
        }

        public Matrix4x4 GetMatrix()
        {
            return transform.worldToLocalMatrix * targetObject.volumeContainerObject.transform.localToWorldMatrix;
        }

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
    }
}
