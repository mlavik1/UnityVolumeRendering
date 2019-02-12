using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class VolumeRenderer : MonoBehaviour
{
    private void Start()
    {
        FileStream fs = new FileStream("DataFiles//manix.dat", FileMode.Open);
        BinaryReader reader = new BinaryReader(fs);

        ushort dimX = reader.ReadUInt16();
        ushort dimY = reader.ReadUInt16();
        ushort dimZ = reader.ReadUInt16();

        Debug.Log(dimX + ", " + dimY + ", " + dimZ);

        int uDimension = dimX * dimY * dimZ;

        Texture3D tex = new Texture3D(dimX, dimY, dimZ, TextureFormat.RGBAFloat, false);
        Color[] cols = new Color[uDimension];
        float minVal = float.PositiveInfinity;
        float maxVal = float.NegativeInfinity;
        for(int i = 0; i < uDimension; i++)
        {
            float val = (float)reader.ReadInt16();
            minVal = Mathf.Min(minVal, val);
            maxVal = Mathf.Max(maxVal, val);
            cols[i] = new Color(val, 0.0f, 0.0f);
        }
        Debug.Log(minVal + "  -  " + maxVal);

        tex.SetPixels(cols);
        tex.Apply();

        const int noiseDimX = 512;
        const int noiseDimY = 512;

        Texture2D noiseTexture = new Texture2D(noiseDimX, noiseDimY);
        Color[] noiseCols = new Color[noiseDimX * noiseDimY];
        for(int iY = 0; iY < noiseDimY; iY++)
        {
            for (int iX = 0; iX < noiseDimX; iX++)
            {
                float pixVal = Random.Range(0.0f, 1.0f);
                noiseCols[iX + iY * noiseDimX] = new Color(pixVal, pixVal, pixVal);
            }
        }
        
        TransferFunction tf = new TransferFunction();
        tf.AddControlPoint(new TFColourControlPoint(0.0f, Color.black));
        tf.AddControlPoint(new TFColourControlPoint(0.055f, Color.yellow));
        tf.AddControlPoint(new TFColourControlPoint(0.11f, Color.red));
        tf.AddControlPoint(new TFColourControlPoint(1.0f, Color.white));

        tf.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
        tf.AddControlPoint(new TFAlphaControlPoint(0.01f, 0.0f));
        tf.AddControlPoint(new TFAlphaControlPoint(0.6f, 0.2f));
        tf.AddControlPoint(new TFAlphaControlPoint(1.0f, 1.0f));

        Texture2D tfTexture = tf.GetTexture();


        GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_DataTex", tex);
        GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_NoiseTex", noiseTexture);
        GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_TFTex", tfTexture);

    }
}
