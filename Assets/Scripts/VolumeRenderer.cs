using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class VolumeRenderer : MonoBehaviour
{
    public TransferFunction tf = null;

    private void Start()
    {
        FileStream fs = new FileStream("DataFiles//manix.dat", FileMode.Open);
        BinaryReader reader = new BinaryReader(fs);

        ushort dimX = reader.ReadUInt16();
        ushort dimY = reader.ReadUInt16();
        ushort dimZ = reader.ReadUInt16();

        reader.Close();
        fs.Close();

        Debug.Log(dimX + ", " + dimY + ", " + dimZ);

        int uDimension = dimX * dimY * dimZ;

        RawDatasetImporter importer = new RawDatasetImporter("DataFiles//manix.dat", dimX, dimY, dimZ, DataContentFormat.Int16);
        VolumeDataset dataset = importer.Import();

        Texture3D tex = dataset.texture;

        const int noiseDimX = 512;
        const int noiseDimY = 512;
        Texture2D noiseTexture = NoiseTextureGenerator.GenerateNoiseTexture(noiseDimX, noiseDimY);

        tf = new TransferFunction();
        tf.AddControlPoint(new TFColourControlPoint(0.0f, new Color(0.11f, 0.14f, 0.13f, 1.0f)));
        tf.AddControlPoint(new TFColourControlPoint(0.2415f, new Color(0.469f, 0.354f, 0.223f, 1.0f)));
        tf.AddControlPoint(new TFColourControlPoint(0.3253f, new Color(1.0f, 1.0f, 1.0f, 1.0f)));

        tf.AddControlPoint(new TFAlphaControlPoint(0.0f, 0.0f));
        tf.AddControlPoint(new TFAlphaControlPoint(0.1787f, 0.0f));
        tf.AddControlPoint(new TFAlphaControlPoint(0.2f, 0.024f));
        tf.AddControlPoint(new TFAlphaControlPoint(0.28f, 0.03f));
        tf.AddControlPoint(new TFAlphaControlPoint(0.4f, 0.546f));
        tf.AddControlPoint(new TFAlphaControlPoint(0.547f, 0.5266f));

        Texture2D tfTexture = tf.GetTexture();

        GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_DataTex", tex);
        GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_NoiseTex", noiseTexture);
        GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_TFTex", tfTexture);

        GetComponent<MeshRenderer>().sharedMaterial.EnableKeyword("MODE_DVR");
        GetComponent<MeshRenderer>().sharedMaterial.DisableKeyword("MODE_MIP");
        GetComponent<MeshRenderer>().sharedMaterial.DisableKeyword("MODE_SURF");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            GetComponent<MeshRenderer>().sharedMaterial.EnableKeyword("MODE_DVR");
            GetComponent<MeshRenderer>().sharedMaterial.DisableKeyword("MODE_MIP");
            GetComponent<MeshRenderer>().sharedMaterial.DisableKeyword("MODE_SURF");
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            GetComponent<MeshRenderer>().sharedMaterial.DisableKeyword("MODE_DVR");
            GetComponent<MeshRenderer>().sharedMaterial.EnableKeyword("MODE_MIP");
            GetComponent<MeshRenderer>().sharedMaterial.DisableKeyword("MODE_SURF");
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            GetComponent<MeshRenderer>().sharedMaterial.DisableKeyword("MODE_DVR");
            GetComponent<MeshRenderer>().sharedMaterial.DisableKeyword("MODE_MIP");
            GetComponent<MeshRenderer>().sharedMaterial.EnableKeyword("MODE_SURF");
        }
    }
}
