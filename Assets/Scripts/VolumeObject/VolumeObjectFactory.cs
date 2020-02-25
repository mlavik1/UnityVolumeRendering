using System;
using UnityEngine;

namespace UnityVolumeRendering
{
    public class VolumeObjectFactory
    {
        public static VolumeRenderedObject CreateObject(VolumeDataset dataset)
        {
            GameObject obj = GameObject.Instantiate((GameObject)Resources.Load("VolumeRenderedObject"));
            VolumeRenderedObject volObj = obj.GetComponent<VolumeRenderedObject>();
            MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();
            meshRenderer.material = new Material(meshRenderer.sharedMaterial);

            volObj.dataset = dataset;

            int dimX = dataset.dimX;
            int dimY = dataset.dimY;
            int dimZ = dataset.dimZ;

            int minValue = dataset.GetMinDataValue();
            int maxValue = dataset.GetMaxDataValue();
            int maxRange = maxValue - minValue;

            Color[] cols = new Color[dataset.data.Length];
            for (int x = 0; x < dataset.dimX; x++)
            {
                for (int y = 0; y < dataset.dimY; y++)
                {
                    for (int z = 0; z < dataset.dimZ; z++)
                    {
                        int iData = x + y * dimX + z * (dimX * dimY);

                        int x1 = dataset.data[Math.Min(x + 1, dimX - 1) + y * dataset.dimX + z * (dataset.dimX * dataset.dimY)] - minValue;
                        int x2 = dataset.data[Math.Max(x - 1, 0) + y * dataset.dimX + z * (dataset.dimX * dataset.dimY)] - minValue;
                        int y1 = dataset.data[x + Math.Min(y + 1, dimY - 1) * dataset.dimX + z * (dataset.dimX * dataset.dimY)] - minValue;
                        int y2 = dataset.data[x + Math.Max(y - 1, 0) * dataset.dimX + z * (dataset.dimX * dataset.dimY)] - minValue;
                        int z1 = dataset.data[x + y * dataset.dimX + Math.Min(z + 1, dimZ - 1) * (dataset.dimX * dataset.dimY)] - minValue;
                        int z2 = dataset.data[x + y * dataset.dimX + Math.Max(z - 1, 0) * (dataset.dimX * dataset.dimY)] - minValue;

                        Vector3 grad = new Vector3((x2 - x1) / (float)maxRange, (y2 - y1) / (float)maxRange, (z2 - z1) / (float)maxRange);

                        cols[iData] = new Color(grad.x, grad.y, grad.z, (float)(dataset.data[iData] - minValue) / maxRange);
                    }
                }
            }

            Texture3D tex = dataset.GetTexture();
            tex.SetPixels(cols);
            tex.Apply();

            const int noiseDimX = 512;
            const int noiseDimY = 512;
            Texture2D noiseTexture = NoiseTextureGenerator.GenerateNoiseTexture(noiseDimX, noiseDimY);

            TransferFunction tf = TransferFunctionDatabase.CreateTransferFunction();
            Texture2D tfTexture = tf.GetTexture();
            volObj.transferFunction = tf;

            tf.histogramTexture = HistogramTextureGenerator.GenerateHistogramTexture(dataset);

            TransferFunction2D tf2D = TransferFunctionDatabase.CreateTransferFunction2D();
            volObj.transferFunction2D = tf2D;

            meshRenderer.sharedMaterial.SetTexture("_DataTex", tex);
            meshRenderer.sharedMaterial.SetTexture("_NoiseTex", noiseTexture);
            meshRenderer.sharedMaterial.SetTexture("_TFTex", tfTexture);

            meshRenderer.sharedMaterial.EnableKeyword("MODE_DVR");
            meshRenderer.sharedMaterial.DisableKeyword("MODE_MIP");
            meshRenderer.sharedMaterial.DisableKeyword("MODE_SURF");

            return volObj;
        }
    }
}
