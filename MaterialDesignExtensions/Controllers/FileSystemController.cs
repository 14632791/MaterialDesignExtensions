﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MaterialDesignThemes.Wpf;

using MaterialDesignExtensions.Model;

namespace MaterialDesignExtensions.Controllers
{
    /// <summary>
    /// Controller behind the <see cref="Controls.OpenDirectoryControl" />, <see cref="Controls.OpenFileControl" /> and <see cref="Controls.SaveFileControl" />.
    /// </summary>
    public class FileSystemController : INotifyPropertyChanged
    {
        /// <summary>
        /// The property changed event.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private DirectoryInfo m_currentDirectory;
        private FileInfo m_currentFile;
        private List<DirectoryInfo> m_currentDirectoryPathParts;
        private List<DirectoryInfo> m_directories;
        private List<FileInfo> m_files;
        private bool m_showHiddenFilesAndDirectories;
        private bool m_showSystemFilesAndDirectories;

        /// <summary>
        /// The current directory shown in the control.
        /// </summary>
        public DirectoryInfo CurrentDirectory
        {
            get
            {
                return m_currentDirectory;
            }

            set
            {
                if (!AreObjectsEqual(m_currentDirectory, value))
                {
                    m_currentDirectory = value;

                    OnPropertyChanged(nameof(CurrentDirectory));
                }
            }
        }

        /// <summary>
        /// The selected file of the control.
        /// </summary>
        public FileInfo CurrentFile
        {
            get
            {
                return m_currentFile;
            }

            set
            {
                if (!AreObjectsEqual(m_currentFile, value))
                {
                    m_currentFile = value;

                    OnPropertyChanged(nameof(CurrentFile));
                }
            }
        }

        /// <summary>
        /// The list of sub directories to <see cref="CurrentDirectory" />.
        /// </summary>
        public List<DirectoryInfo> CurrentDirectoryPathParts
        {
            get
            {
                return m_currentDirectoryPathParts;
            }

            set
            {
                if (!AreObjectsEqual(m_currentDirectoryPathParts, value))
                {
                    m_currentDirectoryPathParts = value;

                    OnPropertyChanged(nameof(CurrentDirectoryPathParts));
                }
            }
        }

        /// <summary>
        /// The directories inside <see cref="CurrentDirectory" />.
        /// </summary>
        public List<DirectoryInfo> Directories
        {
            get
            {
                return m_directories;
            }

            set
            {
                if (!AreObjectsEqual(m_directories, value))
                {
                    m_directories = value;

                    OnPropertyChanged(nameof(Directories));
                }
            }
        }

        /// <summary>
        /// The directories and files inside <see cref="CurrentDirectory" />.
        /// </summary>
        public List<FileSystemInfo> DirectoriesAndFiles
        {
            get
            {
                List<FileSystemInfo> directoriesAndFiles = new List<FileSystemInfo>();

                if (m_directories != null)
                {
                    directoriesAndFiles.AddRange(m_directories);
                }

                if (m_files != null)
                {
                    directoriesAndFiles.AddRange(m_files);
                }

                return directoriesAndFiles;
            }
        }

        /// <summary>
        /// The system's drives.
        /// </summary>
        public List<SpecialDrive> Drives
        {
            get
            {
                DriveType[] supportedDriveTypes = { DriveType.CDRom, DriveType.Fixed, DriveType.Network, DriveType.Removable };

                return DriveInfo.GetDrives()
                    .Where(driveInfo => supportedDriveTypes.Contains(driveInfo.DriveType))
                    .Select(driveInfo =>
                    {
                        PackIconKind icon = PackIconKind.Harddisk;

                        if (driveInfo.DriveType == DriveType.CDRom)
                        {
                            icon = PackIconKind.Disk;
                        }
                        else if (driveInfo.DriveType == DriveType.Removable)
                        {
                            icon = PackIconKind.Usb;
                        }
                        else if (driveInfo.DriveType == DriveType.Network)
                        {
                            icon = PackIconKind.ServerNetwork;
                        }

                        string label = driveInfo.Name;

                        if (label.EndsWith(@"\"))
                        {
                            label = label.Substring(0, label.Length - 1);
                        }

                        string volumeLabel = driveInfo.IsReady ? driveInfo.VolumeLabel : null;

                        if (string.IsNullOrWhiteSpace(volumeLabel) && driveInfo.DriveType == DriveType.Fixed)
                        {
                            volumeLabel = Localization.Strings.LocalDrive;
                        }

                        if (!string.IsNullOrWhiteSpace(volumeLabel))
                        {
                            label = volumeLabel + " (" + label + ")";
                        }

                        return new SpecialDrive() { Info = driveInfo, Icon = icon, Label = label };
                    })
                    .ToList();
            }
        }

        /// <summary>
        /// The files inside <see cref="CurrentDirectory" />.
        /// </summary>
        public List<FileInfo> Files
        {
            get
            {
                return m_files;
            }

            set
            {
                if (!AreObjectsEqual(m_files, value))
                {
                    m_files = value;

                    OnPropertyChanged(nameof(Files));
                }
            }
        }

        /// <summary>
        /// The special directories (e.g. music directory) of the user.
        /// </summary>
        public List<SpecialDirectory> SpecialDirectories
        {
            get
            {
                return new List<SpecialDirectory>()
                {
                    new SpecialDirectory() { Info = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)),
                        Icon = PackIconKind.Account },
                    new SpecialDirectory() { Info = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)),
                        Label = Localization.Strings.Documents, Icon = PackIconKind.FileDocument },
                    new SpecialDirectory() { Info = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)),
                        Label = Localization.Strings.Pictures, Icon = PackIconKind.FileImage },
                    new SpecialDirectory() { Info = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)),
                        Label = Localization.Strings.Music, Icon = PackIconKind.FileMusic },
                    new SpecialDirectory() { Info = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos)),
                        Label = Localization.Strings.Videos, Icon = PackIconKind.FileVideo },
                    new SpecialDirectory() { Info = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop)),
                        Label = Localization.Strings.Desktop, Icon = PackIconKind.Monitor }
                };
            }
        }

        /// <summary>
        /// Specifies whether hidden files and directories will be shown or not.
        /// </summary>
        public bool ShowHiddenFilesAndDirectories
        {
            get
            {
                return m_showHiddenFilesAndDirectories;
            }

            set
            {
                if (!m_showHiddenFilesAndDirectories != value)
                {
                    m_showHiddenFilesAndDirectories = value;

                    OnPropertyChanged(nameof(ShowHiddenFilesAndDirectories));
                }
            }
        }

        /// <summary>
        /// Specifies whether protected system files and directories will be shown or not.
        /// </summary>
        public bool ShowSystemFilesAndDirectories
        {
            get
            {
                return m_showSystemFilesAndDirectories;
            }

            set
            {
                if (m_showSystemFilesAndDirectories != value)
                {
                    m_showSystemFilesAndDirectories = value;

                    OnPropertyChanged(nameof(ShowSystemFilesAndDirectories));
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="FileSystemController" />.
        /// </summary>
        public FileSystemController()
        {
            m_currentDirectory = null;
            m_currentFile = null;
            m_currentDirectoryPathParts = null;
            m_directories = null;
            m_files = null;
            m_showHiddenFilesAndDirectories = false;
            m_showSystemFilesAndDirectories = false;
        }

        /// <summary>
        /// Selects a new current directory.
        /// </summary>
        /// <param name="directory"></param>
        public void SelectDirectory(string directory)
        {
            SelectDirectory(new DirectoryInfo(directory));
        }

        /// <summary>
        /// Selects a new current directory.
        /// </summary>
        /// <param name="directory"></param>
        public void SelectDirectory(DirectoryInfo directory)
        {
            bool ShowFileSystemInfo(FileSystemInfo fileSystemInfo) => (ShowHiddenFilesAndDirectories || !fileSystemInfo.Attributes.HasFlag(FileAttributes.Hidden))
                                                                            && (ShowSystemFilesAndDirectories || !fileSystemInfo.Attributes.HasFlag(FileAttributes.System));

            if (directory != null && !string.IsNullOrWhiteSpace(directory.FullName))
            {
                if (!directory.Exists)
                {
                    throw new FileNotFoundException(string.Format(Localization.Strings.DirectoryXNotFound, directory.Name));
                }

                try
                {
                    // try to access the directory before assigning it as a kind of access control check
                    //     if the user is not allowed to access the directory, the controller does not change the current directory
                    List<DirectoryInfo> directories = directory.GetDirectories()
                        .Where(directoryInfo => ShowFileSystemInfo(directoryInfo))
                        .ToList();

                    List<FileInfo> files = directory.GetFiles()
                        .Where(fileInfo => ShowFileSystemInfo(fileInfo))
                        .ToList();

                    CurrentDirectory = directory;
                    Directories = directories;
                    Files = files;

                    OnPropertyChanged(nameof(DirectoriesAndFiles));
                }
                catch (UnauthorizedAccessException exc)
                {
                    throw new UnauthorizedAccessException(string.Format(Localization.Strings.AccessToDirectoryXDenied, directory.Name), exc);
                }
            }
            else
            {
                CurrentDirectory = null;
                Directories = null;
                Files = null;

                OnPropertyChanged(nameof(DirectoriesAndFiles));
            }

            UpdateCurrentDirectoryPathParts();
        }

        /// <summary>
        /// Selects a file.
        /// </summary>
        /// <param name="file"></param>
        public void SelectFile(String file)
        {
            if (!string.IsNullOrWhiteSpace(file))
            {
                SelectFile(new FileInfo(file));
            }
            else
            {
                CurrentFile = null;
            }
        }

        /// <summary>
        /// Selects a file.
        /// </summary>
        /// <param name="file"></param>
        public void SelectFile(FileInfo file)
        {
            CurrentFile = file;
        }

        private void UpdateCurrentDirectoryPathParts()
        {
            List<DirectoryInfo> currentDirectoryPathParts = null;

            if (m_currentDirectory != null)
            {
                currentDirectoryPathParts = new List<DirectoryInfo>();
                DirectoryInfo directoryInfo = m_currentDirectory;

                while (directoryInfo != null)
                {
                    currentDirectoryPathParts.Add(directoryInfo);
                    directoryInfo = directoryInfo.Parent;
                }
                
                currentDirectoryPathParts.Sort((directoryInfo1, directoryInfo2) => directoryInfo1.FullName.CompareTo(directoryInfo2.FullName));
            }

            CurrentDirectoryPathParts = currentDirectoryPathParts;
        }

        /// <summary>
        /// Setter intended for internal use.
        /// </summary>
        /// <param name="showHiddenFilesAndDirectories"></param>
        public void SetShowHiddenFilesAndDirectories(bool showHiddenFilesAndDirectories)
        {
            ShowHiddenFilesAndDirectories = showHiddenFilesAndDirectories;

            SelectDirectory(m_currentDirectory);
        }

        /// <summary>
        /// Setter intended for internal use.
        /// </summary>
        /// <param name="showSystemFilesAndDirectories"></param>
        public void SetShowSystemFilesAndDirectories(bool showSystemFilesAndDirectories)
        {
            ShowSystemFilesAndDirectories = showSystemFilesAndDirectories;

            SelectDirectory(m_currentDirectory);
        }

        private bool AreObjectsEqual(object o1, object o2)
        {
            if (o1 == o2)
            {
                return true;
            }

            if (o1 != null)
            {
                return o1.Equals(o2);
            }

            return false;
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null && !string.IsNullOrWhiteSpace(propertyName))
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
