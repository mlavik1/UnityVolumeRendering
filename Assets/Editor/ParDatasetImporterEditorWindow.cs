using UnityEngine;
using UnityEditor;
using System.IO;
using System;

namespace UnityVolumeRendering
{
    /// <summary>
    /// Editor window for importing datasets.
    /// </summary>
    public class ParDatasetImporterEditorWindow : EditorWindow // : EditorWindow
    {
        private string fileToImport;
        private string filePath;
        private string moleculeName;
        private int nx;
        private int ny;
        private int nz;


        public ParDatasetImporterEditorWindow(string filePath)
        {
            fileToImport = filePath;

            if (Path.GetExtension(fileToImport) == ".vasp")
            {
                this.minSize = new Vector2(250.0f, 150.0f);
            }
        }

        private void ImportDataset()
        {
            
            ParDatasetImporter importer = new ParDatasetImporter(fileToImport, nx, ny, nz);
            VolumeDataset dataset = importer.Import();

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
            ParDatasetImporter pd = new ParDatasetImporter(filePath, nx, ny, nz);            
            nx = EditorGUILayout.IntField("nx dimension grid values", nx);
            ny = EditorGUILayout.IntField("ny dimension grid values", ny);
            nz = EditorGUILayout.IntField("nz dimension grid values", nz);

            if (GUILayout.Button("Import"))           
               ImportDataset();

            if (GUILayout.Button("Cancel"))
                this.Close();
        }
    }
}
