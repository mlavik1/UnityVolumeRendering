using UnityEngine;
using UnityEditor;

namespace UnityVolumeRendering
{
    public class SelectionHelper
    {
        public static VolumeRenderedObject GetSelectedVolumeObject()
        {
            foreach (GameObject obj in Selection.gameObjects)
            {
                VolumeRenderedObject volrendobj = obj.GetComponent<VolumeRenderedObject>();
                if (volrendobj != null)
                    return volrendobj;
            }
            return null;
        }
    }
}
