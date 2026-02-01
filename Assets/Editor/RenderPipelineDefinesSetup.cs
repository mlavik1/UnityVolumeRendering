using UnityEditor;
using UnityEditor.Build;
using UnityEditor.PackageManager;
using UnityEngine;

namespace UnityVolumeRendering
{
    /// <summary>
    /// Automatically adds scripting define symbols when render pipeline packages are detected.
    /// This allows shaders to use #ifdef UVR_URP and #ifdef UVR_HDRP to conditionally compile code.
    /// Updates automatically on Unity load, when build target changes, and when packages are added/removed.
    /// </summary>
    [InitializeOnLoad]
    public class RenderPipelineDefinesSetup : IActiveBuildTargetChanged
    {
        private const string URP_DEFINE = "UVR_URP";
        private const string HDRP_DEFINE = "UVR_HDRP";
        private const string URP_PACKAGE = "com.unity.render-pipelines.universal";
        private const string HDRP_PACKAGE = "com.unity.render-pipelines.high-definition";

        // IActiveBuildTargetChanged implementation
        public int callbackOrder => 0;

        static RenderPipelineDefinesSetup()
        {
            // Run on Unity load
            UpdateDefines();

            // Subscribe to package manager events
            Events.registeredPackages += OnPackagesChanged;
        }

        /// <summary>
        /// Called when packages are added, removed, or updated.
        /// </summary>
        private static void OnPackagesChanged(PackageRegistrationEventArgs args)
        {
            bool needsUpdate = false;

            // Check if any render pipeline packages were added
            foreach (var package in args.added)
            {
                if (package.name == URP_PACKAGE || package.name == HDRP_PACKAGE)
                {
                    needsUpdate = true;
                    Debug.Log($"[Unity Volume Rendering] Detected package addition: {package.name}");
                    break;
                }
            }

            // Check if any render pipeline packages were removed
            if (!needsUpdate)
            {
                foreach (var package in args.removed)
                {
                    if (package.name == URP_PACKAGE || package.name == HDRP_PACKAGE)
                    {
                        needsUpdate = true;
                        Debug.Log($"[Unity Volume Rendering] Detected package removal: {package.name}");
                        break;
                    }
                }
            }

            if (needsUpdate)
            {
                UpdateDefines();
            }
        }

        // Called when build target changes
        public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
        {
            UpdateDefines();
        }

        [MenuItem("Volume Rendering/Update Render Pipeline Defines")]
        private static void UpdateDefines()
        {
            bool urpInstalled = IsPackageInstalled("com.unity.render-pipelines.universal");
            bool hdrpInstalled = IsPackageInstalled("com.unity.render-pipelines.high-definition");

            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            NamedBuildTarget namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);
            string currentDefines = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);

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
                PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, currentDefines);
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
