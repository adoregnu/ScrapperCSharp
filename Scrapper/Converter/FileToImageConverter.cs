﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Scrapper.Converter
{
    public class FileToImageConverter : IValueConverter
    {
        public static BitmapImage ConvertBitmap(Bitmap bitmap, int width)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Bmp);
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                if (width > 0)
                    image.DecodePixelWidth = width;
                image.CacheOption = BitmapCacheOption.OnLoad;
                ms.Seek(0, SeekOrigin.Begin);
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }

        public object Convert(object value_, Type targetType_,
            object parameter_, CultureInfo culture_)
        {
            try
            {
                using (var bmpTemp = new Bitmap(value_ as string))
                {
                    return ConvertBitmap(bmpTemp,
                        parameter_ != null ? int.Parse(parameter_.ToString()) : 0);
                }
            }
            catch (Exception e)
            {
                Log.Print(e.Message);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
