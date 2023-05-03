using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UnityVolumeRendering
{
    [CustomEditor(typeof(VolumeRenderedObject))]
    public class VolumeRenderedObjectCustomInspector : Editor, IProgressView
    {
        private bool tfSettings = true;
        private bool lightSettings = true;
        private bool otherSettings = true;
        private float currentProgress = 1.0f;
        private string currentProgressDescrition = "";
        private bool progressDirty = false;

        public void StartProgress(string title, string description)
        {
        }

        public void FinishProgress(ProgressStatus status = ProgressStatus.Succeeded)
        {
            currentProgress = 1.0f;
        }

        public void UpdateProgress(float totalProgress, float currentStageProgress, string description)
        {
            currentProgressDescrition = description;
            currentProgress = totalProgress;
            progressDirty = true;
        }
        public override bool RequiresConstantRepaint()
        {
            return progressDirty;
        }

        public override void OnInspectorGUI()
        {
            VolumeRenderedObject volrendObj = (VolumeRenderedObject)target;

            if (currentProgress < 1.0f)
            {
                Rect rect = EditorGUILayout.BeginVertical();
                EditorGUI.ProgressBar(rect, currentProgress, currentProgressDescrition);
                GUILayout.Space(18);
                EditorGUILayout.EndVertical();
            }
            progressDirty = false;

            // Render mode
            RenderMode oldRenderMode = volrendObj.GetRenderMode();
            RenderMode newRenderMode = (RenderMode)EditorGUILayout.EnumPopup("Render mode", oldRenderMode);
            if (newRenderMode != oldRenderMode)
            {
                Task task = volrendObj.SetRenderModeAsync(newRenderMode, new ProgressHandler(this));
            }

            // Visibility window
            Vector2 visibilityWindow = volrendObj.GetVisibilityWindow();
            EditorGUILayout.MinMaxSlider("Visible value range", ref visibilityWindow.x, ref visibilityWindow.y, 0.0f, 1.0f);
            volrendObj.SetVisibilityWindow(visibilityWindow);

            // Transfer function settings
            EditorGUILayout.Space();
            tfSettings = EditorGUILayout.Foldout(tfSettings, "Transfer function");
            if (tfSettings)
            {
                // Transfer function type
                TFRenderMode tfMode = (TFRenderMode)EditorGUILayout.EnumPopup("Transfer function type", volrendObj.GetTransferFunctionMode());
                if (tfMode != volrendObj.GetTransferFunctionMode())
                {
                    Task task = volrendObj.SetTransferFunctionModeAsync(tfMode, new ProgressHandler(this));
                }

                // Show TF button
                if (GUILayout.Button("Edit transfer function"))
                {
                    if (tfMode == TFRenderMode.TF1D)
                        TransferFunctionEditorWindow.ShowWindow(volrendObj);
                    else
                        TransferFunction2DEditorWindow.ShowWindow();
                }
            }

            // Lighting settings
            GUILayout.Space(10);
            lightSettings = EditorGUILayout.Foldout(lightSettings, "Lighting");
            if (lightSettings)
            {
                if (volrendObj.GetRenderMode() == RenderMode.DirectVolumeRendering)
                {
                    Task task = volrendObj.SetLightingEnabledAsync(GUILayout.Toggle(volrendObj.GetLightingEnabled(), "Enable lighting"), new ProgressHandler(this));
                }
                else
                    volrendObj.SetLightingEnabled(false);

                if (volrendObj.GetLightingEnabled() || volrendObj.GetRenderMode() == RenderMode.IsosurfaceRendering)
                {
                    LightSource oldLightSource = volrendObj.GetLightSource();
                    LightSource newLightSource = (LightSource)EditorGUILayout.EnumPopup("Light source", oldLightSource);
                    if (newLightSource != oldLightSource)
                        volrendObj.SetLightSource(newLightSource);
                }
            }

            // Other settings
            GUILayout.Space(10);
            otherSettings = EditorGUILayout.Foldout(otherSettings, "Other Settings");
            if (otherSettings)
            {
                if (volrendObj.GetRenderMode() == RenderMode.DirectVolumeRendering)
                {
                    // Early ray termination
                    volrendObj.SetRayTerminationEnabled(GUILayout.Toggle(volrendObj.GetRayTerminationEnabled(), "Enable early ray termination"));
                }

                volrendObj.SetCubicInterpolationEnabled(GUILayout.Toggle(volrendObj.GetCubicInterpolationEnabled(), "Enable cubic interpolation (better quality)"));
            }
        }
    }
}
