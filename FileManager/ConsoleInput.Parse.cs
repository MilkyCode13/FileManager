using System;
using System.Collections.Generic;
using System.Linq;

namespace FileManager
{
    /// <summary>
    /// Implements console user input.
    /// </summary>
    internal sealed partial class ConsoleInput
    {
        /// <summary>
        /// Adds quotes as needed.
        /// </summary>
        /// <param name="argument">String to quote.</param>
        /// <returns>Quoted string.</returns>
        private static string AddQuotes(string argument)
        {
            // If argument contains spaces, need to add quotes.
            if (argument.Any(char.IsWhiteSpace))
            {
                // If argument contains `"`, need to escape them using `'` or use `'` as quotes.
                if (argument.Contains('"'))
                {
                    if (argument.Contains('\''))
                    {
                        return $"\"{argument.Replace("\"", "\"'\"'\"")}\"";
                    }

                    return $"'{argument}'";
                }

                return $"\"{argument}\"";
            }

            return argument;
        }
        
        /// <summary>
        /// Parses user input into fragments.
        /// </summary>
        /// <param name="leaveStartingQuote">Whether to leave starting quotes.</param>
        /// <param name="leaveTrailingEmptyArg">Whether to leave trailing empty argument.</param>
        /// <returns></returns>
        private IEnumerable<string> GetParsedInput(bool leaveStartingQuote = false, bool leaveTrailingEmptyArg = false)
        {
            var beginIndex = 0;
            var buff = string.Empty;
            var isQuoted = false;
            char quoteChar = default;

            for (int i = 0; i < _inputStringBuilder.Length; i++)
            {
                if (_inputStringBuilder[i] is '\'' or '"')
                {
                    if (!isQuoted)
                    {
                        buff += _inputStringBuilder.ToString(beginIndex, i - beginIndex);
                        beginIndex = i + 1;
                        isQuoted = true;
                        quoteChar = _inputStringBuilder[i];
                    }
                    else if (_inputStringBuilder[i] == quoteChar)
                    {
                        buff += _inputStringBuilder.ToString(beginIndex, i - beginIndex);
                        beginIndex = i + 1;
                        isQuoted = false;
                    }
                }
                else if (!isQuoted && char.IsWhiteSpace(_inputStringBuilder[i]))
                {
                    buff += _inputStringBuilder.ToString(beginIndex, i - beginIndex);
                    beginIndex = i + 1;
                    if (!string.IsNullOrWhiteSpace(buff))
                    {
                        yield return buff;
                    }
                    buff = string.Empty;
                }
            }

            if (leaveStartingQuote && isQuoted)
            {
                buff = quoteChar + buff;
            }
            buff += _inputStringBuilder.ToString(beginIndex, _inputStringBuilder.Length - beginIndex);
            if (leaveTrailingEmptyArg || !string.IsNullOrWhiteSpace(buff))
            {
                yield return buff;
            }
        }
        
        /// <summary>
        /// Gets auto completion string.
        /// </summary>
        /// <param name="includeAdditionalQuotes">Whether to include additional quotes.</param>
        /// <returns>Auto completion string.</returns>
        private string GetAutoCompletionString(bool includeAdditionalQuotes = true)
        {
            // Get current argument list.
            string[] cmd = GetParsedInput(!includeAdditionalQuotes).ToArray();
            if (cmd.Length == 0)
            {
                return string.Empty;
            }

            string[] autoCompletionList = GetAutoCompletionList(cmd).ToArray();

            // If list is empty, return empty string.
            if (!autoCompletionList.Any())
            {
                return string.Empty;
            }

            // Get longest common prefix (best auto completion) for all string in a list.
            string commonPrefix = new string(autoCompletionList.First()[..autoCompletionList.Min(s => s.Length)]
                .TakeWhile((c, i) => autoCompletionList.All(s => s[i] == c)).ToArray());

            // Unconditionally add quotes in previous arguments as needed.
            cmd = cmd.Select(AddQuotes).ToArray();
            
            // If there are any whitespaces in an argument and we are told so, add quotes.
            if (includeAdditionalQuotes)
            {
                commonPrefix = AddQuotes(commonPrefix);
            }

            // Get full auto completion string by replacing last unfinished argument with our enhanced version.
            cmd[^1] = commonPrefix;
            return string.Join(' ', cmd);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private IEnumerable<string> GetAutoCompletionList(string[] cmd)
        {
            // Create list of auto completion variants.
            IEnumerable<string> autoCompletionList = Array.Empty<string>();
            // If there are only one argument, treat it as a command name.
            if (cmd.Length == 1)
            {
                return GetAutoCompletionListCommand(cmd[^1]);
            }

            // Get command name, resolve its alias, and check that we can get its argument type.
            string currCmd = cmd[0];
            if (CommandAliases.ContainsKey(currCmd))
            {
                currCmd = CommandAliases[currCmd];
            }
            if (!CommandParams.ContainsKey(currCmd) || cmd.Length - 1 > CommandParams[currCmd].Length)
            {
                return autoCompletionList;
            }

            // Get auto completion variants for each matching argument type.
            foreach (ParameterType parameterType in Enum.GetValues(typeof(ParameterType)))
            {
                if (CommandParams[currCmd][cmd.Length - 2].HasFlag(parameterType))
                {
                    autoCompletionList =
                        autoCompletionList.Concat(GetAutoCompletionListParameter(cmd[^1], parameterType));
                }
            }
            
            return autoCompletionList;
        }

        /// <summary>
        /// Gets auto completion list based on parameter type.
        /// </summary>
        /// <param name="input">String to complete.</param>
        /// <param name="parameterType">Type of a parameter.</param>
        /// <returns>Completed string variants.</returns>
        private string[] GetAutoCompletionListParameter(string input, ParameterType parameterType)
        {
            return parameterType switch
            {
                ParameterType.Encoding => GetAutoCompletionListEncoding(input),
                ParameterType.DirectoryPath => GetAutoCompletionListDirectory(input),
                ParameterType.FilePath => GetAutoCompletionListFile(input),
                ParameterType.Drive => GetAutoCompletionListDrive(input),
                ParameterType.HelpString => GetAutoCompletionListHelp(input),
                _ => Array.Empty<string>()
            };
        }
    }
}