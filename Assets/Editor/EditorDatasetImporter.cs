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

                        DICOMImporter importer = new DICOMImporter(fileCandidates, Path.GetFileName(directoryPath));

                        List<DICOMImporter.DICOMSeries> seriesList = importer.LoadDICOMSeries();
                        foreach (DICOMImporter.DICOMSeries series in seriesList)
                        {
                            // Only import the series that contains the selected file
                            if(series.dicomFiles.Any(f => Path.GetFileName(f.filePath) == Path.GetFileName(filePath)))
                            {
                                VolumeDataset dataset = importer.ImportDICOMSeries(series);

                                if (dataset != null)
                                {
                                    VolumeRenderedObject obj = VolumeObjectFactory.CreateObject(dataset);
                                }
                            }
                        }
                        break;
                    }
                case DatasetType.PARCHG:
                    {
                        ParDatasetImporterEditorWindow pnd = (ParDatasetImporterEditorWindow)EditorWindow.GetWindow(typeof(ParDatasetImporterEditorWindow));
                        if (pnd != null)
                            {
                                pnd.Close();
                            }
                        pnd = new ParDatasetImporterEditorWindow(filePath);
                        pnd.Show();
                        break;
                    }
            }
        }
    }
}
