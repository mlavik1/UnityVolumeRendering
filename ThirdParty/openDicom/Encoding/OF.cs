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


    $Id: OF.cs 48 2007-03-28 13:49:15Z agnandt $
*/
using System;
using openDicom.DataStructure;
using openDicom.Registry;


namespace openDicom.Encoding
{

    /// <summary>
    ///     This class represents the specific DICOM VR Other Float String (OF).
    /// </summary>
    public sealed class OtherFloatString: ValueRepresentation
    {
        public OtherFloatString(Tag tag): base("OF", tag) {}
        
        public override string ToLongString()
        {
            return "Other Float String (OF)";
        }

        protected override Array DecodeImproper(byte[] bytes)
        {
            float[] floatValue = new float[(int) Math.Floor((double) bytes.Length / 4)];
            byte[] buffer = new byte[4];
            for (int i = 0; i < floatValue.Length; i++)
            {
                Array.Copy(bytes, i * 4, buffer, 0, 4);
                floatValue[i] = BitConverter.ToSingle(
                    TransferSyntax.CorrectByteOrdering(buffer), 0);
            }
            return new float[1][] { floatValue };
        }
        
        protected override Array DecodeProper(byte[] bytes)
        {
            ValueMultiplicity vm = Tag.GetDictionaryEntry().VM;
            if (vm.Equals(1) || vm.IsUndefined)
            {
                if (bytes.Length % 4 != 0)
                    throw new EncodingException(
                        "A value of multiple 4 bytes is only allowed.", Tag,
                        Name + "/value.Length", bytes.Length.ToString());
                if (bytes.Length <= 0xFFFFFFB)
                {
                    float[] floatValue = new float[bytes.Length / 4];
                    byte[] buffer = new byte[4];
                    for (int i = 0; i < floatValue.Length; i++)
                    {
                        Array.Copy(bytes, i * 4, buffer, 0, 4);
                        floatValue[i] = BitConverter.ToSingle(
                            TransferSyntax.CorrectByteOrdering(buffer), 0);
                    }
                    return new float[1][] { floatValue };
                }
                else
                    throw new EncodingException(
                        "A value of max. 2^32 - 4 bytes is only allowed.",
                        Tag, Name + "/value.Length", bytes.Length.ToString());
            }
            else
                throw new EncodingException(
                    "Multiple values are not allowed within this field.",
                    Tag, Name + "/VM", vm.ToString());
        }
    }

}
