#if UVR_USE_SIMPLEITK
using itk.simple;
#endif
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UnityVolumeRendering
{
    public abstract class GradientComputator
    {
        protected float[] data;
        protected int dimX, dimY, dimZ;

        public GradientComputator(VolumeDataset dataset, bool smootheDataValues)
        {
            this.data = dataset.data;
            this.dimX = dataset.dimX;
            this.dimY = dataset.dimY;
            this.dimZ = dataset.dimZ;
            
            if (smootheDataValues)
            {
#if UVR_USE_SIMPLEITK
                Image image = new Image((uint)dimX, (uint)dimY, (uint)dimZ, PixelIDValueEnum.sitkFloat32);

                for (uint z = 0; z < dimZ; z++)
                {
                    for (uint y = 0; y < dimY; y++)
                    {
                        for (uint x = 0; x < dimX; x++)
                        {
                            float value = data[x + y * dimX + z * (dimX * dimY)];
                            image.SetPixelAsFloat(new VectorUInt32() { x, y, z }, value);
                        }
                    }
                }

                BilateralImageFilter filter = new BilateralImageFilter();
                image = filter.Execute(image);

                this.data = new float[data.Length];
                IntPtr imgBuffer = image.GetBufferAsFloat();
                Marshal.Copy(imgBuffer, data, 0, data.Length);
#else
            Debug.LogWarning("SimpleITK is required to generate smooth gradients.");
#endif
            }
        }

        public abstract Vector3 ComputeGradient(int x, int y, int z, float minValue, float maxRange);
    }

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
