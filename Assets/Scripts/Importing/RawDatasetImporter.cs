using System;
using System.IO;
using UnityEngine;

namespace UnityVolumeRendering
{
    public enum DataContentFormat
    {
        Int8,
        Uint8,
        Int16,
        Uint16,
        Int32,
        Uint32
    }

    public class RawDatasetImporter : DatasetImporterBase
    {
        string filePath;
        private int dimX;
        private int dimY;
        private int dimZ;
        private DataContentFormat contentFormat;
        int skipBytes;

        public RawDatasetImporter(string filePath, int dimX, int dimY, int dimZ, DataContentFormat contentFormat, int skipBytes)
        {
            this.filePath = filePath;
            this.dimX = dimX;
            this.dimY = dimY;
            this.dimZ = dimZ;
            this.contentFormat = contentFormat;
            this.skipBytes = skipBytes;
        }

        public override VolumeDataset Import()
        {
            // Check that the file exists
            if(!File.Exists(filePath))
            {
                Debug.LogError("The file does not exist.");
                return null;
            }

            FileStream fs = new FileStream(filePath, FileMode.Open);
            BinaryReader reader = new BinaryReader(fs);

            // Check that the dimension does not exceed the file size
            long expectedFileSize = (long)(dimX * dimY * dimZ) * GetSampleFormatSize(contentFormat) + skipBytes;
            if (fs.Length < expectedFileSize)
            {
                Debug.LogError($"The dimension({dimX}, {dimY}, {dimZ}) exceeds the file size. Expected file size is {expectedFileSize} bytes, while the actual file size is {fs.Length} bytes");
                return null;
            }

            VolumeDataset dataset = new VolumeDataset();
            dataset.dimX = dimX;
            dataset.dimY = dimY;
            dataset.dimZ = dimZ;

            // Skip header (if any)
            if (skipBytes > 0)
                reader.ReadBytes(skipBytes);

            int uDimension = dimX * dimY * dimZ;
            dataset.data = new int[uDimension];

            // Read the data/sample values
            int val = 0;
            for (int i = 0; i < uDimension; i++)
            {
                switch (contentFormat)
                {
                    case DataContentFormat.Int8:
                        val = (int)reader.ReadSByte();
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
                dataset.data[i] = val;
            }
            Debug.Log("Loaded dataset in range: " + dataset.GetMinDataValue() + "  -  " + dataset.GetMaxDataValue());

            return dataset;
        }

        private int GetSampleFormatSize(DataContentFormat format)
        {
            switch (format)
            {
                case DataContentFormat.Int8:
                    return 1;
                    break;
                case DataContentFormat.Uint8:
                    return 1;
                    break;
                case DataContentFormat.Int16:
                    return 2;
                    break;
                case DataContentFormat.Uint16:
                    return 2;
                    break;
                case DataContentFormat.Int32:
                    return 4;
                    break;
                case DataContentFormat.Uint32:
                    return 4;
                    break;
            }
            throw new NotImplementedException();
        }
    }
}