using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.SampleData
{
    public class MockFactualSearch : Microsoft.DPE.ReferenceApps.Food.Lib.Services.Factual.ISearchHelper
    {
        public async Task<IEnumerable<Microsoft.DPE.ReferenceApps.Food.Lib.Services.Factual.SearchHelper.Restaurant>> SearchAsync(IEnumerable<string> factualIds)
        {
            // use sample data
            return from r in (await Restaurants.LoadAsync()).Where(x => x.FactualRecord != null)
                   where factualIds.Contains(r.FactualRecord.factual_id)
                   select r.FactualRecord;
        }

        public async Task<Microsoft.DPE.ReferenceApps.Food.Lib.Services.Factual.SearchHelper.Restaurant> SearchAsync(string factualId)
        {
            // use sample data
            return (from r in (await Restaurants.LoadAsync()).Where(x => x.FactualRecord != null)
                    where factualId == r.FactualRecord.factual_id
                    select r.FactualRecord).FirstOrDefault();
        }

        public async Task<IEnumerable<Microsoft.DPE.ReferenceApps.Food.Lib.Services.Factual.SearchHelper.Restaurant>> SearchAsync(string search, double latitude, double longitude, double minRating = 0d, Microsoft.DPE.ReferenceApps.Food.Lib.Models.Cuisines[] cuisine = null, int limit = 25, int miles = 10)
        {
            // use sample data
            return from r in (await Restaurants.LoadAsync()).Where(x => x.FactualRecord != null)
                   select r.FactualRecord;
        }

    }
}
