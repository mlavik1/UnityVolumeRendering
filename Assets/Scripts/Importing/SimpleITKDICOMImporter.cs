using UnityEngine;
using System;
using itk.simple;
using System.Runtime.InteropServices;

namespace UnityVolumeRendering
{
    /// <summary>
    /// DICOM importer.
    /// Reads a 3D DICOM dataset from a folder.
    /// The folder needs to contain several .dcm/.dicom files, where each file is a slice of the same dataset.
    /// </summary>
    public class SimpleITKDICOMImporter
    {
        public VolumeDataset ImportDICOMSeries(string directory)
        {
            ImageSeriesReader reader = new ImageSeriesReader();

            VectorString dicom_names = ImageSeriesReader.GetGDCMSeriesFileNames(directory);
            reader.SetFileNames(dicom_names);

            Image image = reader.Execute();

            // Cast to 32-bit float
            image = SimpleITK.Cast(image, PixelIDValueEnum.sitkFloat32);

            VectorUInt32 size = image.GetSize();
            Debug.Log("Image size: " + size[0] + " " + size[1] + " " + size[2]);

            int numPixels = 1;
            for (int dim = 0; dim < image.GetDimension(); dim++)
                numPixels *= (int)size[dim];

            // Read pixel data
            float[] pixelData = new float[numPixels];
            IntPtr imgBuffer = image.GetBufferAsFloat();
            Marshal.Copy(imgBuffer, pixelData, 0, numPixels);

            // Create dataset
            VolumeDataset volumeDataset = new VolumeDataset();
            volumeDataset.data = pixelData;
            volumeDataset.dimX = (int)size[0];
            volumeDataset.dimY = (int)size[1];
            volumeDataset.dimZ = (int)size[2];
            volumeDataset.datasetName = "test";
            volumeDataset.filePath = directory;

            return volumeDataset;
        }
    }
}
