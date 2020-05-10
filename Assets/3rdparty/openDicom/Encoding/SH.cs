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


    $Id: SH.cs 48 2007-03-28 13:49:15Z agnandt $
*/
using System;
using openDicom.DataStructure;


namespace openDicom.Encoding
{

    /// <summary>
    ///     This class represents the specific DICOM VR Short Name (SH).
    /// </summary>
    public sealed class ShortName: ValueRepresentation
    {
        public ShortName(Tag tag): base("SH", tag) {}
        
        public override string ToLongString()
        {
            return "Short Name (SH)";
        }

        protected override Array DecodeImproper(byte[] bytes)
        {
            string s = TransferSyntax.ToString(bytes);
            string[] shortName = ToImproperMultiValue(s);
            for (int i = 0; i < shortName.Length; i++)
            {
                string item = shortName[i];
                shortName[i] = item.Trim();
            }
            return shortName;
        }
        
        protected override Array DecodeProper(byte[] bytes)
        {
            string s = TransferSyntax.ToString(bytes);
            string[] shortName = ToProperMultiValue(s);
            for (int i = 0; i < shortName.Length; i++)
            {
                string item = shortName[i];
                if (item.Length <= 16)
                    shortName[i] = item.Trim();
                else
                    throw new EncodingException(
                        "A value of max. 16 characters is only allowed.",
                        Tag, Name + "/item", item);
            }
            return shortName;
        }
    }

}
