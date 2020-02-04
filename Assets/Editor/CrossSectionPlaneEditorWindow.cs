using UnityEngine;
using UnityEditor;

namespace UnityVolumeRendering
{
    public class CrossSectionPlaneEditorWindow : EditorWindow
    {
        [MenuItem("Volume Rendering/Cross section")]
        static void OnMenuItemClick()
        {
            VolumeRenderedObject[] objects = GameObject.FindObjectsOfType<VolumeRenderedObject>();
            if (objects.Length == 1)
                SpawnCrossSectionPlane(objects[0]);
            else
            {
                CrossSectionPlaneEditorWindow wnd = new CrossSectionPlaneEditorWindow();
                wnd.Show();
            }
        }

        private static void SpawnCrossSectionPlane(VolumeRenderedObject volobj)
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.transform.rotation = Quaternion.Euler(270.0f, 0.0f, 0.0f);
            SlicingPlaneAnyDirection csplane = quad.gameObject.AddComponent<SlicingPlaneAnyDirection>();
            csplane.mat = volobj.GetComponent<MeshRenderer>().sharedMaterial;
            csplane.volumeTransform = volobj.transform;
            quad.transform.position = volobj.transform.position;

            Selection.objects = new Object[] { quad };
        }

        private void OnGUI()
        {
            VolumeRenderedObject[] spawnedObjects = GameObject.FindObjectsOfType<VolumeRenderedObject>();
            if (spawnedObjects.Length == 0)
            {
                EditorGUILayout.LabelField("Please load a dataset first.");
            }
            else
            {
                foreach (VolumeRenderedObject volobj in spawnedObjects)
                {
                    if (GUILayout.Button(volobj.gameObject.name))
                        SpawnCrossSectionPlane(volobj);
                }
            }
        }
    }
}
