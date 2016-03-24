using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.DPE.ReferenceApps.Food.Lib.SampleData
{
    public class MenuRecords : List<Services.OpenMenu.MenuHelper.Root>
    {
        public MenuRecords()
        {
            this.Add(new Services.OpenMenu.MenuHelper.Root { OpenmenuId = "Menu1", FactualId = "Factual1" });
            this.Add(new Services.OpenMenu.MenuHelper.Root { OpenmenuId = "Menu2", FactualId = "Factual2" });
            this.Add(new Services.OpenMenu.MenuHelper.Root { OpenmenuId = "Menu3", FactualId = "Factual3" });
        }
    }
}
