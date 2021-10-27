using UnityEditor;
using UnityEngine;

namespace UnityVolumeRendering
{
    public class ImportSettingsEditorWindow : EditorWindow
    {
        public static void ShowWindow()
        {
            ImportSettingsEditorWindow wnd = new ImportSettingsEditorWindow();
            wnd.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Show promt asking if you want to downscale the dataset on import?");
            bool showDownscalePrompt = EditorGUILayout.Toggle("Show downscale prompt", EditorPrefs.GetBool("DownscaleVolumePrompt"));
            EditorPrefs.SetBool("DownscaleVolumePrompt", showDownscalePrompt);
        }
    }
}
