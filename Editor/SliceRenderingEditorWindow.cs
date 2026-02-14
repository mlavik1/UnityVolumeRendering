using UnityEngine;
using UnityEditor;
using System.Linq;

namespace UnityVolumeRendering
{
    public class SliceRenderingEditorWindow : EditorWindow
    {
        private int selectedPlaneIndex = -1;
        private bool mouseIsDown = false;
        private Vector2 mousePressPosition;
        private Vector2 prevMousePos;
        private Vector2 measurePoint;

        private Texture moveIconTexture;
        private Texture inspectIconTexture;
        private Texture measureIconTexture;
        private Texture lRotateIconTexture;
        private Texture rRotateIconTexture;

        private InputMode inputMode;

        private enum InputMode
        {
            Move,
            Inspect,
            Measure
        }
        
        public static void ShowWindow()
        {
            SliceRenderingEditorWindow wnd = EditorWindow.CreateInstance<SliceRenderingEditorWindow>();
            wnd.Show();
            wnd.SetInitialPosition();
        }

        private void SetInitialPosition()
        {
            Rect rect = this.position;
            rect.width = 600.0f;
            rect.height = 600.0f;
            this.position = rect;
        }

        private void Awake()
        {
            moveIconTexture = Resources.Load<Texture>("Icons/MoveIcon");
            inspectIconTexture = Resources.Load<Texture>("Icons/InspectIcon");
            measureIconTexture = Resources.Load<Texture>("Icons/MeasureIcon");
            lRotateIconTexture = Resources.Load<Texture>("Icons/RotateLeft");
            rRotateIconTexture = Resources.Load<Texture>("Icons/RotateRight");
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
            
            if (Selection.activeGameObject != null)
            {
                int index = System.Array.FindIndex(spawnedPlanes, plane => plane.gameObject == Selection.activeGameObject);
                if (index != -1)
                    selectedPlaneIndex = index;
            }

            Rect bgRect = new Rect(0.0f, 40.0f, 0.0f, 0.0f);
            
            if (selectedPlaneIndex != -1 && spawnedPlanes.Length > 0)
            {
                SlicingPlane planeObj = spawnedPlanes[System.Math.Min(selectedPlaneIndex, spawnedPlanes.Length - 1)];
                Vector3 planeScale = planeObj.transform.lossyScale;
                Vector3 planeNormal = -planeObj.transform.up;
                
                float heightWidthRatio = Mathf.Abs(planeScale.z / planeScale.x);
                float bgWidth = Mathf.Min(this.position.width - 20.0f, (this.position.height - 50.0f) * 2.0f);
                float bgHeight = Mathf.Min(bgWidth, this.position.height - 150.0f);
                bgWidth = bgHeight / heightWidthRatio;
                float ratio = bgWidth / this.position.width;
                if (ratio > 1.0f)
                {
                    bgWidth /= ratio;
                    bgHeight /= ratio;
                }
                bgRect = new Rect(0.0f, 40.0f, bgWidth, bgHeight);

                if (GUI.Toggle(new Rect(0.0f, 0.0f, 40.0f, 40.0f), inputMode == InputMode.Move, new GUIContent(moveIconTexture, "Move slice"), GUI.skin.button))
                    inputMode = InputMode.Move;
                if (GUI.Toggle(new Rect(40.0f, 0.0f, 40.0f, 40.0f), inputMode == InputMode.Inspect, new GUIContent(inspectIconTexture, "Inspect values"), GUI.skin.button))
                    inputMode = InputMode.Inspect;
                if (GUI.Toggle(new Rect(80.0f, 0.0f, 40.0f, 40.0f), inputMode == InputMode.Measure, new GUIContent(measureIconTexture, "Measure distances"), GUI.skin.button))
                    inputMode = InputMode.Measure;

                if (GUI.Button(new Rect(Mathf.Max(bgWidth - 80.0f, 120.0f), 0.0f, 40.0f, 40.0f), new GUIContent(lRotateIconTexture, "Rotate plane to the right"), GUI.skin.button))
                    planeObj.transform.Rotate(planeNormal * -90.0f, Space.World);
                if (GUI.Button(new Rect(Mathf.Max(bgWidth - 40.0f, 160.0f), 0.0f, 40.0f, 40.0f), new GUIContent(rRotateIconTexture, "Rotate plane to the left"), GUI.skin.button))
                    planeObj.transform.Rotate(planeNormal * 90.0f, Space.World);

                
                // Draw the slice view
                Material mat = planeObj.GetComponent<MeshRenderer>().sharedMaterial;
                Graphics.DrawTexture(bgRect, mat.GetTexture("_DataTex"), mat);

                Vector2 relMousePos = Event.current.mousePosition - bgRect.position;
                Vector2 relMousePosNormalised = relMousePos / new Vector2(bgRect.width, bgRect.height);

                // Handle mouse click inside slice view (activates moving the plane with mouse)
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && bgRect.Contains(new Vector2(Event.current.mousePosition.x, Event.current.mousePosition.y)))
                {
                    mouseIsDown = true;
                    mousePressPosition = prevMousePos = relMousePosNormalised;
                }
                
                // Move the plane.
                if (inputMode == InputMode.Move && mouseIsDown)
                {
                    Vector2 mouseOffset = relMousePosNormalised - prevMousePos;
                    if (Mathf.Abs(mouseOffset.y) > 0.00001f)
                        planeObj.transform.Translate(planeObj.transform.up * mouseOffset.y, Space.World);
                }
                // Show value at mouse position.
                else if (inputMode == InputMode.Inspect)
                {
                    if (mouseIsDown)
                        measurePoint = relMousePosNormalised;
                    Vector3 worldSpacePoint = GetWorldPosition(measurePoint, planeObj);
                    float value = GetValueAtPosition(measurePoint, planeObj);
                    GUI.Label(new Rect(0.0f, bgRect.y + bgRect.height + 0.0f, 150.0f, 30.0f), $"Value: {value.ToString()}");
                }
                // Measure distance between two points.
                else if (inputMode == InputMode.Measure)
                {
                    if (mouseIsDown)
                        measurePoint = relMousePosNormalised;
                    
                    Vector2 start = mousePressPosition;
                    Vector2 end = measurePoint;
                    // Convert to data coordinates
                    Vector3 startDataPos = GetDataPosition(start, planeObj);
                    Vector3 endDatapos = GetDataPosition(end, planeObj);
                    
                    // Display distance
                    float distance = Vector3.Distance(startDataPos, endDatapos);
                    GUI.Label(new Rect(0.0f, bgRect.y + bgRect.height + 0.0f, 150.0f, 30.0f), $"Distance: {distance.ToString()}");
                    
                    // Draw line
                    Vector2 lineStart = start * new Vector2(bgRect.width, bgRect.height) + new Vector2(bgRect.x, bgRect.y);
                    Vector2 lineEnd = end * new Vector2(bgRect.width, bgRect.height) + new Vector2(bgRect.x, bgRect.y);
                    Handles.BeginGUI();
                    Handles.color = Color.red;
                    Handles.DrawLine(lineStart, lineEnd);
                    Handles.EndGUI();
                }

                if (mouseIsDown)
                    prevMousePos = relMousePosNormalised;
            
                if (Event.current.type == EventType.MouseUp)
                    mouseIsDown = false;
            }

            // Show buttons for changing the active plane
            if (spawnedPlanes.Length > 0)
            {
                GUI.Label(new Rect(0.0f, bgRect.y + bgRect.height + 20.0f, 450.0f, 20.0f), "Select a plane (previous / next).");
                
                if (GUI.Button(new Rect(0.0f, bgRect.y + bgRect.height + 40.0f, 70.0f, 20.0f), "<"))
                {
                    selectedPlaneIndex = selectedPlaneIndex == 0 ? spawnedPlanes.Length - 1 : selectedPlaneIndex - 1;
                    Selection.activeGameObject = spawnedPlanes[selectedPlaneIndex].gameObject;
                }
                if (GUI.Button(new Rect(90.0f, bgRect.y + bgRect.height + 40.0f, 70.0f, 20.0f), ">"))
                {
                    selectedPlaneIndex = (selectedPlaneIndex + 1) % spawnedPlanes.Length;
                    Selection.activeGameObject = spawnedPlanes[selectedPlaneIndex].gameObject;
                }
            }

            // Show button for adding new plane
            VolumeRenderedObject volRend = FindObjectOfType<VolumeRenderedObject>();
            if (volRend != null)
            {
                if (GUI.Button(new Rect(200.0f, bgRect.y + bgRect.height + 0.0f, 120.0f, 20.0f), "Create XY plane"))
                {
                    selectedPlaneIndex = spawnedPlanes.Length;
                    SlicingPlane plane = volRend.CreateSlicingPlane();
                    UnityEditor.Selection.objects = new UnityEngine.Object[] { plane.gameObject };
                }
                else if (GUI.Button(new Rect(200.0f, bgRect.y + bgRect.height + 20.0f, 120.0f, 20.0f), "Create XZ plane"))
                {
                    selectedPlaneIndex = spawnedPlanes.Length;
                    SlicingPlane plane = volRend.CreateSlicingPlane();
                    plane.transform.localRotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
                    UnityEditor.Selection.objects = new UnityEngine.Object[] { plane.gameObject };
                }
                else if (GUI.Button(new Rect(200.0f, bgRect.y + bgRect.height + 40.0f, 120.0f, 20.0f), "Create ZY plane"))
                {
                    selectedPlaneIndex = spawnedPlanes.Length;
                    SlicingPlane plane = volRend.CreateSlicingPlane();
                    plane.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 90.0f);
                    UnityEditor.Selection.objects = new UnityEngine.Object[] { plane.gameObject };
                }
            }

            // Show button for removing
            if (spawnedPlanes.Length > 0 && GUI.Button(new Rect(320.0f, bgRect.y + bgRect.height + 20.0f, 70.0f, 30.0f), "remove\nplane"))
            {
                SlicingPlane planeToRemove = spawnedPlanes[selectedPlaneIndex];
                GameObject.DestroyImmediate(planeToRemove.gameObject);
            }

            // Show hint
            if (inputMode == InputMode.Move && spawnedPlanes.Length > 0)
                GUI.Label(new Rect(0.0f, bgRect.y + bgRect.height + 70.0f, 450.0f, 30.0f), "Move plane by left clicking in the above view and dragging the mouse,\n or simply move it in the object hierarchy.");
            else if (inputMode == InputMode.Inspect)
                GUI.Label(new Rect(0.0f, bgRect.y + bgRect.height + 70.0f, 450.0f, 30.0f), "Click somewhere to display the data value at a location.");
            else if (inputMode == InputMode.Measure)
                GUI.Label(new Rect(0.0f, bgRect.y + bgRect.height + 70.0f, 450.0f, 30.0f), "Click and drag to measure the distance between two points.");
        }

        public void OnInspectorUpdate()
        {
            Repaint();
        }

        private Vector3 GetWorldPosition(Vector2 relativeMousePosition, SlicingPlane slicingPlane)
        {
            Vector3 planePoint = new Vector3(0.5f - relativeMousePosition.x, 0.0f, relativeMousePosition.y - 0.5f) * 10.0f;
            return slicingPlane.transform.TransformPoint(planePoint);
        }
        
        private Vector3 GetDataPosition(Vector2 relativeMousePosition, SlicingPlane slicingPlane)
        {
            Vector3 worldSpacePosition = GetWorldPosition(relativeMousePosition, slicingPlane);
            Vector3 objSpacePoint = slicingPlane.targetObject.volumeContainerObject.transform.InverseTransformPoint(worldSpacePosition);
            Vector3 uvw = objSpacePoint + Vector3.one * 0.5f;
            VolumeDataset dataset = slicingPlane.targetObject.dataset;
            return new Vector3(uvw.x * dataset.scale.x, uvw.y * dataset.scale.y, uvw.z * dataset.scale.z);
        }

        private float GetValueAtPosition(Vector2 relativeMousePosition, SlicingPlane slicingPlane)
        {
            Vector3 worldSpacePosition = GetWorldPosition(relativeMousePosition, slicingPlane);
            Vector3 objSpacePoint = slicingPlane.targetObject.volumeContainerObject.transform.InverseTransformPoint(worldSpacePosition);
            VolumeDataset dataset = slicingPlane.targetObject.dataset;
            // Convert to texture coordinates.
            Vector3 uvw = objSpacePoint + Vector3.one * 0.5f;
            // Look up data value at current position.
            Vector3Int index = new Vector3Int((int)(uvw.x * dataset.dimX), (int)(uvw.y * dataset.dimY), (int)(uvw.z * dataset.dimZ));
            index.x = Mathf.Clamp(index.x, 0, dataset.dimX - 1);
            index.y = Mathf.Clamp(index.y, 0, dataset.dimY - 1);
            index.z = Mathf.Clamp(index.z, 0, dataset.dimZ - 1);
            return dataset.GetData(index.x, index.y, index.z);
        }

    }
}
