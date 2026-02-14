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


    $Id: UI.cs 48 2007-03-28 13:49:15Z agnandt $
*/
using System;
using openDicom.DataStructure;
using openDicom.Registry;


namespace openDicom.Encoding
{

    /// <summary>
    ///     This class represents the specific DICOM VR Unique Identifier (UI).
    /// </summary>
    public sealed class UniqueIdentifier: ValueRepresentation
    {
        public UniqueIdentifier(Tag tag): base("UI", tag) {}
        
        public override string ToLongString()
        {
            return "Unique Identifier (UI)";
        }

        protected override Array DecodeImproper(byte[] bytes)
        {
            string s = TransferSyntax.ToString(bytes);
            string[] multiValue = ToImproperMultiValue(s);
            Uid[] uidValue = new Uid[multiValue.Length];
            for (int i = 0; i < uidValue.Length; i++)
            {
                string item = multiValue[i];
                // trailing zero padding
                if (item.Length > 0)
                {
                    byte b = (byte) item[item.Length - 1];
                    if (b == 0) 
                        item = item.Remove(item.Length - 1, 1);
                    item = item.Replace(" ", null);
                }
                if (item == null || item.Equals(""))
                    item = "0";
                uidValue[i] = new Uid(item);
            }
            return uidValue;
        }
        
        protected override Array DecodeProper(byte[] bytes)
        {
            string s = TransferSyntax.ToString(bytes);
            string[] multiValue = ToProperMultiValue(s);
            Uid[] uidValue = new Uid[multiValue.Length];
            for (int i = 0; i < uidValue.Length; i++)
            {
                string item = multiValue[i];
                if (item.Length > 64)
                    throw new EncodingException(
                        "A value of max. 64 characters is only allowed.",
                        Tag, Name + "/item", item);
                else if (item.Length > 0)
                {
                    // trailing zero padding
                    byte b = (byte) item[item.Length - 1];
                    if (b == 0) 
                        item = item.Remove(item.Length - 1, 1);
                    item = item.Replace(" ", null);
                }
                if (item == null || item.Equals(""))
                    throw new EncodingException("Uid is empty.", Tag,
                        Name + "/item", item);
                uidValue[i] = new Uid(item);
            }
            return uidValue;
        }
    }

}
