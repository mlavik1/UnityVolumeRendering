using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

namespace UnityVolumeRendering
{
    [CustomEditor(typeof(VolumeRenderedObject))]
    public class VolumeRenderedObjectCustomInspector : Editor, IProgressView
    {
        private bool tfSettings = true;
        private bool lightSettings = true;
        private bool otherSettings = true;
        private bool secondaryVolumeSettings = true;
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

            // Secondary volume
            secondaryVolumeSettings = EditorGUILayout.Foldout(secondaryVolumeSettings, "Overlay volume");
            VolumeDataset secondaryDataset = volrendObj.GetSecondaryDataset();
            TransferFunction secondaryTransferFunction = volrendObj.GetSecondaryTransferFunction();
            if (secondaryDataset == null)
            {
                if (GUILayout.Button("Load PET (NRRD, NIFTI)"))
                {
                    ImportPetScan(volrendObj);
                }
                if (GUILayout.Button("Load PET (DICOM)"))
                {
                    ImportPetScanDicom(volrendObj);
                }
            }
            else
            {
                if (GUILayout.Button("Edit secondary transfer function"))
                {
                    TransferFunctionEditorWindow.ShowWindow(volrendObj, secondaryTransferFunction);
                }

                if (GUILayout.Button("Remove secondary volume"))
                {
                    volrendObj.SetSecondaryDataset(null);
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
        private static async void ImportPetScan(VolumeRenderedObject targetObject)
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
                progressHandler.StartStage(1.0f, "Importing PET dataset");
                IImageFileImporter importer = ImporterFactory.CreateImageFileImporter(imageFileFormat);
                Task<VolumeDataset> importTask = importer.ImportAsync(filePath);
                await importTask;
                progressHandler.EndStage();

                TransferFunction secondaryTransferFunction = ScriptableObject.CreateInstance<TransferFunction>();
                secondaryTransferFunction.colourControlPoints = new List<TFColourControlPoint>() { new TFColourControlPoint(0.0f, Color.red), new TFColourControlPoint(1.0f, Color.red) };
                secondaryTransferFunction.GenerateTexture();
                targetObject.SetSecondaryDataset(importTask.Result);
                targetObject.SetSecondaryTransferFunction(secondaryTransferFunction);
            }
        }

        private static async void ImportPetScanDicom(VolumeRenderedObject targetObject)
        {
            string dir = EditorUtility.OpenFolderPanel("Select a folder to load", "", "");
            if (Directory.Exists(dir))
            {
                using (ProgressHandler progressHandler = new ProgressHandler(new EditorProgressView()))
                {
                    progressHandler.StartStage(1.0f, "Importing PET dataset");
                    Task<VolumeDataset[]> importTask = EditorDatasetImportUtils.ImportDicomDirectoryAsync(dir, progressHandler);
                    await importTask;
                    progressHandler.EndStage();

                    Debug.Assert(importTask.Result.Length > 0);
                    TransferFunction secondaryTransferFunction = ScriptableObject.CreateInstance<TransferFunction>();
                    secondaryTransferFunction.colourControlPoints = new List<TFColourControlPoint>() { new TFColourControlPoint(0.0f, Color.red), new TFColourControlPoint(1.0f, Color.red) };
                    secondaryTransferFunction.GenerateTexture();
                    targetObject.SetSecondaryDataset(importTask.Result[0]);
                    targetObject.SetSecondaryTransferFunction(secondaryTransferFunction);
                }
            }
        }
    }
}
