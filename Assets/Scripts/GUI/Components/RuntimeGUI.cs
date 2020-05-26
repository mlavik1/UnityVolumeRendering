using UnityEngine;

namespace UnityVolumeRendering
{
    public class RuntimeGUI : MonoBehaviour
    {
        private void OnGUI()
        {
            GUILayout.BeginVertical();

            if(GUILayout.Button("Import RAW dataset"))
            {
                RuntimeFileBrowser.ShowOpenFileDialog(OnOpenRAWDatasetResult, "DataFiles");
            }
            if (GUILayout.Button("Import DICOM dataset"))
            {
                RuntimeFileBrowser.ShowOpenDirectoryDialog(OnOpenDICOMDatasetResult);
            }

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
                DespawnAllDatasets();

                string filePath = result.path;
                if (System.IO.Path.GetExtension(filePath) == ".ini")
                    filePath = filePath.Replace(".ini", ".raw");

                DatasetIniData initData = DatasetIniReader.ParseIniFile(filePath + ".ini");
                if(initData != null)
                {
                    RawDatasetImporter importer = new RawDatasetImporter(filePath, initData.dimX, initData.dimY, initData.dimZ, initData.format, initData.endianness, initData.bytesToSkip);
                    VolumeDataset dataset = importer.Import();
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
                DespawnAllDatasets();

                DICOMImporter importer = new DICOMImporter(result.path, true);
                VolumeDataset dataset = importer.Import();
                if(dataset != null)
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
