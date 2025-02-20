using System;
using UnityEngine;

namespace UnityVolumeRendering
{
    public enum GradientType
    {
        CentralDifference,
        Sobel
    }

    public class GradientTypeUtils
    {
        public static GradientType GetDefaultGradientType()
        {
            GradientType gradientType = GradientType.CentralDifference;
            Enum.TryParse(PlayerPrefs.GetString("DefaultGradientType"), out gradientType);
            return gradientType;
        }
    }
}
