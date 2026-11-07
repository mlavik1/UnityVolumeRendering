using System;
using System.IO;
using System.Net;
using UnityEditor;
using UnityEngine;
using System.IO.Compression;
using UnityEngine.Scripting.APIUpdating;

namespace MingRay
{
    /// <summary>
    /// Manager for the SimpleITK integration.
    /// Since SimpleITK is a native library that requires binaries to be built for your target platform,
    ///  SimpleITK will be disabled by default and can be enabled through this class.
    /// The binaries will be downloaded into an embedded package in the project's Packages folder.
    /// </summary>
    [MovedFrom(true, sourceNamespace: "UnityVolumeRendering")]
    public class SimpleITKManager
    {
        public const string BinariesPackageName = "com.mlavik1.mingray.simpleitk";
        private const string SimpleITKVersion = "2.2.0";

        public static bool IsSITKEnabled()
        {
            return HasDownloadedBinaries();
        }

        public static void EnableSITK(bool enable)
        {
            if (enable)
                Enable();
            else
                Disable();
        }

        public static bool HasDownloadedBinaries()
        {
            string binDir = GetBinaryDirectoryPath();
            return File.Exists(Path.Combine(binDir, "package.json"))
                && Directory.GetFiles(binDir, "SimpleITKCSharpManaged.dll", SearchOption.AllDirectories).Length > 0;
        }

        public static void DownloadBinaries()
        {
            string extractDirPath = GetBinaryDirectoryPath();
            string zipPath = Path.Combine(Path.GetTempPath(), $"SimpleITK-{SimpleITKVersion}.zip");
            if (HasDownloadedBinaries())
            {
                if (!EditorUtility.DisplayDialog("Download SimpleITK binaries", "SimpleITK has already been downloaded. Do you want to download it again?", "Yes", "No"))
                {
                    return;
                }
            }

#if UNITY_EDITOR_WIN
            string downloadURL = $"https://github.com/SimpleITK/SimpleITK/releases/download/v{SimpleITKVersion}/SimpleITK-{SimpleITKVersion}-CSharp-win64-x64.zip";
#elif UNITY_EDITOR_LINUX
            string downloadURL = $"https://github.com/SimpleITK/SimpleITK/releases/download/v{SimpleITKVersion}/SimpleITK-{SimpleITKVersion}-CSharp-linux.zip";
#else
            string downloadURL = $"https://github.com/SimpleITK/SimpleITK/releases/download/v{SimpleITKVersion}/SimpleITK-{SimpleITKVersion}-CSharp-macosx-10.9-anycpu.zip";
#endif

            try
            {
                EditorUtility.DisplayProgressBar("Downloading SimpleITK", "Downloading SimpleITK binaries.", 0.0f);

                using (var client = new WebClient())
                {
                    try
                    {
                        client.DownloadFile(downloadURL, zipPath);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex.ToString());
                    }
                }

                if (!File.Exists(zipPath))
                {
                    EditorUtility.DisplayDialog("Error downloading SimpleITK binaries.", "Failed to download SimpleITK binaries. Please check your internet connection.", "Close");
                    Debug.LogError($"Failed to download SimpleITK binaries. You can also try to manually download from {downloadURL} and extract it to {extractDirPath}, along with a package.json named {BinariesPackageName}.");
                    return;
                }

                EditorUtility.DisplayProgressBar("Downloading SimpleITK", "Extracting SimpleITK.", 0.7f);

                try
                {
                    ExtractZip(zipPath, extractDirPath);
                    WritePackageManifest(extractDirPath);
                }
                catch (Exception ex)
                {
                    string errorString = $"Extracting binaries failed with error: {ex.Message}\n"
                    + $"Please try downloading the zip from: {downloadURL}\nAnd extract it to: {extractDirPath}\n\n"
                    + "The download URL can be copied from the error log (console).";
                    Debug.LogError(ex.ToString());
                    Debug.LogError(errorString);
                    EditorUtility.DisplayDialog("Failed to extract binaries.", errorString, "Close");
                }
            }
            finally
            {
                if (File.Exists(zipPath))
                    File.Delete(zipPath);
                EditorUtility.ClearProgressBar();
            }

            UnityEditor.PackageManager.Client.Resolve();
        }

        private static void Enable()
        {
            BuildTarget activeTarget = EditorUserBuildSettings.activeBuildTarget;
            BuildTargetGroup activeGroup = BuildPipeline.GetBuildTargetGroup(activeTarget);
            if (activeGroup != BuildTargetGroup.Standalone
                && !EditorUtility.DisplayDialog("Build target does not support SimpleITK.",
                $"SimpleITK is only supported in standalone builds and editor, and will not work on your selected build target ({activeTarget}).\n"
                + "Enable SimpleITK for standalone (Windows, Linux, MacOS) and editor?", "Yes", "No"))
            {
                return;
            }

            if (!HasDownloadedBinaries())
                DownloadBinaries();

            if (!HasDownloadedBinaries())
            {
                EditorUtility.DisplayDialog("Missing SimpleITK binaries", "You need to download the SimpleITK binaries before you can enable SimpleITK.", "Ok");
            }
        }

        private static void Disable()
        {
            string binDir = GetBinaryDirectoryPath();
            if (Directory.Exists(binDir)
                && !EditorUtility.DisplayDialog("Disable SimpleITK", $"This will delete the SimpleITK binaries from {binDir}. Continue?", "Yes", "No"))
            {
                return;
            }

            if (Directory.Exists(binDir))
            {
                string manifestPath = Path.Combine(binDir, "package.json");
                if (File.Exists(manifestPath))
                    File.Delete(manifestPath);

                try
                {
                    Directory.Delete(binDir, true);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(ex.ToString());
                    EditorUtility.DisplayDialog("Disable SimpleITK", $"SimpleITK has been disabled, but some binaries are in use and could not be deleted.\nYou can delete {binDir} after restarting Unity.", "Ok");
                }
            }

            UnityEditor.PackageManager.Client.Resolve();
        }

        private static void ExtractZip(string zipPath, string extractDirPath)
        {
            using (FileStream zipStream = new FileStream(zipPath, FileMode.Open))
            {
                using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
                {
                    if (!Directory.Exists(extractDirPath))
                        Directory.CreateDirectory(extractDirPath);

                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.Name != "" && !entry.Name.EndsWith("/"))
                        {
                            string destFilePath = Path.Combine(extractDirPath, entry.Name);
                            using (Stream inStream = entry.Open())
                            using (Stream outStream = File.Create(destFilePath))
                            {
                                inStream.CopyTo(outStream);
                            }
                        }
                    }
                }
            }
        }

        private static void WritePackageManifest(string packageDirPath)
        {
            string manifest = "{\n"
                + $"  \"name\": \"{BinariesPackageName}\",\n"
                + $"  \"version\": \"{SimpleITKVersion}\",\n"
                + "  \"displayName\": \"MingRay SimpleITK Binaries\",\n"
                + "  \"description\": \"SimpleITK binaries used by MingRay. Downloaded from https://github.com/SimpleITK/SimpleITK. Delete this package to disable SimpleITK.\",\n"
                + "  \"license\": \"Apache-2.0\"\n"
                + "}\n";
            File.WriteAllText(Path.Combine(packageDirPath, "package.json"), manifest);
        }

        private static string GetBinaryDirectoryPath()
        {
            string projectPath = Directory.GetParent(Application.dataPath).FullName;
            return Path.Combine(projectPath, "Packages", BinariesPackageName);
        }
    }
}
