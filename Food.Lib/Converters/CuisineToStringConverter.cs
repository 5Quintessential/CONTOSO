using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Converters
{
    public class CuisineToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (null != value)
            {
                if (value is Lib.Models.Cuisines)
                {
                    string cuisine = ((Lib.Models.Cuisines)value).ToString().Replace("_", " ");

                    if (null != parameter && bool.TrueString.ToLowerInvariant() == parameter.ToString().ToLowerInvariant())
                        cuisine = cuisine.ToUpperInvariant();

                    return cuisine;
                }
                else
                {
                    throw new ArgumentException("Value must be of type int or Lib.Models.Cuisines!");
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
