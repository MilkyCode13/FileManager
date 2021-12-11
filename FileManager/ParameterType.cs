using System;

namespace FileManager
{
    [Flags]
    public enum ParameterType : ushort
    {
        /// <summary>
        /// Parameter is a path to a directory.
        /// </summary>
        DirectoryPath = 1 << 0,
        
        /// <summary>
        /// Parameter is a path to a file.
        /// </summary>
        FilePath = 1 << 1,
        
        /// <summary>
        /// Parameter is a drive name.
        /// </summary>
        Drive = 1 << 2,
        
        /// <summary>
        /// Parameter is an encoding.
        /// </summary>
        Encoding = 1 << 3,
        
        /// <summary>
        /// Parameter is a help string.
        /// </summary>
        HelpString = 1 << 4
    }
}