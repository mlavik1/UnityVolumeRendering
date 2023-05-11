using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using openDicom.Encoding;

namespace UnityVolumeRendering
{
    /// <summary>
    /// Converts a directory of image slices into a VolumeDataset for volumetric rendering.
    /// </summary>
    public class ImageSequenceImporter : IImageSequenceImporter
    {
        public class ImageSequenceFile : IImageSequenceFile
        {
            public string filePath;

            public string GetFilePath()
            {
                return filePath;
            }
        }

        public class ImageSequenceSeries : IImageSequenceSeries
        {
            public List<ImageSequenceFile> files = new List<ImageSequenceFile>();

            public IEnumerable<IImageSequenceFile> GetFiles()
            {
                return files;
            }
        }

        private string directoryPath;
        private HashSet<string> supportedImageTypes = new HashSet<string>
        {
            ".png",
            ".jpg",
            ".jpeg"
        };

        public IEnumerable<IImageSequenceSeries> LoadSeries(IEnumerable<string> files, ImageSequenceImportSettings settings)
        {
            Dictionary<string, ImageSequenceSeries> sequenceByFiletype = new Dictionary<string, ImageSequenceSeries>();

            LoadSeriesInternal(files, sequenceByFiletype, settings.progressHandler);

            if (sequenceByFiletype.Count == 0)
                Debug.LogError("Found no image files of supported formats. Currently supported formats are: " + supportedImageTypes.ToString());

            return sequenceByFiletype.Select(f => f.Value).ToList();
        }
        public async Task<IEnumerable<IImageSequenceSeries>> LoadSeriesAsync(IEnumerable<string> files, ImageSequenceImportSettings settings)
        {
            Dictionary<string, ImageSequenceSeries> sequenceByFiletype = new Dictionary<string, ImageSequenceSeries>();

            await Task.Run(() =>LoadSeriesInternal(files,sequenceByFiletype, settings.progressHandler));

            if (sequenceByFiletype.Count == 0)
                Debug.LogError("Found no image files of supported formats. Currently supported formats are: " + supportedImageTypes.ToString());

            return sequenceByFiletype.Select(f => f.Value).ToList();
        }
        private void LoadSeriesInternal(IEnumerable<string> files, Dictionary<string, ImageSequenceSeries> sequenceByFiletype, IProgressHandler progress)
        {
            int fileIndex = 0, numFiles = files.Count();
            foreach (string filePath in files)
            {
                progress.ReportProgress(fileIndex, numFiles, $"Loading DICOM file {fileIndex} of {numFiles}");
                string fileExt = Path.GetExtension(filePath).ToLower();
                if (supportedImageTypes.Contains(fileExt))
                {
                    if (!sequenceByFiletype.ContainsKey(fileExt))
                        sequenceByFiletype[fileExt] = new ImageSequenceSeries();

                    ImageSequenceFile imgSeqFile = new ImageSequenceFile();
                    imgSeqFile.filePath = filePath;
                    sequenceByFiletype[fileExt].files.Add(imgSeqFile);
                }
            }
        }

        public VolumeDataset ImportSeries(IImageSequenceSeries series, ImageSequenceImportSettings settings)
        {
            List<string> imagePaths = series.GetFiles().Select(f => f.GetFilePath()).ToList();

            Vector3Int dimensions = GetVolumeDimensions(imagePaths);
            int[] data = FillSequentialData(dimensions, imagePaths);
            VolumeDataset dataset = FillVolumeDataset(data, dimensions);

            dataset.FixDimensions();
            dataset.rotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);

            return dataset;
        }
        public async Task<VolumeDataset> ImportSeriesAsync(IImageSequenceSeries series, ImageSequenceImportSettings settings)
        {
            List<string> imagePaths = null;
            VolumeDataset dataset = null;

            await Task.Run(() => { imagePaths = series.GetFiles().Select(f => f.GetFilePath()).ToList(); }); ;

            Vector3Int dimensions = GetVolumeDimensions(imagePaths);
            int[] data = FillSequentialData(dimensions, imagePaths);
            dataset = await FillVolumeDatasetAsync(data, dimensions);
            dataset.FixDimensions();

            return dataset;
        }

        /// <summary>
        /// Gets the XY dimensions of an image at the path.
        /// </summary>
        /// <param name="path">The image path to check.</param>
        /// <returns>The XY dimensions of the image.</returns>
        private Vector2Int GetImageDimensions(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);

            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(bytes);

            Vector2Int dimensions = new Vector2Int()
            {
                x = texture.width,
                y = texture.height
            };
            Texture2D.DestroyImmediate(texture);
            return dimensions;
        }

        /// <summary>
        /// Adds a depth value Z to the XY dimensions of the first image.
        /// </summary>
        /// <param name="paths">The set of image paths comprising the volume.</param>
        /// <returns>The dimensions of the volume.</returns>
        private Vector3Int GetVolumeDimensions(List<string> paths)
        {
            Vector2Int twoDimensional = GetImageDimensions(paths[0]);
            Vector3Int threeDimensional = new Vector3Int()
            {
                x = twoDimensional.x,
                y = twoDimensional.y,
                z = paths.Count
            };
            return threeDimensional;
        }

        /// <summary>
        /// Converts a volume set of images into a sequential series of values.
        /// </summary>
        /// <param name="dimensions">The XYZ dimensions of the volume.</param>
        /// <param name="paths">The set of image paths comprising the volume.</param>
        /// <returns>The set of sequential values for the volume.</returns>
        private int[] FillSequentialData(Vector3Int dimensions, List<string> paths)
        {
            var data = new List<int>(dimensions.x * dimensions.y * dimensions.z);
            var texture = new Texture2D(1, 1);

            foreach (var path in paths)
            {
                byte[] bytes = File.ReadAllBytes(path);
                texture.LoadImage(bytes);

                if (texture.width != dimensions.x || texture.height != dimensions.y)
                {
                    Texture2D.DestroyImmediate(texture);
                    throw new IndexOutOfRangeException("Image sequence has non-uniform dimensions");
                }

                Color[] pixels = texture.GetPixels(); // Order priority: X -> Y -> Z
                int[] imageData = DensityHelper.ConvertColorsToDensities(pixels);

                data.AddRange(imageData);
            }
            Texture2D.DestroyImmediate(texture);
            return data.ToArray();
        }

        /// <summary>
        /// Wraps volume data into a VolumeDataset.
        /// </summary>
        /// <param name="data">Sequential value data for a volume.</param>
        /// <param name="dimensions">The XYZ dimensions of the volume.</param>
        /// <returns>The wrapped volume data.</returns>
        private VolumeDataset FillVolumeDataset(int[] data, Vector3Int dimensions)
        {
            string name = Path.GetFileName(directoryPath);

            VolumeDataset dataset = ScriptableObject.CreateInstance<VolumeDataset>();
            FillVolumeInternal(dataset, name, data, dimensions);

            return dataset;
        }
        private async Task<VolumeDataset> FillVolumeDatasetAsync(int[] data, Vector3Int dimensions)
        {
            VolumeDataset dataset = ScriptableObject.CreateInstance<VolumeDataset>();
            string name = Path.GetFileName(directoryPath);
            dataset.name = name;

            await Task.Run(() => FillVolumeInternal(dataset, name, data, dimensions));
          
            return dataset;
        }
        private void FillVolumeInternal(VolumeDataset dataset,string name,int[] data, Vector3Int dimensions)
        {
            dataset.datasetName = name;
            dataset.data = Array.ConvertAll(data, new Converter<int, float>((int val) => { return Convert.ToSingle(val); }));
            dataset.dimX = dimensions.x;
            dataset.dimY = dimensions.y;
            dataset.dimZ = dimensions.z;
            dataset.scale = new Vector3(
                1f, // Scale arbitrarily normalised around the x-axis 
                (float)dimensions.y / (float)dimensions.x,
                (float)dimensions.z / (float)dimensions.x
            );
        }

        
    }
}
