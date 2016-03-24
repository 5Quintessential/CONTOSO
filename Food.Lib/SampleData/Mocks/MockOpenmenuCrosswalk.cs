using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DPE.ReferenceApps.Food.Lib.Services.OpenMenu;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.SampleData
{
    public class MockOpenmenuCrosswalk : Services.OpenMenu.ICrosswalkHelper
    {
        public async System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<CrosswalkHelper.MultiResult>> SearchAsync(System.Collections.Generic.IEnumerable<string> factualIds)
        {
            return from r in (await Restaurants.LoadAsync()).Where(x => x.FactualRecord != null && x.OpenmenuRecord != null)
                   join f in factualIds on r.FactualRecord.factual_id equals f
                   select new CrosswalkHelper.MultiResult { FactualId = f, OpenMenuId = r.OpenmenuRecord.OpenmenuId };
        }

        public async System.Threading.Tasks.Task<string> SearchAsync(string factualId)
        {
            return (from r in (await Restaurants.LoadAsync()).Where(x => x.FactualRecord != null && x.OpenmenuRecord != null)
                   where factualId == r.FactualRecord.factual_id
                   select r.OpenmenuRecord.OpenmenuId).FirstOrDefault();
        }
    }
}
