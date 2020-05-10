using UnityEngine;
using UnityEditor;

namespace UnityVolumeRendering
{
    [CustomEditor(typeof(VolumeRenderedObject))]
    public class VolumeRenderedObjectCustomInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            VolumeRenderedObject volrendObj = (VolumeRenderedObject)target;

            RenderMode oldRenderMode = volrendObj.GetRenderMode();
            RenderMode newRenderMode = (RenderMode)EditorGUILayout.EnumPopup("Render mode", oldRenderMode);

            if(newRenderMode == RenderMode.IsosurfaceRendering)
            {
                Material mat = volrendObj.GetComponent<MeshRenderer>().sharedMaterial; // TODO
                float minVal = mat.GetFloat("_MinVal");
                float maxVal = mat.GetFloat("_MaxVal");
                EditorGUILayout.MinMaxSlider("Visible value range",  ref minVal, ref maxVal, 0.0f, 1.0f);
                mat.SetFloat("_MinVal", minVal);
                mat.SetFloat("_MaxVal", maxVal);
            }

            if (newRenderMode != oldRenderMode)
                volrendObj.SetRenderMode(newRenderMode);
        }
    }
}
