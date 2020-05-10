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


    $Id: CharacterRepertoire.cs 48 2007-03-28 13:49:15Z agnandt $
*/
using System;
using System.Text;
using openDicom.DataStructure;


namespace openDicom.Encoding
{

    /// <summary>
    ///     This class represents a DICOM character repertoire.
    /// </summary>
    public sealed class CharacterRepertoire
    {
        /// <summary>
        ///     DICOM tag (0008,0005).
        /// </summary>
        public static readonly Tag CharacterSetTag = new Tag("0008", "0005");

        /// <summary>
        ///     DICOM default character repertoire (ISO_IR 6).
        /// </summary>
        public static readonly CharacterRepertoire Default = 
            new CharacterRepertoire("ISO_IR 6");

        /// <summary>
        ///     DICOM ASCII character repertoire.
        /// </summary>
        public static readonly CharacterRepertoire Ascii = 
            new CharacterRepertoire("ASCII");

        /// <summary>
        ///     DICOM UTF-8 character repertoire.
        /// </summary>
        public static readonly CharacterRepertoire Utf8 =
            new CharacterRepertoire("UTF-8");

        /// <summary>
        ///     DICOM character repertoire G0 (ISO_IR 6).
        /// </summary>
        public static readonly CharacterRepertoire G0 = 
            new CharacterRepertoire("ISO_IR 6");

        /// <summary>
        ///     DICOM character repertoire G1 (ISO_IR 100).
        /// </summary>
        public static readonly CharacterRepertoire G1 = 
            new CharacterRepertoire("ISO_IR 100");

        private System.Text.Encoding encoding = null;
        /// <summary>
        ///     DICOM character repertoire text encoding.
        /// </summary>
        public System.Text.Encoding Encoding
        {
            get { return encoding; }
        }


        public CharacterRepertoire(): this(null) {}

        /// <summary>
        ///     Creates a new DICOM character repertoire instance from
        ///     specified character set or map.
        /// </summary>
        public CharacterRepertoire(string characterSet)
        {
            if (characterSet == null) characterSet = "";
            characterSet = characterSet.ToUpper()
                .Replace(" ", null)
                .Replace("-", null)
                .Replace("_", null);
            switch (characterSet)
            {
                case "":                case "ISOIR6":
                case "ASCII":
                    this.encoding = System.Text.Encoding.ASCII;
                    break;
                case "ISOIR100":
                case "ISO88591":
                    this.encoding = 
                        System.Text.Encoding.GetEncoding("ISO-8859-1");
                    break;
                case "ISOIR101":
                case "ISO88592":
                    this.encoding = 
                        System.Text.Encoding.GetEncoding("ISO-8859-2");
                    break;
                case "ISOIR109":
                case "ISO88593":
                    this.encoding = 
                        System.Text.Encoding.GetEncoding("ISO-8859-3");
                    break;
                case "ISOIR110":
                case "ISO88594":
                    this.encoding = 
                        System.Text.Encoding.GetEncoding("ISO-8859-4");
                    break;
                case "ISOIR144":
                case "ISO88595":
                    this.encoding = 
                        System.Text.Encoding.GetEncoding("ISO-8859-5");
                    break;
                case "ISOIR127":
                case "ISO88596":
                    this.encoding = 
                        System.Text.Encoding.GetEncoding("ISO-8859-6");
                    break;
                case "ISOIR126":
                case "ISO88597":
                    this.encoding = 
                        System.Text.Encoding.GetEncoding("ISO-8859-7");
                    break;
                case "ISOIR138":
                case "ISO88598":
                    this.encoding = 
                        System.Text.Encoding.GetEncoding("ISO-8859-8");
                    break;
                case "ISOIR148":
                case "ISO88599":
                    this.encoding = 
                        System.Text.Encoding.GetEncoding("ISO-8859-9");
                    break;
                case "ISOIR192":
                case "UTF8":
                    this.encoding = System.Text.Encoding.UTF8;
                    break;
                default:
                    throw new DicomException("Encoding is not supported.",
                        "characterSet", characterSet);
                    break;
            }
        }
    }

}
