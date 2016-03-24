using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.DPE.ReferenceApps.Food.Lib.Helpers;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Models
{
    public class Review : BaseModel
    {
        public Review(Services.Yelp.V1Helper.Review yelpRecord)
        {
            if (yelpRecord == null) throw new ArgumentNullException("yelpRecord");
            YelpRecord = yelpRecord;
        }
        public string Key { get { return YelpRecord.id; } }
        public string ImagePath { get { return YelpRecord.user_photo_url; } }
        public DateTime Date { get { return DateTime.Parse(YelpRecord.date); } }
        public double Rating { get { return YelpRecord.rating; } }
        public string Comment { get { return YelpRecord.text_excerpt; } }
        public string Author { get { return YelpRecord.user_name; } }
        public Services.Yelp.V1Helper.Review YelpRecord { get; set; }
    }
}
