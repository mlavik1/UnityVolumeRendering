using UnityEngine;

namespace UnityVolumeRendering
{
    [System.Serializable]
    public struct SegmentationLabel
    {
        public int id;
        public string name;
        public Color colour;
        public TransferFunction transferFunction;
        public float minDataValue;
        public float maxDataValue;
    }
}
