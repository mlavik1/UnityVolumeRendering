using UnityEngine;
using UnityEditor;

namespace UnityVolumeRendering
{
    public class TransferFunctionEditorWindow : EditorWindow
    {
        private TransferFunction tf = null;

        private VolumeRenderedObject volRendObject = null;

        private TransferFunctionEditor tfEditor = new TransferFunctionEditor();

        private bool keepTf = false;

        public static void ShowWindow(VolumeRenderedObject volRendObj)
        {
            // Close all (if any) 2D TF editor windows
            TransferFunction2DEditorWindow[] tf2dWnds = Resources.FindObjectsOfTypeAll<TransferFunction2DEditorWindow>();
            foreach (TransferFunction2DEditorWindow tf2dWnd in tf2dWnds)
                tf2dWnd.Close();

            TransferFunctionEditorWindow wnd = (TransferFunctionEditorWindow)EditorWindow.GetWindow(typeof(TransferFunctionEditorWindow));
            if (volRendObj)
                wnd.volRendObject = volRendObj;
            wnd.Show();
            wnd.SetInitialPosition();
        }

        public static void ShowWindow(VolumeRenderedObject volRendObj, TransferFunction transferFunction)
        {
            // Close all (if any) 2D TF editor windows
            TransferFunction2DEditorWindow[] tf2dWnds = Resources.FindObjectsOfTypeAll<TransferFunction2DEditorWindow>();
            foreach (TransferFunction2DEditorWindow tf2dWnd in tf2dWnds)
                tf2dWnd.Close();

            TransferFunctionEditorWindow wnd = (TransferFunctionEditorWindow)EditorWindow.GetWindow(typeof(TransferFunctionEditorWindow));
            wnd.volRendObject = volRendObj;
            wnd.tf = transferFunction;
            wnd.keepTf = true;
            wnd.Show();
            wnd.SetInitialPosition();
        }

        private void SetInitialPosition()
        {
            Rect rect = this.position;
            rect.width = 800.0f;
            rect.height = 500.0f;
            this.position = rect;
        }

        private void OnEnable()
        {
            tfEditor.Initialise();
        }

        private void OnGUI()
        {
            wantsMouseEnterLeaveWindow = true;

            // Update selected object
            if (volRendObject == null)
                volRendObject = SelectionHelper.GetSelectedVolumeObject();

            if (volRendObject == null)
                return;

            if (!keepTf)
                tf = volRendObject.transferFunction;

            Event currentEvent = new Event(Event.current);

            Color oldColour = GUI.color; // Used for setting GUI.color when drawing UI elements
            
            float contentWidth = Mathf.Min(this.position.width, (this.position.height - 100.0f) * 2.0f);
            float contentHeight = contentWidth * 0.5f;
            
            // Interaction area (slightly larger than the histogram rect)
            Rect outerRect = new Rect(0.0f, 0.0f, contentWidth, contentHeight);
            Rect tfEditorRect = new Rect(outerRect.x + 20.0f, outerRect.y + 20.0f, outerRect.width - 40.0f, outerRect.height - 50.0f);

            tfEditor.SetTarget(volRendObject.dataset, tf);
            tfEditor.DrawOnGUI(tfEditorRect);

            // Draw horizontal zoom slider
            float horZoomMin = tfEditor.zoomRect.x;
            float horZoomMax = tfEditor.zoomRect.x + tfEditor.zoomRect.width;
            EditorGUI.MinMaxSlider(new Rect(tfEditorRect.x, tfEditorRect.y + tfEditorRect.height, tfEditorRect.width, 20.0f), ref horZoomMin, ref horZoomMax, 0.0f, 1.0f);
            tfEditor.zoomRect.x = horZoomMin;
            tfEditor.zoomRect.width = horZoomMax - horZoomMin;

            // Draw vertical zoom slider
            GUIUtility.RotateAroundPivot(270.0f, Vector2.zero);
            GUI.matrix = Matrix4x4.Translate(new Vector3(tfEditorRect.x + tfEditorRect.width, tfEditorRect.y + tfEditorRect.height, 0.0f)) * GUI.matrix;
            float vertZoomMin = tfEditor.zoomRect.y;
            float vertZoomMax = tfEditor.zoomRect.y + tfEditor.zoomRect.height;
            EditorGUI.MinMaxSlider(new Rect(0.0f, 0.0f, tfEditorRect.height, 20.0f), ref vertZoomMin, ref vertZoomMax, 0.0f, 1.0f);
            tfEditor.zoomRect.y = vertZoomMin;
            tfEditor.zoomRect.height = vertZoomMax - vertZoomMin;
            GUI.matrix = Matrix4x4.identity;

            // Save TF
            if(GUI.Button(new Rect(tfEditorRect.x, tfEditorRect.y + tfEditorRect.height + 20.0f, 70.0f, 30.0f), "Save"))
            {
                string filepath = EditorUtility.SaveFilePanel("Save transfer function", "", "default.tf", "tf");
                if(filepath != "")
                    TransferFunctionDatabase.SaveTransferFunction(tf, filepath);
            }

            // Load TF
            if(GUI.Button(new Rect(tfEditorRect.x + 75.0f, tfEditorRect.y + tfEditorRect.height + 20.0f, 70.0f, 30.0f), "Load"))
            {
                string filepath = EditorUtility.OpenFilePanel("Save transfer function", "", "tf");
                if(filepath != "")
                {
                    TransferFunction newTF = TransferFunctionDatabase.LoadTransferFunction(filepath);
                    if(newTF != null)
                    {
                        tf.alphaControlPoints = newTF.alphaControlPoints;
                        tf.colourControlPoints = newTF.colourControlPoints;
                        tf.GenerateTexture();
                        tfEditor.ClearSelection();
                    }
                }
            }
             // Clear TF
            if(GUI.Button(new Rect(tfEditorRect.x + 150.0f, tfEditorRect.y + tfEditorRect.height + 20.0f, 70.0f, 30.0f), "Clear"))
            {
                tf.alphaControlPoints.Clear();
                tf.colourControlPoints.Clear();
                tf.alphaControlPoints.Add(new TFAlphaControlPoint(0.2f, 0.0f));
                tf.alphaControlPoints.Add(new TFAlphaControlPoint(0.8f, 1.0f));
                tf.colourControlPoints.Add(new TFColourControlPoint(0.5f, new Color(0.469f, 0.354f, 0.223f, 1.0f)));
                tf.GenerateTexture();
                tfEditor.ClearSelection();
            }

            Color? selectedColour = tfEditor.GetSelectedColour();
            if (selectedColour != null)
            {
                // Colour picker
                Color newColour = EditorGUI.ColorField(new Rect(tfEditorRect.x + 245, tfEditorRect.y + tfEditorRect.height + 20.0f, 100.0f, 40.0f), selectedColour.Value);
                tfEditor.SetSelectedColour(newColour);

                // Remove colour
                if (GUI.Button(new Rect(tfEditorRect.x + 350.0f, tfEditorRect.y + tfEditorRect.height + 20.0f, 70.0f, 30.0f), "Remove"))
                    tfEditor.RemoveSelectedColour();
            }

            GUI.skin.label.wordWrap = false;    
            GUI.Label(new Rect(tfEditorRect.x, tfEditorRect.y + tfEditorRect.height + 55.0f, 720.0f, 50.0f), "Left click to select and move a control point.\nRight click to add a control point, and ctrl + right click to delete.");

            float tDataPos = (currentEvent.mousePosition.x - tfEditorRect.x) / tfEditorRect.width;
            if (tDataPos >= 0.0f && tDataPos <= 1.0f)
            {
                float dataValue = Mathf.Lerp(volRendObject.dataset.GetMinDataValue(), volRendObject.dataset.GetMaxDataValue(), tDataPos);
                GUI.Label(new Rect(tfEditorRect.x, tfEditorRect.y + tfEditorRect.height + 100.0f, 150.0f, 50.0f), $"Data value: {dataValue}");
            }

            GUI.color = oldColour;
        }

        private void OnSelectionChange()
        {
            VolumeRenderedObject newVolRendObj = Selection.activeGameObject?.GetComponent<VolumeRenderedObject>();
            // If we selected another volume object than the one previously edited in this GUI
            if (volRendObject != null && newVolRendObj != null && newVolRendObj != volRendObject)
                this.Close();
        }

        public void OnInspectorUpdate()
        {
            Repaint();
        }
    }
}
