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


    $Id: DataSet.cs 48 2007-03-28 13:49:15Z agnandt $
*/
using System;
using System.IO;
using System.Collections;
using openDicom.DataStructure;
using openDicom.Encoding;


namespace openDicom.DataStructure.DataSet
{

    /// <summary>
    ///     This class represents a DICOM data set.
    /// </summary>
    /// <remarks>
    ///     This is the basic container class for all other DICOM stream
    ///     releated container classes with unique data element support.
    ///     DICOM data set is an ascending ordered sequence of data elements
    ///     with unique DICOM tags.
    /// </remarks>  
    public class DataSet: Sequence
    {
        // key index pair for access of Sequence as array
        private Hashtable keys = new Hashtable();

        /// <summary>
        ///     Access of a DICOM data set instance as array of 
        ///     <see cref="DataElement" />. The index is a DICOM tag.
        /// </summary>
        public DataElement this[Tag tag]
        {
            get 
            { 
                int index = (int) keys[tag.ToString()];
                return base[index]; 
            }
        }


        /// <summary>
        ///     Creates a new empty DICOM data set instance.
        /// </summary>
        public DataSet() {}

        /// <summary>
        ///     Creates a new DICOM data set instance and fills it from
        ///     specified DICOM output stream using the default transfer
        ///     syntax.
        /// </summary>
        public DataSet(Stream stream): base(stream) {}

        /// <summary>
        ///     Creates a new DICOM data set instance and fills it from
        ///     specified DICOM output stream using specified transfer
        ///     syntax.
        /// </summary>
        public DataSet(Stream stream, TransferSyntax transferSyntax):
            base(stream, transferSyntax) {}

        /// <summary>
        ///     Re-creates a new DICOM data set instance and fills it from
        ///     specified DICOM output stream using
        ///     <see cref="Sequence.TransferSyntax" />.
        /// </summary>
        public override void LoadFrom(Stream stream)
        {
            streamPosition = stream.Position;
            TransferSyntax.CharacterRepertoire = CharacterRepertoire.Default;
            bool isTrailingPadding = false;
            while (stream.Position < stream.Length && ! isTrailingPadding)
            {
                DataElement element = new DataElement(stream, TransferSyntax);
                if (element.Tag.Equals(CharacterRepertoire.CharacterSetTag))
                    TransferSyntax.CharacterRepertoire = 
                        new CharacterRepertoire((string) element.Value[0]);
                isTrailingPadding = element.Tag.Equals("(0000,0000)");
                if ( ! isTrailingPadding)
                    Add(element);
            }
        }

        /// <summary>
        ///     Adds a new DICOM data element to this data set instance.
        ///     Multiple data elements of the same DICOM tag are not allowed
        ///     within a data set. Uniqueness is guaranteed by DICOM tags.
        /// </summary>
        public override int Add(DataElement dataElement)
        {
            int index = base.Add(dataElement);
            keys.Add(dataElement.Tag.ToString(), index);
            return index;
        }

        /// <summary>
        ///     Concatenates another DICOM data set instance with this data set
        ///     instance. Multiple data elements of the same DICOM tag are not
        ///     allowed during concatentation. Uniqueness is guaranteed by
        ///     DICOM tags.
        /// </summary>
        public void Add(DataSet dataSet)
        {
            foreach (DataElement element in dataSet)
                Add(element);
        }

        /// <summary>
        ///     Returns all sequences of all levels of a data set tree as
        ///     one concatenated zero-level sequence. Multiple data elements
        ///     of equal instances are allowed within a DICOM sequence.
        /// </summary>
        public override Sequence GetJointSubsequences()
        {
            return base.GetJointSubsequences();
        }

        /// <summary>
        ///     Determines whether this data set instance contains a data
        ///     element with specified DICOM tag.
        /// </summary>
        public bool Contains(Tag tag)
        {
            return keys.Contains(tag.ToString());
        }

        /// <summary>
        ///     Clears all DICOM data set properties.
        /// </summary>
        public override void Clear()
        {
            keys.Clear();
            base.Clear();
        }

        /// <summary>
        ///     Sorts all data elements of a data set instance by their DICOM
        ///     tag in ascending order.
        /// </summary>
        public override void Sort()
        {
            base.Sort();
            for (int index = 0; index < Count; index++)
            {
                string tagKey = base[index].Tag.ToString();
                keys[tagKey] = index;
            }
        }
    }

}
