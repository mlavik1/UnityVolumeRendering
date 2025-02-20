using System;
using UnityEngine;

namespace UnityVolumeRendering
{
    public class SobelGradientComputator : GradientComputator
    {
        public SobelGradientComputator(VolumeDataset dataset) : base(dataset)
        {
        }

        private float GetData(int x, int y, int z)
        {
            return data[x + y * dimX + z * (dimX * dimY)];
        }

        private float Convolve(int x, int y, int z, float[,,] matrix)
        {
            float result = 0;
            for (int iz = 0; iz <= 2; iz++)
            {
                for (int iy = 0; iy <= 2; iy++)
                {
                    for (int ix = 0; ix <= 2; ix++)
                    {
                        float matrixValue = matrix[iz, iy, ix];
                        float dataValue = GetData(x + ix - 1, y + iy - 1, z + iz - 1);
                        result += matrixValue * dataValue;
                    }
                }
            }
            return result;
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

        public override Vector3 ComputeGradient(int x, int y, int z, float minValue, float maxRange)
        {
            // TODO
            if (x < 2 || y < 2 || z < 2 || x > dimX - 3 || y > dimY - 3 || z > dimZ - 3)
            {
                return Vector3.zero;
            }

            float dx = Convolve(x, y, z, kernelx);
            float dy = Convolve(x, y, z, kernely);
            float dz = Convolve(x, y, z, kernelz);

            Vector3 gradient = new Vector3(dx, dy, dz);

            float divident = maxRange * 3;

            return new Vector3(gradient.x / divident, gradient.y / divident, gradient.z / divident);
        }
    }
}
