using UnityEngine;
using UnityEditor;

namespace UnityVolumeRendering
{
    public class SelectionHelper
    {
        public static VolumeRenderedObject GetSelectedVolumeObject(bool autoSelectFirst = false)
        {
            foreach (GameObject obj in Selection.gameObjects)
            {
                VolumeRenderedObject volrendobj = obj.GetComponent<VolumeRenderedObject>();
                if (volrendobj != null)
                    return volrendobj;
            }

            VolumeRenderedObject volRendObject = GameObject.FindObjectOfType<VolumeRenderedObject>();
            if (volRendObject != null)
            {
                Selection.objects = new Object[] { volRendObject.gameObject };
                return volRendObject;
            }

            return null;
        }
    }
}
