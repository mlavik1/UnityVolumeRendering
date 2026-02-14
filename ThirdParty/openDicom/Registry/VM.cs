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


    $Id: VM.cs 48 2007-03-28 13:49:15Z agnandt $
*/
using System;
using System.IO;
using System.Text.RegularExpressions;
using openDicom.DataStructure;
using openDicom.DataStructure.DataSet;


namespace openDicom.Registry
{

    /// <summary>
    ///     This class represents DICOM value multiplicity (VM).
    /// </summary>
    public sealed class ValueMultiplicity
    {
        private ValueRepresentation vr = null;
        /// <summary>
        ///     Corresponding DICOM value representation.
        /// </summary>      
        public ValueRepresentation VR
        {
            get 
            { 
                if (vr != null)
                    return vr; 
                else
                    throw new DicomException("ValueMultiplicity.VR is null.");
            }
        }

        private string vm = "0";
        /// <summary>
        ///     DICOM VM as string representation. For example, "1", "1-3",
        ///     "1-n" or "2-2n" are possible string representations.
        /// </summary>      
        public string Value
        {
            set
            {
                if (value == null || value.Equals("")) value = "0";
                value.ToLower();
                value = value.Replace(" ", null);
                if (Regex.IsMatch(value, 
                    "^ (([0-9]*n | [0-9]+n?) | [0-9]+ - ([0-9]*n | [0-9]+n?)) $",
                    RegexOptions.IgnorePatternWhitespace))
                {
                    vm = value;
                    string[] s = vm.Split('-');
                    if (s.Length == 2)
                    {
                        lowerFactor = int.Parse(s[0]);
                        isUnbounded = Regex.IsMatch(s[1], "^[0-9]*n$");
                        if (IsUnbounded)
                        {
                            s[1] = s[1].Replace("n", null);
                            if (s[1].Equals("")) s[1] = "1";
                        }    
                        upperFactor = int.Parse(s[1]);
                    }
                    else
                    {
                        isUnbounded = Regex.IsMatch(s[0], "^[0-9]*n$");
                        if (IsUnbounded)
                        {
                            s[0] = s[0].Replace("n", null);
                            if (s[0].Equals("")) s[0] = "1";
                        }
                        lowerFactor = int.Parse(s[0]);
                        upperFactor = lowerFactor;
                    }
                }
                else
                    throw new DicomException("VM is invalid.", "VM", value);
            }

            get { return vm; }
        }

        private int lowerFactor = 0;
        /// <summary>
        ///     Lower factor of a VM. In case of "1-3", the lower factor is 1.
        ///     If the DICOM VM is only one number, the lower and upper factors
        ///     will be equal.
        /// </summary>      
        public int LowerFactor
        {
            get { return lowerFactor; }
        }

        private int upperFactor = 0;
        /// <summary>
        ///     Upper factor of a VM. In case of "1-3", the upper factor is 3.
        ///     If the DICOM VM is only one number, the lower and upper factors
        ///     will be equal. In case of use of variables like "2-2n", the
        ///     upper factor will only be the corresponding multiplier. In this
        ///     example the upper factor will be 2.
        /// </summary>      
        public int UpperFactor
        {
            get { return upperFactor; }
        }

        private bool isUnbounded = false;
        /// <summary>
        ///     Returns whether a VM instance is unbounded which means
        ///     the upper factor is only a multiplier and not determined. For
        ///     example, "1-3" is not unbounded unlike "2-2n".
        /// </summary>
        public bool IsUnbounded
        {
            get { return isUnbounded; }
        }

        /// <summary>
        ///     Returns whether a VM instance is undefined. An undefined VM
        ///     equals to a string representation of "0". A data element with
        ///     a undefined VM is not registered to the used data element
        ///     dictionary.
        /// </summary>
        public bool IsUndefined
        {
            get { return lowerFactor == 0 || upperFactor == 0; }
        }


        public ValueMultiplicity(ValueRepresentation vr)
        {
            this.vr = vr;
        }

        public ValueMultiplicity(ValueRepresentation vr, int vm): 
            this(vr, vm.ToString()) {}

        public ValueMultiplicity(ValueRepresentation vr, string vm): this(vr)
        {
            Value = vm;
        }

        /// <summary>
        ///     Determines whether a specified count of values is conform with
        ///     this DICOM VM instance. This method is supposed to be used
        ///     for validation of real count of DICOM values against the
        ///     corresponding DICOM data element dictionary entry.
        /// </summary>
        /// <remarks>
        ///     If this VM instance is not registered in the data element
        ///     dictionary or this instance is undefined, all specified
        ///     count of values will be valid.
        /// </remarks>
        public bool IsValid(int valueCount)
        {
            if (IsUndefined && valueCount >= 1) 
                return true;
            else if (valueCount >= lowerFactor)
            {
                if (IsUnbounded)
                {
                    if (upperFactor > 1)
                    {
                        return valueCount % upperFactor == 0;
                    }
                    else
                        return true;
                }
                else
                    return valueCount <= upperFactor;
            }
            else
                return false;
        }

        /// <summary>
        ///     Determines whether this VM instance is exactly equal to
        ///     a specified count of values. Equality can only be given by
        ///     defined, bounded and invariable DICOM VMs like "1" or "3".
        /// </summary>
        public bool Equals(int valueCount)
        {
            return ( ! IsUndefined && ! IsUnbounded && 
                lowerFactor == upperFactor && valueCount == lowerFactor);
        }

        /// <summary>
        ///     Returns the string representation of a VM instance.
        /// </summary>
        public override string ToString()
        {
            return Value;
        }
    }

}
