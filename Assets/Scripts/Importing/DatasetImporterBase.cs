namespace UnityVolumeRendering
{
    /// <summary>
    /// Base class for all dataset imports.
    /// If you want to add support for a new format, create a sublcass of this.
    /// </summary>
    public abstract class DatasetImporterBase
    {
        public abstract VolumeDataset Import();
    }
}
