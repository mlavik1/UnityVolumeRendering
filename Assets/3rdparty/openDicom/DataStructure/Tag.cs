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


    $Id: Tag.cs 48 2007-03-28 13:49:15Z agnandt $
*/
using System;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;
using openDicom;
using openDicom.Registry;
using openDicom.Encoding;


namespace openDicom.DataStructure
{

    /// <summary>
    ///     This class represents a DICOM tag (gggg,eeee).
    /// </summary>
    public sealed class Tag: IComparable, IDicomStreamMember
    {
        private string group = "0000";
        /// <summary>
        ///     Access of a tag instance's group id. Group is a hexadecimal
        ///     string value of format "gggg".
        /// </summary>
        public string Group
        {
            get { return group; }
            set 
            {
                if (value != null)
                {
                    if (Regex.IsMatch(value, "^[0-9A-Fa-f]{4}$"))
                        group = value.ToUpper();
                    else
                        throw new DicomException("Tag.Group is invalid.", 
                            this, "Group", value);
                }
                else
                    throw new DicomException("Tag.Group is null.", this);
            }
        }
        
        private string element = "0000";
        /// <summary>
        ///     Access of a tag instance's element id. Element is a hexadecimal
        ///     string value of format "eeee".
        /// </summary>
        public string Element
        {
            get { return element; }
            set 
            {
                if (value != null)
                { 
                    if (Regex.IsMatch(value, "^[0-9A-Fa-f]{4}$"))
                        element = value.ToUpper();
                    else
                        throw new DicomException("Tag.Element is invalid.",
                            this, "Element", value);
                }
                else
                    throw new DicomException("Tag.Element is null.");
            } 
        }

        /// <summary>
        ///     Returns true, if this tag is a private tag, a tag defined by 
        ///     user, else false. A private tag is not registered to a data
        ///     element dictionary.
        /// </summary>
        public bool IsUserDefined
        {
            get 
            { 
                return ! DataElementDictionary.Global.Contains(this);
            }
        }

        private long streamPosition = -1;
        /// <summary>
        ///     Access this instance's stream position. If a tag does
        ///     not belong to a stream, a position of -1 will be returned.
        /// </summary>
        public long StreamPosition
        {
            get { return streamPosition; }
        }

        private TransferSyntax transferSyntax = null;
        /// <summary>
        ///     Access this instance's transfer syntax.
        /// </summary>
        public TransferSyntax TransferSyntax
        {
            set { transferSyntax = value; }

            get 
            {
                if (transferSyntax != null)
                    return transferSyntax; 
                else
                    throw new DicomException("Transfer syntax is null. " +
                        "Make sure you are not referencing a dictionary " +
                        "entry tag.", "Tag.TransferSyntax");
            }
        }


        /// <summary>
        ///     Creates a new tag instance from specified string representation
        ///     tag.
        /// </summary>
        /// <param name="tag">
        ///     Tag representation as string of format "(gggg,eeee)".
        /// </param>
        public Tag(string tag): this(tag, (TransferSyntax) null) {}

        public Tag(string tag, TransferSyntax transferSyntax)
        {
            tag = tag.ToUpper().Replace(" ", null);
            if (Regex.IsMatch(tag, "^\\([0-9A-F]{4}\\,[0-9A-F]{4}\\)$"))
            {
                string[] s = tag.Split(',');
                Group = s[0].TrimStart('(').Trim();
                Element = s[1].TrimEnd(')').Trim();
                TransferSyntax = transferSyntax;
            }
            else 
                throw new DicomException("Tag is invalid.", this, "tag", tag); 
        }

        /// <summary>
        ///     Creates a new tag instance from specified string representations
        ///     (group,element).
        /// </summary>
        /// <param name="group">
        ///     Group id as string of format "gggg".
        /// </param>
        /// <param name="element">
        ///     Element id as string of format "eeee".
        /// </param>
        public Tag(string group, string element): this(group, element, null) {}

        public Tag(string group, string element, TransferSyntax transferSyntax)
        {
            Group = group;
            Element = element;
            TransferSyntax = transferSyntax;
        }

        /// <summary>
        ///     Creates a new tag instance from specified integer representations
        ///     (group,element).
        /// </summary>
        /// <param name="group">
        ///     Group id value.
        /// </param>
        /// <param name="element">
        ///     Element id value.
        /// </param>
        public Tag(int group, int element): this(group, element, null) {}

        public Tag(int group, int element, TransferSyntax transferSyntax)
        {
            TransferSyntax = transferSyntax;
            SetInt(group, element);
        }

        /// <summary>
        ///     Creates a new tag instance from a DICOM output stream.
        /// </summary>
        /// <param name="stream">
        ///     Any kind of DICOM output stream.
        /// </param>
        public Tag(Stream stream): this(stream, null) {}
        
        public Tag(Stream stream, TransferSyntax transferSyntax)
        {
            TransferSyntax = transferSyntax;
            LoadFrom(stream);
        }

        private void SetInt(int group, int element)
        {
            if (group < 0x0000 || group > 0xFFFF)
                throw new ArgumentOutOfRangeException("Tag.Group");
            if (element < 0x0000 || element > 0xFFFF)
                throw new ArgumentOutOfRangeException("Tag.Element");
            Group = string.Format("{0:X4}", group);
            Element = string.Format("{0:X4}", element);
        }

        /// <summary>
        ///     Re-creates this instance from a DICOM output stream.
        /// </summary>
        /// <param name="stream">
        ///     Any kind of DICOM output stream.
        /// </param>
        public void LoadFrom(Stream stream)
        {
            streamPosition = stream.Position;
            DicomContext.Set(stream, null);
            if (stream != null)
            {
                byte[] buffer = new byte[2];
                stream.Read(buffer, 0, 2);
                int group = BitConverter.ToUInt16(
                    TransferSyntax.CorrectByteOrdering(buffer), 0);
                stream.Read(buffer, 0, 2);
                int element = BitConverter.ToUInt16(
                    TransferSyntax.CorrectByteOrdering(buffer), 0);
                SetInt(group, element);
            }
            else
                throw new DicomException("Tag.LoadFrom.Stream is null.", 
                    this);
            DicomContext.Reset();
        }

        /// <summary>
        ///     Saves this instance to a DICOM input stream.
        /// </summary>
        /// <param name="stream">
        ///     Any kind of DICOM input stream.
        /// </param>
        public void SaveTo(Stream stream)
        {
            streamPosition = stream.Position;
            DicomContext.Set(stream, this);
            if (stream != null)
            {
                byte[] group = BitConverter.GetBytes(ushort.Parse(Group,
                    NumberStyles.HexNumber));
                byte[] element = BitConverter.GetBytes(ushort.Parse(Element, 
                    NumberStyles.HexNumber));
                group = TransferSyntax.CorrectByteOrdering(group);
                element = TransferSyntax.CorrectByteOrdering(element);
                byte[] buffer = new byte[4];
                Array.Copy(group, 0, buffer, 0, 2);
                Array.Copy(element, 0, buffer, 2, 2);
                stream.Write(buffer, 0, 4);
            }
            else
                throw new DicomException("Tag.SaveToStream.Stream is null.",
                    this);
            DicomContext.Reset();
        }

        /// <summary>
        ///     Determines whether this instance is equal to another DICOM
        ///     tag instance.
        /// </summary>
        /// <param name="tag">
        ///     An instance of class Tag.
        /// </param>
        /// <return>
        ///     True, if equality is given, else false.
        /// </return>
        public bool Equals(Tag tag)
        {
            return CompareTo(tag) == 0;
        }

        /// <summary>
        ///     Determines whether this instance is equal to a string
        ///     representation of the format "(gggg,eeee)".
        /// </summary>
        /// <param name="tag">
        ///     An instance of class Tag.
        /// </param>
        /// <returns>
        ///     True, if equality is given, else false.
        /// </returns>
        public bool Equals(string tag)
        {
            tag = tag.ToUpper().Replace(" ", null);
            if (Regex.IsMatch(tag, "^\\([0-9A-F]{4}\\,[0-9A-F]{4}\\)$"))
                return ToString().Equals(tag);
            else
                throw new DicomException("Tag is invalid.", this, "tag", tag); 
        }

        /// <summary>
        ///     Implementation of the IComparable interface. So the use
        ///     of this instance within collections is guaranteed.
        /// </summary>
        public int CompareTo(object obj)
        {
            return ToString().CompareTo(((Tag) obj).ToString());
        }     

        /// <summary>
        ///     Corresponding data element dictionary entry.
        /// </summary>
        /// <returns>
        ///     A DICOM data element dictionary entry. If this instance represents
        ///     a private tag, a completely useable dictionary entry of type
        ///     unknown is returned.
        /// </returns>
        public DataElementDictionaryEntry GetDictionaryEntry()
        {
            if (IsUserDefined)
                // no data element dictionary entry exists
                return new DataElementDictionaryEntry(this, "Unknown");
            else
                // data element dictionary entry exists
                return DataElementDictionary.Global.GetDictionaryEntry(this);
        }

        /// <summary>
        ///     DICOM tag string representation.
        /// </summary>
        /// <returns>
        ///     A DICOM tag as string of format "(gggg,eeee)".
        /// </returns>                        
        public override string ToString()
        {
            return string.Format("({0},{1})", Group, Element); 
        }
    }
    
}
