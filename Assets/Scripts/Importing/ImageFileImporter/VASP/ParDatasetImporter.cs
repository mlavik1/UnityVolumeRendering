/*----------------------------------------------------------------------------
#   file made by Jason (jasonks2)
#   project start 8-10-2021
#   finished 9-7-2021
#
#       Thank you to prof. Andre Schleife   
#       Thank you to dano - "chg2cube.pl" (perl vasp library)
#       Thank you to mlavik - Unity Volume Rendering
#       Thank you to Sung Sakong, Dept. of Phys., Univsity Duisburg-Essen
#      
#       RCS INFORMATION:
#       $RCSfile: vaspparchgplugin.c,v $
#       $Author: johns $  
------------------------------------------------------------------------------*/
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace UnityVolumeRendering
{
    public class ParDatasetImporter : IImageFileImporter
    {
        string filePath;
        string fileName;
        float latticeConstant;

        string[] atomNames;
        int[] atomCount;
        int totalAtomCount;

        float[][] basisCells;
        float[][] coordinatebasisCells;
        float[][] cartesiancoordinatebasisCells;
        bool isDirect;

        int nx;
        int ny;
        int nz;
        int gridDataLines;
        float volume;
        float volumeScale;

        float[] dataGrid;
        string[] densityLine;

        int[] dimArray;
        int dimTotal;
        string[] densityTrim;
        float[] volumeScaledData;

        string[] fileContentLines;
        int fileContentIndex;

        public VolumeDataset Import(string filePath)
        {
            this.filePath = filePath;
            
            var extension = Path.GetExtension(filePath);
            if (!File.Exists(filePath))
            {
                Debug.LogError("The file does not exist: " + filePath);
                return null;
            }

            fileContentLines = File.ReadLines(filePath).Where(x => x.Trim(' ') != "").ToArray();
            fileContentIndex = 0;

            ReadSystemTitle();
            ReadLatticeConstant();
            ReadLatticeVectors();
            GetVolume();
            ReadAtomNames();
            ReadAtomSum();
            ReadCoordinateSystemType();
            ReadCoordinates();
            if (isDirect)
            {
                cartesiancoordinatebasisCells = ToCartesian();
            }

            ReadDimensions();
            dimTotal = dimArray[0] * dimArray[1] * dimArray[2];
            nx = dimArray[0];
            ny = dimArray[1];
            nz = dimArray[2]; // dimensions 

            CalculateDataLines();
            ReadGrid();

            VolumeDataset dataFiller = new VolumeDataset(); //volume object then gets sent to VolumeObjectFactory
            dataFiller.datasetName = fileName;
            dataFiller.filePath = filePath;
            dataFiller.dimX = nx;
            dataFiller.dimY = ny;
            dataFiller.dimZ = nz;
            dataFiller.volumeScale = (float)(1 / volumeScale);
            dataFiller.data = new float[dimTotal];
            volumeScaledData = new float[dimTotal];

            for (int ix = 0; ix < nx; ix++)
            {
                for (int iy = 0; iy < ny; iy++)
                {
                    for (int iz = 0; iz < nz; iz++)
                    {
                        int itr = (iz * nx * ny) + (iy * nx) + ix;
                        volumeScaledData[itr] = (float)dataGrid[itr] * (float)dataFiller.volumeScale * (float)0.036749309; //density * volumescale * e_units
                    }
                }
            }
            for (int i = 0; i < dimTotal; i++)
            {
                dataFiller.data[i] = dataGrid[i];
            }

            Debug.Log("Loaded dataset in range: " + dataFiller.GetMinDataValue() + "  -  " + dataFiller.GetMaxDataValue());

            return dataFiller;
        }

        private string ParseLine()
        {
            Debug.Assert(fileContentIndex < fileContentLines.Length);
            return fileContentLines[fileContentIndex++];
        }

        private string PeekLine()
        {
            Debug.Assert(fileContentIndex < fileContentLines.Length);
            return fileContentLines[fileContentIndex];
        }

        public void ReadSystemTitle()
        {
            ParseLine(); // We don't use header comment for anything now
        }

        /// <summary>
        /// Reads lattice parameter
        /// </summary>
        public void ReadLatticeConstant()
        {
            var line = ParseLine();
            string[] bits = line.Trim().Split(' ').Where(x => x != "").ToArray();

            latticeConstant = float.Parse(bits[0]);
        }

        /// <summary>
        /// Multiplies basis by lattice parameter
        /// </summary>
        public void ReadLatticeVectors()
        {
            basisCells = new float[3][];
            basisCells[0] = new float[3];
            basisCells[1] = new float[3];
            basisCells[2] = new float[3];

            for (int i = 0; i < 3; i++)
            {
                string latticeLine = ParseLine();
                string[] vectorString = latticeLine.Trim().Split(' ').Where(t => t.Length > 0).ToArray();
                Debug.Assert(vectorString.Length == 3);

                basisCells[i][0] = float.Parse(vectorString[0]) * latticeConstant;
                basisCells[i][1] = float.Parse(vectorString[1]) * latticeConstant;
                basisCells[i][2] = float.Parse(vectorString[2]) * latticeConstant;
            }
        }

        /// <summary>
        /// Density is written in as p * V we must divide the volume (same as multiplying the scale)
        /// </summary>
        public void GetVolume()
        {
            volume = basisCells[0][0] * (basisCells[1][1] * basisCells[2][2] - basisCells[2][1] * basisCells[1][2])
                   - basisCells[1][0] * (basisCells[0][1] * basisCells[2][2] - basisCells[2][1] * basisCells[0][2])
                   + basisCells[2][0] * (basisCells[0][1] * basisCells[1][2] - basisCells[1][1] * basisCells[0][2]);
            Debug.Log(volume);
            // make sure volume is +
            // this volume is in units selected (default bohr) but we need it in ang**3
            volumeScale = Math.Abs(volume) / ((float)Math.Pow(1.889725992, 3)); //bohr/hartree -> ang/eV
        }

        /// <summary>
        /// Reads molecule works for as many atoms
        /// </summary>
        public void ReadAtomNames()
        {
            var line = PeekLine();
            string[] names = line.Trim().Split(' ').Where(t => t.Length > 0).ToArray();
            int num = 0;
            if (int.TryParse(names[0], out num))
                return; // Current line is atom count (no atom names specified in this file)
            else
            {
                ParseLine(); // Increment line index
                atomNames = names;
            }
        }

        public void ReadAtomSum()
        {
            var line = ParseLine();
            string[] atomCountStrings = line.Trim().Split(' ').Where(t => t.Length > 0).ToArray();
            atomCount = new int[atomCountStrings.Length];
            for (int i = 0; i < atomCountStrings.Length; i++)
                atomCount[i] = Int16.Parse(atomCountStrings[i]);

            totalAtomCount = atomCount.Sum();
        }

        public void ReadCoordinates()
        {
            coordinatebasisCells = new float[totalAtomCount][];

            string latticeLine = null;

            // unspecified m-array size initializer for loop M given N = 3
            for (int x = 0; x < totalAtomCount; x++)
            {
                coordinatebasisCells[x] = new float[3]; //3 for  x y z
            }
            for (int i = 0; i < totalAtomCount; i++)
            {
                latticeLine = ParseLine();
                string[] vectorString = latticeLine.Trim().Split(' ').Where(x => x != "").ToArray();
                coordinatebasisCells[i][0] = float.Parse(vectorString[0]);
                coordinatebasisCells[i][1] = float.Parse(vectorString[1]);
                coordinatebasisCells[i][2] = float.Parse(vectorString[2]);
            }
        }

        // Reads whether system is in Direct or Cartesian
        public void ReadCoordinateSystemType()
        {
            string molecule = null;
            string compare = "Direct";

            var line = ParseLine();
            string cardinality = line.Trim();

            isDirect = string.Equals(cardinality, compare);
        }

        /// <summary>
        /// Direct mode -> Cartesian.
        /// TODO: took one day to debug b/c the sum was subject to change via function call getatomcount() SOMEONE FIX
        /// </summary>
        public float[][] ToCartesian()
        {
            float[][] cartesiancoordinatebasisCells = new float[totalAtomCount][];

            //initialize memory
            for (int x = 0; x < totalAtomCount; x++)
            {
                cartesiancoordinatebasisCells[x] = new float[3];
            }
            float[][] coordinatebasisCells = new float[totalAtomCount][];
            for (int x = 0; x < totalAtomCount; x++)
            {
                coordinatebasisCells[x] = new float[3];
            }

            if (!isDirect)
            {
                Debug.Log("Input atomic position array is already Cartesian");
            }
            else
            {
                //conversion here
                for (int i = 0; i < totalAtomCount; i++)
                {
                    float v1 = coordinatebasisCells[i][0] * basisCells[0][0] + coordinatebasisCells[i][1] * basisCells[0][1] + coordinatebasisCells[i][2] * basisCells[0][2];
                    float v2 = coordinatebasisCells[i][0] * basisCells[1][0] + coordinatebasisCells[i][1] * basisCells[1][1] + coordinatebasisCells[i][2] * basisCells[1][2];
                    float v3 = coordinatebasisCells[i][0] * basisCells[2][0] + coordinatebasisCells[i][1] * basisCells[2][1] + coordinatebasisCells[i][2] * basisCells[2][2];
                    cartesiancoordinatebasisCells[i][0] = v1;
                    cartesiancoordinatebasisCells[i][1] = v2;
                    cartesiancoordinatebasisCells[i][2] = v3;
                }
            }

            return cartesiancoordinatebasisCells;
        }

        /// <summary>
        /// Calculates nx * ny * nz
        /// </summary>
        public void ReadDimensions()
        {
            var line = ParseLine();
            string grid = line.Trim();

            dimArray = new int[3]; //size of atom types (Cd Se) -> 2

            // Split on one or more non-digit characters.
            string[] numbers = Regex.Split(grid, @"\D+");
            for (int q = 0; q < numbers.Count(); q++)
            {
                if (!string.IsNullOrEmpty(numbers[q]))
                {
                    dimArray[q] = int.Parse(numbers[q]);
                }
            }

            for (int m = 0; m < 3; m++)
            {
                nx = dimArray[0];
                ny = dimArray[1];
            }
        }

        /// <summary>
        /// Each line contains 10 elements, therefore there are 10 colums. Divide by 10
        /// </summary>
        public void CalculateDataLines()
        {
            gridDataLines = dimTotal / 10;
        }

        public void ReadGrid()
        {
            dataGrid = new float[dimTotal];
            List<float> data = new List<float>();

            for (int i = 0; i < gridDataLines + 1; i++)
            {
                if (fileContentIndex == fileContentLines.Length)
                    break; // TODO: Find a more elegant solution (some datasets will have one extra line -> see the "+ 1" in loop above.)

                string gridRow = ParseLine().Trim();

                densityLine = Regex.Split(gridRow, @"(/^[+\-]?(?=.)(0|[1-9]\d*)?(\.\d*)?(?:(\d)[eE][+\-]?\d+)?$/)"); //thank stackoverflow
                densityTrim = densityLine[0].Trim().Split(' ');

                for (int r = 0; r < densityTrim.Length; r++)
                {
                    if (!string.IsNullOrEmpty(densityTrim[r]) && (Regex.IsMatch(densityTrim[r], @"\d")) && !string.IsNullOrWhiteSpace(densityTrim[r]))
                    {
                        data.Add(float.Parse(densityTrim[r]));
                    }
                }
            }
            dataGrid = data.ToArray();
        }
    }
}
