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


    $Id: PN.cs 48 2007-03-28 13:49:15Z agnandt $
*/
using System;
using openDicom.DataStructure;
using openDicom.Encoding.Type;


namespace openDicom.Encoding
{

    /// <summary>
    ///     This class represents the specific DICOM VR Person Name (PN).
    /// </summary>
    public sealed class PersonName: ValueRepresentation
    {
        public PersonName(Tag tag): base("PN", tag) {}
        
        public override string ToLongString()
        {
            return "Person Name (PN)";
        }

        protected override Array DecodeImproper(byte[] bytes)
        {
            string s = TransferSyntax.ToString(bytes);
            s = s.TrimEnd(null);
            string[] multiValue = ToImproperMultiValue(s);
            Type.PersonName[] personName = new Type.PersonName[multiValue.Length];
            for (int i = 0; i < personName.Length; i++)
            {
                string item = multiValue[i];
                personName[i] = new Type.PersonName(item);
            }
            return personName;
        }
        
        protected override Array DecodeProper(byte[] bytes)
        {
            string s = TransferSyntax.ToString(bytes);
            if (s.Length < 64 * 5)
            {
                s = s.TrimEnd(null);
                string[] multiValue = ToProperMultiValue(s);
                Type.PersonName[] personName = 
                    new Type.PersonName[multiValue.Length];
                for (int i = 0; i < personName.Length; i++)
                {
                    string item = multiValue[i];
                    personName[i] = new Type.PersonName(item);
                }
                return personName;
            }
            else
                throw new EncodingException(
                    "A value of max. 64 * 5 characters is only allowed.",
                    Tag, Name + "/s", s);
        }
    }

}
