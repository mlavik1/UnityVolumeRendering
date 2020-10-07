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


    $Id: DT.cs 48 2007-03-28 13:49:15Z agnandt $
*/
using System;
using openDicom.DataStructure;
using System.Text.RegularExpressions;


namespace openDicom.Encoding
{

    /// <summary>
    ///     This class represents the specific DICOM VR Date Time (DT).
    /// </summary>
    public sealed class DateTime: ValueRepresentation
    {
        public DateTime(Tag tag): base("DT", tag) {}
        
        public override string ToLongString()
        {
            return "Date Time (DT)";
        }

        protected override Array DecodeImproper(byte[] bytes)
        {
            string s = TransferSyntax.ToString(bytes);
            string[] multiValue = ToImproperMultiValue(s);
            System.DateTime[] dateTime = new System.DateTime[multiValue.Length];
            for (int i = 0; i < dateTime.Length; i++)
            {
                string item = multiValue[i];
                if (item.Length > 0)
                {
                    if (Regex.IsMatch(item, "^ [0-9]{10}" +
                        "([0-9]{2} ([0-9]{2} (\\.[0-9]{6}" +
                        "([\\+\\-][0-9]{4})? )? )? )? $",
                        RegexOptions.IgnorePatternWhitespace))
                    {
                        item = item.Replace(".", null);
                        string year = item.Substring(0, 4);
                        string month = item.Substring(4, 2);
                        string day = item.Substring(6, 2);
                        string hour = "0";
                        if (item.Length > 8) hour = item.Substring(8, 2);
                        string minute = "0";
                        if (item.Length > 10) minute = item.Substring(10, 2);
                        string second = "0";
                        if (item.Length > 12) second = item.Substring(12, 2);
                        string millisecond = "0";
                        if (item.Length > 14) 
                            millisecond = item.Substring(14, 6);
                        string timeZone = "+0";
                        if (item.Length > 20) 
                            timeZone = item.Substring(20, 5);
                        // TODO: What to do with the time zone?
                        try
                        {
                            dateTime[i] = new System.DateTime(int.Parse(year), 
                                int.Parse(month), int.Parse(day), int.Parse(hour),
                                int.Parse(minute), int.Parse(second),
                                int.Parse(millisecond));
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogWarning($"Date time format is invalid. tag: {Tag}, name: {Name}");
                            dateTime[i] = System.DateTime.Now;
                        }
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning($"Date time format is invalid. tag: {Tag}, name: {Name}");
                        dateTime[i] = System.DateTime.Now;
                    }
                }
            }
            return dateTime;
        }
        
        protected override Array DecodeProper(byte[] bytes)
        {
            string s = TransferSyntax.ToString(bytes);
            string[] multiValue = ToProperMultiValue(s);
            System.DateTime[] dateTime = new System.DateTime[multiValue.Length];
            for (int i = 0; i < dateTime.Length; i++)
            {
                string item = multiValue[i];
                if (item.Length > 0)
                {
                    if (Regex.IsMatch(item, "^ [0-9]{10}" +
                        "([0-9]{2} ([0-9]{2} (\\.[0-9]{6}" +
                        "([\\+\\-][0-9]{4})? )? )? )? $",
                        RegexOptions.IgnorePatternWhitespace))
                    {
                        item = item.Replace(".", null);
                        string year = item.Substring(0, 4);
                        string month = item.Substring(4, 2);
                        string day = item.Substring(6, 2);
                        string hour = "0";
                        if (item.Length > 8) hour = item.Substring(8, 2);
                        string minute = "0";
                        if (item.Length > 10) minute = item.Substring(10, 2);
                        string second = "0";
                        if (item.Length > 12) second = item.Substring(12, 2);
                        string millisecond = "0";
                        if (item.Length > 14)
                            millisecond = item.Substring(14, 6);
                        string timeZone = "+0";
                        if (item.Length > 20)
                            timeZone = item.Substring(20, 5);
                        // TODO: What to do with the time zone?
                        try
                        {
                            dateTime[i] = new System.DateTime(int.Parse(year),
                                int.Parse(month), int.Parse(day), int.Parse(hour),
                                int.Parse(minute), int.Parse(second),
                                int.Parse(millisecond));
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogWarning($"Date time format is invalid. tag: {Tag}, name: {Name}");
                            dateTime[i] = System.DateTime.Now;
                        }
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning($"Date time format is invalid. tag: {Tag}, name: {Name}");
                        dateTime[i] = System.DateTime.Now;
                    }
                }
            }
            return dateTime;
        }
    }

}
