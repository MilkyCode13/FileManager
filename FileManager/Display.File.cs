using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FileManager.Resources;

namespace FileManager
{
    /// <summary>
    /// Includes methods working with display output.
    /// </summary>
    internal static partial class Display
    {
        /// <summary>
        /// Displays file contents.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <param name="encoding">Encoding of the file.</param>
        public static void DisplayFile(string path, Encoding encoding = null)
        {
            encoding ??= Encoding.Default;
            if (IsFileBinary(path))
            {
                Console.Write(Messages.MessageBinaryFile);
                ConsoleKeyInfo keyInfo = Console.ReadKey();
                Console.WriteLine();
                if (keyInfo.Key != ConsoleKey.Y)
                {
                    return;
                }
            }
            // DisplayStrings(File.ReadLines(path, encoding));
            DisplayStrings(FileOperations.ReadFileLines(path, encoding));
        }

        /// <summary>
        /// Displays file contents.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <param name="encodingStr">String representation of an encoding of the file.</param>
        /// <exception cref="EncodingNotFoundException">Encoding is not found.</exception>
        public static void DisplayFile(string path, string encodingStr)
        {
            try
            {
                Encoding encoding = Encoding.GetEncoding(encodingStr);
                DisplayFile(path, encoding);
            }
            catch (ArgumentException)
            {
                throw new EncodingNotFoundException(encodingStr);
            }
        }

        /// <summary>
        /// Displays strings to screen.
        /// </summary>
        /// <param name="output">Strings to display.</param>
        private static void DisplayStrings(IEnumerable<string> output)
        {
            var currentLinesInPage = 0;
            var fileLineNumber = 0;

            foreach (string line in output)
            {
                fileLineNumber++;
                bool isFirst = true;
                // Ensure that every line fits on the screen.
                foreach (var consoleLine in SplitToConsoleLines(line, Console.WindowWidth - 5))
                {
                    // If page is full, display a pager stop.
                    if (currentLinesInPage + 1 >= Console.WindowHeight)
                    {
                        if (!PagerStop())
                        {
                            return;
                        }
                        currentLinesInPage = 0;
                    }
                    
                    if (isFirst)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write(fileLineNumber.ToString().PadLeft(4));
                        Console.ResetColor();
                        Console.WriteLine($@" {consoleLine}");
                        isFirst = false;
                    }
                    else
                    {
                        Console.WriteLine($@"     {consoleLine}");
                    }
                    
                    currentLinesInPage++;
                }
            }
        }
        
        /// <summary>
        /// Checks if file is likely binary.
        /// </summary>
        /// <param name="path">Path to file to check.</param>
        /// <returns>Whether file is likely binary.</returns>
        private static bool IsFileBinary(string path)
        {
            using var sr = new StreamReader(path);
            int ch;
            var controlCharCount = 0;
            int i;
            for (i = 0; i < 512 && (ch = sr.Read()) != -1; i++)
            {
                if (ch is > 0 and < 8 or > 13 and < 26)
                {
                    controlCharCount++;
                }
            }

            return controlCharCount > 10;
        }
    }
}