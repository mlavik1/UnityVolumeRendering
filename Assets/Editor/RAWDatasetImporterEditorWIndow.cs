using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Threading.Tasks;

namespace UnityVolumeRendering
{
    /// <summary>
    /// Editor window for importing datasets.
    /// </summary>
    public class RAWDatasetImporterEditorWindow : EditorWindow
    {
        private string fileToImport;

        private int dimX;
        private int dimY;
        private int dimZ;
        private int bytesToSkip = 0;
        private DataContentFormat dataFormat = DataContentFormat.Int16;
        private Endianness endianness = Endianness.LittleEndian;
        private bool importing = false;

        public void Initialise(string filePath)
        {
            fileToImport = filePath;

            if (Path.GetExtension(fileToImport) == ".ini")
                fileToImport = fileToImport.Substring(0, fileToImport.Length - 4);

            // Try parse ini file (if available)
            DatasetIniData initData = DatasetIniReader.ParseIniFile(fileToImport + ".ini");
            if (initData != null)
            {
                dimX = initData.dimX;
                dimY = initData.dimY;
                dimZ = initData.dimZ;
                bytesToSkip = initData.bytesToSkip;
                dataFormat = initData.format;
                endianness = initData.endianness;
            }

            this.minSize = new Vector2(300.0f, 200.0f);
        }

        private async Task ImportDatasetAsync()
        {
            using (ProgressHandler progressHandler = new ProgressHandler(new EditorProgressView(), "RAW import"))
            {
                progressHandler.ReportProgress(0.0f, "Importing RAW dataset");

                RawDatasetImporter importer = new RawDatasetImporter(fileToImport, dimX, dimY, dimZ, dataFormat, endianness, bytesToSkip);
                VolumeDataset dataset = await importer.ImportAsync();

                if (dataset != null)
                {
                    if (EditorPrefs.GetBool("DownscaleDatasetPrompt"))
                    {
                        if (EditorUtility.DisplayDialog("Optional DownScaling",
                            $"Do you want to downscale the dataset? The dataset's dimension is: {dataset.dimX} x {dataset.dimY} x {dataset.dimZ}", "Yes", "No"))
                        {
                            Debug.Log("Async dataset downscale. Hold on.");
                            progressHandler.ReportProgress(0.7f, "Downscaling dataset");
                            await Task.Run(() =>  dataset.DownScaleData());
                        }
                    }
                    progressHandler.ReportProgress(0.8f, "Creating object");
                    VolumeRenderedObject obj = await VolumeObjectFactory.CreateObjectAsync(dataset);
                }
                else
                {
                    Debug.LogError("Failed to import datset");
                }

                this.Close();
            }
        }

        private async void StartImport()
        {
            try
            {
                importing = true;
                await ImportDatasetAsync();
            }
            catch (Exception ex)
            {
                importing = false;
                Debug.LogException(ex);
            }
            importing = false;
        }

        private void OnGUI()
        {
            if (importing)
            {
                EditorGUILayout.LabelField("Importing dataset. Please wait..");
            }
            else
            {
                dimX = EditorGUILayout.IntField("X dimension", dimX);
                dimY = EditorGUILayout.IntField("Y dimension", dimY);
                dimZ = EditorGUILayout.IntField("Z dimension", dimZ);
                bytesToSkip = EditorGUILayout.IntField("Bytes to skip", bytesToSkip);
                dataFormat = (DataContentFormat)EditorGUILayout.EnumPopup("Data format", dataFormat);
                endianness = (Endianness)EditorGUILayout.EnumPopup("Endianness", endianness);

                if (GUILayout.Button("Import"))
                {
                    StartImport();
                }

                if (GUILayout.Button("Cancel"))
                    this.Close();
            }
        }
    }
}
