using System;
namespace Microsoft.DPE.ReferenceApps.Food.Lib.Services.Factual
{
    public interface ICrosswalkHelper
    {
        System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<CrosswalkHelper.Result>> SearchFactualAsync(System.Collections.Generic.IEnumerable<string> namespaceIds, FactualCrosswalkNamespaces crosswalkNamespace);
        System.Threading.Tasks.Task<CrosswalkHelper.Result> SearchFactualAsync(string namespaceId, FactualCrosswalkNamespaces crosswalkNamespace);
        System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<CrosswalkHelper.Result>> SearchOtherAsync(FactualCrosswalkNamespaces crosswalkNamespace, System.Collections.Generic.IEnumerable<string> factualIds);
        System.Threading.Tasks.Task<CrosswalkHelper.Result> SearchOtherAsync(string factualId, FactualCrosswalkNamespaces crosswalkNamespace);
    }
}
