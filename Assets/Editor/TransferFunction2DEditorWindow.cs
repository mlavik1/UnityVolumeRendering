using UnityEngine;
using UnityEditor;

public class TransferFunction2DEditorWindow : EditorWindow
{
    private Texture2D hist2DTex = null;

    private bool needsRegenTexture = true;

    private Material tfGUIMat = null;
    private int selectedBoxIndex = -1;

    [MenuItem("Volume Rendering/2D Transfer Function")]
    static void ShowWindow()
    {
        TransferFunctionEditorWindow tf1dWnd = (TransferFunctionEditorWindow)EditorWindow.GetWindow(typeof(TransferFunctionEditorWindow));
        if (tf1dWnd != null)
            tf1dWnd.Close();

        TransferFunction2DEditorWindow tf2dWnd = (TransferFunction2DEditorWindow)EditorWindow.GetWindow(typeof(TransferFunction2DEditorWindow));
        tf2dWnd.Show();
    }

    private void OnEnable()
    {
        tfGUIMat = Resources.Load<Material>("TransferFunction2DGUIMat");
    }

    private void OnGUI()
    {
        VolumeRenderedObject volRend = FindObjectOfType<VolumeRenderedObject>();
        if (volRend == null)
            return;

        if (hist2DTex == null)
            hist2DTex = HistogramTextureGenerator.Generate2DHistogramTexture(volRend.dataset);

        TransferFunction2D tf2d = volRend.transferFunction2D;

        //tf.GenerateTexture();

        Color oldColour = GUI.color;
        float bgWidth = Mathf.Min(this.position.width - 20.0f, (this.position.height - 250.0f) * 2.0f);
        Rect bgRect = new Rect(0.0f, 0.0f, bgWidth, bgWidth * 0.5f);
        Graphics.DrawTexture(bgRect, hist2DTex);
        tfGUIMat.SetTexture("_TFTex", tf2d.GetTexture());
        Graphics.DrawTexture(bgRect, tf2d.GetTexture(), tfGUIMat);

        for (int iBox = 0; iBox < tf2d.boxes.Count; iBox++)
        {
            TransferFunction2D.TF2DBox box = tf2d.boxes[iBox];
            Rect boxRect = new Rect(bgRect.x + box.rect.x * bgRect.width, bgRect.y + (1.0f - box.rect.height - box.rect.y) * bgRect.height, box.rect.width * bgRect.width, box.rect.height * bgRect.height);
            GUI.Label(boxRect, "");

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && boxRect.Contains(new Vector2(Event.current.mousePosition.x, Event.current.mousePosition.y)))
            {
                selectedBoxIndex = iBox;
            }
        }

        if(selectedBoxIndex != -1)
        {
            float startX = bgRect.x;
            float startY = bgRect.y + bgRect.height + 10;
            EditorGUI.BeginChangeCheck();
            TransferFunction2D.TF2DBox box = tf2d.boxes[selectedBoxIndex];
            float oldX = box.rect.x;
            float oldY = box.rect.y;
            box.rect.x = EditorGUI.Slider(new Rect(startX, startY, 200.0f, 20.0f), box.rect.x, 0.0f, 0.99f);
            box.rect.width = EditorGUI.Slider(new Rect(startX + 220.0f, startY, 200.0f, 20.0f), oldX + box.rect.width, box.rect.x + 0.01f, 1.0f) - box.rect.x;
            box.rect.y = EditorGUI.Slider(new Rect(startX, startY + 60, 200.0f, 20.0f), box.rect.y, 0.0f, 1.0f);
            box.rect.height = EditorGUI.Slider(new Rect(startX + 220.0f, startY + 60, 200.0f, 20.0f), oldY + box.rect.height, box.rect.y + 0.01f, 1.0f) - box.rect.y;
            box.colour = EditorGUI.ColorField(new Rect(startX + 450.0f, startY + 10, 100.0f, 20.0f), box.colour);
            box.minAlpha = EditorGUI.Slider(new Rect(startX + 450.0f, startY + 30, 200.0f, 20.0f), box.minAlpha, 0.0f, 1.0f);
            box.alpha = EditorGUI.Slider(new Rect(startX + 450.0f, startY + 60, 200.0f, 20.0f), box.alpha, 0.0f, 1.0f);

            if (GUI.Button(new Rect(bgRect.x + 450.0f, startY + 70, 100.0f, 90.0f), "Add rectangle"))
                tf2d.AddBox(0.1f, 0.1f, 0.8f, 0.8f, Color.white, 0.5f);
            tf2d.boxes[selectedBoxIndex] = box;
            needsRegenTexture |= EditorGUI.EndChangeCheck();
        }

        // TODO: regenerate on add/remove/modify (and do it async)
        if (needsRegenTexture)
        {
            tf2d.GenerateTexture();
            needsRegenTexture = false;
        }
        volRend.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_TFTex", tf2d.GetTexture());
        volRend.GetComponent<MeshRenderer>().sharedMaterial.EnableKeyword("TF2D_ON");

        return;
    }

    public void OnInspectorUpdate()
    {
        Repaint();
    }
}
