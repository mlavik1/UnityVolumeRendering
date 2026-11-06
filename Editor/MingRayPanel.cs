using System;
using UnityEditor;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace MingRay
{
    /// <summary>
    /// Dockable panel with shortcuts to the most common actions.
    /// Opens as a tab next to the Inspector.
    /// </summary>
    public class MingRayPanel : EditorWindow
    {
        private const string DocumentationURL = "https://github.com/mlavik1/UnityVolumeRendering/blob/master/Documentation~/Documentation.md";
        private const string IssuesURL = "https://github.com/mlavik1/UnityVolumeRendering/issues";

        private PackageInfo packageInfo;
        private GUIStyle headerStyle;
        private Vector2 scrollPos;

        [MenuItem(VolumeRendererEditorFunctions.MenuRoot + "MingRay Panel", priority = 2000)]
        public static void ShowWindow()
        {
            // Dock next to the Inspector if possible.
            Type inspectorType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
            MingRayPanel wnd = inspectorType != null
                ? GetWindow<MingRayPanel>("MingRay", true, inspectorType)
                : GetWindow<MingRayPanel>("MingRay", true);
            wnd.minSize = new Vector2(250.0f, 300.0f);
            wnd.Show();
        }

        private void OnEnable()
        {
            packageInfo = WelcomeWindowStartup.GetPackageInfo();
        }

        private void OnGUI()
        {
            if (headerStyle == null)
                headerStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 16 };

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            GUILayout.Label("MingRay Volume Renderer", headerStyle);

            Section("Load dataset");
            ButtonRow(("Raw dataset", VolumeRendererEditorFunctions.ShowDatasetImporter), ("DICOM", VolumeRendererEditorFunctions.ShowDICOMImporter));
            ButtonRow(("NIfTI", VolumeRendererEditorFunctions.ShowNIFTIDatasetImporter), ("NRRD", VolumeRendererEditorFunctions.ShowNRRDDatasetImporter));
            ButtonRow(("Image file", VolumeRendererEditorFunctions.ShowImageFileImporter), ("Image sequence", VolumeRendererEditorFunctions.ShowSequenceImporter));
            ButtonRow(("PARCHG (VASP)", VolumeRendererEditorFunctions.ShowParDatasetImporter));

            Section("Cross section");
            ButtonRow(("Cross section plane", VolumeRendererEditorFunctions.SpawnCrossSectionPlane));
            ButtonRow(("Box cutout", VolumeRendererEditorFunctions.SpawnCutoutBox), ("Sphere cutout", VolumeRendererEditorFunctions.SpawnCutoutSphere));

            Section("Editors");
            ButtonRow(("1D Transfer Function", VolumeRendererEditorFunctions.Show1DTFWindow), ("2D Transfer Function", VolumeRendererEditorFunctions.Show2DTFWindow));
            ButtonRow(("Slice renderer", VolumeRendererEditorFunctions.ShowSliceRenderer), ("Value range", VolumeRendererEditorFunctions.ShowValueRangeWindow));
            ButtonRow(("Settings", VolumeRendererEditorFunctions.ShowSettingsWindow));

            Section("Help");
            ButtonRow(("Documentation", () => Application.OpenURL(DocumentationURL)), ("Report an issue", () => Application.OpenURL(IssuesURL)));
            if (packageInfo != null)
                ButtonRow(("Import samples", () => UnityEditor.PackageManager.UI.Window.Open(packageInfo.name)));
            ButtonRow(("Welcome screen", WelcomeWindow.ShowWindow));

            EditorGUILayout.EndScrollView();
        }

        private void Section(string title)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        }

        private void ButtonRow(params (string label, Action action)[] buttons)
        {
            EditorGUILayout.BeginHorizontal();
            foreach ((string label, Action action) in buttons)
            {
                if (GUILayout.Button(label, GUILayout.Height(24.0f)))
                    EditorApplication.delayCall += () => action();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
