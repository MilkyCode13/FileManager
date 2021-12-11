using System;
using System.IO;
using FileManager.Resources;

namespace FileManager
{
    /// <summary>
    /// Includes main methods of a program.
    /// </summary>
    internal static partial class Program
    {
        /// <summary>
        /// The current directory represented as <see cref="DirectoryInfo"/> object.
        /// </summary>
        private static DirectoryInfo s_currentDirectory = new(Directory.GetCurrentDirectory());
        
        /// <summary>
        /// Current error status.
        /// </summary>
        private static ErrorCode s_status;
        
        /// <summary>
        /// Current directory path.
        /// </summary>
        private static string CurrentDirectoryPath
        {
            get => s_currentDirectory.FullName;
            set
            {
                var newDirectory = new DirectoryInfo(value);
                Directory.SetCurrentDirectory(newDirectory.FullName);
                s_currentDirectory = newDirectory;
            }
        }
        
        /// <summary>
        /// Current directory name.
        /// </summary>
        private static string CurrentDirectoryName => s_currentDirectory.Name;
        
        /// <summary>
        /// Prints command prompt.
        /// </summary>
        public static void PrintPrompt()
        {
            Console.Write(Messages.ConsolePrompt, CurrentDirectoryName);
            Console.ForegroundColor = (s_status == 0 ? ConsoleColor.Green : ConsoleColor.Red);
            Console.Write(@"$ ");
            Console.ResetColor();
        }
        
        /// <summary>
        /// Handles exception and displays error.
        /// </summary>
        /// <param name="exception">Exception to handle.</param>
        /// <param name="msg">Additional message to display.</param>
        /// <returns>Code of an error.</returns>
        public static ErrorCode HandleError(Exception exception, string msg = "")
        {
            switch (exception)
            {
                case DirectoryNotFoundException:
                    PrintError(Messages.ErrorDirectoryNotFound);
                    return ErrorCode.DirectoryNotFound;
                case FileNotFoundException fileNotFoundException:
                    PrintError(string.Format(Messages.ErrorFileNotFound, fileNotFoundException.FileName));
                    return ErrorCode.FileNotFound;
                case UnauthorizedAccessException:
                    PrintError(string.Format(Messages.ErrorAccessDenied, msg));
                    return ErrorCode.UnauthorizedAccess;
                case EncodingNotFoundException encodingNotFoundException:
                    PrintError(
                        string.Format(Messages.ErrorEncodingNotFound, encodingNotFoundException.EncodingString));
                    return ErrorCode.EncodingNotFound;
                case ArgumentException:
                    PrintError(Messages.ErrorInvalidArgumentCount);
                    return ErrorCode.InvalidArguments;
                case IOException:
                    PrintError(Messages.ErrorInputOutput);
                    return ErrorCode.InputOutputError;
                case CustomException customException:
                    PrintError(customException.Message);
                    return customException.ErrorCode;
                default:
                    PrintError(string.Format(Messages.ErrorGeneral, exception.Message));
                    return ErrorCode.Unknown;
            }
        }

        /// <summary>
        /// Main entrypoint of a program.
        /// </summary>
        private static void Main()
        {
            // Clear console and welcome user.
            Console.Clear();
            PrintSplash();

            // While not exiting, prompt user for command input and execute it.
            while (s_status != ErrorCode.Exit)
            {
                try
                {
                    PrintPrompt();
                    ConsoleCommand cmd = new ConsoleInput(s_currentDirectory).GetCommands();
                    // If command is empty, skip.
                    if (!string.IsNullOrWhiteSpace(cmd.Command))
                    {
                        s_status = ProcessCommand(cmd);
                    }
                }
                catch (Exception e)
                {
                    s_status = HandleError(e);
                }
            }
        }

        /// <summary>
        /// Prints splash text.
        /// </summary>
        private static void PrintSplash()
        {
            Console.WriteLine(Messages.MessageSplash);
        }

        /// <summary>
        /// Prints error message.
        /// </summary>
        /// <param name="msg">Message to display.</param>
        private static void PrintError(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ResetColor();
        }
        
        /// <summary>
        /// Changes current directory.
        /// </summary>
        /// <param name="path">Directory to make current.</param>
        /// <returns>Status code of an operation.</returns>
        private static ErrorCode ChangeCurrentDirectory(string path)
        {
            try
            {
                CurrentDirectoryPath = path;
                return ErrorCode.None;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        /// <summary>
        /// Processes command.
        /// </summary>
        /// <param name="cmd">Command.</param>
        /// <returns>Status code of a command.</returns>
        private static ErrorCode ProcessCommand(ConsoleCommand cmd)
        {
            switch (cmd.Command)
            {
                case "help":
                    return RunCommandHelp(cmd);
                case "exit":
                    return ErrorCode.Exit;
                case "drive":
                    return RunCommandDrive(cmd);
                case "cd":
                    return RunCommandChangeDirectory(cmd);
                case "pwd":
                    return RunCommandShowCurrentDirectory(cmd);
                case "dir":
                    return RunCommandShowDirectoryListing(cmd);
                case "find":
                    return RunCommandFind(cmd);
                case "show":
                    return RunCommandShowFileContents(cmd);
                case "copy":
                    return RunCommandCopy(cmd);
                case "move":
                    return RunCommandMove(cmd);
                case "delete":
                    return RunCommandDelete(cmd);
                case "create":
                    return RunCommandCreate(cmd);
                case "concat":
                    return RunCommandConcatenate(cmd);
                case "diff":
                    return RunCommandDiff(cmd);
                default:
                    PrintError(string.Format(Messages.ErrorCommandNotFound, cmd.Command));
                    return ErrorCode.CommandNotFound;
            }
        }
    }
}