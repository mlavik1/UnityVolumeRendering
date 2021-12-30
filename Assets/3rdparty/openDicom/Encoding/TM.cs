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


    $Id: TM.cs 48 2007-03-28 13:49:15Z agnandt $
*/
using System;
using openDicom.DataStructure;
using System.Text.RegularExpressions;


namespace openDicom.Encoding
{

    /// <summary>
    ///     This class represents the specific DICOM VR Time (TM).
    /// </summary>
    public sealed class Time: ValueRepresentation
    {
        public Time(Tag tag): base("TM", tag) {}
        
        public override string ToLongString()
        {
            return "Time (TM)";
        }

        protected override Array DecodeImproper(byte[] bytes)
        {
            string s = TransferSyntax.ToString(bytes);
            string[] multiValue = ToImproperMultiValue(s);
            TimeSpan[] time = new TimeSpan[multiValue.Length];
            for (int i = 0; i < time.Length; i++)
            {
                string item = multiValue[i];
                if (item.Length > 0)
                {
                    item = item.TrimEnd(null);
                    if (Regex.IsMatch(item, 
                        "^[0-9]{2}(:?[0-9]{2}(:?[0-9]{2}(\\.[0-9]{1,6})?)?)?$"))
                    {
                        item = item.Replace(":", null).Replace(".", null);
                        string hour = item.Substring(0, 2);
                        string minute = "0";
                        if (item.Length > 2) minute = item.Substring(2, 2);
                        string second = "0";
                        if (item.Length > 4) second = item.Substring(4, 2);
                        string millisecond = "0";
                        if (item.Length > 6) 
                            millisecond = item.Substring(6, item.Length - 6);
                        try
                        {
                            time[i] = new TimeSpan(0, int.Parse(hour), 
                                int.Parse(minute), int.Parse(second),
                                int.Parse(millisecond));
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogWarning($"Date time format is invalid. tag: {Tag}, name: {Name}, item: {item}");
                            time[i] = TimeSpan.Zero;
                        }
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning($"Time format is invalid. tag: {Tag}, name: {Name}, item: {item}");
                        time[i] = TimeSpan.Zero;
                    }
                }
            }
            return time;
        }
        
        protected override Array DecodeProper(byte[] bytes)
        {
            string s = TransferSyntax.ToString(bytes);
            string[] multiValue = ToProperMultiValue(s);
            TimeSpan[] time = new TimeSpan[multiValue.Length];
            for (int i = 0; i < time.Length; i++)
            {
                string item = multiValue[i];
                if (item.Length > 0)
                {
                    item = item.TrimEnd(null);
                    if (Regex.IsMatch(item, 
                        "^[0-9]{2}(:?[0-9]{2}(:?[0-9]{2}(\\.[0-9]{1,6})?)?)?$"))
                    {
                        item = item.Replace(":", null).Replace(".", null);
                        string hour = item.Substring(0, 2);
                        string minute = "0";
                        if (item.Length > 2) minute = item.Substring(2, 2);
                        string second = "0";
                        if (item.Length > 4) second = item.Substring(4, 2);
                        string millisecond = "0";
                        if (item.Length > 6) 
                            millisecond = item.Substring(6, item.Length - 6);
                        try
                        {
                            time[i] = new TimeSpan(0, int.Parse(hour), 
                                int.Parse(minute), int.Parse(second),
                                int.Parse(millisecond));
                        }
                        catch (Exception e)
                        {
                            throw new EncodingException(
                                "Time format is invalid.",
                                Tag, Name + "/item", item);
                        }
                    }
                    else
                        throw new EncodingException("Time format is invalid.",
                            Tag, Name + "/item", item);
                }
            }
            return time;
        }
    }

}
