using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Converters
{
    public class ZeroVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Visibility desiredVisibility = Visibility.Collapsed;

            if (null != parameter)
                Enum.TryParse<Visibility>(parameter.ToString(), out desiredVisibility);

            bool check = false;

            if(null != value) 
            {
                if (value is int)
                    check = (0 == (int)value);
                else if (value is IEnumerable<object>)
                    check = (0 == (value as IEnumerable<object>).Count());
            }

            if (check)
                return desiredVisibility;
            else
                if (Visibility.Visible == desiredVisibility)
                    return Visibility.Collapsed;
                else
                    return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
