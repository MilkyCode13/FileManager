using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FileManager.Resources;

namespace FileManager
{
    /// <summary>
    /// Calculates diff. Using algorithm from http://www.xmailserver.org/diff2.pdf.
    /// Inspired by http://www.mathertel.de/Diff/ implementation licensed under BSD license.
    /// </summary>
    internal class Diff
    {
        /// <summary>
        /// The first lines.
        /// </summary>
        private string[] _lines1;
        
        /// <summary>
        /// The second lines.
        /// </summary>
        private string[] _lines2;

        /// <summary>
        /// Hashtable matching lines and numbers (numbers are easier to process).
        /// </summary>
        private Hashtable _hashtable;

        /// <summary>
        /// The first sequence of numbers corresponding to lines.
        /// </summary>
        private int[] _seq1;
        
        /// <summary>
        /// The second sequence of numbers corresponding to lines.
        /// </summary>
        private int[] _seq2;
        
        /// <summary>
        /// Array of booleans representing whether the line of the first file was removed.
        /// </summary>
        private bool[] _modified1;
        
        /// <summary>
        /// Array of booleans representing whether the line of the second file was added.
        /// </summary>
        private bool[] _modified2;

        /// <summary>
        /// Vector of values for forward D-path search.
        /// </summary>
        private int[] _forwardVector;
        
        /// <summary>
        /// Vector of values for reverse D-path search.
        /// </summary>
        private int[] _reverseVector;

        /// <summary>
        /// Initializes and calculates the diff of two files.
        /// </summary>
        /// <param name="path1">Path to the first file.</param>
        /// <param name="path2">Path to the second file.</param>
        public Diff(string path1, string path2)
        {
            Path1 = path1;
            Path2 = path2;
            TimeStamp1 = new FileInfo(path1).LastWriteTime;
            TimeStamp2 = new FileInfo(path2).LastWriteTime;
            _lines1 = FileOperations
                .ReadFileLines(path1, Encoding.Default, 100000, 2 * 1024 * 1024).ToArray();
            _lines2 = FileOperations
                .ReadFileLines(path2, Encoding.Default, 100000, 2 * 1024 * 1024).ToArray();
            
            FillData();
            GetLongestCommonSubsequence(0, _lines1.Length, 0, _lines2.Length);
            
            DiffFragments = GetDiffFragments().ToArray();
        }
        
        /// <summary>
        /// Path to the first file.
        /// </summary>
        public string Path1 { get; }
        
        /// <summary>
        /// Path to the second file.
        /// </summary>
        public string Path2 { get; }
        
        /// <summary>
        /// Time stamp of the first file.
        /// </summary>
        public DateTime TimeStamp1 { get; }
        
        /// <summary>
        /// Time stamp of the second file.
        /// </summary>
        public DateTime TimeStamp2 { get; }
        
        /// <summary>
        /// An array of diff fragments.
        /// </summary>
        public DiffFragment[] DiffFragments { get; }
        
        /// <summary>
        /// Gets diff fragments.
        /// </summary>
        /// <returns>List of diff fragments.</returns>
        private List<DiffFragment> GetDiffFragments()
        {
            DiffLine[] diffLines = GetDiffLines().ToArray();
            var diffFragments = new List<DiffFragment>();

            var beginIndex = 0;
            var beginLine1 = 1;
            var beginLine2 = 1;
            var count1 = 0;
            var count2 = 0;
            var consequentUnchangedCount = 0;
            for (var i = 0; i < diffLines.Length; i++)
            {
                // Line is unchanged.
                if (diffLines[i].ChangeType == ' ')
                {
                    consequentUnchangedCount++;
                    count1++;
                    count2++;
                    // If many consequent unchanged lines, break into two different fragments.
                    if (consequentUnchangedCount == 7 && i != 6)
                    {
                        diffFragments.Add(new DiffFragment
                        {
                            BeginLine1 = beginLine1, BeginLine2 = beginLine2, Count1 = count1 - 4, Count2 = count2 - 4,
                            Lines = diffLines[beginIndex..(i - 3)]
                        });
                        beginIndex = i - 3;
                    }
                }
                else
                {
                    // If were many consequent unchanged lines, change the beginning of the next fragment.
                    if (consequentUnchangedCount > (consequentUnchangedCount == i ? 3 : 6))
                    {
                        beginIndex = i - 3;
                        beginLine1 = diffLines[beginIndex].LineNumber1 ?? beginLine1;
                        beginLine2 = diffLines[beginIndex].LineNumber2 ?? beginLine2;
                        count1 = 3;
                        count2 = 3;
                    }
                    consequentUnchangedCount = 0;
                    // Increment corresponding line counter.
                    if (diffLines[i].ChangeType == '-')
                    {
                        count1++;
                    }
                    else
                    {
                        count2++;
                    }
                }
            }

            // Add last fragment if needed.
            if (diffLines.Length - beginIndex > consequentUnchangedCount)
            {
                int leaveLast = Math.Max(consequentUnchangedCount - 3, 0);
                diffFragments.Add(new DiffFragment
                {
                    BeginLine1 = beginLine1, BeginLine2 = beginLine2, Count1 = count1 - leaveLast,
                    Count2 = count2 - leaveLast, Lines = diffLines[beginIndex..^leaveLast]
                });
            }

            return diffFragments;
        }
        
        /// <summary>
        /// Gets diff lines.
        /// </summary>
        /// <returns>List of diff lines.</returns>
        private List<DiffLine> GetDiffLines()
        {
            var diffLines = new List<DiffLine>();

            var index1 = 0;
            var index2 = 0;

            while (index1 < _seq1.Length || index2 < _seq2.Length)
            {
                // Removed line.
                while (index1 < _seq1.Length && _modified1[index1])
                {
                    index1++;
                    diffLines.Add(
                        new DiffLine(index1, null, '-', _lines1[index1 - 1]));
                }
                // Added line.
                while (index2 < _seq2.Length && _modified2[index2])
                {
                    index2++;
                    diffLines.Add(
                        new DiffLine(null, index2, '+', _lines2[index2 - 1]));
                }
                // Unchanged line.
                while (index1 < _seq1.Length && index2 < _seq2.Length && !_modified1[index1] && !_modified2[index2])
                {
                    index1++;
                    index2++;
                    diffLines.Add(
                        new DiffLine(index1, index2, ' ', _lines1[index1 - 1]));
                }
            }
            
            return diffLines;
        }

        /// <summary>
        /// Initializes fields for calculation.
        /// </summary>
        private void FillData()
        {
            // Create a hashtable to map strings to numbers.
            InitHashtable();

            // Init modified arrays.
            _modified1 = new bool[_lines1.Length];
            _modified2 = new bool[_lines2.Length];
            
            // Init forward and reverse vectors.
            int max = _seq1.Length + _seq2.Length + 1;
            _forwardVector = new int[2 * (max + 1)];
            _reverseVector = new int[2 * (max + 1)];
        }
        
        /// <summary>
        /// Initializes hashtable of lines.
        /// </summary>
        private void InitHashtable()
        {
            _hashtable = new Hashtable(_lines1.Length + _lines2.Length);

            _seq1 = new int[_lines1.Length];
            _seq2 = new int[_lines2.Length];

            int codesUsed = 0;
            for (var i = 0; i < _lines1.Length; i++)
            {
                if (_hashtable[_lines1[i]] == null)
                {
                    _hashtable[_lines1[i]] = codesUsed;
                    _seq1[i] = codesUsed;
                    codesUsed++;
                }
                else
                {
                    _seq1[i] = (int) _hashtable[_lines1[i]];
                }
            }
            for (var i = 0; i < _lines2.Length; i++)
            {
                if (_hashtable[_lines2[i]] == null)
                {
                    _hashtable[_lines2[i]] = codesUsed;
                    _seq2[i] = codesUsed;
                    codesUsed++;
                }
                else
                {
                    _seq2[i] = (int) _hashtable[_lines2[i]];
                }
            }
        }
        
        /// <summary>
        /// Gets the shortest middle snake.
        /// </summary>
        /// <param name="begin1">Begin line of 1 file.</param>
        /// <param name="end1">End line of 1 file (exclusive).</param>
        /// <param name="begin2">Begin line of 2 file.</param>
        /// <param name="end2">End line of 2 file (exclusive).</param>
        /// <returns></returns>
        /// <exception cref="ApplicationException">
        /// That should NEVER happen. If it happens, I've badly messed up with algorithm. I'm sorry.
        /// </exception>
        private (int X, int Y) GetShortestMiddleSnake(int begin1, int end1, int begin2, int end2)
        {
            int max = _seq1.Length + _seq2.Length + 1;
            // K-diagonals for forward and reverse searches.
            int forwardK = begin1 - begin2;
            int reverseK = end1 - end2;
            int delta = reverseK - forwardK;
            bool isDeltaOdd = (delta & 1) != 0;
            // Index offsets needed to ensure that they are always positive.
            int forwardVectorOffset = max - forwardK;
            int reverseVectorOffset = max - reverseK;
            // Maximum D (number of non-diagonal edges of path).
            int dMax = (end1 - begin1 + end2 - begin2) / 2 + 1;

            // Initialize vectors.
            _forwardVector[forwardVectorOffset + forwardK + 1] = begin1;
            _reverseVector[reverseVectorOffset + reverseK - 1] = end1;

            for (var d = 0; d <= dMax; d++)
            {
                // Extend forward path.
                for (int k = forwardK - d; k <= forwardK + d; k += 2)
                {
                    TraceForward(end1, end2, d, k, forwardK, forwardVectorOffset);
                    // If forward and reverse paths overlap, return coords of middle snake.
                    if (isDeltaOdd && reverseK - d < k && k < reverseK + d &&
                        _reverseVector[reverseVectorOffset + k] <= _forwardVector[forwardVectorOffset + k])
                    {
                        return (_forwardVector[forwardVectorOffset + k], _forwardVector[forwardVectorOffset + k] - k);
                    }
                }
                // Extend reverse path.
                for (int k = reverseK - d; k <= reverseK + d; k += 2)
                {
                    TraceReverse(begin1, begin2, d, k, reverseK, reverseVectorOffset);
                    // If forward and reverse paths overlap, return coords of middle snake.
                    if (!isDeltaOdd && forwardK - d <= k && k <= forwardK + d &&
                        _reverseVector[reverseVectorOffset + k] <= _forwardVector[forwardVectorOffset + k])
                    {
                        return (_forwardVector[forwardVectorOffset + k], _forwardVector[forwardVectorOffset + k] - k);
                    }
                }
            }

            throw new ApplicationException("RIP algorithm");
        }

        /// <summary>
        /// Traces the D-path forward.
        /// </summary>
        /// <param name="end1">End line of 1 file (exclusive).</param>
        /// <param name="end2">End line of 2 file (exclusive).</param>
        /// <param name="d">D (number of non-diagonal edges of path).</param>
        /// <param name="k">K (diagonal number).</param>
        /// <param name="forwardK">K for (begin1, begin2).</param>
        /// <param name="forwardVectorOffset">Index offset of forward vector.</param>
        private void TraceForward(int end1, int end2, int d, int k, int forwardK, int forwardVectorOffset)
        {
            int x;
            if (k == forwardK - d)
            {
                x = _forwardVector[forwardVectorOffset + k + 1];
            }
            else
            {
                x = _forwardVector[forwardVectorOffset + k - 1] + 1;
                if (k < forwardK + d && _forwardVector[forwardVectorOffset + k + 1] >= x)
                {
                    x = _forwardVector[forwardVectorOffset + k + 1];
                }
            }

            // Calculating y coordinate from x coordinate and diagonal number.
            int y = x - k;

            // Follow the snake.
            while (x < end1 && y < end2 && _seq1[x] == _seq2[y])
            {
                x++;
                y++;
            }
            _forwardVector[forwardVectorOffset + k] = x;
        }

        /// <summary>
        /// Traces the D-path reverse.
        /// </summary>
        /// <param name="begin1">Begin line of 1 file.</param>
        /// <param name="begin2">Begin line of 2 file.</param>
        /// <param name="d">D (number of non-diagonal edges of path).</param>
        /// <param name="k">K (diagonal number).</param>
        /// <param name="reverseK">K for (end1, end2).</param>
        /// <param name="reverseVectorOffset">Index offset of reverse vector.</param>
        private void TraceReverse(int begin1, int begin2, int d, int k, int reverseK, int reverseVectorOffset)
        {
            int x;
            if (k == reverseK + d)
            {
                x = _reverseVector[reverseVectorOffset + k - 1];
            }
            else
            {
                x = _reverseVector[reverseVectorOffset + k + 1] - 1;
                if (k > reverseK - d && _reverseVector[reverseVectorOffset + k - 1] < x)
                {
                    x = _reverseVector[reverseVectorOffset + k - 1];
                }
            }

            // Calculating y coordinate from x coordinate and diagonal number.
            int y = x - k;

            // Follow the snake.
            while ((x > begin1) && (y > begin2) && (_seq1[x - 1] == _seq2[y - 1]))
            {
                x--;
                y--;
            }
            _reverseVector[reverseVectorOffset + k] = x;
        }

        /// <summary>
        /// Calculates the longest common subsequence for two files.
        /// </summary>
        /// <param name="begin1">Begin line of 1 file.</param>
        /// <param name="end1">End line of 1 file (exclusive).</param>
        /// <param name="begin2">Begin line of 2 file.</param>
        /// <param name="end2">End line of 2 file (exclusive).</param>
        private void GetLongestCommonSubsequence(int begin1, int end1, int begin2, int end2)
        {
            // Skip equal lines on begin.
            while (begin1 < end1 && begin2 < end2 && _seq1[begin1] == _seq2[begin2])
            {
                begin1++;
                begin2++;
            }
            // Skip equal lines on end.
            while (begin1 < end1 && begin2 < end2 && _seq1[end1 - 1] == _seq2[end2 - 1])
            {
                end1--;
                end2--;
            }

            // If there are no lines left on one of the files, mark all remaining lines of the other as modified.
            if (begin1 == end1)
            {
                while (begin2 < end2)
                {
                    _modified2[begin2] = true;
                    begin2++;
                }
            }
            else if (begin2 == end2)
            {
                while (begin1 < end1)
                {
                    _modified1[begin1] = true;
                    begin1++;
                }
            }
            else
            {
                // Get the middle snake coordinates and divide the LCS in two.
                (int x, int y) = GetShortestMiddleSnake(begin1, end1, begin2, end2);
                
                GetLongestCommonSubsequence(begin1, x, begin2, y);
                GetLongestCommonSubsequence(x, end1, y, end2);
            }
        }
    }
}