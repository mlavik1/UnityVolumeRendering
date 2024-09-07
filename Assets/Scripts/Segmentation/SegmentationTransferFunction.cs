using UnityEngine;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices.WindowsRuntime;

namespace UnityVolumeRendering
{
    [Serializable]
    public class SegmentationTransferFunction : TransferFunction
    {
        private Texture2D texture = null;
        private Color[] textureColours;
        List<SegmentationLabel> segmentationLabels = new List<SegmentationLabel>();

        private const int TEXTURE_WIDTH = 1024;

        public void SetSegmentationLabels(List<SegmentationLabel> labels)
        {
            this.segmentationLabels = labels;
            GenerateTexture();
        }

        public override void AddControlPoint(TFColourControlPoint ctrlPoint)
        {
        }

        public override void AddControlPoint(TFAlphaControlPoint ctrlPoint)
        {
        }

        public override Texture2D GetTexture()
        {
            if (texture == null)
                GenerateTexture();

            return texture;
        }

        public override void GenerateTexture()
        {

            textureColours = new Color[TEXTURE_WIDTH * segmentationLabels.Count];

            for (int iSegmentation = 0; iSegmentation < segmentationLabels.Count; iSegmentation++)
            {
                SegmentationLabel segmentationLabel = segmentationLabels[iSegmentation];

                if (segmentationLabel.transferFunction == null)
                {
                    for (int i = 0; i < TEXTURE_WIDTH; i++)
                    {
                        textureColours[i + iSegmentation * TEXTURE_WIDTH] = segmentationLabel.colour;
                    }
                }
                else
                {
                    TransferFunction tf = segmentationLabel.transferFunction;
                    List<TFColourControlPoint> cols = new List<TFColourControlPoint>(tf.colourControlPoints);
                    List<TFAlphaControlPoint> alphas = new List<TFAlphaControlPoint>(tf.alphaControlPoints);

                    // Sort lists of control points
                    cols.Sort((a, b) => (a.dataValue.CompareTo(b.dataValue)));
                    alphas.Sort((a, b) => (a.dataValue.CompareTo(b.dataValue)));

                    // Add colour points at beginning and end
                    if (cols.Count == 0 || cols[cols.Count - 1].dataValue < 1.0f)
                        cols.Add(new TFColourControlPoint(1.0f, Color.white));
                    if (cols[0].dataValue > 0.0f)
                        cols.Insert(0, new TFColourControlPoint(0.0f, Color.white));

                    // Add alpha points at beginning and end
                    if (alphas.Count == 0 || alphas[alphas.Count - 1].dataValue < 1.0f)
                        alphas.Add(new TFAlphaControlPoint(1.0f, 1.0f));
                    if (alphas[0].dataValue > 0.0f)
                        alphas.Insert(0, new TFAlphaControlPoint(0.0f, 0.0f));

                    int numColours = cols.Count;
                    int numAlphas = alphas.Count;
                    int iCurrColour = 0;
                    int iCurrAlpha = 0;

                    for (int iX = 0; iX < TEXTURE_WIDTH; iX++)
                    {
                        float t = iX / (float)(TEXTURE_WIDTH - 1);
                        while (iCurrColour < numColours - 2 && cols[iCurrColour + 1].dataValue < t)
                            iCurrColour++;
                        while (iCurrAlpha < numAlphas - 2 && alphas[iCurrAlpha + 1].dataValue < t)
                            iCurrAlpha++;

                        TFColourControlPoint leftCol = cols[iCurrColour];
                        TFColourControlPoint rightCol = cols[iCurrColour + 1];
                        TFAlphaControlPoint leftAlpha = alphas[iCurrAlpha];
                        TFAlphaControlPoint rightAlpha = alphas[iCurrAlpha + 1];

                        float tCol = (Mathf.Clamp(t, leftCol.dataValue, rightCol.dataValue) - leftCol.dataValue) / (rightCol.dataValue - leftCol.dataValue);
                        float tAlpha = (Mathf.Clamp(t, leftAlpha.dataValue, rightAlpha.dataValue) - leftAlpha.dataValue) / (rightAlpha.dataValue - leftAlpha.dataValue);

                        tCol = Mathf.SmoothStep(0.0f, 1.0f, tCol);
                        tAlpha = Mathf.SmoothStep(0.0f, 1.0f, tAlpha);

                        Color pixCol = rightCol.colourValue * tCol + leftCol.colourValue * (1.0f - tCol);
                        pixCol.a = rightAlpha.alphaValue * tAlpha + leftAlpha.alphaValue * (1.0f - tAlpha);

                        textureColours[iX + iSegmentation * TEXTURE_WIDTH] = QualitySettings.activeColorSpace == ColorSpace.Linear ? pixCol.linear : pixCol;
                    }
                }
            }
            if (texture == null || texture.height != segmentationLabels.Count)
            {
                TextureFormat texformat = SystemInfo.SupportsTextureFormat(TextureFormat.RGBAHalf) ? TextureFormat.RGBAHalf : TextureFormat.RGBAFloat;
                texture = new Texture2D(TEXTURE_WIDTH, segmentationLabels.Count, texformat, false);
                texture.wrapMode = TextureWrapMode.Clamp;
                texture.filterMode = FilterMode.Point;
            }
            texture.SetPixels(textureColours);
            texture.Apply();
        }

        public override Color GetColour(float x)
        {
            return Color.black;
        }
    }
}
