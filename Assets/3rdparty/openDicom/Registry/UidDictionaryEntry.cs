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


    $Id: UidDictionaryEntry.cs 74 2007-03-31 00:47:35Z agnandt $
*/
using System;
using openDicom.DataStructure;


namespace openDicom.Registry
{

    /// <summary>
    ///     Available types of DICOM UIDs (Unique Identifiers).
    /// </summary>
    public enum UidType
    {
        /// <summary>
        ///     UID is a DICOM Transfer Syntax.
        /// </summary>
        TransferSyntax,
        /// <summary>
        ///     UID is a DICOM SOP (Service Object Pair) Class.
        /// </summary>
        SopClass,
        /// <summary>
        ///     UID is a DICOM Well-known Frame of Reference.
        /// </summary>
        FrameOfReference,
        /// <summary>
        ///     UID is a DICOM Meta SOP Class.
        /// </summary>
        MetaSopClass,
        /// <summary>
        ///     UID is a DICOM Well-known SOP Instance.
        /// </summary>
        SopInstance,
        /// <summary>
        ///     UID is a DICOM Service Class.
        /// </summary>
        ServiceClass,
        /// <summary>
        ///     UID is a DICOM Well-known Printer SOP Instance.
        /// </summary>
        PrinterSopInstance,
        /// <summary>
        ///     UID is a DICOM Well-known Print Queue SOP Instance.
        /// </summary>
        PrintQueueSopInstance,
        /// <summary>
        ///     UID is a DICOM Coding Scheme.
        /// </summary>
        CodingScheme,
        /// <summary>
        ///     UID is a DICOM Application Context Name.
        /// </summary>
        ApplicationContextName,
        /// <summary>
        ///     UID is a DICOM LDAP OID.
        /// </summary>
        LdapOid,
        /// <summary>
        ///     This UID is unknown. It is not part of the DICOM
        ///     registry and therewith probably user defined.
        /// </summary>
        Unknown
    }


    /// <summary>
    ///     UID (Unique Identifier) data dictionary entry. This class
    ///     represents a registered DICOM UID.
    /// </summary>
    public sealed class UidDictionaryEntry: IComparable
    {
        private Uid uid;
        /// <summary>
        ///     DICOM UID.
        /// </summary>
        public Uid Uid
        {
            get { return uid; }
        }

        private string name;
        /// <summary>
        ///     DICOM UID name.
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        private UidType type;
        /// <summary>
        ///     DICOM UID type.
        /// </summary>
        public UidType Type
        {
            get { return type; }
        }

        private bool isRetired = false;
        /// <summary>
        ///     Returns that this DICOM UID is not supposed to be in use within
        ///     new DICOM content, but available for downgrade compliance.
        /// </summary>
        public bool IsRetired
        {
            get { return isRetired; }
        }


        public UidDictionaryEntry(string uid): this(uid, null, null, null) {}
    
        public UidDictionaryEntry(Uid uid): 
            this(uid, null, UidType.Unknown, false) {}

        public UidDictionaryEntry(string uid, string name): 
            this(uid, name, null, null) {}
    
        public UidDictionaryEntry(Uid uid, string name): 
            this(uid, name, UidType.Unknown, false) {}

        public UidDictionaryEntry(string uid, string name, string type, 
            string retired)
        {
            this.uid = new Uid(uid);
            if (name == null)
                this.name = "";
            else
                this.name = name.Trim();
            if (type == null)
                this.type = UidType.Unknown;
            else
                this.type = (UidType) UidType.Parse(typeof(UidType), type);
            if (retired != null) 
            {
                retired = retired.Trim().ToLower();
                isRetired = (retired == "ret" || retired == "retired" ||
                    retired == "true");
            }
        }

        public UidDictionaryEntry(Uid uid, string name, UidType type, 
            bool retired)
        {
            this.uid = uid;
            if (name == null)
                this.name = "";
            else
                this.name = name.Trim();
            this.type = type;
            isRetired = retired;
        }

        /// <summary>
        ///     Determines whether another UID dictionary entry instance equals
        ///     this instance by its properties.
        /// </summary>
        public bool Equals(UidDictionaryEntry dictionaryEntry)
        {
            return CompareTo(dictionaryEntry) == 0;
        }

        /// <summary>
        ///     Determines the ranking between another UID dictionary instance
        ///     and this instance. This is important for sorting and use of
        ///     arrays.
        /// </summary>
        public int CompareTo(object obj)
        {
            Uid uid = ((UidDictionaryEntry) obj).Uid;
            return Uid.CompareTo(uid);
        }
    }    

}
