using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        private List<VolumeDataset> datasets;
        private float accumulatedTime = 0.0f;
        private float framesPerSecond = 5.0f;

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Volume Rendering/Load dataset/Load raw time series")]
        private static void ImportTimeSeries()
        {
            string directory = UnityEditor.EditorUtility.OpenFolderPanel("Select a folder", "DataFiles", "");
            if (Directory.Exists(directory))
            {
                IEnumerable<string> fileCandidates = Directory.EnumerateFiles(directory, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(p => p.EndsWith(".raw", StringComparison.InvariantCultureIgnoreCase)).OrderBy(f => f);

                List<VolumeDataset> datasets = new List<VolumeDataset>();
                foreach (String file in fileCandidates)
                {
                    RawDatasetImporter importer = new RawDatasetImporter(file, dimX, dimY, dimZ, dataFormat, endianness, bytesToSkip);
                    VolumeDataset dataset = importer.Import();
                    if (dataset != null)
                        datasets.Add(dataset);
                }

                if (datasets.Count > 0)
                {
                    VolumeRenderedObject volObj = VolumeObjectFactory.CreateObject(datasets[0]);
                    TimeSeriesManager timeSeriesManager = volObj.gameObject.AddComponent<TimeSeriesManager>();
                    timeSeriesManager.volumeRenderedObject = volObj;
                    timeSeriesManager.datasets = datasets;
                }
            }
        }
#endif

        private void Start()
        {
            // Create all dataset textures first
            foreach (VolumeDataset dataset in datasets)
                dataset.GetDataTexture();
        }

        private void Update()
        {
            accumulatedTime += Time.deltaTime;
            int index = (int)(accumulatedTime * framesPerSecond) % datasets.Count;
            VolumeDataset dataset = datasets[index];
            volumeRenderedObject.dataset = dataset;
            volumeRenderedObject.meshRenderer.sharedMaterial.SetTexture("_DataTex", dataset.GetDataTexture());
        }
    }
}
