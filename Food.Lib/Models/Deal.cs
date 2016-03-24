using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Models
{
    public class Deal : BaseModel
    {
        public Deal(Microsoft.DPE.ReferenceApps.Food.Lib.Services.Yelp.BusinessHelper.Deal yelpRecord)
        {
            this.YelpRecord = yelpRecord;
        }
        public string Title { get { return YelpRecord.title; } }
        public string ImageUrl { get { return YelpRecord.image_url; } }
        public Microsoft.DPE.ReferenceApps.Food.Lib.Services.Yelp.BusinessHelper.Deal YelpRecord { get; private set; }
    }
}
