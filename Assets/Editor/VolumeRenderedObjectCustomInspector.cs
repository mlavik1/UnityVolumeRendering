using UnityEngine;
using UnityEditor;

namespace UnityVolumeRendering
{
    [CustomEditor(typeof(VolumeRenderedObject))]
    public class VolumeRenderedObjectCustomInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            VolumeRenderedObject myTarget = (VolumeRenderedObject)target;

            RenderMode oldRenderMode = myTarget.GetRenderMode();
            RenderMode newRenderMode = (RenderMode)EditorGUILayout.EnumPopup("Render mode", oldRenderMode);

            if (newRenderMode != oldRenderMode)
                myTarget.SetRenderMode(newRenderMode);
        }
    }
}
