using System.IO;
using UnityEngine;
using System;
using UnityEditor;

namespace UnityVolumeRendering
{
    public class ImporterUtilsInternal
    {
        public static void ConvertLPSToUnityCoordinateSpace(VolumeDataset volumeDataset)
        {
            volumeDataset.scale = new Vector3(
                -volumeDataset.scale.x,
                volumeDataset.scale.y,
                volumeDataset.scale.z
            );
            volumeDataset.rotation = Quaternion.Euler(270.0f, 0.0f, 0.0f);
        }
    }
}
