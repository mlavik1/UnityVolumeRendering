using UnityEngine;
using UnityEditor.AssetImporters;

namespace UnityVolumeRendering
{
    [ScriptedImporter(1, "raw")]
    public class RawScriptedImporter : ScriptedImporter
    {
        [SerializeField]
        private Vector3Int dimension = new Vector3Int(128, 256, 256);
        [SerializeField]
        private DataContentFormat dataFormat = DataContentFormat.Int16;
        [SerializeField]
        private Endianness endianness = Endianness.LittleEndian;
        [SerializeField]
        private int bytesToSkip = 0;

        public override void OnImportAsset(AssetImportContext ctx)
        {
            string fileToImport = ctx.assetPath;

            // Try parse ini file (if available)
            DatasetIniData initData = DatasetIniReader.ParseIniFile(fileToImport + ".ini");
            if (initData != null)
            {
                dimension = new Vector3Int(initData.dimX, initData.dimY, initData.dimZ);
                dataFormat = initData.format;
                endianness = initData.endianness;
                bytesToSkip = initData.bytesToSkip;
            }

            RawDatasetImporter importer = new RawDatasetImporter(fileToImport, dimension.x, dimension.y, dimension.z, dataFormat, endianness, bytesToSkip);
            VolumeDataset dataset = importer.Import();
            
            if (dataset)
            {
                ctx.AddObjectToAsset("main obj", dataset);
                ctx.SetMainObject(dataset);
            }
            else
            {
                Debug.LogError($"Failed to load dataset: {fileToImport}");
            }
        }
    }
}
