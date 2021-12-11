namespace FileManager
{
    /// <summary>
    /// Represents a fragment of diff.
    /// </summary>
    internal sealed class DiffFragment
    {
        /// <summary>
        /// Number of fragment beginning line in the first file.
        /// </summary>
        public int BeginLine1 { get; init; }
        
        /// <summary>
        /// Number of fragment beginning line in the second file.
        /// </summary>
        public int BeginLine2 { get; init; }

        /// <summary>
        /// Number of fragment lines in the first file.
        /// </summary>
        public int Count1 { get; init; }
        
        /// <summary>
        /// Number of fragment lines in the second file.
        /// </summary>
        public int Count2 { get; init; }

        /// <summary>
        /// An array of diff lines of the fragment.
        /// </summary>
        public DiffLine[] Lines { get; init; }
    }
}