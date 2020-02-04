using UnityEngine;
using UnityEditor;
using System.IO;
using System;

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

    private int dimX; // TODO: set good default value
    private int dimY; // TODO: set good default value
    private int dimZ; // TODO: set good default value
    private int bytesToSkip = 0;
    private DataContentFormat dataFormat = DataContentFormat.Int16;

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
        else if (extension == ".dicom")
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
        }

        this.minSize = new Vector2(300.0f, 200.0f);
    }

    private void ImportDataset()
    {
        DatasetImporterBase importer = null;
        switch(datasetType)
        {
            case DatasetType.Raw:
            {
                importer = new RawDatasetImporter(fileToImport, dimX, dimY, dimZ, dataFormat, bytesToSkip);
                break;
            }
            case DatasetType.DICOM:
            {
                throw new System.NotImplementedException("TODO: implement support for DICOM files");
            }
        }

        VolumeDataset dataset = null;
        if(importer != null)
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
        switch(datasetType)
        {
            case DatasetType.Raw:
                {
                    dimX = EditorGUILayout.IntField("X dimension", dimX);
                    dimY = EditorGUILayout.IntField("X dimension", dimY);
                    dimZ = EditorGUILayout.IntField("X dimension", dimZ);
                    bytesToSkip = EditorGUILayout.IntField("Bytes to skip", bytesToSkip);
                    dataFormat = (DataContentFormat)EditorGUILayout.EnumPopup("Data format", dataFormat);
                    break;
                }
        }

        if(GUILayout.Button("Import"))
            ImportDataset();

        if (GUILayout.Button("Cancel"))
            this.Close();
    }
}
