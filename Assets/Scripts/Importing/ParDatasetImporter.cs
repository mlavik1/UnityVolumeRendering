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
using System.Collections;
using System.Collections.Generic;
using System.Linq; 
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;

namespace UnityVolumeRendering
{

    public class ParDatasetImporter
    {
        string filePath;
        string fileName;
        float latticeConstant;
        int i;
        private int x;
        private int y;
        private int z;
        private float v1;
        private float v2;
        private float v3;

        private int counterLine;
        public double newLattice;
        string molecules;
        string[] atomCount;
        string combinedAtomCount;
        int[] numberOfAtoms;
        int[] atomSum;
        char[] atomChar;
        string molecule;
        int sum;
        int totalAtoms;

        public int size;

        float [][] basisCells;
        float [][] coordinatebasisCells;
        float [][] cartesiancoordinatebasisCells;
        bool isDirect;
        string[] vectorString;

        string grid;
        int[] gridInt;
        int nx;
        int ny;
        int nz;
        int gridSize;
        int gridDataLines;
        float volume;
        float volumeScale;
        float vol;

        int dimension;
        int[] list;

        string gridRow;
        double[] dataGrid;
        string[] densityLine;

        int[] dimArray;
        int dimTotal;
        double[] data;
        string currentLine;
        string[] densityTrim;
        double[] volumeScaledData;

        public ParDatasetImporter(string filePath, int nx, int ny, int nz)
        {
            this.filePath = filePath;
            this.nx = nx;
            this.ny = ny;
            this.nz = nz;
        }

        public VolumeDataset Import() //fills VolumeDataset object
        {
            var extension = Path.GetExtension(filePath);
            if(!File.Exists(filePath))
            {
                Debug.LogError("The file does not exist: " + filePath);
                return null;
            }
            // ideal order of function execution
            latticeConstant = getLatticeConstant();
            basisCells = setLatticeVector();
            volumeScale = getVolume();
            sum = getAtomSum();
            
            coordinatebasisCells = readCoordinates();
            isDirect = directOrCartesian();
            if (isDirect)
            {
                cartesiancoordinatebasisCells = toCartesian();
            }

            dimArray = readDimensions();
            dimTotal = dimArray[0] * dimArray[1] * dimArray[2];
            nx = dimArray[0];
            ny = dimArray[1];
            nz = dimArray[2]; // dimensions 

            gridDataLines = dataLines();
            dataGrid = readGrid();

            VolumeDataset dataFiller = new VolumeDataset(); //volume object then gets sent to VolumeObjectFactory
            dataFiller.datasetName = fileName;
            dataFiller.filePath = filePath;
            dataFiller.dimX = nx;
            dataFiller.dimY = ny;
            dataFiller.dimZ = nz;
            dataFiller.nx = nx;
            dataFiller.ny = ny;
            dataFiller.nz = nz;
            dataFiller.volumeScale = (float) (1 / volumeScale);
            dataFiller.dataGrid = new double[dimTotal];
            volumeScaledData = new double[dimTotal];
            
            for (int ix = 0; ix < nx; ix++)
            {
                for (int iy = 0; iy < ny; iy++)
                {
                    for (int iz = 0; iz < nz; iz++)
                    {
                        int itr =  (iz * nx * ny) + (iy * nx) + ix;
                        volumeScaledData[itr] = (float)dataGrid[itr] * (float)dataFiller.volumeScale * (float)0.036749309; //density * volumescale * e_units
                    }
                }
            }
            for (int i = 0; i < dimTotal; i++)
            {
                //dataFiller.dataGrid[i] = volumeScaledData[i];
                dataFiller.dataGrid[i] = dataGrid[i];
            }

            Debug.Log("Loaded dataset in range: " + dataFiller.GetMinDataValueDouble() + "  -  " + dataFiller.GetMaxDataValueDouble());
            
            return dataFiller;
        }

        public void getSystemTitle() //reads system
        {  
            StreamReader stream = new StreamReader(filePath);
            string title = null; 
            string line = null; 
            int lines = 0;
            //string line = stream.ReadLine().Skip(0).Take().First().ToString(); //read first line
            counterLine = 1;

            for (int i  = 0; i < counterLine; ++i) // read first line
            {
                line = stream.ReadLine();
            }
            stream.Close();
            Debug.Log(line);
        }
        public float getLatticeConstant() //reads lattice parameter
        {  
            StreamReader stream = new StreamReader(filePath);
            for (int i = 0; i < 2; ++i)
            {
                if (i == 1)
                {
                var line = stream.ReadLine();
                string[] bits = line.Trim().Split(' '); 

                latticeConstant = float.Parse(bits[0]);
                }
                else if (i != 1)
                {
                    stream.ReadLine();
                }     
            } 
            return latticeConstant;
        }

        public float[][] setLatticeVector() // multiplies basis by lattice parameter
        {
            StreamReader stream = new StreamReader(filePath);
            float[][] basisCells = new float[3][];
            basisCells[0] = new float[3];
            basisCells[1] = new float[3];
            basisCells[2] = new float[3];
            latticeConstant = getLatticeConstant();
            string[] vectorString = new string[9];
            int i;
            int j;
            string molecule;

            for (i = 0; i < 5; i++)
            {
                if (i == 2 || i == 3 | i == 4)
                {
                    var latticeLine = stream.ReadLine();
                    vectorString = latticeLine.Trim().Split(' '); //first line 

                    for (j = 0; j < 3; j++)
                    {
                    
                    if (i == 2 && j == 0)
                    {
                        basisCells[j][0] = float.Parse(vectorString[0]) * latticeConstant; // multiply constant
                        basisCells[j][1] = float.Parse(vectorString[4]) * latticeConstant;
                        basisCells[j][2] = float.Parse(vectorString[8]) * latticeConstant;
                        
                    }
                    if (i == 3 && j == 1)
                    {
                        basisCells[j][0] = float.Parse(vectorString[0]) * latticeConstant;
                        basisCells[j][1] = float.Parse(vectorString[4]) * latticeConstant;
                        basisCells[j][2] = float.Parse(vectorString[8]) * latticeConstant;

                    }
                    if (i == 4 && j == 2)
                    {
                        basisCells[j][0] = float.Parse(vectorString[0]) * latticeConstant;
                        basisCells[j][1] = float.Parse(vectorString[4]) * latticeConstant;
                        basisCells[j][2] = float.Parse(vectorString[8]) * latticeConstant;
                    
                    }

                    }
                }

                else if (i != 2 || i != 3 || i != 4)
                {
                    stream.ReadLine();
                }          
            }
            return basisCells;
        }

        public float getVolume() //density is written in as p * V we must divide the volume (same as multiplying the scale)
        {
            volume = basisCells[0][0]*(basisCells[1][1]*basisCells[2][2] - basisCells[2][1]*basisCells[1][2])
                   - basisCells[1][0]*(basisCells[0][1]*basisCells[2][2] - basisCells[2][1]*basisCells[0][2])
                   + basisCells[2][0]*(basisCells[0][1]*basisCells[1][2] - basisCells[1][1]*basisCells[0][2]);
            Debug.Log(volume);
            // make sure volume is +
            // this volume is in units selected (default bohr) but we need it in ang**3
            volumeScale = Math.Abs(volume) / ( (float)Math.Pow(1.889725992,3)); //bohr/hartree -> ang/eV

            return volumeScale;  
        }


        public string moleculeName() //reads molecule works for as many atoms
        {
            StreamReader stream = new StreamReader(filePath);
            for (int i = 0; i < 6; i++)
            {
                if (i == 5)
                {
                var line = stream.ReadLine();
                string[] atoms = line.Trim().Split(' '); 

                for (int j = 0; j < atoms.Count(); j++)
                {
                    molecules += string.Join("", atoms[j]);
                }

                }
                else if (i != 5)
                {
                    stream.ReadLine();
                }     
            }
            return molecule;
        }

        public int getAtomSum() // returns sum of atoms - USE ONLY ONCE 
        {
            //multiple calls of this function might change the sum
            StreamReader stream = new StreamReader(filePath);
            for (int i = 0; i < 7; i++)
            {
                if (i == 6)
                {
                var line = stream.ReadLine();
                string[] atomCount = line.Trim().Split(' ');

                for (int j = 0; j < atomCount.Count(); j++)
                {
                    combinedAtomCount += string.Join("", atomCount[j]);
                }

                size = System.Text.ASCIIEncoding.ASCII.GetByteCount(combinedAtomCount);
                atomChar = new char[size]; //size of atom types (Cd Se) -> 2 
                atomSum = new int[size];

                for (int k = 0; k < combinedAtomCount.Count(); k++)
                {
                    atomChar[k] = combinedAtomCount[k];
                }

                
                for (int l = 0; l < atomChar.Count(); l++)
                {
                    atomSum[l] = Int16.Parse(atomChar[l].ToString());
                }
                }

                else if (i != 6)
                {
                    stream.ReadLine();
                }    
            }
            sum = atomSum.Sum();
            return sum;
        }
        
        public float[][] readCoordinates() //sizeof(vectorString) == 5
        {
            //implement test = test.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            StreamReader stream = new StreamReader(filePath);
            float[][] coordinatebasisCells = new float[sum][];

            string latticeLine = null;
            
            // unspecified m-array size initializer for loop M given N = 3
            for (int x = 0; x < sum; x++)
            {
                coordinatebasisCells[x] = new float[3]; //3 for  x y z
            } 
            for (int i = 0; i < 9; i++)
            {   
                if (i == 8) 
                {
                    for (int j = 0; j < sum; j++)
                    {
                        latticeLine = stream.ReadLine();
                        vectorString = latticeLine.Trim().Split(' '); //first line
                        coordinatebasisCells[j][0] = float.Parse(vectorString[0].ToString()); //using 0 2 4 because that's how LINQ parses the line
                        coordinatebasisCells[j][1] = float.Parse(vectorString[2]);
                        coordinatebasisCells[j][2] = float.Parse(vectorString[4]);
                    }
                }
                else if (i != 8)
                {
                    stream.ReadLine();
                }          
            }
            return coordinatebasisCells;
        }
        
        public bool directOrCartesian() //reads whether system is in Direct or Cartesian
        {

            StreamReader stream = new StreamReader(filePath);
            string molecule = null;
            string compare = "Direct";

            for (int i = 0; i < 8; i++)
            {
                if (i == 7)
                {
                    var line = stream.ReadLine();
                    string cardinality = line.Trim();

                    isDirect = string.Equals(cardinality, compare);
                } 

                if (i != 7)
                {
                    stream.ReadLine();
                }
            }
            return isDirect;
        }

        public float[][] toCartesian() // Direct mode -> Cartesian. took one day to debug b/c the sum was subject to change via function call getatomcount() SOMEONE FIX
        {
            float[][] cartesiancoordinatebasisCells = new float[sum][];

            //initialize memory
            for (int x = 0; x < sum; x++)
            {
                cartesiancoordinatebasisCells[x] = new float[3];
            }
            float[][] coordinatebasisCells = new float[sum][];
            for (int x = 0; x < sum; x++)
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
                for (int i = 0; i < sum; i++)
                {
                    v1 = coordinatebasisCells[i][0]*basisCells[0][0] +  coordinatebasisCells[i][1]*basisCells[0][1] + coordinatebasisCells[i][2]*basisCells[0][2];
                    v2 = coordinatebasisCells[i][0]*basisCells[1][0] +  coordinatebasisCells[i][1]*basisCells[1][1] + coordinatebasisCells[i][2]*basisCells[1][2];
                    v3 = coordinatebasisCells[i][0]*basisCells[2][0] +  coordinatebasisCells[i][1]*basisCells[2][1] + coordinatebasisCells[i][2]*basisCells[2][2];
                    cartesiancoordinatebasisCells[i][0] = v1;
                    cartesiancoordinatebasisCells[i][1] = v2;
                    cartesiancoordinatebasisCells[i][2] = v3;
                }

            }
            
            return cartesiancoordinatebasisCells;
        }
        public int[] readDimensions() // calculates nx * ny * nz
        {
            StreamReader stream = new StreamReader(filePath);
            for (int i = 0; i < 8 + sum + 2; i++) // + (sum + 2 ) since we aren't given number of atoms
             {
                if (i == 7 + sum + 2) 
                {
                var line = stream.ReadLine();
                string grid = line.Trim();

                gridInt = new int[3]; //size of atom types (Cd Se) -> 2
                
                // Split on one or more non-digit characters.
                string[] numbers = Regex.Split(grid, @"\D+");
                for (int q = 0; q < numbers.Count(); q++)
                {
                    if (!string.IsNullOrEmpty(numbers[q]))
                    {
                        gridInt[q] = int.Parse(numbers[q]);
                    }
                }

                for (int m = 0; m < 3; m++)
                {
                    nx = gridInt[0];
                    ny = gridInt[1];
                    nz = gridInt[2];
                }

                }
                else if (i != 7 + sum + 2)
                {
                    stream.ReadLine();
                }    
            }
            return gridInt;
        }

        public int calculateDimensions()
        {
            list = readDimensions();

            for (int i = 0; i < list.Count(); i++)
            {
                dimension *= list[i];
            }
            return dimension;
        }

        public int dataLines() // each line contains 10 elements, therefore there are 10 colums. Divide by 10
        {
            gridDataLines = dimTotal / 10;
            return gridDataLines;
        } 
        public double[] readGrid() 
        {
            StreamReader stream = new StreamReader(filePath);

            dataGrid = new double[dimTotal]; 

            List<double> data = new List<double>();

            for (int i = 0; i < 8 + sum + 2 + gridDataLines; i++)
             {
                if (i == (8 + sum + 2)) 
                {
                   for (int j = 0; j < gridDataLines + 1; j++)
                   {
                        currentLine = stream.ReadLine();
                        gridRow = currentLine.Trim();

                        //densityLine = Regex.Split(gridRow, @"(?<=\d)\s*[+*/-]\s*(?=-|\d)"); // regex split stores into densityLine[0] .. Maybe append .ToArray()?
                        densityLine = Regex.Split(gridRow, @"(/^[+\-]?(?=.)(0|[1-9]\d*)?(\.\d*)?(?:(\d)[eE][+\-]?\d+)?$/)"); //thank stackoverflow
                        densityTrim = densityLine[0].Trim().Split(' ');

                        for (int r = 0; r < densityTrim.Length; r++)
                        {
                            if (!string.IsNullOrEmpty(densityTrim[r]) && (Regex.IsMatch(densityTrim[r], @"\d")) && !string.IsNullOrWhiteSpace(densityTrim[r]))
                            {
                                data.Add(double.Parse(densityTrim[r]));
                            }
                        }
                   }
                }
                else if (i != 8 + sum + 2)
                {
                    stream.ReadLine();
                }    
             }
            dataGrid = data.ToArray();
            return dataGrid;
        }      
    }
}

