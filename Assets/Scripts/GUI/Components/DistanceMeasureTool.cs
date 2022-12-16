using System.IO;
using UnityEngine;

namespace UnityVolumeRendering
{
    /// <summary>
    /// Rutnime (play mode) GUI for editing a volume's visualisation.
    /// </summary>
    public class DistanceMeasureTool : MonoBehaviour
    {
        public VolumeRenderedObject targetObject;

        private Rect windowRect = new Rect(150, 0, WINDOW_WIDTH, WINDOW_HEIGHT);

        private const int WINDOW_WIDTH = 400;
        private const int WINDOW_HEIGHT = 200;

        private static DistanceMeasureTool instance;

        private int windowID;
        private LineRenderer lineRenderer;

        private void Awake()
        {
            // Fetch a unique ID for our window (see GUI.Window)
            windowID = WindowGUID.GetUniqueWindowID();
        }

        private void Start()
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            Material lineMaterial = new Material(Shader.Find("Standard"));
            lineMaterial.SetColor("_Color", Color.red);
            lineRenderer.material = lineMaterial;
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;
            lineRenderer.startWidth = 0.003f;
            lineRenderer.endWidth = 0.003f;
            lineRenderer.SetPosition(0, Vector3.zero);
            lineRenderer.SetPosition(1, Vector3.zero);
        }
        
        private void Update()
        {
            if (Camera.main != null && Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                VolumeRaycaster raycaster = new VolumeRaycaster();
                if (raycaster.RaycastScene(ray, out RaycastHit hit))
                {
                    //Debug.DrawLine(ray.origin, hit.point, Color.red, 10.0f, true);
                    lineRenderer.SetPosition(0, lineRenderer.GetPosition(1));
                    lineRenderer.SetPosition(1, hit.point);
                }
            }
        }

        public static void ShowWindow()
        {
            if(instance != null)
                GameObject.Destroy(instance);

            GameObject obj = new GameObject("DistanceMeasureTool");
            instance = obj.AddComponent<DistanceMeasureTool>();
        }

        private void OnGUI()
        {
            windowRect = GUI.Window(windowID, windowRect, UpdateWindow, "Distance measure tool");
        }

        private void UpdateWindow(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, 10000, 20));

            GUILayout.BeginVertical();

            // Display distance
            float distance = Vector3.Distance(lineRenderer.GetPosition(0), lineRenderer.GetPosition(1));
            GUILayout.Label($"Distance: {distance}");

            GUILayout.FlexibleSpace();

            GUILayout.Label("Click on the volume to select points to measure between.");

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
