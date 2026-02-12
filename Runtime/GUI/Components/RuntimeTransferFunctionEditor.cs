using UnityEngine;

namespace UnityVolumeRendering
{
    public class RuntimeTransferFunctionEditor : MonoBehaviour
    {
        private static RuntimeTransferFunctionEditor instance = null;
        private TransferFunction tf = null;
        private VolumeRenderedObject volRendObject = null;

        private int windowID;
        private Rect windowRect = new Rect(150, 15, WINDOW_WIDTH, WINDOW_HEIGHT);

        private TransferFunctionEditor tfEditor = new TransferFunctionEditor();

        private const int WINDOW_WIDTH = 620;
        private const int WINDOW_HEIGHT = 400;

        public static void ShowWindow(VolumeRenderedObject volRendObj)
        {
            if(instance != null)
                GameObject.Destroy(instance);

            GameObject obj = new GameObject("RuntimeTransferFunctionEditor");
            instance = obj.AddComponent<RuntimeTransferFunctionEditor>();
            instance.volRendObject = volRendObj;
        }

        private void Awake()
        {
            // Fetch a unique ID for our window (see GUI.Window)
            windowID = WindowGUID.GetUniqueWindowID();
        }

        private void OnEnable()
        {
            tfEditor.Initialise();
        }

        private void Update()
        {
            // Close window if object has been destroyed
            if (!volRendObject)
                CloseWindow();
        }

        private void OnGUI()
        {
            windowRect = GUI.Window(windowID, windowRect, UpdateWindow, "Transfer function");
        }

        private void UpdateWindow(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, 10000, 20));

            GUI.skin.button.alignment = TextAnchor.MiddleCenter;

            Color oldColour = GUI.color; // Used for setting GUI.color when drawing UI elements

            if (volRendObject == null)
                return;

            tf = volRendObject.transferFunction;

            float contentWidth = Mathf.Min(WINDOW_WIDTH, (WINDOW_HEIGHT - 100.0f) * 2.0f);
            float contentHeight = contentWidth * 0.5f;
            
            Rect outerRect = new Rect(0.0f, 0.0f, contentWidth, contentHeight);
            Rect tfEditorRect = new Rect(outerRect.x + 20.0f, outerRect.y + 20.0f, outerRect.width - 40.0f, outerRect.height - 50.0f);

            tfEditor.SetVolumeObject(volRendObject);
            tfEditor.DrawOnGUI(tfEditorRect);

            // Save TF
            if(GUI.Button(new Rect(tfEditorRect.x, tfEditorRect.y + tfEditorRect.height + 20.0f, 70.0f, 30.0f), "Save"))
            {
                RuntimeFileBrowser.ShowSaveFileDialog((RuntimeFileBrowser.DialogResult result) => 
                {
                    if(!result.cancelled)
                    {
                        TransferFunctionDatabase.SaveTransferFunction(tf, result.path);
                    }
                });
            }

            // Load TF
            if(GUI.Button(new Rect(tfEditorRect.x + 75.0f, tfEditorRect.y + tfEditorRect.height + 20.0f, 70.0f, 30.0f), "Load"))
            {
                RuntimeFileBrowser.ShowOpenFileDialog((RuntimeFileBrowser.DialogResult result) => 
                {
                    if(!result.cancelled)
                    {
                        TransferFunction newTF = TransferFunctionDatabase.LoadTransferFunction(result.path);
                        if(newTF != null)
                        {
                            tf = newTF;
                            volRendObject.SetTransferFunction(tf);
                            tfEditor.ClearSelection();
                        }
                    }
                });
            }
             // Clear TF
            if(GUI.Button(new Rect(tfEditorRect.x + 150.0f, tfEditorRect.y + tfEditorRect.height + 20.0f, 70.0f, 30.0f), "Clear"))
            {
                tf = new TransferFunction();
                tf.alphaControlPoints.Add(new TFAlphaControlPoint(0.2f, 0.0f));
                tf.alphaControlPoints.Add(new TFAlphaControlPoint(0.8f, 1.0f));
                tf.colourControlPoints.Add(new TFColourControlPoint(0.5f, new Color(0.469f, 0.354f, 0.223f, 1.0f)));
                volRendObject.SetTransferFunction(tf);
                tfEditor.ClearSelection();
            }

            // Colour picker
            Color? selectedColour = tfEditor.GetSelectedColour();
            if (selectedColour != null)
            {
                Color newColour = GUIUtils.ColourField(new Rect(tfEditorRect.x + 250, tfEditorRect.y + tfEditorRect.height + 20, 80.0f, 30.0f), selectedColour.Value);
                tfEditor.SetSelectedColour(newColour);
            }

            GUI.skin.label.wordWrap = false;    
            GUI.Label(new Rect(tfEditorRect.x, tfEditorRect.y + tfEditorRect.height + 55.0f, 720.0f, 50.0f), "Left click to select and move a control point.\nRight click to add a control point, and ctrl + right click to delete.");

            GUI.color = oldColour;

            if (GUI.Button(new Rect(WINDOW_WIDTH - 100, WINDOW_HEIGHT - 40, 90, 30), "Close"))
            {
                CloseWindow();
            }
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

        private void CloseWindow()
        {
            GameObject.Destroy(this.gameObject);
        }
    }
}
