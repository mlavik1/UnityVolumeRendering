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


    $Id: AE.cs 48 2007-03-28 13:49:15Z agnandt $
*/
using System;
using openDicom.DataStructure;
using System.Text.RegularExpressions;


namespace openDicom.Encoding
{

    /// <summary>
    ///     This class represents the specific DICOM VR Application Entity (AE).
    /// </summary>
    public sealed class ApplicationEntity: ValueRepresentation
    {
        public ApplicationEntity(Tag tag): base("AE", tag) {}
        
        public override string ToLongString()
        {
            return "Application Entity (AE)";
        }

        protected override Array DecodeImproper(byte[] bytes)
        {
            string s = TransferSyntax.ToString(bytes);
            string[] applicationName = ToImproperMultiValue(s);
            for (int i = 0; i < applicationName.Length; i++)
            {
                string item = applicationName[0];
                applicationName[i] = item.Trim();
            }
            return applicationName;
        }
        
        protected override Array DecodeProper(byte[] bytes)
        {
            string s = TransferSyntax.ToString(bytes);
            string[] applicationName = ToProperMultiValue(s);
            for (int i = 0; i < applicationName.Length; i++)
            {
                string item = applicationName[0];
                if (item.Length > 16)
                    throw new EncodingException(
                        "A value of max. 16 bytes is only allowed.", Tag,
                        Name + "/item", item);
                else if (Regex.IsMatch(item, "^[\\s]{16}$"))
                    throw new EncodingException(
                        "No application name specified.", Tag, Name + "/item",
                        item);
                else if (item.Length > 0)
                    applicationName[i] = item.Trim();
            }
            return applicationName;
        }
    }

}
