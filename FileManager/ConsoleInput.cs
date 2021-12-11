using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FileManager.Resources;

namespace FileManager
{
    /// <summary>
    /// Implements console user input.
    /// </summary>
    internal sealed partial class ConsoleInput
    {
        /// <summary>
        /// Step of input slide.
        /// </summary>
        private const int SlideStep = 25;

        /// <summary>
        /// Right limit of cursor position relative to console window.
        /// </summary>
        private const int RightLimit = 30;
        
        /// <summary>
        /// History of commands.
        /// </summary>
        private static readonly List<string> History = new();

        /// <summary>
        /// Cursor initial absolute position.
        /// </summary>
        private readonly int _cursorLeft = Console.CursorLeft;

        /// <summary>
        /// Current text slide size.
        /// </summary>
        private int _textSlide;

        /// <summary>
        /// Contains full string entered by user.
        /// </summary>
        private StringBuilder _inputStringBuilder = new();
        
        /// <summary>
        /// Current cursor position relative to text.
        /// </summary>
        private int _position;

        /// <summary>
        /// Current history position relative to the end.
        /// </summary>
        private int _historyPosition;

        /// <summary>
        /// Whether last key pressed is tab.
        /// </summary>
        private bool _isLastTab;

        /// <summary>
        /// Initializes a new instance of <see cref="ConsoleInput"/> class.
        /// </summary>
        /// <param name="currentDir">Current directory.</param>
        public ConsoleInput(DirectoryInfo currentDir)
        {
            // Set current directory.
            CurrentDir = currentDir;
            
            // Read user input.
            try
            {
                ReadInput();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        
        /// <summary>
        /// Gets current user input.
        /// </summary>
        private string Input => _inputStringBuilder.ToString();
        
        /// <summary>
        /// Gets current directory.
        /// </summary>
        private DirectoryInfo CurrentDir { get; }

        /// <summary>
        /// Reads user input.
        /// </summary>
        private void ReadInput()
        {
            do
            {
                ConsoleKeyInfo keyInput = Console.ReadKey(true);
                if (ProcessKey(keyInput))
                {
                    Refresh(false);
                    Console.WriteLine();
                    return;
                }
                Refresh();
            } while (true);
        }

        /// <summary>
        /// Refreshes the screen.
        /// </summary>
        /// <param name="showAutoCompletion">Whether to show auto completion.</param>
        private void Refresh(bool showAutoCompletion = true)
        {
            int maxWidth = Console.WindowWidth - _cursorLeft;
            string userInput = Input;
            _textSlide = Math.Max(_position + RightLimit - maxWidth, 0);
            _textSlide -= _textSlide % SlideStep;
            
            // Display auto completion string and user input.
            Console.CursorLeft = _cursorLeft;
            var autoCompletionLength = 0;
            if (showAutoCompletion)
            {
                string autoCompletionString = GetAutoCompletionString(false);
                autoCompletionLength = autoCompletionString.Length;
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write((autoCompletionString.Length > _textSlide
                    ? autoCompletionString[_textSlide..] : "").PadRight(maxWidth)[..maxWidth]);
                Console.ResetColor();
                Console.CursorLeft = _cursorLeft;
                Console.Write(userInput[_textSlide..Math.Min(userInput.Length, _textSlide + maxWidth)]);
            }
            else
            {
                Console.Write(userInput[_textSlide..].PadRight(maxWidth)[..maxWidth]);
            }

            // Display ellipses as needed.
            Console.ForegroundColor = ConsoleColor.DarkGray;
            if (_textSlide > 0)
            {
                Console.CursorLeft = _cursorLeft;
                Console.Write(@"...");
            }
            if (Math.Max(userInput.Length, autoCompletionLength) - _textSlide > maxWidth)
            {
                Console.CursorLeft = Console.WindowWidth - 3;
                Console.Write(@"...");
            }
            Console.ResetColor();
            Console.CursorLeft = _cursorLeft + _position - _textSlide;
        }

        /// <summary>
        /// Shows auto completion variants.
        /// </summary>
        private void ShowAutoCompletionVariants()
        {
            string[] cmd = GetParsedInput(false, true).ToArray();
            if (cmd.Length > 0)
            {
                string[] helpList = GetAutoCompletionList(cmd).ToArray();
                if (helpList.Length > 0)
                {
                    // If there are many variants, ask whether user wants to show them all.
                    if (helpList.Length > 20)
                    {
                        Console.Write(Messages.MessageManyAutoCompletionVariants, helpList.Length);
                        ConsoleKeyInfo keyInfo = Console.ReadKey();
                        if (keyInfo.Key != ConsoleKey.Y)
                        {
                            Console.WriteLine();
                            Program.PrintPrompt();
                            return;
                        }
                    }
                    
                    Console.WriteLine();
                    foreach (var variant in helpList)
                    {
                        Console.WriteLine(variant);
                    }
                    Program.PrintPrompt();
                }
            }
        }

        /// <summary>
        /// Processes pressed key.
        /// </summary>
        /// <param name="keyInfo">Key info of pressed key.</param>
        /// <returns>Whether user requested command execution.</returns>
        private bool ProcessKey(ConsoleKeyInfo keyInfo)
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.Tab:
                    ProcessKeyTab();
                    return false;
                case ConsoleKey.LeftArrow:
                    ProcessKeyLeft();
                    break;
                case ConsoleKey.RightArrow:
                    ProcessKeyRight();
                    break;
                case ConsoleKey.UpArrow:
                    ProcessKeyUp();
                    break;
                case ConsoleKey.DownArrow:
                    ProcessKeyDown();
                    break;
                case ConsoleKey.Home:
                    ProcessKeyHome();
                    break;
                case ConsoleKey.End:
                    ProcessKeyEnd();
                    break;
                case ConsoleKey.Backspace:
                    ProcessKeyBackspace();
                    break;
                case ConsoleKey.Delete:
                    ProcessKeyDelete();
                    break;
                case ConsoleKey.Enter:
                    return true;
                default:
                    ProcessKeyText(keyInfo);
                    break;
            }

            _isLastTab = false;
            return false;
        }

        /// <summary>
        /// Processes tab key.
        /// </summary>
        private void ProcessKeyTab()
        {
            if (_isLastTab)
            {
                _isLastTab = false;
                ShowAutoCompletionVariants();
                return;
            }

            string userInput = Input;
            string autoCompletionString = GetAutoCompletionString();
            if (autoCompletionString.Length > userInput.Length)
            {
                _inputStringBuilder = new StringBuilder(autoCompletionString);
                _position = _inputStringBuilder.Length;
                return;
            }
            _isLastTab = true;
        }

        /// <summary>
        /// Processes left arrow key.
        /// </summary>
        private void ProcessKeyLeft()
        {
            if (_position > 0)
            {
                _position--;
            }
        }

        /// <summary>
        /// Processes right arrow key.
        /// </summary>
        private void ProcessKeyRight()
        {
            if (_position < _inputStringBuilder.Length)
            {
                _position++;
            }
        }

        /// <summary>
        /// Processes up arrow key.
        /// </summary>
        private void ProcessKeyUp()
        {
            if (_historyPosition < History.Count)
            {
                _historyPosition++;
                _inputStringBuilder = new StringBuilder(History[^_historyPosition]);
                _position = _inputStringBuilder.Length;
            }
        }

        /// <summary>
        /// Processes down arrow key.
        /// </summary>
        private void ProcessKeyDown()
        {
            if (_historyPosition > 0)
            {
                _historyPosition--;
                _inputStringBuilder = _historyPosition switch
                {
                    0 => new StringBuilder(),
                    _ => new StringBuilder(History[^_historyPosition])
                };
                _position = _inputStringBuilder.Length;
            }
        }

        /// <summary>
        /// Processes home key.
        /// </summary>
        private void ProcessKeyHome()
        {
            _position = 0;
        }

        /// <summary>
        /// Processes end key.
        /// </summary>
        private void ProcessKeyEnd()
        {
            _position = _inputStringBuilder.Length;
        }

        /// <summary>
        /// Processes backspace key.
        /// </summary>
        private void ProcessKeyBackspace()
        {
            if (_position > 0)
            {
                _inputStringBuilder.Remove(_position - 1, 1);
                _position--;
            }
        }

        /// <summary>
        /// Processes delete key.
        /// </summary>
        private void ProcessKeyDelete()
        {
            if (_position < _inputStringBuilder.Length)
            {
                _inputStringBuilder.Remove(_position, 1);
            }
        }

        /// <summary>
        /// Processes non-action key.
        /// </summary>
        private void ProcessKeyText(ConsoleKeyInfo keyInfo)
        {
            if (!char.IsControl(keyInfo.KeyChar))
            {
                _inputStringBuilder.Insert(_position, keyInfo.KeyChar);
                _position++;
            }
        }
    }
}