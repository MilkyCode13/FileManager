using System;

namespace FileManager
{
    /// <summary>
    /// Represents a custom exception that holds an error code.
    /// </summary>
    internal class CustomException : Exception
    {
        /// <summary>
        /// An error code of exception.
        /// </summary>
        public ErrorCode ErrorCode { get; }
        
        /// <summary>
        /// Constructs a custom exception.
        /// </summary>
        /// <param name="msg">Exception message.</param>
        /// <param name="errorCode">An error code.</param>
        public CustomException(string msg, ErrorCode errorCode) : base(msg)
        {
            ErrorCode = errorCode;
        }
    }
}