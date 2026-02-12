using System;
using System.IO;
using UnityEngine;

namespace UnityVolumeRendering
{
    /// <summary>
    /// Runtime filebrowser.
    /// Opens a save/load file/directory browser that can be used during play-mode.
    /// </summary>
    public partial class RuntimeFileBrowser
    {
        public struct DialogResult
        {
            /// <summary>
            /// The user cancelled. Path will be invalid.
            /// </summary>
            public bool cancelled;

            /// <summary>
            /// The path of the file or directory.
            /// </summary>
            public string path;
        }

        public delegate void DialogCallback(DialogResult result);

        /// <summary>
        /// Show a dialog for opening a file
        /// </summary>
        /// <param name="resultCallback">Callback function called when the user has selected a file path</param>
        /// <param name="directory">Path of the file to open</param>
        public static void ShowOpenFileDialog(DialogCallback resultCallback, string directory = "")
        {
            GameObject dialogObject = new GameObject("_OpenFileDialog");
            RuntimeFileBrowserComponent dialogComp = dialogObject.AddComponent<RuntimeFileBrowserComponent>();
            dialogComp.dialogMode = RuntimeFileBrowserComponent.DialogMode.OpenFile;
            dialogComp.callback = resultCallback;
            dialogComp.currentDirectory = GetAbsoluteDirectoryPath(directory);
        }

        /// <summary>
        /// Show a dialog for saving a file
        /// </summary>
        /// <param name="resultCallback">Callback function called when the user has selected a file path</param>
        /// <param name="directory">The selected file path</param>
        public static void ShowSaveFileDialog(DialogCallback resultCallback, string directory = "")
        {
            GameObject dialogObject = new GameObject("_SaveFileDialog");
            RuntimeFileBrowserComponent dialogComp = dialogObject.AddComponent<RuntimeFileBrowserComponent>();
            dialogComp.dialogMode = RuntimeFileBrowserComponent.DialogMode.SaveFile;
            dialogComp.callback = resultCallback;
            dialogComp.currentDirectory = GetAbsoluteDirectoryPath(directory);
        }

        /// <summary>
        /// Show a dialog for opening a directory
        /// </summary>
        /// <param name="resultCallback">Callback function called when the user has selected a directory</param>
        /// <param name="directory">Path of the directory to open</param>
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
