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


    $Id: DataElementDictionaryEntry.cs 64 2007-03-30 17:22:35Z agnandt $
*/
using System;
using openDicom.DataStructure;


namespace openDicom.Registry
{

    /// <summary>
    ///     Data element dictionary entry. This class represents a registered
    ///     DICOM data element.
    /// </summary>
    public sealed class DataElementDictionaryEntry: IComparable
    {
        private Tag tag;
        /// <summary>
        ///     DICOM tag.
        /// </summary>
        public Tag Tag
        {
            get { return tag; }
        }

        private string description = string.Empty;
        /// <summary>
        ///     Human readable free text which describes a DICOM tag.
        /// </summary>
        public string Description
        {
            get { return description; }
        }

        private ValueRepresentation vr;
        /// <summary>
        ///     DICOM value representation (VR).
        /// </summary>
        public ValueRepresentation VR
        {
            get { return vr; }
        }        

        private ValueMultiplicity vm;
        /// <summary>
        ///     DICOM value multiplicity (VM).
        /// </summary>
        public ValueMultiplicity VM
        {
            get { return vm; }
        }

        private bool isRetired = false;
        /// <summary>
        ///     Returns that this DICOM tag is not supposed to be in use within
        ///     new DICOM content, but available for downgrade compliance.
        /// </summary>
        public bool IsRetired
        {
            get { return isRetired; }
        }


        public DataElementDictionaryEntry(string tag): 
            this(tag, null, null, null, null) {}
    
        public DataElementDictionaryEntry(Tag tag): 
            this(tag, null, null, null, false) {}

        public DataElementDictionaryEntry(string tag, string description): 
            this(tag, description, null, null, null) {}
    
        public DataElementDictionaryEntry(Tag tag, string description): 
            this(tag, description, null, null, false) {}

        public DataElementDictionaryEntry(string tag, string description, 
            string vr, string vm, string retired)
        {
            this.tag = new Tag(tag);
            if (description == null) description = "";
            this.description = description.Trim();
            this.vr = ValueRepresentation.GetBy(vr, Tag);
            this.vm = new ValueMultiplicity(VR, vm);
            if (retired != null) 
            {
                retired = retired.Trim().ToLower();
                isRetired = (retired == "ret" || retired == "retired" ||
                    retired == "true");
            }
        }

        public DataElementDictionaryEntry(Tag tag, string description, 
            ValueRepresentation vr, ValueMultiplicity vm, bool retired)
        {
            this.tag = tag;
            if (description == null) description = "";
            this.description = description.Trim();
            if (vr == null) 
                this.vr = ValueRepresentation.GetBy(Tag);
            else 
                this.vr = vr;
            if (vm == null)
                this.vm = new ValueMultiplicity(VR);
            else
                this.vm = vm;
            isRetired = retired;
        }

        /// <summary>
        ///     Determines whether this instance is equal to another
        ///     data element dictionary entry instance.
        /// </summary>
        public bool Equals(DataElementDictionaryEntry dictionaryEntry)
        {
            return CompareTo(dictionaryEntry) == 0;
        }

        /// <summary>
        ///     Returns the ranking between another data element entry 
        ///     instance and this instance. This is important for sorting and
        ///     use of arrays.
        /// </summary>
        public int CompareTo(object obj)
        {
            Tag tag = ((DataElementDictionaryEntry) obj).Tag;
            return Tag.CompareTo(tag);
        }
    }    

}
