using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                        string directoryPath = new FileInfo(filePath).Directory.FullName;

                        // Find all DICOM files in directory
                        IEnumerable<string> fileCandidates = Directory.EnumerateFiles(directoryPath, "*.*", SearchOption.TopDirectoryOnly)
                            .Where(p => p.EndsWith(".dcm", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".dicom", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".dicm", StringComparison.InvariantCultureIgnoreCase));

                        DatasetImporterBase importer = new DICOMImporter(fileCandidates, Path.GetFileName(directoryPath));
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
