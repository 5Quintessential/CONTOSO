using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Converters
{
    public class PriceRangeToDollarsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (null != value)
            {
                if (value is int || value is Lib.Models.PriceRanges)
                {
                    int priceRange = (int)value;
                    StringBuilder dollarBuilder = new StringBuilder();
                    for (int index = 0; index < priceRange; index++)
                        dollarBuilder.Append("$");

                    return dollarBuilder.ToString();
                }
                else
                {
                    throw new ArgumentException("Value must be of type int or Lib.Models.PriceRanges!");
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
