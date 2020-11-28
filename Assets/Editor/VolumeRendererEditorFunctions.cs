using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;

namespace UnityVolumeRendering
{
    public class VolumeRendererEditorFunctions
    {
        [MenuItem("Volume Rendering/Load raw dataset")]
        static void ShowDatasetImporter()
        {
            string file = EditorUtility.OpenFilePanel("Select a dataset to load", "DataFiles", "");
            if (File.Exists(file))
            {
                EditorDatasetImporter.ImportDataset(file);
            }
            else
            {
                Debug.LogError("File doesn't exist: " + file);
            }
        }

        [MenuItem("Volume Rendering/Load DICOM")]
        static void ShowDICOMImporter()
        {
            string dir = EditorUtility.OpenFolderPanel("Select a folder to load", "", "");
            if (Directory.Exists(dir))
            {
                bool recursive = true;

                // Read all files
                IEnumerable<string> fileCandidates = Directory.EnumerateFiles(dir, "*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                    .Where(p => p.EndsWith(".dcm", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".dicom", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".dicm", StringComparison.InvariantCultureIgnoreCase));

                if (!fileCandidates.Any())
                {
#if UNITY_EDITOR
                    if (UnityEditor.EditorUtility.DisplayDialog("Could not find any DICOM files",
                        $"Failed to find any files with DICOM file extension.{Environment.NewLine}Do you want to include files without DICOM file extension?", "Yes", "No"))
                    {
                        fileCandidates = Directory.EnumerateFiles(dir, "*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                    }
#endif
                }

                if (fileCandidates.Any())
                {
                    DICOMImporter importer = new DICOMImporter(fileCandidates, Path.GetFileName(dir));
                    VolumeDataset dataset = importer.Import();
                    if (dataset != null)
                        VolumeObjectFactory.CreateObject(dataset);
                }    
                else
                    Debug.LogError("Could not find any DICOM files to import.");

                
            }
            else
            {
                Debug.LogError("Directory doesn't exist: " + dir);
            }
        }

        [MenuItem("Volume Rendering/Load image sequence")]
        static void ShowSequenceImporter()
        {
            string dir = EditorUtility.OpenFolderPanel("Select a folder to load", "", "");
            if (Directory.Exists(dir))
            {
                ImageSequenceImporter importer = new ImageSequenceImporter(dir);
                VolumeDataset dataset = importer.Import();
                if (dataset != null)
                    VolumeObjectFactory.CreateObject(dataset);
            }
            else
            {
                Debug.LogError("Directory doesn't exist: " + dir);
            }
        }
    }
}
