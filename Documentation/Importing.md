# Importing datasets

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
- ImageFileFormat.NRRD (requires [SimpleITK](SimpleITK.md))
- ImageFileFormat.NIFTI (requires [SimpleITK](SimpleITK.md))
- ImageFileFormat.VASP

The available importer implementations are:
- _ParDatasetImporter_: For VASP/PARCHG.
- SimpleITKImageFileImporter: For NRRD and NIFTI. Currently only works on Windows.

For more information about NRRD and NIFTI support, see the page about [SimpleITK](SimpleITK.md).

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
- SimpleITKImageSequenceImporter: For DICOM (see [SimpleITK.md](SimpleITK.md) for more info.)
- DICOMImporter: For DICOM. Uses OpenDICOM library, and works on all platforms. This is the default when SimpleITK is disabled.
- ImageSequenceImporter: For image sequences (directory containing multiple image files, typically JPEG or PNG)

### Notes about DICOM support

The SimpleITK-based importer is the recommended way to import DICOM datasets, as it supports JPEG compression. See the [SimpleITK documentation](SimpleITK.md) for information about how to enable it. Once enabled, _ImporterFactory.CreateImageSequenceImporter_ will automatically return an importer of type `SimpleITKImageSequenceImporter`.
