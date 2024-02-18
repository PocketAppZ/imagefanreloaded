using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using ImageFanReloaded.CommonTypes.CustomEventArgs;
using ImageFanReloaded.CommonTypes.Info;
using ImageFanReloaded.Factories;
using ImageFanReloaded.Infrastructure;

namespace ImageFanReloaded.Controls.Implementation;

public partial class ContentTabItem : UserControl, IContentTabItem
{
	public ContentTabItem()
	{
		InitializeComponent();
    }

    public TabItem? TabItem { get; set; }
    public Window? Window { get; set; }

	public IImageViewFactory? ImageViewFactory { get; set; }
	public object? GenerateThumbnailsLockObject { get; set; }

	public IFolderVisualState? FolderVisualState { get; set; }

	public event EventHandler<FolderChangedEventArgs>? FolderChanged;
	
	public void OnKeyPressed(object? sender, KeyEventArgs e)
	{
		if (_selectedThumbnailBox is not null)
		{
			var keyPressed = e.Key;
	        
			if (GlobalData.BackwardNavigationKeys.Contains(keyPressed))
			{
				AdvanceToThumbnailIndex(-1);
			}
			else if (GlobalData.ForwardNavigationKeys.Contains(keyPressed))
			{
				AdvanceToThumbnailIndex(1);
			}
			else if (keyPressed == GlobalData.EnterKey)
			{
				DisplayImage();
			}
		}
	}
	
	public void SetTitle(string title) => TabItem!.Header = title;

	public void PopulateSubFoldersTree(IReadOnlyCollection<FileSystemEntryInfo> subFolders,
									   bool rootNodes)
	{
		if (rootNodes)
		{
			foreach (var aSubFolder in subFolders)
			{
				var treeViewItem = GetTreeViewItem(aSubFolder);

				_folderTreeView.Items.Add(treeViewItem);
			}
		}
		else if (_folderTreeView.SelectedItem is not null)
		{
			var selectedItem = (TreeViewItem)_folderTreeView.SelectedItem;

			selectedItem.Items.Clear();

			foreach (var aSubFolder in subFolders)
			{
				var treeViewItem = GetTreeViewItem(aSubFolder);

				selectedItem.Items.Add(treeViewItem);
			}
		}
	}

	public void ClearThumbnailBoxes()
	{
		_thumbnailWrapPanel.Children.Clear();

		if (_thumbnailBoxList is not null)
		{
			foreach (var aThumbnailBox in _thumbnailBoxList)
			{
				aThumbnailBox.DisposeThumbnail();
			}
		}

		_thumbnailBoxList = new List<ThumbnailBox>();
		_selectedThumbnailIndex = -1;
		_selectedThumbnailBox = null;

		_thumbnailScrollViewer.Offset = new Vector(
			_thumbnailScrollViewer.Offset.X, 0);
	}

	public void PopulateThumbnailBoxes(
		IReadOnlyCollection<ThumbnailInfo> thumbnailInfoCollection)
	{
		foreach (var thumbnailInfo in thumbnailInfoCollection)
		{
			var aThumbnailBox = new ThumbnailBox
			{
				ThumbnailInfo = thumbnailInfo
			};

			thumbnailInfo.ThumbnailBox = aThumbnailBox;

			aThumbnailBox.ThumbnailBoxClicked +=
				(sender, e) =>
				{
					var thumbnailBox = (ThumbnailBox)sender!;

					if (thumbnailBox.IsSelected)
					{
						DisplayImage();
					}
					else
					{
						SelectThumbnailBox(thumbnailBox);
					}
				};

			_thumbnailBoxList!.Add(aThumbnailBox);

			if (_thumbnailBoxList.Count == 1)
			{
				SelectThumbnailBox(aThumbnailBox);
				_thumbnailScrollViewer.Focus();
			}

			var aSurroundingStackPanel = new StackPanel
			{
				Focusable = true
			};
			aSurroundingStackPanel.Children.Add(aThumbnailBox);
			_thumbnailWrapPanel.Children.Add(aSurroundingStackPanel);
		}
	}

	public void RefreshThumbnailBoxes(
		IReadOnlyCollection<ThumbnailInfo> thumbnailInfoCollection)
	{
		foreach (var thumbnailInfo in thumbnailInfoCollection)
		{
			thumbnailInfo.RefreshThumbnail();
		}
	}

	public void SetStatusBarText(string statusBarText)
	{
		_textBlockInfo.Text = statusBarText;
	}
	
    #region Private

    private const string FakeTreeViewItemChild = "Loading...";

    private List<ThumbnailBox>? _thumbnailBoxList;
	private int _selectedThumbnailIndex;
	private ThumbnailBox? _selectedThumbnailBox;

    private void OnFolderTreeViewSelectedItemChanged(object? sender,
													 SelectionChangedEventArgs e)
	{
		var folderChangedHandler = FolderChanged;

		if (folderChangedHandler is not null)
		{
			var selectedFolderTreeViewItem = (TreeViewItem)e.AddedItems[0]!;
			var fileSystemEntryItem = (IFileSystemEntryItem)
				selectedFolderTreeViewItem.Header!;

			var fileSystemEntryInfo = fileSystemEntryItem.FileSystemEntryInfo;
			var selectedFolderPath = fileSystemEntryInfo.Path;

			folderChangedHandler(this,
								 new FolderChangedEventArgs(selectedFolderPath));
		}
	}

    private void SelectThumbnailBox(ThumbnailBox thumbnailBox)
	{
		if (_selectedThumbnailBox != thumbnailBox)
		{
			_selectedThumbnailBox?.UnselectThumbnail();
			
			_selectedThumbnailBox = thumbnailBox;
			_selectedThumbnailIndex = _thumbnailBoxList
				!.FindIndex(aThumbnailBox => aThumbnailBox == _selectedThumbnailBox);

			_selectedThumbnailBox.SelectThumbnail();
		}
	}

    private bool AdvanceToThumbnailIndex(int increment)
	{
		if (_selectedThumbnailBox is not null)
		{
			var newSelectedThumbnailIndex = _selectedThumbnailIndex + increment;

			if (newSelectedThumbnailIndex >= 0 &&
				newSelectedThumbnailIndex < _thumbnailBoxList!.Count)
			{
				_selectedThumbnailBox.UnselectThumbnail();

				_selectedThumbnailIndex = newSelectedThumbnailIndex;
				_selectedThumbnailBox = _thumbnailBoxList[_selectedThumbnailIndex];

				_selectedThumbnailBox.SelectThumbnail();

				return true;
			}
		}

		return false;
	}

	private async void DisplayImage()
	{
		var imageView = ImageViewFactory!.GetImageView();
		imageView.SetImage(_selectedThumbnailBox!.ImageFile!);

		imageView.ThumbnailChanged +=
			(sender, e) =>
			{
				if (AdvanceToThumbnailIndex(e.Increment))
				{
					imageView.SetImage(_selectedThumbnailBox!.ImageFile!);
				}
			};

		await imageView.ShowDialog(Window!);

		_selectedThumbnailBox?.Focus();
	}

	private static TreeViewItem GetTreeViewItem(FileSystemEntryInfo fileSystemEntryInfo)
	{
		IFileSystemEntryItem fileSystemEntryItem = new FileSystemEntryItem
		{
			FileSystemEntryInfo = fileSystemEntryInfo
		};

		var treeViewItem = new TreeViewItem
		{
			Header = fileSystemEntryItem
		};

		if (fileSystemEntryInfo.HasSubFolders)
		{
			treeViewItem.Items.Add(FakeTreeViewItemChild);
		}

		return treeViewItem;
	}

	#endregion
}
