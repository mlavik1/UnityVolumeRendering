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
        private static void ShowDatasetImporter()
        {
            string file = EditorUtility.OpenFilePanel("Select a dataset to load", "DataFiles", "");
            if (File.Exists(file))
            {
                RAWDatasetImporterEditorWindow wnd = (RAWDatasetImporterEditorWindow)EditorWindow.GetWindow(typeof(RAWDatasetImporterEditorWindow));
                if (wnd != null)
                    wnd.Close();

                wnd = EditorWindow.CreateInstance<RAWDatasetImporterEditorWindow>();
                wnd.Initialise(file);
                wnd.Show();
            }
            else
            {
                Debug.LogError("File doesn't exist: " + file);
            }
        }

        [MenuItem("Volume Rendering/Load dataset/Load DICOM")]
        private static void ShowDICOMImporter()
        {
            DicomImportAsync(true);
        }

        [MenuItem("Assets/Volume Rendering/Import dataset/Import DICOM")]
        private static void ImportDICOMAsset()
        {
            DicomImportAsync(false);
        }

        private static async void DicomImportAsync(bool spawnInScene)
        {
            string dir = EditorUtility.OpenFolderPanel("Select a folder to load", "", "");
            if (Directory.Exists(dir))
            {
                Debug.Log("Async dataset load. Hold on.");
                using (ProgressHandler progressHandler = new ProgressHandler(new EditorProgressView()))
                {
                    progressHandler.StartStage(0.7f, "Importing dataset");
                    Task<VolumeDataset[]> importTask = EditorDatasetImportUtils.ImportDicomDirectoryAsync(dir, progressHandler);
                    await importTask;
                    progressHandler.EndStage();
                    progressHandler.StartStage(0.3f, "Spawning dataset");
                    for (int i = 0; i < importTask.Result.Length; i++)
                    {
                        if (spawnInScene)
                        {
                            VolumeDataset dataset = importTask.Result[i];
                            VolumeRenderedObject obj = await VolumeObjectFactory.CreateObjectAsync(dataset);
                            obj.transform.position = new Vector3(i, 0, 0);
                        }
                        else
                        {
                            VolumeDataset dataset = importTask.Result[i];
                            ProjectWindowUtil.CreateAsset(dataset, $"{dataset.datasetName}.asset");
                            AssetDatabase.SaveAssets();
                        }
                    }
                    progressHandler.EndStage();
                }
            }
            else
            {
                Debug.LogError("Directory doesn't exist: " + dir);
            }
        }

        [MenuItem("Volume Rendering/Load dataset/Load NRRD dataset")]
        private static void ShowNRRDDatasetImporter()
        {
            ImportNRRDDatasetAsync(true);
        }

        [MenuItem("Assets/Volume Rendering/Import dataset/Import NRRD")]
        private static void ImportNRRDAsset()
        {
            ImportNRRDDatasetAsync(false);
        }

        private static async void ImportNRRDDatasetAsync(bool spawnInScene)
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

            string file = EditorUtility.OpenFilePanel("Select a dataset to load (.nrrd)", "DataFiles", "");
            if (File.Exists(file))
            {
                Debug.Log("Async dataset load. Hold on.");
                using (ProgressHandler progressHandler = new ProgressHandler(new EditorProgressView(), "NRRD import"))
                {
                    progressHandler.ReportProgress(0.0f, "Importing NRRD dataset");

                    IImageFileImporter importer = ImporterFactory.CreateImageFileImporter(ImageFileFormat.NRRD);
                    VolumeDataset dataset = await importer.ImportAsync(file);

                    progressHandler.ReportProgress(0.8f, "Creating object");
                    if (dataset != null)
                    {
                        await EditorDatasetImportUtils.OptionallyDownscale(dataset);
                        if (spawnInScene)
                        {
                            await VolumeObjectFactory.CreateObjectAsync(dataset);
                        }
                        else    
                        {
                            ProjectWindowUtil.CreateAsset(dataset, $"{dataset.datasetName}.asset");
                            AssetDatabase.SaveAssets();
                        }
                    }
                    else
                    {
                        Debug.LogError("Failed to import datset");
                    }
                }
            }
            else
            {
                Debug.LogError("File doesn't exist: " + file);
            }
        }

        [MenuItem("Volume Rendering/Load dataset/Load NIFTI dataset")]
        private static void ShowNIFTIDatasetImporter()
        {
            ImportNIFTIDatasetAsync(true);
        }

        [MenuItem("Assets/Volume Rendering/Import dataset/Import NIFTI")]
        private static void ImportNIFTIAsset()
        {
            ImportNIFTIDatasetAsync(false);
        }

        private static async void ImportNIFTIDatasetAsync(bool spawnInScene)
        {
            string file = EditorUtility.OpenFilePanel("Select a dataset to load (.nii)", "DataFiles", "");
            if (File.Exists(file))
            {
                Debug.Log("Async dataset load. Hold on.");
                using (ProgressHandler progressHandler = new ProgressHandler(new EditorProgressView(), "NIFTI import"))
                {
                    progressHandler.ReportProgress(0.0f, "Importing NIfTI dataset");

                    IImageFileImporter importer = ImporterFactory.CreateImageFileImporter(ImageFileFormat.NIFTI);
                    VolumeDataset dataset = await importer.ImportAsync(file);

                    progressHandler.ReportProgress(0.0f, "Creating object");

                    if (dataset != null)
                    {
                        await EditorDatasetImportUtils.OptionallyDownscale(dataset);
                        if (spawnInScene)
                        {
                            await VolumeObjectFactory.CreateObjectAsync(dataset);
                        }
                        else    
                        {
                            ProjectWindowUtil.CreateAsset(dataset, $"{dataset.datasetName}.asset");
                            AssetDatabase.SaveAssets();
                        }
                    }
                    else
                    {
                        Debug.LogError("Failed to import datset");
                    }
                }
            }
            else
            {
                Debug.LogError("File doesn't exist: " + file);
            }
        }

        [MenuItem("Volume Rendering/Load dataset/Load image file")]
        private static void ShowImageFileImporter()
        {
            ImporImageFileDatasetAsync(true);
        }

        [MenuItem("Assets/Volume Rendering/Import dataset/Import image file")]
        private static void ImportImageFileAsset()
        {
            ImporImageFileDatasetAsync(false);
        }

        private static async void ImporImageFileDatasetAsync(bool spawnInScene)
        {
            string file = EditorUtility.OpenFilePanel("Select a dataset to load", "DataFiles", "");
            if (File.Exists(file))
            {
                Debug.Log("Async dataset load. Hold on.");
                using (ProgressHandler progressHandler = new ProgressHandler(new EditorProgressView(), "Image file import"))
                {
                    progressHandler.ReportProgress(0.0f, "Importing image file dataset");

                    IImageFileImporter importer = ImporterFactory.CreateImageFileImporter(ImageFileFormat.Unknown);
                    VolumeDataset dataset = await importer.ImportAsync(file);

                    progressHandler.ReportProgress(0.0f, "Creating object");

                    if (dataset != null)
                    {
                        await EditorDatasetImportUtils.OptionallyDownscale(dataset);
                        if (spawnInScene)
                        {
                            await VolumeObjectFactory.CreateObjectAsync(dataset);
                        }
                        else
                        {
                            ProjectWindowUtil.CreateAsset(dataset, $"{dataset.datasetName}.asset");
                            AssetDatabase.SaveAssets();
                        }
                    }
                    else
                    {
                        Debug.LogError("Failed to import datset");
                    }
                }
            }
            else
            {
                Debug.LogError("File doesn't exist: " + file);
            }
        }

        [MenuItem("Volume Rendering/Load dataset/Load PARCHG dataset")]
        private static void ShowParDatasetImporter()
        {
            ImportParDatasetAsync(true);
        }

        [MenuItem("Assets/Volume Rendering/Import dataset/Import PARCHG")]
        private static void ImportParAsset()
        {
            ImportParDatasetAsync(false);
        }

        private static async void ImportParDatasetAsync(bool spawnInScene)
        {
            string file = EditorUtility.OpenFilePanel("Select a dataset to load", "DataFiles", "");
            if (File.Exists(file))
            {
                Debug.Log("Async dataset load. Hold on.");
                using (ProgressHandler progressHandler = new ProgressHandler(new EditorProgressView(), "AVSP import"))
                {
                    progressHandler.ReportProgress(0.0f, "Importing VASP dataset");

                    IImageFileImporter importer = ImporterFactory.CreateImageFileImporter(ImageFileFormat.VASP);
                    VolumeDataset dataset = await importer.ImportAsync(file);

                    progressHandler.ReportProgress(0.0f, "Creating object");

                    if (dataset != null)
                    {
                        await EditorDatasetImportUtils.OptionallyDownscale(dataset);
                        if (spawnInScene)
                        {
                            await VolumeObjectFactory.CreateObjectAsync(dataset);
                        }
                        else    
                        {
                            ProjectWindowUtil.CreateAsset(dataset, $"{dataset.datasetName}.asset");
                            AssetDatabase.SaveAssets();
                        }
                    }
                    else
                    {
                        Debug.LogError("Failed to import datset");
                    }
                }
            }
            else
            {
                Debug.LogError("File doesn't exist: " + file);
            }
        }

        [MenuItem("Volume Rendering/Load dataset/Load image sequence")]
        private static void ShowSequenceImporter()
        {
            ImportSequenceAsync();
        }

        private static async void ImportSequenceAsync()
        {
            string dir = EditorUtility.OpenFolderPanel("Select a folder to load", "", "");

            if (Directory.Exists(dir))
            {
                Debug.Log("Async dataset load. Hold on.");

                List<string> filePaths = Directory.GetFiles(dir).ToList();
                IImageSequenceImporter importer = ImporterFactory.CreateImageSequenceImporter(ImageSequenceFormat.ImageSequence);

                IEnumerable<IImageSequenceSeries> seriesList = await importer.LoadSeriesAsync(filePaths);

                if (seriesList.Count() == 0)
                {
                    Debug.LogWarning("Found no series to import.");
                }

                foreach (IImageSequenceSeries series in seriesList)
                {
                    VolumeDataset dataset = await importer.ImportSeriesAsync(series);
                    if (dataset != null)
                    {
                        await EditorDatasetImportUtils.OptionallyDownscale(dataset);
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
        private static void OnMenuItemClick()
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
        private static void SpawnCutoutBox()
        {
            VolumeRenderedObject[] objects = GameObject.FindObjectsOfType<VolumeRenderedObject>();
            if (objects.Length == 1)
                VolumeObjectFactory.SpawnCutoutBox(objects[0]);
        }
        [MenuItem("Volume Rendering/Cross section/Sphere cutout")]
        private static void SpawnCutoutSphere()
        {
            VolumeRenderedObject[] objects = GameObject.FindObjectsOfType<VolumeRenderedObject>();
            if (objects.Length == 1)
                VolumeObjectFactory.SpawnCutoutSphere(objects[0]);
        }

        [MenuItem("Volume Rendering/1D Transfer Function")]
        private static void Show1DTFWindow()
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
        private static void Show2DTFWindow()
        {
            TransferFunction2DEditorWindow.ShowWindow();
        }

        [MenuItem("Volume Rendering/Slice renderer")]
        private static void ShowSliceRenderer()
        {
            SliceRenderingEditorWindow.ShowWindow();
        }

        [MenuItem("Volume Rendering/Value range")]
        private static void ShowValueRangeWindow()
        {
            ValueRangeEditorWindow.ShowWindow();
        }

        [MenuItem("Volume Rendering/Settings")]
        private static void ShowSettingsWindow()
        {
            ImportSettingsEditorWindow.ShowWindow();
        }
    }
}
