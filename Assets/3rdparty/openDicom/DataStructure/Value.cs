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


    $Id: Value.cs 54 2007-03-29 14:49:35Z agnandt $
*/
using System;
using System.IO;
using System.Collections;
using openDicom;
using openDicom.DataStructure.DataSet;
using openDicom.Encoding;
using openDicom.Encoding.Type;


namespace openDicom.DataStructure
{

    /// <summary>
    ///     This class represents a DICOM value.
    /// </summary>
    public sealed class Value: IComparable, IDicomStreamMember
    {
        private ArrayList valueList = new ArrayList();

        /// <summary>
        ///     Access this DICOM value instance as array.
        /// </summary>
        public object this[int index]
        {
            get { return valueList[index]; }
        }

        /// <summary>
        ///     Returns the count of single values within a DICOM value instance.
        /// </summary>
        public int Count
        {
            get { return valueList.Count; }
        }

        /// <summary>
        ///     Returns whether this DICOM value instance contains any values.
        /// </summary>
        public bool IsEmpty
        {
            get { return Count == 0; }
        }

        /// <summary>
        ///     Returns whether this DICOM value instance deals with undefined
        ///     value representation or value types. In this case, only values
        ///     as byte arrays are expected.
        /// </summary>
        public bool IsUndefined
        {
            get { return VR.IsUndefined; }
        }

        /// <summary>
        ///     Returns whether this DICOM value instance contains of
        ///     multiple values.
        /// </summary>
        public bool IsMultiValue
        {
            get { return Count > 1; }
        }

        /// <summary>
        ///     Returns whether this DICOM value instance deals with DICOM
        ///     VR Unknown (UN) values. In this case, only values as byte
        ///     arrays are expected.
        /// </summary>
        public bool IsUnknown
        {
            get { return VR.Name.Equals("UN"); }
        }

        /// <summary>
        ///     Returns whether this DICOM value instance contains values
        ///     that are build up of a DICOM <see cref="Sequence" />
        /// </summary>
        public bool IsSequence
        {
            get 
            { 
                return VR.Name.Equals("SQ") || 
                    (ValueLength.IsUndefined && IsPixelData); 
            }
        }

        /// <summary>
        ///     Returns whether this DICOM value instance contains values
        ///     that are build up of a DICOM <see cref="NestedDataSet" />
        /// </summary>
        public bool IsNestedDataSet
        {
            get 
            { 
                return ValueLength.IsUndefined && VR.Tag.Equals("(FFFE,E000)"); 
            }
        }

        /// <summary>
        ///     Returns whether this DICOM value instance contains a value
        ///     that is DICOM pixel data. This value can be processed with
        ///     help of <see cref="openDicom.Image.PixelData" />.
        /// </summary>
        public bool IsPixelData
        {
            get { return VR.Tag.Equals("(7FE0,0010)"); }
        }

        /// <summary>
        ///     Returns whether this DICOM value instance contains values
        ///     of type string.
        /// </summary>
        public bool IsString
        {
            get 
            { 
                return VR.Name.Equals("AE") || VR.Name.Equals("CS") ||
                    VR.Name.Equals("LO") || VR.Name.Equals("LT") ||
                    VR.Name.Equals("SH") || VR.Name.Equals("ST") ||
                    VR.Name.Equals("UI") || VR.Name.Equals("UT");
            }
        }

        /// <summary>
        ///     Returns whether this DICOM value instance contains values
        ///     of a numeric type like int or float. Easy processing might
        ///     be given by <see cref="Decimal" />.
        /// </summary>
        public bool IsNumeric
        {
            get 
            { 
                return VR.Name.Equals("DS") || VR.Name.Equals("FL") ||
                    VR.Name.Equals("FD") || VR.Name.Equals("IS") ||
                    VR.Name.Equals("OF") || VR.Name.Equals("OW") ||
                    VR.Name.Equals("SS") || VR.Name.Equals("SL") ||
                    VR.Name.Equals("UL") || VR.Name.Equals("US");
            }
        }

        /// <summary>
        ///     Returns whether this DICOM value instance contains values
        ///     of type DICOM <see cref="Tag" />.
        /// </summary>
        public bool IsTag
        {
            get { return VR.Name.Equals("AT"); }
        }

        /// <summary>
        ///     Returns whether this DICOM value instance contains values
        ///     of type DICOM <see cref="openDicom.Registry.Uid" />.
        /// </summary>
        public bool IsUid
        {
            get { return VR.Name.Equals("UI"); }
        }

        /// <summary>
        ///     Returns whether this DICOM value instance contains values
        ///     of type <see cref="openDicom.Encoding.Type.PersonName" />.
        /// </summary>
        public bool IsPersonName
        {
            get { return VR.Name.Equals("PN"); }
        }

        /// <summary>
        ///     Returns whether this DICOM value instance contains values
        ///     of type <see cref="Age" />.
        /// </summary>
        public bool IsAge
        {
            get { return VR.Name.Equals("AS"); }
        }

        /// <summary>
        ///     Returns whether this DICOM value instance contains values
        ///     that are dates. This values can be understood as
        ///     <see cref="System.DateTime" />.
        /// </summary>
        public bool IsDate
        {
            get 
            { 
                return VR.Name.Equals("DA") || VR.Name.Equals("DT");
            }
        }

        /// <summary>
        ///     Returns whether this DICOM value instance contains values
        ///     that are times. This values can be understood as 
        ///     <see cref="TimeSpan" />.
        /// </summary>
        public bool IsTime
        {
            get { return VR.Name.Equals("TM"); }
        }

        /// <summary>
        ///     Returns whether this DICOM value instance contains values
        ///     of type string. Be careful! Do not confuse with
        ///     <see cref="IsMultiValue" />! This property concerns
        ///     a single value entry.
        /// </summary>
        public bool IsArray
        {
            get 
            { 
                return (VR.Name.Equals("OB") || VR.Name.Equals("OW") || 
                    VR.Name.Equals("UN") || VR.IsUndefined) && 
                    ! IsSequence && ! IsNestedDataSet;
            }
        }


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
                    throw new DicomException("Value.VR is null.", (Tag) null);
            }
        }

        private ValueLength valueLength = null;
        /// <summary>
        ///     Access corresponding DICOM value length.
        /// </summary>
        public ValueLength ValueLength
        {
            get 
            {
                if (valueLength != null)
                    return valueLength;
                else
                    throw new DicomException("Value.ValueLength is null.",
                        this.VR.Tag);
            }
        }

        /// <summary>
        ///     Access corresponding DICOM transfer syntax.
        /// </summary>
        public TransferSyntax TransferSyntax
        {
            get { return VR.TransferSyntax; }
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


        public Value(ValueRepresentation vr, ValueLength length)
        {
            this.vr = vr;
            valueLength = length;
        }

        public Value(ValueRepresentation vr, ValueLength length, 
            object value): this(vr, length)
        {
            Add(value);
        }

        public Value(ValueRepresentation vr, ValueLength length, 
            Array multiValue): this(vr, length)
        {
            Add(multiValue);
        }

        /// <summary>
        ///     Re-creates this DICOM value length instance from specified DICOM
        ///     output stream using specified DICOM VR and value length.
        /// </summary>
        public Value(Stream stream, ValueRepresentation vr, ValueLength length):
            this(vr, length)
        {
            LoadFrom(stream);
        }

        /// <summary>
        ///     Re-creates this DICOM value instance from specified DICOM
        ///     output stream using <see cref="Value.TransferSyntax" />.
        /// </summary>
        public void LoadFrom(Stream stream)
        {
            streamPosition = stream.Position;
            DicomContext.Set(stream, VR.Tag);
            if (ValueLength.IsUndefined)
            {
                // use of delimitation (undefined length)
                if (IsSequence)
                {
                    // sequence delimitation
                    Sequence sq = new Sequence(stream, TransferSyntax);
                    Add(sq);
                }
                else if (IsNestedDataSet)
                {
                    // item delimitation
                    NestedDataSet ds = new NestedDataSet(stream, 
                        TransferSyntax);
                    Add(ds);
                }
                else
                    throw new DicomException(
                        "Value length is undefined but value is whether " +
                        "sequence nor data set.", this.VR.Tag);
            }
            else
            {
                if (stream.Position + valueLength.Value <= stream.Length)
                {
                    // use of length value
                    byte[] buffer = new byte[ValueLength.Value];
                    stream.Read(buffer, 0, ValueLength.Value);
                    Array multiValue = VR.Decode(buffer);
                    Add(multiValue);
                }
                else
                    throw new DicomException("Value length is out of bounds.",
                        "Value/stream.Length, Value/ValueLength.Value", 
                        stream.Length.ToString() + ", " + 
                        ValueLength.Value.ToString());
            }
            DicomContext.Reset();
        }

        /// <summary>
        ///     Adds a single value to a DICOM value instance.
        /// </summary>
        public void Add(object value)
        {
            if (valueList.Count > 0)
            {
                if (valueList[0].GetType().Equals(value.GetType()))
                    valueList.Add(value);
                else
                    throw new DicomException("Only values of the same type " +
                        "are allowed to be added.", VR.Tag, "value", 
                        value.GetType().ToString());
            }
            else
                valueList.Add(value);
        }

        /// <summary>
        ///     Adds an array of values to a DICOM value instance. If the
        ///     specified multiple value is a byte array, the multiple value
        ///     is understood as a single entry.
        /// </summary>
        public void Add(Array multiValue)
        {
            if (multiValue is byte[])
                Add((object) multiValue);
            else
                foreach (object item in multiValue)
                    Add(item);
        }

        /// <summary>
        ///     Needed by C# foreach-statements. Makes life easier.
        /// </summary>
        public IEnumerator GetEnumerator()
        {
            return valueList.GetEnumerator();
        }

        /// <summary>
        ///     Returns all value entries of a DICOM value instance as array.
        /// </summary>
        public object[] ToArray()
        {
            return valueList.ToArray();
        }

        /// <summary>
        ///     Implementation of the IComparable interface. So the use
        ///     of this instance within collections is guaranteed.
        /// </summary>
        public int CompareTo(object obj)
        {
            return ToString().CompareTo(((Value) obj).ToString());
        }

        // TODO: Value.ToString() implementation!
    }

}
