using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FileManager.Resources;

namespace FileManager
{
    /// <summary>
    /// Includes main methods of a program.
    /// </summary>
    internal static partial class Program
    {
        /// <summary>
        /// Help text for each string.
        /// </summary>
        private static readonly Dictionary<string, string> HelpStrings = new()
        {
            {"console", Help.HelpConsole},
            {"commands", Help.HelpCommands},
            {"help", Help.HelpCommandHelp},
            {"exit", Help.HelpCommandExit},
            {"drive", Help.HelpCommandDrive},
            {"cd", Help.HelpCommandChangeDirectory},
            {"pwd", Help.HelpCommandShowCurrentDirectory},
            {"dir", Help.HelpCommandShowDirectoryContents},
            {"find", Help.HelpCommandFind},
            {"show", Help.HelpCommandShowFile},
            {"copy", Help.HelpCommandCopy},
            {"move", Help.HelpCommandMove},
            {"delete", Help.HelpCommandDelete},
            {"create", Help.HelpCommandCreateFile},
            {"concat", Help.HelpCommandConcatenate},
            {"diff", Help.HelpCommandDiff}
        };

        /// <summary>
        /// Runs help command.
        /// </summary>
        /// <param name="cmd">Command.</param>
        /// <returns>Error status of command.</returns>
        private static ErrorCode RunCommandHelp(ConsoleCommand cmd)
        {
            if (cmd.Parameters.Length == 0)
            {
                Console.WriteLine(Help.HelpCommandHelp);
            }
            else if (HelpStrings.ContainsKey(cmd.Parameters[0]))
            {
                Console.WriteLine(HelpStrings[cmd.Parameters[0]]);
            }
            else
            {
                PrintError(Messages.ErrorInvalidHelpString);
                return ErrorCode.InvalidArguments;
            }
            return ErrorCode.None;
        }

        /// <summary>
        /// Runs drive command.
        /// </summary>
        /// <param name="cmd">Command.</param>
        /// <returns>Error status of command.</returns>
        private static ErrorCode RunCommandDrive(ConsoleCommand cmd)
        {
            if (!cmd.CheckOptionsEmpty())
            {
                PrintError(Messages.ErrorInvalidOptions);
                return ErrorCode.InvalidOptions;
            }

            try
            {
                switch (cmd.Parameters.Length)
                {
                    case 0:
                        Display.DisplayDrives();
                        return ErrorCode.None;
                    case 1:
                        var drive = new DriveInfo(cmd.Parameters[0]);
                        if (!drive.IsReady)
                        {
                            PrintError(Messages.ErrorDriveNotFound);
                            return ErrorCode.DriveNotFound;
                        }
                        return ChangeCurrentDirectory(cmd.Parameters[0]);
                    default:
                        PrintError(Messages.ErrorInvalidArgumentCount);
                        return ErrorCode.InvalidArguments;
                }
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        /// <summary>
        /// Runs cd command.
        /// </summary>
        /// <param name="cmd">Command.</param>
        /// <returns>Error status of command.</returns>
        private static ErrorCode RunCommandChangeDirectory(ConsoleCommand cmd)
        {
            if (!cmd.CheckOptionsEmpty())
            {
                PrintError(Messages.ErrorInvalidOptions);
                return ErrorCode.InvalidOptions;
            }

            switch (cmd.Parameters.Length)
            {
                case 1:
                    return ChangeCurrentDirectory(cmd.Parameters[0]);
                default:
                    PrintError(Messages.ErrorInvalidArgumentCount);
                    return ErrorCode.InvalidArguments;
            }
        }

        /// <summary>
        /// Runs pwd command.
        /// </summary>
        /// <param name="cmd">Command.</param>
        /// <returns>Error status of command.</returns>
        private static ErrorCode RunCommandShowCurrentDirectory(ConsoleCommand cmd)
        {
            if (!cmd.CheckOptionsEmpty())
            {
                PrintError(Messages.ErrorInvalidOptions);
                return ErrorCode.InvalidOptions;
            }

            switch (cmd.Parameters.Length)
            {
                case 0:
                    Console.WriteLine(CurrentDirectoryPath);
                    return ErrorCode.None;
                default:
                    PrintError(Messages.ErrorInvalidArgumentCount);
                    return ErrorCode.InvalidArguments;
            }
        }

        /// <summary>
        /// Runs dir command.
        /// </summary>
        /// <param name="cmd">Command.</param>
        /// <returns>Error status of command.</returns>
        private static ErrorCode RunCommandShowDirectoryListing(ConsoleCommand cmd)
        {
            if (!cmd.CheckOptionsEmpty())
            {
                PrintError(Messages.ErrorInvalidOptions);
                return ErrorCode.InvalidOptions;
            }
            
            try
            {
                switch (cmd.Parameters.Length)
                {
                    case 0:
                        Display.DisplayDirectory(s_currentDirectory);
                        return ErrorCode.None;
                    case 1:
                        var dir = new DirectoryInfo(cmd.Parameters[0]);
                        Display.DisplayDirectory(dir);
                        return ErrorCode.None;
                    default:
                        PrintError(Messages.ErrorInvalidArgumentCount);
                        return ErrorCode.InvalidArguments;
                }
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        /// <summary>
        /// Runs find command.
        /// </summary>
        /// <param name="cmd">Command.</param>
        /// <returns>Error status of command.</returns>
        private static ErrorCode RunCommandFind(ConsoleCommand cmd)
        {
            if (!cmd.CheckOptions(new []{'r'}, out bool[] optionsPresent))
            {
                PrintError(Messages.ErrorInvalidOptions);
                return ErrorCode.InvalidOptions;
            }
            bool recursive = optionsPresent[0];
            
            try
            {
                switch (cmd.Parameters.Length)
                {
                    case 1:
                        // Recursive search in current directory by file pattern.
                        if (recursive)
                        {
                            Display.DisplayFind(
                                s_currentDirectory, s_currentDirectory, cmd.Parameters[0], true);
                            return ErrorCode.None;
                        }
                        // Search by expression.
                        Display.DisplayFind(s_currentDirectory, cmd.Parameters[0]);
                        return ErrorCode.None;
                    case 2:
                        // Search in chosen directory by file pattern.
                        var dir = new DirectoryInfo(cmd.Parameters[0]);
                        Display.DisplayFind(s_currentDirectory, dir, cmd.Parameters[1], recursive);
                        return ErrorCode.None;
                    default:
                        PrintError(Messages.ErrorInvalidArgumentCount);
                        return ErrorCode.InvalidArguments;
                }
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        /// <summary>
        /// Runs show command.
        /// </summary>
        /// <param name="cmd">Command.</param>
        /// <returns>Error status of command.</returns>
        private static ErrorCode RunCommandShowFileContents(ConsoleCommand cmd)
        {
            if (!cmd.CheckOptionsEmpty())
            {
                PrintError(Messages.ErrorInvalidOptions);
                return ErrorCode.InvalidOptions;
            }
            
            try
            {
                switch (cmd.Parameters.Length)
                {
                    case 1:
                        // Using default encoding.
                        Display.DisplayFile(cmd.Parameters[0]);
                        return ErrorCode.None;
                    case 2:
                        // Using chosen encoding.
                        Display.DisplayFile(cmd.Parameters[1], cmd.Parameters[0]);
                        return ErrorCode.None;
                    default:
                        PrintError(Messages.ErrorInvalidArgumentCount);
                        return ErrorCode.InvalidArguments;
                }
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        /// <summary>
        /// Runs copy command.
        /// </summary>
        /// <param name="cmd">Command.</param>
        /// <returns>Error status of command.</returns>
        private static ErrorCode RunCommandCopy(ConsoleCommand cmd)
        {
            if (!cmd.CheckOptions(new []{'r', 'f', 'p'}, out bool[] optionsPresent))
            {
                PrintError(Messages.ErrorInvalidOptions);
                return ErrorCode.InvalidOptions;
            }
            bool recursive = optionsPresent[0];
            bool force = optionsPresent[1];
            var pattern = "*";
            string sourcePath;
            string destinationPath;

            switch (cmd.Parameters.Length)
            {
                case 3 when optionsPresent[2]:
                    pattern = cmd.Parameters[0];
                    sourcePath = cmd.Parameters[1];
                    destinationPath = cmd.Parameters[2];
                    break;
                case 2:
                    sourcePath = cmd.Parameters[0];
                    destinationPath = cmd.Parameters[1];
                    break;
                default:
                    PrintError(Messages.ErrorInvalidArgumentCount);
                    return ErrorCode.InvalidArguments;
            }
            
            try
            {
                FileOperations.Copy(sourcePath, destinationPath, recursive, force, pattern);
                Console.WriteLine(Messages.MessageOperationCompletedSuccessfully);
                return ErrorCode.None;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        /// <summary>
        /// Runs move command.
        /// </summary>
        /// <param name="cmd">Command.</param>
        /// <returns>Error status of command.</returns>
        private static ErrorCode RunCommandMove(ConsoleCommand cmd)
        {
            if (!cmd.CheckOptionsEmpty())
            {
                PrintError(Messages.ErrorInvalidOptions);
                return ErrorCode.InvalidOptions;
            }

            if (cmd.Parameters.Length != 2)
            {
                PrintError(Messages.ErrorInvalidArgumentCount);
                return ErrorCode.InvalidArguments;
            }
            string sourcePath = cmd.Parameters[0];
            string destinationPath = cmd.Parameters[1];

            try
            {
                FileOperations.Move(sourcePath, destinationPath);
                Console.WriteLine(Messages.MessageOperationCompletedSuccessfully);
                return ErrorCode.None;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        /// <summary>
        /// Runs delete command.
        /// </summary>
        /// <param name="cmd">Command.</param>
        /// <returns>Error status of command.</returns>
        private static ErrorCode RunCommandDelete(ConsoleCommand cmd)
        {
            if (!cmd.CheckOptionsEmpty())
            {
                PrintError(Messages.ErrorInvalidOptions);
                return ErrorCode.InvalidOptions;
            }
            if (cmd.Parameters.Length != 1)
            {
                PrintError(Messages.ErrorInvalidArgumentCount);
                return ErrorCode.InvalidArguments;
            }

            try
            {
                string path = cmd.Parameters[0];
                if (FileOperations.ConfirmDelete(path))
                {
                    FileInfo file = new FileInfo(path);
                    file.Delete();
                }
                return ErrorCode.None;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        /// <summary>
        /// Runs create command.
        /// </summary>
        /// <param name="cmd">Command.</param>
        /// <returns>Error status of command.</returns>
        private static ErrorCode RunCommandCreate(ConsoleCommand cmd)
        {
            if (!cmd.CheckOptionsEmpty())
            {
                PrintError(Messages.ErrorInvalidOptions);
                return ErrorCode.InvalidOptions;
            }

            if (cmd.Parameters.Length is < 1 or > 3)
            {
                PrintError(Messages.ErrorInvalidArgumentCount);
                return ErrorCode.InvalidArguments;
            }
            
            try
            {
                string path = cmd.Parameters[0];
                var encoding = Encoding.Default;
                string text = cmd.Parameters.Length > 1 ? cmd.Parameters[^1] : "";
                if (cmd.Parameters.Length == 3)
                {
                    try
                    {
                        encoding = Encoding.GetEncoding(cmd.Parameters[1]);
                    }
                    catch (ArgumentException)
                    {
                        throw new EncodingNotFoundException(cmd.Parameters[1]);
                    }
                }
                FileOperations.Create(path, text, encoding);
                return ErrorCode.None;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        /// <summary>
        /// Runs concat command.
        /// </summary>
        /// <param name="cmd">Command.</param>
        /// <returns>Error status of command.</returns>
        private static ErrorCode RunCommandConcatenate(ConsoleCommand cmd)
        {
            if (!cmd.CheckOptionsEmpty())
            {
                PrintError(Messages.ErrorInvalidOptions);
                return ErrorCode.InvalidOptions;
            }
            if (cmd.Parameters.Length < 2)
            {
                PrintError(Messages.ErrorInvalidArgumentCount);
                return ErrorCode.InvalidArguments;
            }

            foreach (string path in cmd.Parameters)
            {
                try
                {
                    Display.DisplayFile(path);
                }
                catch (Exception e)
                {
                    HandleError(e);
                }
            }

            return ErrorCode.None;
        }

        /// <summary>
        /// Runs diff command.
        /// </summary>
        /// <param name="cmd">Command.</param>
        /// <returns>Error status of command.</returns>
        private static ErrorCode RunCommandDiff(ConsoleCommand cmd)
        {
            if (!cmd.CheckOptionsEmpty())
            {
                PrintError(Messages.ErrorInvalidOptions);
                return ErrorCode.InvalidOptions;
            }
            if (cmd.Parameters.Length != 2)
            {
                PrintError(Messages.ErrorInvalidArgumentCount);
                return ErrorCode.InvalidArguments;
            }

            try
            {
                Console.WriteLine(Messages.MessagePleaseWait);
                var diff = new Diff(cmd.Parameters[0], cmd.Parameters[1]);
                Display.DisplayDiff(diff);
                return ErrorCode.None;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }
    }
}