using UnityEngine;

public class HistogramTextureGenerator
{
    public static Texture2D GenerateHistogramTexture(VolumeDataset dataset)
    {
        int numSamples = dataset.maxDataValue + 1;
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
}
