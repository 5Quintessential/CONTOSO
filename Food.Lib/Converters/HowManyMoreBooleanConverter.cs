using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Converters
{
    public class HowManyMoreBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (null != value)
            {
                if (value is int || value is IEnumerable<Lib.Models.Restaurant>)
                {
                    if (value is int)
                        return ((int)value) > 0;
                    else
                        return ((IEnumerable<Lib.Models.Restaurant>)value).Count() > 0;
                }
                else
                {
                    throw new ArgumentException("Value must be of type int or IEnumerable<Lib.Models.Restaurant>!");
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
