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


    $Id: TransferSyntax.cs 48 2007-03-28 13:49:15Z agnandt $
*/
using System;
using System.Text.RegularExpressions;
using openDicom.Registry;
using openDicom.DataStructure;
using openDicom.DataStructure.DataSet;


namespace openDicom.Encoding
{

    using System.Text;

    /// <summary>
    ///     This class represents a DICOM transfer syntax.
    /// </summary>
    public class TransferSyntax
    {
        /// <summary>
        ///     DICOM transfer syntax UID (0002,0010).
        /// </summary>
        public static readonly Tag UidTag = new Tag("0002", "0010");

        /// <summary>
        ///     DICOM default transfer syntax with UID 1.2.840.10008.1.2.
        /// </summary>
        public static readonly TransferSyntax Default = new TransferSyntax();

        /// <summary>
        ///     DICOM transfer syntax for file meta information data sets
        ///     (UID is 1.2.840.10008.1.2.1).
        /// </summary>
        public static readonly TransferSyntax FileMetaInformation =
            new TransferSyntax("1.2.840.10008.1.2.1");

        private bool isImplicitVR = true;
        /// <summary>
        ///     Returns whether this transfer syntax instance uses implicit
        ///     DICOM value representations. Implicit VRs can only be accessed
        ///     from the data element dictionary and are not part of a
        ///     DICOM stream.
        /// </summary>
        public bool IsImplicitVR
        {
            get { return isImplicitVR; }
        }

        private bool isLittleEndian = true;
        /// <summary>
        ///     Returns whether this transfer syntax uses little endian byte
        ///     ordering. This is relevant in context of de-/encoding of
        ///     DICOM stream content according to
        ///     <see cref="IsMachineLittleEndian" />.
        /// </summary>
        public bool IsLittleEndian
        {
            get { return isLittleEndian; }
        }

        /// <summary>
        ///     Returns whether the underlying machine is a little endian
        ///     byte ordering architecture or not.
        /// </summary>
        public bool IsMachineLittleEndian
        {
            get { return BitConverter.IsLittleEndian; }
        }

        private CharacterRepertoire characterRepertoire = 
            CharacterRepertoire.Default;
        /// <summary>
        ///     Returns the corresponding DICOM character repertoire.
        /// </summary>
        public CharacterRepertoire CharacterRepertoire
        {
            set
            {
                if (value == null)
                    characterRepertoire = CharacterRepertoire.Default;
                else
                    characterRepertoire = value;
            }

            get { return characterRepertoire; }
        }

        private Uid uid = new Uid("1.2.840.10008.1.2");
        /// <summary>
        ///     Access the DICOM UID indentifying this transfer syntax instance.
        /// </summary>
        public Uid Uid
        {
            set 
            {
                if (value != null)
                {
                    uid = value;
                    if (Regex.IsMatch(uid.ToString(), 
                        "^1\\.2\\.840\\.10008\\.1\\.2"))
                    {
                        switch (uid.ToString())
                        {
                            case "1.2.840.10008.1.2":
                                isImplicitVR = true;
                                isLittleEndian = true;
                                break;
                            case "1.2.840.10008.1.2.1": 
                                isImplicitVR = false;
                                isLittleEndian = true;
                                break;
                            case "1.2.840.10008.1.2.2": 
                                isImplicitVR = false;
                                isLittleEndian = false;
                                break;
                            case "1.2.840.10008.1.2.99": 
                                throw new DicomException("The deflated " +
                                    "transfer syntax is not supported.",
                                    "uid", uid.ToString());
                                break;
                            default:
                                // defaults for transfer syntax for JPEG
                                // (1.2.840.10008.1.2.4.*) and RLE
                                // (1.2.840.10008.1.2.5) encoding according
                                // to the DICOM standard
                                isImplicitVR = false;
                                isLittleEndian = true;
                                break;
                        }
                    }
                    else
                        throw new DicomException("UID is not a valid transfer " +
                            "syntax UID.", "TransferSyntax.Uid", uid.ToString());
                }
                else
                    throw new DicomException("UID is null.", "Uid.Uid");
            }        

            get { return uid; }
        }


        /// <summary>
        ///     Creates a new DICOM default transfer syntax instance with default
        ///     character repertoire.
        /// </summary>
        public TransferSyntax() {}

        /// <summary>
        ///     Creates a new DICOM transfer syntax instance from specified
        ///     DICOM UID string representation and default character repertoire.
        /// </summary>
        public TransferSyntax(string uid): this(new Uid(uid), null) {}

        /// <summary>
        ///     Creates a new DICOM transfer syntax instance from specified
        ///     DICOM UID string representation and specified DICOM character
        ///     repertoire.
        /// </summary>
        public TransferSyntax(string uid, 
            CharacterRepertoire characterRepertoire):
            this(new Uid(uid), characterRepertoire) {}

        /// <summary>
        ///     Creates a new DICOM transfer syntax instance from specified
        ///     DICOM UID and default character repertoire.
        /// </summary>
        public TransferSyntax(Uid uid): this(uid, null) {}

        /// <summary>
        ///     Creates a new DICOM transfer syntax instance from specified DICOM
        ///     UID and default character repertoire.
        /// </summary>
        public TransferSyntax(Uid uid, CharacterRepertoire characterRepertoire)
        {
            Uid = uid;
            CharacterRepertoire = characterRepertoire;
        }

        /// <summary>
        ///     Creates a new DICOM transfer syntax instance from specified transfer
        ///     syntax UID data element and default character repertoire.
        /// </summary>
        public TransferSyntax(DataElement transferSyntaxUid):
            this(transferSyntaxUid, null) {}

        /// <summary>
        ///     Creates a new DICOM transfer syntax instance from specified
        ///     transfer syntax UID data element and specified DICOM character
        ///     repertoire.
        /// </summary>
        public TransferSyntax(DataElement transferSyntaxUid,
            CharacterRepertoire characterRepertoire)
        {
            CharacterRepertoire = characterRepertoire;
            LoadFrom(transferSyntaxUid);
        }

        /// <summary>
        ///     Creates a new DICOM transfer syntax instance from specified data
        ///     set containing a transfer syntax UID data element and default
        ///     character repertoire.
        /// </summary>
        public TransferSyntax(DataSet dataSet): this(dataSet, null) {}

        /// <summary>
        ///     Creates a new DICOM transfer syntax instance from specified data
        ///     set containing a transfer syntax UID data element and specified
        ///     character repertoire.
        /// </summary>
        public TransferSyntax(DataSet dataSet, 
            CharacterRepertoire characterRepertoire)
        {
            CharacterRepertoire = characterRepertoire;
            LoadFrom(dataSet);
        }

        /// <summary>
        ///     Re-creates a DICOM transfer syntax instance from specified data
        ///     set containing a transfer syntax UID data element and default
        ///     character repertoire.
        /// </summary>
        public void LoadFrom(DataSet dataSet)
        {
            // character repertoire content cannot be read from data set,
            // because data set is already read in.
            if (dataSet.Contains(UidTag))
                LoadFrom(dataSet[UidTag]);
            else
                throw new DicomException("Data set does not contain a " +
                    "transfer syntax UID.", "dataSet");
        }

        /// <summary>
        ///     Determines whether this instance is equal to another DICOM
        ///     transfer syntax instance or not. Equality of the UIDs will
        ///     be checked.
        /// </summary>
        public void LoadFrom(DataElement transferSyntaxUid)
        {
            if (transferSyntaxUid != null)
            {
                if (transferSyntaxUid.Tag.Equals(UidTag))
                    Uid = (Uid) transferSyntaxUid.Value[0];
                else
                    throw new DicomException("Data element is not a transfer " +
                        "syntax UID.", "transferSyntaxUID.Tag", 
                        transferSyntaxUid.Tag.ToString());
            }
            else
                throw new DicomException("Data element is null.", 
                    "transferSyntaxUID");                        
        }

        /// <summary>
        ///     Determines whether this instance is equal to another DICOM
        ///     transfer syntax instance or not. Equality of the UIDs will
        ///     be checked.
        /// </summary>
        public bool Equals(TransferSyntax transferSyntax)
        {
            if (transferSyntax != null)
                return Uid.Equals(transferSyntax.Uid);
            else
                return false;
        }

        /// <summary>
        ///     Converts an array of bytes into a string by this
        ///     DICOM transfer syntax instance's character repertoire.
        /// </summary>
        public string ToString(byte[] bytes)
        {
            return ByteConvert.ToString(bytes, CharacterRepertoire);
        }

        /// <summary>
        ///     Converts count of bytes from a byte array into a string by this
        ///     DICOM transfer syntax instance's character repertoire.
        /// </summary>
        public string ToString(byte[] bytes, int count)
        {
            return ByteConvert.ToString(bytes, count, CharacterRepertoire);
        }

        /// <summary>
        ///     Converts count of bytes from a byte array starting at 
        ///     specified offset into a string by this DICOM transfer syntax
        ///     instance's character repertoire.
        /// </summary>
        public virtual string ToString(byte[] bytes, int offset, int count)
        {
            return ByteConvert.ToString(bytes, offset, count, 
                CharacterRepertoire);
        }

        /// <summary>
        ///     Converts a string into an array of bytes by this
        ///     DICOM transfer syntax instance's character repertoire.
        /// </summary>
        public virtual byte[] ToBytes(string s)
        {
            return ByteConvert.ToBytes(s, CharacterRepertoire);
        }

        /// <summary>
        ///     Determines whether the bytes of an unsigned word have to be
        ///     swapped according to the used transfer syntax. There is
        ///     considered the endian type of underlying machine and transfer
        ///     syntax.
        /// </summary>
        /// <param name="word">
        ///     Unsigned word to process.
        /// </param>
        /// <returns>
        ///     Unsigned word with or without bytes swapped.
        /// </returns>
        public ushort CorrectByteOrdering(ushort word)
        {
            if ((IsMachineLittleEndian && ! IsLittleEndian) ||
                ( ! IsMachineLittleEndian && IsLittleEndian))
                return ByteConvert.SwapBytes(word);
            else
                return word;
        }

        /// <summary>
        ///     Determines whether the bytes of a signed word have to be
        ///     swapped according to the used transfer syntax. There is
        ///     considered the endian type of underlying machine and transfer
        ///     syntax.
        /// </summary>
        /// <param name="word">
        ///     Signed word to process.
        /// </param>
        /// <returns>
        ///     Signed word with or without bytes swapped.
        /// </returns>
        public short CorrectByteOrdering(short word)
        {
            if ((IsMachineLittleEndian && ! IsLittleEndian) ||
                ( ! IsMachineLittleEndian && IsLittleEndian))
                return ByteConvert.SwapBytes(word);
            else
                return word;
        }

        /// <summary>
        ///     Determines whether the bytes of an unsigned integer have to be
        ///     swapped according to the used transfer syntax. There is
        ///     considered the endian type of underlying machine and transfer
        ///     syntax.
        /// </summary>
        /// <param name="value">
        ///     Unsigned integer to process.
        /// </param>
        /// <returns>
        ///     Unsigned integer with or without bytes swapped.
        /// </returns>
        public uint CorrectByteOrdering(uint value)
        {
            if ((IsMachineLittleEndian && ! IsLittleEndian) ||
                ( ! IsMachineLittleEndian && IsLittleEndian))
                return ByteConvert.SwapBytes(value);
            else
                return value;
        }

        /// <summary>
        ///     Determines whether the bytes of a signed integer have to be
        ///     swapped according to the used transfer syntax. There is
        ///     considered the endian type of underlying machine and transfer
        ///     syntax.
        /// </summary>
        /// <param name="value">
        ///     Signed integer to process.
        /// </param>
        /// <returns>
        ///     Signed integer with or without bytes swapped.
        /// </returns>
        public int CorrectByteOrdering(int value)
        {
            if ((IsMachineLittleEndian && ! IsLittleEndian) ||
                ( ! IsMachineLittleEndian && IsLittleEndian))
                return ByteConvert.SwapBytes(value);
            else
                return value;
        }

        /// <summary>
        ///     Determines whether the bytes of an array have to be swapped
        ///     according to the used transfer syntax. This method only is
        ///     relevant to numeric representations as byte arrays. There is
        ///     considered the endian type of underlying machine and transfer
        ///     syntax.
        /// </summary>
        /// <param name="bytes">
        ///     Byte array to process.
        /// </param>
        /// <returns>
        ///     Byte array with or without bytes swapped.
        /// </returns>
        public byte[] CorrectByteOrdering(byte[] bytes)
        {
            if ((IsMachineLittleEndian && ! IsLittleEndian) ||
                ( ! IsMachineLittleEndian && IsLittleEndian))
                return ByteConvert.SwapBytes(bytes);
            else
                return bytes;
        }

        /// <summary>
        ///     Returns a DICOM transfer syntax UID as string representation.
        /// </summary>
        public override string ToString()
        {
            return Uid.ToString();
        }
    }
}
