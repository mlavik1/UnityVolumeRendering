using System;
using System.IO;
using UnityEngine;
using openDicom.Registry;
using openDicom.File;
using openDicom.DataStructure.DataSet;
using openDicom.DataStructure;
using System.Collections.Generic;
using openDicom.Image;

namespace UnityVolumeRendering
{
    public class DICOMImporter : DatasetImporterBase
    {
        private class DICOMSliceFile
        {
            public AcrNemaFile file;
            public float location;
            public float intercept;
            public float slope;
        }

        private string diroctoryPath;
        private bool recursive;

        public DICOMImporter(string diroctoryPath, bool recursive)
        {
            this.diroctoryPath = diroctoryPath;
            this.recursive = recursive;
        }

        public override VolumeDataset Import()
        {
            DataElementDictionary dataElementDictionary = new DataElementDictionary();
            UidDictionary uidDictionary = new UidDictionary();
            try
            {
                dataElementDictionary.LoadFrom(Path.Combine(Application.streamingAssetsPath, "dicom-elements-2007.dic"), DictionaryFileFormat.BinaryFile);
                uidDictionary.LoadFrom(Path.Combine(Application.streamingAssetsPath, "dicom-uids-2007.dic"), DictionaryFileFormat.BinaryFile);
            }
            catch (Exception dictionaryException)
            {
                Debug.LogError("Problems processing dictionaries:\n" + dictionaryException);
                return null;
            }

            List<DICOMSliceFile> files = new List<DICOMSliceFile>();
            foreach (string filePath in Directory.EnumerateFiles(diroctoryPath, "*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
            {
                AcrNemaFile file = LoadFile(filePath);
                if(file != null && file.HasPixelData)
                {
                    DICOMSliceFile slice = new DICOMSliceFile();
                    slice.file = file;
                    DataElement elemLoc = file.DataSet[new Tag("(0020,1041)")];
                    DataElement elemIntercept = file.DataSet[new Tag("(0028,1052)")];
                    DataElement elemSlope = file.DataSet[new Tag("(0028,1053)")];
                    slice.location = (float)Convert.ToDouble(elemLoc.Value[0]);
                    slice.intercept = (float)Convert.ToDouble(elemIntercept.Value[0]);
                    slice.slope = (float)Convert.ToDouble(elemSlope.Value[0]);
                    files.Add(slice);
                }
            }
            files.Sort((DICOMSliceFile a, DICOMSliceFile b) => { return a.location.CompareTo(b.location); });

            Debug.Log($"Imported {files.Count} datasets");
            
            if(files.Count <= 1)
            {
                Debug.LogError("Insufficient number of slices.");
                return null;
            }

            float minLoc = (float)files[0].location;
            float maxLoc = (float)files[files.Count - 1].location;
            float locRange = maxLoc - minLoc;

            VolumeDataset dataset = new VolumeDataset();
            dataset.dimX = files[0].file.PixelData.Columns;
            dataset.dimY = files[0].file.PixelData.Rows;
            dataset.dimZ = files.Count;

            int dimension = dataset.dimX * dataset.dimY * dataset.dimZ;
            dataset.data = new int[dimension];

            for(int iSlice = 0; iSlice < files.Count; iSlice++)
            {
                DICOMSliceFile slice = files[iSlice];
                PixelData pixelData = slice.file.PixelData;
                int[] pixelArr = ToPixelArray(pixelData);
                if (pixelArr == null) // This should not happen
                    pixelArr = new int[pixelData.Rows * pixelData.Columns];
                
                for(int iRow = 0; iRow < pixelData.Rows; iRow++)
                {
                    for(int iCol = 0; iCol < pixelData.Columns; iCol++)
                    {
                        int pixelIndex = (iRow * pixelData.Columns) + iCol;
                        int dataIndex = (iSlice * pixelData.Columns * pixelData.Rows) + (iRow * pixelData.Columns) + iCol;

                        int pixelValue = pixelArr[pixelIndex];
                        float hounsfieldValue = pixelValue * slice.slope + slice.intercept;

                        dataset.data[dataIndex] = (int)Mathf.Clamp(hounsfieldValue, -1024.0f, 3071.0f);
                    }
                }
            }

            /*foreach (openDicom.DataStructure.DataSet.DataElement element in file.DataSet)
            {
                Debug.Log(file.DataSet.StreamPosition);
                Debug.Log(element.Tag.ToString() + " - " + element.VR.Tag.GetDictionaryEntry().Description);
            }*/

            /*
            dataset.dimX = file.DataSet.GetEnumerator;
            dataset.dimY = dimY;
            dataset.dimZ = dimZ;

            int uDimension = dimX * dimY * dimZ;
            dataset.data = new int[uDimension];*/

            return dataset;
        }

        private AcrNemaFile LoadFile(string filePath)
        {
            AcrNemaFile file = null;
            try
            {
                if (DicomFile.IsDicomFile(filePath))
                    file = new DicomFile(filePath, false);
                else if (AcrNemaFile.IsAcrNemaFile(filePath))
                    file = new AcrNemaFile(filePath, false);
                else
                    Debug.LogError("Selected file is neither a DICOM nor an ACR-NEMA file.");
            }
            catch (Exception dicomFileException)
            {
                Debug.LogError("Problems processing DICOM file:\n" + dicomFileException);
                return null;
            }
            return file;
        }

        private static int[] ToPixelArray(PixelData pixelData)
        {
            int[] intArray;
            if (pixelData.Data.Value.IsSequence)
            {
                Sequence sq = (Sequence)pixelData.Data.Value[0];
                intArray = new int[sq.Count];
                for (int i = 0; i < sq.Count; i++)
                    intArray[i] = Convert.ToInt32(sq[i].Value[0]);
                return intArray;
            }
            else if (pixelData.Data.Value.IsArray)
            {
                Array arr = (Array)pixelData.Data.Value[0];
                intArray = new int[arr.Length];
                for (int i = 0; i < arr.Length; i++)
                    intArray[i] = Convert.ToInt32(arr.GetValue(i));
                return intArray;
            }
            else
            {
                Debug.LogError("Pixel array is invalid");
                return null;
            }
        }
    }
}
