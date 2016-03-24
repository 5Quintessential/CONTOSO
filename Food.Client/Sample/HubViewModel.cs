using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.DPE.ReferenceApps.Food.Client.Sample
{
    public class HubViewModel
    {
        public HubViewModel()
        {
            Location = "Denver, CO";
            TrendingHub = new List<Restaurant>
            {
                new Restaurant(1){ ColSpan = 330, RowSpan = 312, ImagePath="http://t1.gstatic.com/images?q=tbn:ANd9GcSeIu9iC4iYQjyNDXgPnQF7ItBpDbxJRkTzzWbcAAMOCmmvory0" },
                new Restaurant(2){ ColSpan = 200, RowSpan = 124, ImagePath="http://t1.gstatic.com/images?q=tbn:ANd9GcSeIu9iC4iYQjyNDXgPnQF7ItBpDbxJRkTzzWbcAAMOCmmvory0" },
                new Restaurant(3){ ColSpan = 130, RowSpan = 124, ImagePath="http://t1.gstatic.com/images?q=tbn:ANd9GcSeIu9iC4iYQjyNDXgPnQF7ItBpDbxJRkTzzWbcAAMOCmmvory0" },
                new Restaurant(4){ ColSpan = 268, RowSpan = 142, ImagePath="http://t1.gstatic.com/images?q=tbn:ANd9GcSeIu9iC4iYQjyNDXgPnQF7ItBpDbxJRkTzzWbcAAMOCmmvory0" },
                new Restaurant(5){ ColSpan = 268, RowSpan = 100, ImagePath="http://t1.gstatic.com/images?q=tbn:ANd9GcSeIu9iC4iYQjyNDXgPnQF7ItBpDbxJRkTzzWbcAAMOCmmvory0" },
                new Restaurant(6){ ColSpan = 118, RowSpan = 193, ImagePath="http://t1.gstatic.com/images?q=tbn:ANd9GcSeIu9iC4iYQjyNDXgPnQF7ItBpDbxJRkTzzWbcAAMOCmmvory0" },
                new Restaurant(7){ ColSpan = 150, RowSpan = 193, ImagePath="http://t1.gstatic.com/images?q=tbn:ANd9GcSeIu9iC4iYQjyNDXgPnQF7ItBpDbxJRkTzzWbcAAMOCmmvory0" },
            };
            HowManyMoreTrending = 12;
            RecentHub = new List<Restaurant>
            {
                new Restaurant(1){ },
                new Restaurant(2){ },
                new Restaurant(3){ },
                new Restaurant(4){ },
            };
            NearMeHub = new List<Restaurant>
            {
                new Restaurant(1){ },
                new Restaurant(2){ },
                new Restaurant(3){ },
                new Restaurant(4){ },
            };
            FavoritesHub = new List<Restaurant>
            {
                new Restaurant(1){ ColSpan = 228, RowSpan = 189 },
                new Restaurant(2){ ColSpan = 163, RowSpan = 133 },
                new Restaurant(3){ ColSpan = 226, RowSpan = 113 },
                new Restaurant(4){ ColSpan = 183, RowSpan = 133 },
                new Restaurant(5){ ColSpan = 250, RowSpan = 113 },
                new Restaurant(6){ ColSpan = 118, RowSpan = 189 },
                new Restaurant(7){ ColSpan = 130, RowSpan = 129 },
                new Restaurant(8){ ColSpan = 130, RowSpan = 193 },
            };
            HowManyMoreRecent = 12;
            StaticMapUri = "Images/Map.jpg";
        }
        public List<Restaurant> TrendingHub { get; set; }
        public int HowManyMoreTrending { get; set; }
        public List<Restaurant> RecentHub { get; set; }
        public int HowManyMoreRecent { get; set; }
        public List<Restaurant> NearMeHub { get; set; }
        public List<Restaurant> FavoritesHub { get; set; }
        public string Location { get; set; }
        public string StaticMapUri { get; set; }
    }
}
