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


    $Id: VR.cs 48 2007-03-28 13:49:15Z agnandt $
*/
using System;
using System.IO;
using System.Text.RegularExpressions;
using openDicom;
using openDicom.DataStructure.DataSet;
using openDicom.Encoding;


namespace openDicom.DataStructure
{

    using DateTime = openDicom.Encoding.DateTime;

    /// <summary>
    ///     This class represents an unspecific DICOM value representation (VR).
    /// </summary>
    public class ValueRepresentation: IDicomStreamMember
    {
        private string vrName = "";
        /// <summary>
        ///     Access the string representation of this instance. The string
        ///     representation is a two characters string of format "VR" and
        ///     is an abbreviation for the type of DICOM value representation.
        ///     If a value representation is unspecified, an empty string will
        ///     be returned.
        /// </summary>
        public string Name
        {
            get { return vrName; }
        }

        private Tag tag = null;
        /// <summary>
        ///     Access corresponding DICOM tag.
        /// </summary>
        public Tag Tag
        {
            get 
            {
                if (tag != null)
                    return tag;
                else
                    throw new DicomException(
                        "ValueRepresentation.Tag is null.", (Tag) null);
            }
        }

        /// <summary>
        ///     Access corresponding DICOM transfer syntax.
        /// </summary>
        public TransferSyntax TransferSyntax
        {
            get { return Tag.TransferSyntax; }
        }

        /// <summary>
        ///     Returns whether this instance is an implicit VR, a value 
        ///     representation without corresponding DICOM data stream entry.
        ///     Therefore, the transfer syntax is considered.
        /// </summary>
        public bool IsImplicit
        {
            get
            {
                if (IsUndefined)
                {
                    if (Tag.IsUserDefined)
                        // no data dictionary entry exists
                        return TransferSyntax.IsImplicitVR;
                    else
                        // use data set and sequence delimiters
                        return true;
                }
                else
                    return TransferSyntax.IsImplicitVR;
            }
        }

        /// <summary>
        ///     Returns whether this instance is a defined or undefined
        ///     value representation. An undefined value representation
        ///     also is an unspecific value representation. Attention: The value
        ///     representation UN (Unknown) is not an undefined value
        ///     representation and thus specific. An undefined value 
        ///     representation is represented by an empty string.
        /// </summary>
        public bool IsUndefined
        {
            get { return Name.Equals(""); }
        }

        /// <summary>
        ///     Returns true, if this value representation instance is unknown (UN).
        /// </summary>
        public bool IsUnknown
        {
            get { return Name.Equals("UN"); }
        }

        private static bool isStrictDecoded = true;
        /// <summary>
        ///     Global switch for controlling strictness of DICOM content
        ///     decoding. If this switch is set to false, a lot of conditions
        ///     within <see cref="ValueRepresentation.Decode" /> will be ignored.
        /// </summary>
        public static bool IsStrictDecoded
        {
            set { isStrictDecoded = value; }
            get { return isStrictDecoded; }
        }

        private long streamPosition = -1;
        /// <summary>
        ///     Position within a DICOM stream of this VR instance. If this
        ///     instance does not belong to a stream, -1 will be returned. This
        ///     will be the case, if this VR instance is registered in a
        ///     data element dictionary.
        /// </summary>
        public long StreamPosition
        {
            get { return streamPosition; }
        }

        /// <summary>
        ///     Creates a new value representation instance and registers it
        ///     by the defined tag.
        /// </summary>
        /// <param name="tag">
        ///     DICOM tag.
        /// </param>
        public ValueRepresentation(Tag tag): this((string) null, tag) {}
              
        /// <summary>
        ///     Creates a new value representation instance from its string 
        ///     representation and registers it by the defined tag.
        /// </summary>
        /// <param name="vr">
        ///     DICOM string value representation of format "VR".
        /// </param>
        /// <param name="tag">
        ///     DICOM tag.
        /// </param>
        public ValueRepresentation(string vr, Tag tag)
        {
            if (vr == null)
                vrName = "";
            else
                vrName = vr.Trim().ToUpper();
            if ( ! (Regex.IsMatch(vrName, 
                "^(AE|AS|AT|CS|DA|DS|DT|FL|FD|IS|LO|LT|OB|OF|OW|PN|SH|SL|SQ|" +
                "SS|ST|TM|UI|UL|UN|US|UT)$") || vrName.Equals("")))
               throw new DicomException(
                   "Value representation is not valid.", "vr", vr); 
            this.tag = tag;
        }

        /// <summary>
        ///     Creates a new VR instance form a DICOM output stream and
        ///     registers it by the specified tag.
        /// </summary>
        public ValueRepresentation(Stream stream, Tag tag)
        {
            this.tag = tag;
            LoadFrom(stream);
        }

        /// <summary>
        ///     Returns a new value representation instance registered by the
        ///     defined tag.
        /// </summary>
        /// <param name="tag">
        ///     DICOM tag.
        /// </param>
        /// <returns>
        ///     New undefined DICOM VR.
        /// </returns>
        public static ValueRepresentation GetBy(Tag tag)
        {
            return GetBy(null, tag);
        }

        /// <summary>
        ///     Returns a new value representation instance specified by the
        ///     defined string representation and registered by the defined tag.
        /// </summary>
        /// <param name="name">
        ///     DICOM string value representation of format "VR". If 'name'
        ///     is not a known DICOM VR, an undefined VR will be returned.
        /// </param>
        /// <param name="tag">
        ///     DICOM tag.
        /// </param>
        /// <returns>
        ///     New specific DICOM VR or a new unspecific VR.
        /// </returns>
        public static ValueRepresentation GetBy(string name, Tag tag)
        {
            switch (name)
            {
                case "AE": return new ApplicationEntity(tag); break;
                case "AS": return new AgeString(tag); break;
                case "AT": return new AttributeTag(tag); break;
                case "CS": return new CodeString(tag); break;
                case "DA": return new Date(tag); break;
                case "DS": return new DecimalString(tag); break;
                case "DT": return new DateTime(tag); break;
                case "FL": return new FloatingPointSingle(tag); break;
                case "FD": return new FloatingPointDouble(tag); break;
                case "IS": return new IntegerString(tag); break;
                case "LO": return new LongString(tag); break;
                case "LT": return new LongText(tag); break;
                case "OB": return new OtherByteString(tag); break;
                case "OF": return new OtherFloatString(tag); break;
                case "OW": return new OtherWordString(tag); break;
                case "PN": return new PersonName(tag); break;
                case "SH": return new ShortName(tag); break;
                case "SL": return new SignedLong(tag); break;
                case "SQ": return new SequenceOfItems(tag); break;
                case "SS": return new SignedShort(tag); break;
                case "ST": return new ShortText(tag); break;
                case "TM": return new Time(tag); break;
                case "UI": return new UniqueIdentifier(tag); break;
                case "UL": return new UnsignedLong(tag); break;
                case "UN": return new Unknown(tag); break;
                case "US": return new UnsignedShort(tag); break;
                case "UT": return new UnlimitedText(tag); break;
                case null: 
                case "": return new ValueRepresentation(tag); break;
                default:
                    throw new DicomException(
                        "Value representation is not valid.", "name", name);
                    break;
            }            
        }

        /// <summary>
        ///     The same as <see cref="IsImplicit" />, but in a static manner.
        /// </summary>
        public static bool IsImplicitBy(Tag tag)
        {
            if (tag.GetDictionaryEntry().VR.IsUndefined)
            {
                if (tag.IsUserDefined)
                    // no data dictionary entry exists
                    return tag.TransferSyntax.IsImplicitVR;
                else
                    // use data set and sequence delimiters
                    return true;
            }
            else
                return tag.TransferSyntax.IsImplicitVR;
        }

        /// <summary>
        ///     Re-creates this instance from a DICOM output stream.
        /// </summary>
        public void LoadFrom(Stream stream)
        {
            streamPosition = stream.Position;
            DicomContext.Set(stream, tag);
            if (IsImplicit)
            {
                if (Tag.IsUserDefined)
                    // implicit but unknown value representation
                    vrName = "UN";
                else
                    // implicit but known value representation;
                    // return new instance, dictionary entry do not have a
                    // transfer syntax
                    vrName = Tag.GetDictionaryEntry().VR.Name;
            }
            else
            {
                // explicit value representation
                byte[] buffer = new byte[2];
                stream.Read(buffer, 0, 2);
                vrName = ByteConvert.ToString(buffer, 
                    CharacterRepertoire.Default);
            }
            DicomContext.Reset();
        }

        /// <summary>
        ///     Creates a new VR instance from a DICOM output stream.
        /// </summary>
        /// <param name="stream">
        ///     Any kind of DICOM output stream.
        /// </param>
        /// <returns>
        ///     Output stream position of this instance.
        /// </returns>
        public static ValueRepresentation LoadFrom(Stream stream, Tag tag)
        {
            DicomContext.Set(stream, tag);
            if (IsImplicitBy(tag))
            {
                if (tag.IsUserDefined)
                    // implicit but unknown value representation
                    return GetBy("UN", tag);
                else
                    // implicit but known value representation;
                    // return new instance, dictionary entry do not have a
                    // transfer syntax
                    return GetBy(tag.GetDictionaryEntry().VR.Name, tag);            
            }
            else
            {
                // explicit value representation
                byte[] buffer = new byte[2];
                stream.Read(buffer, 0, 2);
                string name = ByteConvert.ToString(buffer,
                    CharacterRepertoire.Default);
                return GetBy(name, tag);
            }
            DicomContext.Reset();           
        }

        /// <summary>
        ///     DICOM VR string representation.
        /// </summary>
        /// <returns>
        ///     A DICOM VR as string of the format "VR". If this instance
        ///     is an undefined VR, an empty string will be returned.
        /// </returns>                        
        public override string ToString()
        {
            return Name;
        }
        
        /// <summary>
        ///     DICOM VR detailed string representation.
        /// </summary>
        /// <returns>
        ///     A DICOM VR as string of the format "Value Representation (VR)".
        /// </returns>                        
        public virtual string ToLongString()
        {
            if ( ! Name.Equals(""))
                return "Undefined (" + Name + ")";
            else
                return "Undefined";
        }

        protected byte[][] ToImproperMultiValue(byte[] jointMultiValue,
            int valueLength)
        {
            byte[][] result = null;
            if (jointMultiValue.Length > valueLength)
            {
                int count = (int) Math.Floor((double) jointMultiValue.Length / valueLength);
                result = new byte[count][];
                for (int i = 0; i < count; i++)
                {
                    result[i] = new byte[valueLength];
                    Array.Copy(jointMultiValue, i * valueLength, result[i], 0,
                        valueLength);
                }
            }
            else
            {
                if (jointMultiValue.Length > 0)
                    result = new byte[1][] {jointMultiValue};
                else
                    result = new byte[0][];
            }
            return result;
        }

        protected byte[][] ToProperMultiValue(byte[] jointMultiValue,
            int valueLength)
        {
            byte[][] result = null;
            if (jointMultiValue.Length > valueLength)
            {
                int count = 0;
                if (jointMultiValue.Length % valueLength == 0)
                    count = jointMultiValue.Length / valueLength;
                else
                    throw new EncodingException(
                        "Joint multi value cannot be seperated into single " +
                        "multi values by the specified value length.",
                        Name + "/jointMultiValue.Length, " + Name + 
                        "/valueLength", jointMultiValue.Length.ToString() + 
                        ", " + valueLength.ToString());
                if (jointMultiValue.Length > 0 &&
                    ! Tag.GetDictionaryEntry().VM.IsValid(count))
                    throw new EncodingException("Count of values is invalid.",
                        Tag, Name + "/VM, " + Name + "/count", 
                        Tag.GetDictionaryEntry().VM + ", " + 
                        count.ToString());
                result = new byte[count][];
                for (int i = 0; i < count; i++)
                {
                    result[i] = new byte[valueLength];
                    Array.Copy(jointMultiValue, i * valueLength, result[i], 0,
                        valueLength);
                }
            }
            else
                result = new byte[1][] {jointMultiValue};
            return result;
        }

        protected byte[] ToJointMultiValue(byte[][] multiValue)
        {
            int jointLength = 0;
            foreach (byte[] value in multiValue)
                jointLength += value.Length;
            byte[] result = new byte[jointLength];
            int resultIndex = 0;
            int multiValueIndex = 0;
            while (multiValueIndex < multiValue.Length)
            {
                byte[] value = multiValue[multiValueIndex];
                Array.Copy(value, 0, result, resultIndex, value.Length);
                resultIndex += value.Length;
                multiValueIndex++;
            }
            if (result.Length == 0 ||
                Tag.GetDictionaryEntry().VM.IsValid(multiValue.Length))
                return result;
            else
                throw new EncodingException("Count of values is invalid.",
                    Tag, Name + "/VM, " + Name + "/multiValue.Length", 
                    Tag.GetDictionaryEntry().VM + ", " + 
                    multiValue.Length.ToString());
        }

        protected string[] ToImproperMultiValue(string jointMultiValue)
        {
            string[] result = jointMultiValue.Split('\\');
            return result;
        }

        protected string[] ToProperMultiValue(string jointMultiValue)
        {
            string[] result = jointMultiValue.Split('\\');
            if (jointMultiValue.Length == 0 ||
                Tag.GetDictionaryEntry().VM.IsValid(result.Length))
                return result;
            else
                throw new EncodingException("Count of values is invalid.",
                    Tag, Name + "/VM, " + Name + "/result.Length", 
                    Tag.GetDictionaryEntry().VM + ", " + 
                    result.Length.ToString());
        }

        protected string ToJointMultiValue(string[] multiValue)
        {
            string result = "";
            foreach (string value in multiValue)
            {
                if (result.Equals("")) result = value;
                else result += "\\" + value;
            }
            if (result.Length == 0 ||
                Tag.GetDictionaryEntry().VM.IsValid(multiValue.Length))
                return result;
            else
                throw new EncodingException("Count of values is invalid.",
                    Tag, Name + "/VM, " + Name + "/multiValue.Length", 
                    Tag.GetDictionaryEntry().VM + ", " + 
                    multiValue.Length.ToString());
        }

        protected virtual Array DecodeProper(byte[] bytes)
        {
            return new byte[1][] { bytes };
        }

        protected virtual Array DecodeImproper(byte[] bytes)
        {
            return new byte[1][] { bytes };
        }
        
        /// <summary>
        ///     Determines the correct type and multiplicity of a DICOM value.
        /// </summary>
        /// <remarks>
        ///     This method is overwritten by all specific DICOM VR
        ///     implementations.
        /// </remarks>
        /// <param name="bytes">
        ///     DICOM byte array.
        /// </param>
        /// <returns>
        ///     DICOM value as array of a specific type.
        /// </returns>                        
        public Array Decode(byte[] bytes)
        {
            if (IsStrictDecoded)
                return DecodeProper(bytes);
            else
                return DecodeImproper(bytes);
        }

        /// <summary>
        ///     Determines the correct type and multiplicity of a DICOM value
        ///     and converts it to a DICOM value class.
        /// </summary>
        /// <param name="bytes">
        ///     DICOM byte array.
        /// </param>
        /// <returns>
        ///     DICOM Value class instance.
        /// </returns>                        
        public Value DecodeToValue(byte[] bytes)
        {
            return new Value(this, new ValueLength(this, bytes.Length),
                Decode(bytes));
        }
    }

}
