using System;
using System.IO;
using UnityEngine;

namespace UnityVolumeRendering
{
    public partial class RuntimeFileBrowser
    {
        public struct DialogResult
        {
            public bool cancelled;
            public string path;
        }

        public delegate void DialogCallback(DialogResult result);

        public static void ShowOpenFileDialog(DialogCallback resultCallback, string directory = "")
        {
            GameObject dialogObject = new GameObject("_OpenFileDialog");
            RuntimeFileBrowserComponent dialogComp = dialogObject.AddComponent<RuntimeFileBrowserComponent>();
            dialogComp.dialogMode = RuntimeFileBrowserComponent.DialogMode.OpenFile;
            dialogComp.callback = resultCallback;
            dialogComp.currentDirectory = GetAbsoluteDirectoryPath(directory);
        }

        public static void ShowSaveFileDialog(DialogCallback resultCallback, string directory = "")
        {
            GameObject dialogObject = new GameObject("_SaveFileDialog");
            RuntimeFileBrowserComponent dialogComp = dialogObject.AddComponent<RuntimeFileBrowserComponent>();
            dialogComp.dialogMode = RuntimeFileBrowserComponent.DialogMode.SaveFile;
            dialogComp.callback = resultCallback;
            dialogComp.currentDirectory = GetAbsoluteDirectoryPath(directory);
        }

        public static void ShowOpenDirectoryDialog(DialogCallback resultCallback, string directory = "")
        {
            GameObject dialogObject = new GameObject("_OpenDirectoryDialog");
            RuntimeFileBrowserComponent dialogComp = dialogObject.AddComponent<RuntimeFileBrowserComponent>();
            dialogComp.dialogMode = RuntimeFileBrowserComponent.DialogMode.OpenDirectory;
            dialogComp.callback = resultCallback;
            dialogComp.currentDirectory = GetAbsoluteDirectoryPath(directory);
        }

        private static string GetAbsoluteDirectoryPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return "";
            else if (Path.IsPathRooted(path))
                return path;
            else
            {
                path = Path.Combine(Path.GetFullPath("."), path);
                if (Directory.Exists(path))
                    return path;
                else
                    return "";
            }
        }
    }
}
