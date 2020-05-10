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


    $Id: Sequence.cs 48 2007-03-28 13:49:15Z agnandt $
*/
using System;
using System.IO;
using System.Collections;
using openDicom.DataStructure;
using openDicom.Encoding;


namespace openDicom.DataStructure.DataSet
{

    /// <summary>
    ///     This class represents a DICOM sequence.
    /// </summary>
    /// <remarks>
    ///     This is the basic container class for all other DICOM stream
    ///     releated container classes. DICOM sequences of items is an
    ///     ordered sequence of data element with the same DICOM tag (FFFE,E000).
    /// </remarks>  
    public class Sequence: IDicomStreamMember
    {
        /// <summary>
        ///     DICOM tag (FFFE,E0DD).
        /// </summary>
        public static readonly Tag DelimiterTag = new Tag("FFFE", "E0DD");

        protected ArrayList itemList = new ArrayList();

        /// <summary>
        ///     Access of this sequence instance as array of
        ///     <see cref="DataElement" />.
        /// </summary>
        public DataElement this[int index]
        {
            get { return (DataElement) itemList[index]; }
        }

        /// <summary>
        ///     Returns count of DICOM data elements.
        /// </summary>        
        public int Count
        {
            get { return itemList.Count; }
        }

        /// <summary>
        ///     Returns whether this instance contains DICOM data elements.
        /// </summary>        
        public bool IsEmpty
        {
            get { return Count == 0; }
        }

        private TransferSyntax transferSyntax = TransferSyntax.Default;
        /// <summary>
        ///     Access corresponding DICOM transfer syntax. If null is assinged,
        ///     the DICOM default transfer syntax will be used instead.
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

        protected long streamPosition = -1;
        /// <summary>
        ///     Returns this instance's position within a DICOM data stream.
        ///     If this instance has not get in contact with a DICOM stream,
        ///     no position will be marked and -1 will be returned.
        /// </summary>        
        public long StreamPosition
        {
            get { return streamPosition; }
        }


        /// <summary>
        ///     Creates a new empty DICOM sequence instance.
        /// </summary>        
        public Sequence() {}

        /// <summary>
        ///     Creates a new DICOM sequence instance and fills it with
        ///     DICOM data elements from specified DICOM output stream using
        ///     the default transfer syntax.
        /// </summary>        
        public Sequence(Stream stream): this(stream, null) {}

        /// <summary>
        ///     Creates a new DICOM sequence instance and fills it with
        ///     DICOM data elements from specified DICOM output stream using
        ///     specified transfer syntax.
        /// </summary>        
        public Sequence(Stream stream, TransferSyntax transferSyntax)
        {
            TransferSyntax = transferSyntax;
            LoadFrom(stream);
        }

        /// <summary>
        ///     Adds a DICOM data element instance to this instance. Multiple
        ///     data elements of equal instances are allowed within a
        ///     sequence.
        /// </summary>        
        public virtual int Add(DataElement dataElement)
        {
            return itemList.Add(dataElement);
        }

        /// <summary>
        ///     Concatenates another DICOM sequence instance with this sequence
        ///     instance. Multiple data elements of the same DICOM tag are
        ///     allowed during concatentation.
        /// </summary>
        public void Add(Sequence sequence)
        {
            foreach (DataElement element in sequence)
                Add(element);
        }

        /// <summary>
        ///     Re-creates a new DICOM sequence instance and fills it with
        ///     DICOM data elements from specified DICOM output stream using
        ///     <see cref="Sequence.TransferSyntax" />.
        /// </summary>        
        public virtual void LoadFrom(Stream stream)
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

        private Sequence TreeNodeToSequence(Sequence sequence)
        {
            Sequence result = new Sequence();
            foreach (DataElement element in sequence)
            {
                result.Add(element);
                foreach (object value in element.Value)
                {
                    if (value is Sequence)
                        result.Add(TreeNodeToSequence((Sequence) value));
                }
            }
            return result;
        }

        /// <summary>
        ///     Returns all sequences of all levels of a sequence tree as
        ///     one concatenated zero-level sequence. Multiple data elements
        ///     of equal instances are allowed within a DICOM sequence.
        /// </summary>
        public virtual Sequence GetJointSubsequences()
        {
            return TreeNodeToSequence(this);
        }

        /// <summary>
        ///     Clears all DICOM sequence properties.
        /// </summary>
        public virtual void Clear()
        {
            itemList.Clear();
        } 

        /// <summary>
        ///     Sorts all data elements of a sequence instance using
        ///     <see cref="ArrayList.Sort" />. This method is normally supposed
        ///     not to be used, except by derivated classes that
        ///     use unique data element identifiers like DICOM data set.
        /// </summary>
        public virtual void Sort()
        {
            itemList.Sort();
        }

        /// <summary>
        ///     Returns all containing data elements as array of
        ///     <see cref="DataElement" />.
        /// </summary>
        public DataElement[] ToArray()
        {
            return (DataElement[]) itemList.ToArray();
        }

        /// <summary>
        ///     Needed by the C# foreach-statement. Makes life easier.
        /// </summary>
        public IEnumerator GetEnumerator()
        {
            return itemList.GetEnumerator();
        }   
    }

}
