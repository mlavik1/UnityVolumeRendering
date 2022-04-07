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

    /// <summary>
    /// Importer for image sequence datasets, such as DICOM and image sequences.
    /// These datasets usually contain one file per slice.
    /// </summary>
    public interface IImageSequenceImporter
    {
        /// <summary>
        /// Read a list of files, and return all image sequence series.
        /// Normally a directory will only contain a single series,
        ///  but if a folder contains multiple series/studies than this function will return all of them.
        /// Each series should be imported separately, resulting in one dataset per series. (mostly relevant for DICOM)
        /// </summary>
        /// <param name="files">Files to load. Typically all the files stored in a specific (DICOM) directory.</param>
        /// <returns>List of image sequence series.</returns>
        IEnumerable<IImageSequenceSeries> LoadSeries(IEnumerable<string> files);
        
        /// <summary>
        /// Import a single image sequence series.
        /// </summary>
        /// <param name="series">The series to import</param>
        /// <returns>Imported 3D volume dataset.</returns>
        VolumeDataset ImportSeries(IImageSequenceSeries series);
    }
}
