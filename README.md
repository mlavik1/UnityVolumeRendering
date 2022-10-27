# UnityVolumeRendering
A volume renderer, made in Unity3D.
I have written a [tutorial explaining the basic implementation](https://matiaslavik.wordpress.com/2020/01/19/volume-rendering-in-unity/).
Have any questions? Create an issue or [contact me on Mastodon](https://fosstodon.org/web/accounts/106931417271268077).

I also have [a tutorial video that shows how to use the project](https://www.youtube.com/watch?v=1pIvmhXPh0w)

![alt tag](Screenshots/front.jpg)

# Requirements:
- Unity 2018 1.5 or newer (should also work with some older versions, but I haven't tested)

# How to use sample scene
- Open "TestScene.unity"
- Click "Volume Rendering" in the menu bar
- Select "Load Raw dataset" (or something else, if you already have a dataset you want to import)
- Pick a file in the "DataFiles" folder (I recommend VisMale.raw)
- Click the "import"-button

# Step-by-step instructions
**1. Import model**

**Raw datasets:**

In the menu bar, click "Volume Rendering" and "Load raw dataset"

<img src="Screenshots/menubar2.png" width="200px">

Then select the dataset you wish to import. Currently only raw datasets are supported (you can add your own importer for other datasets).

In the next menu you can optionally set the import setting for the raw dataset. For the sample files you don't need to change anything.

<img src="Screenshots/import.png" width="200px">

**DICOM:**

To import a DICOM dataset, click "Volume Rendering" and "Load DICOM" and select the folder containing your DICOM files.
The dataset must be of 3D nature, and contain several files - each being a slice along the Z axis.

**2. Moving the model**

You can move the model like any other GameObject. Simply select it in the scene view or scene hierarchy, and move/rotate it like normal.

<img src="Screenshots/movement.gif" width="400px">

**3. Changing the visualisation**

Select the model and find the "Volume Render Object" in the inspector.

Here you can change the "Render mode":

<img src="Screenshots/rendermode.png" width="200px">

Example:

<img src="Screenshots/rendermodes.gif" width="500px">

There are 3 render modes:
- Direct Volume Rendering (using transfer functions)
- Maximum Intensity Projection (shows the maximum density)
- Isosurface Rendering

There are also some other settings that you can adjust:
- "Enable lighting": Enable lighting calculations during volume rendering.
- Enable back-to-front direct volume rendering: Will raymarch the dataset back-to-front (towards camera). You normally *don't* want this.
- Enable early ray termination: Optimisation (you usually want this on). Requires the above setting to be disabled.
- Enable cubic interpolation: Use cubic interpolation of the 3D volume texture and gradient texture.

<img src="Screenshots/volume-inspector-settings.jpg" width="300px">

****

# Direct Volume Rendering

Direct volume rendering is the most standard rendering mode. It sends rays through the dataset, and uses "transfer functions" (1D or 2D) to determine the colour and opacity. Transfer functions map density (2D: also gradient magnitude) to a colour and opacity.
- **Modifying transfer functions**: Click "Volume Rendering" in the menu bar and select "1D Transfer Function" or "2D Transfer Function"
  - **1D Transfer Function**: X-axis represents density and Y-axis represents alpha (opaccity). Move the grey alpha knots to create a curve for opacity by density. Right-click to add new alpha knots. The bottom gradient-coloured panel maps colour to density. Right-click to add new knots and click on an existing colour knot to modify its colour.
  - **2D Transfer Function**: X-axis represents density and Y-axis represents gradient magnitude. Click "add rectangle" to add a new rectangle-shape. Move the four sliders (bottom left) to modify size/position. Modify the two sliders to the right to change min/max alpha/opacity. Each rectangle can have one colour (see colour picker).

# Isosurface Rendering

Isosurface rendering draws the first thing the ray hits, with a density higher than some threshold. You can set this threshold yourself, by selecting the object and changing the "Visible value range" in the inspector.
These can also be used with direct volume rendering mode.

<img src="Screenshots/isosurface.gif" width="500px">

# Importing DICOM and NRRD

If you're on Windows or Linux, I recommend [enabling the SimpleITK importer](Documentation/SimpleITK.md), which is a requirement for JPEG2000 compressed DICOM and NRRD.

# (VR) performance

Since VR requires two cameras to render each frame, you can expect worse performance. However, you can improve the FPS in two ways:
- Open _DirectVolumeRenderingShader.shader_ and reduce the value of _MAX_NUM_STEPS_ in the  _frag_dvr_, _frag_mip_ and _frag_surf_ functions. This will sacrifice quality for performance.
- Disable the DEPTHWRITE_ON shader variant. You can do this from code, or just remove the line "#pragma multi_compile DEPTHWRITE_ON DEPTHWRITE_OFF" in _DirectVolumeRenderingShader.shader_. Note: this will remove depth writing, so you won't be able to intersect multiple datasets.
- Make sure "Enable cubic interpolation" is checked on the volume object's inspector.

Your bottleneck will most likely be the pixel/fragment shader (where we do raymarching), so it might be possible to get better performance by enabling [DLSS](https://docs.unity3d.com/Manual/deep-learning-super-sampling.html). This requires HDRP, which this project currently does not officially support (might need to do some upgrading).

# How to use in your own project
- Create an instance of an importer (Directly, or indirectly using the `ImporterFactory`):<br>
`IImageFileImporter importer = ImporterFactory.CreateImageFileImporter(ImageFileFormat.NRRD);`
- Call the Import()-function, which returns a Dataset:<br>
`VolumeDataset dataset = importer.Import(file);`
- Use _VolumeObjectFactory_ to create an object from the dataset:<br> 
`VolumeRenderedObject obj = VolumeObjectFactory.CreateObject(dataset);`

See the [importer documentation](Documentation/Importing.md) for more detailed information.

![alt tag](Screenshots/slices.gif)
![alt tag](Screenshots/1.png)
![alt tag](Screenshots/2.png)
![alt tag](Screenshots/4.png)
![alt tag](Screenshots/5.png)
![alt tag](Screenshots/6.png)
![alt tag](Screenshots/regions.png)

# Contributing
See [CONTRIBUTING.md](CONTRIBUTING.md) for how to contribute.

Thanks to [everyone who have contributed so far](CREDITS.md).

See ACKNOWLEDGEMENTS.txt for libraries used by this project.
