using System;
using System.IO;
using UnityEngine;

namespace UnityVolumeRendering
{
    public class ColourPickerPopup : MonoBehaviour
    {
        public Action<Color> callback = null;
        private Color currentColour;
        private Rect windowRect = new Rect(150, 15, WINDOW_WIDTH, WINDOW_HEIGHT);
        private Rect colourBoxRect = new Rect(10, 30, 250, 250);
        private int windowID;
        private Texture2D texture;
        private Color[] gradientColours = { Color.red, Color.green, Color.blue, Color.red };

        private const int WINDOW_WIDTH = 500;
        private const int WINDOW_HEIGHT = 400;
        private const int TEXTURE_WIDTH = 128;
        private const int TEXTURE_HEIGHT = 128;


        public Color GetColour()
        {
            return currentColour;
        }

        private void Awake()
        {
            // Fetch a unique ID for our window (see GUI.Window)
            windowID = WindowGUID.GetUniqueWindowID();
        }

        private void OnGUI()
        {
            windowRect = GUI.Window(windowID, windowRect, UpdateWindow, "Colour picker");
        }

        private void UpdateWindow(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, 10000, 20));

            GUI.DrawTexture(colourBoxRect, GetTexture());

            if (GUI.Button(new Rect(400, 350, 100, 60), "Close"))
            {
                CloseBrowser();
            }

            Event currentEvent = Event.current;
            Vector2 mousePos = currentEvent.mousePosition;
            if (currentEvent.type == EventType.MouseDown && colourBoxRect.Contains(mousePos))
            {
                Vector2 mousePosNorm = new Vector2((mousePos.x - colourBoxRect.x) / colourBoxRect.width, (mousePos.y - colourBoxRect.y) / colourBoxRect.height);
                currentColour = GetColourAtPoint(mousePosNorm);
            }
        }

        private Color GetColourAtPoint(Vector2 point)
        {
            Color hue = GetHueAtPoint(point);
            if (point.y < 0.5f)
                return Color.Lerp(Color.black, hue, point.y * 2.0f);
            else
                return Color.Lerp(hue, Color.white, (point.y - 0.5f) * 2.0f);

        }

        private Color GetHueAtPoint(Vector2 point)
        {
            float tmax = gradientColours.Length - 1.01f;
            float tx = Mathf.Min(point.x * (gradientColours.Length - 1), tmax);
            int xint = (int)tx;
            float xdec = tx % 1;
            Color aCol = gradientColours[xint];
            Color bCol = gradientColours[xint + 1];
            Color col = Color.Lerp(aCol, bCol, xdec);
            return col;
        }

        private Texture2D GetTexture()
        {
            if (texture == null)
            {
                int textureDimension = TEXTURE_WIDTH * TEXTURE_HEIGHT;
                Color[] colours = new Color[textureDimension];

                for (int ix = 0; ix < TEXTURE_WIDTH; ix++)
                {
                    for (int iy = 0; iy < TEXTURE_HEIGHT; iy++)
                    {
                        Color colour = GetColourAtPoint(new Vector2((float)ix / TEXTURE_WIDTH, (float)iy / TEXTURE_HEIGHT));
                        colours[ix + iy * TEXTURE_WIDTH] = colour;
                    }
                }

                texture = new Texture2D(TEXTURE_WIDTH, TEXTURE_HEIGHT, TextureFormat.RGBAFloat, false);
                texture.wrapMode = TextureWrapMode.Clamp;
                texture.SetPixels(colours);
                texture.Apply();
            }
            return texture;
        }

        private void CloseBrowser()
        {
            callback?.Invoke(currentColour);

            GameObject.Destroy(this.gameObject);
        }
    }
}
