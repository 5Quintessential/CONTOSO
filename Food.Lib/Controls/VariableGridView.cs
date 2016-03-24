using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Reflection;
using Microsoft.DPE.ReferenceApps.Food.Lib.Models;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Controls
{
    public class VariableGridView : GridView
    {
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            var _Item = item as IVariableGridItem;
            if (_Item != null)
            {
                element.SetValue(Windows.UI.Xaml.Controls.VariableSizedWrapGrid.ColumnSpanProperty, _Item.ColSpan);
                element.SetValue(Windows.UI.Xaml.Controls.VariableSizedWrapGrid.RowSpanProperty, _Item.RowSpan);
            }
            base.PrepareContainerForItemOverride(element, item);
        }
    }
}
