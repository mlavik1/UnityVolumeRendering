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


    $Id: AS.cs 48 2007-03-28 13:49:15Z agnandt $
*/
using System;
using openDicom.DataStructure;
using openDicom.Encoding.Type;
using System.Text.RegularExpressions;


namespace openDicom.Encoding
{

    /// <summary>
    ///     This class represents the specific DICOM VR Age String (AS).
    /// </summary>
    public sealed class AgeString: ValueRepresentation
    {
        public AgeString(Tag tag): base("AS", tag) {}
        
        public override string ToLongString()
        {
            return "Age String (AS)";
        }

        protected override Array DecodeImproper(byte[] bytes)
        {
            string s = TransferSyntax.ToString(bytes);
            string[] multiValue = ToImproperMultiValue(s);
            Age[] age = new Age[multiValue.Length];
            for (int i = 0; i < age.Length; i++)
            {
                string item = multiValue[i];
                try
                {
                    if (item == null || item.Equals(""))
                        item = "000D";
                    age[i] = new Age(item);
                }
                catch (Exception e)
                {
                    throw new EncodingException("Age string format is invalid.",
                        Name + "/item", item);
                }
            }
            return age;
        }

        protected override Array DecodeProper(byte[] bytes)
        {
            string s = TransferSyntax.ToString(bytes);
            string[] multiValue = ToProperMultiValue(s);
            Age[] age = new Age[multiValue.Length];
            for (int i = 0; i < age.Length; i++)
            {
                string item = multiValue[i];
                try
                {
                    age[i] = new Age(item);
                }
                catch (Exception e)
                {
                    throw new EncodingException("Age string format is invalid.",
                        Name + "/item", item);
                }
            }
            return age;
        }
    }

}
