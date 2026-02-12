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


    $Id: ValueLength.cs 48 2007-03-28 13:49:15Z agnandt $
*/
using System;
using System.IO;
using openDicom;
using openDicom.DataStructure.DataSet;
using openDicom.Encoding;


namespace openDicom.DataStructure
{

    /// <summary>
    ///     This class represents a DICOM value length.
    /// </summary>
    public sealed class ValueLength: IDicomStreamMember
    {
        private ValueRepresentation vr = null;
        /// <summary>
        ///     Access corresponding DICOM VR.
        /// </summary>
        public ValueRepresentation VR
        {
            get 
            { 
                if (vr != null)
                    return vr;
                else
                    throw new DicomException("ValueLength.VR is null.", 
                        (Tag) null);
            }
        }

        /// <summary>
        ///     Access corresponding DICOM transfer syntax.
        /// </summary>
        public TransferSyntax TransferSyntax
        {
            get { return VR.TransferSyntax; }
        }

        private int length = 0;
        /// <summary>
        ///     Access value length.
        /// </summary>
        public int Value
        {
            get { return length; }
        }

        /// <summary>
        ///     Returns whether this value length instance is negativ which is
        ///     an undefined state.
        /// </summary>
        public bool IsUndefined
        {
            get { return Value < 0; }
        }

        private long streamPosition = -1;
        /// <summary>
        ///     Returns this instance's position within a DICOM data stream. If
        ///     this instance does not belong to a stream, -1 will be returned.
        /// </summary>
        public long StreamPosition
        {
            get { return streamPosition; }
        }


        public ValueLength(ValueRepresentation vr)
        {
            this.vr = vr;
        }

        public ValueLength(ValueRepresentation vr, int length): this(vr)
        {
            this.length = length;
        }

        /// <summary>
        ///     Creates this DICOM value length instance from specified DICOM
        ///     output stream using specified DICOM VR.
        /// </summary>
        public ValueLength(Stream stream, ValueRepresentation vr): this(vr)
        {
            LoadFrom(stream);
        }

        /// <summary>
        ///     Re-creates this DICOM value length instance from specified DICOM
        ///     output stream using <see cref="ValueLength.TransferSyntax" />.
        /// </summary>
        public void LoadFrom(Stream stream)
        {
            streamPosition = stream.Position;
            DicomContext.Set(stream, VR.Tag);
            int count = 2;
            bool isCertainVR = VR.Name.Equals("OB") || VR.Name.Equals("OW") ||
                VR.Name.Equals("OF") || VR.Name.Equals("SQ") || 
                VR.Name.Equals("UT") || VR.Name.Equals("UN");
            if (isCertainVR && ! VR.IsImplicit)
            {
                // explicit value representation for certain VRs
                byte[] reserved = new byte[2];
                stream.Read(reserved, 0 , 2);
                if (TransferSyntax.CorrectByteOrdering(
                    BitConverter.ToUInt16(reserved, 0)) != 0)
                    throw new ArgumentException("Reserved 2 bytes block is " +
                        "not 0x0000.");
                count = 4;
            }
            else if (VR.IsImplicit)
                // implicit value representation
                count = 4;
            byte[] buffer = new byte[count];
            stream.Read(buffer, 0, count);
            if (count == 2)
            {
                length = TransferSyntax.CorrectByteOrdering(
                    BitConverter.ToUInt16(buffer, 0));
            }
            else
            {
                uint len = TransferSyntax.CorrectByteOrdering(
                    BitConverter.ToUInt32(buffer, 0));
                if (len == 0xFFFFFFFF)
                    // undefined length
                    length = -1;
                else if (len > int.MaxValue)
                    // casting problem from uint32 to int32
                    throw new DicomException("Value length is " +
                        "too big for this implementation.", this.VR.Tag, "len", 
                        len.ToString());
                else
                    length = (int) len;
            }
            DicomContext.Reset();
        }

        /// <summary>
        ///     Returns <see cref="ValueLength.Value" /> as string.
        /// </summary>
        public override string ToString()
        {
            return Value.ToString();
        }
    }

}
