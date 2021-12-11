using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FileManager.Resources;

namespace FileManager
{
    public static class FileOperations
    {
        private const long BigFileThreshold = 25 * 1024 * 1024;
        
        /// <summary>
        /// Request user confirmation of recursive operation.
        /// </summary>
        /// <returns>Whether user confirmed the operation.</returns>
        public static bool ConfirmRecursive()
        {
            Console.Write(Messages.MessageRecursiveOperation);
            ConsoleKeyInfo keyInfo = Console.ReadKey();
            Console.WriteLine();
            return keyInfo.Key == ConsoleKey.Y;
        }
        
        /// <summary>
        /// Request user confirmation to overwrite file.
        /// </summary>
        /// <param name="destinationPath">File to overwrite.</param>
        /// <returns>Whether user confirmed the operation.</returns>
        public static bool ConfirmOverwrite(string destinationPath)
        {
            Console.Write(Messages.MessageOverwriteFile, destinationPath);
            ConsoleKeyInfo keyInfo = Console.ReadKey();
            Console.WriteLine();
            return keyInfo.Key == ConsoleKey.Y;
        }
        
        /// <summary>
        /// Request user confirmation to delete file.
        /// </summary>
        /// <param name="destinationPath">File to delete.</param>
        /// <returns>Whether user confirmed the operation.</returns>
        public static bool ConfirmDelete(string destinationPath)
        {
            Console.Write(Messages.MessageDeleteFile, destinationPath);
            ConsoleKeyInfo keyInfo = Console.ReadKey();
            Console.WriteLine();
            return keyInfo.Key == ConsoleKey.Y;
        }
        
        /// <summary>
        /// Create a text file.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <param name="text">Text to put in the file.</param>
        /// <param name="encoding">Encoding of the file.</param>
        public static void Create(string path, string text, Encoding encoding)
        {
            if (!File.Exists(path) || ConfirmOverwrite(path))
            {
                File.WriteAllText(path, text, encoding);
            }
        }
        
        /// <summary>
        /// Copy a file or directory.
        /// </summary>
        /// <param name="sourcePath">Source file or directory.</param>
        /// <param name="destinationPath">Destination path.</param>
        /// <param name="recursive">Whether to copy a directory recursively.</param>
        /// <param name="force">Whether to overwrite files without confirmation.</param>
        /// <param name="pattern">Pattern to copy files by.</param>
        /// <exception cref="CustomException">Trying to copy a directory without recursive option.</exception>
        public static void Copy(
            string sourcePath, string destinationPath, bool recursive = false, bool force = false, string pattern = "*")
        {
            if (recursive && !ConfirmRecursive())
            {
                return;
            }

            FileAttributes attributes = File.GetAttributes(sourcePath);
            // If source is a directory, copy the directory if recursive option is present.
            if (attributes.HasFlag(FileAttributes.Directory))
            {
                if (!recursive)
                {
                    throw new CustomException(Messages.ErrorNeedRecursive, ErrorCode.InvalidArguments);
                }
                
                CopyDirectory(sourcePath, destinationPath, force, 
                    !sourcePath.EndsWith(Path.DirectorySeparatorChar)
                    && destinationPath.EndsWith(Path.DirectorySeparatorChar), pattern);
                return;
            }
            
            // If source is an individual file, copy the file.
            CopyFile(sourcePath, destinationPath, force);
        }

        public static void Move(string sourcePath, string destinationPath)
        {
            FileInfo source = new FileInfo(sourcePath);
            if (Directory.Exists(destinationPath) || destinationPath.EndsWith(Path.DirectorySeparatorChar))
            {
                Directory.CreateDirectory(destinationPath);
                destinationPath = Path.Combine(destinationPath, source.Name);
            }
            if (File.Exists(destinationPath))
            {
                if (ConfirmOverwrite(destinationPath))
                {
                    if (source.Length > BigFileThreshold)
                    {
                        Console.WriteLine(Messages.MessagePleaseWait);
                    }
                    source.MoveTo(destinationPath, true);
                }
                return;
            }
            if (source.Length > BigFileThreshold)
            {
                Console.WriteLine(Messages.MessagePleaseWait);
            }
            source.MoveTo(destinationPath);
        }

        /// <summary>
        /// Reads file lines.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <param name="encoding">Encoding of the file.</param>
        /// <param name="lineLimit">Limit of line count, -1 means no limit.</param>
        /// <param name="sizeLimit">Limit of file size, -1 means no limit.</param>
        /// <returns>Enumerable of all lines of the file.</returns>
        public static IEnumerable<string> ReadFileLines(string path, Encoding encoding,
            int lineLimit = -1, long sizeLimit = -1)
        {
            var fileInfo = new FileInfo(path);
            if (sizeLimit != -1 && fileInfo.Length > sizeLimit)
            {
                throw new CustomException(Messages.ErrorFileTooBig, ErrorCode.InputOutputError);
            }
            using StreamReader reader = new StreamReader(path, encoding, false);
            var i = 0;
            while (true)
            {
                string line = reader.ReadLine();
                if (line == null)
                {
                    yield break;
                }

                yield return line;
                
                i++;
                if (i == lineLimit)
                {
                    throw new CustomException(Messages.ErrorTooManyLines, ErrorCode.InputOutputError);
                }
            }
        }

        /// <summary>
        /// Enumerates files and directories by expression.
        /// </summary>
        /// <param name="expression">Expression to enumerate by.</param>
        /// <returns>Enumerable of files.</returns>
        public static IEnumerable<FileSystemInfo> EnumerateByExpression(string expression)
        {
            // If the expression does not contain any wildcard characters, treat it as a regular path.
            if (!expression.Contains('*') && !expression.Contains('?'))
            {
                FileAttributes attributes = File.GetAttributes(expression);
                if (attributes.HasFlag(FileAttributes.Directory))
                {
                    yield return new DirectoryInfo(expression);
                }
                else
                {
                    yield return new FileInfo(expression);
                }
                yield break;
            }
            
            // Enumerate for each possible parent directory.
            string parentDir = Path.GetDirectoryName(expression);
            string name = Path.GetFileName(expression);
            IEnumerable<DirectoryInfo> dirs = EnumerateByExpression(parentDir).OfType<DirectoryInfo>();
            foreach (var dir in dirs)
            {
                IEnumerable<FileSystemInfo> items;
                try
                {
                    items = dir.EnumerateFileSystemInfos(name);
                }
                catch (Exception e)
                {
                    Program.HandleError(e, dir.FullName);
                    items = Array.Empty<FileSystemInfo>();
                }
                foreach (var item in items)
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Find all files matching the pattern recursively.
        /// </summary>
        /// <param name="dir">Base directory of the search.</param>
        /// <param name="pattern">Search pattern.</param>
        /// <returns></returns>
        public static IEnumerable<FileSystemInfo> FindRecursive(DirectoryInfo dir, string pattern)
        {
            Display.EraseLine(Console.WindowWidth - 1);
            Console.Write(Messages.MessageDirectoryEnumeration,
                Display.TruncatePath(dir.FullName, Console.WindowWidth - 18));
            IEnumerable<FileSystemInfo> items;
            try
            {
                items = dir.EnumerateFileSystemInfos(pattern).ToArray();
            }
            catch (Exception e)
            {
                Display.EraseLine(Console.WindowWidth - 1);
                Program.HandleError(e, dir.FullName);
                yield break;
            }
            foreach (FileSystemInfo file in items)
            {
                yield return file;
            }

            foreach (var subDir in dir.EnumerateDirectories("*",
                new EnumerationOptions {AttributesToSkip = FileAttributes.ReparsePoint}))
            {
                foreach (var item in FindRecursive(subDir, pattern))
                {
                    yield return item;
                }
            }
            Display.EraseLine(Console.WindowWidth - 1);
        }

        /// <summary>
        /// Copy a directory.
        /// </summary>
        /// <param name="sourcePath">Source directory.</param>
        /// <param name="destinationPath">Destination directory.</param>
        /// <param name="force">Whether to overwrite files without confirmation.</param>
        /// <param name="createSubDirectory">Whether to create an additional subdirectory.</param>
        /// <param name="pattern">Pattern to copy files by.</param>
        private static void CopyDirectory(string sourcePath, string destinationPath, bool force = false,
            bool createSubDirectory = false, string pattern = "*")
        {
            DirectoryInfo source = new DirectoryInfo(sourcePath);
            DirectoryInfo destination = Directory.CreateDirectory(destinationPath);

            // Create subdirectory as needed.
            if (createSubDirectory)
            {
                destination = destination.CreateSubdirectory(source.Name);
            }

            // Enumerate over each file recursively.
            IEnumerable<FileSystemInfo> copyItems = FindRecursive(source, pattern);
            foreach (FileSystemInfo item in copyItems.ToArray())
            {
                switch (item)
                {
                    case DirectoryInfo dir:
                        Directory.CreateDirectory(Path.Combine(destination.FullName,
                            Path.GetRelativePath(source.FullName, dir.FullName)));
                        break;
                    case FileInfo file:
                        CopyFile(file, Path.Combine(destination.FullName,
                            Path.GetRelativePath(source.FullName, file.FullName)), force);
                        break;
                }
            }
        }

        /// <summary>
        /// Copy a file.
        /// </summary>
        /// <param name="sourcePath">Source path.</param>
        /// <param name="destinationPath">Destination path.</param>
        /// <param name="force">Whether to overwrite file without confirmation.</param>
        private static void CopyFile(string sourcePath, string destinationPath, bool force = false)
        {
            FileInfo source = new FileInfo(sourcePath);
            CopyFile(source, destinationPath, force);
        }

        /// <summary>
        /// Copy a file.
        /// </summary>
        /// <param name="source">Source file.</param>
        /// <param name="destinationPath">Destination path.</param>
        /// <param name="force">Whether to overwrite file without confirmation.</param>
        private static void CopyFile(FileInfo source, string destinationPath, bool force = false)
        {
            if (Directory.Exists(destinationPath) || destinationPath.EndsWith(Path.DirectorySeparatorChar))
            {
                Directory.CreateDirectory(destinationPath);
                destinationPath = Path.Combine(destinationPath, source.Name);
            }
            if (File.Exists(destinationPath))
            {
                if (force || ConfirmOverwrite(destinationPath))
                {
                    if (source.Length > BigFileThreshold)
                    {
                        Console.WriteLine(Messages.MessagePleaseWait);
                    }
                    source.CopyTo(destinationPath, true);
                }
                return;
            }
            if (source.Length > BigFileThreshold)
            {
                Console.WriteLine(Messages.MessagePleaseWait);
            }
            source.CopyTo(destinationPath);
        }
    }
}