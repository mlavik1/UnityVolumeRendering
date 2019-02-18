using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;

public class VolumeRenderer : MonoBehaviour
{
    public TransferFunction tf = null;
    public TransferFunction2D tf2D = null;
    public VolumeDataset volumeDataset = null;

    private void Start()
    {
        string fileToLoad = "DataFiles//manix.dat";
        FileStream fs = new FileStream(fileToLoad, FileMode.Open);
        BinaryReader reader = new BinaryReader(fs);

        ushort dimX = reader.ReadUInt16();
        ushort dimY = reader.ReadUInt16();
        ushort dimZ = reader.ReadUInt16();

        reader.Close();
        fs.Close();

        Debug.Log(dimX + ", " + dimY + ", " + dimZ);

        int uDimension = dimX * dimY * dimZ;

        RawDatasetImporter importer = new RawDatasetImporter(fileToLoad, dimX, dimY, dimZ, DataContentFormat.Int16, 6);
        VolumeDataset dataset = importer.Import();
        volumeDataset = dataset;

        int maxRange = dataset.maxDataValue - dataset.minDataValue;
        //const float maxNormalisedMagnitude = 1.75f; // sqrt(1^2 + 1^2 + 1^2) = swrt(3) = a bit less than 1.75

        Color[] cols = new Color[dataset.data.Length];
        for(int x = 0; x < dataset.dimX; x++)
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

                    cols[iData] = new Color(grad.x, grad.y, grad.z, (float)dataset.data[iData] / (float)dataset.maxDataValue);
                }
            }
        }

        dataset.texture.SetPixels(cols);
        dataset.texture.Apply();

        Texture3D tex = dataset.texture;

        const int noiseDimX = 512;
        const int noiseDimY = 512;
        Texture2D noiseTexture = NoiseTextureGenerator.GenerateNoiseTexture(noiseDimX, noiseDimY);

        tf = new TransferFunction();
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

        tf.histogramTexture = HistogramTextureGenerator.GenerateHistogramTexture(dataset);

        tf2D = new TransferFunction2D();
        tf2D.AddBox(0.05f, 0.1f, 0.8f, 0.7f, Color.white, 0.4f);

        GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_DataTex", tex);
        GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_NoiseTex", noiseTexture);
        GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_TFTex", tfTexture);

        GetComponent<MeshRenderer>().sharedMaterial.EnableKeyword("MODE_DVR");
        GetComponent<MeshRenderer>().sharedMaterial.DisableKeyword("MODE_MIP");
        GetComponent<MeshRenderer>().sharedMaterial.DisableKeyword("MODE_SURF");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            GetComponent<MeshRenderer>().sharedMaterial.EnableKeyword("MODE_DVR");
            GetComponent<MeshRenderer>().sharedMaterial.DisableKeyword("MODE_MIP");
            GetComponent<MeshRenderer>().sharedMaterial.DisableKeyword("MODE_SURF");
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            GetComponent<MeshRenderer>().sharedMaterial.DisableKeyword("MODE_DVR");
            GetComponent<MeshRenderer>().sharedMaterial.EnableKeyword("MODE_MIP");
            GetComponent<MeshRenderer>().sharedMaterial.DisableKeyword("MODE_SURF");
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            GetComponent<MeshRenderer>().sharedMaterial.DisableKeyword("MODE_DVR");
            GetComponent<MeshRenderer>().sharedMaterial.DisableKeyword("MODE_MIP");
            GetComponent<MeshRenderer>().sharedMaterial.EnableKeyword("MODE_SURF");
        }
    }
}
