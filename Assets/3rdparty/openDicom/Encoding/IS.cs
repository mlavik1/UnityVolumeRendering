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


    $Id: IS.cs 48 2007-03-28 13:49:15Z agnandt $
*/
using System;
using openDicom.DataStructure;


namespace openDicom.Encoding
{

    /// <summary>
    ///     This class represents the specific DICOM VR Integer String (IS).
    /// </summary>
    public sealed class IntegerString: ValueRepresentation
    {
        public IntegerString(Tag tag): base("IS", tag) {}
        
        public override string ToLongString()
        {
            return "Integer String (IS)";
        }

        protected override Array DecodeImproper(byte[] bytes)
        {
            string s = TransferSyntax.ToString(bytes);
            string[] multiValue = ToImproperMultiValue(s);
            long[] intValue = new long[multiValue.Length];
            for (int i = 0; i < intValue.Length; i++)
            {
                string item = multiValue[i];
                item = item.Trim();
                try
                {
                    if (item.Length > 0)
                        intValue[i] = long.Parse(item);
                }
                catch (Exception e)
                {
                   throw new EncodingException(
                      "Integer string format is invalid.", Tag,
                      Name + "/item", item);
                }
            }
            return intValue;
        }
        
        protected override Array DecodeProper(byte[] bytes)
        {
            string s = TransferSyntax.ToString(bytes);
            string[] multiValue = ToProperMultiValue(s);
            long[] intValue = new long[multiValue.Length];
            for (int i = 0; i < intValue.Length; i++)
            {
                string item = multiValue[i];
                if (item.Length <= 12)
                {
                    item = item.Trim();
                    try
                    {
                        if (item.Length > 0)
                            intValue[i] = long.Parse(item);
                    }
                    catch (Exception e)
                    {
                        throw new EncodingException(
                            "Integer string format is invalid.", Tag,
                            Name + "/item", item);
                    }
                }
                else
                    throw new EncodingException(
                        "A value of max. 12 bytes is only allowed.", Tag,
                        Name + "/item", item);
            }
            return intValue;
        }
    }

}
