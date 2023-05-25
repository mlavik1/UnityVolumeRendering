# UnityVolumeRendering
A volume renderer, made in Unity3D.
I have written a [tutorial explaining the basic implementation](https://matiaslavik.wordpress.com/2020/01/19/volume-rendering-in-unity/).
Have any questions? Create an issue or [contact me on Mastodon](https://fosstodon.org/@unityvolrend).

I also have [a tutorial video that shows how to use the project](https://www.youtube.com/watch?v=1pIvmhXPh0w)

![alt tag](Screenshots/front.jpg)

**`Documentation`** |
------------------- |
[See full documentation here](Documentation/Documentation.md)|

# Table of contents

This Readme contains a quick introduction to the library. For more info, see the [complete documentation](Documentation/Documentation.md).

- [Requirements](#requirements)
- [How to use sample scene](#how-to-use-sample-scene)
- [Step-by-step instructions](#step-by-step-instructions)
- [Direct Volume Rendering](#direct-volume-rendering)
- [Isosurface Rendering](#isosurface-rendering)
- [Importing DICOM and NRRD](#importing-dicom-and-nrrd)
- [How to use in your own project](#how-to-use-in-your-own-project)
- [FAQ (Frequently Asked Questions)](#faq-frequently-asked-questions)
- [Contributing](#contributing)

# Requirements
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

Then select the dataset you wish to import..

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

If you're on Windows or Linux, I recommend [enabling the SimpleITK importer](Documentation/SimpleITK/SimpleITK.md), which is a requirement for JPEG2000 compressed DICOM and NRRD.

# How to use in your own project
- Create an instance of an importer (Directly, or indirectly using the `ImporterFactory`):<br>
`IImageFileImporter importer = ImporterFactory.CreateImageFileImporter(ImageFileFormat.NRRD);`
- Call the Import()-function, which returns a Dataset:<br>
`VolumeDataset dataset = importer.Import(file);`
- Use _VolumeObjectFactory_ to create an object from the dataset:<br> 
`VolumeRenderedObject obj = VolumeObjectFactory.CreateObject(dataset);`

See the [importer documentation](Documentation/Scripting/Importing.md) for more detailed information.

# FAQ (Frequently Asked Questions)
- [How to preserve real world scale of my datasets?](#how-to-preserve-real-world-scale-of-my-datasets)
- [Does this work in VR?](#does-this-work-in-vr)
  - [What about VR performance?](#what-about-vr-performance)
- [Can I use WebGL?](#can-i-use-webgl)
- [Is this project free to use?](#is-this-project-free-to-use)
- [How can I make it look better?](#how-can-i-make-it-look-better)
- [How can I raycast the scene to find an intersection?](#how-can-i-raycast-the-scene-to-find-an-intersection)
- [I'm stuck! How can I get help?](#im-stuck-how-can-i-get-help)

# How to preserve real world scale of my datasets?
Imported datasets are automatically normalised, to make sure datasets where the scale unit info is missing or wrong don't become too large or small.
You can undy this simply by setting the scale of the outer GameObject (the one containing the `VolumeRenderedObject` component) to 1,1,1.

## Does this work in VR?
Yes, hoewever you will need to change "stereo rendering mode" to "multi pass" in the XR settings in Unity. See [#71](https://github.com/mlavik1/UnityVolumeRendering/issues/71).

### What about VR performance?
Since VR requires two cameras to render each frame, you can expect worse performance. However, you can improve the FPS in two ways:
- Open _DirectVolumeRenderingShader.shader_ and reduce the value of _MAX_NUM_STEPS_ in the  _frag_dvr_, _frag_mip_ and _frag_surf_ functions. This will sacrifice quality for performance.
- Disable the DEPTHWRITE_ON shader variant. You can do this from code, or just remove the line "#pragma multi_compile DEPTHWRITE_ON DEPTHWRITE_OFF" in _DirectVolumeRenderingShader.shader_. Note: this will remove depth writing, so you won't be able to intersect multiple datasets.
- Make sure "Enable cubic interpolation" is checked on the volume object's inspector.

Your bottleneck will most likely be the pixel/fragment shader (where we do raymarching), so it might be possible to get better performance by enabling [DLSS](https://docs.unity3d.com/Manual/deep-learning-super-sampling.html). This requires HDRP, which this project currently does not officially support (but it might still work fine).

Also, some users have reporter having significantly lower performance with OpenXR mode, compared to OpenVR. It might we worth a try to switch between these.

## Can I use WebGL?
Yes! But keep in mind that memory will be limited, so you might not be able to load very large datasets.

I recommend that you [enable ALLOW_MEMORY_GROWTH](https://github.com/mlavik1/UnityVolumeRendering/issues/125#issuecomment-1307765842). See [#125](https://github.com/mlavik1/UnityVolumeRendering/issues/125) for more info.

Also, since WebGL builds do not have access to your local filesystem, you will not be able to upload files directly (using the runtime GUI in the sample scene, etc.). You can either:
- Import the dataset in the editor, save the scene, and create a build with the scene containing the already imported dataset.
- Create prefabs for all the datasets you want, and make a build where you spawn these on demand.
- Use [UnityWebRequest](https://docs.unity3d.com/ScriptReference/Networking.UnityWebRequest.html) to download the files from somewhere.

## Is this project free to use?
Yes, it's free even for commercial projects. The license ([MIT](https://choosealicense.com/licenses/mit/)) only requires attribution and a copyright/license notice.

## How can I make it look better?
- Try [enabling cubic sampling](https://github.com/mlavik1/UnityVolumeRendering/pull/121#issuecomment-1281289885) in the inspector.
- Try increasing the value of "MAX_NUM_STEPS" in the [DirectVolumeRenderingShader.shader](https://github.com/mlavik1/UnityVolumeRendering/blob/master/Assets/Shaders/DirectVolumeRenderingShader.shader)

## How can I raycast the scene to find an intersection?
- See the [VolumeRaycasting documentation](Documentation/Scripting/VolumeRaycasting.md).

## I'm stuck! How can I get help?
[Create an issue](https://github.com/mlavik1/UnityVolumeRendering/issues).
You can also reach me on [Mastodon](https://floss.social/@sigsegv).

# Contributing
See [CONTRIBUTING.md](CONTRIBUTING.md) for how to contribute.

Thanks to [everyone who have contributed so far](CREDITS.md).

See ACKNOWLEDGEMENTS.txt for libraries used by this project.
