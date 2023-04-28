#if UNITY_2021_2_OR_NEWER
using UnityEditor;
using UnityEngine;

namespace UnityVolumeRendering
{
    static class DragDropHandler
    {
        [InitializeOnLoadMethod]
        static void OnLoad()
        {
            DragAndDrop.AddDropHandler(OnSceneDrop);
            DragAndDrop.AddDropHandler(OnHierarchyDrop);
        }

        private static DragAndDropVisualMode OnSceneDrop(Object dropUpon, Vector3 worldPosition, Vector2 viewportPosition, Transform parentForDraggedObjects, bool perform)
        {
            if (perform)
            {
                VolumeDataset datasetAsset = (VolumeDataset)DragAndDrop.objectReferences[0];
                VolumeObjectFactory.CreateObject(datasetAsset);
            }
            return DragAndDropVisualMode.Move;
        }

        private static DragAndDropVisualMode OnHierarchyDrop(int dropTargetInstanceID, HierarchyDropFlags dropMode, Transform parentForDraggedObjects, bool perform)
        {
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
    }
}
#endif
