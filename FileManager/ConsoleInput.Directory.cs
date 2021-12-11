using System;
using System.IO;
using System.Linq;

namespace FileManager
{
    /// <summary>
    /// Implements console user input.
    /// </summary>
    internal sealed partial class ConsoleInput
    {
        /// <summary>
        /// Gets auto completion list for directory paths.
        /// </summary>
        /// <param name="input">String to complete.</param>
        /// <returns>Completed string variants.</returns>
        private string[] GetAutoCompletionListDirectory(string input)
        {
            char startQuote = default;
            if (input.Length > 0 && input[0] is '"' or '\'')
            {
                startQuote = input[0];
                input = input[1..];
            }
            string[] pathElements =  input.Split(Path.DirectorySeparatorChar);
            if (pathElements.Length == 0)
            {
                return Array.Empty<string>();
            }

            string parentPath = string.Empty;
            DirectoryInfo parentDir = CurrentDir;
            if (pathElements.Length > 1)
            {
                parentPath = string.Join(Path.DirectorySeparatorChar, pathElements[..^1]) + Path.DirectorySeparatorChar;
                parentDir = new DirectoryInfo(parentPath);
            }

            if (startQuote is '"' or '\'')
            {
                parentPath = startQuote + parentPath;
                input = startQuote + input;
            }

            try
            {
                return parentDir.EnumerateDirectories()
                    .Select(dir => parentPath + dir.Name + Path.DirectorySeparatorChar)
                    .Where(str => str.StartsWith(input)).ToArray();
            }
            catch (Exception)
            {
                return Array.Empty<string>();
            }
        }
    }
}