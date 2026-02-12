using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityVolumeRendering
{
    public class EditorDatasetImportUtils
    {
        public static async Task<VolumeDataset[]> ImportDicomDirectoryAsync(string dir, ProgressHandler progressHandler)
        {
            Debug.Log("Async dataset load. Hold on.");

            List<VolumeDataset> importedDatasets = new List<VolumeDataset>();
            bool recursive = true;

            // Read all files
            IEnumerable<string> fileCandidates = Directory.EnumerateFiles(dir, "*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                .Where(p => p.EndsWith(".dcm", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".dicom", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".dicm", StringComparison.InvariantCultureIgnoreCase));

            if (!fileCandidates.Any())
            {
                if (UnityEditor.EditorUtility.DisplayDialog("Could not find any DICOM files",
                    $"Failed to find any files with DICOM file extension.{Environment.NewLine}Do you want to include files without DICOM file extension?", "Yes", "No"))
                {
                    fileCandidates = Directory.EnumerateFiles(dir, "*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                }
            }

            if (fileCandidates.Any())
            {
                progressHandler.StartStage(0.2f, "Loading DICOM series");

                IImageSequenceImporter importer = ImporterFactory.CreateImageSequenceImporter(ImageSequenceFormat.DICOM);
                IEnumerable<IImageSequenceSeries> seriesList = await importer.LoadSeriesAsync(fileCandidates, new ImageSequenceImportSettings { progressHandler = progressHandler });

                progressHandler.EndStage();
                progressHandler.StartStage(0.8f);

                int seriesIndex = 0, numSeries = seriesList.Count();
                foreach (IImageSequenceSeries series in seriesList)
                {
                    progressHandler.StartStage(1.0f / numSeries, $"Importing series {seriesIndex + 1} of {numSeries}");
                    VolumeDataset dataset = await importer.ImportSeriesAsync(series, new ImageSequenceImportSettings { progressHandler = progressHandler });
                    if (dataset != null)
                    {
                        await OptionallyDownscale(dataset);
                        importedDatasets.Add(dataset);
                    }
                    seriesIndex++;
                    progressHandler.EndStage();
                }

                progressHandler.EndStage();
            }
            else
                Debug.LogError("Could not find any DICOM files to import.");

            return importedDatasets.ToArray();
        }

        public static async Task OptionallyDownscale(VolumeDataset dataset)
        {
            if (EditorPrefs.GetBool("DownscaleDatasetPrompt"))
            {
                if (EditorUtility.DisplayDialog("Optional DownScaling",
                    $"Do you want to downscale the dataset? The dataset's dimension is: {dataset.dimX} x {dataset.dimY} x {dataset.dimZ}", "Yes", "No"))
                {
                    Debug.Log("Async dataset downscale. Hold on.");
                    await Task.Run(() => dataset.DownScaleData());
                }
            }
        }
    }
}
