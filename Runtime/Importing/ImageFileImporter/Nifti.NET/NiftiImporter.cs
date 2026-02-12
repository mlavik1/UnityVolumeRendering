using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using Nifti.NET;
using System.Threading.Tasks;

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
           
            // Create dataset
            VolumeDataset volumeDataset = ScriptableObject.CreateInstance<VolumeDataset>();
            bool succeeded = ImportInternal(volumeDataset, niftiFile, filePath);

            if (!succeeded)
                volumeDataset = null;

            return volumeDataset;
        }

        public async Task<VolumeDataset> ImportAsync(string filePath)
        {
            Nifti.NET.Nifti niftiFile = null;
            VolumeDataset volumeDataset = ScriptableObject.CreateInstance<VolumeDataset>();

            await Task.Run(() =>niftiFile = NiftiFile.Read(filePath));

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

            bool succeeded = await Task.Run(() => ImportInternal(volumeDataset,niftiFile,filePath));

            if (!succeeded)
                volumeDataset = null;

            return volumeDataset;
        }
        private bool ImportInternal(VolumeDataset volumeDataset,Nifti.NET.Nifti niftiFile,string filePath)
        {
            int dimX = niftiFile.Header.dim[1];
            int dimY = niftiFile.Header.dim[2];
            int dimZ = niftiFile.Header.dim[3];
            float[] pixelData = niftiFile.ToSingleArray();

            if (pixelData == null)
            {
                Debug.LogError($"Failed to read data, of type: {niftiFile.Data?.GetType()}");
                return false;
            }

            Vector3 pixdim = new Vector3(niftiFile.Header.pixdim[1], niftiFile.Header.pixdim[2], niftiFile.Header.pixdim[3]);
            Vector3 size = new Vector3(dimX * pixdim.x, dimY * pixdim.y, dimZ * pixdim.z);

            // Create dataset
            volumeDataset.data = pixelData;
            volumeDataset.dimX = dimX;
            volumeDataset.dimY = dimY;
            volumeDataset.dimZ = dimZ;
            volumeDataset.datasetName = Path.GetFileName(filePath);
            volumeDataset.filePath = filePath;
            volumeDataset.scale = size;

            volumeDataset.FixDimensions();
            volumeDataset.rotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);

            return true;
        }
    }
}
