using UnityEngine;
using UnityEditor;

namespace UnityVolumeRendering
{
    public class TransferFunctionEditorWindow : EditorWindow
    {
        private TransferFunction tf = null;

        private int movingColPointIndex = -1;
        private int movingAlphaPointIndex = -1;

        private int selectedColPointIndex = -1;

        private VolumeRenderedObject volRendObject = null;
        private Texture2D histTex = null;

        private Material tfGUIMat = null;
        private Material tfPaletteGUIMat = null;

        public static void ShowWindow()
        {
            // Close all (if any) 2D TF editor windows
            TransferFunction2DEditorWindow[] tf2dWnds = Resources.FindObjectsOfTypeAll<TransferFunction2DEditorWindow>();
            foreach (TransferFunction2DEditorWindow tf2dWnd in tf2dWnds)
                tf2dWnd.Close();

            TransferFunctionEditorWindow wnd = (TransferFunctionEditorWindow)EditorWindow.GetWindow(typeof(TransferFunctionEditorWindow));
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
            tfGUIMat = Resources.Load<Material>("TransferFunctionGUIMat");
            tfPaletteGUIMat = Resources.Load<Material>("TransferFunctionPaletteGUIMat");

            volRendObject = SelectionHelper.GetSelectedVolumeObject();
            if (volRendObject == null)
            {
                volRendObject = GameObject.FindObjectOfType<VolumeRenderedObject>();
                if (volRendObject != null)
                    Selection.objects = new Object[] { volRendObject.gameObject };
            }

            if(volRendObject != null)
                volRendObject.SetTransferFunctionMode(TFRenderMode.TF1D);
        }

        private void OnGUI()
        {
            // Update selected object
            if (volRendObject == null)
                volRendObject = SelectionHelper.GetSelectedVolumeObject();

            if (volRendObject == null)
                return;
            tf = volRendObject.transferFunction;

            Event currentEvent = new Event(Event.current);

            Color oldColour = GUI.color; // Used for setting GUI.color when drawing UI elements
            
            float contentWidth = Mathf.Min(this.position.width, (this.position.height - 100.0f) * 2.0f);
            float contentHeight = contentWidth * 0.5f;
            
            // Interaction area (slightly larger than the histogram rect)
            Rect interactRect = new Rect(0.0f, 0.0f, contentWidth, contentHeight);
            // Histogram rect (histogram view and alpha control points)
            Rect histRect = new Rect(interactRect.x + 20.0f, interactRect.y + 20.0f, interactRect.width - 40.0f, interactRect.height - 40.0f);
            // Colour palette rect (colour control points)
            Rect paletteRect = new Rect(histRect.x, histRect.y + histRect.height + 20, histRect.width, 20.0f);

            // TODO: Don't do this every frame
            tf.GenerateTexture();

            // Create histogram texture
            if(histTex == null)
            {
                if(SystemInfo.supportsComputeShaders)
                    histTex = HistogramTextureGenerator.GenerateHistogramTextureOnGPU(volRendObject.dataset);
                else
                    histTex = HistogramTextureGenerator.GenerateHistogramTexture(volRendObject.dataset);
            }

            // Draw histogram
            tfGUIMat.SetTexture("_TFTex", tf.GetTexture());
            tfGUIMat.SetTexture("_HistTex", histTex);
            Graphics.DrawTexture(histRect, tf.GetTexture(), tfGUIMat);

            // Draw colour palette
            Texture2D tfTexture = tf.GetTexture();
            tfPaletteGUIMat.SetTexture("_TFTex", tf.GetTexture());
            Graphics.DrawTexture(new Rect(paletteRect.x, paletteRect.y, paletteRect.width, paletteRect.height), tfTexture, tfPaletteGUIMat);
            
            // Release selected colour/alpha points if mouse leaves window
            if (movingAlphaPointIndex != -1 && !interactRect.Contains(currentEvent.mousePosition))
                movingAlphaPointIndex = -1;
            if (movingColPointIndex != -1 && !(currentEvent.mousePosition.x >= paletteRect.x && currentEvent.mousePosition.x <= paletteRect.x + paletteRect.width))
                movingColPointIndex = -1;

            // Mouse down => Move or remove selected colour control point
            if (currentEvent.type == EventType.MouseDown && paletteRect.Contains(currentEvent.mousePosition))
            {
                float mousePos = (currentEvent.mousePosition.x - paletteRect.x) / paletteRect.width;
                int pointIndex = PickColourControlPoint(mousePos);
                if (pointIndex != -1)
                {
                    // Add control point
                    if(currentEvent.button == 0 && !currentEvent.control)
                    {
                        movingColPointIndex = selectedColPointIndex = pointIndex;
                    }
                    // Remove control point
                    else if(currentEvent.button == 1 && currentEvent.control)
                    {
                        tf.colourControlPoints.RemoveAt(pointIndex);
                        currentEvent.type = EventType.Ignore;
                        movingColPointIndex = selectedColPointIndex = -1;
                    }
                }
            }
            else if (currentEvent.type == EventType.MouseUp)
                movingColPointIndex = -1;

            // Mouse down => Move or remove selected alpha control point
            if (currentEvent.type == EventType.MouseDown)
            {
                Vector2 mousePos = new Vector2((currentEvent.mousePosition.x - histRect.x) / histRect.width, 1.0f - (currentEvent.mousePosition.y - histRect.y) / histRect.height);
                int pointIndex = PickAlphaControlPoint(mousePos);
                if (pointIndex != -1)
                {
                    // Add control point
                    if(currentEvent.button == 0 && !currentEvent.control)
                    {
                        movingAlphaPointIndex = pointIndex;
                    }
                    // Remove control point
                    else if(currentEvent.button == 1 && currentEvent.control)
                    {
                        tf.alphaControlPoints.RemoveAt(pointIndex);
                        currentEvent.type = EventType.Ignore;
                        selectedColPointIndex = -1;
                    }
                }
            }

            // Move selected alpha control point
            if (movingAlphaPointIndex != -1)
            {
                TFAlphaControlPoint alphaPoint = tf.alphaControlPoints[movingAlphaPointIndex];
                alphaPoint.dataValue = Mathf.Clamp((currentEvent.mousePosition.x - histRect.x) / histRect.width, 0.0f, 1.0f);
                alphaPoint.alphaValue = Mathf.Clamp(1.0f - (currentEvent.mousePosition.y - histRect.y) / histRect.height, 0.0f, 1.0f);
                tf.alphaControlPoints[movingAlphaPointIndex] = alphaPoint;
            }

            // Move selected colour control point
            if (movingColPointIndex != -1)
            {
                TFColourControlPoint colPoint = tf.colourControlPoints[movingColPointIndex];
                colPoint.dataValue = Mathf.Clamp((currentEvent.mousePosition.x - paletteRect.x) / paletteRect.width, 0.0f, 1.0f);
                tf.colourControlPoints[movingColPointIndex] = colPoint;
            }

            // Draw colour control points
            for (int iCol = 0; iCol < tf.colourControlPoints.Count; iCol++)
            {
                TFColourControlPoint colPoint = tf.colourControlPoints[iCol];
                Rect ctrlBox = new Rect(histRect.x + histRect.width * colPoint.dataValue, histRect.y + histRect.height + 20, 10, 20);
                GUI.color = Color.red;
                GUI.skin.box.fontSize = 6;
                GUI.Box(ctrlBox, "*");
            }

            // Draw alpha control points
            for (int iAlpha = 0; iAlpha < tf.alphaControlPoints.Count; iAlpha++)
            {
                const int pointSize = 10;
                TFAlphaControlPoint alphaPoint = tf.alphaControlPoints[iAlpha];
                Rect ctrlBox = new Rect(histRect.x + histRect.width * alphaPoint.dataValue - pointSize / 2, histRect.y + (1.0f - alphaPoint.alphaValue) * histRect.height - pointSize / 2, pointSize, pointSize);
                GUI.color = Color.red;
                GUI.skin.box.fontSize = 6;
                GUI.Box(ctrlBox, "*");
                GUI.color = oldColour;
            }

            if (currentEvent.type == EventType.MouseUp)
            {
                movingColPointIndex = -1;
                movingAlphaPointIndex = -1;
            }

            // Add points
            if (currentEvent.type == EventType.MouseDown && currentEvent.button == 1)
            {
                if (histRect.Contains(new Vector2(currentEvent.mousePosition.x, currentEvent.mousePosition.y)))
                    tf.alphaControlPoints.Add(new TFAlphaControlPoint(Mathf.Clamp((currentEvent.mousePosition.x - histRect.x) / histRect.width, 0.0f, 1.0f), Mathf.Clamp(1.0f - (currentEvent.mousePosition.y - histRect.y) / histRect.height, 0.0f, 1.0f)));
                else
                    tf.colourControlPoints.Add(new TFColourControlPoint(Mathf.Clamp((currentEvent.mousePosition.x - histRect.x) / histRect.width, 0.0f, 1.0f), Random.ColorHSV()));
                selectedColPointIndex = -1;
            }

            // Save TF
            if(GUI.Button(new Rect(histRect.x, histRect.y + histRect.height + 50.0f, 70.0f, 30.0f), "Save"))
            {
                string filepath = EditorUtility.SaveFilePanel("Save transfer function", "", "default.tf", "tf");
                if(filepath != "")
                    TransferFunctionDatabase.SaveTransferFunction(tf, filepath);
            }

            // Load TF
            if(GUI.Button(new Rect(histRect.x + 75.0f, histRect.y + histRect.height + 50.0f, 70.0f, 30.0f), "Load"))
            {
                string filepath = EditorUtility.OpenFilePanel("Save transfer function", "", "tf");
                if(filepath != "")
                {
                    TransferFunction newTF = TransferFunctionDatabase.LoadTransferFunction(filepath);
                    if(newTF != null)
                        volRendObject.transferFunction = tf = newTF;
                }
            }
             // Clear TF
            if(GUI.Button(new Rect(histRect.x + 150.0f, histRect.y + histRect.height + 50.0f, 70.0f, 30.0f), "Clear"))
            {
                tf = volRendObject.transferFunction = new TransferFunction();
                tf.alphaControlPoints.Add(new TFAlphaControlPoint(0.2f, 0.0f));
                tf.alphaControlPoints.Add(new TFAlphaControlPoint(0.8f, 1.0f));
                tf.colourControlPoints.Add(new TFColourControlPoint(0.5f, new Color(0.469f, 0.354f, 0.223f, 1.0f)));
                selectedColPointIndex = -1;
            }

            // Colour picker
            if (selectedColPointIndex != -1)
            {
                TFColourControlPoint colPoint = tf.colourControlPoints[selectedColPointIndex];
                colPoint.colourValue = EditorGUI.ColorField(new Rect(histRect.x + 225, histRect.y + histRect.height + 50, 100.0f, 40.0f), colPoint.colourValue);
                tf.colourControlPoints[selectedColPointIndex] = colPoint;
            }

            GUI.skin.label.wordWrap = false;    
            GUI.Label(new Rect(histRect.x, histRect.y + histRect.height + 85.0f, 720.0f, 30.0f), "Left click to select and move a control point. Right click to add a control point, and ctrl + right click to delete.");

            GUI.color = oldColour;
        }

        /// <summary>
        /// Pick the colour control point, nearest to the specified position.
        /// </summary>
        /// <param name="maxDistance">Threshold for maximum distance. Points further away than this won't get picked.</param>
        private int PickColourControlPoint(float position, float maxDistance = 0.03f)
        {
            int nearestPointIndex = -1;
            float nearestDist = 1000.0f;
            for (int i = 0; i < tf.colourControlPoints.Count; i++)
            {
                TFColourControlPoint ctrlPoint = tf.colourControlPoints[i];
                float dist = Mathf.Abs(ctrlPoint.dataValue - position);
                if (dist < maxDistance && dist < nearestDist)
                {
                    nearestPointIndex = i;
                    nearestDist = dist;
                }
            }
            return nearestPointIndex;
        }

        /// <summary>
        /// Pick the alpha control point, nearest to the specified position.
        /// </summary>
        /// <param name="maxDistance">Threshold for maximum distance. Points further away than this won't get picked.</param>
        private int PickAlphaControlPoint(Vector2 position, float maxDistance = 0.05f)
        {
            int nearestPointIndex = -1;
            float nearestDist = 1000.0f;
            for (int i = 0; i < tf.alphaControlPoints.Count; i++)
            {
                TFAlphaControlPoint ctrlPoint = tf.alphaControlPoints[i];
                Vector2 ctrlPos = new Vector2(ctrlPoint.dataValue, ctrlPoint.alphaValue);
                float dist = (ctrlPos - position).magnitude;
                if (dist < maxDistance && dist < nearestDist)
                {
                    nearestPointIndex = i;
                    nearestDist = dist;
                }
            }
            return nearestPointIndex;
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
