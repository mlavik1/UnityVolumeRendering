using System;
using UnityEngine;

namespace UnityVolumeRendering
{
    /// <summary>
    /// An imported dataset. Has a dimension and a 3D pixel array.
    /// </summary>
    [Serializable]
    public class VolumeDataset : ScriptableObject
    {
        // Flattened 3D array of data sample values.
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

        /// <summary>
        /// Ensures that the dataset is not too large.
        /// </summary>
        public void FixDimensions()
        {
            int MAX_DIM = 2048; // 3D texture max size. See: https://docs.unity3d.com/Manual/class-Texture3D.html

            if (Mathf.Max(dimX, dimY, dimZ) > MAX_DIM)
            {
                Debug.LogWarning("Dimension exceeds limits. Cropping dataset. This might result in an incomplete dataset.");

                int newDimX = Mathf.Min(dimX, MAX_DIM);
                int newDimY = Mathf.Min(dimY, MAX_DIM);
                int newDimZ = Mathf.Min(dimZ, MAX_DIM);
                int[] newData = new int[dimX * dimY * dimZ];

                for (int z = 0; z < newDimZ; z++)
                {
                    for (int y = 0; y < newDimY; y++)
                    {
                        for (int x = 0; x < newDimX; x++)
                        {
                            int oldIndex = (z * dimX * dimY) + (y * dimX) + x;
                            int newIndex = (z * newDimX * newDimY) + (y * newDimX) + x;
                            newData[newIndex] = data[oldIndex];
                        }
                    }
                }
                data = newData;
                dimX = newDimX;
                dimY = newDimY;
                dimZ = newDimZ;
            }
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

            bool isHalfFloat = texformat == TextureFormat.RHalf;
            try
            {
                // Create a bute array for filling the texture. Store has half (16 bit) or single (32 bit) float values.
                int sampleSize = isHalfFloat ? 2 : 4;
                byte[] bytes = new byte[data.Length * sampleSize]; // This can cause OutOfMemoryException
                for (int iData = 0; iData < data.Length; iData++)
                {
                    float pixelValue = (float)(data[iData] - minValue) / maxRange;
                    byte[] pixelBytes = isHalfFloat ? BitConverter.GetBytes(Mathf.FloatToHalf(pixelValue)) : BitConverter.GetBytes(pixelValue);

                    Array.Copy(pixelBytes, 0, bytes, iData * sampleSize, sampleSize);
                }

                texture.SetPixelData(bytes, 0);
            }
            catch (OutOfMemoryException ex)
            {
                Debug.LogWarning("Out of memory when creating texture. Using fallback method.");
                for (int x = 0; x < dimX; x++)
                    for (int y = 0; y < dimY; y++)
                        for (int z = 0; z < dimZ; z++)
                            texture.SetPixel(x, y, z, new Color((float)(data[x + y * dimX + z * (dimX * dimY)] - minValue) / maxRange, 0.0f, 0.0f, 0.0f));
            }

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

            Color[] cols;
            try
            {
                cols = new Color[data.Length];
            }
            catch (OutOfMemoryException ex)
            {
                cols = null;
            }
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

                        if (cols == null)
                        {
                            texture.SetPixel(x, y, z, new Color(grad.x, grad.y, grad.z, (float)(data[iData] - minValue) / maxRange));
                        }
                        else
                        {
                            cols[iData] = new Color(grad.x, grad.y, grad.z, (float)(data[iData] - minValue) / maxRange);
                        }
                    }
                }
            }
            if (cols != null) texture.SetPixels(cols);
            texture.Apply();
            return texture;
        }
    }
}
