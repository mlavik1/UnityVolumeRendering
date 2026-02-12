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


    $Id: FD.cs 93 2007-04-02 21:37:46Z agnandt $
*/
using System;
using openDicom.DataStructure;


namespace openDicom.Encoding
{

    /// <summary>
    ///     This class represents the specific DICOM VR Floating Point Double (FD).
    /// </summary>
    public sealed class FloatingPointDouble: ValueRepresentation
    {
        public FloatingPointDouble(Tag tag): base("FD", tag) {}
        
        public override string ToLongString()
        {
            return "Floating Point Double (FD)";
        }

        protected override Array DecodeImproper(byte[] bytes)
        {
            byte[][] multiValue = ToImproperMultiValue(bytes, 8);
            double[] number = new double[multiValue.Length];
            for (int i = 0; i < number.Length; i++)
                number[i] = BitConverter.ToDouble(
                    TransferSyntax.CorrectByteOrdering(multiValue[i]), 0);
            return number;
        }
        
        protected override Array DecodeProper(byte[] bytes)
        {
            byte[][] multiValue = ToProperMultiValue(bytes, 8);
            if (bytes.Length == 8 * multiValue.Length)
            {
                double[] number = new double[multiValue.Length];
                for (int i = 0; i < number.Length; i++)
                    number[i] = BitConverter.ToDouble(
                        TransferSyntax.CorrectByteOrdering(multiValue[i]), 0);
                return number;
            }
            else
                throw new EncodingException(
                    "A value of multiple 8 bytes is only allowed.", Tag,
                    Name + "/value.Length", bytes.Length.ToString());
        }
    }

}
