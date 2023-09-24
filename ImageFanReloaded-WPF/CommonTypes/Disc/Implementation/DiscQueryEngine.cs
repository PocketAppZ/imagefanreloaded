using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ImageFanReloaded.CommonTypes.ImageHandling;
using ImageFanReloaded.CommonTypes.Info;
using ImageFanReloaded.Factories;

namespace ImageFanReloaded.CommonTypes.Disc.Implementation
{
    public class DiscQueryEngine
        : IDiscQueryEngine
    {
		static DiscQueryEngine()
		{
			EmptyFileSystemEntryInfoCollection = Enumerable
				.Empty<FileSystemEntryInfo>()
				.ToArray();

			EmptyImageFileCollection = Enumerable
				.Empty<IImageFile>()
				.ToArray();

			UserProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

			SpecialFolders = new List<string>
			{
				"Desktop",
				"Documents",
				"Downloads",
				"Pictures"
			};

			ImageFileExtensions = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
			{
				".bmp",
				".gif",
				".ico",
				".jpg", ".jpe", ".jpeg",
				".png",
				".tif", ".tiff"
			};
		}

		public DiscQueryEngine(
            IImageFileFactory imageFileFactory,
            IFileSystemEntryComparer fileSystemEntryComparer)
        {
            _imageFileFactory = imageFileFactory;
            _fileSystemEntryComparer = fileSystemEntryComparer;
        }

        public ICollection<FileSystemEntryInfo> GetSpecialFoldersWithPaths()
        {
            if (_specialFoldersWithPaths == null)
            {
				_specialFoldersWithPaths = SpecialFolders
							                .Select(aSpecialFolder =>
								                new FileSystemEntryInfo(
									                aSpecialFolder,
									                Path.Combine(UserProfilePath, aSpecialFolder),
									                GlobalData.FolderIcon))
							                .OrderBy(aSpecialFolder => aSpecialFolder.Name)
							                .ToArray();
			}

            return _specialFoldersWithPaths;
        }

		public ICollection<FileSystemEntryInfo> GetDrives()
		{
			if (_drives == null)
			{
				_drives = DriveInfo.GetDrives()
							.Select(aDriveInfo => aDriveInfo.Name)
							.Select(aDriveName =>
								new FileSystemEntryInfo(aDriveName,
														aDriveName,
														GlobalData.DriveIcon))
							.OrderBy(aDriveInfo => aDriveInfo.Name)
							.ToArray();
			}

			return _drives;
		}

		public ICollection<FileSystemEntryInfo> GetSubFolders(string folderPath)
        {
            try
            {
                return new DirectoryInfo(folderPath)
                                            .GetDirectories()
                                            .Select(aDirectory =>
                                                new FileSystemEntryInfo(
                                                    aDirectory.Name,
                                                    aDirectory.FullName,
                                                    GlobalData.FolderIcon))
                                            .OrderBy(aDirectory => aDirectory.Name, _fileSystemEntryComparer)
                                            .ToArray();
            }
            catch
            {
                return EmptyFileSystemEntryInfoCollection;
            }
        }

        public ICollection<IImageFile> GetImageFiles(string folderPath)
        {
            ICollection<FileInfo> filesInformation;
            try
            {
                filesInformation = new DirectoryInfo(folderPath)
                                            .GetFiles("*", SearchOption.TopDirectoryOnly)
                                            .ToArray();
            }
            catch
            {
                return EmptyImageFileCollection;
            }

            var imageFiles = filesInformation
                                .Where(aFileInfo =>
                                       ImageFileExtensions.Contains(aFileInfo.Extension))
                                .OrderBy(aFileInfo => aFileInfo.Name)
                                .Select(aFileInfo => _imageFileFactory.GetImageFile(aFileInfo.FullName))
                                .OrderBy(aFileInfo => aFileInfo.FileName, _fileSystemEntryComparer)
                                .ToArray();

            return imageFiles;
        }

        #region Private

        private static readonly ICollection<FileSystemEntryInfo> EmptyFileSystemEntryInfoCollection;
        private static readonly ICollection<IImageFile> EmptyImageFileCollection;

        private static readonly string UserProfilePath;

        private static readonly ICollection<string> SpecialFolders;

		private static readonly HashSet<string> ImageFileExtensions;

		private readonly IImageFileFactory _imageFileFactory;
        private readonly IFileSystemEntryComparer _fileSystemEntryComparer;

        private ICollection<FileSystemEntryInfo> _specialFoldersWithPaths;
		private ICollection<FileSystemEntryInfo> _drives;

		#endregion
	}
}
