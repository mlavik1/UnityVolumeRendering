using UnityEditor;
using UnityEngine;
using System.IO;

namespace UnityVolumeRendering
{
    public class VolumeRendererEditorFunctions
    {
        [MenuItem("Volume Rendering/Load raw dataset")]
        static void ShowDatasetImporter()
        {
            string file = EditorUtility.OpenFilePanel("Select a dataset to load", "DataFiles", "");
            if (File.Exists(file))
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

        [MenuItem("Volume Rendering/Load DICOM")]
        static void ShowDICOMImporter()
        {
            string dir = EditorUtility.OpenFolderPanel("Select a folder to load", "", "");
            if (Directory.Exists(dir))
            {
                DICOMImporter importer = new DICOMImporter(dir, true);
                VolumeDataset dataset = importer.Import();
                if(dataset != null)
                    VolumeObjectFactory.CreateObject(dataset);
            }
            else
            {
                Debug.LogError("Directory doesn't exist: " + dir);
            }
        }
    }
}
