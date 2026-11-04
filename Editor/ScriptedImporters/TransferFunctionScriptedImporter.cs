#if UNITY_2020_2_OR_NEWER
using UnityEngine;
using UnityEditor.AssetImporters;
using UnityEngine.Scripting.APIUpdating;

namespace MingRay
{
    /// <summary>
    /// ScriptedImporter for raw datasets.
    /// Allows you to import datasets as assets by dragging them into the project view.
    /// Imported dataset assets can be dragged-and-dropped into the scene view/hierarchy, or spawned from code.
    /// </summary>
    [MovedFrom(true, sourceNamespace: "UnityVolumeRendering")]
    [ScriptedImporter(1, "tf")]
    public class TransferFunctionScriptedImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            string fileToImport = ctx.assetPath;

            TransferFunction tf = TransferFunctionDatabase.LoadTransferFunction(fileToImport);
            
            if (tf)
            {
                ctx.AddObjectToAsset("main obj", tf);
                ctx.SetMainObject(tf);
            }
            else
            {
                Debug.LogError($"Failed to load transfer function: {fileToImport}");
            }
        }
    }
}
#endif
