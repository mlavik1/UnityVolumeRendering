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
using System.Threading.Tasks;
using System.Data;

namespace UnityVolumeRendering
{
    /// <summary>
    /// DICOM importer.
    /// Reads a 3D DICOM dataset from a list of DICOM files.
    /// </summary>
    public class DICOMImporter : IImageSequenceImporter
    {
        public class DICOMSliceFile : IImageSequenceFile
        {
            public AcrNemaFile file;
            public string filePath;
            public float location = 0;
            public Vector3 position = Vector3.zero;
            public float intercept = 0.0f;
            public float slope = 1.0f;
            public float pixelSpacing = 0.0f;
            public float[] imageOrientation = null;
            public string seriesUID = "";

            public string GetFilePath()
            {
                return filePath;
            }
        }

        public class DICOMSeries : IImageSequenceSeries
        {
            public List<DICOMSliceFile> dicomFiles = new List<DICOMSliceFile>();

            public IEnumerable<IImageSequenceFile> GetFiles()
            {
                return dicomFiles;
            }
        }

        private int iFallbackLoc = 0;

        public IEnumerable<IImageSequenceSeries> LoadSeries(IEnumerable<string> fileCandidates, ImageSequenceImportSettings settings)
        {
            DataElementDictionary dataElementDictionary = new DataElementDictionary();
            UidDictionary uidDictionary = new UidDictionary();

            // Split parsed DICOM files into series (by DICOM series UID)
            Dictionary<string, DICOMSeries> seriesByUID = new Dictionary<string, DICOMSeries>();

            LoadSeriesFromResourcesInternal(dataElementDictionary, uidDictionary);

            // Load all DICOM files
            LoadSeriesInternal(fileCandidates, seriesByUID, settings.progressHandler);

            Debug.Log($"Loaded {seriesByUID.Count} DICOM series");

            return new List<DICOMSeries>(seriesByUID.Values);
        }
        public async Task<IEnumerable<IImageSequenceSeries>> LoadSeriesAsync(IEnumerable<string> fileCandidates, ImageSequenceImportSettings settings)
        {
            DataElementDictionary dataElementDictionary = new DataElementDictionary();
            UidDictionary uidDictionary = new UidDictionary();

            // Split parsed DICOM files into series (by DICOM series UID)
            Dictionary<string, DICOMSeries> seriesByUID = new Dictionary<string, DICOMSeries>();

            LoadSeriesFromResourcesInternal(dataElementDictionary, uidDictionary);

            await Task.Run(()=> LoadSeriesInternal(fileCandidates, seriesByUID, settings.progressHandler));

            Debug.Log($"Loaded {seriesByUID.Count} DICOM series");


            return new List<DICOMSeries>(seriesByUID.Values);
        }
        private void LoadSeriesInternal(IEnumerable<string> fileCandidates, Dictionary<string, DICOMSeries> seriesByUID, IProgressHandler progress)
        {
            // Load all DICOM files
            List<DICOMSliceFile> files = new List<DICOMSliceFile>();

            IEnumerable<string> sortedFiles = fileCandidates.OrderBy(s => s);

            int fileIndex = 0, numFiles = sortedFiles.Count();
            foreach (string filePath in sortedFiles)
            {
                progress.ReportProgress(fileIndex, numFiles, $"Loading DICOM file {fileIndex} of {numFiles}");
                DICOMSliceFile sliceFile = ReadDICOMFile(filePath);
                if (sliceFile != null)
                {
                    if (sliceFile.file.PixelData.IsJpeg)
                        Debug.LogError("DICOM with JPEG not supported by importer. Please enable SimpleITK from volume rendering import settings.");
                    else
                        files.Add(sliceFile);
                }
                fileIndex++;
            }

            foreach (DICOMSliceFile file in files)
            {
                if (!seriesByUID.ContainsKey(file.seriesUID))
                {
                    seriesByUID.Add(file.seriesUID, new DICOMSeries());
                }
                seriesByUID[file.seriesUID].dicomFiles.Add(file);
            }
        }
        private void LoadSeriesFromResourcesInternal(DataElementDictionary dataElementDictionary, UidDictionary uidDictionary)
        {
            try
            {
                // Load .dic files from Resources
                TextAsset dcmElemAsset = (TextAsset)Resources.Load("dicom-elements-2007.dic");
                Debug.Assert(dcmElemAsset != null, "dicom-elements-2007.dic is missing from the Resources folder");
                TextAsset dcmUidsAsset = (TextAsset)Resources.Load("dicom-uids-2007.dic");
                Debug.Assert(dcmUidsAsset != null, "dicom-uids-2007.dic is missing from the Resources folder");

                dataElementDictionary.LoadFromMemory(new MemoryStream(dcmElemAsset.bytes), DictionaryFileFormat.BinaryFile);
                uidDictionary.LoadFromMemory(new MemoryStream(dcmUidsAsset.bytes), DictionaryFileFormat.BinaryFile);
            }
            catch (Exception dictionaryException)
            {
                Debug.LogError("Problems processing dictionaries:\n" + dictionaryException);
            }
        }

        public VolumeDataset ImportSeries(IImageSequenceSeries series, ImageSequenceImportSettings settings)
        {
            DICOMSeries dicomSeries = (DICOMSeries)series;
            List<DICOMSliceFile> files = dicomSeries.dicomFiles;

            if (files.Count <= 1)
            {
                Debug.LogError("Insufficient number of slices.");
                return null;
            }

            // Create dataset
            VolumeDataset dataset = ScriptableObject.CreateInstance<VolumeDataset>();

            ImportSeriesInternal(files, dataset, settings.progressHandler);

            return dataset;
        }
        public async Task<VolumeDataset> ImportSeriesAsync(IImageSequenceSeries series, ImageSequenceImportSettings settings)
        {
            DICOMSeries dicomSeries = (DICOMSeries)series;
            List<DICOMSliceFile> files = dicomSeries.dicomFiles;

            if (files.Count <= 1)
            {
                Debug.LogError("Insufficient number of slices.");
                return null;
            }

            // Create dataset
            VolumeDataset dataset = ScriptableObject.CreateInstance<VolumeDataset>();

            await Task.Run(() => ImportSeriesInternal(files,dataset, settings.progressHandler));

            return dataset;
        }
        private void ImportSeriesInternal(List<DICOMSliceFile> files,VolumeDataset dataset, IProgressHandler progress)
        {
            // Calculate slice location from "Image Position" (0020,0032)
            CalculateSliceLocations(files);

            // Sort files by slice location
            files.Sort((DICOMSliceFile a, DICOMSliceFile b) => { return a.location.CompareTo(b.location); });

            Debug.Log($"Importing {files.Count} DICOM slices");

            dataset.datasetName = Path.GetFileName(files[0].filePath);
            dataset.dimX = files[0].file.PixelData.Columns;
            dataset.dimY = files[0].file.PixelData.Rows;
            dataset.dimZ = files.Count;
            int dimension = dataset.dimX * dataset.dimY * dataset.dimZ;
            dataset.data = new float[dimension];

            for (int iSlice = 0; iSlice < files.Count; iSlice++)
            {
                progress.ReportProgress(iSlice, files.Count, $"Importing slice {iSlice} of {files.Count}");
                DICOMSliceFile slice = files[iSlice];
                PixelData pixelData = slice.file.PixelData;
                int[] pixelArr = ToPixelArray(pixelData);
                if (pixelArr == null) // This should not happen
                    pixelArr = new int[pixelData.Rows * pixelData.Columns];

                for (int iRow = 0; iRow < pixelData.Rows; iRow++)
                {
                    for (int iCol = 0; iCol < pixelData.Columns; iCol++)
                    {
                        int pixelIndex = (iRow * pixelData.Columns) + iCol;
                        int dataIndex = (iSlice * pixelData.Columns * pixelData.Rows) + (iRow * pixelData.Columns) + iCol;

                        int pixelValue = pixelArr[pixelIndex];
                        float hounsfieldValue = pixelValue * slice.slope + slice.intercept;

                        dataset.data[dataIndex] = Mathf.Clamp(hounsfieldValue, -1024.0f, 3071.0f);
                    }
                }
            }

            if (files[0].pixelSpacing > 0.0f)
            {
                dataset.scale = new Vector3(
                    files[0].pixelSpacing * dataset.dimX,
                    files[0].pixelSpacing * dataset.dimY,
                    Mathf.Abs(files[files.Count - 1].location - files[0].location)
                ) / 1000.0f;
            }

            dataset.FixDimensions();
            
            // Convert from LPS to Unity's coordinate system
            ImporterUtilsInternal.ConvertLPSToUnityCoordinateSpace(dataset);
        }

        private DICOMSliceFile ReadDICOMFile(string filePath)
        {
            AcrNemaFile file = LoadFile(filePath);

            if (file != null && file.HasPixelData)
            {
                DICOMSliceFile slice = new DICOMSliceFile();
                slice.file = file;
                slice.filePath = filePath;

                Tag imagePositionTag = new Tag("(0020,0032)");
                Tag imageOrientationTag = new Tag("(0020,0037)");
                Tag locationTag = new Tag("(0020,1041)");
                Tag interceptTag = new Tag("(0028,1052)");
                Tag slopeTag = new Tag("(0028,1053)");
                Tag pixelSpacingTag = new Tag("(0028,0030)");
                Tag seriesUIDTag = new Tag("(0020,000E)");

                // Read position tag
                if (file.DataSet.Contains(imagePositionTag))
                {
                    DataElement elemLoc = file.DataSet[imagePositionTag];
                    Vector3 pos = Vector3.zero;
                    pos.x = (float)Convert.ToDouble(elemLoc.Value[0]);
                    pos.y = (float)Convert.ToDouble(elemLoc.Value[1]);
                    pos.z = (float)Convert.ToDouble(elemLoc.Value[2]);
                    slice.position = pos;
                }
                // Read location (fallback - should never happen)
                else if (file.DataSet.Contains(locationTag))
                {
                    DataElement elemLoc = file.DataSet[locationTag];
                    slice.location = (float)Convert.ToDouble(elemLoc.Value[0]);
                }
                else
                {
                    Debug.LogError($"Missing position tag in file: {filePath}.\n The file will not be imported correctly.");
                    // Fallback: use counter as location
                    slice.location = iFallbackLoc++ / 256.0f;
                }

                // Read image orientation
                if (file.DataSet.Contains(imageOrientationTag))
                {
                    DataElement elemImageOrientation = file.DataSet[imageOrientationTag];
                    slice.imageOrientation = new float[6];
                    for (int i = 0; i < 6; i++)
                        slice.imageOrientation[i] = (float)Convert.ToDouble(elemImageOrientation.Value[i]);
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

                // Read series UID
                if (file.DataSet.Contains(seriesUIDTag))
                {
                    DataElement elemSeriesUID = file.DataSet[seriesUIDTag];
                    slice.seriesUID = Convert.ToString(elemSeriesUID.Value[0]);
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

        private void CalculateSliceLocations(List<DICOMSliceFile> slices)
        {
            if (slices.Count == 0 || slices[0].imageOrientation == null)
                return;

            // Get the direction cosines
            float[] cosines = slices[0].imageOrientation;
            // Construct the basis vectors
            Vector3 xBase = new Vector3(cosines[0], cosines[1], cosines[2]);
            Vector3 yBase = new Vector3(cosines[3], cosines[4], cosines[5]);
            Vector3 normal = Vector3.Cross(xBase, yBase);

            for(int i = 0; i < slices.Count; i++)
            {
                Vector3 position = slices[i].position;
                // Project p onto n. d = dot(p,n) / |n| = dot(p,n)
                float distance = Vector3.Dot(position, normal);
                slices[i].location = distance;
            }
        }  
    }
}
