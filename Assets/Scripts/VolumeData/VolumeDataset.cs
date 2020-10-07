using System;
using UnityEngine;

namespace UnityVolumeRendering
{
    [Serializable]
    public class VolumeDataset : ScriptableObject
    {
        [SerializeField]
        public int[] data = null;
        [SerializeField]
        public int dimX, dimY, dimZ;
        [SerializeField]
        public float scaleX = 0.0f, scaleY = 0.0f, scaleZ = 0.0f;

        [SerializeField]
        public string datasetName;

        private int minDataValue = int.MaxValue;
        private int maxDataValue = int.MinValue;
        private Texture3D dataTexture = null;
        private Texture3D gradientTexture = null;

        public Texture3D GetDataTexture()
        {
            if (dataTexture == null)
            {
                dataTexture = CreateTextureInternal();
            }
            return dataTexture;
        }

        public Texture3D GetGradientTexture()
        {
            if (gradientTexture == null)
            {
                gradientTexture = CreateGradientTextureInternal();
            }
            return gradientTexture;
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

        private Texture3D CreateTextureInternal()
        {
            TextureFormat texformat = SystemInfo.SupportsTextureFormat(TextureFormat.RHalf) ? TextureFormat.RHalf : TextureFormat.RFloat;
            Texture3D texture = new Texture3D(dimX, dimY, dimZ, texformat, false);
            texture.wrapMode = TextureWrapMode.Clamp;

            int minValue = GetMinDataValue();
            int maxValue = GetMaxDataValue();
            int maxRange = maxValue - minValue;

            Color[] cols = new Color[data.Length];
            for (int x = 0; x < dimX; x++)
            {
                for (int y = 0; y < dimY; y++)
                {
                    for (int z = 0; z < dimZ; z++)
                    {
                        int iData = x + y * dimX + z * (dimX * dimY);
                        cols[iData] = new Color((float)(data[iData] - minValue) / maxRange, 0.0f, 0.0f, 0.0f);
                    }
                }
            }
            texture.SetPixels(cols);
            texture.Apply();
            return texture;
        }

        private Texture3D CreateGradientTextureInternal()
        {
            TextureFormat texformat = SystemInfo.SupportsTextureFormat(TextureFormat.RGBAHalf) ? TextureFormat.RGBAHalf : TextureFormat.RGBAFloat;
            Texture3D texture = new Texture3D(dimX, dimY, dimZ, texformat, false);
            texture.wrapMode = TextureWrapMode.Clamp;

            int minValue = GetMinDataValue();
            int maxValue = GetMaxDataValue();
            int maxRange = maxValue - minValue;

            Color[] cols = new Color[data.Length];
            for (int x = 0; x < dimX; x++)
            {
                for (int y = 0; y < dimY; y++)
                {
                    for (int z = 0; z < dimZ; z++)
                    {
                        int iData = x + y * dimX + z * (dimX * dimY);

                        int x1 = data[Math.Min(x + 1, dimX - 1) + y * dimX + z * (dimX * dimY)] - minValue;
                        int x2 = data[Math.Max(x - 1, 0) + y * dimX + z * (dimX * dimY)] - minValue;
                        int y1 = data[x + Math.Min(y + 1, dimY - 1) * dimX + z * (dimX * dimY)] - minValue;
                        int y2 = data[x + Math.Max(y - 1, 0) * dimX + z * (dimX * dimY)] - minValue;
                        int z1 = data[x + y * dimX + Math.Min(z + 1, dimZ - 1) * (dimX * dimY)] - minValue;
                        int z2 = data[x + y * dimX + Math.Max(z - 1, 0) * (dimX * dimY)] - minValue;

                        Vector3 grad = new Vector3((x2 - x1) / (float)maxRange, (y2 - y1) / (float)maxRange, (z2 - z1) / (float)maxRange);

                        cols[iData] = new Color(grad.x, grad.y, grad.z, (float)(data[iData] - minValue) / maxRange);
                    }
                }
            }
            texture.SetPixels(cols);
            texture.Apply();
            return texture;
        }
    }
}
