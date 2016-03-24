using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Controls
{
    public class FirstItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate FirstDataTemplate { get; set; }
        public DataTemplate OtherDataTemplate { get; set; }
        protected override Windows.UI.Xaml.DataTemplate SelectTemplateCore(object item, Windows.UI.Xaml.DependencyObject container)
        {
            var _Item = item as Lib.Models.IVariableGridItem;
            if (null == _Item)
                return null;
            return (_Item.Index == 1) ? FirstDataTemplate : OtherDataTemplate;
        }
    }
}
