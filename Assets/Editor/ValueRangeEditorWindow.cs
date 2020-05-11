using UnityEditor;
using UnityEngine;

namespace UnityVolumeRendering
{
    public class ValueRangeEditorWindow : EditorWindow
    {
        [MenuItem("Volume Rendering/Value range")]
        static void ShowWindow()
        {
            ValueRangeEditorWindow wnd = new ValueRangeEditorWindow();
            wnd.Show();
        }

        private void OnGUI()
        {
            // Update selected object
            VolumeRenderedObject volRendObject = SelectionHelper.GetSelectedVolumeObject();
            if (volRendObject == null)
                volRendObject = GameObject.FindObjectOfType<VolumeRenderedObject>();
            if (volRendObject == null)
                return;

            EditorGUILayout.LabelField("Edit the visible value range (min/max value) with the slider.");

            // TODO: cache reference in VolumeRenderedObject?
            MeshRenderer mr = volRendObject.GetComponent<MeshRenderer>();
            Material mat = mr.sharedMaterial;
            float minVal = mat.GetFloat("_MinVal");
            float maxVal = mat.GetFloat("_MaxVal");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.MinMaxSlider("Visible value range", ref minVal, ref maxVal, 0.0f, 1.0f);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            mat.SetFloat("_MinVal", minVal);
            mat.SetFloat("_MaxVal", maxVal);
        }
    }
}
