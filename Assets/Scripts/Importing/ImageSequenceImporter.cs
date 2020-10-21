using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UnityVolumeRendering
{
    /// <summary>
    /// Converts a directory of image slices into a VolumeDataset for volumetric rendering.
    /// </summary>
    public class ImageSequenceImporter : DatasetImporterBase
    {
        private string directoryPath;
        private string[] supportedImageTypes = new string[] 
        {
            "*.png",
            "*.jpg",
        };

        public ImageSequenceImporter(string directoryPath)
        {
            this.directoryPath = directoryPath;
        }

        public override VolumeDataset Import()
        {
            if (!Directory.Exists(directoryPath))
                throw new NullReferenceException("No directory found: " + directoryPath);

            List<string> imagePaths = GetSortedImagePaths();

            if (!ImageSetHasUniformDimensions(imagePaths))
                throw new IndexOutOfRangeException("Image sequence has non-uniform dimensions");

            Vector3Int dimensions = GetVolumeDimensions(imagePaths);
            int[] data = FillSequentialData(dimensions, imagePaths);
            VolumeDataset dataset = FillVolumeDataset(data, dimensions);

            return dataset;
        }

        /// <summary>
        /// Gets every file path in the directory with a supported suffix.
        /// </summary>
        /// /// <returns>A sorted list of image file paths.</returns>
        private List<string> GetSortedImagePaths()
        {
            var imagePaths = new List<string>();

            foreach (var type in supportedImageTypes)
            {
                imagePaths.AddRange(Directory.GetFiles(directoryPath, type));
            }

            imagePaths.Sort();

            return imagePaths;
        }

        /// <summary>
        /// Checks if every image in the set has the same XY dimensions.
        /// </summary>
        /// <param name="imagePaths">The list of image paths to check.</param>
        /// <returns>True if at least one image differs from another.</returns>
        private bool ImageSetHasUniformDimensions(List<string> imagePaths)
        {
            bool hasUniformDimension = true;

            Vector2Int previous, current;
            previous = GetImageDimensions(imagePaths[0]);

            foreach (var path in imagePaths)
            {
                current = GetImageDimensions(path);

                if (current.x != previous.x || current.y != previous.y)
                {
                    hasUniformDimension = false;
                    break;
                }

                previous = current;
            }

            return hasUniformDimension;
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

                Color[] pixels = texture.GetPixels(); // Order priority: X -> Y -> Z
                int[] imageData = DensityHelper.ConvertColorsToDensities(pixels);

                data.AddRange(imageData);
            }

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
                data = data,
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