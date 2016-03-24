using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Models
{
    [DataContract]
    public class BaseModel : Common.BindableBase
    {
        public void RaisePropertyChanged(object obj)
        {
            // raise change on every single property
            var _Properties = obj.GetType().GetRuntimeProperties();
            foreach (var item in _Properties)
                OnPropertyChanged(item.Name);
        }
    }
}
