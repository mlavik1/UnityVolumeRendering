using UnityEditor;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace MingRay
{
    /// <summary>
    /// Welcome popup, telling the user where to find things and what changed in the rename.
    /// Opens automatically the first time each version of the package is loaded.
    /// </summary>
    public class WelcomeWindow : EditorWindow
    {
        private const string DocumentationURL = "https://github.com/mlavik1/UnityVolumeRendering/blob/master/Documentation~/Documentation.md";

        private PackageInfo packageInfo;
        private GUIStyle headerStyle;

        [MenuItem(VolumeRendererEditorFunctions.MenuRoot + "Welcome", priority = 2001)]
        public static void ShowWindow()
        {
            WelcomeWindow wnd = GetWindow<WelcomeWindow>(true, "Welcome to MingRay", true);
            wnd.minSize = new Vector2(440.0f, 400.0f);
            wnd.maxSize = wnd.minSize;
            wnd.Show();
        }

        private void OnEnable()
        {
            packageInfo = WelcomeWindowStartup.GetPackageInfo();
        }

        private void OnGUI()
        {
            if (headerStyle == null)
                headerStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 18 };

            GUILayout.Label("Welcome to MingRay", headerStyle);
            if (packageInfo != null)
                EditorGUILayout.LabelField("Version " + packageInfo.version, EditorStyles.miniLabel);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Where to find things", EditorStyles.boldLabel);
            Bullet("Menu bar: \"Tools > MingRay\"");
            Bullet("MingRay panel: all actions in one place, docked next to the Inspector. Open it with the button below or from \"Tools > MingRay > MingRay Panel\".");
            Bullet("Import datasets as assets: right-click in the Project window and choose \"MingRay > Import dataset\".");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Formerly \"Unity Volume Rendering\"", EditorStyles.boldLabel);
            Bullet("The \"Volume Rendering\" menu has moved to \"Tools > MingRay\".");
            Bullet("The namespace changed from \"UnityVolumeRendering\" to \"MingRay\". Unity's API Updater will offer to update your scripts automatically.");

            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Open MingRay panel", GUILayout.Height(28.0f)))
            {
                EditorApplication.delayCall += MingRayPanel.ShowWindow;
                Close();
            }
            if (GUILayout.Button("Documentation", GUILayout.Height(28.0f)))
                Application.OpenURL(DocumentationURL);
            if (GUILayout.Button("Close", GUILayout.Height(28.0f)))
                Close();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        private void Bullet(string text)
        {
            EditorGUILayout.LabelField("• " + text, EditorStyles.wordWrappedLabel);
        }
    }

    [InitializeOnLoad]
    internal static class WelcomeWindowStartup
    {
        private const string LastShownVersionKey = "MingRay.WelcomeLastShownVersion";
        private const string CheckedThisSessionKey = "MingRay.WelcomeCheckedThisSession";

        static WelcomeWindowStartup()
        {
            // Only first time
            if (Application.isBatchMode || SessionState.GetBool(CheckedThisSessionKey, false))
                return;
            SessionState.SetBool(CheckedThisSessionKey, true);
            EditorApplication.delayCall += ShowIfNeeded;
        }

        internal static PackageInfo GetPackageInfo()
        {
            return PackageInfo.FindForAssembly(typeof(WelcomeWindow).Assembly);
        }

        private static void ShowIfNeeded()
        {
            PackageInfo packageInfo = GetPackageInfo();
            string version = packageInfo != null ? packageInfo.version : "";
            if (!EditorPrefs.HasKey(LastShownVersionKey) || EditorPrefs.GetString(LastShownVersionKey) != version)
            {
                EditorPrefs.SetString(LastShownVersionKey, version);
                WelcomeWindow.ShowWindow();
            }
        }
    }
}
