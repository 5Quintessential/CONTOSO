using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Microsoft.DPE.ReferenceApps.Food.Client.Common
{
    public class StarRatingConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var _Double = (double)value;
            return _Double / 5d;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
