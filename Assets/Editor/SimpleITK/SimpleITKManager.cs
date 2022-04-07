using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UnityEditor;
using UnityEngine;
using System.IO.Compression;

namespace UnityVolumeRendering
{
    /// <summary>
    /// Manager for the SimpleITK integration.
    /// Since SimpleITK is a native library that requires binaries to be built for your target platform,
    ///  SimpleITK will be disabled by default and can be enabled through this class.
    /// The binaries will be downloaded automatically.
    /// </summary>
    public class SimpleITKManager
    {
        private static string SimpleITKDefinition = "UVR_USE_SIMPLEITK";

        public static bool IsSITKEnabled()
        {
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(target);

            HashSet<string> defines = new HashSet<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';'));
            return defines.Contains(SimpleITKDefinition);
        }

        public static void EnableSITK(bool enable)
        {
            if (!HasDownloadedBinaries())
            {
                EditorUtility.DisplayDialog("Missing SimpleITK binaries", "You need to download the SimpleITK binaries before you can enable SimpleITK.", "Ok");
                return;
            }

            // Enable the UVR_USE_SIMPLEITK preprocessor definition for standalone target
            List<BuildTargetGroup> buildTargetGroups = new List<BuildTargetGroup> (){ BuildTargetGroup.Standalone };
            foreach (BuildTargetGroup group in buildTargetGroups)
            {
                List<string> defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';').ToList();
                defines.Remove(SimpleITKDefinition);
                if (enable)
                    defines.Add(SimpleITKDefinition);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, String.Join(";", defines));
            }

            // Save project and recompile scripts
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#if UNITY_2019_3_OR_NEWER
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
#endif
        }

        public static bool HasDownloadedBinaries()
        {
            string binDir = GetBinaryDirectoryPath();
            return Directory.Exists(binDir) && Directory.GetFiles(binDir).Length > 0; // TODO: Check actual files?
        }

        public static void DownloadBinaries()
        {
            string extractDirPath = GetBinaryDirectoryPath();
            string zipPath = Path.Combine(Directory.GetParent(extractDirPath).FullName, "SimpleITK.zip");
            if (HasDownloadedBinaries())
            {
                if (!EditorUtility.DisplayDialog("Download SimpleITK binaries", "SimpleITK has already been downloaded. Do you want to delete it and download again?", "Yes", "No"))
                {
                    return;
                }
            }

            EditorUtility.DisplayProgressBar("Downloading SimpleITK", "Downloading SimpleITK binaries.", 0);

            // Downlaod binaries zip
            using (var client = new WebClient())
            {
                string downloadURL = "https://sourceforge.net/projects/simpleitk/files/SimpleITK/1.2.4/CSharp/SimpleITK-1.2.4-CSharp-win64-x64.zip/download";
                client.DownloadFile(downloadURL, zipPath);

                EditorUtility.DisplayProgressBar("Downloading SimpleITK", "Downloading SimpleITK binaries.", 70);

                if (!File.Exists(zipPath))
                {
                    Debug.Log(zipPath);
                    EditorUtility.DisplayDialog("Error downloadig SimpleITK binaries.", "Failed to download SimpleITK binaries. Please check your internet connection.", "Close");
                    Debug.Log($"Failed to download SimpleITK binaries. You can also try to manually download from {downloadURL} and extract it to some folder inside the Assets folder.");
                    return;
                }

                try
                {
                    ExtractZip(zipPath, extractDirPath);
                }
                catch (Exception ex)
                {
                    string errorString = $"Extracting binaries failed with error: {ex.Message}\n"
                    + $"Please try downloading the zip from: {downloadURL}\nAnd extract it somewhere in the Assets folder.\n\n"
                    + "The download URL can be copied from the error log (console).";
                    Debug.LogError(ex.ToString());
                    Debug.LogError(errorString);
                    EditorUtility.DisplayDialog("Failed to extract binaries.", errorString, "Close");
                }
            }

            File.Delete(zipPath);

            EditorUtility.ClearProgressBar();
        }

        private static void ExtractZip(string zipPath, string extractDirPath)
        {
            // Extract zip
            using (FileStream zipStream = new FileStream(zipPath, FileMode.Open))
            {
                using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Update))
                {
                    if (!Directory.Exists(extractDirPath))
                        Directory.CreateDirectory(extractDirPath);

                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.Name != "" && !entry.Name.EndsWith("/"))
                        {
                            string destFilePath = Path.Combine(extractDirPath, entry.Name);
                            //TextAsset destAsset = new TextAsset("abc");
                            //AssetDatabase.CreateAsset(destAsset, extractDirRelPath + "/" + entry.Name);
                            Stream inStream = entry.Open();

                            using (Stream outStream = File.OpenWrite(destFilePath))
                            {
                                inStream.CopyTo(outStream);
                            }
                        }
                    }
                }
            }
        }

        private static string GetBinaryDirectoryPath()
        {
            string dataPath = Application.dataPath;
            foreach (string file in Directory.EnumerateFiles(Application.dataPath, "*.*", SearchOption.AllDirectories))
            {
                // Search for magic file stored in Assets directory.
                // This is necessary for cases where the UVR plugin is stored in a subfolder (thatæs the case for the asset store version)
                if (Path.GetFileName(file) == "DONOTREMOVE-PathSearchFile.txt")
                {
                    dataPath = Path.GetDirectoryName(file);
                }
            }
            return Path.Combine(dataPath, "3rdparty", "SimpleITK"); // TODO: What is UVR is in a subfolder?
        }
    }
}
