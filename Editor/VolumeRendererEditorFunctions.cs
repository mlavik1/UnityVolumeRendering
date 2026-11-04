using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Scripting.APIUpdating;

namespace MingRay
{
    [MovedFrom(true, sourceNamespace: "UnityVolumeRendering")]
    public class VolumeRendererEditorFunctions
    {
        internal const string MenuRoot = "Tools/MingRay/";
        internal const string AssetsMenuRoot = "Assets/MingRay/";

        [MenuItem(MenuRoot + "Load dataset/Load raw dataset", priority = 0)]
        internal static void ShowDatasetImporter()
        {
            string file = EditorUtility.OpenFilePanel("Select a dataset to load", "SampleData", "");
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

        [MenuItem(MenuRoot + "Load dataset/Load DICOM", priority = 1)]
        internal static void ShowDICOMImporter()
        {
            DicomImportAsync(true);
        }

        [MenuItem(AssetsMenuRoot + "Import dataset/Import DICOM")]
        internal static void ImportDICOMAsset()
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

        [MenuItem(MenuRoot + "Load dataset/Load NRRD dataset", priority = 3)]
        internal static void ShowNRRDDatasetImporter()
        {
            ImportNRRDDatasetAsync(true);
        }

        [MenuItem(AssetsMenuRoot + "Import dataset/Import NRRD")]
        internal static void ImportNRRDAsset()
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

            string file = EditorUtility.OpenFilePanel("Select a dataset to load (.nrrd)", "SampleData", "");
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

        [MenuItem(MenuRoot + "Load dataset/Load NIFTI dataset", priority = 2)]
        internal static void ShowNIFTIDatasetImporter()
        {
            ImportNIFTIDatasetAsync(true);
        }

        [MenuItem(AssetsMenuRoot + "Import dataset/Import NIFTI")]
        internal static void ImportNIFTIAsset()
        {
            ImportNIFTIDatasetAsync(false);
        }

        private static async void ImportNIFTIDatasetAsync(bool spawnInScene)
        {
            string file = EditorUtility.OpenFilePanel("Select a dataset to load (.nii)", "SampleData", "");
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

        [MenuItem(MenuRoot + "Load dataset/Load image file", priority = 4)]
        internal static void ShowImageFileImporter()
        {
            ImporImageFileDatasetAsync(true);
        }

        [MenuItem(AssetsMenuRoot + "Import dataset/Import image file")]
        internal static void ImportImageFileAsset()
        {
            ImporImageFileDatasetAsync(false);
        }

        private static async void ImporImageFileDatasetAsync(bool spawnInScene)
        {
            string file = EditorUtility.OpenFilePanel("Select a dataset to load", "SampleData", "");
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

        [MenuItem(MenuRoot + "Load dataset/Load PARCHG dataset", priority = 6)]
        internal static void ShowParDatasetImporter()
        {
            ImportParDatasetAsync(true);
        }

        [MenuItem(AssetsMenuRoot + "Import dataset/Import PARCHG")]
        internal static void ImportParAsset()
        {
            ImportParDatasetAsync(false);
        }

        private static async void ImportParDatasetAsync(bool spawnInScene)
        {
            string file = EditorUtility.OpenFilePanel("Select a dataset to load", "SampleData", "");
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

        [MenuItem(MenuRoot + "Load dataset/Load image sequence", priority = 5)]
        internal static void ShowSequenceImporter()
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

        [MenuItem(MenuRoot + "Cross section/Cross section plane", priority = 20)]
        internal static void SpawnCrossSectionPlane()
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

        [MenuItem(MenuRoot + "Cross section/Box cutout", priority = 21)]
        internal static void SpawnCutoutBox()
        {
            VolumeRenderedObject[] objects = GameObject.FindObjectsOfType<VolumeRenderedObject>();
            if (objects.Length == 1)
                VolumeObjectFactory.SpawnCutoutBox(objects[0]);
        }
        [MenuItem(MenuRoot + "Cross section/Sphere cutout", priority = 22)]
        internal static void SpawnCutoutSphere()
        {
            VolumeRenderedObject[] objects = GameObject.FindObjectsOfType<VolumeRenderedObject>();
            if (objects.Length == 1)
                VolumeObjectFactory.SpawnCutoutSphere(objects[0]);
        }

        [MenuItem(MenuRoot + "1D Transfer Function", priority = 40)]
        internal static void Show1DTFWindow()
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

        [MenuItem(MenuRoot + "2D Transfer Function", priority = 41)]
        internal static void Show2DTFWindow()
        {
            TransferFunction2DEditorWindow.ShowWindow();
        }

        [MenuItem(MenuRoot + "Slice renderer", priority = 42)]
        internal static void ShowSliceRenderer()
        {
            SliceRenderingEditorWindow.ShowWindow();
        }

        [MenuItem(MenuRoot + "Value range", priority = 43)]
        internal static void ShowValueRangeWindow()
        {
            ValueRangeEditorWindow.ShowWindow();
        }

        [MenuItem(MenuRoot + "Settings", priority = 60)]
        internal static void ShowSettingsWindow()
        {
            ImportSettingsEditorWindow.ShowWindow();
        }
    }
}
