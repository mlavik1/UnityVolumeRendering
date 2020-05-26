using System.IO;
using UnityEngine;

namespace UnityVolumeRendering
{
    /// <summary>
    /// Rutnime (play mode) GUI for editing a volume's visualisation.
    /// </summary>
    public class EditVolumeGUI : MonoBehaviour
    {
        public VolumeRenderedObject targetObject;

        private Rect windowRect = new Rect(150, 0, WINDOW_WIDTH, WINDOW_HEIGHT);

        private const int WINDOW_WIDTH = 400;
        private const int WINDOW_HEIGHT = 400;

        private int selectedRenderModeIndex = 0;
        private Vector3 rotation;

        private static EditVolumeGUI instance;

        private int windowID;

        private void Awake()
        {
            // Fetch a unique ID for our window (see GUI.Window)
            windowID = WindowGUID.GetUniqueWindowID();
        }

        private void Start()
        {
            rotation = targetObject.transform.rotation.eulerAngles;
        }

        public static void ShowWindow(VolumeRenderedObject volRendObj)
        {
            if(instance != null)
                GameObject.Destroy(instance);

            GameObject obj = new GameObject($"EditVolumeGUI_{volRendObj.name}");
            instance = obj.AddComponent<EditVolumeGUI>();
            instance.targetObject = volRendObj;
        }

        private void OnGUI()
        {
            windowRect = GUI.Window(windowID, windowRect, UpdateWindow, $"Edit volume ({targetObject.dataset.datasetName})");
        }

        private void UpdateWindow(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, 10000, 20));

            GUILayout.BeginVertical();

            if(targetObject != null)
            {
                // Render mode
                GUILayout.Label("Render mode");
                selectedRenderModeIndex = GUILayout.SelectionGrid(selectedRenderModeIndex, new string[] { "Direct volume rendering", "Maximum intensity projection", "Isosurface rendering" }, 2);
                targetObject.SetRenderMode((RenderMode)selectedRenderModeIndex);

                // Visibility window
                GUILayout.Label("Visibility window (min - max visible values)");
                GUILayout.BeginHorizontal();
                Vector2 visibilityWindow = targetObject.GetVisibilityWindow();
                GUILayout.Label("min:");
                visibilityWindow.x = GUILayout.HorizontalSlider(visibilityWindow.x, 0.0f, visibilityWindow.y, GUILayout.Width(150.0f));
                GUILayout.Label("max:");
                visibilityWindow.y = GUILayout.HorizontalSlider(visibilityWindow.y, visibilityWindow.x, 1.0f, GUILayout.Width(150.0f));
                targetObject.SetVisibilityWindow(visibilityWindow);
                GUILayout.EndHorizontal();

                // Rotation
                GUILayout.Label("Rotation");
                rotation.x = GUILayout.HorizontalSlider(rotation.x, 0.0f, 360.0f);
                rotation.y = GUILayout.HorizontalSlider(rotation.y, 0.0f, 360.0f);
                rotation.z = GUILayout.HorizontalSlider(rotation.z, 0.0f, 360.0f);
                targetObject.transform.rotation = Quaternion.Euler(rotation);

                // Load transfer function
                if(GUILayout.Button("Load transfer function", GUILayout.Width(150.0f)))
                {
                    RuntimeFileBrowser.ShowOpenFileDialog(OnLoadTransferFunction);
                }
            }

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            // Show close button
            if (GUILayout.Button("Close"))
            {
                GameObject.Destroy(this.gameObject);
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void OnLoadTransferFunction(RuntimeFileBrowser.DialogResult result)
        {
            if(!result.cancelled)
            {
                string extension = Path.GetExtension(result.path);
                if(extension == ".tf")
                {
                    TransferFunction tf = TransferFunctionDatabase.LoadTransferFunction(result.path);
                    if (tf != null)
                    {
                        targetObject.transferFunction = tf;
                        targetObject.SetTransferFunctionMode(TFRenderMode.TF1D);
                    }
                }
                if (extension == ".tf2d")
                {
                    TransferFunction2D tf = TransferFunctionDatabase.LoadTransferFunction2D(result.path);
                    if (tf != null)
                    {
                        targetObject.transferFunction2D = tf;
                        targetObject.SetTransferFunctionMode(TFRenderMode.TF2D);
                    }
                }
            }
        }
    }
}
