using System;
using UnityEngine;

namespace UnityVolumeRendering
{
    public class ImporterFactory
    {
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

        private static Type GetImageSequenceImporterType(ImageSequenceFormat format)
        {
            switch (format)
            {
                case ImageSequenceFormat.ImageSequence:
                    {
                        return typeof(ImageSequenceImporter);
                    }
                case ImageSequenceFormat.DICOM:
                    {
                        return typeof(SimpleITKImageSequenceImporter);
                    }
                default:
                    return null;
            }
        }

        private static Type GetImageFileImporterType(ImageFileFormat format)
        {
            switch (format)
            {
                case ImageFileFormat.VASP:
                    {
                        return typeof(ParDatasetImporter);
                    }
                case ImageFileFormat.NRRD:
                    {
                        return typeof(SimpleITKImageFileImporter);
                    }
                case ImageFileFormat.NIFTI:
                    {
                        return typeof(SimpleITKImageFileImporter);
                    }
                default:
                    return null;
            }
        }
    }
}
