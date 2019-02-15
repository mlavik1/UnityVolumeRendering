using System;
using System.IO;
using UnityEngine;

public enum DataContentFormat
{
    Int8,
    Uint8,
    Int16,
    Uint16,
    Int32,
    Uint32
}

public class RawDatasetImporter
{
    string filePath;
    private int dimX;
    private int dimY;
    private int dimZ;
    private DataContentFormat contentFormat;

    public RawDatasetImporter(string filePath, int dimX, int dimY, int dimZ, DataContentFormat contentFormat)
    {
        this.filePath = filePath;
        this.dimX = dimX;
        this.dimY = dimY;
        this.dimZ = dimZ;
        this.contentFormat = contentFormat;
    }

    public VolumeDataset Import()
    {
        VolumeDataset dataset = new VolumeDataset();

        FileStream fs = new FileStream(filePath, FileMode.Open);
        BinaryReader reader = new BinaryReader(fs);

        int uDimension = dimX * dimY * dimZ;
        dataset.texture = new Texture3D(dimX, dimY, dimZ, TextureFormat.RGBAFloat, false);
        dataset.colours = new Color[uDimension];

        float minVal = float.PositiveInfinity;
        float maxVal = float.NegativeInfinity;
        float val = 0.0f;
        for (int i = 0; i < uDimension; i++)
        {
            switch(contentFormat)
            {
                case DataContentFormat.Int8:
                    val = (float)reader.ReadByte();
                    break;
                case DataContentFormat.Int16:
                    val = (float)reader.ReadInt16();
                    break;
                case DataContentFormat.Int32:
                    val = (float)reader.ReadInt32();
                    break;
                case DataContentFormat.Uint8:
                    val = (float)reader.ReadByte();
                    break;
                case DataContentFormat.Uint16:
                    val = (float)reader.ReadUInt16();
                    break;
                case DataContentFormat.Uint32:
                    val = (float)reader.ReadUInt32();
                    break;
            }
            minVal = Mathf.Min(minVal, val);
            maxVal = Mathf.Max(maxVal, val);
            dataset.colours[i] = new Color(val, 0.0f, 0.0f);
        }
        Debug.Log("Loaded dataset in range: " + minVal + "  -  " + maxVal);
        Debug.Log(minVal + "  -  " + maxVal);

        dataset.texture.SetPixels(dataset.colours);
        dataset.texture.Apply();

        return dataset;
    }
}
