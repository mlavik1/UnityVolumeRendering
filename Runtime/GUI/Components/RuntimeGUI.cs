using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            if (GUILayout.Button("Import RAW dataset"))
            {
                RuntimeFileBrowser.ShowOpenFileDialog(OnOpenRAWDatasetResultAsync, "DataFiles");
            }

            if(GUILayout.Button("Import PARCHG dataset"))
            {
                    RuntimeFileBrowser.ShowOpenFileDialog(OnOpenPARDatasetResultAsync, "DataFiles");
            }

            if (GUILayout.Button("Import DICOM dataset"))
            {
                RuntimeFileBrowser.ShowOpenDirectoryDialog(OnOpenDICOMDatasetResultAsync);
            }

            if (GUILayout.Button("Import NIFTI dataset"))
            {
                RuntimeFileBrowser.ShowOpenFileDialog(OnOpenNIFTIDatasetResultAsync);
            }

            if (GUILayout.Button("Import NRRD dataset"))
            {
                RuntimeFileBrowser.ShowOpenFileDialog(OnOpenNRRDDatasetResultAsync);
            }

            // Show button for opening the dataset editor (for changing the visualisation)
            if (GameObject.FindObjectOfType<VolumeRenderedObject>() != null && GUILayout.Button("Edit imported dataset"))
            {
                EditVolumeGUI.ShowWindow(GameObject.FindObjectOfType<VolumeRenderedObject>());
            }

            // Show button for opening the slicing plane editor (for changing the orientation and position)
            if (GameObject.FindObjectOfType<SlicingPlane>() != null && GUILayout.Button("Edit slicing plane"))
            {
                EditSliceGUI.ShowWindow(GameObject.FindObjectOfType<SlicingPlane>());
            }
            
            if (GUILayout.Button("Show distance measure tool"))
            {
                DistanceMeasureTool.ShowWindow();
            }

            GUILayout.EndVertical();
        }

        private async void OnOpenPARDatasetResultAsync(RuntimeFileBrowser.DialogResult result)
        {
            if (!result.cancelled)
            {
                Debug.Log("Async dataset load. Hold on.");

                DespawnAllDatasets();
                string filePath = result.path;
                IImageFileImporter parimporter = ImporterFactory.CreateImageFileImporter(ImageFileFormat.VASP);
                VolumeDataset dataset = await parimporter.ImportAsync(filePath);
                if (dataset != null)
                {
                    await VolumeObjectFactory.CreateObjectAsync(dataset);
                }
            }
        }

        private async void OnOpenRAWDatasetResultAsync(RuntimeFileBrowser.DialogResult result)
        {
            if (!result.cancelled)
            {
                Debug.Log("Async dataset load. Hold on.");

                // We'll only allow one dataset at a time in the runtime GUI (for simplicity)
                DespawnAllDatasets();

                // Did the user try to import an .ini-file? Open the corresponding .raw file instead
                string filePath = result.path;
                if (System.IO.Path.GetExtension(filePath) == ".ini")
                    filePath = filePath.Substring(0, filePath.Length - 4);

                // Parse .ini file
                DatasetIniData initData = DatasetIniReader.ParseIniFile(filePath + ".ini");
                if (initData != null)
                {
                    // Import the dataset
                    RawDatasetImporter importer = new RawDatasetImporter(filePath, initData.dimX, initData.dimY, initData.dimZ, initData.format, initData.endianness, initData.bytesToSkip);
                    VolumeDataset dataset = await importer.ImportAsync();
                    // Spawn the object
                    if (dataset != null)
                    {
                        await VolumeObjectFactory.CreateObjectAsync(dataset);
                    }
                }
            }
        }

        private async void OnOpenDICOMDatasetResultAsync(RuntimeFileBrowser.DialogResult result)
        {
            if (!result.cancelled)
            {
                Debug.Log("Async dataset load. Hold on.");

                // We'll only allow one dataset at a time in the runtime GUI (for simplicity)
                DespawnAllDatasets();

                bool recursive = true;

                // Read all files
                IEnumerable<string> fileCandidates = Directory.EnumerateFiles(result.path, "*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                    .Where(p => p.EndsWith(".dcm", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".dicom", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".dicm", StringComparison.InvariantCultureIgnoreCase));

                // Import the dataset
                IImageSequenceImporter importer = ImporterFactory.CreateImageSequenceImporter(ImageSequenceFormat.DICOM);
                IEnumerable<IImageSequenceSeries> seriesList = await importer.LoadSeriesAsync(fileCandidates);
                float numVolumesCreated = 0;
                foreach (IImageSequenceSeries series in seriesList)
                {
                    VolumeDataset dataset = await importer.ImportSeriesAsync(series);
                    // Spawn the object
                    if (dataset != null)
                    {
                        VolumeRenderedObject obj = await VolumeObjectFactory.CreateObjectAsync(dataset);
                        obj.transform.position = new Vector3(numVolumesCreated, 0, 0);
                        numVolumesCreated++;
                    }
                }
            }
        }

        private async void OnOpenNIFTIDatasetResultAsync(RuntimeFileBrowser.DialogResult result)
        {
            if (!result.cancelled)
            {
                IImageFileImporter importer = ImporterFactory.CreateImageFileImporter(ImageFileFormat.NIFTI);
                VolumeDataset dataset = await importer.ImportAsync(result.path);

                if (dataset != null)
                {
                    await VolumeObjectFactory.CreateObjectAsync(dataset);
                }
                else
                {
                    Debug.LogError("Failed to import datset");
                }
            }
        }

        private async void OnOpenNRRDDatasetResultAsync(RuntimeFileBrowser.DialogResult result)
        {
            if (!result.cancelled)
            {
                IImageFileImporter importer = ImporterFactory.CreateImageFileImporter(ImageFileFormat.NRRD);
                VolumeDataset dataset = await importer.ImportAsync(result.path);

                if (dataset != null)
                {
                    await VolumeObjectFactory.CreateObjectAsync(dataset);
                }
                else
                {
                    Debug.LogError("Failed to import datset");
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
