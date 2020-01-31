using System;
using UnityEngine;

public class VolumeObjectFactory
{
    public static VolumeRenderedObject CreateObject(VolumeDataset dataset)
    {
        GameObject obj = GameObject.Instantiate((GameObject)Resources.Load("VolumeRenderedObject"));
        VolumeRenderedObject volObj = obj.GetComponent<VolumeRenderedObject>();
        MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();
        meshRenderer.material = new Material(meshRenderer.sharedMaterial); 

        volObj.dataset = dataset;

        int dimX = dataset.dimX;
        int dimY = dataset.dimY;
        int dimZ = dataset.dimZ;

        int maxRange = dataset.GetMaxDataValue() - dataset.GetMinDataValue();

        Color[] cols = new Color[dataset.data.Length];
        for (int x = 0; x < dataset.dimX; x++)
        {
            for (int y = 0; y < dataset.dimY; y++)
            {
                for (int z = 0; z < dataset.dimZ; z++)
                {
                    int iData = x + y * dimX + z * (dimX * dimY);

                    int x1 = dataset.data[Math.Min(x + 1, dimX - 1) + y * dataset.dimX + z * (dataset.dimX * dataset.dimY)];
                    int x2 = dataset.data[Math.Max(x - 1, 0) + y * dataset.dimX + z * (dataset.dimX * dataset.dimY)];
                    int y1 = dataset.data[x + Math.Min(y + 1, dimY - 1) * dataset.dimX + z * (dataset.dimX * dataset.dimY)];
                    int y2 = dataset.data[x + Math.Max(y - 1, 0) * dataset.dimX + z * (dataset.dimX * dataset.dimY)];
                    int z1 = dataset.data[x + y * dataset.dimX + Math.Min(z + 1, dimZ - 1) * (dataset.dimX * dataset.dimY)];
                    int z2 = dataset.data[x + y * dataset.dimX + Math.Max(z - 1, 0) * (dataset.dimX * dataset.dimY)];

                    Vector3 grad = new Vector3((x2 - x1) / (float)maxRange, (y2 - y1) / (float)maxRange, (z2 - z1) / (float)maxRange);

                    cols[iData] = new Color(grad.x, grad.y, grad.z, (float)dataset.data[iData] / (float)dataset.GetMaxDataValue());
                }
            }
        }

        Texture3D tex = dataset.GetTexture();
        tex.SetPixels(cols);
        tex.Apply();

        const int noiseDimX = 512;
        const int noiseDimY = 512;
        Texture2D noiseTexture = NoiseTextureGenerator.GenerateNoiseTexture(noiseDimX, noiseDimY);

        TransferFunction tf = new TransferFunction();
        tf.AddControlPoint(new TFColourControlPoint(0.0f, new Color(0.11f, 0.14f, 0.13f, 1.0f)));
        tf.AddControlPoint(new TFColourControlPoint(0.2415f, new Color(0.469f, 0.354f, 0.223f, 1.0f)));
        tf.AddControlPoint(new TFColourControlPoint(0.3253f, new Color(1.0f, 1.0f, 1.0f, 1.0f)));

        tf.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
        tf.AddControlPoint(new TFAlphaControlPoint(0.1787f, 0.0f));
        tf.AddControlPoint(new TFAlphaControlPoint(0.2f, 0.024f));
        tf.AddControlPoint(new TFAlphaControlPoint(0.28f, 0.03f));
        tf.AddControlPoint(new TFAlphaControlPoint(0.4f, 0.546f));
        tf.AddControlPoint(new TFAlphaControlPoint(0.547f, 0.5266f));

        tf.GenerateTexture();
        Texture2D tfTexture = tf.GetTexture();
        volObj.transferFunction = tf;

        tf.histogramTexture = HistogramTextureGenerator.GenerateHistogramTexture(dataset);

        TransferFunction2D tf2D = new TransferFunction2D();
        tf2D.AddBox(0.05f, 0.1f, 0.8f, 0.7f, Color.white, 0.4f);
        volObj.transferFunction2D = tf2D;

        meshRenderer.sharedMaterial.SetTexture("_DataTex", tex);
        meshRenderer.sharedMaterial.SetTexture("_NoiseTex", noiseTexture);
        meshRenderer.sharedMaterial.SetTexture("_TFTex", tfTexture);

        meshRenderer.sharedMaterial.EnableKeyword("MODE_DVR");
        meshRenderer.sharedMaterial.DisableKeyword("MODE_MIP");
        meshRenderer.sharedMaterial.DisableKeyword("MODE_SURF");

        return volObj;
    }
}
