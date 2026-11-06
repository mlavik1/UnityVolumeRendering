using System.IO;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace MingRay
{
    internal static class EditorPathUtils
    {
        private const string SampleDataDisplayName = "Sample Data";

        internal static string GetPluginRootPath()
        {
            // Try UPM package resolution first
            var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(EditorPathUtils).Assembly);
            if (packageInfo != null)
            {
                return packageInfo.resolvedPath;
            }

            // Fallback: search for magic file in Assets (for .unitypackage import).
            foreach (string file in Directory.EnumerateFiles(Application.dataPath, "*.*", SearchOption.AllDirectories))
            {
                if (Path.GetFileName(file) == "DONOTREMOVE-PathSearchFile.txt")
                {
                    return Path.GetDirectoryName(file);
                }
            }

            return Application.dataPath;
        }

        internal static string GetSampleDataPath()
        {
            var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(EditorPathUtils).Assembly);
            if (packageInfo != null)
            {
                foreach (Sample sample in Sample.FindByPackage(packageInfo.name, packageInfo.version))
                {
                    if (sample.displayName == SampleDataDisplayName && sample.isImported && Directory.Exists(sample.importPath))
                    {
                        return sample.importPath;
                    }
                }

                string packageSampleDataPath = Path.Combine(packageInfo.resolvedPath, "Samples~", "SampleData");
                if (Directory.Exists(packageSampleDataPath))
                {
                    return packageSampleDataPath;
                }
            }
            else
            {
                string sampleDataPath = Path.Combine(GetPluginRootPath(), "SampleData");
                if (Directory.Exists(sampleDataPath))
                {
                    return sampleDataPath;
                }
            }

            return "";
        }
    }
}
