using System.IO;
using UnityEngine;

namespace UnityVolumeRendering
{
    /// <summary>
    /// Runtime (play mode) GUI for editing a slice orientation.
    /// </summary>
    public class EditSliceGUI : MonoBehaviour
    {
        public SlicingPlane slicingPlane;

        private Rect windowRect = new Rect(150, 0, WINDOW_WIDTH, WINDOW_HEIGHT);

        private const int WINDOW_WIDTH = 250;
        private const int WINDOW_HEIGHT = 200;

        private Vector3 rotation;
        private Vector3 position;
        
        private static EditSliceGUI instance;

        private int windowID;

        private void Awake()
        {
            // Fetch a unique ID for our window (see GUI.Window)
            windowID = WindowGUID.GetUniqueWindowID();
        }

        private void Start()
        {
            rotation = slicingPlane.transform.rotation.eulerAngles;
            position = slicingPlane.transform.position;
        }

        public static void ShowWindow(SlicingPlane sliceRendObj)
        {
            if(instance != null)
                GameObject.Destroy(instance);

            GameObject obj = new GameObject($"EditSliceGUI");
            instance = obj.AddComponent<EditSliceGUI>();
            instance.slicingPlane = sliceRendObj;
        }

        private void OnGUI()
        {
            windowRect = GUI.Window(windowID, windowRect, UpdateWindow, $"Edit slice");
        }

        private void UpdateWindow(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, 10000, 20));

            GUILayout.BeginVertical();

            if(slicingPlane != null)
            {
                // Slice Rotation
                GUILayout.Label("Rotation");
                //GUILayout.Label("x:");
                rotation.x = GUILayout.HorizontalSlider(rotation.x, 0.0f, 360.0f);
                //GUILayout.Label("y:");
                rotation.y = GUILayout.HorizontalSlider(rotation.y, 0.0f, 360.0f);
                //GUILayout.Label("z:");
                rotation.z = GUILayout.HorizontalSlider(rotation.z, 0.0f, 360.0f);
                slicingPlane.transform.rotation = Quaternion.Euler(rotation);

                // Slice Translation
                GUILayout.Label("Translation");
                //GUILayout.Label("x:");
                position.x = GUILayout.HorizontalSlider(position.x, -0.5f, 0.5f);
                //GUILayout.Label("y:");
                position.y = GUILayout.HorizontalSlider(position.y, -0.5f, 0.5f);
                //GUILayout.Label("z:");
                position.z = GUILayout.HorizontalSlider(position.z, -0.5f, 0.5f);
                slicingPlane.transform.position = position;
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
    }
}
