using UnityEngine;

namespace UnityVolumeRendering
{
    public partial class GUIUtils
    {
        private static ColourPickerPopup colourPicker = null; // TODO: Not static?

        public static Color ColourField(Rect rect, Color colour)
        {
            Color oldColour = GUI.backgroundColor;
            GUI.backgroundColor = colour;
            if (GUI.Button(rect, ""))
            {
                if (colourPicker == null)
                {
                    GameObject obj = new GameObject();
                    colourPicker = obj.AddComponent<ColourPickerPopup>();
                    colourPicker.callback = (Color col) =>
                    {
                        // ???
                    };
                }
            }
            GUI.backgroundColor = oldColour;
            if (colourPicker)
                return colourPicker.GetColour();
            else
                return colour;
        }
    }
}
