using UnityEngine;

namespace MingRay
{
    internal static class SimpleITKRegistration
    {
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Register()
        {
            ImporterFactory.RegisterImageSequenceImporter(ImageSequenceFormat.ImageSequence, typeof(SimpleITKImageSequenceImporter));
            ImporterFactory.RegisterImageSequenceImporter(ImageSequenceFormat.DICOM, typeof(SimpleITKDICOMImporter));
            ImporterFactory.RegisterImageFileImporter(ImageFileFormat.NRRD, typeof(SimpleITKImageFileImporter));
            ImporterFactory.RegisterImageFileImporter(ImageFileFormat.NIFTI, typeof(SimpleITKImageFileImporter));
            ImporterFactory.RegisterImageFileImporter(ImageFileFormat.Unknown, typeof(SimpleITKImageFileImporter));
            GradientComputator.SetSmoothingFilter(SimpleITKSmoothingFilter.Apply);
        }
    }
}
