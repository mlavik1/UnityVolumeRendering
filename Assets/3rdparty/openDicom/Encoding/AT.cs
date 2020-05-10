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


    $Id: AT.cs 48 2007-03-28 13:49:15Z agnandt $
*/
using System;
using openDicom.DataStructure;


namespace openDicom.Encoding
{

    /// <summary>
    ///     This class represents the specific DICOM VR Attribute Tag (AT).
    /// </summary>
    public sealed class AttributeTag: ValueRepresentation
    {
        public AttributeTag(Tag tag): base("AT", tag) {}
        
        public override string ToLongString()
        {
            return "Attribute Tag (AT)";
        }

        protected override Array DecodeImproper(byte[] bytes)
        {
            byte[][] multiValue = ToImproperMultiValue(bytes, 4);
            Tag[] tagValue = new Tag[multiValue.Length];
            for (int i = 0; i < tagValue.Length; i++)
            {
                int group = TransferSyntax.CorrectByteOrdering(
                    BitConverter.ToUInt16(multiValue[i], 0));
                int element = TransferSyntax.CorrectByteOrdering(
                    BitConverter.ToUInt16(multiValue[i], 2));
                tagValue[i] = new Tag(group, element);
            }
            return tagValue;
        }
        
        protected override Array DecodeProper(byte[] bytes)
        {
            byte[][] multiValue = ToProperMultiValue(bytes, 4);
            if (bytes.Length == 4 * multiValue.Length)
            {
                Tag[] tagValue = new Tag[multiValue.Length];
                for (int i = 0; i < tagValue.Length; i++)
                {
                    int group = TransferSyntax.CorrectByteOrdering(
                        BitConverter.ToUInt16(multiValue[i], 0));
                    int element = TransferSyntax.CorrectByteOrdering(
                        BitConverter.ToUInt16(multiValue[i], 2));
                    tagValue[i] = new Tag(group, element);
                }
                return tagValue;
            }
            else
                throw new EncodingException(
                    "A value of multiple 4 bytes is only allowed.", Tag, 
                    Name + "/value.Length", bytes.Length.ToString());
        }
    }

}
