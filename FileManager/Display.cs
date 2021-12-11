using System;
using System.Collections.Generic;
using System.Linq;
using FileManager.Resources;

namespace FileManager
{
    /// <summary>
    /// Includes methods working with display output.
    /// </summary>
    internal static partial class Display
    {
        /// <summary>
        /// Tab size in console.
        /// </summary>
        private const int TabSize = 4;
        
        /// <summary>
        /// Erases last line in console.
        /// </summary>
        /// <param name="lineLen">Number of characters to erase.</param>
        public static void EraseLine(int lineLen)
        {
            Console.CursorLeft = 0;
            Console.Write(new string(' ', lineLen));
            Console.CursorLeft = 0;
        }
        
        /// <summary>
        /// Displays a pager stop.
        /// </summary>
        /// <returns>Whether user wants to continue viewing.</returns>
        private static bool PagerStop()
        {
            string msg = Messages.MessagePagerStop;
            Console.Write(msg);
            do
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                switch (keyInfo.Key)
                {
                    case ConsoleKey.Spacebar:
                        EraseLine(msg.Length);
                        return true;
                    case ConsoleKey.Q:
                    case ConsoleKey.Escape:
                        EraseLine(msg.Length);
                        return false;
                }
            } while (true);
        }

        /// <summary>
        /// Splits string to console lines of specified length.
        /// </summary>
        /// <param name="str">String.</param>
        /// <param name="length">Length of line.</param>
        /// <returns>Enumerable of lines.</returns>
        private static IEnumerable<string> SplitToConsoleLines(string str, int length)
        {
            if (string.IsNullOrEmpty(str))
            {
                yield return string.Empty;
                yield break;
            }

            str = new string(str.Where(c => c != 27).ToArray());
            
            var i = 0;
            while (i + length <= str.Length)
            {
                yield return str.Substring(i, length);
                i += length;
            }

            if (i != str.Length)
            {
                yield return str[i..];
            }
        }

        /// <summary>
        /// Displays data in table form.
        /// </summary>
        /// <param name="headings">Headings of columns.</param>
        /// <param name="data">Data to display.</param>
        /// <param name="alignRight">Whether to align each column to the right or not.</param>
        /// <exception cref="ArgumentException">Array sizes do not match.</exception>
        private static void DisplayTable(string[] headings, string[][] data, bool[] alignRight = null)
        {
            // Process the arguments.
            int columns = headings.Length;
            alignRight ??= new bool[columns];
            if (alignRight.Length != columns)
            {
                throw new ArgumentException("Array sizes must match.");
            }
            
            // Get column widths.
            int[] columnWidths = GetTableColumnWidths(headings, data);

            // Display the table.
            string header = string.Join("  ", PadColumns(headings, columnWidths, alignRight));
            Console.WriteLine(header);
            Console.WriteLine(new string('-', header.Length));

            var currentLinesInPage = 2;
            foreach (string[] entry in data)
            {
                if (currentLinesInPage + 1 >= Console.WindowHeight)
                {
                    if (!PagerStop())
                    {
                        return;
                    }
                    currentLinesInPage = 0;
                }
                Console.WriteLine(string.Join("  ", PadColumns(entry, columnWidths, alignRight)));
                currentLinesInPage++;
            }
        }

        /// <summary>
        /// Calculates table column widths.
        /// </summary>
        /// <param name="headings">Headings of columns.</param>
        /// <param name="data">Table data.</param>
        /// <returns>Array of column widths.</returns>
        private static int[] GetTableColumnWidths(string[] headings, string[][] data)
        {
            int columns = headings.Length;
            
            // Calculate maximum data widths per column.
            int[] columnWidths = headings.Select(s => s.Length).ToArray();
            foreach (string[] entry in data)
            {
                for (var i = 0; i < columns; i++)
                {
                    if (entry[i].Length > columnWidths[i])
                    {
                        columnWidths[i] = entry[i].Length;
                    }
                }
            }
            // Round widths to nearest tab stop.
            columnWidths = columnWidths.Select(width => width + (width + 2) % TabSize).ToArray();
            
            return columnWidths;
        }

        /// <summary>
        /// Pads each column of table row to match the table column widths.
        /// </summary>
        /// <param name="entry">Row of a table.</param>
        /// <param name="columnWidths">Widths of columns.</param>
        /// <param name="alignRight">Whether to align each column to the right or not.</param>
        /// <returns>Enumerable of padded entries.</returns>
        private static IEnumerable<string> PadColumns(string[] entry, int[] columnWidths, bool[] alignRight)
        {
            return entry.Select((value, i) =>
                alignRight[i] ? value.PadLeft(columnWidths[i]) : value.PadRight(columnWidths[i]));
        }

        /// <summary>
        /// Converts size to human readable format.
        /// </summary>
        /// <param name="bytes">Size in bytes.</param>
        /// <returns>Human readable representation of size.</returns>
        private static string GetHumanReadableSize(long bytes)
        {
            char[] suffixes = {'B', 'k', 'M', 'G', 'T'};
            float size = bytes;
            var power = 0;
            while (size >= 10000 && power < suffixes.Length - 1)
            {
                size /= 1024;
                power++;
            }

            return size.ToString("G4") + suffixes[power];
        }
    }
}