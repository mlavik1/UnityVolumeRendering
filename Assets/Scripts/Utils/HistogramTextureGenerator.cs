using UnityEngine;

public class HistogramTextureGenerator
{
    public static Texture2D GenerateHistogramTexture(VolumeDataset dataset)
    {
        int numSamples = dataset.GetMaxDataValue() + 1;
        int[] values = new int[numSamples];
        Color[] cols = new Color[numSamples];
        Texture2D texture = new Texture2D(numSamples, 1, TextureFormat.RGBAFloat, false);

        int maxFreq = 0;
        for (int iData = 0; iData < dataset.data.Length; iData++)
        {
            values[dataset.data[iData]] += 1;
            maxFreq = System.Math.Max(values[dataset.data[iData]], maxFreq);
        }

        for (int iSample = 0; iSample < numSamples; iSample++)
            cols[iSample] = new Color(Mathf.Log10((float)values[iSample]) / Mathf.Log10((float)maxFreq), 0.0f, 0.0f, 1.0f);

        texture.SetPixels(cols);
        texture.Apply();

        return texture;
    }

    public static Texture2D Generate2DHistogramTexture(VolumeDataset dataset)
    {
        int numSamples = dataset.GetMaxDataValue() + 1;
        int numGradientSamples = 256;
        Color[] cols = new Color[numSamples * numGradientSamples];
        Texture2D texture = new Texture2D(numSamples, numGradientSamples, TextureFormat.RGBAFloat, false);

        for (int iCol = 0; iCol < cols.Length; iCol++)
            cols[iCol] = new Color(0.0f, 0.0f, 0.0f, 0.0f);

        int maxRange = dataset.GetMaxDataValue() - dataset.GetMinDataValue();
        const float maxNormalisedMagnitude = 1.75f; // sqrt(1^2 + 1^2 + 1^2) = swrt(3) = a bit less than 1.75

        for (int x = 1; x < dataset.dimX-1; x++)
        {
            for (int y = 1; y < dataset.dimY-1; y++)
            {
                for (int z = 1; z < dataset.dimZ-1; z++)
                {
                    int iData = x + y * dataset.dimX + z * (dataset.dimX * dataset.dimY);
                    int density = dataset.data[iData];

                    int x1 = dataset.data[(x + 1) + y * dataset.dimX + z * (dataset.dimX * dataset.dimY)];
                    int x2 = dataset.data[(x - 1) + y * dataset.dimX + z * (dataset.dimX * dataset.dimY)];
                    int y1 = dataset.data[x + (y + 1) * dataset.dimX + z * (dataset.dimX * dataset.dimY)];
                    int y2 = dataset.data[x + (y - 1) * dataset.dimX + z * (dataset.dimX * dataset.dimY)];
                    int z1 = dataset.data[x + y * dataset.dimX + (z + 1) * (dataset.dimX * dataset.dimY)];
                    int z2 = dataset.data[x + y * dataset.dimX + (z - 1) * (dataset.dimX * dataset.dimY)];

                    Vector3 grad = new Vector3((x2 - x1) / (float)maxRange, (y2 - y1) / (float)maxRange, (z2 - z1) / (float)maxRange);
                    cols[density + (int)(grad.magnitude * numGradientSamples / maxNormalisedMagnitude) * numSamples] = Color.white;
                }
            }
        }

        texture.SetPixels(cols);
        texture.Apply();

        return texture;
    }
}
