using System;
using UnityEngine;

namespace UnityVolumeRendering
{
    public abstract class GradientComputator
    {
        protected float[] data;
        protected int dimX, dimY, dimZ;

        public GradientComputator(VolumeDataset dataset)
        {
            this.data = dataset.data;
            this.dimX = dataset.dimX;
            this.dimY = dataset.dimY;
            this.dimZ = dataset.dimZ;
        }

        public abstract Vector3 ComputeGradient(int x, int y, int z, float minValue, float maxRange);
    }
}
