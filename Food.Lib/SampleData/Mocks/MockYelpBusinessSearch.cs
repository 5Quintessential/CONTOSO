using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.SampleData
{
    public class MockYelpBusinessSearch : Microsoft.DPE.ReferenceApps.Food.Lib.Services.Yelp.IBusinessHelper
    {
        public async Task<IEnumerable<Services.Yelp.BusinessHelper.Business>> SearchAsync(IEnumerable<string> businessIds)
        {
            return from r in (await Restaurants.LoadAsync()).Where(x => x.YelpBusinessRecord != null)
                   where businessIds.Contains(r.YelpBusinessRecord.id)
                   select r.YelpBusinessRecord;
        }

        public async Task<Services.Yelp.BusinessHelper.Business> SearchAsync(string businessId)
        {
            return (from r in (await Restaurants.LoadAsync()).Where(x => x.YelpBusinessRecord != null)
                    where businessId == r.YelpBusinessRecord.id
                    select r.YelpBusinessRecord).FirstOrDefault();
        }

        public async Task<Services.Yelp.BusinessHelper.Business> SearchAsync(Uri uri)
        {
            return (from r in (await Restaurants.LoadAsync()).Where(x => x.YelpBusinessRecord != null)
                    where uri.ToString().Equals(r.YelpBusinessRecord.url, StringComparison.CurrentCultureIgnoreCase)
                    select r.YelpBusinessRecord).FirstOrDefault();
        }
    }
}
