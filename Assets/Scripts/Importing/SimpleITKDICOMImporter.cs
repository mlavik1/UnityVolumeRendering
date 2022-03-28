using UnityEngine;
using System;
using itk.simple;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;

namespace UnityVolumeRendering
{
    /// <summary>
    /// DICOM importer.
    /// Reads a 3D DICOM dataset from a folder.
    /// The folder needs to contain several .dcm/.dicom files, where each file is a slice of the same dataset.
    /// </summary>
    public class SimpleITKDICOMImporter
    {
        private IEnumerable<string> fileCandidates;
        private HashSet<string> directories;
        private string datasetName;

        public SimpleITKDICOMImporter(IEnumerable<string> files, string name = "DICOM_Dataset")
        {
            this.fileCandidates = files;
            datasetName = name;
            directories = new HashSet<string>();

            foreach (string file in files)
            {
                string dir = Path.GetDirectoryName(file);
                if(!directories.Contains(dir))
                directories.Add(dir);
            }
        }

        public List<DICOMImporter.DICOMSeries> LoadDICOMSeries()
        {
            List<DICOMImporter.DICOMSeries> seriesList = new List<DICOMImporter.DICOMSeries>();
            Dictionary<string, VectorString> directorySeries = new Dictionary<string, VectorString>();
            foreach (string directory in directories)
            {
                VectorString seriesIDs = ImageSeriesReader.GetGDCMSeriesIDs(directory);
                directorySeries.Add(directory, seriesIDs);

            }

            foreach(var dirSeries in directorySeries)
            {
                foreach(string seriesID in dirSeries.Value)
                {
                    VectorString dicom_names = ImageSeriesReader.GetGDCMSeriesFileNames(dirSeries.Key, seriesID);
                    DICOMImporter.DICOMSeries series = new DICOMImporter.DICOMSeries();
                    foreach(string file in dicom_names)
                    {
                        DICOMImporter.DICOMSliceFile sliceFile = new DICOMImporter.DICOMSliceFile();
                        sliceFile.filePath = file;
                        series.dicomFiles.Add(sliceFile);
                    }
                    seriesList.Add(series);
                }
            }

            return seriesList;
        }

        public VolumeDataset ImportDICOMSeries(DICOMImporter.DICOMSeries series)
        {
            if (series.dicomFiles.Count == 0)
            {
                Debug.LogError("Empty series. No files to load.");
                return null;
            }

            ImageSeriesReader reader = new ImageSeriesReader();

            VectorString dicomNames = new VectorString();
            foreach (var dicomFile in series.dicomFiles)
                dicomNames.Add(dicomFile.filePath);
            reader.SetFileNames(dicomNames);

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
            volumeDataset.filePath = dicomNames[0];

            return volumeDataset;
        }
    }
}
