using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Event_Finder.Converters
{
    class LongLatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {   
            return System.Convert.ToDouble(value) * 111.12;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return System.Convert.ToDouble(value) / 111.12; 
        }
    }
}
