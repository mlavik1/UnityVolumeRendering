using UnityEngine;

namespace UnityVolumeRendering
{
    public class TransferFunctionEditor
    {
        private int movingColPointIndex = -1;
        private int movingAlphaPointIndex = -1;
        private int selectedColPointIndex = -1;

        private VolumeRenderedObject volRendObject = null;
        private Texture2D histTex = null;

        private Material tfGUIMat = null;
        private Material tfPaletteGUIMat = null;

        private bool rightMouseBtnDown = false;

        private const float COLOUR_PALETTE_HEIGHT = 20.0f;
        private const float COLOUR_POINT_WIDTH = 10.0f;

        // Rectangle to zoom into on the TF (all coordinates are between 0 and 1)
        public Rect zoomRect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);

        public void Initialise()
        {
            tfGUIMat = Resources.Load<Material>("TransferFunctionGUIMat");
            tfPaletteGUIMat = Resources.Load<Material>("TransferFunctionPaletteGUIMat");
        }

        public void SetVolumeObject(VolumeRenderedObject volRendObject)
        {
            this.volRendObject = volRendObject;
        }

        public void DrawOnGUI(Rect rect)
        {
            GUI.skin.button.alignment = TextAnchor.MiddleCenter;

            if (volRendObject == null)
                return;

            TransferFunction tf = volRendObject.transferFunction;

            Event currentEvent = Event.current;

            Color oldColour = GUI.color; // Used for setting GUI.color when drawing UI elements
            
            float contentWidth = rect.width;
            float contentHeight = rect.height;

            // Histogram rect (histogram view and alpha control points)
            Rect histRect = new Rect(rect.x, rect.y, rect.width, rect.height - 40);
            // Mouse interaction area
            Rect histMouseRect = new Rect(histRect.x - 20.0f, histRect.y - 20.0f, histRect.width + 40.0f, histRect.height + 40.0f);
            // Colour palette rect (colour control points)
            Rect paletteRect = new Rect(histRect.x, histRect.y + histRect.height + 20, histRect.width, COLOUR_PALETTE_HEIGHT);
            Rect paletteInteractionRect = new Rect(paletteRect.x - 10.0f, paletteRect.y, paletteRect.width + 30.0f, paletteRect.height);

            Vector2 mousePos = new Vector2((currentEvent.mousePosition.x - histRect.x) / histRect.width, 1.0f - (currentEvent.mousePosition.y - histRect.y) / histRect.height);
            mousePos = ApplyZoom(mousePos);

            // TODO: Don't do this every frame
            tf.GenerateTexture();

            // Create histogram texture
            if(histTex == null)
            {
                if(SystemInfo.supportsComputeShaders)
                    histTex = HistogramTextureGenerator.GenerateHistogramTextureOnGPU(volRendObject.dataset);
                else
                    histTex = HistogramTextureGenerator.GenerateHistogramTexture(volRendObject.dataset);
            }

            // Draw histogram
            tfGUIMat.SetTexture("_TFTex", tf.GetTexture());
            tfGUIMat.SetTexture("_HistTex", histTex);
            tfGUIMat.SetTextureOffset("_TFTex", zoomRect.position);
            tfGUIMat.SetTextureScale("_TFTex", zoomRect.size);
            Graphics.DrawTexture(histRect, tf.GetTexture(), tfGUIMat);

            // Draw colour palette
            Texture2D tfTexture = tf.GetTexture();
            tfPaletteGUIMat.SetTexture("_TFTex", tf.GetTexture());
            tfPaletteGUIMat.SetTextureOffset("_TFTex", zoomRect.position);
            tfPaletteGUIMat.SetTextureScale("_TFTex", zoomRect.size);
            Graphics.DrawTexture(new Rect(paletteRect.x, paletteRect.y, paletteRect.width, paletteRect.height), tfTexture, tfPaletteGUIMat);

            // Release selected colour/alpha points if mouse leaves window
            if (movingAlphaPointIndex != -1 && !histMouseRect.Contains(currentEvent.mousePosition))
                movingAlphaPointIndex = -1;
            if (movingColPointIndex != -1 && !(currentEvent.mousePosition.x >= paletteRect.x && currentEvent.mousePosition.x <= paletteRect.x + paletteRect.width + 20.0f))
                movingColPointIndex = -1;
            if (currentEvent.type == EventType.MouseLeaveWindow)
                movingColPointIndex = -1;

            // Mouse scroll => handle zoom
            if (currentEvent.type == EventType.ScrollWheel && histRect.Contains(currentEvent.mousePosition))
            {
                float zoomDelta = Mathf.Sign(currentEvent.delta.y) * 0.1f;
                HandleZoom(zoomDelta, mousePos);
            }

            // Mouse down => Move or remove selected colour control point
            if (currentEvent.type == EventType.MouseDown && paletteInteractionRect.Contains(currentEvent.mousePosition))
            {
                int pointIndex = PickColourControlPoint(mousePos.x);
                if (pointIndex != -1)
                {
                    // Add control point
                    if(currentEvent.button == 0 && !currentEvent.control)
                    {
                        movingColPointIndex = selectedColPointIndex = pointIndex;
                    }
                    // Remove control point
                    else if(currentEvent.button == 1 && currentEvent.control)
                    {
                        tf.colourControlPoints.RemoveAt(pointIndex);
                        currentEvent.type = EventType.Ignore;
                        movingColPointIndex = selectedColPointIndex = -1;
                    }
                }
            }
            else if (currentEvent.type == EventType.MouseUp)
                movingColPointIndex = -1;

            // Mouse down => Move or remove selected alpha control point
            if (currentEvent.type == EventType.MouseDown && histMouseRect.Contains(currentEvent.mousePosition))
            {
                int pointIndex = PickAlphaControlPoint(mousePos);
                if (pointIndex != -1)
                {
                    // Add control point
                    if(currentEvent.button == 0 && !currentEvent.control)
                    {
                        movingAlphaPointIndex = pointIndex;
                    }
                    // Remove control point
                    else if(currentEvent.button == 1 && currentEvent.control)
                    {
                        tf.alphaControlPoints.RemoveAt(pointIndex);
                        currentEvent.type = EventType.Ignore;
                        selectedColPointIndex = -1;
                    }
                }
            }

            // Move selected alpha control point
            if (movingAlphaPointIndex != -1)
            {
                TFAlphaControlPoint alphaPoint = tf.alphaControlPoints[movingAlphaPointIndex];
                alphaPoint.dataValue = Mathf.Clamp(mousePos.x, 0.0f, 1.0f);
                alphaPoint.alphaValue = Mathf.Clamp(mousePos.y, 0.0f, 1.0f);
                tf.alphaControlPoints[movingAlphaPointIndex] = alphaPoint;
            }

            // Move selected colour control point
            if (movingColPointIndex != -1)
            {
                TFColourControlPoint colPoint = tf.colourControlPoints[movingColPointIndex];
                colPoint.dataValue = Mathf.Clamp(mousePos.x - (COLOUR_POINT_WIDTH / 2.0f) / paletteRect.width * zoomRect.width, 0.0f, 1.0f);
                tf.colourControlPoints[movingColPointIndex] = colPoint;
            }

            // Draw colour control points
            for (int iCol = 0; iCol < tf.colourControlPoints.Count; iCol++)
            {
                TFColourControlPoint colPoint = tf.colourControlPoints[iCol];
                Vector2 colourPointPos = ApplyZoomInverse(new Vector2(colPoint.dataValue, 0.0f));
                if (colourPointPos.x < 0.0f || colourPointPos.x > 1.0f)
                    continue;
                Rect ctrlBox = new Rect(histRect.x + histRect.width * colourPointPos.x, histRect.y + histRect.height + 20, COLOUR_POINT_WIDTH, COLOUR_PALETTE_HEIGHT);
                GUI.color = Color.red;
                GUI.skin.box.fontSize = 6;
                GUI.Box(ctrlBox, "*");
            }

            // Draw alpha control points
            for (int iAlpha = 0; iAlpha < tf.alphaControlPoints.Count; iAlpha++)
            {
                const int pointSize = 10;
                TFAlphaControlPoint alphaPoint = tf.alphaControlPoints[iAlpha];
                Vector2 alphaPointPos = ApplyZoomInverse(new Vector2(alphaPoint.dataValue, alphaPoint.alphaValue));
                if (alphaPointPos.x < 0.0f || alphaPointPos.x > 1.0f || alphaPointPos.y < 0.0f || alphaPointPos.y > 1.0f)
                    continue;
                Rect ctrlBox = new Rect(histRect.x + histRect.width * alphaPointPos.x - pointSize / 2, histRect.y + (1.0f - alphaPointPos.y) * histRect.height - pointSize / 2, pointSize, pointSize);
                GUI.color = Color.red;
                GUI.skin.box.fontSize = 6;
                GUI.Box(ctrlBox, "*");
                GUI.color = oldColour;
            }

            if (currentEvent.type == EventType.MouseUp)
            {
                movingColPointIndex = -1;
                movingAlphaPointIndex = -1;
            }

            // Add points
            if (currentEvent.type == EventType.MouseDown && currentEvent.button == 1)
            {
                rightMouseBtnDown = true;
            }
            else if (currentEvent.type == EventType.MouseUp && currentEvent.button == 1 && rightMouseBtnDown)
            {
                if (histRect.Contains(new Vector2(currentEvent.mousePosition.x, currentEvent.mousePosition.y)))
                {
                    tf.alphaControlPoints.Add(new TFAlphaControlPoint(Mathf.Clamp(mousePos.x, 0.0f, 1.0f), Mathf.Clamp(mousePos.y, 0.0f, 1.0f)));
                }
                else
                {
                    float hue = Random.Range(0.0f, 1.0f);
                    Color newColour = Color.HSVToRGB(hue, 1.0f, 1.0f);
                    tf.colourControlPoints.Add(new TFColourControlPoint(Mathf.Clamp(mousePos.x, 0.0f, 1.0f), newColour));
                }
                selectedColPointIndex = -1;
                
                rightMouseBtnDown = false;
                currentEvent.Use();
            }

            GUI.color = oldColour;
        }

        public void ClearSelection()
        {
            movingColPointIndex = -1;
            movingAlphaPointIndex = -1;
            selectedColPointIndex = -1;
        }

        public Color? GetSelectedColour()
        {
            if (selectedColPointIndex != -1)
                return volRendObject.transferFunction.colourControlPoints[selectedColPointIndex].colourValue;
            else
                return null;
        }

        public void SetSelectedColour(Color colour)
        {
            if (selectedColPointIndex != -1)
            {
                TFColourControlPoint colPoint = volRendObject.transferFunction.colourControlPoints[selectedColPointIndex];
                colPoint.colourValue = colour;
                volRendObject.transferFunction.colourControlPoints[selectedColPointIndex] = colPoint;
            }
        }

        public void RemoveSelectedColour()
        {
            if (selectedColPointIndex != -1)
            {
                volRendObject.transferFunction.colourControlPoints.RemoveAt(selectedColPointIndex);
                selectedColPointIndex = -1;
            }
        }

        // Zoom in/out on TF, centred at a target position
        private void HandleZoom(float zoomDelta, Vector2 zoomTarget)
        {
            if (zoomDelta == 0.0f)
                return;
            
            // Calculate zoom target relative to zoom rectangle
            Vector2 zoomTargetRelative = (zoomTarget - zoomRect.position) / zoomRect.size;

            // Change zoom rect size
            zoomRect.width = Mathf.Clamp(zoomRect.width + zoomDelta, 0.01f, 1.0f);
            zoomRect.height = Mathf.Clamp(zoomRect.height + zoomDelta, 0.01f, 1.0f);

            // Convert zoomTargetRelative back to absolute coordinates (after resizing rect)
            Vector2 currTargetAbsolute = zoomTargetRelative * zoomRect.size + zoomRect.position;
            // Offset rect, to ensure relative zoom target remains fixed
            Vector2 zoomTargetDir = zoomTarget - currTargetAbsolute;
            Vector2 zoomOffset = new Vector2(
                Mathf.Clamp(zoomTargetDir.x, -Mathf.Abs(zoomDelta), Mathf.Abs(zoomDelta)),
                Mathf.Clamp(zoomTargetDir.y, -Mathf.Abs(zoomDelta), Mathf.Abs(zoomDelta))
            );
            zoomRect.position = new Vector2(
                Mathf.Clamp(zoomRect.position.x + zoomOffset.x, 0.0f, 1.0f - zoomRect.width),
                Mathf.Clamp(zoomRect.position.y + zoomOffset.y, 0.0f, 1.0f - zoomRect.height)
            );
        }

        /// <summary>
        /// Pick the colour control point, nearest to the specified position.
        /// </summary>
        /// <param name="maxDistance">Threshold for maximum distance. Points further away than this won't get picked.</param>
        private int PickColourControlPoint(float position, float maxDistance = 0.03f)
        {
            TransferFunction tf = volRendObject.transferFunction;
            int nearestPointIndex = -1;
            float nearestDist = 1000.0f;
            for (int i = 0; i < tf.colourControlPoints.Count; i++)
            {
                TFColourControlPoint ctrlPoint = tf.colourControlPoints[i];
                float dist = Mathf.Abs(ctrlPoint.dataValue - position) / zoomRect.width;
                if (dist < maxDistance && dist < nearestDist)
                {
                    nearestPointIndex = i;
                    nearestDist = dist;
                }
            }
            return nearestPointIndex;
        }

        /// <summary>
        /// Pick the alpha control point, nearest to the specified position.
        /// </summary>
        /// <param name="maxDistance">Threshold for maximum distance. Points further away than this won't get picked.</param>
        private int PickAlphaControlPoint(Vector2 position, float maxDistance = 0.05f)
        {
            Vector2 distMultiplier = new Vector2(1.0f / zoomRect.width, 1.0f / zoomRect.height);
            TransferFunction tf = volRendObject.transferFunction;
            int nearestPointIndex = -1;
            float nearestDist = 1000.0f;
            for (int i = 0; i < tf.alphaControlPoints.Count; i++)
            {
                TFAlphaControlPoint ctrlPoint = tf.alphaControlPoints[i];
                Vector2 ctrlPos = new Vector2(ctrlPoint.dataValue, ctrlPoint.alphaValue);
                Vector2 distVec = (ctrlPos - position) * distMultiplier;
                float dist = distVec.magnitude;
                if (dist < maxDistance && dist < nearestDist)
                {
                    nearestPointIndex = i;
                    nearestDist = dist;
                }
            }
            return nearestPointIndex;
        }

        private Vector2 ApplyZoom(Vector2 position)
        {
            position.x = Mathf.Lerp(zoomRect.x, zoomRect.x + zoomRect.width, position.x);
            position.y = Mathf.Lerp(zoomRect.y, zoomRect.y + zoomRect.height, position.y);
            return position;
        }
        
        private Vector2 ApplyZoomInverse(Vector2 position)
        {
            position.x = InverseLerpUnclamped(zoomRect.x, zoomRect.x + zoomRect.width, position.x);
            position.y = InverseLerpUnclamped(zoomRect.y, zoomRect.y + zoomRect.height, position.y);
            return position;
        }

        private float InverseLerpUnclamped(float start, float end, float value)
        {
            return (value - start) / (end - start);
        }
    }
}
