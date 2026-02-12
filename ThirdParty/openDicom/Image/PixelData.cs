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


    $Id: PixelData.cs 48 2007-03-28 13:49:15Z agnandt $
*/
using System;
using System.IO;
using System.Text.RegularExpressions;
using openDicom;
using openDicom.DataStructure;
using openDicom.DataStructure.DataSet;
using openDicom.Encoding;


namespace openDicom.Image
{

    /// <summary>
    ///     Basic class for working with DICOM pixel data.
    /// </summary>
    public sealed class PixelData
    {
        /// <summary>
        ///     DICOM tag (0028,0002).
        /// </summary>
        public static readonly Tag SamplesPerPixelTag  = 
            new Tag("0028", "0002");
        /// <summary>
        ///     DICOM tag (0028,0006).
        /// </summary>
        public static readonly Tag PlanarConfigurationTag = 
            new Tag("0028", "0006");
        /// <summary>
        ///     DICOM tag (0028,0010).
        /// </summary>
        public static readonly Tag RowsTag          = new Tag("0028", "0010");
        /// <summary>
        ///     DICOM tag (0028,0011).
        /// </summary>
        public static readonly Tag ColumnsTag       = new Tag("0028", "0011");
        /// <summary>
        ///     DICOM tag (0028,0100).
        /// </summary>
        public static readonly Tag BitsAllocatedTag = new Tag("0028", "0100");
        /// <summary>
        ///     DICOM tag (0028,0101).
        /// </summary>
        public static readonly Tag BitsStoredTag    = new Tag("0028", "0101");
        /// <summary>
        ///     DICOM tag (0028,0102).
        /// </summary>
        public static readonly Tag HighBitTag       = new Tag("0028", "0102");
        /// <summary>
        ///     DICOM tag (7FE0,0010).
        /// </summary>
        public static readonly Tag PixelDataTag     = new Tag("7FE0", "0010");

        private int samplesPerPixel = 0;
        /// <summary>
        ///     Value from DICOM tag (0028,0002). If this value is not
        ///     specified, 0 will be returned.
        /// </summary>
        public int SamplesPerPixel
        {
            get
            {
                if (samplesPerPixel > 0)
                    return samplesPerPixel;
                else
                    throw new DicomException("Pixel data samples per pixel " +
                        "are invalid.", "PixelData.SamplesPerPixel", 
                        samplesPerPixel.ToString());
            }
        }

        private int planarConfiguration = -1;
        /// <summary>
        ///     Value from DICOM tag (0028,0006). If this value is not
        ///     specified, -1 will be returned.
        /// </summary>
        public int PlanarConfiguration
        {
            get
            {
                if (planarConfiguration >= 0 && planarConfiguration <= 1)
                    return planarConfiguration;
                else
                    throw new DicomException("Pixel data planar configuration " +
                        "is invalid.", "PixelData.PlanarConfiguration", 
                        planarConfiguration.ToString());
            }
        }

        private int rows = 0;
        /// <summary>
        ///     Value from DICOM tag (0028,0010). If this value is not
        ///     specified, 0 will be returned.
        /// </summary>
        public int Rows
        {
            get
            {
                if (rows > 0)
                    return rows;
                else
                    throw new DicomException("Pixel data rows are invalid.", 
                        "PixelData.Rows", rows.ToString());
            }
        }

        private int columns = 0;
        /// <summary>
        ///     Value from DICOM tag (0028,0011). If this value is not
        ///     specified, 0 will be returned.
        /// </summary>
        public int Columns
        {
            get
            {
                if (columns > 0)
                    return columns;
                else
                    throw new DicomException("Pixel data columns are invalid.", 
                        "PixelData.Columns", columns.ToString());
            }
        }

        private int bitsAllocated = 0;
        /// <summary>
        ///     Value from DICOM tag (0028,0100). If this value is not
        ///     specified, 0 will be returned.
        /// </summary>
        public int BitsAllocated
        {
            get
            {
                if (bitsAllocated > 0)
                    return bitsAllocated;
                else
                    throw new DicomException("Pixel data bits allocated is invalid.", 
                        "PixelData.BitsAllocated", bitsAllocated.ToString());
            }
        }

        private int bitsStored = 0;
        /// <summary>
        ///     Value from DICOM tag (0028,0101). If this value is not
        ///     specified, 0 will be returned.
        /// </summary>
        public int BitsStored
        {
            get
            {
                if (bitsStored > 0)
                    return bitsStored;
                else
                    throw new DicomException("Pixel data bits stored is invalid.", 
                        "PixelData.BitsStored", bitsStored.ToString());
            }
        }

        private int highBit = -1;
        /// <summary>
        ///     Value from DICOM tag (0028,0102). If this value is not
        ///     specified, -1 will be returned.
        /// </summary>
        public int HighBit
        {
            get
            {
                if (highBit > -1)
                    return highBit;
                else
                    throw new DicomException("Pixel data high bit is invalid.", 
                        "PixelData.HighBit", highBit.ToString());
            }
        }

        private DataElement data = null;
        /// <summary>
        ///     Pixel data, DICOM tag (7FE0,0010), as DICOM data element.
        /// </summary>
        public DataElement Data
        {
            get
            {
                if (data != null)
                    return data;
                else
                    throw new DicomException("Pixel data is null.", 
                        "PixelData.Data");
            }
        }

        private TransferSyntax transferSyntax = null;
        /// <summary>
        ///     Determines by the DICOM transfer syntax whether pixel data
        ///     is stored as JPEG. If the transfer syntax is unknown,
        ///     pixel data will not be understood as JPEG data.
        /// </summary>
        public bool IsJpeg
        {
            get
            {
                if (transferSyntax != null)
                    return Regex.IsMatch(transferSyntax.Uid.ToString(),
                        "^1\\.2\\.840\\.10008\\.1\\.2\\.4");
                else
                    return false;
            }
        }


        public PixelData(int samplesPerPixel, int planarConfiguration, int rows,
            int columns, int bitsAllocated, int bitsStored, int highBit,
            DataElement data, TransferSyntax transferSyntax)
        {
            LoadFrom(samplesPerPixel, planarConfiguration, rows, columns,
                bitsAllocated, bitsStored, highBit, data, transferSyntax);
        }

        public PixelData(DataElement samplesPerPixel, 
            DataElement planarConfiguration, DataElement rows,
            DataElement columns, DataElement bitsAllocated, 
            DataElement bitsStored, DataElement highBit, DataElement data)
        {
            LoadFrom(samplesPerPixel, planarConfiguration, rows, columns, 
                bitsAllocated, bitsStored, highBit, data);
        }

        /// <summary>
        ///     Creates a pixel data instance from the specified data set.
        /// </summary>
        public PixelData(DataSet dataSet)
        {
            LoadFrom(dataSet);
        }

        public void LoadFrom(int samplesPerPixel, int planarConfiguration, 
            int rows, int columns, int bitsAllocated, int bitsStored, 
            int highBit, DataElement data, TransferSyntax transferSyntax)
        {
            this.samplesPerPixel = samplesPerPixel;
            this.planarConfiguration = planarConfiguration;
            this.rows = rows;
            this.columns = columns;
            this.bitsAllocated = bitsAllocated;
            this.bitsAllocated = bitsStored;
            this.highBit = highBit;
            this.data = data;
            this.transferSyntax = transferSyntax;
        }

        public void LoadFrom(DataElement samplesPerPixel, 
            DataElement planarConfiguration, DataElement rows,
            DataElement columns, DataElement bitsAllocated, 
            DataElement bitsStored, DataElement highBit, DataElement data)
        {
            this.samplesPerPixel = ToValue(samplesPerPixel);
            this.planarConfiguration = ToValue(planarConfiguration);
            this.rows = ToValue(rows);
            this.columns = ToValue(columns);
            this.bitsAllocated = ToValue(bitsAllocated);
            this.bitsStored = ToValue(bitsStored);
            this.highBit = ToValue(highBit);
            this.data = data;
        }

        /// <summary>
        ///     Re-creates a pixel data instance from the specified data set.
        /// </summary>
        public void LoadFrom(DataSet dataSet)
        {
            if (dataSet != null)
            {
                foreach (DataElement element in dataSet)
                {
                    if (element.Tag.Equals(SamplesPerPixelTag))
                        samplesPerPixel = ToValue(element);
                    else if (element.Tag.Equals(PlanarConfigurationTag))
                        planarConfiguration = ToValue(element);
                    else if (element.Tag.Equals(RowsTag))
                        rows = ToValue(element);
                    else if (element.Tag.Equals(ColumnsTag))
                        columns = ToValue(element);
                    else if (element.Tag.Equals(BitsAllocatedTag))
                        bitsAllocated = ToValue(element);
                    else if (element.Tag.Equals(BitsStoredTag))
                        bitsStored = ToValue(element);
                    else if (element.Tag.Equals(HighBitTag))
                        highBit = ToValue(element);
                    else if (element.Tag.Equals(PixelDataTag))
                        data = element;
                    else if (element.Tag.Equals(TransferSyntax.UidTag))
                        transferSyntax = new TransferSyntax(element);
                }
            }
            else
                throw new DicomException("Data set is null.", "dataSet");
        }

        private int ToValue(DataElement element)
        {
            if (element != null)
            {
                if ( ! element.Tag.Equals(PixelDataTag))
                    return (ushort) element.Value[0];
                else
                    throw new DicomException("Data element does not belong " +
                        "to pixel data.", "element.Tag", 
                        element.Tag.ToString());
            }
            else
                throw new DicomException("Data element is null.", "element");            
        }

        /// <summary>
        ///     Determines whether specified data set contains pixel data.
        /// </summary>
        public static bool HasPixelData(DataSet dataSet)
        {
            if (dataSet != null)
                return dataSet.Contains(PixelDataTag);
            else
                return false;
        }

        /// <summary>
        ///     Determines whether specified data set contains the minimum
        ///     of necessary content for working with pixel data.
        /// </summary>
        public static bool IsValid(DataSet dataSet)
        {
            if (dataSet != null)
                return dataSet.Contains(SamplesPerPixelTag)
                    && dataSet.Contains(PlanarConfigurationTag)
                    && dataSet.Contains(RowsTag)
                    && dataSet.Contains(ColumnsTag)
                    && dataSet.Contains(BitsAllocatedTag)
                    && dataSet.Contains(BitsStoredTag) 
                    && dataSet.Contains(HighBitTag)
                    && dataSet.Contains(PixelDataTag);
            else
                return false;
        }

        /// <summary>
        ///     Returns the entire DICOM pixel data as array of binary arrays.
        /// </summary>
        /// <remarks>
        ///     If a DICOM pixel data element is not a DICOM sequence of items,
        ///     an array with a single binary array entry will be returned.
        ///     Binary arrays are supposed to be of the type byte[], ushort[]
        ///     or short[].
        /// </remarks>
        public object[] ToArray()
        {
            if (Data.Value.IsSequence)
            {
                Sequence sq = (Sequence) Data.Value[0];
                object[] array = new object[sq.Count];
                for (int i = 0; i < sq.Count; i++)
                    array[i] = sq[i].Value[0];
                return array;
            }
            else
                return new object[1] { Data.Value[0] };
        }

        /// <summary>
        ///     Returns the entire DICOM pixel data as array of byte arrays.
        /// </summary>
        /// <remarks>
        ///     All non-byte arrays are transcoded into byte arrays. If a DICOM
        ///     pixel data element is not a DICOM sequence of items, an array
        ///     with a single byte array entry will be returned.
        /// </remarks>
        public byte[][] ToBytesArray()
        {
            byte[][] bytesArray;
            if (Data.Value.IsSequence)
            {
                Sequence sq = (Sequence) Data.Value[0];
                bytesArray = new byte[sq.Count][];
                for (int i = 0; i < sq.Count; i++)
                {
                    if (sq[i].Value[0] is ushort[])
                        bytesArray[i] = ByteConvert.ToBytes(
                            (ushort[]) sq[i].Value[0]);
                    else
                        bytesArray[i] = (byte[]) sq[i].Value[0];
                }
            }
            else
            {
                bytesArray = new byte[1][];
                if (Data.Value[0] is ushort[])
                    bytesArray[0] = ByteConvert.ToBytes(
                        (ushort[]) Data.Value[0]);
                else
                    bytesArray[0] = (byte[]) Data.Value[0];
            }
            return bytesArray;
        }
    }

}
