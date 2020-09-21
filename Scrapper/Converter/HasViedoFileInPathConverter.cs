using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

using Scrapper.Model;
namespace Scrapper.Converter
{
    class HasViedoFileInPathConverter : IValueConverter
    {
        public object Convert(object value_, Type targetType_,
            object parameter_, CultureInfo culture_)
        {
            string path = value_ as string;
            var attr = File.GetAttributes(path);
            if (!attr.HasFlag(FileAttributes.Directory))
            {
                if (MediaItem.VideoExts.Any(s => path.EndsWith(s,
                    StringComparison.CurrentCultureIgnoreCase)))
                    return true;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
