using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DPE.ReferenceApps.Food.Lib.Services.Factual;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.SampleData
{
    public class MockFactualCrosswalk : Services.Factual.ICrosswalkHelper
    {
        public new async Task<IEnumerable<CrosswalkHelper.Result>> SearchOtherAsync(FactualCrosswalkNamespaces crosswalkNamespace, IEnumerable<string> factualIds)
        {
            // use sample data
            if (crosswalkNamespace != FactualCrosswalkNamespaces.Yelp)
                throw new NotImplementedException();
            var _List =
                from r in (await Restaurants.LoadAsync()).Where(x => x.YelpRecord != null && x.FactualRecord != null)
                join f in factualIds on r.FactualRecord.factual_id equals f
                select new CrosswalkHelper.Result { FactualId = f, Key = r.YelpRecord.id };
            return _List;
        }

        public async Task<IEnumerable<CrosswalkHelper.Result>> SearchFactualAsync(IEnumerable<string> namespaceIds, FactualCrosswalkNamespaces crosswalkNamespace)
        {
            // use sample data
            if (crosswalkNamespace != FactualCrosswalkNamespaces.Yelp)
                throw new NotImplementedException();
            var _List =
                from r in (await Restaurants.LoadAsync()).Where(x => x.YelpRecord != null && x.FactualRecord != null)
                join n in namespaceIds on r.YelpRecord.id equals n
                select new CrosswalkHelper.Result { FactualId = r.FactualRecord.factual_id, Key = n };
            return _List;
        }

        public async Task<CrosswalkHelper.Result> SearchOtherAsync(string factualId, FactualCrosswalkNamespaces crosswalkNamespace)
        {
            // use sample data
            if (crosswalkNamespace != FactualCrosswalkNamespaces.Yelp)
                throw new NotImplementedException();
            var _List =
                from r in (await Restaurants.LoadAsync()).Where(x => x.YelpRecord != null && x.FactualRecord != null)
                where r.FactualRecord.factual_id == factualId
                select new CrosswalkHelper.Result { FactualId = factualId, Key = r.YelpRecord.id };
            return _List.FirstOrDefault();
        }

        public async Task<CrosswalkHelper.Result> SearchFactualAsync(string namespaceId, FactualCrosswalkNamespaces crosswalkNamespace)
        {
            // use sample data
            if (crosswalkNamespace != FactualCrosswalkNamespaces.Yelp)
                throw new NotImplementedException();
            var _List =
                from r in (await Restaurants.LoadAsync()).Where(x => x.YelpRecord != null && x.FactualRecord != null)
                where r.YelpRecord.id == namespaceId
                select new CrosswalkHelper.Result { FactualId = r.FactualRecord.factual_id, Key = namespaceId };
            return _List.FirstOrDefault();
        }
    }
}
