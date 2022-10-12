using UnityEngine;

namespace UnityVolumeRendering
{
    /// <summary>
    /// Interface for cross section objects.
    /// </summary>
    public interface CrossSectionObject
    {
        CrossSectionType GetCrossSectionType();
        Matrix4x4 GetMatrix();
    }
}
