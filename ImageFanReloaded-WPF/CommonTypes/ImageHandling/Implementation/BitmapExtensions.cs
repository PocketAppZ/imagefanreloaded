﻿using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageFanReloaded.CommonTypes.ImageHandling.Implementation
{
    public static class BitmapExtensions
    {
        public static ImageSource ConvertToImageSource(this Bitmap inputImage)
        {
            IntPtr hBitmap = IntPtr.Zero;

            try
            {
                hBitmap = inputImage.GetHbitmap();
                
                return Imaging.CreateBitmapSourceFromHBitmap(
                             hBitmap,
                             IntPtr.Zero,
                             Int32Rect.Empty,
                             BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                if (hBitmap != IntPtr.Zero)
                {
                    DeleteObject(hBitmap);
                }
            }
        }

        #region Private

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        #endregion
    }
}
