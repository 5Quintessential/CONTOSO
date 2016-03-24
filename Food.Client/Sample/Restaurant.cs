using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.DPE.ReferenceApps.Food.Client.Sample
{
    public class Restaurant : Lib.Models.IVariableGridItem
    {
        public Restaurant(int index)
            : this()
        {
            this.Name = index.ToString();
            this.Index = index;
        }
        public Restaurant()
        {
            Name = "Restaurant " + Guid.NewGuid().ToString();
            Address = "123 Main Street";
            City = "Denver";
            State = "CO";
            Zip = "80202";
            Phone = "3038164850";
            AverageRating = 3.5d;
            ImagePath = "https://encrypted-tbn2.google.com/images?q=tbn:ANd9GcTAmqrq8N9dGD7XBq1qj9hHOX6CS63UaNIy3o1-QZTCz_FH6BHOZA";
            Distance = 2.3;
            Reviews = new List<Review> { new Review(), new Review(), new Review() };
        }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Phone { get; set; }
        public string ImagePath { get; set; }
        public double Distance { get; set; }
        public double AverageRating { get; set; }
        public List<Review> Reviews { get; set; }

        public int ColSpan { get; set; }
        public int RowSpan { get; set; }
        public int Index { get; set; }
    }

    public class Review
    {
        public Review()
        {
            Author = "Jerry Nixon";
            ImagePath = "http://media.celebrity-pictures.ca/Celebrities/Jane-Seymour/Jane-Seymour-1126183.jpg";
            Rating = 3.5;
            Comment = "Now is the time for all good men to come to theaid of their country. The quick brown fox jumps over the lazy dog.";
            Date = DateTime.Now;
        }
        public string Author { get; set; }
        public string ImagePath { get; set; }
        public double Rating { get; set; }
        public string Comment { get; set; }
        public DateTime Date { get; set; }
    }
}
