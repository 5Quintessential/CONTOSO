using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.Services.Exceptions
{
    public class FoodLibServiceException : Exception
    {
        public FoodLibServiceException(string message) : base(message) { }
        public FoodLibServiceException(Exception e) : base(string.Empty, e) { }
        public string JSON { get; set; }
        public Uri Query { get; set; }
    }
}
