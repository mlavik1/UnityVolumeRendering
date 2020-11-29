using System;
using System.IO;
using UnityEngine;
using openDicom.Registry;
using openDicom.File;
using openDicom.DataStructure.DataSet;
using openDicom.DataStructure;
using System.Collections.Generic;
using openDicom.Image;
using System.Linq;
using System.Runtime.InteropServices;

namespace UnityVolumeRendering
{
    /// <summary>
    /// DICOM importer.
    /// Reads a 3D DICOM dataset from a folder.
    /// The folder needs to contain several .dcm/.dicom files, where each file is a slice of the same dataset.
    /// </summary>
    public class DICOMImporter : DatasetImporterBase
    {
        private class DICOMSliceFile
        {
            public AcrNemaFile file;
            public float location = 0;
            public Vector3 position = Vector3.zero;
            public float intercept = 0.0f;
            public float slope = 1.0f;
            public float pixelSpacing = 0.0f;
            public bool missingLocation = false;
        }

        private IEnumerable<string> fileCandidates;
        private string datasetName;

        public DICOMImporter(IEnumerable<string> files, string name = "DICOM_Dataset")
        {
            this.fileCandidates = files;
            datasetName = name;
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
            bool needsCalcLoc = false;
            foreach (string filePath in fileCandidates)
            {
                DICOMSliceFile sliceFile = ReadDICOMFile(filePath);
                if(sliceFile != null)
                {
                    needsCalcLoc |= sliceFile.missingLocation;
                    files.Add(sliceFile);
                }
            }

            // Calculate slice location from "Image Position" (0020,0032)
            if (needsCalcLoc)
                CalcSliceLocFromPos(files);

            // Sort files by slice location
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

            // Create dataset
            VolumeDataset dataset = new VolumeDataset();
            dataset.datasetName = Path.GetFileName(datasetName);
            dataset.dimX = files[0].file.PixelData.Columns;
            dataset.dimY = files[0].file.PixelData.Rows;
            dataset.dimZ = files.Count;

            int dimension = dataset.dimX * dataset.dimY * dataset.dimZ;
            dataset.data = new int[dimension];

            for (int iSlice = 0; iSlice < files.Count; iSlice++)
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

            if (files[0].pixelSpacing > 0.0f)
            {
                dataset.scaleX = files[0].pixelSpacing * dataset.dimX;
                dataset.scaleY = files[0].pixelSpacing * dataset.dimY;
                dataset.scaleZ = Mathf.Abs(files[files.Count - 1].location - files[0].location);
            }

            return dataset;
        }

        private DICOMSliceFile ReadDICOMFile(string filePath)
        {
            AcrNemaFile file = LoadFile(filePath);

            if (file != null && file.HasPixelData)
            {
                DICOMSliceFile slice = new DICOMSliceFile();
                slice.file = file;
                
                Tag locTag = new Tag("(0020,1041)");
                Tag posTag = new Tag("(0020,0032)");
                Tag interceptTag = new Tag("(0028,1052)");
                Tag slopeTag = new Tag("(0028,1053)");
                Tag pixelSpacingTag = new Tag("(0028,0030)");

                // Read location (optional)
                if (file.DataSet.Contains(locTag))
                {
                    DataElement elemLoc = file.DataSet[locTag];
                    slice.location = (float)Convert.ToDouble(elemLoc.Value[0]);
                }
                // If no location tag, read position tag (will need to calculate location afterwards)
                else if (file.DataSet.Contains(posTag))
                {
                    DataElement elemLoc = file.DataSet[posTag];
                    Vector3 pos = Vector3.zero;
                    pos.x = (float)Convert.ToDouble(elemLoc.Value[0]);
                    pos.y = (float)Convert.ToDouble(elemLoc.Value[1]);
                    pos.z = (float)Convert.ToDouble(elemLoc.Value[2]);
                    slice.position = pos;
                    slice.missingLocation = true;
                }
                else
                {
                    Debug.LogError($"Missing location/position tag in file: {filePath}.\n The file will not be imported");
                    return null;
                }
                
                // Read intercept
                if (file.DataSet.Contains(interceptTag))
                {
                    DataElement elemIntercept = file.DataSet[interceptTag];
                    slice.intercept = (float)Convert.ToDouble(elemIntercept.Value[0]);
                }
                else
                    Debug.LogWarning($"The file {filePath} is missing the intercept element. As a result, the default transfer function might not look good.");
                
                // Read slope
                if (file.DataSet.Contains(slopeTag))
                {
                    DataElement elemSlope = file.DataSet[slopeTag];
                    slice.slope = (float)Convert.ToDouble(elemSlope.Value[0]);
                }
                else
                    Debug.LogWarning($"The file {filePath} is missing the intercept element. As a result, the default transfer function might not look good.");
                
                // Read pixel spacing
                if (file.DataSet.Contains(pixelSpacingTag))
                {
                    DataElement elemPixelSpacing = file.DataSet[pixelSpacingTag];
                    slice.pixelSpacing = (float)Convert.ToDouble(elemPixelSpacing.Value[0]);
                }

                return slice;
            }
            return null;
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
                Debug.LogError($"Problems processing the DICOM file {filePath} :\n {dicomFileException}");
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
                byte[][] bytesArray = pixelData.ToBytesArray();
                if (bytesArray != null && bytesArray.Length > 0)
                {
                    byte[] bytes = bytesArray[0];

                    int cellSize = pixelData.BitsAllocated / 8;
                    int pixelCount = bytes.Length / cellSize;

                    intArray = new int[pixelCount];
                    int pixelIndex = 0;

                    // Byte array for a single cell/pixel value
                    byte[] cellData = new byte[cellSize];
                    for(int iByte = 0; iByte < bytes.Length; iByte++)
                    {
                        // Collect bytes for one cell (sample)
                        int index = iByte % cellSize;
                        cellData[index] = bytes[iByte];
                        // We have collected enough bytes for one cell => convert and add it to pixel array
                        if (index == cellSize - 1)
                        {
                            int cellValue = 0;
                            if (pixelData.BitsAllocated == 8)
                                cellValue = cellData[0];
                            else if (pixelData.BitsAllocated == 16)
                                cellValue = BitConverter.ToInt16(cellData, 0);
                            else if (pixelData.BitsAllocated == 32)
                                cellValue = BitConverter.ToInt32(cellData, 0);
                            else
                                Debug.LogError("Invalid format!");

                            intArray[pixelIndex] = cellValue;
                            pixelIndex++;
                        }
                    }
                    return intArray;
                }
                else
                    return null;
            }
            else
            {
                Debug.LogError("Pixel array is invalid");
                return null;
            }
        }

        private void CalcSliceLocFromPos(List<DICOMSliceFile> slices)
        {
            // We use the first slice as a starting point (a), andthe normalised vector (v) between the first and second slice as a direction.
            Vector3 v = (slices[1].position - slices[0].position).normalized;
            Vector3 a = slices[0].position;
            slices[0].location = 0.0f;

            for(int i = 1; i < slices.Count; i++)
            {
                // Calculate the vector between a and p (ap) and dot it with v to get the distance along the v vector (distance when projected onto v)
                Vector3 p = slices[i].position;
                Vector3 ap = p - a;
                float dot = Vector3.Dot(ap, v);
                slices[i].location = dot;
            }
        }
    }
}
