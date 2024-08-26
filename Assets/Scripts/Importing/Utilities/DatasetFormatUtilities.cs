namespace UnityVolumeRendering
{
    public class DatasetFormatUtilities
    {
        public static ImageFileFormat GetImageFileFormat(string filePath)
        {
            string extension = System.IO.Path.GetExtension(filePath);
            switch (extension)
            {
                case ".nrrd":
                    return ImageFileFormat.NRRD;
                case ".vasp":
                    return ImageFileFormat.VASP;
                case ".nii":
                    return ImageFileFormat.NIFTI;
                default:
                    return ImageFileFormat.Unknown;
            }
        }
    }
}
