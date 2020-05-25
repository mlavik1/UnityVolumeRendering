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

        public static void ShowOpenFileDialog(DialogCallback resultCallback)
        {
            GameObject dialogObject = new GameObject("_OpenFileDialog");
            RuntimeFileBrowserComponent dialogComp = dialogObject.AddComponent<RuntimeFileBrowserComponent>();
            dialogComp.dialogMode = RuntimeFileBrowserComponent.DialogMode.OpenFile;
            dialogComp.callback = resultCallback;
        }

        public static void ShowSaveFileDialog(DialogCallback resultCallback)
        {
            GameObject dialogObject = new GameObject("_SaveFileDialog");
            RuntimeFileBrowserComponent dialogComp = dialogObject.AddComponent<RuntimeFileBrowserComponent>();
            dialogComp.dialogMode = RuntimeFileBrowserComponent.DialogMode.SaveFile;
            dialogComp.callback = resultCallback;
        }

        public static void ShowOpenDirectoryDialog(DialogCallback resultCallback)
        {
            GameObject dialogObject = new GameObject("_OpenDirectoryDialog");
            RuntimeFileBrowserComponent dialogComp = dialogObject.AddComponent<RuntimeFileBrowserComponent>();
            dialogComp.dialogMode = RuntimeFileBrowserComponent.DialogMode.OpenDirectory;
            dialogComp.callback = resultCallback;
        }
    }
}
