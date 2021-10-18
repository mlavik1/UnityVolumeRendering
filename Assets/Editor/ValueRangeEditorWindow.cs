using UnityEditor;
using UnityEngine;

namespace UnityVolumeRendering
{
    public class ValueRangeEditorWindow : EditorWindow
    {
        public static void ShowWindow()
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

            Vector2 visibilityWindow = volRendObject.GetVisibilityWindow();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.MinMaxSlider("Visible value range", ref visibilityWindow.x, ref visibilityWindow.y, 0.0f, 1.0f);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();

            volRendObject.SetVisibilityWindow(visibilityWindow);
        }
    }
}
