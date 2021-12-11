using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileManager
{
    /// <summary>
    /// Includes methods working with display output.
    /// </summary>
    internal static partial class Display
    {
        /// <summary>
        /// Headings of a table of files and directories.
        /// </summary>
        private static readonly string[] DirectoryTableHeadings = {"Name", "Size", "Date", "Time"};
        
        /// <summary>
        /// Column alignments of a table of files and directories.
        /// </summary>
        private static readonly bool[] DirectoryTableAlign = {false, true, true, true};

        /// <summary>
        /// Displays directory contents.
        /// </summary>
        /// <param name="dir">Directory to display.</param>
        public static void DisplayDirectory(DirectoryInfo dir)
        {
            DisplayFileSystemInfos(dir, dir.GetFileSystemInfos());
        }

        /// <summary>
        /// Displays search results of searching using expression.
        /// </summary>
        /// <param name="currentDir">Current directory.</param>
        /// <param name="expression">Search expression.</param>
        /// <exception cref="ArgumentException">Expression is null or empty.</exception>
        public static void DisplayFind(DirectoryInfo currentDir, string expression = "*")
        {
            if (string.IsNullOrEmpty(expression))
            {
                throw new ArgumentException();
            }

            // searchPattern is a dir
            if (expression[^1] == Path.DirectorySeparatorChar)
            {
                expression += "*";
            }
            
            FileSystemInfo[] items = FileOperations
                .EnumerateByExpression(Path.Combine(currentDir.FullName, expression)).ToArray();
            DisplayFileSystemInfos(currentDir, items, Path.IsPathFullyQualified(expression));
        }
        
        /// <summary>
        /// Displays search results of searching files by pattern in a selected directory.
        /// </summary>
        /// <param name="currentDir">Current directory.</param>
        /// <param name="baseDir">Base directory of search.</param>
        /// <param name="searchPattern">Search pattern.</param>
        /// <param name="recursive">Whether to search recursive.</param>
        /// <exception cref="ArgumentException">Pattern is null or empty.</exception>
        public static void DisplayFind(DirectoryInfo currentDir, DirectoryInfo baseDir, string searchPattern = "*",
            bool recursive = false)
        {
            if (string.IsNullOrEmpty(searchPattern))
            {
                throw new ArgumentException();
            }
            if (recursive && !FileOperations.ConfirmRecursive())
            {
                return;
            }

            FileSystemInfo[] items = recursive
                ? FileOperations.FindRecursive(baseDir, searchPattern).ToArray()
                : baseDir.GetFileSystemInfos(searchPattern);
                DisplayFileSystemInfos(baseDir, items);
        }
        
        /// <summary>
        /// Truncates path to fit in specified length.
        /// </summary>
        /// <param name="path">Path to truncate.</param>
        /// <param name="length">Target length.</param>
        /// <returns>Truncated path.</returns>
        public static string TruncatePath(string path, int length)
        {
            if (path.Length <= length)
            {
                return path;
            }

            string dir = Path.GetDirectoryName(path);
            string file = Path.GetFileName(path);

            if (dir is not null)
            {
                dir = dir[..Math.Max(length - file.Length - 4, 1)] + "...";
            }

            path = dir is null ? file : Path.Combine(dir, file);

            if (path.Length > length)
            {
                path = path[..Math.Max(length - 3, 1)] + "...";
            }

            return path;
        }

        /// <summary>
        /// Displays a list of files and directories.
        /// </summary>
        /// <param name="baseDir">Base directory of display.</param>
        /// <param name="items">Items to display.</param>
        /// <param name="isFullyQualified">Whether to display full path or relative.</param>
        private static void DisplayFileSystemInfos(DirectoryInfo baseDir, FileSystemInfo[] items,
            bool isFullyQualified = false)
        {
            IEnumerable<DirectoryInfo> subdirectories = items.OfType<DirectoryInfo>();
            IEnumerable<FileInfo> files = items.OfType<FileInfo>();
            // Convert each directory entry to table row.
            var dirData = subdirectories
                .Where(subDir => subDir.Exists)
                .OrderBy(subDir => subDir.FullName)
                .Select(subDir => new[]
                {
                    TruncatePath(isFullyQualified ? subDir.FullName 
                            : Path.GetRelativePath(baseDir.FullName, subDir.FullName), 
                        Console.WindowWidth - 41) + Path.DirectorySeparatorChar,
                    "Dir",
                    subDir.LastWriteTime.ToShortDateString(),
                    subDir.LastWriteTime.ToShortTimeString()
                });
            // Convert each file entry to table row.
            var fileData = files
                .Where(file => file.Exists)
                .OrderBy(file => file.FullName)
                .Select(file => new[]
                {
                    TruncatePath(isFullyQualified ? file.FullName
                            : Path.GetRelativePath(baseDir.FullName, file.FullName),
                        Console.WindowWidth - 40),
                    GetHumanReadableSize(file.Length),
                    file.LastWriteTime.ToShortDateString(),
                    file.LastWriteTime.ToShortTimeString()
                });

            string[][] data = dirData.Concat(fileData).ToArray();
            DisplayTable(DirectoryTableHeadings, data, DirectoryTableAlign);
        }
    }
}