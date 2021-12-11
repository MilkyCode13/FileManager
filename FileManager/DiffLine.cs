using System;
using FileManager.Resources;

namespace FileManager
{
    /// <summary>
    /// Represents a line of a diff.
    /// </summary>
    internal readonly struct DiffLine
    {
        /// <summary>
        /// Number of line in the first file, null if absent.
        /// </summary>
        public int? LineNumber1 { get; }
        
        /// <summary>
        /// Number of line in the second file, null if absent.
        /// </summary>
        public int? LineNumber2 { get; }
        
        /// <summary>
        /// Type of change of the line. '+' for addition, '-' for deletion, ' ' for unchanged.
        /// </summary>
        public char ChangeType { get; }
        
        /// <summary>
        /// Line in string form.
        /// </summary>
        public string Line { get; }

        /// <summary>
        /// Constructs a diff line from line numbers, change type and string.
        /// </summary>
        /// <param name="lineNumber1">Number of line in the first file, null if absent.</param>
        /// <param name="lineNumber2">Number of line in the second file, null if absent.</param>
        /// <param name="changeType">Type of change of the line. '+' for addition, '-' for deletion, ' ' for unchanged.</param>
        /// <param name="line">Line in string form.</param>
        /// <exception cref="ArgumentException">Invalid arguments.</exception>
        public DiffLine(int? lineNumber1, int? lineNumber2, char changeType, string line)
        {
            if (!lineNumber1.HasValue && changeType != '+')
            {
                throw new ArgumentException(Messages.ErrorDiffFirstLineNumber, nameof(lineNumber1));
            }
            if (!lineNumber2.HasValue && changeType != '-')
            {
                throw new ArgumentException(Messages.ErrorDiffSecondLineNumber, nameof(lineNumber2));
            }
            if (changeType != '+' && changeType != '-' && changeType != ' ')
            {
                throw new ArgumentException(Messages.ErrorDiffInvalidChangeType, nameof(changeType));
            }

            LineNumber1 = lineNumber1;
            LineNumber2 = lineNumber2;
            ChangeType = changeType;
            Line = line;
        }
    }
}