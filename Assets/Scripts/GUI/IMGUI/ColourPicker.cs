using UnityEngine;

namespace UnityVolumeRendering
{
    public partial class GUIUtils
    {
        private static ColourPickerPopup colourPicker = null; // TODO: Not static?

        private static Texture2D previewTexture = null;
        private static GUIStyle previewStyle = new GUIStyle();

        public static Color ColourField(Rect rect, Color colour)
        {
            if (previewTexture == null)
            {
                previewTexture = new Texture2D(2, 2, TextureFormat.RGBAFloat, false);
            }

            if (colourPicker)
                colour = colourPicker.GetColour();
            
            Color[] previewCols = { colour, colour, colour, colour };
            previewTexture.SetPixels(previewCols);
            previewTexture.Apply();
            previewStyle.normal.background = previewTexture;
            previewStyle.alignment = TextAnchor.MiddleCenter;
            if (GUI.Button(rect, "Colour", previewStyle))
            {
                if (colourPicker == null)
                {
                    GameObject obj = new GameObject();
                    colourPicker = obj.AddComponent<ColourPickerPopup>();
                }
                colourPicker.SetColour(colour);
            }

            if (colourPicker)
                return colourPicker.GetColour();
            else
                return colour;
        }
    }
}
