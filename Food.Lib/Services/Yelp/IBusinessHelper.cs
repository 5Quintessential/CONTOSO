using System;
namespace Microsoft.DPE.ReferenceApps.Food.Lib.Services.Yelp
{
    public interface IBusinessHelper
    {
        System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<BusinessHelper.Business>> SearchAsync(System.Collections.Generic.IEnumerable<string> businessIds);
        System.Threading.Tasks.Task<BusinessHelper.Business> SearchAsync(string businessId);
        System.Threading.Tasks.Task<BusinessHelper.Business> SearchAsync(Uri uri);
    }
}
