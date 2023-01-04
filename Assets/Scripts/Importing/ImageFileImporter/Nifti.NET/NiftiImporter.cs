using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using Nifti.NET;

namespace UnityVolumeRendering
{
    /// <summary>
    /// SimpleITK-based DICOM importer.
    /// </summary>
    public class NiftiImporter : IImageFileImporter
    {
        public VolumeDataset Import(string filePath)
        {
            Nifti.NET.Nifti niftiFile = NiftiFile.Read(filePath);
            if (niftiFile == null)
            {
                Debug.LogError("Failed to read NIFTI dataset");
                return null;
            }
            int numDimensions = niftiFile.Header.dim[0];
            if (numDimensions > 3)
            {
                Debug.LogError($"Unsupported dimension. Expected 3-dimensional dataset, but got {numDimensions}.");
                return null;
            }
            int dimX = niftiFile.Header.dim[1];
            int dimY = niftiFile.Header.dim[2];
            int dimZ = niftiFile.Header.dim[3];
            float[] pixelData = niftiFile.ToSingleArray();

            Vector3 pixdim = new Vector3(niftiFile.Header.pixdim[1], niftiFile.Header.pixdim[2], niftiFile.Header.pixdim[3]);
            Vector3 size = new Vector3(dimX * pixdim.x, dimY * pixdim.y, dimZ * pixdim.z);

            // Create dataset
            VolumeDataset volumeDataset = new VolumeDataset();
            volumeDataset.data = pixelData;
            volumeDataset.dimX = dimX;
            volumeDataset.dimY = dimY;
            volumeDataset.dimZ = dimZ;
            volumeDataset.datasetName = "test";
            volumeDataset.filePath = filePath;
            volumeDataset.scaleX = size.x;
            volumeDataset.scaleY = size.y;
            volumeDataset.scaleZ = size.z;

            volumeDataset.FixDimensions();

            return volumeDataset;
        }
    }
}
