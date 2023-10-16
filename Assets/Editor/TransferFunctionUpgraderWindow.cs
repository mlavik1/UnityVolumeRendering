using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnityVolumeRendering
{
    public class TransferFunctionUpgraderWindow : EditorWindow
    {
        private enum TFConversionState
        {
            ReadyToImport,
            ReadyToConvert,
            ReadyToSave
        }

        private TransferFunction transferFunction = null;
        private VolumeRenderedObject targetObject = null;
        private string infoText = "";
        private string errorText = "";
        private string tfFilePath = "";

        private TFConversionState state = TFConversionState.ReadyToImport;

        public static void ShowWindow()
        {
            TransferFunctionUpgraderWindow wnd = new TransferFunctionUpgraderWindow();
            wnd.Show();
        }

        private void OnGUI()
        {
            if (state == TFConversionState.ReadyToImport)
            {
                targetObject = null;
                transferFunction = null;
            }

            GUIStyle headerStyle = new GUIStyle(EditorStyles.label);
            headerStyle.fontSize = 20;

            GUIStyle groupHeaderStyle = new GUIStyle(EditorStyles.label);
            groupHeaderStyle.fontSize = 16;
            groupHeaderStyle.fontStyle = FontStyle.Bold;

            GUIStyle errorStyle = new GUIStyle(EditorStyles.label);
            errorStyle.normal.textColor = Color.red;

            GUIStyle infoStyle = new GUIStyle(EditorStyles.label);
            infoStyle.wordWrap = true;

            EditorGUILayout.LabelField("Transfer function upgrader tool", headerStyle);
            
            EditorGUILayout.Space();

            if (infoText != "")
            {
                EditorGUILayout.LabelField(infoText);
                EditorGUILayout.Space();
            }

            if (errorText != "")
            {
                EditorGUILayout.LabelField(errorText, errorStyle);
                EditorGUILayout.Space();
            }

            GUILayout.Label("Import", groupHeaderStyle);
            if (GUILayout.Button("Load transfer function"))
            {
                tfFilePath = EditorUtility.OpenFilePanel("Load transfer function", "", "tf");
                transferFunction = TransferFunctionDatabase.LoadTransferFunction(tfFilePath);
                if (transferFunction == null)
                    errorText = "Invalid transfer function";
                else
                {
                    if (transferFunction.relativeScale)
                    {
                        infoText = "Old transfer function with relative scale detected. Please convert it.";
                        state = TFConversionState.ReadyToConvert;
                    }
                    else
                    {
                        infoText = "Transfer function already up-to-date. Nothing was done.";
                    }
                }
            }
            EditorGUILayout.Space();

            if (state == TFConversionState.ReadyToConvert || state == TFConversionState.ReadyToSave)
            {
                GUILayout.Label("Select target object (dataset)", groupHeaderStyle);
                targetObject = EditorGUILayout.ObjectField("Target object", targetObject, typeof(VolumeRenderedObject), true) as VolumeRenderedObject;
                if (targetObject != null && targetObject.dataset != null)
                {
                    ConvertToAbsoluteTF(transferFunction, targetObject.dataset);
                    infoText = "The transfer function has been converted. Please save it.";
                    state = TFConversionState.ReadyToSave;
                }
                GUILayout.Label("Since the transfer function was using data values relative to a dataset," +
                    "we need a reference to this dataset in order to convert the values to Hounsfield scale, which is now the default.", infoStyle);
                EditorGUILayout.Space();
            }

            if (state == TFConversionState.ReadyToSave)
            {
                GUILayout.Label("Save converted dataset", groupHeaderStyle);
                if (targetObject == null || transferFunction == null)
                {
                    state = TFConversionState.ReadyToImport;
                    return;
                }

                if (GUILayout.Button("Save transfer function"))
                {
                    string filepath = EditorUtility.SaveFilePanel("Save transfer function", Path.GetDirectoryName(tfFilePath), Path.GetFileName(tfFilePath), "tf");
                    TransferFunctionDatabase.SaveTransferFunction(transferFunction, filepath);
                    infoText = "Transfer function saved. You can now import a new one.";
                    state = TFConversionState.ReadyToImport;
                }
            }
        }

        private void ConvertToAbsoluteTF(TransferFunction transferFunction, VolumeDataset dataset)
        {
            if (transferFunction.relativeScale)
            {
                float minValue = dataset.GetMinDataValue();
                float maxValue = dataset.GetMaxDataValue();
                for (int i = 0; i < transferFunction.colourControlPoints.Count; i++)
                {
                    TFColourControlPoint point = transferFunction.colourControlPoints[i];
                    point.dataValue = Mathf.Lerp(minValue, maxValue, point.dataValue);
                    transferFunction.colourControlPoints[i] = point;
                }
                for (int i = 0; i < transferFunction.alphaControlPoints.Count; i++)
                {
                    TFAlphaControlPoint point = transferFunction.alphaControlPoints[i];
                    point.dataValue = Mathf.Lerp(minValue, maxValue, point.dataValue);
                    transferFunction.alphaControlPoints[i] = point;
                }
                transferFunction.relativeScale = false;
            }
        }
    }
}
