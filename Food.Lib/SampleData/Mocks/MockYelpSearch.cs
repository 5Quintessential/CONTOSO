using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.SampleData
{
    public class MockYelpSearch : Microsoft.DPE.ReferenceApps.Food.Lib.Services.Yelp.IV1Helper
    {
        public async Task<IEnumerable<Microsoft.DPE.ReferenceApps.Food.Lib.Services.Yelp.V1Helper.Business>> SearchAsync(string query, double latitude, double longitude, string category = null, int limit = 20, int radius = 25)
        {
            // use sample data
            return from r in (await Restaurants.LoadAsync()).Where(x => x.YelpRecord != null)
                   select r.YelpRecord;
        }

    }
}
