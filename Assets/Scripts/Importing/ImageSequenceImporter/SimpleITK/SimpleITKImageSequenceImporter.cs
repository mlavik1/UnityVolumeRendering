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
    public class SimpleITKImageSequenceImporter : IImageSequenceImporter
    {
        public class ImageSequenceSlice : IImageSequenceFile
        {
            public string filePath;

            public string GetFilePath()
            {
                return filePath;
            }
        }

        public class ImageSequenceSeries : IImageSequenceSeries
        {
            public List<ImageSequenceSlice> files = new List<ImageSequenceSlice>();

            public IEnumerable<IImageSequenceFile> GetFiles()
            {
                return files;
            }
        }

        public IEnumerable<IImageSequenceSeries> LoadSeries(IEnumerable<string> files)
        {
            HashSet<string>  directories = new HashSet<string>();

            foreach (string file in files)
            {
                string dir = Path.GetDirectoryName(file);
                if (!directories.Contains(dir))
                    directories.Add(dir);
            }

            List<ImageSequenceSeries> seriesList = new List<ImageSequenceSeries>();
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
                    ImageSequenceSeries series = new ImageSequenceSeries();
                    foreach(string file in dicom_names)
                    {
                        ImageSequenceSlice sliceFile = new ImageSequenceSlice();
                        sliceFile.filePath = file;
                        series.files.Add(sliceFile);
                    }
                    seriesList.Add(series);
                }
            }

            return seriesList;
        }

        public VolumeDataset ImportSeries(IImageSequenceSeries series)
        {
            ImageSequenceSeries sequenceSeries = (ImageSequenceSeries)series;
            if (sequenceSeries.files.Count == 0)
            {
                Debug.LogError("Empty series. No files to load.");
                return null;
            }

            ImageSeriesReader reader = new ImageSeriesReader();

            VectorString dicomNames = new VectorString();
            foreach (var dicomFile in sequenceSeries.files)
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
