using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Converters
{
    public class SearchDistanceMilesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {

            string milesText = null;
            if (value is double)
            {

                int miles = (int)Math.Round((double)value, 0);
                milesText = (1 == miles) ? "mile" : "miles";
                return string.Format("{0} {1}",
                    value,
                    milesText);
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
