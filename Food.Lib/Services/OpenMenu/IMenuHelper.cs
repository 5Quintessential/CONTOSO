using System;
namespace Microsoft.DPE.ReferenceApps.Food.Lib.Services.OpenMenu
{
    interface IMenuHelper
    {
        System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<MenuHelper.Root>> SearchAsync(System.Collections.Generic.IEnumerable<string> openMenuIds);
        System.Threading.Tasks.Task<MenuHelper.Root> SearchAsync(string openMenuId);
    }
}
