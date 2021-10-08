using UnityEngine;
using UnityEditor;

namespace UnityVolumeRendering
{
    public class SliceRenderingEditorWindow : EditorWindow
    {
        private int selectedPlaneIndex = -1;
        private bool handleMouseMovement = false;
        private Vector2 prevMousePos;

        public static void ShowWindow()
        {
            SliceRenderingEditorWindow wnd = new SliceRenderingEditorWindow();
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

        private void OnFocus()
        {
            // set selected plane as active GameObject in Hierarchy
            SlicingPlane[] spawnedPlanes = FindObjectsOfType<SlicingPlane>();
            if (selectedPlaneIndex != -1 && spawnedPlanes.Length > 0)
            {
                Selection.activeGameObject = spawnedPlanes[selectedPlaneIndex].gameObject;
            }
        }

        private void OnGUI()
        {
            SlicingPlane[] spawnedPlanes = FindObjectsOfType<SlicingPlane>();

            if (spawnedPlanes.Length > 0)
                selectedPlaneIndex = selectedPlaneIndex % spawnedPlanes.Length;

            float bgWidth = Mathf.Min(this.position.width - 20.0f, (this.position.height - 50.0f) * 2.0f);
            Rect bgRect = new Rect(0.0f, 0.0f, bgWidth, bgWidth * 0.5f);
            if (selectedPlaneIndex != -1 && spawnedPlanes.Length > 0)
            {
                SlicingPlane planeObj = spawnedPlanes[System.Math.Min(selectedPlaneIndex, spawnedPlanes.Length - 1)];
                // Draw the slice view
                Material mat = planeObj.GetComponent<MeshRenderer>().sharedMaterial;
                Graphics.DrawTexture(bgRect, mat.GetTexture("_DataTex"), mat);

                // Handle mouse click inside slice view (activates moving the plane with mouse)
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && bgRect.Contains(new Vector2(Event.current.mousePosition.x, Event.current.mousePosition.y)))
                {
                    handleMouseMovement = true;
                    prevMousePos = Event.current.mousePosition;
                }

                // Handle mouse movement (move the plane)
                if (handleMouseMovement)
                {
                    Vector2 mouseOffset = (Event.current.mousePosition - prevMousePos) / new Vector2(bgRect.width, bgRect.height);
                    if (Mathf.Abs(mouseOffset.y) > 0.00001f)
                    {
                        planeObj.transform.Translate(Vector3.up * mouseOffset.y);
                        prevMousePos = Event.current.mousePosition;
                    }
                }
            }

            if (Event.current.type == EventType.MouseUp)
                handleMouseMovement = false;

            // Show buttons for changing the active plane
            if (spawnedPlanes.Length > 0)
            {
                if (GUI.Button(new Rect(0.0f, bgRect.y + bgRect.height + 20.0f, 70.0f, 30.0f), "previous\nplane"))
                {
                    selectedPlaneIndex = (selectedPlaneIndex - 1) % spawnedPlanes.Length;
                    Selection.activeGameObject = spawnedPlanes[selectedPlaneIndex].gameObject;
                }
                if (GUI.Button(new Rect(90.0f, bgRect.y + bgRect.height + 20.0f, 70.0f, 30.0f), "next\nplane"))
                {
                    selectedPlaneIndex = (selectedPlaneIndex + 1) % spawnedPlanes.Length;
                    Selection.activeGameObject = spawnedPlanes[selectedPlaneIndex].gameObject;
                }
            }

            // Show button for adding new plane
            if (GUI.Button(new Rect(180.0f, bgRect.y + bgRect.height + 20.0f, 70.0f, 30.0f), "add\nplane"))
            {
                VolumeRenderedObject volRend = FindObjectOfType<VolumeRenderedObject>();
                if (volRend != null)
                {
                    selectedPlaneIndex = spawnedPlanes.Length;
                    volRend.CreateSlicingPlane();
                }
            }

            // Show button for removing
            if (spawnedPlanes.Length > 0 && GUI.Button(new Rect(270.0f, bgRect.y + bgRect.height + 20.0f, 70.0f, 30.0f), "remove\nplane"))
            {
                SlicingPlane planeToRemove = spawnedPlanes[selectedPlaneIndex];
                GameObject.DestroyImmediate(planeToRemove.gameObject);
            }

            // Show hint
            if (spawnedPlanes.Length > 0)
                GUI.Label(new Rect(0.0f, bgRect.y + bgRect.height + 60.0f, 450.0f, 30.0f), "Move plane by left clicking in the above view and dragging the mouse,\n or simply move it in the object hierarchy.");
        }

        public void OnInspectorUpdate()
        {
            Repaint();
        }
    }
}
