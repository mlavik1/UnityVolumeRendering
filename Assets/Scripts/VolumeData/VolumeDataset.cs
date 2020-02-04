using System;
using UnityEngine;

namespace UnityVolumeRendering
{
    public class VolumeDataset
    {
        public int[] data = null;
        public int dimX, dimY, dimZ;

        private int minDataValue = int.MaxValue;
        private int maxDataValue = int.MinValue;
        private Texture3D texture = null;

        public Texture3D GetTexture()
        {
            if (texture == null)
            {
                texture = new Texture3D(dimX, dimY, dimZ, TextureFormat.RGBAFloat, false);
                texture.wrapMode = TextureWrapMode.Clamp;
            }
            return texture;
        }

        public int GetMinDataValue()
        {
            if (minDataValue == int.MaxValue)
                CalculateValueBounds();
            return minDataValue;
        }

        public int GetMaxDataValue()
        {
            if (maxDataValue == int.MinValue)
                CalculateValueBounds();
            return maxDataValue;
        }

        private void CalculateValueBounds()
        {
            minDataValue = int.MaxValue;
            maxDataValue = int.MinValue;
            int dim = dimX * dimY * dimZ;
            for (int i = 0; i < dim; i++)
            {
                int val = data[i];
                minDataValue = Math.Min(minDataValue, val);
                maxDataValue = Math.Max(maxDataValue, val);
            }
        }
    }
}
