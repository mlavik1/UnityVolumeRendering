using System.Linq;
using UnityEngine;

namespace UnityVolumeRendering
{
    /// <summary>
    /// Utility class for generating histograms fo rthe dataset.
    /// </summary>
    public class HistogramTextureGenerator
    {
        /// <summary>
        /// Generates a histogram where:
        ///   X-axis = the data sample (density) value
        ///   Y-axis = the sample count (number of data samples with the specified density)
        /// </summary>
        /// <param name="dataset"></param>
        /// <returns></returns>
        public static Texture2D GenerateHistogramTexture(VolumeDataset dataset)
        {
            float minValue = dataset.GetMinDataValue();
            float maxValue = dataset.GetMaxDataValue();
            float valueRange = maxValue - minValue;

            int numFrequencies = Mathf.Min((int)valueRange, 1024);
            int[] frequencies = new int[numFrequencies];

            int maxFreq = 0;
            float valRangeRecip = 1.0f / (maxValue - minValue);
            for (int iData = 0; iData < dataset.data.Length; iData++)
            {
                float dataValue = dataset.data[iData];
                float tValue = (dataValue - minValue) * valRangeRecip;
                int freqIndex = (int)(tValue * (numFrequencies - 1));
                frequencies[freqIndex] += 1;
                maxFreq = System.Math.Max(frequencies[freqIndex], maxFreq);
            }

            Color[] cols = new Color[numFrequencies];
            Texture2D texture = new Texture2D(numFrequencies, 1, TextureFormat.RGBAFloat, false);

            for (int iSample = 0; iSample < numFrequencies; iSample++)
                cols[iSample] = new Color(Mathf.Log10((float)frequencies[iSample]) / Mathf.Log10((float)maxFreq), 0.0f, 0.0f, 1.0f);

            texture.SetPixels(cols);
            //texture.filterMode = FilterMode.Point;
            texture.Apply();

            return texture;
        }

        /// <summary>
        /// Generates a histogram (but computaion is done on GPU) where:
        ///   X-axis = the data sample (density) value
        ///   Y-axis = the sample count (number of data samples with the specified density)
        /// </summary>
        /// <param name="dataset"></param>
        /// <returns></returns>
        public static Texture2D GenerateHistogramTextureOnGPU(VolumeDataset dataset)
        {
            double actualBound = dataset.GetMaxDataValue() - dataset.GetMinDataValue() + 1;
            int numValues = System.Convert.ToInt32(dataset.GetMaxDataValue() - dataset.GetMinDataValue() + 1); // removed +1
            int sampleCount = System.Math.Min(numValues, 256);

            ComputeShader computeHistogram = Resources.Load("ComputeHistogram") as ComputeShader;
            int handleInitialize = computeHistogram.FindKernel("HistogramInitialize");
            int handleMain = computeHistogram.FindKernel("HistogramMain");

            ComputeBuffer histogramBuffer = new ComputeBuffer(sampleCount, sizeof(uint) * 1);
            uint[] histogramData = new uint[sampleCount];
            Color32[] histogramCols = new Color32[sampleCount];

            Texture3D dataTexture = dataset.GetDataTexture();

            if (handleInitialize < 0 || handleMain < 0)
            {
                Debug.LogError("Histogram compute shader initialization failed.");
            }

            computeHistogram.SetFloat("ValueRange", (float)(sampleCount - 1));
            computeHistogram.SetInts("Dimension", new int[] { dataTexture.width, dataTexture.height, dataTexture.depth });
            computeHistogram.SetTexture(handleMain, "VolumeTexture", dataTexture);
            computeHistogram.SetBuffer(handleMain, "HistogramBuffer", histogramBuffer);
            computeHistogram.SetBuffer(handleInitialize, "HistogramBuffer", histogramBuffer);

            computeHistogram.Dispatch(handleInitialize, sampleCount / 8, 1, 1);
            computeHistogram.Dispatch(handleMain, (dataTexture.width + 7) / 8, (dataTexture.height + 7) / 8, (dataTexture.depth + 7) / 8);

            histogramBuffer.GetData(histogramData);

            int maxValue = (int)histogramData.Max();

            Texture2D texture = new Texture2D(sampleCount, 1, TextureFormat.RGBA32, false);
            for (int iSample = 0; iSample < sampleCount; iSample++)
            {
                histogramCols[iSample] = new Color(Mathf.Log10((float)histogramData[iSample]) / Mathf.Log10((float)maxValue), 0.0f, 0.0f, 1.0f);
            }

            texture.SetPixels32(histogramCols);
            texture.Apply();

            return texture;
        }

        /// <summary>
        /// Creates a histogram texture for 2D transfer functions.
        ///   X-axis = data sample (density) value
        ///   Y-axis = gradient magnitude
        ///   colour = white (if there is a data sample with the specified value and gradient magnitude) or black (if not)
        /// </summary>
        /// <param name="dataset"></param>
        /// <returns></returns>
        public static Texture2D Generate2DHistogramTexture(VolumeDataset dataset)
        {
            float minValue = dataset.GetMinDataValue();
            float maxValue = dataset.GetMaxDataValue();

            // Value range of the density values.
            float densityValRange = maxValue - minValue + 1.0f;
            float densityRangeRecip = 1.0f / (maxValue - minValue); // reciprocal
            // Clamp density value samples.
            int numDensitySamples = System.Math.Min((int)densityValRange, 512);
            int numGradientSamples = 256;
          
            Color[] cols = new Color[numDensitySamples * numGradientSamples];
            Texture2D texture = new Texture2D(numDensitySamples, numGradientSamples, TextureFormat.RGBAFloat, false);

            // Zero-initialise colours.
            for (int iCol = 0; iCol < cols.Length; iCol++)
                cols[iCol] = new Color(0.0f, 0.0f, 0.0f, 0.0f);

            float maxRange = dataset.GetMaxDataValue() - dataset.GetMinDataValue();
            const float maxNormalisedMagnitude = 1.75f; // sqrt(1^2 + 1^2 + 1^2) = swrt(3) = a bit less than 1.75

            for (int x = 1; x < dataset.dimX - 1; x++)
            {
                for (int y = 1; y < dataset.dimY - 1; y++)
                {
                    for (int z = 1; z < dataset.dimZ - 1; z++)
                    {
                        int iData = x + y * dataset.dimX + z * (dataset.dimX * dataset.dimY);
                        int density = Mathf.RoundToInt(dataset.data[iData]); // FIXME

                        float x1 = dataset.data[(x + 1) + y * dataset.dimX + z * (dataset.dimX * dataset.dimY)];
                        float x2 = dataset.data[(x - 1) + y * dataset.dimX + z * (dataset.dimX * dataset.dimY)];
                        float y1 = dataset.data[x + (y + 1) * dataset.dimX + z * (dataset.dimX * dataset.dimY)];
                        float y2 = dataset.data[x + (y - 1) * dataset.dimX + z * (dataset.dimX * dataset.dimY)];
                        float z1 = dataset.data[x + y * dataset.dimX + (z + 1) * (dataset.dimX * dataset.dimY)];
                        float z2 = dataset.data[x + y * dataset.dimX + (z - 1) * (dataset.dimX * dataset.dimY)];
                      
                        // Calculate gradient
                        Vector3 grad = new Vector3((x2 - x1) / (float)maxRange, (y2 - y1) / (float)maxRange, (z2 - z1) / (float)maxRange);

                        // Calculate density and gradient value indices (in flattened 2D array)
                        float tDensity = (density - minValue) * densityRangeRecip;
                        int iDensity = Mathf.RoundToInt((numDensitySamples - 1) * tDensity);
                        int iGrad = (int)(grad.magnitude * numGradientSamples / maxNormalisedMagnitude);

                        // Assign a white colour to all samples (in a histogram where x = density and y = gradient magnitude).
                        cols[iDensity + iGrad * numDensitySamples] = Color.white;
                    }
                }
            }

            texture.SetPixels(cols);
            texture.Apply();

            return texture;
        }
    }
}
