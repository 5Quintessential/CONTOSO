using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DPE.ReferenceApps.Food.Lib.Services.OpenMenu;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.SampleData
{
    public class MockOpenmenuSearch : Services.OpenMenu.IMenuHelper
    {
        public async System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<MenuHelper.Root>> SearchAsync(System.Collections.Generic.IEnumerable<string> openMenuIds)
        {
            return from r in (await Restaurants.LoadAsync()).Where(x => x.OpenmenuRecord != null)
                   where openMenuIds.Contains(r.OpenmenuRecord.OpenmenuId)
                   select r.OpenmenuRecord;
        }

        public async System.Threading.Tasks.Task<MenuHelper.Root> SearchAsync(string openMenuId)
        {
            return (from r in (await Restaurants.LoadAsync()).Where(x => x.OpenmenuRecord != null)
                   where openMenuId == r.OpenmenuRecord.OpenmenuId
                   select r.OpenmenuRecord).FirstOrDefault();
        }
    }
}
