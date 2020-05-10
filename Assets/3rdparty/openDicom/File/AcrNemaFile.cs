/*
   
    openDICOM.NET openDICOM# 0.1.1

    openDICOM# provides a library for DICOM related development on Mono.
    Copyright (C) 2006-2007  Albert Gnandt

    This library is free software; you can redistribute it and/or
    modify it under the terms of the GNU Lesser General Public
    License as published by the Free Software Foundation; either
    version 2.1 of the License, or (at your option) any later version.

    This library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
    Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public
    License along with this library; if not, write to the Free Software
    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301 USA


    $Id: AcrNemaFile.cs 48 2007-03-28 13:49:15Z agnandt $
*/
using System;
using System.IO;
using openDicom;
using openDicom.DataStructure;
using openDicom.DataStructure.DataSet;
using openDicom.Encoding;
using openDicom.Image;


namespace openDicom.File
{

    /// <summary>
    ///     This class represents a prior DICOM file, an ACR-NEMA file.
    /// </summary>
    public class AcrNemaFile
    {
        protected DataSet dataSet = null;
        /// <summary>
        ///     DICOM data set containing the entire file content.
        /// </summary>
        public DataSet DataSet
        {
            get 
            { 
                if (dataSet != null)
                    return dataSet;
                else
                    throw new DicomException("Data set is null.",
                        "AcrNemaFile.DataSet");
            }
        }

        protected PixelData pixelData = null;
        /// <summary>
        ///     This file instance's pixel data.
        /// </summary>
        /// <remarks>
        ///     Use in combination with <see cref="HasPixelData" /> in order to
        ///     prevent null pointer exceptions.
        /// </remarks>
        public PixelData PixelData
        {
            get 
            { 
                if (pixelData != null)
                    return pixelData;
                else
                    throw new DicomException("Pixel data is null.", 
                        "AcrNemaFile.PixelData");
            }            
        }

        /// <summary>
        ///     Determines whether an ACR-NEMA file contains pixel data.
        /// </summary>
        public bool HasPixelData
        {
            get { return PixelData.HasPixelData(dataSet); }
        }

        /// <summary>
        ///     Switch for controlling strictness of entire file content
        ///     decoding.
        /// </summary>
        public bool IsStrictDecoded
        {
            set { ValueRepresentation.IsStrictDecoded = value; }
            get { return ValueRepresentation.IsStrictDecoded; }
        }


        /// <summary>
        ///     Creates an ACR-NEMA file instance from a ACR-NEMA output stream.
        /// </summary>
        public AcrNemaFile(Stream stream): this(stream, true) {}

        /// <summary>
        ///     Creates an ACR-NEMA file instance from an ACR-NEMA output stream.
        ///     <see cref="IsStrictDecoded" /> is set to specified decoding
        ///     attribute.
        /// </summary>
        public AcrNemaFile(Stream stream, bool useStrictDecoding)
        {
            IsStrictDecoded = useStrictDecoding;
            LoadFrom(stream);
        }

        /// <summary>
        ///     Creates an ACR-NEMA file instance from an ACR-NEMA file defined
        ///     by file name.
        /// </summary>
        public AcrNemaFile(string fileName): this(fileName, true) {}

        /// <summary>
        ///     Creates an ACR-NEMA file instance from an ACR-NEMA file defined
        ///     by file name. <see cref="IsStrictDecoded" /> is set to given
        ///     decoding attribute.
        /// </summary>
        public AcrNemaFile(string fileName, bool useStrictDecoding)
        {
            IsStrictDecoded = useStrictDecoding;
            FileStream fileStream = new FileStream(fileName, FileMode.Open, 
                FileAccess.Read);
            try
            {
                LoadFrom(fileStream);
            }
            finally
            {
                fileStream.Close();
            }
        }

        /// <summary>
        ///     Creates an ACR-NEMA file instance from an ACR-NEMA file defined
        ///     by file name. <see cref="IsStrictDecoded" /> is set to given
        ///     decoding attribute. If memory preloading is activated, the
        ///     entire file content will be written to memory and afterwards
        ///     processed.
        /// </summary>
        public AcrNemaFile(string fileName, bool preloadToMemory, 
            bool useStrictDecoding)
        {
            IsStrictDecoded = useStrictDecoding;
            FileStream fileStream = new FileStream(fileName, FileMode.Open, 
                FileAccess.Read);
            byte[] buffer = new byte[fileStream.Length];
            try
            {
                fileStream.Read(buffer, 0, buffer.Length);
            }
            finally
            {
                fileStream.Close();
            }
            MemoryStream memoryStream = new MemoryStream(buffer);
            try
            {
                LoadFrom(memoryStream);
            }
            finally
            {
                memoryStream.Close();
            }
        }

        /// <summary>
        ///     Determines whether a specified file is an ACR-NEMA file.
        /// </summary>
        public static bool IsAcrNemaFile(string fileName)
        {
            FileStream fileStream = new FileStream(fileName, FileMode.Open, 
                FileAccess.Read);
            bool isStrictDecoded = ValueRepresentation.IsStrictDecoded;
            ValueRepresentation.IsStrictDecoded = false;
            try
            {
                Tag tag = new Tag(fileStream, TransferSyntax.Default);
                ValueRepresentation vr = 
                    ValueRepresentation.LoadFrom(fileStream, tag);
                if (vr.IsUndefined) return false;
                ValueLength valueLength = new ValueLength(fileStream, vr);
                Value value = null;
                if (fileStream.Position + valueLength.Value <= fileStream.Length)
                    value = new Value(fileStream, vr, valueLength);
                else
                    return false;
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
            finally
            {
                fileStream.Close();
                ValueRepresentation.IsStrictDecoded = isStrictDecoded;
            }
        }

        /// <summary>
        ///     Re-creates an ACR-NEMA file instance from given ACR-NEMA output
        ///     stream.
        /// </summary>
        public virtual void LoadFrom(Stream stream)
        {
            dataSet = new DataSet(stream);
            pixelData = new PixelData(dataSet);
        }

        /// <summary>
        ///     Concatenates all DICOM or ACR-NEMA data sets to one and returns it.
        ///     Aim of this method is to ease the use of <see cref="AcrNemaFile" />
        ///     and <see cref="DicomFile" /> without differentiation.
        /// </summary>
        /// <remarks>
        ///     If an exception occurres, because of duplicate key ids
        ///     (DICOM tags), DICOM tag uniqueness overall data sets
        ///     will not be given. This exception is not supposed to be
        ///     thrown. DICOM data sets for concatenation are supposed to
        ///     complement one another.
        /// </remarks>
        public virtual DataSet GetJointDataSets()
        {
            return dataSet;
        }
    }

}
