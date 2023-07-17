#if UVR_USE_SIMPLEITK
using UnityEngine;
using System;
using itk.simple;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace UnityVolumeRendering
{
    /// <summary>
    /// SimpleITK-based DICOM importer.
    /// Has support for JPEG2000 and more.
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

        public IEnumerable<IImageSequenceSeries> LoadSeries(IEnumerable<string> files, ImageSequenceImportSettings settings)
        {
            List<ImageSequenceSeries> seriesList= LoadSeriesInternal(files);

            return seriesList;
        }

        public async Task<IEnumerable<IImageSequenceSeries>> LoadSeriesAsync(IEnumerable<string> files, ImageSequenceImportSettings settings)
        {
            List<ImageSequenceSeries> seriesList = null;
            await Task.Run(() => seriesList=LoadSeriesInternal(files));

            return seriesList;
        }

        private List<ImageSequenceSeries> LoadSeriesInternal(IEnumerable<string> files)
        {
            HashSet<string> directories = new HashSet<string>();

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

            foreach (var dirSeries in directorySeries)
            {
                foreach (string seriesID in dirSeries.Value)
                {
                    VectorString dicom_names = ImageSeriesReader.GetGDCMSeriesFileNames(dirSeries.Key, seriesID);
                    ImageSequenceSeries series = new ImageSequenceSeries();
                    foreach (string file in dicom_names)
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

        public VolumeDataset ImportSeries(IImageSequenceSeries series, ImageSequenceImportSettings settings)
        {
            Image image = null;
            float[] pixelData = null;
            VectorUInt32 size = null;
            VectorString dicomNames = null;

            // Create dataset
            VolumeDataset volumeDataset = ScriptableObject.CreateInstance<VolumeDataset>();

            ImageSequenceSeries sequenceSeries = (ImageSequenceSeries)series;
            if (sequenceSeries.files.Count == 0)
            {
                Debug.LogError("Empty series. No files to load.");
                return null;
            }

            ImportSeriesInternal(dicomNames, sequenceSeries, image, size, pixelData, volumeDataset);

            return volumeDataset;
        }

        public async Task<VolumeDataset> ImportSeriesAsync(IImageSequenceSeries series, ImageSequenceImportSettings settings)
        {
            Image image = null;
            float[] pixelData = null;
            VectorUInt32 size = null;
            VectorString dicomNames = null;

            // Create dataset
            VolumeDataset volumeDataset = ScriptableObject.CreateInstance<VolumeDataset>();

            ImageSequenceSeries sequenceSeries = (ImageSequenceSeries)series;
            if (sequenceSeries.files.Count == 0)
            {
                Debug.LogError("Empty series. No files to load.");
                settings.progressHandler.Fail();
                return null;
            }

            await Task.Run(() => ImportSeriesInternal(dicomNames, sequenceSeries, image, size, pixelData, volumeDataset));

            return volumeDataset;
        }

        private void ImportSeriesInternal(VectorString dicomNames, ImageSequenceSeries sequenceSeries, Image image, VectorUInt32 size, float[] pixelData, VolumeDataset volumeDataset)
        {
            ImageSeriesReader reader = new ImageSeriesReader();

            dicomNames = new VectorString();

            foreach (var dicomFile in sequenceSeries.files)
                dicomNames.Add(dicomFile.filePath);
            reader.SetFileNames(dicomNames);

            image = reader.Execute();

            // Cast to 32-bit float
            image = SimpleITK.Cast(image, PixelIDValueEnum.sitkFloat32);

            size = image.GetSize();

            int numPixels = 1;
            for (int dim = 0; dim < image.GetDimension(); dim++)
                numPixels *= (int)size[dim];

            // Read pixel data
            pixelData = new float[numPixels];
            IntPtr imgBuffer = image.GetBufferAsFloat();
            Marshal.Copy(imgBuffer, pixelData, 0, numPixels);

            for (int i = 0; i < pixelData.Length; i++)
                pixelData[i] = Mathf.Clamp(pixelData[i], -1024, 3071);

            VectorDouble spacing = image.GetSpacing();

            volumeDataset.data = pixelData;
            volumeDataset.dimX = (int)size[0];
            volumeDataset.dimY = (int)size[1];
            volumeDataset.dimZ = (int)size[2];
            volumeDataset.datasetName = Path.GetFileName(dicomNames[0]);
            volumeDataset.filePath = dicomNames[0];
            volumeDataset.scale = new Vector3(
                (float)(spacing[0] * size[0]) / 1000.0f, // mm to m
                (float)(spacing[1] * size[1]) / 1000.0f, // mm to m
                (float)(spacing[2] * size[2]) / 1000.0f // mm to m
            );

            // Convert from LPS to Unity's coordinate system
            ImporterUtilsInternal.ConvertLPSToUnityCoordinateSpace(volumeDataset);

            volumeDataset.FixDimensions();
        }
    }
}
#endif
