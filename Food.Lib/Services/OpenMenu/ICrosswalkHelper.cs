using System;
namespace Microsoft.DPE.ReferenceApps.Food.Lib.Services.OpenMenu
{
    interface ICrosswalkHelper
    {
        System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<CrosswalkHelper.MultiResult>> SearchAsync(System.Collections.Generic.IEnumerable<string> factualIds);
        System.Threading.Tasks.Task<string> SearchAsync(string factualId);
    }
}
