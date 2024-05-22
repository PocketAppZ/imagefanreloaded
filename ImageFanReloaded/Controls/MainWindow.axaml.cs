using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using ImageFanReloaded.Core.Controls;
using ImageFanReloaded.Core.CustomEventArgs;
using ImageFanReloaded.Core.Global;
using ImageFanReloaded.Core.Synchronization;
using ImageFanReloaded.Keyboard;

namespace ImageFanReloaded.Controls;

public partial class MainWindow : Window, IMainView
{
	public MainWindow()
    {
        InitializeComponent();

		_windowFontSize = FontSize;
		
		AddHandler(KeyDownEvent, OnKeyPressing, RoutingStrategies.Tunnel);
		AddHandler(KeyUpEvent, OnKeyPressed, RoutingStrategies.Tunnel);
    }
	
	public IGlobalParameters? GlobalParameters { get; set; }
	public IFolderChangedEventHandleFactory? FolderChangedEventHandleFactory { get; set; }

	public event EventHandler<ContentTabItemEventArgs>? ContentTabItemAdded;
	public event EventHandler<ContentTabItemEventArgs>? ContentTabItemClosed;
	public event EventHandler<TabCountChangedEventArgs>? TabCountChanged;
	public event EventHandler? HelpMenuRequested;

	public void AddFakeTabItem()
	{
		var fakeTabItem = new TabItem
		{
			Header = FakeTabItemTitle,
			FontSize = _windowFontSize
		};

		_tabControl.Items.Add(fakeTabItem);
	}
	
	public async Task ShowInfoMessage(string title, string text)
	{
		var infoMessageBox = MessageBoxManager.GetMessageBoxStandard(
			title,
			text,
			MsBox.Avalonia.Enums.ButtonEnum.Ok,
			MsBox.Avalonia.Enums.Icon.Info,
			WindowStartupLocation.CenterOwner);

		await infoMessageBox.ShowWindowDialogAsync(this);
	}
	
	#region Private

	private const string DefaultTabItemTitle = "New Tab";
	private const string FakeTabItemTitle = "➕";

	private readonly double _windowFontSize;

	private void OnKeyPressing(object? sender, KeyEventArgs e)
	{
		e.Handled = true;
	}

	private void OnKeyPressed(object? sender, KeyEventArgs e)
    {
        var keyPressed = e.Key.ToCoreKey();
        var keyModifiers = e.KeyModifiers.ToCoreKeyModifiers();

        if (ShouldCloseWindow(keyPressed, keyModifiers))
        {
	        CloseWindow();
        }
        else if (keyPressed == GlobalParameters!.TabKey)
        {
	        var contentTabItemCount = GetContentTabItemCount();
	        var canNavigateAcrossTabs = contentTabItemCount > 1;

	        if (canNavigateAcrossTabs)
	        {
		        var selectedTabItemIndex = _tabControl.SelectedIndex;
		        var nextSelectedTabItemIndex = (selectedTabItemIndex + 1) % contentTabItemCount;
		        _tabControl.SelectedIndex = nextSelectedTabItemIndex;
	        }
		}
        else if (keyPressed == GlobalParameters!.F1Key)
        {
	        HelpMenuRequested?.Invoke(this, EventArgs.Empty);
        }
        else
        {
	        var contentTabItem = GetActiveContentTabItem();
	        var keyboardKey = e.Key.ToCoreKey();
	        contentTabItem!.OnKeyPressed(sender, new KeyboardKeyEventArgs(keyboardKey));
        }
        
        e.Handled = true;
    }

	private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		var contentTabItem = GetActiveContentTabItem();
		var isFakeTabItem = contentTabItem is null;

		if (isFakeTabItem)
		{
			AddContentTabItem();
		}
	}

	private void AddContentTabItem()
	{
		var (contentTabItem, tabItem) = BuildTabItemData();

		var contentTabItemCount = GetContentTabItemCount();
		_tabControl.Items.Insert(contentTabItemCount, tabItem);

		ContentTabItemAdded?.Invoke(this, new ContentTabItemEventArgs(contentTabItem));
		TabCountChanged?.Invoke(this, new TabCountChangedEventArgs(ShouldAllowTabClose()));
	}

	private (IContentTabItem contentTabItem, object tabItem) BuildTabItemData()
	{
		var contentTabItem = new ContentTabItem
		{
			MainView = this,
			GlobalParameters = GlobalParameters,
			FolderChangedEventHandle = FolderChangedEventHandleFactory!.GetFolderChangedEventHandle()
		};

		var contentTabItemHeader = new ContentTabItemHeader
		{
			ContentTabItem = contentTabItem
		};

		contentTabItem.ContentTabItemHeader = contentTabItemHeader;
		contentTabItem.ContentTabItemHeader.TabClosed += CloseContentTabItem;
		contentTabItem.RegisterMainViewEvents();
		contentTabItem.SetTitle(DefaultTabItemTitle);
		
		var tabItem = new TabItem
		{
			FontSize = _windowFontSize,
			Header = contentTabItem.ContentTabItemHeader,
			Content = contentTabItem
		};

		contentTabItem.WrapperTabItem = tabItem;

		return (contentTabItem, tabItem);
	}

	private int GetContentTabItemCount() => _tabControl.Items.Count - 1;
	private bool ShouldAllowTabClose() => GetContentTabItemCount() > 1;

	private IContentTabItem? GetActiveContentTabItem()
    {
	    var tabItem = (TabItem)_tabControl.SelectedItem!;
	    var contentTabItem = tabItem.Content as IContentTabItem;

	    return contentTabItem;
    }

	private void SelectLastTabItem()
	{
		var lastTabItemIndex = GetContentTabItemCount() - 1;
		var lastTabItem = (TabItem)_tabControl.Items[lastTabItemIndex]!;
		lastTabItem.IsSelected = true;
	}

	private void CloseContentTabItem(object? sender, ContentTabItemEventArgs e)
	{
		var contentTabItem = e.ContentTabItem;
		ContentTabItemClosed?.Invoke(this, new ContentTabItemEventArgs(contentTabItem));
		
		var tabItem = contentTabItem.WrapperTabItem;
		_tabControl.Items.Remove(tabItem);
		TabCountChanged?.Invoke(this, new TabCountChangedEventArgs(ShouldAllowTabClose()));
		
		contentTabItem.ContentTabItemHeader!.TabClosed -= CloseContentTabItem;
		contentTabItem.UnregisterMainViewEvents();

		SelectLastTabItem();
	}

	private bool ShouldCloseWindow(
		ImageFanReloaded.Core.Keyboard.Key keyPressed, ImageFanReloaded.Core.Keyboard.KeyModifiers keyModifiers)
		=> keyPressed == GlobalParameters!.EscapeKey || 
		   (keyModifiers == GlobalParameters!.AltKeyModifier && keyPressed == GlobalParameters!.F4Key);

	private void CloseWindow() => Close();

	#endregion
}
