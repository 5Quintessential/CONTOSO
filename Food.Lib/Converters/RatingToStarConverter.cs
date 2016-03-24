using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Converters
{
    public class RatingToStarConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (null != value)
            {
                if (value is double)
                {
                    int rating = (int)Math.Round(((double)value), 0);
                    StringBuilder starBuilder = new StringBuilder();

                    int index = 0;
                    for (index = 0; index < rating; index++)
                        starBuilder.Append("\uE1CF");

                    if (rating < 5)
                        for (index = rating; index < 5; index++)
                            starBuilder.Append("\uE1CE");

                    return starBuilder.ToString();
                }
                else
                {
                    throw new ArgumentException("Value must be of type double!");
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
