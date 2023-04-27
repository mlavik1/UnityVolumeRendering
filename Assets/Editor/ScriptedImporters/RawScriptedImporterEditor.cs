using UnityEngine;
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

            if (GUILayout.Button("Spawn in scene"))
            {
                VolumeDataset datasetAsset = AssetDatabase.LoadAssetAtPath<VolumeDataset>(importer.assetPath);
                VolumeObjectFactory.CreateObject(datasetAsset);
            }

            serializedObject.ApplyModifiedProperties();

            ApplyRevertGUI();
        }
    }
}
