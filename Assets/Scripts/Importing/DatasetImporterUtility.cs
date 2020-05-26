
using System.IO;

namespace UnityVolumeRendering
{
    public enum DatasetType
    {
        Unknown,
        Raw,
        DICOM
    }

    public class DatasetImporterUtility
    {

        public static DatasetType GetDatasetType(string filePath)
        {
            DatasetType datasetType;

            // Check file extension
            string extension = Path.GetExtension(filePath);
            if (extension == ".dat" || extension == ".raw" || extension == ".vol")
                datasetType = DatasetType.Raw;
            else if (extension == ".ini")
            {
                filePath = filePath.Substring(0, filePath.LastIndexOf("."));
                datasetType = DatasetType.Raw;
            }
            else if (extension == ".dicom" || extension == ".dcm")
                datasetType = DatasetType.DICOM;
            else
                datasetType = DatasetType.Unknown;

            return datasetType;
        }
    }
}
