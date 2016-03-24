using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.SampleData
{
    public class YelpRecords : List<Services.Yelp.V1Helper.Business>
    {
        public YelpRecords()
        {
            this.Add(new Services.Yelp.V1Helper.Business { id = "Yelp1", name = "Name1", address1 = "Address1", city = "City1", state = "State1", zip = "Zip1", phone = "Phone1", avg_rating = 0d, photo_url = "http://www.dhanbadportal.com/wp-content/uploads/2010/09/LA_PIAZA.jpg", reviews = new List<Services.Yelp.V1Helper.Review> { new Services.Yelp.V1Helper.Review { id = "Review1", date = DateTime.Now.ToString(), rating = 4.5d, text_excerpt = "This is the review", user_name = "User1" }, new Services.Yelp.V1Helper.Review { id = "Review2", date = DateTime.Now.ToString(), rating = 3.5d, text_excerpt = "This is the review", user_name = "User2" } }, categories = new List<Services.Yelp.V1Helper.Category> { new Services.Yelp.V1Helper.Category { name = "Category1" }, new Services.Yelp.V1Helper.Category { name = "Category2" } } });
            this.Add(new Services.Yelp.V1Helper.Business { id = "Yelp2", name = "Name2", address1 = "Address2", city = "City2", state = "State2", zip = "Zip2", phone = "Phone2", avg_rating = 0d, photo_url = "http://www.dhanbadportal.com/wp-content/uploads/2010/09/LA_PIAZA.jpg", reviews = new List<Services.Yelp.V1Helper.Review> { new Services.Yelp.V1Helper.Review { id = "Review1", date = DateTime.Now.ToString(), rating = 4.5d, text_excerpt = "This is the review", user_name = "User1" }, new Services.Yelp.V1Helper.Review { id = "Review2", date = DateTime.Now.ToString(), rating = 3.5d, text_excerpt = "This is the review", user_name = "User2" } }, categories = new List<Services.Yelp.V1Helper.Category> { new Services.Yelp.V1Helper.Category { name = "Category1" }, new Services.Yelp.V1Helper.Category { name = "Category2" } } });
            this.Add(new Services.Yelp.V1Helper.Business { id = "Yelp3", name = "Name3", address1 = "Address3", city = "City3", state = "State3", zip = "Zip3", phone = "Phone3", avg_rating = 0d, photo_url = "http://www.dhanbadportal.com/wp-content/uploads/2010/09/LA_PIAZA.jpg", reviews = new List<Services.Yelp.V1Helper.Review> { new Services.Yelp.V1Helper.Review { id = "Review1", date = DateTime.Now.ToString(), rating = 4.5d, text_excerpt = "This is the review", user_name = "User1" }, new Services.Yelp.V1Helper.Review { id = "Review2", date = DateTime.Now.ToString(), rating = 3.5d, text_excerpt = "This is the review", user_name = "User2" } }, categories = new List<Services.Yelp.V1Helper.Category> { new Services.Yelp.V1Helper.Category { name = "Category1" }, new Services.Yelp.V1Helper.Category { name = "Category2" } } });
        }
    }
}
