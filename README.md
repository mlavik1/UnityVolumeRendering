# UnityVolumeRendering
A volume renderer, made in Unity3D. See slides from presentation here: https://speakerdeck.com/mlavik1/volume-rendering-in-unity3d

![alt tag](https://github.com/mlavik1/UnityVolumeRendering/blob/master/Screenshots/1.png)
![alt tag](https://github.com/mlavik1/UnityVolumeRendering/blob/master/Screenshots/2.png)
![alt tag](https://github.com/mlavik1/UnityVolumeRendering/blob/master/Screenshots/3.png)
![alt tag](https://github.com/mlavik1/UnityVolumeRendering/blob/master/Screenshots/4.png)
![alt tag](https://github.com/mlavik1/UnityVolumeRendering/blob/master/Screenshots/5.png)

# Requirements:
- Unity 2018 1.5 (should also work with other versions, but I haven't tested)

# How to use sample scene
- Open "TestScene.unity"
- Click "Volume Rendering" in the menu bar
- Select "Load Asset"
- Pick a file in the "DataFiles" folder (I recommend manix.dat)
- Click the "import"-button

# How to use in your own project
- Create an instance of an importer (for example _RawDatasetImporter_):<br>
`DatasetImporterBase importer = new RawDatasetImporter(fileToImport, dimX, dimY, dimZ, DataContentFormat.Int16, 6);`
- Call the Import()-function, which returns a Dataset:<br>
`VolumeDataset dataset = importer.Import();`
- Use _VolumeObjectFactory_ to create an object from the dataset:<br> 
`VolumeRenderedObject obj = VolumeObjectFactory.CreateObject(dataset);`

See "DatasetImporterEditorWIndow.cs" for an example.

# Note:
THe _RawDatasetImporter_ imports raw datasets, where the data is stored sequentially. Some raw datasets contain a header where you can read information about how the data is stored (content format, dimension, etc.), while some datasets expect you to know the layout and format.
The importer takes the following parameters:
- filePath: Filepath of the dataset
- dimX: X-dimension (number of samples in the X-axis)
- dimY: Y-dimension
- dimZ: Z-dimension
- contentFormat: Value type of the data (Int8, Uint8, Int16, Uint16, etc..)
- skipBytes: Number of bytes to skip (offset to where the data begins). This is usually the same as the header size, and will be 0 if there is no header.

# Todo:
- DICOM support
- Optimise histogram generation
- Support very large datasets (currently we naively try to create 3D textures with the same dimension as the data)

![alt tag](https://github.com/mlavik1/UnityVolumeRendering/blob/master/Screenshots/6.png)
![alt tag](https://github.com/mlavik1/UnityVolumeRendering/blob/master/Screenshots/7.png)
