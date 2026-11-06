using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MingRay
{
    [MovedFrom(true, sourceNamespace: "UnityVolumeRendering")]
    [System.Serializable]
    public struct SegmentationLabel
    {
        public int id;
        public string name;
        public Color colour;
    }
}
