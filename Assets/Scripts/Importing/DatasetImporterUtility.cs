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
        PARCHG,
        NRRD,
        NIFTI,
        ImageSequence
    }

    public class DatasetImporterUtility
    {
        public static DatasetType GetDatasetType(string filePath)
        {
            DatasetType datasetType;

            // Check file extension
            string extension = Path.GetExtension(filePath);

            if (String.Equals(extension, ".vasp"))
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
            else if(extension == ".nrrd")
            {
                datasetType = DatasetType.NRRD;
            }
            else if(extension == ".nii")
            {
                datasetType = DatasetType.NIFTI;
            }
            else if(extension == ".jpg" || extension == ".jpeg" || extension == ".png")
            {
                datasetType = DatasetType.ImageSequence;
            }
            else 
            {
                datasetType = DatasetType.Unknown;
            }
        
            return datasetType;
        }
    }
}
