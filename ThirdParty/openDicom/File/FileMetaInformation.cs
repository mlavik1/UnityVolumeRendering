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


    $Id: FileMetaInformation.cs 48 2007-03-28 13:49:15Z agnandt $
*/
using System;
using System.IO;
using System.Collections;
using openDicom;
using openDicom.DataStructure.DataSet;
using openDicom.Encoding;


namespace openDicom.File
{

    /// <summary>
    ///     This class represents DICOM file meta information.
    /// </summary>
    public sealed class FileMetaInformation: DataSet
    {
        /// <summary>
        ///     DICOM file prefix (DICM).
        /// </summary>
        public static readonly string DicomPrefix = 
            ByteConvert.ToString(new byte[4] { 0x44, 0x49, 0x43, 0x4D },
                CharacterRepertoire.Ascii);

        private string filePreamble = "";
        /// <summary>
        ///     DICOM file preamble.
        /// </summary>
        /// <remarks>
        ///     Zero filled file preambles are returned as empty string.
        /// </remarks>
        public string FilePreamble
        {
            get { return filePreamble; }
        }


        /// <summary>
        ///     Creates a DICOM file meta information instance from a DICOM
        ///     output stream. The stream position is assumed to be the DICOM
        ///     output stream's start position (mostly zero).
        /// </summary>
        public FileMetaInformation(Stream stream):
            base(stream, TransferSyntax.FileMetaInformation) {}

        /// <summary>
        ///     Re-creates a DICOM file meta information instance from a DICOM
        ///     output stream. The output stream position is assumed to be the
        ///     DICOM stream's start position (mostly zero).
        /// </summary>
        public override void LoadFrom(Stream stream)
        {
            base.streamPosition = stream.Position;
            DicomContext.Set(stream, null);
            byte[] buffer = new byte[132];
            stream.Read(buffer, 0, 132);
            filePreamble = TransferSyntax.ToString(buffer, 128);
            filePreamble = filePreamble.Replace("\0", null);
            string dicomPrefix = TransferSyntax.ToString(buffer, 128, 4);
            if (dicomPrefix.Equals(DicomPrefix))
            {
                // group length
                DataElement element = new DataElement(stream, TransferSyntax);
                Add(element);
                uint groupLength = (uint) element.Value[0];
                long streamPosition = stream.Position;
                // consider omission of current stream position
                buffer = new byte[streamPosition + groupLength];
                stream.Read(buffer, (int) streamPosition, (int) groupLength);
                MemoryStream memoryStream = new MemoryStream(buffer);
                try
                {
                    memoryStream.Seek(streamPosition, 0);
                    base.LoadFrom(memoryStream);
                }
                finally
                {
                    memoryStream.Close();
                }
            }
            else
            {
                stream.Close();
                throw new DicomException("Working with a non-valid DICOM file.");
            }
            DicomContext.Reset();
        }

        /// <summary>
        ///     Clears all properties of a DICOM file meta information instance.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            filePreamble = "";
        }
    }

}
