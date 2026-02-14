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


    $Id: DicomContext.cs 48 2007-03-28 13:49:15Z agnandt $
*/
using System;
using System.IO;
using openDicom.DataStructure;
using openDicom.Registry;
using openDicom.Encoding;


namespace openDicom
{

    /// <summary>
    ///     Global context for DICOM exception.
    /// </summary>
    public sealed class DicomContext
    {
        /// <summary>
        ///     Access of the global DICOM data element dictionary instance.
        /// </summary>
        public static DataElementDictionary DataElementDictionary
        {
            set { DataElementDictionary.Global = value; }
            get { return DataElementDictionary.Global; }
        }

        /// <summary>
        ///     Access of the global DICOM UID dictionary instance.
        /// </summary>
        public static UidDictionary UidDictionary
        {
            set { UidDictionary.Global = value; }
            get { return UidDictionary.Global; }
        }

        private static Tag currentTag = null;
        /// <summary>
        ///     Access of currently referenced DICOM tag.
        /// </summary>
        public static Tag CurrentTag
        {
            set { currentTag = value; }
            get { return currentTag; }
        }

        private static Stream baseStream = null;
        /// <summary>
        ///     Access of currently referenced DICOM stream.
        /// </summary>
        public static Stream BaseStream
        {
            set { baseStream = value; }
            get { return baseStream; }
        }

        /// <summary>
        ///     Return of current position within <see cref="BaseStream" />.
        /// </summary>
        /// <remarks>
        ///     If no DICOM stream is assigned to <see cref="BaseStream" />, 
        ///     -1 will be returned.
        /// </remarks>
        public static long StreamPosition
        {
            get
            {
                if (BaseStream != null)
                    return BaseStream.Position;
                else
                    return -1;
            }
        }

        /// <summary>
        ///     Assigns current DICOM tag and stream to specified instances.
        /// </summary>
        public static void Set(Stream baseStream, Tag currentTag)
        {
            BaseStream = baseStream;
            CurrentTag = currentTag;
        }

        /// <summary>
        ///     Assigns current DICOM tag and stream to null.
        /// </summary>
        public static void Reset()
        {
            CurrentTag = null;
            BaseStream = null;
        }
    }

}
