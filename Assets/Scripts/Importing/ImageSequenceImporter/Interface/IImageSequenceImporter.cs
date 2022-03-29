using System.Collections.Generic;

namespace UnityVolumeRendering
{
    public enum ImageSequenceFormat
    {
        ImageSequence,
        DICOM
    }

    public interface IImageSequenceFile
    {
        string GetFilePath();
    }

    public interface IImageSequenceSeries
    {
        IEnumerable<IImageSequenceFile> GetFiles();
    }

    public interface IImageSequenceImporter
    {
        IEnumerable<IImageSequenceSeries> LoadSeries(IEnumerable<string> files);
        VolumeDataset ImportSeries(IImageSequenceSeries series);
    }
}
