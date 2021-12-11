using System;
using FileManager.Resources;

namespace FileManager
{
    /// <summary>
    /// Includes methods working with display output.
    /// </summary>
    internal static partial class Display
    {
        /// <summary>
        /// Displays a diff.
        /// </summary>
        /// <param name="diff">Diff object.</param>
        public static void DisplayDiff(Diff diff)
        {
            // Display file names with timestamps.
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($@"--- {diff.Path1}	{diff.TimeStamp1:O}");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($@"+++ {diff.Path2}	{diff.TimeStamp2:O}");
            Console.ResetColor();

            var currentLinesInPage = 2;
            // Display each diff fragment.
            foreach (DiffFragment fragment in diff.DiffFragments)
            {
                if (!PrintDiffFragment(fragment, ref currentLinesInPage))
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Prints a diff fragment.
        /// </summary>
        /// <param name="fragment">Diff fragment to display.</param>
        /// <param name="currentLinesInPage">Count of lines already displayed on current page.</param>
        /// <returns>Whether to continue printing or stop.</returns>
        private static bool PrintDiffFragment(DiffFragment fragment, ref int currentLinesInPage)
        {
            // Display a fragment header: begin line number and length for each file.
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(
                $@"@@ -{fragment.BeginLine1},{fragment.Count1} +{fragment.BeginLine2},{fragment.Count2} @@");
            Console.ResetColor();
            currentLinesInPage++;
            // If page is full, display a pager stop.
            if (currentLinesInPage + 1 >= Console.WindowHeight)
            {
                if (!PagerStop())
                {
                    return false;
                }
                currentLinesInPage = 0;
            }

            // Display each line of diff.
            foreach (DiffLine line in fragment.Lines)
            {
                if (!PrintDiffLine(line, ref currentLinesInPage))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Displays a diff line.
        /// </summary>
        /// <param name="line">Diff line to display.</param>
        /// <param name="currentLinesInPage">Count of lines already displayed on current page.</param>
        /// <returns>Whether to continue printing or stop.</returns>
        private static bool PrintDiffLine(DiffLine line, ref int currentLinesInPage)
        {
            switch (line.ChangeType)
            {
                case '-':
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($@"{line.LineNumber1.ToString().PadLeft(4)}      - {line.Line}");
                    Console.ResetColor();
                    break;
                case '+':
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($@"     {line.LineNumber2.ToString().PadLeft(4)} + {line.Line}");
                    Console.ResetColor();
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write(
                        $@"{line.LineNumber1.ToString().PadLeft(4)} {line.LineNumber2.ToString().PadLeft(4)}");
                    Console.ResetColor();
                    Console.WriteLine($@"   {line.Line}");
                    break;
            }

            currentLinesInPage++;
            // If page is full, display a pager stop.
            if (currentLinesInPage + 1 >= Console.WindowHeight)
            {
                if (!PagerStop())
                {
                    return false;
                }
                currentLinesInPage = 0;
            }

            return true;
        }
    }
}