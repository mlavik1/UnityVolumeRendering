using UnityEngine;
using System.Collections.Generic;
using System;

namespace UnityVolumeRendering
{
    [Serializable]
    public class TransferFunctionInstance : ScriptableObject
    {
        [SerializeField]
        public TransferFunction transferFunction;

        public VolumeDataset dataset;

        private Texture2D texture = null;
        private Color[] tfCols;

        private const int TEXTURE_WIDTH = 512;
        private const int TEXTURE_HEIGHT = 2;

        public void Initialise(TransferFunction tf, VolumeDataset dataset)
        {
            this.transferFunction = tf;
            this.dataset = dataset;
            EnsureAbsoluteScale();
        }

        public void AddControlPoint(TFColourControlPoint ctrlPoint)
        {
            transferFunction.AddControlPoint(ctrlPoint);
        }

        public void AddControlPoint(TFAlphaControlPoint ctrlPoint)
        {
            transferFunction.AddControlPoint(ctrlPoint);
        }

        public Texture2D GetTexture()
        {
            if (texture == null)
                GenerateTexture();

            return texture;
        }

        public void EnsureAbsoluteScale()
        {
            if (transferFunction.relativeScale)
            {
                float minValue = dataset.GetMinDataValue();
                float maxValue = dataset.GetMaxDataValue();
                for (int i = 0; i < transferFunction.colourControlPoints.Count; i++)
                {
                    TFColourControlPoint point = transferFunction.colourControlPoints[i];
                    point.dataValue = Mathf.InverseLerp(minValue, maxValue, point.dataValue);
                    transferFunction.colourControlPoints[i] = point;
                }
                for (int i = 0; i < transferFunction.alphaControlPoints.Count; i++)
                {
                    TFAlphaControlPoint point = transferFunction.alphaControlPoints[i];
                    point.dataValue = Mathf.InverseLerp(minValue, maxValue, point.dataValue);
                    transferFunction.alphaControlPoints[i] = point;
                }
                transferFunction.relativeScale = false;
            }
        }

        public void GenerateTexture()
        {
            if (texture == null)
                CreateTexture();

            List<TFColourControlPoint> cols = new List<TFColourControlPoint>(transferFunction.colourControlPoints);
            List<TFAlphaControlPoint> alphas = new List<TFAlphaControlPoint>(transferFunction.alphaControlPoints);

            if (transferFunction.relativeScale)
            {
                float minValue = dataset.GetMinDataValue();
                float maxValue = dataset.GetMaxDataValue();
                for (int i = 0; i < cols.Count; i++)
                {
                    TFColourControlPoint point = cols[i];
                    point.dataValue = (Mathf.Clamp(point.dataValue, minValue, maxValue) + minValue) / (maxValue - minValue);
                    cols[i] = point;
                }
                for (int i = 0; i < alphas.Count; i++)
                {
                    TFAlphaControlPoint point = alphas[i];
                    point.dataValue = (Mathf.Clamp(point.dataValue, minValue, maxValue) + minValue) / (maxValue - minValue);
                    alphas[i] = point;
                }
            }

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

                for (int iY = 0; iY < TEXTURE_HEIGHT; iY++)
                {
                    tfCols[iX + iY * TEXTURE_WIDTH] = QualitySettings.activeColorSpace == ColorSpace.Linear ? pixCol.linear : pixCol;
                }
            }

            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixels(tfCols);
            texture.Apply();
        }

        public Color GetColour(float x)
        {
            int index = Mathf.RoundToInt(x * TEXTURE_WIDTH);
            return tfCols[index];
        }

        private void CreateTexture()
        {
            TextureFormat texformat = SystemInfo.SupportsTextureFormat(TextureFormat.RGBAHalf) ? TextureFormat.RGBAHalf : TextureFormat.RGBAFloat;
            texture = new Texture2D(TEXTURE_WIDTH, TEXTURE_HEIGHT, texformat, false);
            tfCols = new Color[TEXTURE_WIDTH * TEXTURE_HEIGHT];
        }
    }
}
