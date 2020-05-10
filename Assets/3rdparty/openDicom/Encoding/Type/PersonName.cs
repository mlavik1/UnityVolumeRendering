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


    $Id$
*/
using System;
using openDicom;


namespace openDicom.Encoding.Type
{

    /// <summary>
    ///     This class represents a single DICOM Person Name (PN) value.
    /// </summary>
    /// <remarks>
    ///     TODO: Support of ideographic representation is not implemented.
    /// </remarks>
    public sealed class PersonName
    {
        private string[] innerArray = new string[5] {null, null, null, null, 
            null };

        /// <summary>
        ///     Access this person name instance as array. Index range is
        ///     bounded between 0 (family name) and 4 (name suffix).
        /// </summary>
        public string this[int index]
        {
            set 
            { 
                if (value == null || value.Length < 64)
                    innerArray[index] = value;
                else
                    throw new DicomException(
                        "Length of new entry exceeds 64 characters.", 
                        "PersonName[" + index.ToString() + "]",
                        value);
            }

            get { return innerArray[index]; }
        }

        private const int familyNameIndex = 0;
        private const int givenNameIndex  = 1;
        private const int middleNameIndex = 2;
        private const int namePrefixIndex = 3;
        private const int nameSuffixIndex = 4;

        /// <summary>
        ///     Access person name part family name.
        /// </summary>
        public string FamilyName
        {
            set
            {
                if (value == null || value.Length < 64)
                {
                    fullName = null;
                    innerArray[familyNameIndex] = value;
                }
                else
                    throw new DicomException(
                        "Length of family name exceeds 64 characters.",
                        "PersonName.FamilyName", value);
            }

            get { return innerArray[familyNameIndex]; }
        }

        /// <summary>
        ///     Access person name part given name.
        /// </summary>
        public string GivenName
        {
            set
            {
                if (value == null || value.Length < 64)
                {
                    fullName = null;
                    innerArray[givenNameIndex] = value;
                }
                else
                    throw new DicomException(
                        "Length of given name exceeds 64 characters.",
                        "PersonName.GivenName", value);
            }

            get { return innerArray[givenNameIndex]; }
        }

        /// <summary>
        ///     Access person name part middle name.
        /// </summary>
        public string MiddleName
        {
            set
            {
                if (value == null || value.Length < 64)
                {
                    fullName = null;
                    innerArray[middleNameIndex] = value;
                }
                else
                    throw new DicomException(
                        "Length of middle name exceeds 64 characters.",
                        "PersonName.MiddleName", value);
            }

            get { return innerArray[middleNameIndex]; }
        }

        /// <summary>
        ///     Access person name part name prefix.
        /// </summary>
        public string NamePrefix
        {
            set
            {
                if (value == null || value.Length < 64)
                {
                    fullName = null;
                    innerArray[namePrefixIndex] = value;
                }
                else
                    throw new DicomException(
                        "Length of name prefix exceeds 64 characters.",
                        "PersonName.NamePrefix", value);
            }

            get { return innerArray[namePrefixIndex]; }
        }

        /// <summary>
        ///     Access person name part name suffix.
        /// </summary>
        public string NameSuffix
        {
            set
            {
                if (value == null || value.Length < 64)
                {
                    fullName = null;
                    innerArray[nameSuffixIndex] = value;
                }
                else
                    throw new DicomException(
                        "Length of name suffix exceeds 64 characters.",
                        "PersonName.NameSuffix", value);
            }

            get { return innerArray[nameSuffixIndex]; }
        }

        private string fullName = null;
        /// <summary>
        ///     Access full person name string representation. According
        ///     to the DICOM standard "^" is used as seperator of different
        ///     person name parts.
        /// </summary>
        public string FullName
        {
            set
            {
                fullName = value;
                if (fullName != null)
                {
                    string[] s = fullName.Split('^');
                    int i;
                    for (i = 0; i < s.Length; i++)
                    {
                        if (s[i].Length < 64)
                            innerArray[i] = s[i];
                        else
                            throw new DicomException(
                                "Length of new entry exceeds 64 characters.",
                                "PersonName.FullName/s[ + " + i.ToString() + "]",
                                s[i]);
                    }
                    for (int k = i; k < innerArray.Length; k++)
                        innerArray[k] = null;
                }
            }

            get
            {
                if (fullName == null)
                {
                    fullName = innerArray[0];
                    bool isNotNull = fullName != null;
                    int i = 1;
                    while (isNotNull && i < innerArray.Length)
                    {
                        isNotNull = innerArray[i] != null;
                        if (isNotNull)
                            fullName += "^" + innerArray[i];
                        i++;
                    }
                }
                return fullName;
            }
        }            


        /// <summary>
        ///     Creates a new empty person name instance.
        /// </summary>
        public PersonName() {}

        /// <summary>
        ///     Creates a new person name instance from specified full name.
        ///     All person name parts have to be seperated by "^" according to
        ///     the DICOM standard.
        /// </summary>
        public PersonName(string fullName)
        {
            FullName = fullName;
        }

        /// <summary>
        ///     Creates a new person name instance from the different person
        ///     name parts.
        /// </summary>
        public PersonName(string familyName, string givenName,
            string middleName, string namePrefix, string nameSuffix)
        {
            FamilyName = familyName;
            GivenName = givenName;
            MiddleName = middleName;
            NamePrefix = namePrefix;
            NameSuffix = nameSuffix;
        }
            
        /// <summary>
        ///     Return this person name instance's <see cref="FullName" />.
        /// </summary>
        public override string ToString()
        {
            return FullName;
        }
    }

}
