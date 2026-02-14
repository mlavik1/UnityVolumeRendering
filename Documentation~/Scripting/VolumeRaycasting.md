# Volume raycasts (finding intersections with the volume)

To find ray intersections with the volume, for example to find out where on the volume the user has clicked, you can use the `VolumeRaycaster` class.

For an example, see the `DistanceMeasureTool` class, which can be used from the SampleScene.

## Example:

```csharp
// Create a ray form where the user clicked
Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
// Create a raycaster instance
VolumeRaycaster raycaster = new VolumeRaycaster();
// Raycast the scene, with our ray. The "hit" output variable will contain the result, if any.
if (raycaster.RaycastScene(ray, out RaycastHit hit))
{
    // Debug draw a line representing the ray (from eye to hit point). Only visible in the scene view.
    Debug.DrawLine(ray.origin, hit.point, Color.red, 10.0f, true);
}
```