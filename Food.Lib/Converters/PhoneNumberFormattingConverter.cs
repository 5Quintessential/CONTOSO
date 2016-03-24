using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Converters
{
    public class PhoneNumberFormattingConverter : IValueConverter
    {
        Regex pnPattern = new Regex("[0-9]{10}");

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (null != value)
            {
                if (value is string)
                {
                    string val = value.ToString();
                    if(pnPattern.IsMatch(val))
                    {
                        return string.Format("({0}) {1}-{2}",
                            val.Substring(0, 3),
                            val.Substring(3, 3),
                            val.Substring(5, 4));
                    }
                }
                else
                {                    
                    throw new ArgumentException("Value must be of string!");
                }
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
