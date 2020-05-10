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


    $Id: UT.cs 48 2007-03-28 13:49:15Z agnandt $
*/
using System;
using openDicom.Registry;
using openDicom.DataStructure;


namespace openDicom.Encoding
{

    /// <summary>
    ///     This class represents the specific DICOM VR Unlimited Text (UT).
    /// </summary>
    public sealed class UnlimitedText: ValueRepresentation
    {
        public UnlimitedText(Tag tag): base("UT", tag) {}
        
        public override string ToLongString()
        {
            return "Unlimited Text (UT)";
        }

        protected override Array DecodeImproper(byte[] bytes)
        {
            string unlimitedText = TransferSyntax.ToString(bytes);
            unlimitedText = unlimitedText.TrimEnd(null);
            return new string[] { unlimitedText };
        }
        
        protected override Array DecodeProper(byte[] bytes)
        {
            string unlimitedText = TransferSyntax.ToString(bytes);
            ValueMultiplicity vm = Tag.GetDictionaryEntry().VM;
            if (vm.Equals(1) || vm.IsUndefined)
            {
                // 0xFFFFFFFF is reserved!
                if (unlimitedText.Length <= 0xFFFFFFFE)
                    unlimitedText = unlimitedText.TrimEnd(null);
                else
                    throw new EncodingException(
                        "A value of max. 2^32 - 2 characters is only allowed.",
                        Tag, Name + "/unlimitedText", unlimitedText);
            }
            else
                throw new EncodingException(
                    "Multiple values are not allowed within this field.", Tag,
                    Name + "/unlimitedText", unlimitedText);
            return new string[] { unlimitedText };
        }
    }
    
}
