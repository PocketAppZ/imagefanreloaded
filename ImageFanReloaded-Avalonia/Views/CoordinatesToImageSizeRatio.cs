﻿using Avalonia;
using ImageFanReloaded.CommonTypes.ImageHandling;

namespace ImageFanReloaded.Views;

public record class CoordinatesToImageSizeRatio
{
	static CoordinatesToImageSizeRatio()
	{
		_imageCenter = new CoordinatesToImageSizeRatio(0.5, 0.5);
	}
	
	public CoordinatesToImageSizeRatio(Point coordinates, ImageSize imageSize)
    {
        RatioX = coordinates.X / imageSize.Width;
		RatioY = coordinates.Y / imageSize.Height;
	}

	public CoordinatesToImageSizeRatio(double ratioX, double ratioY)
	{
		RatioX = ratioX;
		RatioY = ratioY;
	}

	public double RatioX { get; }
	public double RatioY { get; }

	public static CoordinatesToImageSizeRatio ImageCenter => _imageCenter;

	#region Private

	private static readonly CoordinatesToImageSizeRatio _imageCenter;

	#endregion
}
