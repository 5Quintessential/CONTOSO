using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Converters
{
    public class TitleCaseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (null != value)
            {
                if (value is string)
                {
                    StringBuilder result = new StringBuilder(value.ToString());
                    result[0] = char.ToUpper(result[0]);

                    for (int i = 1; i < result.Length; ++i)
                    {
                        if (char.IsWhiteSpace(result[i - 1]))
                            result[i] = char.ToUpper(result[i]);
                        else
                            result[i] = char.ToLower(result[i]);
                    }

                    return result.ToString();
                }
                else
                {
                    throw new ArgumentException("Value must be of type string!");
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
