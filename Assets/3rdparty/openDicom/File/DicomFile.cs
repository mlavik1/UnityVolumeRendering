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


    $Id: DicomFile.cs 48 2007-03-28 13:49:15Z agnandt $
*/
using System;
using System.IO;
using openDicom;
using openDicom.Registry;
using openDicom.DataStructure;
using openDicom.DataStructure.DataSet;
using openDicom.Encoding;
using openDicom.Image;


namespace openDicom.File
{

    /// <summary>
    ///     This class represents a DICOM file.
    /// </summary>
    public class DicomFile: AcrNemaFile
    {
        private FileMetaInformation metaInformation = null;
        /// <summary>
        ///     DICOM file meta information. This is a DICOM data set.
        /// </summary>
        public FileMetaInformation MetaInformation
        {
            get 
            { 
                if (metaInformation != null)
                    return metaInformation; 
                else
                    throw new DicomException("DicomFile.MetaInformation is null.");
            }
        }


        /// <summary>
        ///     Creates a DICOM file instance from any DICOM output stream.
        /// </summary>
        public DicomFile(Stream stream): base(stream) {}

        /// <summary>
        ///     Creates a DICOM file instance from any DICOM output stream.
        ///     <see cref="IsStrictDecoded" /> is set to specified decoding
        ///     attribute.
        /// </summary>
        public DicomFile(Stream stream, bool useStrictDecoding): 
            base(stream, useStrictDecoding) {}

        /// <summary>
        ///     Creates a DICOM file instance from a DICOM file.
        /// </summary>
        public DicomFile(string fileName): base(fileName) {}

        /// <summary>
        ///     Creates a DICOM file instance from a DICOM file.
        ///     <see cref="IsStrictDecoded" /> is set to given decoding
        ///     attribute.
        /// </summary>
        public DicomFile(string fileName, bool useStrictDecoding): 
            base(fileName, useStrictDecoding) {}

        /// <summary>
        ///     Creates a DICOM file instance from a DICOM file.
        ///     <see cref="IsStrictDecoded" /> is set to given decoding
        ///     attribute. If memory preloading is activated, the
        ///     entire file content will be written to memory and afterwards
        ///     processed.
        /// </summary>
        public DicomFile(string fileName, bool preloadToMemory, 
            bool useStrictDecoding): 
            base(fileName, preloadToMemory, useStrictDecoding) {}

        /// <summary>
        ///     Determines whether a specified file is a DICOM file.
        /// </summary>
        public static bool IsDicomFile(string fileName)
        {
            FileStream fileStream = new FileStream(fileName, FileMode.Open, 
                FileAccess.Read);
            try
            {
                if (fileStream.Length > 132)
                {
                    byte[] buffer = new byte[132];
                    fileStream.Read(buffer, 0, 132);
                    string dicomPrefix = 
                        TransferSyntax.FileMetaInformation
                            .ToString(buffer, 128, 4);
                    return dicomPrefix.Equals(FileMetaInformation.DicomPrefix);
                }
                else return false;
            }
            finally
            {
                fileStream.Close();
            }
        }

        /// <summary>
        ///     Re-creates a DICOM file instance from specified DICOM output
        ///     stream.
        /// </summary>
        public override void LoadFrom(Stream stream)
        {
            metaInformation = new FileMetaInformation(stream);
            DataElement transferSyntaxDataElement = 
                MetaInformation[TransferSyntax.UidTag];
            Uid uid = (Uid) transferSyntaxDataElement.Value[0];
            dataSet = new DataSet(stream, new TransferSyntax(uid));
            pixelData = new PixelData(GetJointDataSets());
        }


        /// <summary>
        ///     Concatenates DICOM file meta information and DICOM data set to one
        ///     and returns it. Aim of this method is to ease the use of
        ///     <see cref="AcrNemaFile" /> and <see cref="DicomFile" /> without
        ///     differentiation. But be careful! Resulting data set will always
        ///     use the default transfer syntax!
        /// </summary>
        /// <remarks>
        ///     If an exception occurres, because of duplicate key ids
        ///     (DICOM tags), DICOM tag uniqueness overall data sets
        ///     will not be given. This exception is not supposed to be
        ///     thrown. DICOM data sets for concatenation are supposed to
        ///     complement one another. The differences in transfer syntaxes
        ///     cannot be processed. Thus, the default transfer syntax is
        ///     defined for concatenation.
        /// </remarks>
        public override DataSet GetJointDataSets()
        {
            DataSet dataSet = new DataSet();
            dataSet.Add(MetaInformation);
            dataSet.Add(DataSet);
            return dataSet;
        }
    }

}
