using UnityEngine;
using UnityEditor;

public class TransferFunctionEditorWindow : EditorWindow
{
    private TransferFunction tf = null;
    private Texture2D histogramTexture = null;

    private int movingColPointIndex = -1;
    private int movingAlphaPointIndex = -1;

    [MenuItem("Volume Rendering/Transfer Function")]
    static void ShowWindow()
    {
        TransferFunctionEditorWindow wnd = (TransferFunctionEditorWindow)EditorWindow.GetWindow(typeof(TransferFunctionEditorWindow));
        wnd.Show();
    }

    private void OnEnable()
    {
        const int HIST_WIDTH = 800;
        const int HIST_HEIGHT = 600;
        histogramTexture = new Texture2D(HIST_WIDTH, HIST_HEIGHT, TextureFormat.RGBAFloat, false);
        Color[] histCols = new Color[HIST_WIDTH * HIST_HEIGHT];
        for (int iY = 0; iY < HIST_HEIGHT; iY++)
        {
            for (int iX = 0; iX < HIST_WIDTH; iX++)
            {
                histCols[iX + iY * HIST_WIDTH] = new Color(iX / (float)HIST_WIDTH, iY / (float)HIST_HEIGHT, 0.0f, 1.0f);
            }
        }
        histogramTexture.SetPixels(histCols);
        histogramTexture.Apply();

        tf = new TransferFunction();
        tf.AddControlPoint(new TFColourControlPoint(0.0f, Color.black));
        tf.AddControlPoint(new TFColourControlPoint(0.5f, Color.red));
        tf.AddControlPoint(new TFColourControlPoint(1.0f, Color.white));

        tf.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
        tf.AddControlPoint(new TFAlphaControlPoint(0.5f, 0.5f));
        tf.AddControlPoint(new TFAlphaControlPoint(1.0f, 1.0f));
    }

    private void OnGUI()
    {
        Color oldColour = GUI.color;
        float bgWidth = Mathf.Min(this.position.width - 20.0f, (this.position.height - 50.0f) * 2.0f);
        Rect bgRect = new Rect(0.0f, 0.0f, bgWidth, bgWidth * 0.5f);

        //GUI.DrawTexture(bgRect, histogramTexture);
        tf.GenerateTexture();
        Texture2D tfTexture = tf.GetTexture();
        GUI.DrawTexture(bgRect, tfTexture);

        GUI.color = Color.white;
        GUI.Box(new Rect(bgRect.x, bgRect.y + bgRect.height + 20, bgRect.width, 20.0f), "");

        // Colour control points
        for (int iCol = 0; iCol < tf.colourControlPoints.Count; iCol++)
        {
            TFColourControlPoint colPoint = tf.colourControlPoints[iCol];
            Rect ctrlBox = new Rect(bgRect.x + bgRect.width * colPoint.dataValue, bgRect.y + bgRect.height + 20, 10, 20);
            GUI.color = colPoint.colourValue;
            GUI.skin.box.fontSize = 8;
            GUI.Box(ctrlBox, "|");
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && ctrlBox.Contains(new Vector2(Event.current.mousePosition.x, Event.current.mousePosition.y)))
            {
                movingColPointIndex = iCol;
            }
            else if(movingColPointIndex == iCol)
            {
                colPoint.dataValue = Mathf.Clamp((Event.current.mousePosition.x - bgRect.x) / bgRect.width, 0.0f, 1.0f);
            }
            tf.colourControlPoints[iCol] = colPoint;
        }

        // Alpha control points
        for (int iAlpha = 0; iAlpha < tf.alphaControlPoints.Count; iAlpha++)
        {
            TFAlphaControlPoint alphaPoint = tf.alphaControlPoints[iAlpha];
            Rect ctrlBox = new Rect(bgRect.x + bgRect.width * alphaPoint.dataValue, bgRect.y + (1.0f - alphaPoint.alphaValue) * bgRect.height, 10, 10);
            GUI.color = oldColour;
            GUI.skin.box.fontSize = 6;
            GUI.Box(ctrlBox, "a");
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && ctrlBox.Contains(new Vector2(Event.current.mousePosition.x, Event.current.mousePosition.y)))
            {
                movingAlphaPointIndex = iAlpha;
            }
            else if (movingAlphaPointIndex == iAlpha)
            {
                alphaPoint.dataValue = Mathf.Clamp((Event.current.mousePosition.x - bgRect.x) / bgRect.width, 0.0f, 1.0f);
                alphaPoint.alphaValue = Mathf.Clamp(1.0f - (Event.current.mousePosition.y - bgRect.y) / bgRect.height, 0.0f, 1.0f);
            }
            tf.alphaControlPoints[iAlpha] = alphaPoint;
        }

        if (Event.current.type == EventType.MouseUp)
        {
            movingColPointIndex = -1;
            movingAlphaPointIndex = -1;
        }

        if(Event.current.type == EventType.MouseDown && Event.current.button == 1)
        {
            if (bgRect.Contains(new Vector2(Event.current.mousePosition.x, Event.current.mousePosition.y)))
                tf.alphaControlPoints.Add(new TFAlphaControlPoint(Mathf.Clamp((Event.current.mousePosition.x - bgRect.x) / bgRect.width, 0.0f, 1.0f), Mathf.Clamp(1.0f - (Event.current.mousePosition.y - bgRect.y) / bgRect.height, 0.0f, 1.0f)));
            else
                tf.colourControlPoints.Add(new TFColourControlPoint(Mathf.Clamp((Event.current.mousePosition.x - bgRect.x) / bgRect.width, 0.0f, 1.0f), Random.ColorHSV()));
        }

        if(Application.isPlaying)
        {
            // TEST!!!
            VolumeRenderer volRend = FindObjectOfType<VolumeRenderer>();
            if(volRend != null)
            {
                volRend.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_TFTex", tfTexture);
            }
        }
        GUI.color = oldColour;
    }

    public void OnInspectorUpdate()
    {
        Repaint();
    }
}
