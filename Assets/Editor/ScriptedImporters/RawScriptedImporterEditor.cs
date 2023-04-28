#if UNITY_2020_2_OR_NEWER
using UnityEditor;
using UnityEditor.AssetImporters;

namespace UnityVolumeRendering
{
    [CustomEditor(typeof(RawScriptedImporter))]
    public class RawScriptedImporterEditor : ScriptedImporterEditor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            RawScriptedImporter importer = (RawScriptedImporter)target;
            SerializedProperty dimension = serializedObject.FindProperty("dimension");
            SerializedProperty dataFormat = serializedObject.FindProperty("dataFormat");
            SerializedProperty endianness = serializedObject.FindProperty("endianness");
            SerializedProperty bytesToSkip = serializedObject.FindProperty("bytesToSkip");
            
            EditorGUILayout.PropertyField(dimension);
            EditorGUILayout.PropertyField(dataFormat);
            EditorGUILayout.PropertyField(endianness);
            EditorGUILayout.PropertyField(bytesToSkip);

            serializedObject.ApplyModifiedProperties();

            ApplyRevertGUI();
        }
    }
}
#endif
