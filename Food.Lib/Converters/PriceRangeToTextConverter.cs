using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DPE.ReferenceApps.Food.Lib.Models;
using Windows.UI.Xaml.Data;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Converters
{

    public class PriceRangeToTextConverter : IValueConverter
    {
        public static string[] PriceRangeText = new string[] {"Unknown", "Up to $15", "$15 - $30", "$30 - $50", "$50 - $75", "$75 and higher" };

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return PriceRangeToTextConverter.PriceRangeText[(int)value];
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

    }
}
