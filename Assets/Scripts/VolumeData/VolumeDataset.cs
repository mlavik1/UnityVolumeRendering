using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnityVolumeRendering
{
    /// <summary>
    /// An imported dataset. Has a dimension and a 3D pixel array.
    /// </summary>
    [Serializable]
    public class VolumeDataset : ScriptableObject, ISerializationCallbackReceiver
    {
        public string filePath;
        
        // Flattened 3D array of data sample values.
        [SerializeField]
        public float[] data;

        [SerializeField]
        public int dimX, dimY, dimZ;

        [SerializeField]
        public Vector3 scale = Vector3.one;

        [SerializeField]
        public Quaternion rotation;
        
        public float volumeScale;

        [SerializeField]
        public string datasetName;

        private float minDataValue = float.MaxValue;
        private float maxDataValue = float.MinValue;

        private Texture3D dataTexture = null;
        private Texture3D gradientTexture = null;

        private SemaphoreSlim createDataTextureLock = new SemaphoreSlim(1, 1);
        private SemaphoreSlim createGradientTextureLock = new SemaphoreSlim(1, 1);

        [SerializeField, System.Obsolete("Use scale instead")]
        private float scaleX = 1.0f;
        [SerializeField, System.Obsolete("Use scale instead")]
        private float scaleY = 1.0f;
        [SerializeField, System.Obsolete("Use scale instead")]
        private float scaleZ = 1.0f;

        public Texture3D GetDataTexture()
        {
            if (dataTexture == null)
            {
                dataTexture = AsyncHelper.RunSync<Texture3D>(() => CreateTextureInternalAsync(NullProgressHandler.instance));
                return dataTexture;
            }
            else
            {
                return dataTexture;
            }
        }
        public async Task<Texture3D> GetDataTextureAsync(IProgressHandler progressHandler = null)
        {
            if (dataTexture == null)
            {
                await createDataTextureLock.WaitAsync();
                try
                {
                    if (progressHandler == null)
                        progressHandler = NullProgressHandler.instance;
                    dataTexture = await CreateTextureInternalAsync(progressHandler);
                }
                finally
                {
                    createDataTextureLock.Release();
                }
            }
            return dataTexture;
        }

        public Texture3D GetGradientTexture()
        {
            if (gradientTexture == null)
            {
                gradientTexture = AsyncHelper.RunSync<Texture3D>(() => CreateGradientTextureInternalAsync(NullProgressHandler.instance));
                return gradientTexture;
            }
            else
            {
                return gradientTexture;
            }
        }
        public async Task<Texture3D> GetGradientTextureAsync(IProgressHandler progressHandler = null)
        {
            if (gradientTexture == null)
            {
                await createGradientTextureLock.WaitAsync();
                try
                {
                    if (progressHandler == null)
                        progressHandler = new NullProgressHandler();
                    gradientTexture = await CreateGradientTextureInternalAsync(progressHandler != null ? progressHandler : NullProgressHandler.instance);
                }
                finally
                {
                    createGradientTextureLock.Release();
                }
            }
            return gradientTexture;
        }

        public float GetMinDataValue()
        {
            if (minDataValue == float.MaxValue)
                CalculateValueBounds(new NullProgressHandler());
            return minDataValue;
        }

        public float GetMaxDataValue()
        {
            if (maxDataValue == float.MinValue)
                CalculateValueBounds(new NullProgressHandler());
            return maxDataValue;
        }

        /// <summary>
        /// Ensures that the dataset is not too large.
        /// </summary>
        public void FixDimensions()
        {
            int MAX_DIM = 2048; // 3D texture max size. See: https://docs.unity3d.com/Manual/class-Texture3D.html

            while (Mathf.Max(dimX, dimY, dimZ) > MAX_DIM)
            {
                Debug.LogWarning("Dimension exceeds limits (maximum: "+MAX_DIM+"). Dataset is downscaled by 2 on each axis!");

                DownScaleData();
            }
        }

        /// <summary>
        /// Downscales the data by averaging 8 voxels per each new voxel,
        /// and replaces downscaled data with the original data
        /// </summary>
        public void DownScaleData()
        {
            int halfDimX = dimX / 2 + dimX % 2;
            int halfDimY = dimY / 2 + dimY % 2;
            int halfDimZ = dimZ / 2 + dimZ % 2;
            float[] downScaledData = new float[halfDimX * halfDimY * halfDimZ];

            for (int x = 0; x < halfDimX; x++)
            {
                for (int y = 0; y < halfDimY; y++)
                {
                    for (int z = 0; z < halfDimZ; z++)
                    {
                        downScaledData[x + y * halfDimX + z * (halfDimX * halfDimY)] = Mathf.Round(GetAvgerageVoxelValues(x * 2, y * 2, z * 2));
                    }
                }
            }

            //Update data & data dimensions
            data = downScaledData;
            dimX = halfDimX;
            dimY = halfDimY;
            dimZ = halfDimZ;
        }

        private void CalculateValueBounds(IProgressHandler progressHandler)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            minDataValue = float.MaxValue;
            maxDataValue = float.MinValue;

            if (data != null)
            {
                int dimension = dimX * dimY * dimZ;
                int sliceDimension = dimX * dimY;
                for (int i = 0; i < dimension;)
                {
                    for (int j = 0; j < sliceDimension; j++, i++)
                    {
                        float val = data[i];
                        minDataValue = Mathf.Min(minDataValue, val);
                        maxDataValue = Mathf.Max(maxDataValue, val);
                    }
                    progressHandler.ReportProgress(i, dimension, "Calculating value bounds");
                }
            }

            stopwatch.Stop();
            TimeSpan stopwatchElapsed = stopwatch.Elapsed;
            Debug.Log("TIME: " + stopwatchElapsed.TotalMilliseconds);
        }

        private async Task<Texture3D> CreateTextureInternalAsync(IProgressHandler progressHandler)                                        
        {
            Debug.Log("Async texture generation. Hold on.");

            Texture3D.allowThreadedTextureCreation = true;
            TextureFormat texformat = SystemInfo.SupportsTextureFormat(TextureFormat.RHalf) ? TextureFormat.RHalf : TextureFormat.RFloat;

            float minValue = 0;
            float maxValue = 0;
            float maxRange = 0;

            progressHandler.StartStage(0.2f, "Calculating value bounds");
            await Task.Run(() =>
            {
                minValue = GetMinDataValue();
                maxValue = GetMaxDataValue();
                maxRange = maxValue - minValue;
            });
            progressHandler.EndStage();

            Texture3D texture = null;
            bool isHalfFloat = texformat == TextureFormat.RHalf;

            progressHandler.StartStage(0.8f, "Creating texture");
            try
            {
                if (isHalfFloat)
                {
                    progressHandler.ReportProgress(0.0f, "Allocating pixel data");
                    NativeArray<ushort> pixelBytes = new NativeArray<ushort>(data.Length, Allocator.Persistent);

                    await Task.Run(() => {
                        for (int iData = 0; iData < data.Length; iData++)
                            pixelBytes[iData] = Mathf.FloatToHalf((float)(data[iData] - minValue) / maxRange);
                    });
                    progressHandler.ReportProgress(0.8f, "Applying texture");

                    texture = new Texture3D(dimX, dimY, dimZ, texformat, false);
                    texture.wrapMode = TextureWrapMode.Clamp;
                    texture.SetPixelData(pixelBytes, 0);
                    texture.Apply();
                    dataTexture = texture;
                    pixelBytes.Dispose();
                }
                else
                {
                    progressHandler.ReportProgress(0.0f, "Allocating pixel data");
                    NativeArray<float> pixelBytes = new NativeArray<float>(data.Length, Allocator.Persistent);

                    await Task.Run(() => {
                        for (int iData = 0; iData < data.Length; iData++)
                            pixelBytes[iData] = (float)(data[iData] - minValue) / maxRange;
                    });
                    progressHandler.ReportProgress(0.8f, "Applying texture");

                    texture = new Texture3D(dimX, dimY, dimZ, texformat, false);
                    texture.wrapMode = TextureWrapMode.Clamp;
                    texture.SetPixelData(pixelBytes, 0);
                    texture.Apply();
                    pixelBytes.Dispose();
                }
            }
            catch (OutOfMemoryException)
            {
                texture = new Texture3D(dimX, dimY, dimZ, texformat, false);               
                texture.wrapMode = TextureWrapMode.Clamp;


                Debug.LogWarning("Out of memory when creating texture. Using fallback method.");
                for (int x = 0; x < dimX; x++)
                    for (int y = 0; y < dimY; y++)
                        for (int z = 0; z < dimZ; z++)
                            texture.SetPixel(x, y, z, new Color((float)(data[x + y * dimX + z * (dimX * dimY)] - minValue) / maxRange, 0.0f, 0.0f, 0.0f));

                texture.Apply();
            }
            progressHandler.EndStage();
            Debug.Log("Texture generation done.");
            return texture;
        }

        private async Task<Texture3D> CreateGradientTextureInternalAsync(IProgressHandler progressHandler)
        {
            Debug.Log("Async gradient generation. Hold on.");

            Texture3D.allowThreadedTextureCreation = true;
            TextureFormat texformat = SystemInfo.SupportsTextureFormat(TextureFormat.RGBAHalf) ? TextureFormat.RGBAHalf : TextureFormat.RGBAFloat;

            float minValue = 0;
            float maxValue = 0;
            float maxRange = 0;
            Color[] cols = null;

            progressHandler.StartStage(0.2f, "Calculating value bounds");
            await Task.Run(() => {
                if (minDataValue == float.MaxValue || maxDataValue == float.MinValue)
                    CalculateValueBounds(progressHandler);
                minValue = GetMinDataValue();
                maxValue = GetMaxDataValue();
                maxRange = maxValue - minValue;
            });
            progressHandler.EndStage();

            try
            {
                await Task.Run(() => cols = new Color[data.Length]);
            }
            catch (OutOfMemoryException)
            {
                progressHandler.StartStage(0.6f, "Creating gradient texture");
                Texture3D textureTmp = new Texture3D(dimX, dimY, dimZ, texformat, false);
                textureTmp.wrapMode = TextureWrapMode.Clamp;

                for (int x = 0; x < dimX; x++)
                {
                    progressHandler.ReportProgress(x, dimX, "Calculating gradients for slice");
                    for (int y = 0; y < dimY; y++)
                    {
                        for (int z = 0; z < dimZ; z++)
                        {
                            int iData = x + y * dimX + z * (dimX * dimY);
                            Vector3 grad = GetGrad(x, y, z, minValue, maxRange);

                            textureTmp.SetPixel(x, y, z, new Color(grad.x, grad.y, grad.z, (float)(data[iData] - minValue) / maxRange));
                        }
                    }
                }
                progressHandler.EndStage();
                progressHandler.StartStage(0.2f, "Uploading gradient texture");
                textureTmp.Apply();

                progressHandler.EndStage();
                Debug.Log("Gradient gereneration done.");

                return textureTmp;
            }

            await Task.Run(() => {
                progressHandler.StartStage(0.6f, "Creating gradient texture");
                for (int x = 0; x < dimX; x++)
                {
                    progressHandler.ReportProgress(x, dimX, "Calculating gradients for slice");
                    for (int y = 0; y < dimY; y++)
                    {
                        for (int z = 0; z < dimZ; z++)
                        {
                            int iData = x + y * dimX + z * (dimX * dimY);
                            Vector3 grad = GetGrad(x, y, z, minValue, maxRange);

                            cols[iData] = new Color(grad.x, grad.y, grad.z, (float)(data[iData] - minValue) / maxRange);
                        }
                    }
                }
                progressHandler.EndStage();
            });

            progressHandler.StartStage(0.2f, "Uploading gradient texture");
            Texture3D texture = new Texture3D(dimX, dimY, dimZ, texformat, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixels(cols);
            texture.Apply();
            progressHandler.EndStage();

            Debug.Log("Gradient gereneration done.");
            return texture;

        }
        public Vector3 GetGrad(int x, int y, int z, float minValue, float maxRange)
        {
            float x1 = data[Math.Min(x + 1, dimX - 1) + y * dimX + z * (dimX * dimY)] - minValue;
            float x2 = data[Math.Max(x - 1, 0) + y * dimX + z * (dimX * dimY)] - minValue;
            float y1 = data[x + Math.Min(y + 1, dimY - 1) * dimX + z * (dimX * dimY)] - minValue;
            float y2 = data[x + Math.Max(y - 1, 0) * dimX + z * (dimX * dimY)] - minValue;
            float z1 = data[x + y * dimX + Math.Min(z + 1, dimZ - 1) * (dimX * dimY)] - minValue;
            float z2 = data[x + y * dimX + Math.Max(z - 1, 0) * (dimX * dimY)] - minValue;

            return new Vector3((x2 - x1) / maxRange, (y2 - y1) / maxRange, (z2 - z1) / maxRange);
        }

        public float GetAvgerageVoxelValues(int x, int y, int z)
        {
            // if a dimension length is not an even number
            bool xC = x + 1 == dimX;
            bool yC = y + 1 == dimY;
            bool zC = z + 1 == dimZ;

            //if expression can only be true on the edges of the texture
            if (xC || yC || zC)
            {
                if (!xC && yC && zC) return (GetData(x, y, z) + GetData(x + 1, y, z)) / 2.0f;
                else if (xC && !yC && zC) return (GetData(x, y, z) + GetData(x, y + 1, z)) / 2.0f;
                else if (xC && yC && !zC) return (GetData(x, y, z) + GetData(x, y, z + 1)) / 2.0f;
                else if (!xC && !yC && zC) return (GetData(x, y, z) + GetData(x + 1, y, z) + GetData(x, y + 1, z) + GetData(x + 1, y + 1, z)) / 4.0f;
                else if (!xC && yC && !zC) return (GetData(x, y, z) + GetData(x + 1, y, z) + GetData(x, y, z + 1) + GetData(x + 1, y, z + 1)) / 4.0f;
                else if (xC && !yC && !zC) return (GetData(x, y, z) + GetData(x, y + 1, z) + GetData(x, y, z + 1) + GetData(x, y + 1, z + 1)) / 4.0f;
                else return GetData(x, y, z); // if xC && yC && zC
            }
            return (GetData(x, y, z) + GetData(x + 1, y, z) + GetData(x, y + 1, z) + GetData(x + 1, y + 1, z)
                    + GetData(x, y, z + 1) + GetData(x, y + 1, z + 1) + GetData(x + 1, y, z + 1) + GetData(x + 1, y + 1, z + 1)) / 8.0f;
        }

        public float GetData(int x, int y, int z)
        {
            return data[x + y * dimX + z * (dimX * dimY)];
        }

        public void OnBeforeSerialize()
        {
            scaleX = scale.x;
            scaleY = scale.y;
            scaleZ = scale.z;
        }

        public void OnAfterDeserialize()
        {
            scale = new Vector3(scaleX, scaleY, scaleZ);
            Debug.Log(scale);
        }
    }
}
