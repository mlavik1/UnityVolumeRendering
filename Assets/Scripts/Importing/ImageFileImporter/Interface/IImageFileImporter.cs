using System;

namespace UnityVolumeRendering
{
    public enum ImageFileFormat
    {
        VASP,
        NRRD,
        NIFTI
    }

    public interface IImageFileImporter
    {
        VolumeDataset Import(String filePath);
    }
}
