using UnityEngine;

namespace UnityVolumeRendering
{
    public class NoiseTextureGenerator
    {
        public static Texture2D GenerateNoiseTexture(int noiseDimX, int noiseDimY)
        {
            Texture2D noiseTexture = new Texture2D(noiseDimX, noiseDimY);
            Color[] noiseCols = new Color[noiseDimX * noiseDimY];
            for (int iY = 0; iY < noiseDimY; iY++)
            {
                for (int iX = 0; iX < noiseDimX; iX++)
                {
                    float pixVal = Random.Range(0.0f, 1.0f);
                    noiseCols[iX + iY * noiseDimX] = new Color(pixVal, pixVal, pixVal);
                }
            }

            noiseTexture.SetPixels(noiseCols);
            noiseTexture.Apply();
            return noiseTexture;
        }
    }
}
