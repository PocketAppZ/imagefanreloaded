﻿using ImageFanReloaded.CommonTypes.ImageHandling.Interface;
using ImageFanReloaded.CommonTypes.Info;
using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ImageFanReloaded.Controls
{
    public partial class ThumbnailBox
        : UserControl
    {
        public ThumbnailBox(ThumbnailInfo thumbnailInfo)
        {
            if (thumbnailInfo == null)
                throw new ArgumentNullException("thumbnailInfo",
                                                "Thumbnail info cannot be null.");
            
            InitializeComponent();

            SetControlProperties();
            SetThumbnailInfo(thumbnailInfo);
        }

        public event EventHandler<EventArgs> ThumbnailBoxClicked;

        public IImageFile ImageFile { get; private set; }
        public bool IsSelected { get; private set; }

        public void SelectThumbnail()
        {
            _thumbnailBoxBorder.BorderBrush = Brushes.Gray;
            Cursor = Cursors.Hand;
            IsSelected = true;

            BringIntoView();
        }

        public void UnselectThumbnail()
        {
            _thumbnailBoxBorder.BorderBrush = Brushes.LightGray;
            Cursor = Cursors.Arrow;
            IsSelected = false;
        }


        #region Private

        private ThumbnailInfo _thumbnailInfo;

        private void SetControlProperties()
        {
            _thumbnailImage.MaxWidth = GlobalData.ThumbnailSize;
            _thumbnailImage.MaxHeight = GlobalData.ThumbnailSize;
        }

        private void SetThumbnailInfo(ThumbnailInfo thumbnailInfo)
        {
            if (_thumbnailInfo != null)
                _thumbnailInfo.ThumbnailImageChanged -= OnThumbnailImageChanged;

            _thumbnailInfo = thumbnailInfo;
            ImageFile = _thumbnailInfo.ImageFile;

            _thumbnailImage.Source = _thumbnailInfo.ThumbnailImage;
            _thumbnailTextBlock.Text = _thumbnailInfo.ThumbnailText;

            _thumbnailInfo.ThumbnailImageChanged += OnThumbnailImageChanged;
        }

        private void OnThumbnailImageChanged(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() => _thumbnailImage.Source = _thumbnailInfo.ThumbnailImage);
        }

        private void OnMouseClick(object sender, MouseButtonEventArgs e)
        {
            var thumbnailBoxClickedHandler = ThumbnailBoxClicked;
            if (thumbnailBoxClickedHandler != null)
                thumbnailBoxClickedHandler(this, EventArgs.Empty);
        }

        #endregion
    }
}
