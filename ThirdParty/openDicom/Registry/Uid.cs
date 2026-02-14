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


    $Id: Uid.cs 48 2007-03-28 13:49:15Z agnandt $
*/
using System;
using System.IO;
using System.Collections;
using System.Xml;
using System.Text.RegularExpressions;
using openDicom.DataStructure;
using openDicom.Encoding;


namespace openDicom.Registry
{

    /// <summary>
    ///     This class represents a DICOM Unique Identifier (UID).
    /// </summary>
    public class Uid: IComparable
    {
        private string uid = null;
        /// <summary>
        ///     DICOM UID as string representation.
        /// </summary>
        public string Value
        {
            set
            {
                if (value != null)
                {
                    string uid = value.Trim();
                    if (uid.Length > 64)
                        throw new DicomException("Uid exceeds max. length of " +
                            "64 characters.", "Uid.Value.Length", 
                            uid.Length.ToString());
                    if (Regex.IsMatch(uid, "^[0-9]+(\\.[0-9]+)*$"))
                        this.uid = uid;
                    else
                        throw new DicomException("Uid is invalid.",
                            "Uid.Value", uid);
                }
                else
                    throw new DicomException("UID is null.", "Uid.Value");
            }

            get
            {
                return uid;
            }
        }

        /// <summary>
        ///     Returns whether this DICOM UID instance is known by the
        ///     global UID dictionary or user defined. 
        /// </summary>
        public bool IsUserDefined
        {
            get { return ! UidDictionary.Global.Contains(this); }
        }


        /// <summary>
        ///     Creates a new DICOM UID instance from a given string
        ///     representation.
        /// </summary>
        public Uid(string uid)
        {
            Value = uid;
        }

        /// <summary>
        ///     Determines whether this instance is equal to another UID string
        ///     representation.
        /// </summary>
        public bool Equals(string uid)
        {
            return Value.Equals(uid);
        }

        /// <summary>
        ///     Determines whether this instance is equal to another UID.
        /// </summary>
        public bool Equals(Uid uid)
        {
            if (uid != null)
                return Value.Equals(uid.Value);
            else
                return false;
        }

        /// <summary>
        ///     Determines the ranking between another UID instance
        ///     and this instance. This is important for sorting and use of
        ///     arrays.
        /// </summary>
        public int CompareTo(object obj)
        {
            return Value.CompareTo(((Uid) obj).Value);
        }

        /// <summary>
        ///     Returns the corresponding DICOM transfer syntax of this UID
        ///     instance.
        /// </summary>
        public TransferSyntax GetTransferSyntax()
        {
            return new TransferSyntax(this);
        }

        /// <summary>
        ///     Returns the corresponding UID dictionary entry of this UID
        ///     instance.
        /// </summary>
        public UidDictionaryEntry GetDictionaryEntry()
        {
            if (IsUserDefined)
                // no UID dictionary entry exists
                return new UidDictionaryEntry(this, "Unknown");
            else
                // UID dictionary entry exists
                return UidDictionary.Global.GetDictionaryEntry(this);
        }

        /// <summary>
        ///     Returns the DICOM UID string representation.
        /// </summary>
        public override string ToString()
        {
            return Value;
        }
    }

}
