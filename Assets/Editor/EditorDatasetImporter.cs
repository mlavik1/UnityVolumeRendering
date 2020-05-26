using System.IO;
using UnityEditor;

namespace UnityVolumeRendering
{
    public class EditorDatasetImporter
    {
        public static void ImportDataset(string filePath)
        {
            DatasetType datasetType = DatasetImporterUtility.GetDatasetType(filePath);

            switch (datasetType)
            {
                case DatasetType.Raw:
                    {
                        RAWDatasetImporterEditorWindow wnd = (RAWDatasetImporterEditorWindow)EditorWindow.GetWindow(typeof(RAWDatasetImporterEditorWindow));
                        if (wnd != null)
                            wnd.Close();

                        wnd = new RAWDatasetImporterEditorWindow(filePath);
                        wnd.Show();
                        break;
                    }
                case DatasetType.DICOM:
                    {
                        DatasetImporterBase importer = new DICOMImporter(new FileInfo(filePath).Directory.FullName, false);
                        VolumeDataset dataset = importer.Import();

                        if (dataset != null)
                        {
                            VolumeRenderedObject obj = VolumeObjectFactory.CreateObject(dataset);
                        }
                        break;
                    }
            }
        }
    }
}
