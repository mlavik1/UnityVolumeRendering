# Manipulating spawned datasets

Datasets will be spawned as GameObjects, with a `VolumeRenderedObject` component attached to them.

You can move, rotate and scale these objects like any other GameObject in the Unity Editor.

<img src="movement.gif" width="400px">

# Saving spawned datasets

Spawned datasets can be saved as a part of the scene (simply save the scene).

However, if the datasets are large (or many) this will cause the scene asset to become very large, and saving/loading will be slow (or crash!).
To prevent this, you may consider importing your datasets as an Asset (see [import documentation](ImportingDatasets.md)) and referencing the already imported dataset through some script and spawning it through the [VolumeObjectFactory](../../Scripting/Importing.md).

# Changing the appearance settings

To change the appearance settings and other volume rendering related settings, see [the appearance settings documentation](VolumeRenderingSettings.md).
