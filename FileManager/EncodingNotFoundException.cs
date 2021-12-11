using System;

namespace FileManager
{
    /// <summary>
    /// The exception that is thrown when encoding is not found.
    /// </summary>
    internal class EncodingNotFoundException : ArgumentException
    {
        /// <summary>
        /// Constructs an exception using string which represents missing encoding.
        /// </summary>
        /// <param name="encodingString">String which represents missing encoding.</param>
        public EncodingNotFoundException(string encodingString)
        {
            EncodingString = encodingString;
        }
        
        /// <summary>
        /// String which represents missing encoding.
        /// </summary>
        public string EncodingString { get; }
    }
}