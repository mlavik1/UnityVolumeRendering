using UnityEngine;
using UnityEditor;

namespace UnityVolumeRendering
{
    [CustomEditor(typeof(VolumeRenderedObject))]
    public class VolumeRenderedObjectCustomInspector : Editor
    {
        bool otherSettings = false;
        public override void OnInspectorGUI()
        {
            VolumeRenderedObject volrendObj = (VolumeRenderedObject)target;

            // Render mode
            RenderMode oldRenderMode = volrendObj.GetRenderMode();
            RenderMode newRenderMode = (RenderMode)EditorGUILayout.EnumPopup("Render mode", oldRenderMode);
            if (newRenderMode != oldRenderMode)
                volrendObj.SetRenderMode(newRenderMode);

            // Lighting settings
            if (volrendObj.GetRenderMode() == RenderMode.DirectVolumeRendering)
                volrendObj.SetLightingEnabled(GUILayout.Toggle(volrendObj.GetLightingEnabled(), "Enable lighting"));
            else
                volrendObj.SetLightingEnabled(false);

            // Visibility window
            Vector2 visibilityWindow = volrendObj.GetVisibilityWindow();
            EditorGUILayout.MinMaxSlider("Visible value range", ref visibilityWindow.x, ref visibilityWindow.y, 0.0f, 1.0f);
            EditorGUILayout.Space();
            volrendObj.SetVisibilityWindow(visibilityWindow);

            // Transfer function type
            TFRenderMode tfMode = (TFRenderMode)EditorGUILayout.EnumPopup("Transfer function type", volrendObj.GetTransferFunctionMode());
            if (tfMode != volrendObj.GetTransferFunctionMode())
                volrendObj.SetTransferFunctionMode(tfMode);

            // Show TF button
            if (GUILayout.Button("Edit transfer function"))
            {
                if (tfMode == TFRenderMode.TF1D)
                    TransferFunctionEditorWindow.ShowWindow();
                else
                    TransferFunction2DEditorWindow.ShowWindow();
            }

            // Other settings for direct volume rendering
            if (volrendObj.GetRenderMode() == RenderMode.DirectVolumeRendering)
            {
                GUILayout.Space(10);
                otherSettings = EditorGUILayout.Foldout(otherSettings, "Other Settings");
                if (otherSettings)
                {
                    // Temporary back-to-front rendering option
                    volrendObj.SetDVRBackwardEnabled(GUILayout.Toggle(volrendObj.GetDVRBackwardEnabled(), "Enable Back-to-Front Direct Volume Rendering"));

                    // Early ray termination for Front-to-back DVR
                    if (!volrendObj.GetDVRBackwardEnabled())
                    {
                        volrendObj.SetRayTerminationEnabled(GUILayout.Toggle(volrendObj.GetRayTerminationEnabled(), "Enable early ray termination"));
                    }
                }
            }
        }
    }
}
