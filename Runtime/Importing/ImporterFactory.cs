using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MingRay
{
    /// <summary>
    /// Factory for creating importers, for each format.
    /// Use this if you only want to import a dataset, without deciding which importer to use.
    /// Some dataset formats can be imported using several different importers, in which case this factory will return the best alternative.
    /// </summary>
    [MovedFrom(true, sourceNamespace: "UnityVolumeRendering")]
    public class ImporterFactory
    {
        /// <summary>
        /// Create an importer for an image sequence dataset (multiple files) of the specified format.
        /// Use this for DICOM and image sequences.
        /// </summary>
        /// <param name="format">Format of the dataset.</param>
        /// <returns></returns>
        public static IImageSequenceImporter CreateImageSequenceImporter(ImageSequenceFormat format)
        {
            Type importerType = GetImageSequenceImporterType(format);
            if (importerType != null)
            {
                return (IImageSequenceImporter)Activator.CreateInstance(importerType);
            }
            else
            {
                Debug.LogError("No supported importer for format: " + format);
                return null;
            }
        }

        /// <summary>
        /// Create an importer for an image file dataset (single file) of the specified format.
        /// Use this for NRRD, NIFTI and VASP/PARCHG.
        /// </summary>
        /// <param name="format">Format of the dataset.</param>
        /// <returns></returns>
        public static IImageFileImporter CreateImageFileImporter(ImageFileFormat format)
        {
            Type importerType = GetImageFileImporterType(format);
            if (importerType != null)
            {
                return (IImageFileImporter)Activator.CreateInstance(importerType);
            }
            else
            {
                Debug.LogError("No supported importer for format: " + format);
                return null;
            }
        }

        public static void RegisterImageSequenceImporter(ImageSequenceFormat format, Type importerType)
        {
            if (importerType != null && !typeof(IImageSequenceImporter).IsAssignableFrom(importerType))
                throw new ArgumentException($"{importerType} does not implement {nameof(IImageSequenceImporter)}");
            lock (registryLock)
            {
                if (importerType != null)
                    imageSequenceImporters[format] = importerType;
                else
                    imageSequenceImporters.Remove(format);
            }
        }

        public static void RegisterImageFileImporter(ImageFileFormat format, Type importerType)
        {
            if (importerType != null && !typeof(IImageFileImporter).IsAssignableFrom(importerType))
                throw new ArgumentException($"{importerType} does not implement {nameof(IImageFileImporter)}");
            lock (registryLock)
            {
                if (importerType != null)
                    imageFileImporters[format] = importerType;
                else
                    imageFileImporters.Remove(format);
            }
        }

        public static bool HasImageFileImporter(ImageFileFormat format)
        {
            return GetImageFileImporterType(format) != null;
        }

        private static readonly object registryLock = new object();
        private static readonly Dictionary<ImageSequenceFormat, Type> imageSequenceImporters = new Dictionary<ImageSequenceFormat, Type>();
        private static readonly Dictionary<ImageFileFormat, Type> imageFileImporters = new Dictionary<ImageFileFormat, Type>();

        private static Type GetImageSequenceImporterType(ImageSequenceFormat format)
        {
            lock (registryLock)
            {
                if (imageSequenceImporters.TryGetValue(format, out Type registeredType))
                    return registeredType;
            }

            switch (format)
            {
                case ImageSequenceFormat.ImageSequence:
                    return typeof(ImageSequenceImporter);
                case ImageSequenceFormat.DICOM:
                    return typeof(DICOMImporter);
                default:
                    return null;
            }
        }

        private static Type GetImageFileImporterType(ImageFileFormat format)
        {
            lock (registryLock)
            {
                if (imageFileImporters.TryGetValue(format, out Type registeredType))
                    return registeredType;
            }

            switch (format)
            {
                case ImageFileFormat.VASP:
                    return typeof(ParDatasetImporter);
                case ImageFileFormat.NIFTI:
                    return typeof(NiftiImporter);
                default:
                    return null;
            }
        }
    }
}
