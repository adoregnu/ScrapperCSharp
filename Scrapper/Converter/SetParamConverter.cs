using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

using Scrapper.Model;
using Scrapper.ViewModel.MediaPlayer;

namespace Scrapper.Converter
{
    class SetParamConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        { 
			if (values == null)
				return Binding.DoNothing;

            PlayerViewModel vm = values[0] as PlayerViewModel;
            //vm.SetMediaItem(values[1] as MediaItem);
            return vm.MediaPlayer;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        { 
			throw new NotImplementedException();
        }
    }

    class MultiParamConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Clone();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        { 
			throw new NotImplementedException();
        }

    }

}
