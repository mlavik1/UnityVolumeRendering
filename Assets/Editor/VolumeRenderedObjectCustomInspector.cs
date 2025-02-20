using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using UnityEngine.Events;

namespace UnityVolumeRendering
{
    [CustomEditor(typeof(VolumeRenderedObject))]
    public class VolumeRenderedObjectCustomInspector : Editor, IProgressView
    {
        private bool tfSettings = true;
        private bool lightSettings = true;
        private bool otherSettings = true;
        private bool overlayVolumeSettings = false;
        private bool segmentationSettings = false;
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

            if (newRenderMode == RenderMode.IsosurfaceRendering)
            {
                float oldThreshold = volrendObj.GetGradientVisibilityThreshold();
                float oldThresholdSqrt = Mathf.Sqrt(oldThreshold); // Convert to square root scaling (=> more precision close to 0)
                float newThreshold = EditorGUILayout.Slider(
                    new GUIContent("Gradient visibility threshold", "Minimum gradient maginitude value that will be visible"),
                    oldThresholdSqrt, 0.0f, 1.0f
                );
                newThreshold = newThreshold * newThreshold; // Convert back to linear scaling
                if (newThreshold != oldThreshold)
                    volrendObj.SetGradientVisibilityThreshold(newThreshold);
            }

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

                GradientType oldGradientType = volrendObj.GetGradientType();
                GradientType newGradientType = (GradientType)EditorGUILayout.EnumPopup("Gradient", oldGradientType);

                if (newGradientType != oldGradientType)
                {
                    volrendObj.SetGradientTypeAsync(newGradientType, new ProgressHandler(this));
                }

                if (volrendObj.GetLightingEnabled() || volrendObj.GetRenderMode() == RenderMode.IsosurfaceRendering)
                {
                    LightSource oldLightSource = volrendObj.GetLightSource();
                    LightSource newLightSource = (LightSource)EditorGUILayout.EnumPopup("Light source", oldLightSource);
                    if (newLightSource != oldLightSource)
                        volrendObj.SetLightSource(newLightSource);

                    // Gradient lighting threshold: Threshold for how low gradients can contribute to lighting.
                    Vector2 gradLightThreshold = volrendObj.GetGradientLightingThreshold();
                    // Convert to square root scaling (=> more precision close to 0)
                    gradLightThreshold = new Vector2(Mathf.Sqrt(gradLightThreshold.x), Mathf.Sqrt(gradLightThreshold.y));
                    EditorGUILayout.MinMaxSlider(
                        new GUIContent("Gradient lighting threshold",
                            "Minimum and maximum threshold for gradient contribution to lighting.\n"
                            + "Voxels with gradient less than min will be unlit, and with gradient >= max will fully shaded."),
                        ref gradLightThreshold.x, ref gradLightThreshold.y, 0.0f, 1.0f
                    );
                    // Convert back to linear scale, before setting updated value.
                    volrendObj.SetGradientLightingThreshold(new Vector2(gradLightThreshold.x * gradLightThreshold.x, gradLightThreshold.y * gradLightThreshold.y));

                    ShadowVolumeManager shadowVoumeManager = volrendObj.GetComponent<ShadowVolumeManager>();
                    bool enableShadowVolume = GUILayout.Toggle(shadowVoumeManager != null, "Enable shadow volume (expensive)");
                    if (enableShadowVolume && shadowVoumeManager == null)
                        shadowVoumeManager = volrendObj.gameObject.AddComponent<ShadowVolumeManager>();
                    else if (!enableShadowVolume && shadowVoumeManager != null)
                        GameObject.DestroyImmediate(shadowVoumeManager);
                }
            }

            // Overlay volume
            overlayVolumeSettings = EditorGUILayout.Foldout(overlayVolumeSettings, "PET/overlay volume");
            if (overlayVolumeSettings)
            {
                OverlayType overlayType = volrendObj.GetOverlayType();
                TransferFunction secondaryTransferFunction = volrendObj.GetSecondaryTransferFunction();
                if (overlayType != OverlayType.Overlay)
                {
                    if (GUILayout.Button("Load PET (NRRD, NIFTI)"))
                    {
                        ImportImageFileDataset(volrendObj, (VolumeDataset dataset) =>
                        {
                            TransferFunction secondaryTransferFunction = ScriptableObject.CreateInstance<TransferFunction>();
                            secondaryTransferFunction.colourControlPoints = new List<TFColourControlPoint>() { new TFColourControlPoint(0.0f, Color.red), new TFColourControlPoint(1.0f, Color.red) };
                            secondaryTransferFunction.GenerateTexture();
                            volrendObj.SetOverlayDataset(dataset);
                            volrendObj.SetSecondaryTransferFunction(secondaryTransferFunction);
                        });
                    }
                    if (GUILayout.Button("Load PET (DICOM)"))
                    {
                        ImportDicomDataset(volrendObj, (VolumeDataset dataset) =>
                        {
                            TransferFunction secondaryTransferFunction = ScriptableObject.CreateInstance<TransferFunction>();
                            secondaryTransferFunction.colourControlPoints = new List<TFColourControlPoint>() { new TFColourControlPoint(0.0f, Color.red), new TFColourControlPoint(1.0f, Color.red) };
                            secondaryTransferFunction.GenerateTexture();
                            volrendObj.SetOverlayDataset(dataset);
                            volrendObj.SetSecondaryTransferFunction(secondaryTransferFunction);
                        });
                    }
                }
                else
                {
                    if (GUILayout.Button("Edit overlay transfer function"))
                    {
                        TransferFunctionEditorWindow.ShowWindow(volrendObj, secondaryTransferFunction);
                    }

                    if (GUILayout.Button("Remove secondary volume"))
                    {
                        volrendObj.SetOverlayDataset(null);
                    }
                }
            }

            // Segmentations
            segmentationSettings = EditorGUILayout.Foldout(segmentationSettings, "Segmentations");
            if (segmentationSettings)
            {
                List<SegmentationLabel> segmentationLabels = volrendObj.GetSegmentationLabels();
                if (segmentationLabels != null && segmentationLabels.Count > 0)
                {
                    for (int i = 0; i < segmentationLabels.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        SegmentationLabel segmentationlabel = segmentationLabels[i];
                        EditorGUI.BeginChangeCheck();
                        segmentationlabel.name = EditorGUILayout.TextField(segmentationlabel.name);
                        segmentationlabel.colour = EditorGUILayout.ColorField(segmentationlabel.colour);
                        bool changed = EditorGUI.EndChangeCheck();
                        segmentationLabels[i] = segmentationlabel;
                        if (GUILayout.Button("delete"))
                        {
                            volrendObj.RemoveSegmentation(segmentationlabel.id);
                        }
                        EditorGUILayout.EndHorizontal();
                        if (changed)
                        {
                            volrendObj.UpdateSegmentationLabels();
                        }
                    }

                    SegmentationRenderMode segmentationRendreMode = (SegmentationRenderMode)EditorGUILayout.EnumPopup("Render mode", volrendObj.GetSegmentationRenderMode());
                    volrendObj.SetSegmentationRenderMode(segmentationRendreMode);
                }
                if (GUILayout.Button("Add segmentation (NRRD, NIFTI)"))
                {
                    ImportImageFileDataset(volrendObj, (VolumeDataset dataset) =>
                    {
                        List<SegmentationLabel> labels = SegmentationBuilder.BuildSegmentations(dataset);
                        volrendObj.AddSegmentation(dataset, labels);
                    });
                }
                if (GUILayout.Button("Add segmentation (DICOM)"))
                {
                    ImportDicomDataset(volrendObj, (VolumeDataset dataset) =>
                    {
                        List<SegmentationLabel> labels = SegmentationBuilder.BuildSegmentations(dataset);
                        volrendObj.AddSegmentation(dataset, labels);
                    });
                }
                if (GUILayout.Button("Clear segmentations"))
                {
                    volrendObj.ClearSegmentations();
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
                volrendObj.SetSamplingRateMultiplier(EditorGUILayout.Slider("Sampling rate multiplier", volrendObj.GetSamplingRateMultiplier(), 0.2f, 2.0f));
            }
        }
        private static async void ImportImageFileDataset(VolumeRenderedObject targetObject, UnityAction<VolumeDataset> onLoad)
        {
            string filePath = EditorUtility.OpenFilePanel("Select a folder to load", "", "");
            ImageFileFormat imageFileFormat = DatasetFormatUtilities.GetImageFileFormat(filePath);
            if (!File.Exists(filePath))
            {
                Debug.LogError($"File doesn't exist: {filePath}");
                return;
            }
            if (imageFileFormat == ImageFileFormat.Unknown)
            {
                Debug.LogError($"Invalid file format: {Path.GetExtension(filePath)}");
                return;
            }

            using (ProgressHandler progressHandler = new ProgressHandler(new EditorProgressView()))
            {
                progressHandler.StartStage(1.0f, "Importing dataset");
                IImageFileImporter importer = ImporterFactory.CreateImageFileImporter(imageFileFormat);
                Task<VolumeDataset> importTask = importer.ImportAsync(filePath);
                await importTask;
                progressHandler.EndStage();

                if (importTask.Result != null)
                {
                    onLoad.Invoke(importTask.Result);
                }
            }
        }

        private static async void ImportDicomDataset(VolumeRenderedObject targetObject, UnityAction<VolumeDataset> onLoad)
        {
            string dir = EditorUtility.OpenFolderPanel("Select a folder to load", "", "");
            if (Directory.Exists(dir))
            {
                using (ProgressHandler progressHandler = new ProgressHandler(new EditorProgressView()))
                {
                    progressHandler.StartStage(1.0f, "Importing dataset");
                    Task<VolumeDataset[]> importTask = EditorDatasetImportUtils.ImportDicomDirectoryAsync(dir, progressHandler);
                    await importTask;
                    progressHandler.EndStage();

                    if (importTask.Result.Length > 0)
                    {
                        onLoad.Invoke(importTask.Result[0]);
                    }
                }
            }
        }
    }
}
