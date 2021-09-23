
using System.IO;
using UnityEngine;
using System;
using UnityEditor;

namespace UnityVolumeRendering
{
    public enum DatasetType
    {
        Unknown,
        Raw,
        DICOM,
        PARCHG
    }

    public class DatasetImporterUtility
    {

        public static DatasetType GetDatasetType(string filePath)
        {
            DatasetType datasetType;

            // Check file extension
            string extension = Path.GetExtension(filePath);
            string vasp  = ".vasp";

            if (String.Equals(vasp ,extension.ToString() ) )
            {
                datasetType = DatasetType.PARCHG;
            }

            else if (extension == ".dat" || extension == ".raw" || extension == ".vol")
                datasetType = DatasetType.Raw;
            
            else if (extension == ".ini")
            {
                filePath = filePath.Substring(0, filePath.LastIndexOf("."));
                datasetType = DatasetType.Raw;
            }
            else if (extension == ".dicom" || extension == ".dcm")
            {
                datasetType = DatasetType.DICOM;
            }

            else 
            {
                datasetType = DatasetType.Unknown;
            }
        
            return datasetType;
        }
    }
}
