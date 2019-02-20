using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VolumeRenderedObject))]
public class VolumeRenderedObjectCustomInspector : Editor
{
    public override void OnInspectorGUI()
    {
        VolumeRenderedObject myTarget = (VolumeRenderedObject)target;

        RenderMode oldRenderMode = myTarget.GetRemderMode();
        RenderMode newRenderMode = (RenderMode)EditorGUILayout.EnumPopup("Render mode", oldRenderMode);

        if (newRenderMode != oldRenderMode)
            myTarget.SetRenderMode(newRenderMode);
    }
}
