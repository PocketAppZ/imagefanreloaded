using System;
using System.Collections.Generic;
using ImageFanReloaded.Core.AboutInformation;
using ImageFanReloaded.Core.ImageHandling;
using ImageFanReloaded.Core.Keyboard;
using ImageFanReloaded.Core.TextHandling.Implementation;

namespace ImageFanReloaded.Core.Global.Implementation;

public abstract class GlobalParametersBase : IGlobalParameters
{
	public int ProcessorCount { get; }
	public ImageSize ThumbnailSize { get; }
	
	public Key AltKey { get; }
	public Key TabKey { get; }
	public Key EscapeKey { get; }
	public Key EnterKey { get; }
	public Key F1Key { get; }
	public Key F4Key { get; }
	
	public KeyModifiers AltKeyModifier { get; }
	
	public HashSet<Key> BackwardNavigationKeys { get; }
	public HashSet<Key> ForwardNavigationKeys { get; }
	
	public StringComparer NameComparer { get; }

	public bool CanDisposeImage(IImage image) => !PersistentImages.Contains(image);
	
	public HashSet<string> ImageFileExtensions { get; }
	
	public string UserProfilePath { get; }
	public IReadOnlyList<string> SpecialFolders { get; }
	
	public string AboutTitle { get; }
	public string AboutText { get; }

	public abstract IImage InvalidImage { get; }
	public abstract IImage InvalidImageThumbnail { get; }
	public abstract IImage LoadingImageThumbnail { get; }
	
	public abstract HashSet<IImage> PersistentImages { get; }
	
	public abstract IImage DesktopFolderIcon { get; }
	public abstract IImage DocumentsFolderIcon { get; }
	public abstract IImage DownloadsFolderIcon { get; }
	public abstract IImage DriveIcon { get; }
	public abstract IImage FolderIcon { get; }
	public abstract IImage HomeFolderIcon { get; }
	public abstract IImage MediaFolderIcon { get; }
	public abstract IImage PicturesFolderIcon { get; }

	#region Protected
	
	protected GlobalParametersBase(IAboutInformationProvider aboutInformationProvider)
	{
		ProcessorCount = Environment.ProcessorCount;
		ThumbnailSize = new ImageSize(ThumbnailSizeSquareLength);

		AltKey = Key.Alt;
		TabKey = Key.Tab;
		EscapeKey = Key.Escape;
		EnterKey = Key.Enter;
		F1Key = Key.F1;
		F4Key = Key.F4;

		AltKeyModifier = KeyModifiers.Alt; 

		BackwardNavigationKeys = [
			Key.W,
			Key.A,
			Key.Up,
			Key.Left,
			Key.Backspace,
			Key.PageUp
		];

		ForwardNavigationKeys = [
			Key.S,
			Key.D,
			Key.Down,
			Key.Right,
			Key.Space,
			Key.PageDown
		];
		
		NameComparer = OperatingSystem.IsWindows()
			? new NaturalSortingComparer(StringComparer.InvariantCultureIgnoreCase)
			: new NaturalSortingComparer(StringComparer.InvariantCulture);
		
		ImageFileExtensions = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
		{
			".bmp",
			".cr2",
			".cur",
			".dng",
			".gif",
			".ico",
			".jfif",
			".jpe", ".jpeg", ".jpg",
			".jps",
			".nef",
			".nrw",
			".pef",
			".png",
			".raf",
			".rw2",
			".wbmp",
			".webp"
		};
		
		UserProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

		SpecialFolders = new List<string>
		{
			"Desktop",
			"Documents",
			"Downloads",
			"Media",
			"Pictures"
		};
		
		AboutTitle = "About ImageFan Reloaded";
		
		AboutText =
@$"Cross-platform, light-weight, tab-based image viewer, supporting multi-core processing

Version {aboutInformationProvider.VersionString}
Copyright © Mihnea Rădulescu 2017 - {aboutInformationProvider.Year}

https://github.com/mihnea-radulescu/imagefanreloaded";
	}
	
	protected const int IconSizeSquareLength = 36;
	
	#endregion
	
	#region Private
	
	private const int ThumbnailSizeSquareLength = 250;

	#endregion
}
