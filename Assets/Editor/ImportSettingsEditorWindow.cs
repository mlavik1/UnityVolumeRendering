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

            EditorGUILayout.LabelField("Volume rendering import settings", headerStyle);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Show promt asking if you want to downscale the dataset on import?");
            bool showDownscalePrompt = EditorGUILayout.Toggle("Show downscale prompt", EditorPrefs.GetBool("DownscaleDatasetPrompt"));
            EditorPrefs.SetBool("DownscaleDatasetPrompt", showDownscalePrompt);

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
