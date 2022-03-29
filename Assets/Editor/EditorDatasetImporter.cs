using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

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

                        IImageSequenceImporter importer = ImporterFactory.CreateImageSequenceImporter(ImageSequenceFormat.DICOM);

                        IEnumerable<IImageSequenceSeries> seriesList = importer.LoadSeries(fileCandidates);
                        foreach (IImageSequenceSeries series in seriesList)
                        {
                            // Only import the series that contains the selected file
                            if(series.GetFiles().Any(f => Path.GetFileName(f.GetFilePath()) == Path.GetFileName(filePath)))
                            {
                                VolumeDataset dataset = importer.ImportSeries(series);

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
                        IImageFileImporter importer = ImporterFactory.CreateImageFileImporter(ImageFileFormat.VASP);
                        VolumeDataset dataset = importer.Import(filePath);

                        if (dataset != null)
                        {
                            VolumeRenderedObject obj = VolumeObjectFactory.CreateObject(dataset);
                        }
                        else
                        {
                            Debug.LogError("Failed to import datset");
                        }
                        break;
                    }
            }
        }
    }
}
