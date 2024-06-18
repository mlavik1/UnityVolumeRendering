using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace UnityVolumeRendering
{
    public class VolumeLoader
    {
        
        public static void LoadNRRDDataset(VolumeRenderedObject renderedObject)
        {
            LoadNRRDDatasetAsync(renderedObject);
        }

        public static void LoadNIFTIDataset(VolumeRenderedObject renderedObject)
        {
            LoadNIFTIDatasetAsync(renderedObject);
        }

        public static void LoadImageFileDataset(VolumeRenderedObject renderedObject)
        {
            LoadImageFileDatasetAsync(renderedObject);
   
        }

        public static void LoadParDataset(VolumeRenderedObject renderedObject)
        {
            LoadParDatasetAsync(renderedObject);
        }

        // TODO : Load Dicom Async

        public static async void LoadNRRDDatasetAsync(VolumeRenderedObject renderedObject)
        {
            if (!SimpleITKManager.IsSITKEnabled())
            {
                if (EditorUtility.DisplayDialog("Missing SimpleITK", "You need to download SimpleITK to load NRRD datasets from the import settings menu.\n" +
                    "Do you want to open the import settings menu?", "Yes", "No"))
                {
                    ImportSettingsEditorWindow.ShowWindow();
                }
            }

            string file = EditorUtility.OpenFilePanel("Select a dataset to load (.nrrd)", "DataFiles", "");
            if (File.Exists(file))
            {
                Debug.Log("Async dataset load. Hold on.");
                using (ProgressHandler progressHandler = new ProgressHandler(new EditorProgressView(), "NRRD import"))
                {
                    progressHandler.ReportProgress(0.0f, "Importing NRRD dataset");

                    IImageFileImporter importer = ImporterFactory.CreateImageFileImporter(ImageFileFormat.NRRD);
                    
                    VolumeDataset dataset = await importer.ImportAsync(file);
    
                    renderedObject.SetSegmentationDataset(dataset);

                    progressHandler.ReportProgress(0.8f, "Loading object");
  
                }
            }
            else
            {
                Debug.LogError("File doesn't exist: " + file);
            }
        }

        public static async void LoadNIFTIDatasetAsync(VolumeRenderedObject renderedObject)
        {
            string file = EditorUtility.OpenFilePanel("Select a dataset to load (.nii)", "DataFiles", "");
            if (File.Exists(file))
            {
                Debug.Log("Async dataset load. Hold on.");
                using (ProgressHandler progressHandler = new ProgressHandler(new EditorProgressView(), "NIFTI import"))
                {
                    progressHandler.ReportProgress(0.0f, "Importing NIfTI dataset");

                    IImageFileImporter importer = ImporterFactory.CreateImageFileImporter(ImageFileFormat.NIFTI);
                    
                    VolumeDataset dataset = await importer.ImportAsync(file);
                    
                    renderedObject.SetSegmentationDataset(dataset);
                    
                    progressHandler.ReportProgress(0.0f, "Loaded object");

                    
                }
            }
            else
            {
                Debug.LogError("File doesn't exist: " + file);
                
            }
        }

        public static async void LoadImageFileDatasetAsync(VolumeRenderedObject renderedObject)
        {
            string file = EditorUtility.OpenFilePanel("Select a dataset to load", "DataFiles", "");
            if (File.Exists(file))
            {
                Debug.Log("Async dataset load. Hold on.");
                using (ProgressHandler progressHandler = new ProgressHandler(new EditorProgressView(), "Image file import"))
                {
                    progressHandler.ReportProgress(0.0f, "Importing image file dataset");

                    IImageFileImporter importer = ImporterFactory.CreateImageFileImporter(ImageFileFormat.Unknown);
                    
                    VolumeDataset dataset = await importer.ImportAsync(file);
                    
                    renderedObject.SetSegmentationDataset(dataset, progressHandler);

                    progressHandler.ReportProgress(0.0f, "Loaded object");

                }
            }
            else
            {
                Debug.LogError("File doesn't exist: " + file);
            }
        }

        public static async void LoadParDatasetAsync(VolumeRenderedObject renderedObject)
        {
            string file = EditorUtility.OpenFilePanel("Select a dataset to load", "DataFiles", "");
            if (File.Exists(file))
            {
                Debug.Log("Async dataset load. Hold on.");
                using (ProgressHandler progressHandler = new ProgressHandler(new EditorProgressView(), "AVSP import"))
                {
                    progressHandler.ReportProgress(0.0f, "Importing VASP dataset");

                    IImageFileImporter importer = ImporterFactory.CreateImageFileImporter(ImageFileFormat.VASP);
                    
                    VolumeDataset dataset = await importer.ImportAsync(file);

                    renderedObject.SetSegmentationDataset(dataset, progressHandler);

                    progressHandler.ReportProgress(0.0f, "Loaded object");

                }
            }
            else
            {
                Debug.LogError("File doesn't exist: " + file);
            }
        }

        // TODO : Load Sequence Async
    }
}