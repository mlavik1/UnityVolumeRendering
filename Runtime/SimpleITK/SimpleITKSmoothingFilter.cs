using System;
using System.Runtime.InteropServices;
using itk.simple;

namespace MingRay
{
    internal static class SimpleITKSmoothingFilter
    {
        internal static float[] Apply(float[] data, int dimX, int dimY, int dimZ)
        {
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

            float[] result = new float[data.Length];
            IntPtr imgBuffer = image.GetBufferAsFloat();
            Marshal.Copy(imgBuffer, result, 0, result.Length);
            return result;
        }
    }
}
