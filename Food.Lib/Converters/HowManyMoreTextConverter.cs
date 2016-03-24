using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Converters
{
    public class HowManyMoreTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int)
            {
                string moreText = "MORE";
                if (null != parameter && parameter is string && !string.IsNullOrWhiteSpace(parameter.ToString()))
                    moreText = parameter.ToString();

                return string.Format("{0} {1}",
                    value,
                    moreText);
            }
            else
            {
                throw new ArgumentException("Value must be of type int!");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
