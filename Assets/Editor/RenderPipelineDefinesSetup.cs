using UnityEditor;
using UnityEngine;

namespace UnityVolumeRendering
{
    /// <summary>
    /// Automatically adds scripting define symbols when render pipeline packages are detected.
    /// This allows shaders to use #ifdef UVR_URP and #ifdef UVR_HDRP to conditionally compile code.
    /// </summary>
    [InitializeOnLoad]
    public class RenderPipelineDefinesSetup
    {
        private const string URP_DEFINE = "UVR_URP";
        private const string HDRP_DEFINE = "UVR_HDRP";

        static RenderPipelineDefinesSetup()
        {
            UpdateDefines();
        }

        [MenuItem("Tools/Unity Volume Rendering/Update Render Pipeline Defines")]
        public static void UpdateDefines()
        {
            bool urpInstalled = IsPackageInstalled("com.unity.render-pipelines.universal");
            bool hdrpInstalled = IsPackageInstalled("com.unity.render-pipelines.high-definition");

            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            string currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

            bool modified = false;

            // Add or remove URP define
            if (urpInstalled && !currentDefines.Contains(URP_DEFINE))
            {
                currentDefines = AddDefine(currentDefines, URP_DEFINE);
                modified = true;
                Debug.Log($"[Unity Volume Rendering] Added {URP_DEFINE} define (URP package detected)");
            }
            else if (!urpInstalled && currentDefines.Contains(URP_DEFINE))
            {
                currentDefines = RemoveDefine(currentDefines, URP_DEFINE);
                modified = true;
                Debug.Log($"[Unity Volume Rendering] Removed {URP_DEFINE} define (URP package not found)");
            }

            // Add or remove HDRP define
            if (hdrpInstalled && !currentDefines.Contains(HDRP_DEFINE))
            {
                currentDefines = AddDefine(currentDefines, HDRP_DEFINE);
                modified = true;
                Debug.Log($"[Unity Volume Rendering] Added {HDRP_DEFINE} define (HDRP package detected)");
            }
            else if (!hdrpInstalled && currentDefines.Contains(HDRP_DEFINE))
            {
                currentDefines = RemoveDefine(currentDefines, HDRP_DEFINE);
                modified = true;
                Debug.Log($"[Unity Volume Rendering] Removed {HDRP_DEFINE} define (HDRP package not found)");
            }

            if (modified)
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, currentDefines);
            }
        }

        private static bool IsPackageInstalled(string packageName)
        {
            var listRequest = UnityEditor.PackageManager.Client.List(true, false);
            while (!listRequest.IsCompleted) { }

            if (listRequest.Error != null)
            {
                Debug.LogError($"Error checking for package {packageName}: {listRequest.Error.message}");
                return false;
            }

            foreach (var package in listRequest.Result)
            {
                if (package.name == packageName)
                {
                    return true;
                }
            }

            return false;
        }

        private static string AddDefine(string defines, string newDefine)
        {
            if (string.IsNullOrEmpty(defines))
                return newDefine;

            return defines + ";" + newDefine;
        }

        private static string RemoveDefine(string defines, string defineToRemove)
        {
            if (string.IsNullOrEmpty(defines))
                return "";

            var definesList = new System.Collections.Generic.List<string>(defines.Split(';'));
            definesList.RemoveAll(d => d.Trim() == defineToRemove);
            return string.Join(";", definesList);
        }
    }
}
