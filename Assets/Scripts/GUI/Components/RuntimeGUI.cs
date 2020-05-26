using UnityEngine;

namespace UnityVolumeRendering
{
    /// <summary>
    /// This is a basic runtime GUI, that can be used during play mode.
    /// You can import datasets, and edit them.
    /// Add this component to an empty GameObject in your scene (it's already in the test scene) and click play to see the GUI.
    /// </summary>
    public class RuntimeGUI : MonoBehaviour
    {
        private void OnGUI()
        {
            GUILayout.BeginVertical();

            // Show dataset import buttons
            if(GUILayout.Button("Import RAW dataset"))
            {
                RuntimeFileBrowser.ShowOpenFileDialog(OnOpenRAWDatasetResult, "DataFiles");
            }
            if (GUILayout.Button("Import DICOM dataset"))
            {
                RuntimeFileBrowser.ShowOpenDirectoryDialog(OnOpenDICOMDatasetResult);
            }

            // Show button for opening the dataset editor (for changing the visualisation)
            if (GameObject.FindObjectOfType<VolumeRenderedObject>() != null && GUILayout.Button("Edit imported dataset"))
            {
                EditVolumeGUI.ShowWindow(GameObject.FindObjectOfType<VolumeRenderedObject>());
            }

            GUILayout.EndVertical();
        }

        private void OnOpenRAWDatasetResult(RuntimeFileBrowser.DialogResult result)
        {
            if(!result.cancelled)
            {
                // We'll only allow one dataset at a time in the runtime GUI (for simplicity)
                DespawnAllDatasets();

                // Did the user try to import an .ini-file? Open the corresponding .raw file instead
                string filePath = result.path;
                if (System.IO.Path.GetExtension(filePath) == ".ini")
                    filePath = filePath.Replace(".ini", ".raw");

                // Parse .ini file
                DatasetIniData initData = DatasetIniReader.ParseIniFile(filePath + ".ini");
                if(initData != null)
                {
                    // Import the dataset
                    RawDatasetImporter importer = new RawDatasetImporter(filePath, initData.dimX, initData.dimY, initData.dimZ, initData.format, initData.endianness, initData.bytesToSkip);
                    VolumeDataset dataset = importer.Import();
                    // Spawn the object
                    if (dataset != null)
                    {
                        VolumeObjectFactory.CreateObject(dataset);
                    }
                }
            }
        }

        private void OnOpenDICOMDatasetResult(RuntimeFileBrowser.DialogResult result)
        {
            if (!result.cancelled)
            {
                // We'll only allow one dataset at a time in the runtime GUI (for simplicity)
                DespawnAllDatasets();

                // Import the dataset
                DICOMImporter importer = new DICOMImporter(result.path, true);
                VolumeDataset dataset = importer.Import();
                // Spawn the object
                if (dataset != null)
                {
                    VolumeObjectFactory.CreateObject(dataset);
                }
            }
        }

        private void DespawnAllDatasets()
        {
            VolumeRenderedObject[] volobjs = GameObject.FindObjectsOfType<VolumeRenderedObject>();
            foreach(VolumeRenderedObject volobj in volobjs)
            {
                GameObject.Destroy(volobj.gameObject);
            }
        }
    }
}
