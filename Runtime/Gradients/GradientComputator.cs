using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MingRay
{
    [MovedFrom(true, sourceNamespace: "UnityVolumeRendering")]
    public abstract class GradientComputator
    {
        public delegate float[] SmoothingFilter(float[] data, int dimX, int dimY, int dimZ);

        private static SmoothingFilter smoothingFilter;

        protected float[] data;
        protected int dimX, dimY, dimZ;

        public static void SetSmoothingFilter(SmoothingFilter filter)
        {
            smoothingFilter = filter;
        }

        public GradientComputator(VolumeDataset dataset, bool smootheDataValues)
        {
            this.data = dataset.data;
            this.dimX = dataset.dimX;
            this.dimY = dataset.dimY;
            this.dimZ = dataset.dimZ;

            if (smootheDataValues)
            {
                if (smoothingFilter != null)
                    this.data = smoothingFilter(data, dimX, dimY, dimZ);
                else
                    Debug.LogWarning("SimpleITK is required to generate smooth gradients.");
            }
        }

        public abstract Vector3 ComputeGradient(int x, int y, int z, float minValue, float maxRange);
    }

    [MovedFrom(true, sourceNamespace: "UnityVolumeRendering")]
    public class GradientComputatorFactory
    {
        public static GradientComputator CreateGradientComputator(VolumeDataset dataset, GradientType gradientType)
        {
            switch (gradientType)
            {
                case GradientType.CentralDifference:
                    return new CentralDifferenceGradientComputator(dataset, false);
                case GradientType.SmoothedCentralDifference:
                    return new CentralDifferenceGradientComputator(dataset, true);
                case GradientType.Sobel:
                    return new SobelGradientComputator(dataset, false);
                case GradientType.SmoothedSobel:
                    return new SobelGradientComputator(dataset, true);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
