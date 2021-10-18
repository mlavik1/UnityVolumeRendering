using System;
using System.IO;
using UnityEngine;

namespace UnityVolumeRendering
{
    /// <summary>
    /// An imported dataset. Has a dimension and a 3D pixel array.
    /// </summary>
    [Serializable]
    public class VolumeDataset : ScriptableObject
    {
        public string filePath;
        
        // Flattened 3D array of data sample values.
        [SerializeField]
        public float[] data;

        [SerializeField]
        public int dimX, dimY, dimZ;
        
        [SerializeField]
        public float scaleX = 0.0f, scaleY = 0.0f, scaleZ = 0.0f;
        public float volumeScale;

        [SerializeField]
        public string datasetName;

        private float minDataValue = float.MaxValue;
        private float maxDataValue = float.MinValue;

        private Texture3D dataTexture = null;
        private Texture3D gradientTexture = null;
        
        
        public Texture3D GetDataTexture()
        {
            dataTexture = CreateTextureInternal();
            return dataTexture;
        }

        public Texture3D GetGradientTexture()
        {
            gradientTexture = CreateGradientTextureInternal();
            return gradientTexture;
        }

        public float GetMinDataValue()
        {
            if (minDataValue == float.MaxValue)
                CalculateValueBounds();
            return minDataValue;
        }

        public float GetMaxDataValue()
        {
            if (maxDataValue == float.MinValue)
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
                float[] newData = new float[dimX * dimY * dimZ];

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
            minDataValue = float.MaxValue;
            maxDataValue = float.MinValue;

            for (int i = 0; i < dimX * dimY * dimZ; i++)
            {
                if (data != null)
                {
                    float val = data[i];
                    minDataValue = Mathf.Min(minDataValue, val);
                    maxDataValue = Mathf.Max(maxDataValue, val);
                }
            }
            
        }

        private Texture3D CreateTextureInternal()
        {
            TextureFormat texformat = SystemInfo.SupportsTextureFormat(TextureFormat.RHalf) ? TextureFormat.RHalf : TextureFormat.RFloat;
            Texture3D texture = new Texture3D(dimX, dimY, dimZ, texformat, false);
            texture.wrapMode = TextureWrapMode.Clamp;

            float minValue = GetMinDataValue();
            float maxValue = GetMaxDataValue();

            float maxRange = maxValue - minValue;

            Color[] cols = new Color[data.Length]; // data exists
            for (int x = 0; x < dimX; x++)
            {
                for (int y = 0; y < dimY; y++)
                {
                    for (int z = 0; z < dimZ; z++)
                    {
                        int iData = x + y * dimX + z * (dimX * dimY);
                        cols[iData] = new Color((data[iData] - minValue) / maxRange, 0.0f, 0.0f, 0.0f);
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

            float minValue = GetMinDataValue();
            float maxValue = GetMaxDataValue();
            float maxRange = maxValue - minValue;

            Color[] cols = new Color[data.Length];
            for (int x = 0; x < dimX; x++)
            {
                for (int y = 0; y < dimY; y++)
                {
                    for (int z = 0; z < dimZ; z++)
                    {
                        int iData = x + y * dimX + z * (dimX * dimY);

                        float x1 = data[Math.Min(x + 1, dimX - 1) + y * dimX + z * (dimX * dimY)] - minValue;
                        float x2 = data[Math.Max(x - 1, 0) + y * dimX + z * (dimX * dimY)] - minValue;
                        float y1 = data[x + Math.Min(y + 1, dimY - 1) * dimX + z * (dimX * dimY)] - minValue;
                        float y2 = data[x + Math.Max(y - 1, 0) * dimX + z * (dimX * dimY)] - minValue;
                        float z1 = data[x + y * dimX + Math.Min(z + 1, dimZ - 1) * (dimX * dimY)] - minValue;
                        float z2 = data[x + y * dimX + Math.Max(z - 1, 0) * (dimX * dimY)] - minValue;

                        Vector3 grad = new Vector3((x2 - x1) / maxRange, (y2 - y1) / maxRange, (z2 - z1) / maxRange);

                        cols[iData] = new Color(grad.x, grad.y, grad.z, (data[iData] - minValue) / maxRange);
                    }
                }
            }
            texture.SetPixels(cols);
            texture.Apply();
            return texture;
        }
    }
}
