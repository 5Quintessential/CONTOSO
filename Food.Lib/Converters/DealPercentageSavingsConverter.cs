using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Converters
{
    public class DealPercentageSavingsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (null != value)
            {
                if (value is Lib.Services.Yelp.BusinessHelper.Option)
                {
                    Lib.Services.Yelp.BusinessHelper.Option dealOption = value as Lib.Services.Yelp.BusinessHelper.Option;

                    float originalPrice = (dealOption.original_price / 100);
                    float dealPrice = (dealOption.price / 100);
                    float discount = ((originalPrice - dealPrice) / originalPrice) * 100;

                    int precentage = (int)Math.Round(discount, 0);

                    return "%" + precentage;
                }
                else
                {
                    throw new ArgumentException("Value must be of type Lib.Services.Yelp.BusinessHelper.Option!");
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
