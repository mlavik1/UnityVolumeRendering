using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

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

        public IEnumerable<IImageSequenceSeries> LoadSeries(IEnumerable<string> files)
        {
            Dictionary<string, ImageSequenceSeries> sequenceByFiletype = new Dictionary<string, ImageSequenceSeries>();
            foreach(string filePath in files)
            {
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

            if (sequenceByFiletype.Count == 0)
                Debug.LogError("Found no image files of supported formats. Currently supported formats are: " + supportedImageTypes.ToString());

            return sequenceByFiletype.Select(f => f.Value).ToList();
        }

        public VolumeDataset ImportSeries(IImageSequenceSeries series)
        {
            List<string> imagePaths = series.GetFiles().Select(f => f.GetFilePath()).ToList();

            Vector3Int dimensions = GetVolumeDimensions(imagePaths);
            int[] data = FillSequentialData(dimensions, imagePaths);
            VolumeDataset dataset = FillVolumeDataset(data, dimensions);

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

            VolumeDataset dataset = new VolumeDataset()
            {
                name = name,
                datasetName = name,
                data = Array.ConvertAll(data, new Converter<int, float>((int val) => { return Convert.ToSingle(val); })),
                dimX = dimensions.x,
                dimY = dimensions.y,
                dimZ = dimensions.z,
                scaleX = 1f, // Scale arbitrarily normalised around the x-axis 
                scaleY = (float)dimensions.y / (float)dimensions.x,
                scaleZ = (float)dimensions.z / (float)dimensions.x
            };

            return dataset;
        }
    }
}
