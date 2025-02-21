using System;
using UnityEngine;

namespace UnityVolumeRendering
{
    public class SobelGradientComputator : GradientComputator
    {
        public SobelGradientComputator(VolumeDataset dataset) : base(dataset)
        {
        }

        private static readonly float[,,] kernelx = new float[,,]
        {
            { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } },
            { { -2, 0, 2 }, { -4, 0, 4 }, { -2, 0, 2 } },
            { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } }
        };

        private static readonly float[,,] kernely = new float[,,]
        {
            { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } },
            { { -2, -4, -2 }, { 0, 0, 0 }, { 2, 4, 2 } },
            { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } }
        };

        private static readonly float[,,] kernelz = new float[,,]
        {
            { { -1, -2, -1 }, { -2, -4, -2 }, { -1, -2, -1 } },
            { {  0,  0,  0 }, {  0,  0,  0 }, {  0,  0,  0 } },
            { {  1,  2,  1 }, {  2,  4,  2 }, {  1,  2,  1 } }
        };

        private float GetData(int x, int y, int z)
        {
            return data[x + y * dimX + z * (dimX * dimY)];
        }

        private Vector3 ConvolveWithKernels(int x, int y, int z)
        {
            Vector3 result = Vector3.zero;
            for (int iz = 0; iz <= 2; iz++)
            {
                for (int iy = 0; iy <= 2; iy++)
                {
                    for (int ix = 0; ix <= 2; ix++)
                    {
                        float dataValue = GetData(x + ix - 1, y + iy - 1, z + iz - 1);
                        result.x += kernelx[iz, iy, ix] * dataValue;
                        result.y += kernely[iz, iy, ix] * dataValue;
                        result.z += kernelz[iz, iy, ix] * dataValue;
                    }
                }
            }
            return result;
        }

        public override Vector3 ComputeGradient(int x, int y, int z, float minValue, float maxRange)
        {
            // TODO
            if (x < 2 || y < 2 || z < 2 || x > dimX - 3 || y > dimY - 3 || z > dimZ - 3)
            {
                return Vector3.zero;
            }

            Vector3 gradient = ConvolveWithKernels(x, y, z);

            float divident = maxRange * 12;

            return new Vector3(gradient.x / divident, gradient.y / divident, gradient.z / divident);
        }
    }
}
