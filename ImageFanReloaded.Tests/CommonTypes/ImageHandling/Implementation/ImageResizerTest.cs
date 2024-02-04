using FluentAssertions;
using NSubstitute;
using Xunit;
using Avalonia.Media.Imaging;
using ImageFanReloaded.CommonTypes.ImageHandling;
using ImageFanReloaded.CommonTypes.ImageHandling.Implementation;

namespace ImageFanReloaded.Tests.CommonTypes.ImageHandling.Implementation;

public class ImageResizerTest : TestBase
{
	public ImageResizerTest()
    {
		_imageResizeCalculator = Substitute.For<IImageResizeCalculator>();

        _imageResizer = new ImageResizer(_imageResizeCalculator);
    }

	[Theory]
	[InlineData(3840, 2160, 1920, 1080)]
	[InlineData(1920, 1080, 1920, 1080)]
	[InlineData(1280, 720, 1280, 720)]
	[InlineData(960, 540, 960, 540)]
	public void CreateResizedImage_LandscapeImage_ReturnsCorrectlyResizedImage(
		int viewPortWidth, int viewPortHeight, int resizedImageWidth, int resizedImageHeight)
	{
		// Arrange
		var image = new Bitmap($"{LandscapeImageFileName}{InputFileExtension}");

		var viewPortSize = new ImageSize(viewPortWidth, viewPortHeight);
		var referenceResizedImageSize = new ImageSize(resizedImageWidth, resizedImageHeight);

		_imageResizeCalculator
			.GetResizedImageSize(
				new ImageSize(image.Size.Width, image.Size.Height), viewPortSize)
			.Returns(referenceResizedImageSize);

		// Act
		var resizedImage = _imageResizer.CreateResizedImage(image, viewPortSize);

		// Assert
		resizedImage.Size.Width.Should().Be(referenceResizedImageSize.Width);
		resizedImage.Size.Height.Should().Be(referenceResizedImageSize.Height);

		var outputFileName = GetOutputImageFileName(
			LandscapeImageFileName, viewPortSize, referenceResizedImageSize);
		SaveImageToDisc(resizedImage, outputFileName);
	}

	[Theory]
	[InlineData(3840, 2160, 1080, 1920)]
	[InlineData(1920, 1080, 607, 1080)]
	[InlineData(1280, 720, 405, 720)]
	[InlineData(960, 540, 303, 540)]
	public void CreateResizedImage_PortraitImage_ReturnsCorrectlyResizedImage(
		int viewPortWidth, int viewPortHeight, int resizedImageWidth, int resizedImageHeight)
	{
		// Arrange
		var image = new Bitmap($"{PortraitImageFileName}{InputFileExtension}");

		var viewPortSize = new ImageSize(viewPortWidth, viewPortHeight);
		var referenceResizedImageSize = new ImageSize(resizedImageWidth, resizedImageHeight);

		_imageResizeCalculator
			.GetResizedImageSize(
				new ImageSize(image.Size.Width, image.Size.Height), viewPortSize)
			.Returns(referenceResizedImageSize);

		// Act
		var resizedImage = _imageResizer.CreateResizedImage(image, viewPortSize);

		// Assert
		resizedImage.Size.Width.Should().Be(referenceResizedImageSize.Width);
		resizedImage.Size.Height.Should().Be(referenceResizedImageSize.Height);

		var outputFileName = GetOutputImageFileName(
			PortraitImageFileName, viewPortSize, referenceResizedImageSize);
		SaveImageToDisc(resizedImage, outputFileName);
	}

	#region Private

	private const string LandscapeImageFileName = "Landscape";
	private const string PortraitImageFileName = "Portrait";

	private const string InputFileExtension = ".jpg";

	private readonly IImageResizeCalculator _imageResizeCalculator;

	private readonly ImageResizer _imageResizer;

	private static string GetOutputImageFileName(
		string imageFileName, ImageSize viewPortSize, ImageSize resizedImageSize)
		=> $"{imageFileName}-ViewPort-{viewPortSize.Width}x{viewPortSize.Height}-Resized-{resizedImageSize.Width}x{resizedImageSize.Height}{OutputFileExtension}";

    #endregion
}
