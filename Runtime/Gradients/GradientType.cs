using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MingRay
{
    [MovedFrom(true, sourceNamespace: "UnityVolumeRendering")]
    public enum GradientType
    {
        CentralDifference,
        SmoothedCentralDifference,
        Sobel,
        SmoothedSobel
    }

    [MovedFrom(true, sourceNamespace: "UnityVolumeRendering")]
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
