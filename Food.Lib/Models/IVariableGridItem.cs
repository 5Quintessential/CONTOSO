using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Models
{
    public interface IVariableGridItem
    {
        int ColSpan { get; set; }
        int RowSpan { get; set; }
        int Index { get; set; }
    }
}
