using System.Collections.Generic;
using System.Linq;

namespace FileManager
{
    /// <summary>
    /// Includes methods working with display output.
    /// </summary>
    internal static partial class Display
    {
        /// <summary>
        /// Headings of a table of drives.
        /// </summary>
        private static readonly string[] DriveTableHeadings = {"Name", "Type", "File System", "Avail", "Free", "Total"};

        /// <summary>
        /// Column alignments of a table of drives.
        /// </summary>
        private static readonly bool[] DriveTableAlign = {false, false, false, true, true, true};
        
        /// <summary>
        /// Displays drive list.
        /// </summary>
        public static void DisplayDrives()
        {
            IEnumerable<DriveManager> drives = DriveManager.GetDrives();
            string[][] data = drives.Select(drive => new[] {
                drive.Name,
                drive.DriveType.ToString(),
                drive.DriveFormat,
                GetHumanReadableSize(drive.AvailableFreeSpace),
                GetHumanReadableSize(drive.TotalFreeSpace),
                GetHumanReadableSize(drive.TotalSize)
            }).ToArray();
            DisplayTable(DriveTableHeadings, data, DriveTableAlign);
        }
    }
}