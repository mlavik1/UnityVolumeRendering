# Importing datasets form code

**Table of contents:**
<!-- TOC -->

- [Importing datasets form code](#importing-datasets-form-code)
    - [Raw importer](#raw-importer)
    - [Image file importer](#image-file-importer)
    - [Image sequence importer](#image-sequence-importer)
        - [Notes about DICOM support](#notes-about-dicom-support)

<!-- /TOC -->

There are 3 types of importers:
- Raw importer
    - Used for importing raw binary datasets.
    - These datasets can optionally have a header, followed by raw 3D data in various formats (int, uint, etc.).
- Image file importer
    - Used for importing a single file dataset.
    - Supported formats: VASP, NRRD; NIFTI
- Image sequence importer 
    - Used for importing sequences datasets, where each slice maybe be stored in a separate file (multiple files per dataset).

## Raw importer

The _RawDatasetImporter_ imports raw datasets, where the data is stored sequentially. Some raw datasets contain a header where you can read information about how the data is stored (content format, dimension, etc.), while some datasets expect you to know the layout and format.

To import a RAW dataset, do the following:

```csharp
// Create the importer
RawDatasetImporter importer = new RawDatasetImporter(filePath, initData.dimX, initData.dimY, initData.dimZ, initData.format, initData.endianness, initData.bytesToSkip);
// Import the dataset
VolumeDataset dataset = importer.Import();
// Spawn the object
VolumeObjectFactory.CreateObject(dataset);
```

The _RawDatasetImporter_ constructor takes the following parameters:
- filePath: File path to the dataset.
- dimX, dimY, dimZ: The dimension of the dataset.
- contentFormat: The format of the content. Possible values: Int8, Uint8, Int16, Uint16, Int32, Uint32.
- endianness: The byte [endianness](https://en.wikipedia.org/wiki/Endianness) of the dataset.
- skipBytes: Number of bytes to skip before reading the content. This is used in cases where the dataset has a header. Some raw datasets formats store information about the dimension, format and endianness in a header. To import these datasets you can read the header yourself and pass this info to the  _RawDatasetImporter_ constructor. The skipBytes parameter should then be equal to the header size.

All this info can be added to a ".ini"-file, which the importer will use (if it finds any). See the sample files (in the  "DataFiles" folder for an example).

## Image file importer

To import single-file datasets, such as VASP/PARCHG, NRRD and NIFTI, you can use one of the image file importers. You can manually create an instance of your desired importer, or you can simply use the `ImporterFactory` class, which will select one for your desired file format.

Example:

```csharp
IImageFileImporter importer = ImporterFactory.CreateImageFileImporter(ImageFileFormat.NRRD);
VolumeDataset dataset = importer.Import(file);
VolumeRenderedObject obj = VolumeObjectFactory.CreateObject(dataset);
```

Possible parameters to _ImporterFactory.CreateImageFileImporter_:
- ImageFileFormat.NRRD (requires [SimpleITK](../SimpleITK/SimpleITK.md))
- ImageFileFormat.NIFTI (requires [SimpleITK](../SimpleITK/SimpleITK.md))
- ImageFileFormat.VASP

The available importer implementations are:
- _ParDatasetImporter_: For VASP/PARCHG.
- SimpleITKImageFileImporter: For NRRD and NIFTI. Works on Windows and Linux (and hopefully MacOS too).

For more information about NRRD support, see the page about [SimpleITK](../SimpleITK/SimpleITK.md).

## Image sequence importer

To import an image sequence dataset, such as DICOM, you can manually create an instance of one of the image sequence importers or simply use the `ImporterFactory` class, which will select one for you.

Example:

```csharp
// Get all files in DICOM directory
List<string> filePaths = Directory.GetFiles(dir).ToList();
// Create importer
IImageSequenceImporter importer = ImporterFactory.CreateImageSequenceImporter(ImageSequenceFormat.DICOM);
// Load list of DICOM series (normally just one series)
IEnumerable<IImageSequenceSeries> seriesList = importer.LoadSeries(filePaths);
// There will usually just be one series
foreach(IImageSequenceSeries series in seriesList)
{
    // Import single DICOm series
    VolumeDataset dataset = importer.ImportSeries(series);
    VolumeObjectFactory.CreateObject(dataset);
}
```

These importers can import one or several _series_. In most cases there will only be one series. However, in DICOM each DICOM slice can be associated with a "series". This allows you to store several datasets in the same folder.

Supported formats:
- ImageSequenceFormat.DICOM
- ImageSequenceFormat.ImageSequence

The available importer implementations are:
- SimpleITKImageSequenceImporter: For DICOM (see [SimpleITK.md](../SimpleITK/SimpleITK.md) for more info.)
- DICOMImporter: For DICOM. Uses OpenDICOM library, and works on all platforms. This is the default when SimpleITK is disabled.
- ImageSequenceImporter: For image sequences (directory containing multiple image files, typically JPEG or PNG)

### Notes about DICOM support

The SimpleITK-based importer is the recommended way to import DICOM datasets, as it supports JPEG compression. See the [SimpleITK documentation](../SimpleITK/SimpleITK.md) for information about how to enable it. Once enabled, _ImporterFactory.CreateImageSequenceImporter_ will automatically return an importer of type `SimpleITKImageSequenceImporter`.

# Async import

Most of the importers also support asynchronous import. This is very useful for VR/AR applications where you defnitely don't want the import to freeze the whole application for too long.

To do async import, create and run an async Task that calls the `Async` version for the importer factory's import methods. Below is an example:

```csharp
private static async Task DicomImportDirectoryAsync(IEnumerable<string> files)
{
    
    using (ProgressHandler progressHandler = new ProgressHandler(new EditorProgressView()))
    {
        progressHandler.StartStage(0.2f, "Loading DICOM series");

        IImageSequenceImporter importer = ImporterFactory.CreateImageSequenceImporter(ImageSequenceFormat.DICOM);
        IEnumerable<IImageSequenceSeries> seriesList = await importer.LoadSeriesAsync(files, new ImageSequenceImportSettings { progressHandler = progressHandler });

        progressHandler.EndStage();
        progressHandler.StartStage(0.8f);

        int seriesIndex = 0, numSeries = seriesList.Count();
        foreach (IImageSequenceSeries series in seriesList)
        {
            progressHandler.StartStage(1.0f / numSeries, $"Importing series {seriesIndex + 1} of {numSeries}");
            VolumeDataset dataset = await importer.ImportSeriesAsync(series, new ImageSequenceImportSettings { progressHandler = progressHandler });
            progressHandler.EndStage();
        }

        progressHandler.EndStage();
    }
}
```

You can optionally pass in a progress handler, which is used to track the progress of the async import. The `ProgressView` is used to display the progress, either in the Unity Editor or in your own GUI. In the above example we use the `EditorProgressView`, which will show a progress bar in the editor - but you can also create your own.
