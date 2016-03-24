using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Converters
{
    public class DoubleRoundupConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (null != value)
            {
                if (value is double)
                {
                    return Math.Round(((double)value), (null != parameter && parameter is int) ? (int)parameter : 1);
                }
                else
                {
                    throw new ArgumentException("Value must be of type double!");
                }
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

