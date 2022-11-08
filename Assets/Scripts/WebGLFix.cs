#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

class MyCustomBuildProcessor : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }
    public void OnPreprocessBuild(BuildReport report)
    {
        PlayerSettings.SetPropertyString("emscriptenArgs", "-s ALLOW_MEMORY_GROWTH=1", BuildTargetGroup.WebGL);
        Debug.Log("OK!");
    }
}
#endif
