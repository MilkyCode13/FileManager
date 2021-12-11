using System;
using System.Linq;

namespace FileManager
{
    /// <summary>
    /// Contains all command information.
    /// </summary>
    internal readonly struct ConsoleCommand
    {
        /// <summary>
        /// The main command.
        /// </summary>
        public string Command { get; init; }
        
        /// <summary>
        /// Options of the command.
        /// </summary>
        public string Options { get; init; }
        
        /// <summary>
        /// Parameters of the command.
        /// </summary>
        public string[] Parameters { get; init; }

        /// <summary>
        /// Check if there are no options.
        /// </summary>
        /// <returns>Whether there are no options.</returns>
        public bool CheckOptionsEmpty()
        {
            return CheckOptions(Array.Empty<char>(), out _);
        }
        
        /// <summary>
        /// Check if options are valid and present.
        /// </summary>
        /// <param name="validOptions">Valid options array.</param>
        /// <param name="optionsPresent">Whether corresponding option is present.</param>
        /// <returns>Whether options are valid.</returns>
        public bool CheckOptions(char[] validOptions, out bool[] optionsPresent)
        {
            string options = Options;
            optionsPresent = validOptions.Select(option => options.Contains(option)).ToArray();
            return options.Length <= 1 || options[1..].All(validOptions.Contains);
        }
    }
}