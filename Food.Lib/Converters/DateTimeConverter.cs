using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Converters
{
    public class DateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if(null != value) 
            {
                if (value is DateTime || value is TimeSpan)
                {
                    DateTime dt;
                    if (value is TimeSpan)
                    {
                        TimeSpan ts = (TimeSpan)value;
                        dt = new DateTime(ts.Ticks);
                    }
                    else
                    {
                        dt = (DateTime)value;
                    }

                    if (null != parameter && parameter is string)
                        return dt.ToString(parameter.ToString());
                    else
                        return dt.ToString();
                }
                else
                {
                    throw new ArgumentException("Value must be of type DateTime or TimeSpan!");
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
