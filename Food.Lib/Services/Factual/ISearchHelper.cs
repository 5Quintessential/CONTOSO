using System;
namespace Microsoft.DPE.ReferenceApps.Food.Lib.Services.Factual
{
    interface ISearchHelper
    {
        System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<SearchHelper.Restaurant>> SearchAsync(System.Collections.Generic.IEnumerable<string> factualIds);
        System.Threading.Tasks.Task<SearchHelper.Restaurant> SearchAsync(string factualId);
        System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<SearchHelper.Restaurant>> SearchAsync(string search, double latitude, double longitude, double minRating = 0d, Lib.Models.Cuisines[] cuisine = null, int limit = 25, int miles = 10);
    }
}
