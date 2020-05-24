using UnityEngine;
using UnityEditor;
using System.IO;
using System;

namespace UnityVolumeRendering
{
    /// <summary>
    /// Editor window for importing datasets.
    /// </summary>
    public class DatasetImporterEditorWindow : EditorWindow
    {
        private enum DatasetType
        {
            Unknown,
            Raw,
            DICOM
        }

        private string fileToImport;
        private DatasetType datasetType;

        private int dimX;
        private int dimY;
        private int dimZ;
        private int bytesToSkip = 0;
        private DataContentFormat dataFormat = DataContentFormat.Int16;
        private Endianness endianness = Endianness.LittleEndian;

        public DatasetImporterEditorWindow(string fileToImport)
        {
            // Check file extension
            string extension = Path.GetExtension(fileToImport);
            if (extension == ".dat" || extension == ".raw" || extension == ".vol")
                datasetType = DatasetType.Raw;
            else if (extension == ".ini")
            {
                fileToImport = fileToImport.Substring(0, fileToImport.LastIndexOf("."));
                datasetType = DatasetType.Raw;
            }
            else if (extension == ".dicom" || extension == ".dcm")
                datasetType = DatasetType.DICOM;
            else
                datasetType = DatasetType.Unknown;

            this.fileToImport = fileToImport;

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

        private void ImportDataset()
        {
            DatasetImporterBase importer = null;
            switch (datasetType)
            {
                case DatasetType.Raw:
                {
                    importer = new RawDatasetImporter(fileToImport, dimX, dimY, dimZ, dataFormat, endianness, bytesToSkip);
                    break;
                }
                case DatasetType.DICOM:
                {
                    importer = new DICOMImporter(new FileInfo(fileToImport).Directory.FullName, false);
                    break;
                }
            }

            VolumeDataset dataset = null;
            if (importer != null)
                dataset = importer.Import();

            if (dataset != null)
            {
                VolumeRenderedObject obj = VolumeObjectFactory.CreateObject(dataset);
            }
            else
            {
                Debug.LogError("Failed to import datset");
            }

            this.Close();
        }

        private void OnGUI()
        {
            switch (datasetType)
            {
                case DatasetType.Raw:
                {
                    dimX = EditorGUILayout.IntField("X dimension", dimX);
                    dimY = EditorGUILayout.IntField("Y dimension", dimY);
                    dimZ = EditorGUILayout.IntField("Z dimension", dimZ);
                    bytesToSkip = EditorGUILayout.IntField("Bytes to skip", bytesToSkip);
                    dataFormat = (DataContentFormat)EditorGUILayout.EnumPopup("Data format", dataFormat);
                    endianness = (Endianness)EditorGUILayout.EnumPopup("Endianness", endianness);
                    break;
                }
            }

            if (GUILayout.Button("Import"))
                ImportDataset();

            if (GUILayout.Button("Cancel"))
                this.Close();
        }
    }
}
