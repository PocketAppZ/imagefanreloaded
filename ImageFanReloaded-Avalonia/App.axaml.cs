﻿using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using ImageFanReloaded.CommonTypes.Disc;
using ImageFanReloaded.CommonTypes.Disc.Implementation;
using ImageFanReloaded.CommonTypes.ImageHandling;
using ImageFanReloaded.CommonTypes.ImageHandling.Implementation;
using ImageFanReloaded.Factories;
using ImageFanReloaded.Factories.Implementation;
using ImageFanReloaded.Infrastructure;
using ImageFanReloaded.Infrastructure.Implementation;
using ImageFanReloaded.Presenters;
using ImageFanReloaded.Views;
using ImageFanReloaded.Views.Implementation;

namespace ImageFanReloaded;

public partial class App
    : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        base.OnFrameworkInitializationCompleted();

		IImageFileFactory imageFileFactory = new ImageFileFactory();

		IImageResizeCalculator imageResizeCalculator = new ImageResizeCalculator();
		IImageResizer imageResizer = new ImageResizer(imageResizeCalculator);

		IDiscQueryEngine discQueryEngine =
			new DiscQueryEngine(imageFileFactory, imageResizer);

		var mainWindow = new MainWindow();
		IScreenInformation screenInformation = new ScreenInformation(mainWindow);
		IImageViewFactory imageViewFactory = new ImageViewFactory(screenInformation);
		mainWindow.ImageViewFactory = imageViewFactory;

		var desktop = (IClassicDesktopStyleApplicationLifetime)ApplicationLifetime;
		desktop.MainWindow = mainWindow;

		IVisualActionDispatcher visualActionDispatcher = new VisualActionDispatcher(Dispatcher.UIThread);
		IFolderVisualStateFactory folderVisualStateFactory = new FolderVisualStateFactory();

		new MainPresenter(discQueryEngine, mainWindow, visualActionDispatcher, folderVisualStateFactory);
		mainWindow.Show();
	}
}
