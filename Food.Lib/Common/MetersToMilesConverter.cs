using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Common
{
    public class MetersToMilesConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return 0d;
            double _Value;
            if (!double.TryParse(value.ToString(), out _Value))
                return 0d;
            var _Kilometer = _Value / 1000;
            return ConvertKilometersToMiles(_Kilometer);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return 0d;
            double _Value;
            if (!double.TryParse(value.ToString(), out _Value))
                return 0d;
            return ConvertMilesToKilometers(_Value);
        }

        public static double ConvertKilometersToMiles(double kilometers)
        {
            return kilometers * 0.621371192;
        }

        public static double ConvertMilesToKilometers(double miles)
        {
            return miles * 1.609344;
        }
    }
}
