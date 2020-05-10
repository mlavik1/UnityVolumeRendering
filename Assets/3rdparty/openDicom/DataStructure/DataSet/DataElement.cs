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


    $Id: DataElement.cs 48 2007-03-28 13:49:15Z agnandt $
*/
using System;
using System.IO;
using openDicom.Registry;
using openDicom.DataStructure;
using openDicom.Encoding;


namespace openDicom.DataStructure.DataSet
{

    /// <summary>
    ///     This class represents a DICOM data element.
    /// </summary>
    public sealed class DataElement: IComparable, IDicomStreamMember
    {
        private Tag tag = null;
        /// <summary>
        ///     Data element tag.
        /// </summary>
        public Tag Tag
        {
            get 
            {
                if (tag != null) 
                    return tag; 
                else
                    throw new DicomException("DataElement.Tag is null.", 
                        (Tag) null);
            }
        }

        private ValueRepresentation vr = null;
        /// <summary>
        ///     Data element value representation.
        /// </summary>
        public ValueRepresentation VR
        {
            get 
            {
                if (vr != null) 
                    return vr; 
                else
                    throw new DicomException("DataElement.VR is null.",
                        this.Tag);
            }
        }

        private ValueLength valueLength = null;
        /// <summary>
        ///     Data element value length.
        /// </summary>
        public ValueLength ValueLength
        {
            get 
            {
                if (valueLength != null) 
                    return valueLength; 
                else
                    throw new DicomException("DataElement.ValueLength is null.",
                        this.Tag);
            }
        }

        private Value value = null;
        /// <summary>
        ///     Data element value.
        /// </summary>
        public Value Value
        {
            get 
            {
                if (value != null) 
                    return value; 
                else
                    throw new DicomException("DataElement.Value is null.",
                        this.Tag);
            }
        }

        /// <summary>
        ///     DICOM stream position of this data element instance.
        /// </summary>
        public long StreamPosition
        {
            get { return Tag.StreamPosition; }
        }

        private TransferSyntax transferSyntax = TransferSyntax.Default;
        /// <summary>
        ///     Transfer syntax of this data element instance.
        /// </summary>
        public TransferSyntax TransferSyntax
        {
            set
            {
                if (value == null)
                    transferSyntax = TransferSyntax.Default;
                else
                    transferSyntax = value;
            }

            get { return transferSyntax; }
        }

    
        /// <summary>
        ///     Creates a new data element instance from specified DICOM
        ///     output stream using the DICOM default transfer syntax.
        /// </summary>
        public DataElement(Stream stream): this(stream, null) {}

        /// <summary>
        ///     Creates a new data element instance from specified DICOM
        ///     output stream using specified DICOM transfer syntax.
        /// </summary>
        public DataElement(Stream stream, TransferSyntax transferSyntax)
        {
            TransferSyntax = transferSyntax;
            LoadFrom(stream);
        }

        public DataElement(string tag, string vr): this(tag, vr, null) {}

        public DataElement(string tag, string vr, TransferSyntax transferSyntax)
        {
            TransferSyntax = transferSyntax;
            this.tag = new Tag(tag, TransferSyntax);
            this.vr = ValueRepresentation.GetBy(vr, Tag);
        }

        public DataElement(Tag tag, ValueRepresentation vr)
        {
            this.tag = tag;
            TransferSyntax = Tag.TransferSyntax;
            if (vr == null) vr = ValueRepresentation.GetBy(Tag);
            this.vr = vr;
        }

        /// <summary>
        ///     Re-creates a data element instance from specified DICOM
        ///     output stream using <see cref="DataElement.TransferSyntax" />.
        /// </summary>
        public void LoadFrom(Stream stream)
        {
            tag = new Tag(stream, TransferSyntax);
            vr = ValueRepresentation.LoadFrom(stream, Tag);
            valueLength = new ValueLength(stream, VR);
            value = new Value(stream, VR, ValueLength);
        }

        /// <summary>
        ///     Implementation of the IComparable interface. So use
        ///     of this class within collections is guaranteed.
        /// </summary>
        public int CompareTo(object obj)
        {
            return Tag.CompareTo(((DataElement) obj).Tag);
        }
    }
    
}
