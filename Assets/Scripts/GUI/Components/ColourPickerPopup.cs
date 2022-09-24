using System;
using System.IO;
using UnityEngine;

namespace UnityVolumeRendering
{
    public class ColourPickerPopup : MonoBehaviour
    {
        public Action<Color> callback = null;
        private Vector3 selectedHSV = new Vector3(0.0f, 1.0f, 1.0f);
        private Rect windowRect = new Rect(150, 15, WINDOW_WIDTH, WINDOW_HEIGHT);
        private Rect colourBoxRect = new Rect(10, 30, 250, 250);
        private Rect valueSliderRect = new Rect(280, 30, 20, 250);
        private int windowID;
        private Texture2D texture;
        private Color[] gradientColours = { Color.red, Color.green, Color.blue, Color.red };

        private const int WINDOW_WIDTH = 400;
        private const int WINDOW_HEIGHT = 400;
        private const int TEXTURE_WIDTH = 128;
        private const int TEXTURE_HEIGHT = 128;

        private Vector2 selectedPosition = Vector2.zero;
        private bool movingPoint = false;

        public Color GetColour()
        {
            return Color.HSVToRGB(selectedHSV.x, selectedHSV.y, selectedHSV.z);
        }

        public void SetColour(Color col)
        {
            Color.RGBToHSV(col, out selectedHSV.x, out selectedHSV.y, out selectedHSV.z);
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

            Rect ctrlBox = new Rect(selectedPosition.x, selectedPosition.y , 10, 10);
            GUI.skin.box.fontSize = 6;
            GUI.Box(ctrlBox, "*");

            if (GUI.Button(new Rect(WINDOW_WIDTH - 100, WINDOW_HEIGHT - 40, 90, 30), "Done"))
            {
                CloseBrowser();
            }

            selectedHSV.z = GUI.VerticalSlider(valueSliderRect, selectedHSV.z, 0.0f, 1.0f);

            Event currentEvent = Event.current;
            Vector2 mousePos = currentEvent.mousePosition;
            if (currentEvent.type == EventType.MouseDown && colourBoxRect.Contains(mousePos))
                movingPoint = true;
            else if (currentEvent.type == EventType.MouseUp)
                movingPoint = false;

            if (movingPoint)
            {
                Vector2 unitPos = new Vector2((mousePos.x - colourBoxRect.x) / colourBoxRect.width, (mousePos.y - colourBoxRect.y) / colourBoxRect.height);
                unitPos = (unitPos - new Vector2(0.5f, 0.5f)) * new Vector2(2.0f, -2.0f);
                if (unitPos.magnitude <= 1.0f)
                {
                    selectedPosition = mousePos;
                    selectedHSV = GetHSVAtPoint(unitPos, selectedHSV.z);
                }
            }
        }

        private Vector3 GetHSVAtPoint(Vector2 point, float value)
        {
            Vector2 a = new Vector2(0.0f, 1.0f);
            Vector2 b = point;
            float signedAngle = Vector2.SignedAngle(a, b);
            float angle = (signedAngle < 0.0f ? signedAngle + 360.0f : signedAngle);
            float hue = angle / 360.0f;
            float saturation = point.magnitude;
            return new Vector3(hue, saturation, value);
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
                        Vector2 unitPos = new Vector2((float)ix / TEXTURE_WIDTH, (float)iy / TEXTURE_HEIGHT);
                        unitPos = (unitPos - new Vector2(0.5f, 0.5f)) * 2.0f;
                        Color colour;
                        if (unitPos.magnitude <= 1.0f)
                        {
                            Vector3 hsv = GetHSVAtPoint(unitPos, 1.0f);
                            colour = Color.HSVToRGB(hsv.x, hsv.y, hsv.z);
                        }
                        else
                        {
                            colour = Color.clear;
                        }
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
            callback?.Invoke(GetColour());

            GameObject.Destroy(this.gameObject);
        }
    }
}
