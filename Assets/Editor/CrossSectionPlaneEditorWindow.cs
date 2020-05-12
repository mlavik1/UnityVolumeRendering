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
                VolumeObjectFactory.SpawnCrossSectionPlane(objects[0]);
            else
            {
                CrossSectionPlaneEditorWindow wnd = new CrossSectionPlaneEditorWindow();
                wnd.Show();
            }
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
                        VolumeObjectFactory.SpawnCrossSectionPlane(volobj);
                }
            }
        }
    }
}
