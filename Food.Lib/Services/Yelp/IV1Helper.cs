using System;
using System.Collections.Generic;
namespace Microsoft.DPE.ReferenceApps.Food.Lib.Services.Yelp
{
    interface IV1Helper
    {
        System.Threading.Tasks.Task<IEnumerable<V1Helper.Business>> SearchAsync(string query, double latitude, double longitude, string category = null, int limit = 20, int radius = 25);
    }
}
