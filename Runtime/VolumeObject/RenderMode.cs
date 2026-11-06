using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MingRay
{
    [MovedFrom(true, sourceNamespace: "UnityVolumeRendering")]
    public enum RenderMode
    {
        DirectVolumeRendering,
        MaximumIntensityProjectipon,
        IsosurfaceRendering
    }

    [MovedFrom(true, sourceNamespace: "UnityVolumeRendering")]
    public enum TFRenderMode
    {
        [InspectorName("1D Transfer Function")]
        TF1D,
        [InspectorName("2D Transfer Function")]
        TF2D
    }
}
