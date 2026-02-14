# Shadow volumes

To get more realistic rendering, you can optionally enable shadow volumes.

<img src="../../../Screenshots/shadow-volume.jpg" width="300px">

This can be enabled from the VolumeRenderedObject inspector in the editor, or by adding a `ShadowVolumeManager` component:

<img src="../../../Screenshots/volume-inspector-settings.jpg" width="300px">

## How it works

A shadow volume 3D texture is generated for the whole dataset.
A compute shader is repsonsible for updating the shadow volume, by casting rays from the light source through the dataset, and storing information about shadows.
Since this can take a long time, and async compute is not available on all platforms, the shadow volume manager divides the shadow volume into smaller chunks, and updates one chunk every frame.

## Performance

Because of the extra work of computing the shadow volume (compute shader) and the extra texture lookups during volume rendreing, this can have a bad impact on performance.

There are luckily some ways to work around this, at least on desktop applications (Windows, Linux, etc.).
- If you're using HDRP: [Enable DLSS](https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@12.0/manual/deep-learning-super-sampling-in-hdrp.html) and reduce the render scale.
- If you're using URP: [Enable FidelityFX Super Resolution](https://forum.unity.com/threads/amd-fidelityfx-super-resolution-fsr-preview-now-available.1141495/) and reduce the [render scale](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@10.1/manual/universalrp-asset.html).

This works by rendering to a smaller render target (which is usually the bottleneck during volume rendering) and then doing "magic" upscaling on the rendered image.
