namespace FileManager
{
    /// <summary>
    /// Specifies various error codes possible.
    /// </summary>
    internal enum ErrorCode : byte
    {
        /// <summary>
        /// No errors.
        /// </summary>
        None = 0,
        
        /// <summary>
        /// Command is not found.
        /// </summary>
        CommandNotFound = 1,
        
        /// <summary>
        /// Arguments are invalid.
        /// </summary>
        InvalidArguments = 2,
        
        /// <summary>
        /// Options are invalid.
        /// </summary>
        InvalidOptions = 3,
        
        /// <summary>
        /// Drive is not found.
        /// </summary>
        DriveNotFound = 4,
        
        /// <summary>
        /// File is not found.
        /// </summary>
        FileNotFound = 5,
        
        /// <summary>
        /// Directory is not found.
        /// </summary>
        DirectoryNotFound = 6,
        
        /// <summary>
        /// Input/output error.
        /// </summary>
        InputOutputError = 7,
        
        /// <summary>
        /// Access is denied.
        /// </summary>
        UnauthorizedAccess = 10,
        
        /// <summary>
        /// Encoding is not found.
        /// </summary>
        EncodingNotFound = 20,
        
        /// <summary>
        /// Unknown error.
        /// </summary>
        Unknown = 127,
        
        /// <summary>
        /// Function is not implemented.
        /// </summary>
        NotImplemented = 128,
        
        /// <summary>
        /// Exit.
        /// </summary>
        Exit = 255
    }
}