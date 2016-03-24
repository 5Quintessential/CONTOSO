using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.SampleData
{
    public class FactualRecords: List<Services.Factual.SearchHelper.Restaurant>
    {
        public FactualRecords()
        {
            this.Add(new Services.Factual.SearchHelper.Restaurant { factual_id = "Factual1", YelpId = "Yelp1" });
            this.Add(new Services.Factual.SearchHelper.Restaurant { factual_id = "Factual2", YelpId = "Yelp2" });
            this.Add(new Services.Factual.SearchHelper.Restaurant { factual_id = "Factual3", YelpId = "Yelp3" });
        }
    }
}
