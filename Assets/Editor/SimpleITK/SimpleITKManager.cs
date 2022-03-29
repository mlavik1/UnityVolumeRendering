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
            List<BuildTargetGroup> buildTargetGroups = new List<BuildTargetGroup> (){ BuildTargetGroup.Standalone };

            foreach (BuildTargetGroup group in buildTargetGroups)
            {
                List<string> defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';').ToList();
                defines.Remove(SimpleITKDefinition);
                if (enable)
                    defines.Add(SimpleITKDefinition);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, String.Join(";", defines));
            }
        }

        public static void DownloadBinaries()
        {
            string downloadURL = "https://sourceforge.net/projects/simpleitk/files/SimpleITK/1.2.4/CSharp/SimpleITK-1.2.4-CSharp-win64-x64.zip/download";
            string zipPath = Path.Combine(Application.dataPath, "3rdparty", "SimpleITK.zip");
            string extractDirRelPath = Path.Combine("Assets", "3rdparty", "SimpleITK");
            string extractDirPath = Path.Combine(Application.dataPath, "3rdparty", "SimpleITK");
            if (Directory.Exists(extractDirPath))
            {
                if (!EditorUtility.DisplayDialog("Download SimpleITK binaries", "SimpleITK has already been downloaded. Do you want to delete it and download again?", "Yes", "No"))
                {
                    return;
                }
            }

            using (var client = new WebClient())
            {
                client.DownloadFile(downloadURL, zipPath);
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
                                TextAsset destAsset = new TextAsset("abc");
                                AssetDatabase.CreateAsset(destAsset, extractDirRelPath + "/" + entry.Name);
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
        }
    }
}
