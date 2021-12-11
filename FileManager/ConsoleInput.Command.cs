using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FileManager
{
    /// <summary>
    /// Implements console user input.
    /// </summary>
    internal sealed partial class ConsoleInput
    {
        /// <summary>
        /// Regex to detect command options.
        /// </summary>
        private static readonly Regex OptionRegex = new(@"^-[A-Za-z]+$");
        
        /// <summary>
        /// Dictionary of command parameters.
        /// </summary>
        private static readonly Dictionary<string, ParameterType[]> CommandParams = new()
        {
            {"help", new [] {ParameterType.HelpString}},
            {"exit", Array.Empty<ParameterType>()},
            {"drive", new [] {ParameterType.Drive}},
            {"cd", new [] {ParameterType.DirectoryPath}},
            {"pwd", Array.Empty<ParameterType>()},
            {"dir", new [] {ParameterType.DirectoryPath}},
            {"find", new [] {ParameterType.DirectoryPath, ParameterType.DirectoryPath}},
            {"show", new [] {ParameterType.FilePath | ParameterType.Encoding, ParameterType.FilePath}},
            {"copy", new [] {ParameterType.FilePath, ParameterType.FilePath, ParameterType.FilePath}},
            {"move", new [] {ParameterType.FilePath, ParameterType.FilePath, ParameterType.FilePath}},
            {"delete", new [] {ParameterType.FilePath}},
            {"create", new [] {ParameterType.FilePath, ParameterType.Encoding}},
            {"concat", Enumerable.Repeat(ParameterType.FilePath, 256).ToArray()},
            {"diff", new [] {ParameterType.FilePath, ParameterType.FilePath}}
        };
        
        /// <summary>
        /// Dictionary of command aliases.
        /// </summary>
        private static readonly Dictionary<string, string> CommandAliases = new()
        {
            {"?", "help"},
            {"man", "help"},
            {"ls", "dir"},
            {"cat", "show"},
            {"less", "show"},
            {"more", "show"},
            {"cp", "copy"},
            {"mv", "move"},
            {"del", "delete"},
            {"rm", "delete"},
            {"touch", "create"}
        };

        /// <summary>
        /// Gets commands from user input.
        /// </summary>
        /// <returns>Console command object.</returns>
        public ConsoleCommand GetCommands()
        {
            string[] cmd = GetCommandsRaw();
            string command = "";
            string options = "";
            string[] parameters = Array.Empty<string>();

            if (cmd.Length > 0)
            {
                command = cmd[0];
            }
            if (cmd.Length > 1)
            {
                if (OptionRegex.IsMatch(cmd[1]))
                {
                    options = cmd[1].ToLower();
                    if (cmd.Length > 2)
                    {
                        parameters = cmd[2..];
                    }
                }
                else
                {
                    parameters = cmd[1..];
                }
            }

            return new ConsoleCommand {Command = command, Options = options, Parameters = parameters};
        }

        /// <summary>
        /// Gets commands from user input.
        /// </summary>
        /// <returns>Array of strings in command.</returns>
        private string[] GetCommandsRaw()
        {
            if (!string.IsNullOrWhiteSpace(Input))
            {
                History.Add(Input);
            }

            string[] parsedInput = GetParsedInput().ToArray();
            if (parsedInput.Length == 0)
            {
                return parsedInput;
            }
            return parsedInput.Select((s, i) =>
                (i == 0 || parsedInput[0] == "help") && CommandAliases.ContainsKey(s)
                    ? CommandAliases[s] : s).ToArray();
        }

        /// <summary>
        /// Gets auto completion list for command names.
        /// </summary>
        /// <param name="input">String to complete.</param>
        /// <returns>Completed string variants.</returns>
        private string[] GetAutoCompletionListCommand(string input)
        {
            return CommandParams.Keys.Concat(CommandAliases.Keys)
                .Where(s => s.StartsWith(input))
                .Select(s => s).ToArray();
        }
    }
}