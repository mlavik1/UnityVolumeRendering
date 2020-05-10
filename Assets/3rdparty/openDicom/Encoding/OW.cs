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


    $Id: OW.cs 48 2007-03-28 13:49:15Z agnandt $
*/
using System;
using openDicom.DataStructure;
using openDicom.Registry;


namespace openDicom.Encoding
{

    /// <summary>
    ///     This class represents the specific DICOM VR Other Word String (OW).
    /// </summary>
    public sealed class OtherWordString: ValueRepresentation
    {
        public OtherWordString(Tag tag): base("OW", tag) {}
        
        public override string ToLongString()
        {
            return "Other Word String (OW)";
        }

        protected override Array DecodeImproper(byte[] bytes)
        {
            ushort[] wordValue = new ushort[(int) Math.Floor((double) bytes.Length / 2)];
            byte[] buffer = new byte[2];
            for (int i = 0; i < wordValue.Length; i++)
            {
                Array.Copy(bytes, i * 2, buffer, 0, 2);
                wordValue[i] = BitConverter.ToUInt16(
                    TransferSyntax.CorrectByteOrdering(buffer), 0);
            }
            return new ushort[1][] { wordValue };
        }
        
        protected override Array DecodeProper(byte[] bytes)
        {
            ValueMultiplicity vm = Tag.GetDictionaryEntry().VM;
            if (vm.Equals(1) || vm.IsUndefined)
            {
                if (bytes.Length % 2 != 0)
                    throw new EncodingException(
                        "A value of multiple 2 bytes is only allowed.", Tag,
                        Name + "/value.Length", bytes.Length.ToString()); 
                // TODO: Get allowed length from transfer syntax.
                ushort[] wordValue = new ushort[bytes.Length / 2];
                byte[] buffer = new byte[2];
                for (int i = 0; i < wordValue.Length; i++)
                {
                    Array.Copy(bytes, i * 2, buffer, 0, 2);
                    wordValue[i] = BitConverter.ToUInt16(
                        TransferSyntax.CorrectByteOrdering(buffer), 0);
                }
                return new ushort[1][] { wordValue };
            }
            else
                throw new EncodingException(
                    "Multiple values are not allowed within this field.", Tag,
                    Name + "/VM", vm.ToString());
        }
    }

}
