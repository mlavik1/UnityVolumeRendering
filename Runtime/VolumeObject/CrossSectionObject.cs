using UnityEngine;

namespace UnityVolumeRendering
{
    /// <summary>
    /// Interface for cross section objects.
    /// Implement this interface if you want to add a custom cross section tool.
    /// This is recommended if you wish to control cross sections from code.
    /// <br/>
    /// CrossSectionObjects can be added to the <see cref="CrossSectionManager"/>.
    /// </summary>
    public interface CrossSectionObject
    {
        /// <summary>
        /// Returns the type of the cross section tool (plane, box, etc.).
        /// </summary>
        /// <returns>Cross section type</returns>
        CrossSectionType GetCrossSectionType();

        /// <summary>
        /// Returns a matrix that converts coordinates relative to the volumetric datasets to coordinates relative to the cross section tool.
        /// <br/>
        /// See possible implementation in <see cref="CrossSectionPlane"/>.
        /// </summary>
        /// <returns>Transformation matrix</returns>
        Matrix4x4 GetMatrix();
    }
}
