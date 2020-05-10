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


    $Id: SQ.cs 48 2007-03-28 13:49:15Z agnandt $
*/
using System;
using System.IO;
using openDicom.DataStructure;
using openDicom.DataStructure.DataSet;


namespace openDicom.Encoding
{

    /// <summary>
    ///     This class represents the specific DICOM VR Sequence of Items (SQ).
    /// </summary>
    public sealed class SequenceOfItems: ValueRepresentation
    {
        public SequenceOfItems(Tag tag): base("SQ", tag) {}
        
        public override string ToLongString()
        {
            return "Sequence Of Items (SQ)";
        }

        protected override Array DecodeImproper(byte[] bytes)
        {
            return DecodeProper(bytes);
        }
        
        protected override Array DecodeProper(byte[] bytes)
        {
            // sequences are special cases that are normally taken care of by
            // the class Value, use this method if you know what you are doing
            MemoryStream memoryStream = new MemoryStream(bytes);
            Sequence sq = new Sequence(memoryStream);
            return new Sequence[1] { sq };
        }
    }

}
