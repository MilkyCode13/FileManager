using System.Linq;

namespace FileManager
{
    /// <summary>
    /// Implements console user input.
    /// </summary>
    internal sealed partial class ConsoleInput
    {
        /// <summary>
        /// Additional help strings.
        /// </summary>
        private static readonly string[] HelpStringsAdditional = {"console", "commands"};
        
        /// <summary>
        /// Gets auto completion list for help strings.
        /// </summary>
        /// <param name="input">String to complete.</param>
        /// <returns>Completed string variants.</returns>
        private static string[] GetAutoCompletionListHelp(string input)
        {
            return CommandParams.Keys.Concat(CommandAliases.Keys).Concat(HelpStringsAdditional)
                .Where(s => s.StartsWith(input))
                .Select(s => s).ToArray();
        }
    }
}