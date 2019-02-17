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

        dataset.dimX = dimX;
        dataset.dimY = dimY;
        dataset.dimZ = dimZ;

        FileStream fs = new FileStream(filePath, FileMode.Open);
        BinaryReader reader = new BinaryReader(fs);

        int uDimension = dimX * dimY * dimZ;
        dataset.texture = new Texture3D(dimX, dimY, dimZ, TextureFormat.RGBAFloat, false);
        dataset.data = new int[uDimension];

        int minVal = int.MaxValue;
        int maxVal = int.MinValue;
        int val = 0;
        for (int i = 0; i < uDimension; i++)
        {
            switch(contentFormat)
            {
                case DataContentFormat.Int8:
                    val = (int)reader.ReadByte();
                    break;
                case DataContentFormat.Int16:
                    val = (int)reader.ReadInt16();
                    break;
                case DataContentFormat.Int32:
                    val = (int)reader.ReadInt32();
                    break;
                case DataContentFormat.Uint8:
                    val = (int)reader.ReadByte();
                    break;
                case DataContentFormat.Uint16:
                    val = (int)reader.ReadUInt16();
                    break;
                case DataContentFormat.Uint32:
                    val = (int)reader.ReadUInt32();
                    break;
            }
            minVal = Math.Min(minVal, val);
            maxVal = Math.Max(maxVal, val);
            dataset.data[i] = val;
        }
        Debug.Log("Loaded dataset in range: " + minVal + "  -  " + maxVal);
        Debug.Log(minVal + "  -  " + maxVal);

        dataset.minDataValue = minVal;
        dataset.maxDataValue = maxVal;

        return dataset;
    }
}
