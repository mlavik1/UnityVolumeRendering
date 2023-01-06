using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Collections;

namespace UnityVolumeRendering
{
    public class TimeSeriesManager : MonoBehaviour
    { 
        private static int dimX = 256;
        private static int dimY = 256;
        private static int dimZ = 256;
        private static DataContentFormat dataFormat = DataContentFormat.Uint8;
        private static Endianness endianness = Endianness.LittleEndian;
        private static int bytesToSkip = 0;
        [SerializeField]
        private VolumeRenderedObject volumeRenderedObject;
        [SerializeField]
        private VolumeDataset dataset;
        private float accumulatedTime = 0.0f;
        private float framesPerSecond = 5.0f;
        private int lastCounter = 0;
        private FileStream fileStream;
        private BinaryReader reader;

        [SerializeField]
        private string serialisedDatasetPath;

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Volume Rendering/Load dataset/Load raw time series")]
        private static void ImportTimeSeries()
        {
            string directory = UnityEditor.EditorUtility.OpenFolderPanel("Select a folder", "DataFiles", "");
            if (Directory.Exists(directory))
            {
                IEnumerable<string> fileCandidates = Directory.EnumerateFiles(directory, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(p => p.EndsWith(".raw", StringComparison.InvariantCultureIgnoreCase)).OrderBy(f => f);

                string serialisedFilePath = UnityEditor.EditorUtility.SaveFilePanel("Select a file path for your serialised dataset (DO NOT USE A PATH INSIDE THE UNITY PROJECT)", "", "dataset", ".bin");
                FileStream serialisedFile = File.Create(serialisedFilePath);
                BinaryWriter serialisedFileWriter = new BinaryWriter(serialisedFile);

                VolumeDataset firstDataset = null;
                foreach (String file in fileCandidates)
                {
                    RawDatasetImporter importer = new RawDatasetImporter(file, dimX, dimY, dimZ, dataFormat, endianness, bytesToSkip);
                    VolumeDataset dataset = importer.Import();
                    if (dataset)
                    {
                        if (firstDataset == null)
                            firstDataset = dataset;
                        
                        float minValue = dataset.GetMinDataValue();
                        float maxValue = dataset.GetMaxDataValue();
                        float maxRange = maxValue - minValue;

                        serialisedFileWriter.Write(dataset.data.Length);
                        foreach (float value in dataset.data)
                            serialisedFileWriter.Write((float)(value - minValue) / maxRange);
                    }
                }

                if (firstDataset)
                {
                    VolumeRenderedObject volObj = VolumeObjectFactory.CreateObject(firstDataset);
                    TimeSeriesManager timeSeriesManager = volObj.gameObject.AddComponent<TimeSeriesManager>();
                    timeSeriesManager.volumeRenderedObject = volObj;
                    timeSeriesManager.dataset = firstDataset;
                    timeSeriesManager.serialisedDatasetPath = serialisedFilePath;
                }
            }
        }
#endif

        private void Start()
        {
            // Create dataset texture first
            dataset.GetDataTexture();

            fileStream = new FileStream(serialisedDatasetPath, FileMode.Open);
            reader = new BinaryReader(fileStream);
        }

        private void Update()
        {
            accumulatedTime += Time.deltaTime;
            int counter = (int)(accumulatedTime * framesPerSecond);
            int frames = counter - lastCounter;
            while(frames > 0)
            {
                int length = reader.ReadInt32();
                if (frames ==  1)
                {
                    byte[] bytes = reader.ReadBytes(length * 4);
                    Texture3D texture = dataset.GetDataTexture();
                    texture.SetPixelData(bytes, 0);
                    texture.Apply();
                }
                else
                    reader.BaseStream.Position += length * 4;
                if (reader.BaseStream.Position >= reader.BaseStream.Length - 1)
                    reader.BaseStream.Position = 0;
                frames--;
            }
            volumeRenderedObject.dataset = dataset;
            volumeRenderedObject.meshRenderer.sharedMaterial.SetTexture("_DataTex", dataset.GetDataTexture());

            lastCounter = counter;
        }
    }
}
