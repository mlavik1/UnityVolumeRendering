using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

namespace UnityVolumeRendering
{
    public class LocalisationManager
    {
        private enum Locale
        {
            English,
            Chinese
        }

        private struct LocalisationEntry
        {
            public string english;
            public string chineseSimplified;
        }

        private static Locale currentLocale = Locale.English;

        private static Dictionary<string, LocalisationEntry> translations = new Dictionary<string, LocalisationEntry>()
        {
            {"MenuBar:VolumeRendering", new LocalisationEntry {
                english= "Volume Rendering",
                chineseSimplified= "体绘制"
            }},
            {"MenuBar:LoadDataset", new LocalisationEntry {
                english= "Load dataset",
                chineseSimplified= "导入数据集"
            }},
            {"MenuBar:LoadDICOM", new LocalisationEntry {
                english= "Load DICOM",
                chineseSimplified= "导入DICOM数据"
            }},
            {"MenuBar:LoadRaw", new LocalisationEntry {
                english= "Load raw dataset",
                chineseSimplified= "导入RAW数据"
            }},
            {"MenuBar:LoadNRRD", new LocalisationEntry {
                english= "Load NRRD dataset",
                chineseSimplified= "导入NRRD数据"
            }},
            {"MenuBar:LoadNIFTI", new LocalisationEntry {
                english= "Load NIFTI dataset",
                chineseSimplified= "导入NIFTI数据"
            }},
            {"MenuBar:LoadPARCHG", new LocalisationEntry {
                english= "Load PARCHG dataset",
                chineseSimplified= "导入PARCHG数据"
            }},
            {"MenuBar:LoadNIFTI", new LocalisationEntry {
                english= "Load image sequence dataset",
                chineseSimplified= "导入图像序列格式"
            }}
        };

        static LocalisationManager()
        {
            currentLocale = Locale.English;
#if UNITY_EDITOR
            if (UnityEditor.EditorPrefs.GetString("Editor.kEditorLocale") == "ChineseSimplified")
                currentLocale = Locale.Chinese;
#endif
            if (!Application.isEditor && Application.systemLanguage.ToString() == "ChineseSimplified")
                currentLocale = Locale.Chinese;
        }

        public static string GetString(string id)
        {
            if (!translations.ContainsKey(id))
            {
                Debug.LogWarning("Localisation key not found: " + id);
                return "";
            }
            LocalisationEntry translation = translations[id];
            return currentLocale == Locale.Chinese ? translation.chineseSimplified : translation.english;
        }
    }
}
