using System;
using System.Threading.Tasks;
using UnityEngine.Scripting.APIUpdating;

namespace MingRay
{
    [MovedFrom(true, sourceNamespace: "UnityVolumeRendering")]
    public enum ImageFileFormat
    {
        VASP,
        NRRD,
        NIFTI,
        Unknown
    }

    /// <summary>
    /// Interface for single file dataset importers (NRRD, NIFTI, etc.).
    /// These datasets contain only one single file.
    /// </summary>
    [MovedFrom(true, sourceNamespace: "UnityVolumeRendering")]
    public interface IImageFileImporter
    {
        VolumeDataset Import(String filePath);
        Task<VolumeDataset> ImportAsync(String filePath);
    }
}
