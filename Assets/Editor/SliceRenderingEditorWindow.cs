using UnityEngine;
using UnityEditor;

public class SliceRenderingEditorWindow : EditorWindow
{
    private int selectedPlaneIndex = -1;

    [MenuItem("Volume Rendering/Slice renderer")]
    static void ShowWindow()
    {
        SliceRenderingEditorWindow wnd = (SliceRenderingEditorWindow)EditorWindow.GetWindow(typeof(SliceRenderingEditorWindow));
        wnd.Show();
    }

    private void OnGUI()
    {
        SlicingPlane[] spawnedPlanes = FindObjectsOfType<SlicingPlane>();

        if(spawnedPlanes.Length > 0)
            selectedPlaneIndex = selectedPlaneIndex % spawnedPlanes.Length;

        float bgWidth = Mathf.Min(this.position.width - 20.0f, (this.position.height - 50.0f) * 2.0f);
        Rect bgRect = new Rect(0.0f, 0.0f, bgWidth, bgWidth * 0.5f);
        if(selectedPlaneIndex != -1 && spawnedPlanes.Length > 0)
        {
            SlicingPlane planeObj = spawnedPlanes[System.Math.Min(selectedPlaneIndex, spawnedPlanes.Length - 1)];
            Material mat = planeObj.GetComponent<MeshRenderer>().sharedMaterial;
            Graphics.DrawTexture(bgRect, mat.GetTexture("_DataTex"), mat);
        }

        if(GUI.Button(new Rect(0.0f, bgRect.y + bgRect.height + 20.0f, 100.0f, 100.0f), "<"))
        {
            selectedPlaneIndex = (selectedPlaneIndex - 1) % spawnedPlanes.Length;
        }
        if (GUI.Button(new Rect(120.0f, bgRect.y + bgRect.height + 20.0f, 100.0f, 100.0f), ">"))
        {
            selectedPlaneIndex = (selectedPlaneIndex + 1) % spawnedPlanes.Length;
        }
        if (GUI.Button(new Rect(240.0f, bgRect.y + bgRect.height + 20.0f, 100.0f, 100.0f), "+"))
        {
            VolumeRenderedObject volRend = FindObjectOfType<VolumeRenderedObject>();
            if(volRend != null)
            {
                selectedPlaneIndex = spawnedPlanes.Length;
                volRend.CreateSlicingPlane();
            }
        }
    }

    public void OnInspectorUpdate()
    {
        Repaint();
    }
}
