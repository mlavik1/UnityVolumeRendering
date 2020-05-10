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
using System.Text.RegularExpressions;
using openDicom;


namespace openDicom.Encoding.Type
{
    /// <summary>
    ///     All available age measuring units.
    /// </summary>
    public enum AgeContext
    {
        /// <summary>
        ///     DICOM VR Age String (AS) representation is "xxxD".
        /// </summary>
        Days,
        /// <summary>
        ///     DICOM VR Age String (AS) representation is "xxxW".
        /// </summary>
        Weeks,
        /// <summary>
        ///     DICOM VR Age String (AS) representation is "xxxM".
        /// </summary>
        Months,
        /// <summary>
        ///     DICOM VR Age String (AS) representation is "xxxY".
        /// </summary>
        Years
    }


    /// <summary>
    ///     This class represents a single DICOM Age String (AS) value.
    /// </summary>
    public sealed class Age
    {
        private const char days   = 'D';
        private const char weeks  = 'W';
        private const char months = 'M';
        private const char years  = 'Y';

        private AgeContext context = AgeContext.Days;
        /// <summary>
        ///     Returns the choosen age measuring unit.
        /// </summary>
        public AgeContext Context
        {
            get { return context; }
        }

        /// <summary>
        ///     Access <see cref="Context" /> for measuring unit in days.
        /// </summary>
        /// <remarks>
        ///     Only one measuring unit can be assigned to this instance at
        ///     once. If this property is set on true, <see cref="Context" />
        ///     will be re-assigned to days.
        /// </remarks>
        public bool IsDays
        {
            set { if (value) context = AgeContext.Days; }
            get { return context == AgeContext.Days; }
        }

        /// <summary>
        ///     Access <see cref="Context" /> for measuring unit in weeks.
        /// </summary>
        /// <remarks>
        ///     Only one measuring unit can be assigned to this instance at
        ///     once. If this property is set on true, <see cref="Context" />
        ///     will be re-assigned to weeks.
        /// </remarks>
        public bool IsWeeks
        {
            set { if (value) context = AgeContext.Weeks; }
            get { return context == AgeContext.Weeks; }
        }

        /// <summary>
        ///     Access <see cref="Context" /> for measuring unit in months.
        /// </summary>
        /// <remarks>
        ///     Only one measuring unit can be assigned to this instance at
        ///     once. If this property is set on true, <see cref="Context" />
        ///     will re-assigned to months.
        /// </remarks>
        public bool IsMonths
        {
            set { if (value) context = AgeContext.Months; }
            get { return context == AgeContext.Months; }
        }

        /// <summary>
        ///     Access <see cref="Context" /> for measuring unit in years.
        /// </summary>
        /// <remarks>
        ///     Only one measuring unit can be assigned to this instance at
        ///     once. If this property is set on true, <see cref="Context" />
        ///     will be re-assigned to years.
        /// </remarks>
        public bool IsYears
        {
            set { if (value) context = AgeContext.Years; }
            get { return context == AgeContext.Years; }
        }

        private int ageValue = 0;
        /// <summary>
        ///     Access of age value.
        /// </summary>
        public int AgeValue
        {
            set 
            { 
                if (value >= 0)
                    ageValue = value; 
                else
                    throw new DicomException("Age cannot be negativ.", 
                        "Age.AgeValue", value.ToString());
            }
            get { return ageValue; }
        }


        /// <summary>
        ///     Creates a new age instance from specified age string of
        ///     format "xxxM", where "xxx" is a decimal number, the age value, 
        ///     like "013" and "M", is the age context or measuring unit.
        /// </summary>
        public Age(string ageString)
        {
            if (Regex.IsMatch(ageString, 
                "^[0-9]{3}[" + days + weeks + months + years + "]$"))
            {
                AgeValue = int.Parse(ageString.Substring(0, 3));
                char context = (char) ageString[3];
                switch (context)
                {
                    case days: IsDays = true; break;
                    case weeks: IsWeeks = true; break;
                    case months: IsMonths = true; break;
                    case years: IsYears = true; break;
                    default:
                        throw new DicomException("Age context is invalid.",
                            "ageString", ageString);
                        break;
                }
            }
            else
                throw new DicomException("Age string is invalid.", "ageString",
                    ageString);
        }

        /// <summary>
        ///     Creates a new age instance from specified age value and
        ///     context.
        /// </summary>
        public Age(int ageValue, AgeContext context)
        {
            this.context = context;
            AgeValue = ageValue;
        }

        /// <summary>
        ///     Creates a new age instance from specified age value and
        ///     context as single character. Only DICOM VR Age String (AS)
        ///     measuring units are allowed (see <see cref="AgeContext" />).
        /// </summary>
        public Age(int ageValue, char context)
        {
            switch (context)
            {
                case days: IsDays = true; break;
                case weeks: IsWeeks = true; break;
                case months: IsMonths = true; break;
                case years: IsYears = true; break;
                default:
                    throw new DicomException("Age context is invalid.",
                        "context", context.ToString());
                    break;
            }
            AgeValue = ageValue;
        }

        /// <summary>
        ///     Returns the DICOM VR Age String (AS) representation of this
        ///     instance. Format is "xxxM", where "xxx" is a decimal number
        ///     and "M" is a single character that represents the
        ///     <see cref="AgeContext" />.
        /// </summary>
        public override string ToString()
        {
            char charContext = ' ';
            switch (Context)
            {
                case AgeContext.Days: charContext = days; break;
                case AgeContext.Weeks: charContext = weeks; break;
                case AgeContext.Months: charContext = months; break;
                case AgeContext.Years: charContext = years; break;
            }
            return string.Format("{0:D3}", AgeValue) + charContext;
        }
    }

}
