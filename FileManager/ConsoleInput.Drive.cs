using System.Collections.Generic;
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
        /// Gets auto completion list for drive names.
        /// </summary>
        /// <param name="input">String to complete.</param>
        /// <returns>Completed string variants.</returns>
        private string[] GetAutoCompletionListDrive(string input)
        {
            char startQuote = default;
            if (input.Length > 0 && input[0] is '"' or '\'')
            {
                startQuote = input[0];
                input = input[1..];
            }
            
            IEnumerable<string> drives = DriveInfo.GetDrives()
                .Select(drive => drive.Name).Where(str => str.StartsWith(input));

            if (startQuote is '"' or '\'')
            {
                return drives.Select(s => startQuote + s).ToArray();
            }

            return drives.ToArray();
        }
    }
}