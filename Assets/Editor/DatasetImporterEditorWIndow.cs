using UnityEngine;
using UnityEditor;
using System.IO;

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
        this.fileToImport = fileToImport;
        string extension = Path.GetExtension(fileToImport);
        if (extension == ".dat" || extension == ".raw")
            datasetType = DatasetType.Raw;
        else
            datasetType = DatasetType.Unknown;

        if(extension == ".dat")
        {
            FileStream fs = new FileStream(fileToImport, FileMode.Open);
            BinaryReader reader = new BinaryReader(fs);

            dimX = reader.ReadUInt16();
            dimY = reader.ReadUInt16();
            dimZ = reader.ReadUInt16();
            bytesToSkip = 6;

            reader.Close();
            fs.Close();
        }
    }

    private void ImportDataset()
    {
        DatasetImporterBase importer = null;
        switch(datasetType)
        {
            case DatasetType.Raw:
                {
                    importer = new RawDatasetImporter(fileToImport, dimX, dimY, dimZ, DataContentFormat.Int16, 6);
                    break;
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