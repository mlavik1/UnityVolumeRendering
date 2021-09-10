using System;
using System.IO;
using UnityEngine;

namespace UnityVolumeRendering
{
    [Serializable]
    public class VolumeDataset : ScriptableObject
    {
        [SerializeField]
        public string filePath;
        public int[] data;
        public double[] dataGrid;

        [SerializeField]
        public int dimX, dimY, dimZ;
        public int nx, ny, nz;
        [SerializeField]
        public float scaleX = 0.0f, scaleY = 0.0f, scaleZ = 0.0f;
        public float volumeScale;

        [SerializeField]
        public string datasetName;

        private int minDataValue = int.MaxValue;
        private int maxDataValue = int.MinValue;

        private double minDataValueDouble = double.MaxValue;
        private double maxDataValueDouble = double.MinValue;

        private Texture3D dataTexture = null;
        private Texture3D gradientTexture = null;

        private Texture3D dataTexturePar = null;
        private Texture3D gradientTexturePar = null;
        private string vasp;
        private string ext;
        public bool PARCHG;
        
        
        public Texture3D GetDataTexture()
        { 
            vasp = ".vasp";
            ext = Path.GetExtension(filePath);
            
            var equals = String.Equals(vasp,ext.ToString());
            if (equals)
            {
                PARCHG = true;
            }

            if (PARCHG)
            {
                dataTexturePar = CreateTextureInternalParChg();
                return dataTexturePar;
            }

            else if (!PARCHG)
            {
                dataTexture = CreateTextureInternal();
                return dataTexture;
            }
            return dataTexture;
        }
        /*
        public Texture3D GetDataTexturePar() //par chg
        {
            if (dataTexturePar == null)
            {
                dataTexturePar = CreateTextureInternalParChg();
            }
            return dataTexturePar;
        }*/

        public Texture3D GetGradientTexture()
        {
            if (PARCHG)
            {
                gradientTexturePar = CreateGradientTextureInternalParChg();
                return gradientTexturePar;
            }

            else if (!PARCHG)
            {
                gradientTexture = CreateGradientTextureInternal();
                return gradientTexture;
            }
            return null;
        }
        /*
        public Texture3D GetGradientTexturePar() //par chg
        {
            if (gradientTexturePar == null)
            {
                gradientTexturePar = CreateGradientTextureInternalParChg();
            }
            return gradientTexturePar;
        }
    */


        public int GetMinDataValue()
        {
            if (minDataValue == int.MaxValue)
                CalculateValueBounds();
            return minDataValue;
        }

        public double GetMinDataValueDouble() // par
        {
            if (minDataValueDouble == double.MaxValue)
                CalculateValueBounds();
            return minDataValueDouble;
        }

        public int GetMaxDataValue()
        {
            if (maxDataValue == int.MinValue)
                CalculateValueBounds();
            return maxDataValue;
        }

        public double GetMaxDataValueDouble() //par
        {
            if (maxDataValueDouble == double.MinValue)
                CalculateValueBounds();
            return maxDataValueDouble;
        }

        private void CalculateValueBounds() 
        {
            minDataValue = int.MaxValue;
            maxDataValue = int.MinValue;

            minDataValueDouble = double.MaxValue;
            maxDataValueDouble = double.MinValue;

            int dim = nx * ny * nz;

            for (int j = 0; j < dim; j++)
            {
                if (dataGrid != null) 
                {
                    double value = dataGrid[j];
                    minDataValueDouble = Math.Min(minDataValueDouble, value);
                    maxDataValueDouble = Math.Max(maxDataValueDouble, value);
                }
            }
            
            for (int i = 0; i < dimX * dimY * dimZ; i++)
            {
                if (data != null)
                {
                    int val = data[i];
                    minDataValue = Math.Min(minDataValue, val);
                     maxDataValue = Math.Max(maxDataValue, val);
                }
            }
            
        }

        private Texture3D CreateTextureInternal()
        {
            TextureFormat texformat = SystemInfo.SupportsTextureFormat(TextureFormat.RHalf) ? TextureFormat.RHalf : TextureFormat.RFloat;
            Texture3D texture = new Texture3D(dimX, dimY, dimZ, texformat, false);
            texture.wrapMode = TextureWrapMode.Clamp;

            int minValue = GetMinDataValue();
            int maxValue = GetMaxDataValue();

            int maxRange = maxValue - minValue;

            Color[] cols = new Color[data.Length]; // data exists
            for (int x = 0; x < dimX; x++)
            {
                for (int y = 0; y < dimY; y++)
                {
                    for (int z = 0; z < dimZ; z++)
                    {
                        int iData = x + y * dimX + z * (dimX * dimY);
                        cols[iData] = new Color((float)(data[iData] - minValue) / maxRange, 0.0f, 0.0f, 0.0f);
                    }
                }
            }
            
            texture.SetPixels(cols);
            texture.Apply();
            return texture;
        }

        private Texture3D CreateTextureInternalParChg() //par
        {
            TextureFormat texformat = SystemInfo.SupportsTextureFormat(TextureFormat.RHalf) ? TextureFormat.RHalf : TextureFormat.RFloat;
            Texture3D texture = new Texture3D(nx, ny, nz, texformat, false);
            texture.wrapMode = TextureWrapMode.Clamp;

            double minValue = GetMinDataValueDouble();
            double maxValue = GetMaxDataValueDouble();


            double maxRange  = maxValue - minValue;


            Color[] colors = new Color[dataGrid.Length]; // data exists
            for (int x = 0; x < dimX; x++)
            {
                for (int y = 0; y < dimY; y++)
                {
                    for (int z = 0; z < dimZ; z++)
                    {
                        int iData = x + y * dimX + z * (dimX * dimY);
                        colors[iData] = new Color((float)(dataGrid[iData] - (float)minValue) / (float)maxRange, 0.0f, 0.0f, 0.0f);
                    }
                }
            }  
            texture.SetPixels(colors);
            texture.Apply();
            return texture;
        }

        private Texture3D CreateGradientTextureInternal() 
        {
            TextureFormat texformat = SystemInfo.SupportsTextureFormat(TextureFormat.RGBAHalf) ? TextureFormat.RGBAHalf : TextureFormat.RGBAFloat;
            Texture3D texture = new Texture3D(dimX, dimY, dimZ, texformat, false);
            texture.wrapMode = TextureWrapMode.Clamp;

            int minValue = GetMinDataValue();
            int maxValue = GetMaxDataValue();
            int maxRange = maxValue - minValue;

            Color[] cols = new Color[data.Length];
            for (int x = 0; x < dimX; x++)
            {
                for (int y = 0; y < dimY; y++)
                {
                    for (int z = 0; z < dimZ; z++)
                    {
                        int iData = x + y * dimX + z * (dimX * dimY);

                        int x1 = data[Math.Min(x + 1, dimX - 1) + y * dimX + z * (dimX * dimY)] - minValue;
                        int x2 = data[Math.Max(x - 1, 0) + y * dimX + z * (dimX * dimY)] - minValue;
                        int y1 = data[x + Math.Min(y + 1, dimY - 1) * dimX + z * (dimX * dimY)] - minValue;
                        int y2 = data[x + Math.Max(y - 1, 0) * dimX + z * (dimX * dimY)] - minValue;
                        int z1 = data[x + y * dimX + Math.Min(z + 1, dimZ - 1) * (dimX * dimY)] - minValue;
                        int z2 = data[x + y * dimX + Math.Max(z - 1, 0) * (dimX * dimY)] - minValue;

                        Vector3 grad = new Vector3((x2 - x1) / (float)maxRange, (y2 - y1) / (float)maxRange, (z2 - z1) / (float)maxRange);

                        cols[iData] = new Color(grad.x, grad.y, grad.z, (float)(data[iData] - minValue) / maxRange);
                    }
                }
            }
            texture.SetPixels(cols);
            texture.Apply();
            return texture;
        }

        private Texture3D CreateGradientTextureInternalParChg() //par
        {
            TextureFormat texformat = SystemInfo.SupportsTextureFormat(TextureFormat.RGBAHalf) ? TextureFormat.RGBAHalf : TextureFormat.RGBAFloat;
            Texture3D texture = new Texture3D(dimX, dimY, dimZ, texformat, false);
            texture.wrapMode = TextureWrapMode.Clamp;

            double minValue = GetMinDataValueDouble();
            double maxValue = GetMinDataValueDouble();

            double maxRange  = maxValue - minValue;


            Color[] gradColors = new Color[dataGrid.Length];
            for (int x = 0; x < dimX; x++)
            {
                for (int y = 0; y < dimY; y++)
                {
                    for (int z = 0; z < dimZ; z++)
                    {
                        int iData = x + y * dimX + z * (dimX * dimY);

                        double x1 = dataGrid[Math.Min(x + 1, dimX - 1) + y * dimX + z * (dimX * dimY)] - minValue;
                        double x2 = dataGrid[Math.Max(x - 1, 0) + y * dimX + z * (dimX * dimY)] - minValue;
                        double y1 = dataGrid[x + Math.Min(y + 1, dimY - 1) * dimX + z * (dimX * dimY)] - minValue;
                        double y2 = dataGrid[x + Math.Max(y - 1, 0) * dimX + z * (dimX * dimY)] - minValue;
                        double z1 = dataGrid[x + y * dimX + Math.Min(z + 1, dimZ - 1) * (dimX * dimY)] - minValue;
                        double z2 = dataGrid[x + y * dimX + Math.Max(z - 1, 0) * (dimX * dimY)] - minValue;

                        Vector3 grad = new Vector3((float)(x2 - x1) / (float)maxRange, (float)(y2 - y1) / (float)maxRange, (float)(z2 - z1) / (float)maxRange);

                        gradColors[iData] = new Color(grad.x, grad.y, grad.z, (float)(dataGrid[iData] - (float)minValue) / (float)maxRange);
                    }
                }
            }
            texture.SetPixels(gradColors);
            texture.Apply();
            return texture;
        }
    }
}
