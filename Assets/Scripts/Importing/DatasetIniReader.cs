using System;
using System.IO;

namespace UnityVolumeRendering
{
    public class DatasetIniData
    {
        public int dimX = 0;
        public int dimY = 0;
        public int dimZ = 0;
        public int bytesToSkip = 0;
        public DataContentFormat format = DataContentFormat.Uint8;
    }

    public class DatasetIniReader
    {
        public static DatasetIniData ParseIniFile(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            string[] lines = File.ReadAllLines(filePath);

            DatasetIniData iniData = new DatasetIniData();

            foreach (string line in lines)
            {
                string[] parts = line.Trim(' ').Split(':');
                if (parts.Length != 2)
                    continue;

                string name = parts[0];
                string value = parts[1];

                if (name == "dimx")
                    Int32.TryParse(value, out iniData.dimX);
                else if (name == "dimy")
                    Int32.TryParse(value, out iniData.dimY);
                else if (name == "dimz")
                    Int32.TryParse(value, out iniData.dimZ);
                else if (name == "skip")
                    Int32.TryParse(value, out iniData.bytesToSkip);
                else if (name == "format")
                {
                    iniData.format = GetFormatByName(value);
                }
            }

            return iniData;
        }

        private static DataContentFormat GetFormatByName(string format)
        {
            switch (format)
            {
                case "int16":
                    return DataContentFormat.Int16;
                case "int32":
                    return DataContentFormat.Int32;
                case "int8":
                    return DataContentFormat.Int8;
                case "uint16":
                    return DataContentFormat.Uint16;
                case "uint32":
                    return DataContentFormat.Uint32;
                case "uint8":
                    return DataContentFormat.Uint8;
                default:
                    return DataContentFormat.Uint8;
            }
        }
    }
}
