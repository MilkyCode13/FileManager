using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace FileManager
{
    /// <summary>
    /// Manages drive operations safely.
    /// </summary>
    internal sealed class DriveManager
    {
        /// <summary>
        /// DriveInfo object containing information for drive.
        /// </summary>
        private readonly DriveInfo _driveInfo;

        /// <summary>
        /// Constructs a drive manager object using <see cref="DriveInfo"/>.
        /// </summary>
        /// <param name="driveInfo">Corresponding <see cref="DriveInfo"/> object.</param>
        /// <exception cref="ArgumentNullException">Argument is null.</exception>
        /// <exception cref="DriveNotFoundException">Drive is not present and ready.</exception>
        private DriveManager([DisallowNull] DriveInfo driveInfo)
        {
            if (driveInfo is null)
            {
                throw new ArgumentNullException(nameof(driveInfo));
            }
            
            if (!driveInfo.IsReady)
            {
                throw new DriveNotFoundException("Drive is not present and ready.");
            }
            _driveInfo = driveInfo;
        }
        
        /// <summary>
        /// Gets the name of a drive, such as C:\.
        /// </summary>
        [NotNull]
        public string Name => _driveInfo.Name;

        /// <summary>
        /// Gets the drive type, such as CD-ROM, removable, network, or fixed.
        /// </summary>
        public DriveType DriveType => _driveInfo.DriveType;

        /// <summary>
        /// Gets the name of the file system on the specified drive.
        /// </summary>
        [NotNull]
        public string DriveFormat
        {
            get
            {
                try
                {
                    return _driveInfo.DriveFormat;
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Gets the amount of available free space on a drive, in bytes.
        /// </summary>
        public long AvailableFreeSpace
        {
            get
            {
                try
                {
                    return _driveInfo.AvailableFreeSpace;
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }
        
        /// <summary>
        /// Gets the total amount of free space available on a drive, in bytes.
        /// </summary>
        public long TotalFreeSpace
        {
            get
            {
                try
                {
                    return _driveInfo.TotalFreeSpace;
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }
        
        /// <summary>
        /// Gets the total size of storage space on a drive, in bytes.
        /// </summary>
        public long TotalSize
        {
            get
            {
                try
                {
                    return _driveInfo.TotalSize;
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }
        
        /// <summary>
        /// Retrieves the drive names of all logical drives on a computer.
        /// </summary>
        /// <returns>An array of DriveManager objects.</returns>
        [return: NotNull]
        public static IEnumerable<DriveManager> GetDrives()
        {
            return DriveInfo.GetDrives().
                Where(drive => drive.IsReady).Select(drive => new DriveManager(drive));
        }
    }
}