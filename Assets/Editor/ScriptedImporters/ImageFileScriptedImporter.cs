#if UNITY_2020_2_OR_NEWER
using UnityEngine;
using UnityEditor.AssetImporters;

namespace UnityVolumeRendering
{
#if UVR_USE_SIMPLEITK
    [ScriptedImporter(1, new string[]{"nrrd", "nii.gz", "nii", "vasp"})]
#else
    [ScriptedImporter(1, new string[]{"nii.gz", "nii", "vasp"})]
#endif
    public class ImageFileScriptedImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            string fileToImport = ctx.assetPath;

            ImageFileFormat format = GetImageFileFormat(ctx.assetPath);
            IImageFileImporter importer = ImporterFactory.CreateImageFileImporter(format);
            VolumeDataset dataset = importer.Import(ctx.assetPath);
            
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

        private ImageFileFormat GetImageFileFormat(string filePath)
        {
            string extension = System.IO.Path.GetExtension(filePath);
            switch (extension)
            {
                case ".nrrd":
                    return ImageFileFormat.NRRD;
                case ".vasp":
                    return ImageFileFormat.VASP;
                default:
                    return ImageFileFormat.NIFTI;
            }
        }
    }
}
#endif
