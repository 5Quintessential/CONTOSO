using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Converters
{
    public class BackgroundBrushConverter : IValueConverter
    {
        private int count = 0;
        private int maxCount = 10;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (null != value && value is Page)
            {
                Page page = value as Page;
                count++;
                if (null != parameter && int.TryParse(parameter.ToString(), out maxCount))
                    if (count > maxCount)
                        count = 1;

                string bgColorKey = string.Format("BGBrush{0}", count);

                if (page.Resources.ContainsKey(bgColorKey) && page.Resources[bgColorKey] is Brush)
                    return page.Resources[bgColorKey];
            }

            return new SolidColorBrush(Color.FromArgb(255,255,255,255));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return !(value is bool && (bool)value);
        }
    }
}
