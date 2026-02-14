#if UNITY_2021_2_OR_NEWER
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityVolumeRendering
{
    /// <summary>
    /// This class handles drag-and-drop of <see cref="VolumeDataset"/> assets into the scene view or scene hierarchy.
    /// </summary>
    static class DragDropHandler
    {
        [InitializeOnLoadMethod]
        static void OnLoad()
        {
            // Scene view
            DragAndDrop.AddDropHandler(OnSceneDrop);
            // Scene hierarchy
            DragAndDrop.AddDropHandler(OnHierarchyDrop);
            // Project browser
            DragAndDrop.AddDropHandler(OnProjectBrowserDrop);
        }

        private static DragAndDropVisualMode OnSceneDrop(Object dropUpon, Vector3 worldPosition, Vector2 viewportPosition, Transform parentForDraggedObjects, bool perform)
        {
            if (DragAndDrop.objectReferences.Length == 0 || !(DragAndDrop.objectReferences[0] is VolumeDataset))
            {
                return DragAndDropVisualMode.None;
            }

            if (perform && DragAndDrop.objectReferences[0] is VolumeDataset)
            {
                VolumeDataset datasetAsset = (VolumeDataset)DragAndDrop.objectReferences[0];
                VolumeObjectFactory.CreateObject(datasetAsset);
            }
            return DragAndDropVisualMode.Move;
        }

        private static DragAndDropVisualMode OnHierarchyDrop(int dropTargetInstanceID, HierarchyDropFlags dropMode, Transform parentForDraggedObjects, bool perform)
        {
            if (DragAndDrop.objectReferences.Length == 0 || !(DragAndDrop.objectReferences[0] is VolumeDataset))
            {
                return DragAndDropVisualMode.None;
            }

            if (perform)
            {
                VolumeDataset datasetAsset = (VolumeDataset)DragAndDrop.objectReferences[0];
                VolumeRenderedObject spawnedObject = VolumeObjectFactory.CreateObject(datasetAsset);
                GameObject parentObject = (GameObject)EditorUtility.InstanceIDToObject(dropTargetInstanceID);
                if (parentObject)
                {
                    spawnedObject.gameObject.transform.SetParent(parentObject.transform);
                }
            }

            return DragAndDropVisualMode.Move;
        }

        private static DragAndDropVisualMode OnProjectBrowserDrop(int dragInstanceId, string dropUponPath, bool perform)
        {
            bool shouldHandle = DragAndDrop.objectReferences.Any(obj =>
                obj is GameObject && (obj as GameObject).GetComponentsInChildren<VolumeRenderedObject>().Length > 0);
            if (!shouldHandle)
                return DragAndDropVisualMode.None; 
            else if (!perform)
                return DragAndDropVisualMode.Copy;

            foreach (Object objRef in DragAndDrop.objectReferences)
            {
                GameObject gameObject = objRef as GameObject;
                if (gameObject == null)
                    continue;
                VolumeRenderedObject[] volRendObjects = gameObject.GetComponentsInChildren<VolumeRenderedObject>();
                if (volRendObjects.Length == 0)
                    continue;

                string prefabPath = Path.Combine(dropUponPath, gameObject.name + ".prefab");
                GameObject prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(gameObject, prefabPath, InteractionMode.AutomatedAction);
                VolumeRenderedObject[] prefabVolRendObjects = prefab.GetComponentsInChildren<VolumeRenderedObject>();
                Debug.Assert(volRendObjects.Length == prefabVolRendObjects.Length);
                for (int i = 0; i < volRendObjects.Length; i++)
                {
                    VolumeRenderedObject srcVolRendObj = volRendObjects[i];
                    VolumeRenderedObject prefabVolRendObj = prefabVolRendObjects[i];
                    if (srcVolRendObj.dataset != prefabVolRendObj.dataset)
                    {
                        // Dataset changed => remove old one form asset to avoid wasting space
                        if (prefabVolRendObj.dataset != null)
                            AssetDatabase.RemoveObjectFromAsset(prefabVolRendObj.dataset);
                        VolumeDataset dataset = ScriptableObject.Instantiate(srcVolRendObj.dataset);
                        AssetDatabase.AddObjectToAsset(dataset, prefab);
                        prefabVolRendObj.dataset = dataset;
                    }
                    if (srcVolRendObj.transferFunction != prefabVolRendObj.transferFunction)
                    {
                        if (prefabVolRendObj.transferFunction != null)
                            AssetDatabase.RemoveObjectFromAsset(prefabVolRendObj.transferFunction);
                        TransferFunction transferFunction = ScriptableObject.Instantiate(srcVolRendObj.transferFunction);
                        AssetDatabase.AddObjectToAsset(transferFunction, prefab);
                        prefabVolRendObj.transferFunction = transferFunction;
                    }
                    if (srcVolRendObj.meshRenderer.sharedMaterial != prefabVolRendObj.meshRenderer.sharedMaterial)
                    {
                        if (prefabVolRendObj.meshRenderer.sharedMaterial != null)
                            AssetDatabase.RemoveObjectFromAsset(prefabVolRendObj.meshRenderer.sharedMaterial);
                        Material material = Material.Instantiate(srcVolRendObj.meshRenderer.sharedMaterial);
                        AssetDatabase.AddObjectToAsset(material, prefab);
                        prefabVolRendObj.meshRenderer.material = material;
                    }
                }
                PrefabUtility.SavePrefabAsset(prefab);
                for (int i = 0; i < volRendObjects.Length; i++)
                {
                    VolumeRenderedObject srcVolRendObj = volRendObjects[i];
                    VolumeRenderedObject prefabVolRendObj = prefabVolRendObjects[i];
                    if (!AssetDatabase.Contains(srcVolRendObj.dataset))
                        ScriptableObject.DestroyImmediate(srcVolRendObj.dataset);
                    srcVolRendObj.dataset = prefabVolRendObj.dataset;
                    srcVolRendObj.transferFunction = prefabVolRendObj.transferFunction;
                    srcVolRendObj.meshRenderer.sharedMaterial = prefabVolRendObj.meshRenderer.sharedMaterial;
                    srcVolRendObj.UpdateMaterialProperties();
                }
            }

            return DragAndDropVisualMode.Copy;
        }
    }
}
#endif
