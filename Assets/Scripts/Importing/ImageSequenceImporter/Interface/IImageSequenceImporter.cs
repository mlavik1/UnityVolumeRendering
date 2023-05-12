using System.Collections.Generic;
using System.Threading.Tasks;

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

    public struct ImageSequenceImportSettings
    {
        public IProgressHandler progressHandler
        {
            get
            {
                return progressHandlerInternal ?? new NullProgressHandler();
            }

            set
            {
                progressHandlerInternal = value;
            }
        }

        private IProgressHandler progressHandlerInternal;
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
        /// Each series should be imported separately, resulting in one dataset per series. (mostly relevant for DICOM).
        /// <br/>
        /// Note: This will block the main thread until done. You may want to use <see cref="LoadSeriesAsync"/> instead.
        /// </summary>
        /// <param name="files">Files to load. Typically all the files stored in a specific (DICOM) directory.</param>
        /// <returns>List of image sequence series.</returns>
        IEnumerable<IImageSequenceSeries> LoadSeries(IEnumerable<string> files, ImageSequenceImportSettings settings = new ImageSequenceImportSettings());

        /// <summary>
        /// Asynchronously read a list of files, and return all image sequence series.
        /// Normally a directory will only contain a single series,
        ///  but if a folder contains multiple series/studies than this function will return all of them.
        /// Each series should be imported separately, resulting in one dataset per series. (mostly relevant for DICOM).
        /// </summary>
        /// <param name="files">Files to load. Typically all the files stored in a specific (DICOM) directory.</param>
        /// <returns>Async task, returning a list of image sequence series.</returns>
        Task<IEnumerable<IImageSequenceSeries>> LoadSeriesAsync(IEnumerable<string> files, ImageSequenceImportSettings settings = new ImageSequenceImportSettings());

        /// <summary>
        /// Import a single image sequence series.
        /// </summary>
        /// <param name="series">The series to import</param>
        /// <returns>Imported 3D volume dataset.</returns>
        VolumeDataset ImportSeries(IImageSequenceSeries series, ImageSequenceImportSettings settings = new ImageSequenceImportSettings());

        /// <summary>
        /// Asynchronousely import a single image sequence series.
        /// <br/>
        /// Note: This will block the main thread until done. You may want to use <see cref="ImportSeriesAsync"/> instead.
        /// </summary>
        /// <param name="series">The series to import</param>
        /// <returns>Imported 3D volume dataset.</returns>
        Task<VolumeDataset> ImportSeriesAsync(IImageSequenceSeries series, ImageSequenceImportSettings settings = new ImageSequenceImportSettings());
    }
}
