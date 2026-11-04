using UnityEngine;
using UnityEditor;
using UnityEngine.Scripting.APIUpdating;

namespace MingRay
{
    [MovedFrom(true, sourceNamespace: "UnityVolumeRendering")]
    public class CrossSectionPlaneEditorWindow : EditorWindow
    {
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
