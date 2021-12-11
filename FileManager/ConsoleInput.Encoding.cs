using System.Linq;
using System.Text;

namespace FileManager
{
    /// <summary>
    /// Implements console user input.
    /// </summary>
    internal sealed partial class ConsoleInput
    {
        /// <summary>
        /// Gets auto completion list for encoding names.
        /// </summary>
        /// <param name="input">String to complete.</param>
        /// <returns>Completed string variants.</returns>
        private string[] GetAutoCompletionListEncoding(string input)
        {
            return Encoding.GetEncodings()
                .Select(encoding => encoding.Name).Where(s => s.StartsWith(input)).ToArray();
        }
    }
}