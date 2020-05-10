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


    $Id: NestedDataSet.cs 48 2007-03-28 13:49:15Z agnandt $
*/
using System;
using System.IO;
using openDicom.DataStructure;
using openDicom.Encoding;


namespace openDicom.DataStructure.DataSet
{

    /// <summary>
    ///     This class represents a DICOM nested data set.
    /// </summary>
    public sealed class NestedDataSet: DataSet
    {
        /// <summary>
        ///     DICOM tag (FFFE,E00D).
        /// </summary>
        public new static readonly Tag DelimiterTag = new Tag("FFFE", "E00D");


        /// <summary>
        ///     Creates a new DICOM nested data set instance from specified
        ///     DICOM output stream using the default transfer syntax.
        /// </summary>
        public NestedDataSet(Stream stream): base(stream) {}

        /// <summary>
        ///     Creates a new DICOM nested data set instance from specified
        ///     DICOM output stream using specified transfer syntax.
        /// </summary>
        public NestedDataSet(Stream stream, TransferSyntax transferSyntax):
            base(stream, transferSyntax) {}

        /// <summary>
        ///     Re-creates a DICOM nested data set instance from specified
        ///     DICOM output stream using <see cref="Sequence.TransferSyntax" />.
        /// </summary>
        public override void LoadFrom(Stream stream)
        {
            streamPosition = stream.Position;
            DataElement element = new DataElement(stream, TransferSyntax);
            bool isTrailingPadding = false;
            while ( ! element.Tag.Equals(DelimiterTag) &&
                stream.Position < stream.Length)
            {
                isTrailingPadding = element.Tag.Equals("(0000,0000)");
                if ( ! isTrailingPadding)
                    Add(element);
                element = new DataElement(stream, TransferSyntax);
            }
        }
    }

}
