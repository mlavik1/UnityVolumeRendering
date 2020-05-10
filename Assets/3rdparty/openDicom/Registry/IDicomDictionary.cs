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


    $Id: IDicomDictionary.cs 48 2007-03-28 13:49:15Z agnandt $
*/


namespace openDicom.Registry
{

    /// <summary>
    ///     Available file formats for dictionary files.
    /// </summary>      
    public enum DictionaryFileFormat
    {
        /// <summary>
        ///     Dictionary content is stored not human readable. The implemented
        ///     binary format is not standardized. Use of file extensions .dic 
        ///     is recommended.
        /// </summary>      
        BinaryFile,
        /// <summary>
        ///     Dictionary content is stored in form of a property text file,
        ///     well-known in context of the Java configuration environment.
        ///     Files of this format mostly have the extension .properties.
        /// </summary>      
        PropertyFile,
        /// <summary>
        ///     Dictionary content is stored in form of a XML text file.
        /// </summary>      
        XmlFile,
        /// <summary>
        ///     Dictionary content is stored in form of a CSV (Comma Seperated
        ///     Values) text file, well-known in context of database batch
        ///     jobs.
        /// </summary>
        CsvFile
    }

    /// <summary>
    ///     Dictionary interface for DICOM registry dictionaries. Each
    ///     dictionary is supposed to implement this common interface.
    /// </summary>      
    public interface IDicomDictionary
    {   
        /// <summary>
        ///     Re-creates a dictionary instance from a file of specified
        ///     format.
        /// </summary>      
        void LoadFrom(string fileName, DictionaryFileFormat fileFormat);

        /// <summary>
        ///     Saves dictionary instance content to a file of specified
        ///     format.
        /// </summary>      
        void SaveTo(string fileName, DictionaryFileFormat fileFormat);
    }
    
}
