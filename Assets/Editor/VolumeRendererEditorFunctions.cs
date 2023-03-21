using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace UnityVolumeRendering
{
    public class VolumeRendererEditorFunctions
    {
        [MenuItem("Volume Rendering/Load dataset/Load raw dataset")]
        static void ShowDatasetImporter()
        {
            string file = EditorUtility.OpenFilePanel("Select a dataset to load", "DataFiles", "");
            if (File.Exists(file))
            {
                RAWDatasetImporterEditorWindow wnd = (RAWDatasetImporterEditorWindow)EditorWindow.GetWindow(typeof(RAWDatasetImporterEditorWindow));
                if (wnd != null)
                    wnd.Close();

                wnd = new RAWDatasetImporterEditorWindow(file);
                wnd.Show();
            }
            else
            {
                Debug.LogError("File doesn't exist: " + file);
            }
        }

        [MenuItem("Volume Rendering/Load dataset/Load DICOM")]
        static void ShowDICOMImporter()
        {
            DicomImportAsync();
        }

        static async void DicomImportAsync()
        {
            string dir = EditorUtility.OpenFolderPanel("Select a folder to load", "", "");
            if (Directory.Exists(dir))
            {
                Debug.Log("Async dataset load. Hold on.");
                using (ProgressHandler progressHandler = new ProgressHandler(new EditorProgressView()))
                {
                    await DicomImportDirectoryAsync(dir, progressHandler);
                    progressHandler.Finish();
                }
            }
            else
            {
                Debug.LogError("Directory doesn't exist: " + dir);
            }
        }

        static async Task DicomImportDirectoryAsync(string dir, ProgressHandler progressHandler)
        {
            Debug.Log("Async dataset load. Hold on.");
            progressHandler.Start("DICOM import", "Importing DICOM...");

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
                progressHandler.StartStage(0.6f, "Loading DICOM series");

                IImageSequenceImporter importer = ImporterFactory.CreateImageSequenceImporter(ImageSequenceFormat.DICOM);
                IEnumerable<IImageSequenceSeries> seriesList = await importer.LoadSeriesAsync(fileCandidates, new ImageSequenceImportSettings { progressHandler = progressHandler });
                float numVolumesCreated = 0;

                progressHandler.EndStage();
                progressHandler.StartStage(0.4f);

                int seriesIndex = 0, numSeries = seriesList.Count();
                foreach (IImageSequenceSeries series in seriesList)
                {
                    progressHandler.ReportProgress(seriesIndex, numSeries, $"Importing series {seriesIndex} of {numSeries}");
                    progressHandler.StartStage(0.8f);
                    VolumeDataset dataset = await importer.ImportSeriesAsync(series, new ImageSequenceImportSettings { progressHandler = progressHandler });
                    progressHandler.EndStage();
                    if (dataset != null)
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

                        VolumeRenderedObject obj = await VolumeObjectFactory.CreateObjectAsync(dataset);
                        obj.transform.position = new Vector3(numVolumesCreated, 0, 0);
                        numVolumesCreated++;
                    }
                    seriesIndex++;
                }

                progressHandler.EndStage();
            }
            else
                Debug.LogError("Could not find any DICOM files to import.");
        }

        [MenuItem("Volume Rendering/Load dataset/Load NRRD dataset")]
        static void ShowNRRDDatasetImporter()
        {
            if (!SimpleITKManager.IsSITKEnabled())
            {
                if (EditorUtility.DisplayDialog("Missing SimpleITK", "You need to download SimpleITK to load NRRD datasets from the import settings menu.\n" +
                    "Do you want to open the import settings menu?", "Yes", "No"))
                {
                    ImportSettingsEditorWindow.ShowWindow();
                }
                return;
            }

            ImportNRRDDatasetAsync();
        }

        static async void ImportNRRDDatasetAsync()
        {
            string file = EditorUtility.OpenFilePanel("Select a dataset to load (.nrrd)", "DataFiles", "");
            if (File.Exists(file))
            {
                Debug.Log("Async dataset load. Hold on.");
                using (ProgressHandler progressHandler = new ProgressHandler(new EditorProgressView()))
                {
                    progressHandler.Start("NRRD import", "Importing NRRD...");
                    progressHandler.ReportProgress(0.0f, "Importing NRRD dataset");

                    IImageFileImporter importer = ImporterFactory.CreateImageFileImporter(ImageFileFormat.NRRD);
                    VolumeDataset dataset = await importer.ImportAsync(file);

                    progressHandler.ReportProgress(0.8f, "Creating object");
                    if (dataset != null)
                    {
                        VolumeRenderedObject obj = await VolumeObjectFactory.CreateObjectAsync(dataset);
                    }
                    else
                    {
                        Debug.LogError("Failed to import datset");
                    }
                    progressHandler.Finish();
                }
            }
            else
            {
                Debug.LogError("File doesn't exist: " + file);
            }
        }

        [MenuItem("Volume Rendering/Load dataset/Load NIFTI dataset")]
        static void ShowNIFTIDatasetImporter()
        {
            ImportNIFTIDatasetAsync();
        }

        static async void ImportNIFTIDatasetAsync()
        {
            string file = EditorUtility.OpenFilePanel("Select a dataset to load (.nii)", "DataFiles", "");
            if (File.Exists(file))
            {
                Debug.Log("Async dataset load. Hold on.");
                using (ProgressHandler progressHandler = new ProgressHandler(new EditorProgressView()))
                {
                    progressHandler.Start("NIfTI import", "Importing NIfTI...");
                    progressHandler.ReportProgress(0.0f, "Importing NIfTI dataset");

                    IImageFileImporter importer = ImporterFactory.CreateImageFileImporter(ImageFileFormat.NIFTI);
                    VolumeDataset dataset = await importer.ImportAsync(file);

                    progressHandler.ReportProgress(0.0f, "Creating object");

                    if (dataset != null)
                    {
                        VolumeRenderedObject obj = await VolumeObjectFactory.CreateObjectAsync(dataset);
                    }
                    else
                    {
                        Debug.LogError("Failed to import datset");
                    }
                    progressHandler.Finish();
                }
            }
            else
            {
                Debug.LogError("File doesn't exist: " + file);
            }
        }

        [MenuItem("Volume Rendering/Load dataset/Load PARCHG dataset")]
        static void ShowParDatasetImporter()
        {
            ImportParDatasetAsync();
        }

        static async void ImportParDatasetAsync()
        {
            string file = EditorUtility.OpenFilePanel("Select a dataset to load", "DataFiles", "");
            if (File.Exists(file))
            {
                Debug.Log("Async dataset load. Hold on.");
                using (ProgressHandler progressHandler = new ProgressHandler(new EditorProgressView()))
                {
                    progressHandler.Start("VASP import", "Importing VASP...");
                    progressHandler.ReportProgress(0.0f, "Importing VASP dataset");

                    IImageFileImporter importer = ImporterFactory.CreateImageFileImporter(ImageFileFormat.VASP);
                    VolumeDataset dataset = await importer.ImportAsync(file);

                    progressHandler.ReportProgress(0.0f, "Creating object");

                    if (dataset != null)
                    {
                        VolumeRenderedObject obj = await VolumeObjectFactory.CreateObjectAsync(dataset);
                    }
                    else
                    {
                        Debug.LogError("Failed to import datset");
                    }
                    progressHandler.Finish();
                }
            }
            else
            {
                Debug.LogError("File doesn't exist: " + file);
            }
        }

        [MenuItem("Volume Rendering/Load dataset/Load image sequence")]
        static void ShowSequenceImporter()
        {
            ImportSequenceAsync();
        }

        static async void ImportSequenceAsync()
        {
            string dir = EditorUtility.OpenFolderPanel("Select a folder to load", "", "");

            if (Directory.Exists(dir))
            {
                Debug.Log("Async dataset load. Hold on.");

                List<string> filePaths = Directory.GetFiles(dir).ToList();
                IImageSequenceImporter importer = ImporterFactory.CreateImageSequenceImporter(ImageSequenceFormat.ImageSequence);

                IEnumerable<IImageSequenceSeries> seriesList = await importer.LoadSeriesAsync(filePaths);

                foreach (IImageSequenceSeries series in seriesList)
                {
                    VolumeDataset dataset = await importer.ImportSeriesAsync(series);
                    if (dataset != null)
                    {
                        if (EditorPrefs.GetBool("DownscaleDatasetPrompt"))
                        {
                            if (EditorUtility.DisplayDialog("Optional DownScaling",
                                $"Do you want to downscale the dataset? The dataset's dimension is: {dataset.dimX} x {dataset.dimY} x {dataset.dimZ}", "Yes", "No"))
                            {
                                Debug.Log("Async dataset downscale. Hold on.");
                                await Task.Run(()=>dataset.DownScaleData());
                            }
                        }
                        await VolumeObjectFactory.CreateObjectAsync(dataset);
                    }
                }
            }
            else
            {
                Debug.LogError("Directory doesn't exist: " + dir);
            }
        }

        [MenuItem("Volume Rendering/Cross section/Cross section plane")]
        static void OnMenuItemClick()
        {
            VolumeRenderedObject[] objects = GameObject.FindObjectsOfType<VolumeRenderedObject>();
            if (objects.Length == 1)
                VolumeObjectFactory.SpawnCrossSectionPlane(objects[0]);
            else
            {
                CrossSectionPlaneEditorWindow wnd = new CrossSectionPlaneEditorWindow();
                wnd.Show();
            }
        }

        [MenuItem("Volume Rendering/Cross section/Box cutout")]
        static void SpawnCutoutBox()
        {
            VolumeRenderedObject[] objects = GameObject.FindObjectsOfType<VolumeRenderedObject>();
            if (objects.Length == 1)
                VolumeObjectFactory.SpawnCutoutBox(objects[0]);
        }

        [MenuItem("Volume Rendering/1D Transfer Function")]
        public static void Show1DTFWindow()
        {
            VolumeRenderedObject volRendObj = SelectionHelper.GetSelectedVolumeObject();
            if (volRendObj != null)
            {
                volRendObj.SetTransferFunctionMode(TFRenderMode.TF1D);
                TransferFunctionEditorWindow.ShowWindow(volRendObj);
            }
            else
            {
                EditorUtility.DisplayDialog("No imported dataset", "You need to import a dataset first", "Ok");
            }
        }

        [MenuItem("Volume Rendering/2D Transfer Function")]
        public static void Show2DTFWindow()
        {
            TransferFunction2DEditorWindow.ShowWindow();
        }

        [MenuItem("Volume Rendering/Slice renderer")]
        static void ShowSliceRenderer()
        {
            SliceRenderingEditorWindow.ShowWindow();
        }

        [MenuItem("Volume Rendering/Value range")]
        static void ShowValueRangeWindow()
        {
            ValueRangeEditorWindow.ShowWindow();
        }

        [MenuItem("Volume Rendering/Settings")]
        static void ShowSettingsWindow()
        {
            ImportSettingsEditorWindow.ShowWindow();
        }
    }
}
