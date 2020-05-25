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
                RuntimeFileBrowser.ShowOpenFileDialog(OnOpenRAWDatasetResult);
            }
            if (GUILayout.Button("Import DICOM dataset"))
            {
                RuntimeFileBrowser.ShowOpenDirectoryDialog(OnOpenDICOMDatasetResult);
            }

            GUILayout.EndVertical();
        }

        private void OnOpenRAWDatasetResult(RuntimeFileBrowser.DialogResult result)
        {
            if(!result.cancelled)
            {
                // TODO
            }
        }

        private void OnOpenDICOMDatasetResult(RuntimeFileBrowser.DialogResult result)
        {
            if (!result.cancelled)
            {
                DICOMImporter importer = new DICOMImporter(result.path, true);
                VolumeDataset dataset = importer.Import();
                if(dataset != null)
                {
                    VolumeObjectFactory.CreateObject(dataset);
                }
            }
        }
    }
}
