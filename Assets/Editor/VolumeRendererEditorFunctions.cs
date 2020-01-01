using UnityEditor;
using UnityEngine;
using System.IO;

public class VolumeRendererEditorFunctions
{
    [MenuItem("Volume Rendering/Load dataset")]
    static void ShowWindow()
    {
        string file = EditorUtility.OpenFilePanel("Select a dataset to load", "DataFiles", "");
        if(File.Exists(file))
        {
            DatasetImporterEditorWindow wnd = (DatasetImporterEditorWindow)EditorWindow.GetWindow(typeof(DatasetImporterEditorWindow));
            if (wnd != null)
                wnd.Close();

            wnd = new DatasetImporterEditorWindow(file);
            wnd.Show();
        }
        else
        {
            Debug.LogError("File doesn't exist: " + file);
        }
    }
}
