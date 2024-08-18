using System.Collections.Generic;
using System.Linq;
using System;
using UnityEditor;
using UnityEngine;

namespace UnityVolumeRendering
{
    public class ImportSettingsEditorWindow : EditorWindow
    {
        public static void ShowWindow()
        {
            ImportSettingsEditorWindow wnd = new ImportSettingsEditorWindow();
            wnd.Show();
        }

        private void OnGUI()
        {
            GUIStyle headerStyle = new GUIStyle(EditorStyles.label);
            headerStyle.fontSize = 20;
            headerStyle.fixedHeight = 20;

            EditorGUILayout.LabelField("Volume rendering import settings", headerStyle);
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Show prompt asking if you want to downscale the dataset on import?");
            bool showDownscalePrompt = EditorGUILayout.Toggle("", PlayerPrefs.GetInt("DownscaleDatasetPrompt") > 0);
            PlayerPrefs.SetInt("DownscaleDatasetPrompt", showDownscalePrompt ? 1 : 0);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Normalise dataset scale on import?");
            bool normaliseScaleOnImport = EditorGUILayout.Toggle("", PlayerPrefs.GetInt("NormaliseScaleOnImport") > 0);
            PlayerPrefs.SetInt("NormaliseScaleOnImport", normaliseScaleOnImport ? 1 : 0);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Clamp Hounsfield values to body tissues range?");
            bool clampHounsfield = EditorGUILayout.Toggle("", PlayerPrefs.GetInt("ClampHounsfield") > 0);
            PlayerPrefs.SetInt("ClampHounsfield", clampHounsfield ? 1 : 0);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("SimpleITK", headerStyle);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("SimpleITK is a library that adds support for JPEG-compressed DICOM, as well as NRRD and NIFTI formats.\n" +
                "Enabling it will start a download of ca 100MBs of binaries. Supported platforms: Windows, Linux, MacOS.", EditorStyles.wordWrappedLabel);

            if (!SimpleITKManager.IsSITKEnabled())
            {
                if (GUILayout.Button("Enable SimpleITK"))
                {
                    SimpleITKManager.DownloadBinaries();
                    SimpleITKManager.EnableSITK(true);
                }
            }
            else
            {
                if (GUILayout.Button("Disable SimpleITK"))
                {
                    SimpleITKManager.EnableSITK(false);
                }
            }
        }
    }
}
