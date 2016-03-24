using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Converters
{
  public class CusineRawStringToCusineDisplayNameConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, string language)
    {
      string s = value as string;
      return s.Trim().ToLowerInvariant().Split(' ')
               .Select(s1 => s1.Substring(0, 1).ToUpperInvariant() + s1.Substring(1) + ' ')
               .Aggregate((agg, s2) => agg += s2).Trim();
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
      string s = value as string;
      return s.ToLowerInvariant();
    }
  }
}
