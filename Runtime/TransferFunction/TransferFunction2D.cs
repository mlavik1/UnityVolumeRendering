using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityVolumeRendering
{
    [Serializable]
    public class TransferFunction2D : ScriptableObject
    {
        [System.Serializable]
        public struct TF2DBox
        {
            public Color colour;
            public float alpha;
            public float minAlpha;
            public Rect rect;
        }

        [SerializeField]
        public List<TF2DBox> boxes = new List<TF2DBox>();

        private Texture2D texture = null;

        private const int TEXTURE_WIDTH = 512;
        private const int TEXTURE_HEIGHT = 512;

        public void AddBox(float x, float y, float width, float height, Color colour, float alpha)
        {
            TF2DBox box = new TF2DBox();
            box.rect.x = x;
            box.rect.y = y;
            box.rect.width = width;
            box.rect.height = height;
            box.colour = colour;
            box.alpha = alpha;
            boxes.Add(box);
        }

        public Texture2D GetTexture()
        {
            if(texture == null)
                GenerateTexture();

            return texture;
        }

        private void CreateTexture()
        {
            TextureFormat texformat = SystemInfo.SupportsTextureFormat(TextureFormat.RGBAHalf) ? TextureFormat.RGBAHalf : TextureFormat.RGBAFloat;
            texture = new Texture2D(TEXTURE_WIDTH, TEXTURE_HEIGHT, texformat, false);
        }

        public void GenerateTexture()
        {
            if (texture == null)
                CreateTexture();

            Color[] cols = new Color[TEXTURE_WIDTH * TEXTURE_HEIGHT];
            for (int iX = 0; iX < TEXTURE_WIDTH; iX++)
            {
                for (int iY = 0; iY < TEXTURE_WIDTH; iY++)
                {
                    cols[iX + iY * TEXTURE_WIDTH] = Color.clear; // TODO
                    foreach (TF2DBox box in boxes)
                    {
                        if (box.rect.Contains(new Vector2(iX / (float)TEXTURE_WIDTH, iY / (float)TEXTURE_HEIGHT)))
                        {
                            float x = iX / (float)TEXTURE_WIDTH;
                            float alpha = Mathf.Lerp(box.alpha, box.minAlpha, Mathf.Abs(box.rect.x + box.rect.width * 0.5f - x) * 2.0f);
                            cols[iX + iY * TEXTURE_WIDTH] = new Color(box.colour.r, box.colour.g, box.colour.b, alpha);
                            // TODO: combine with other overlapping boxes
                        }
                        //cols[iX + iY * TEXTURE_WIDTH] = new Color(iX / (float)TEXTURE_WIDTH, iY / (float)TEXTURE_HEIGHT, 0.0f, 1.0f);
                    }
                }
            }
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixels(cols);
            texture.Apply();
        }
    }

}
