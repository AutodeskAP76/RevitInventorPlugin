using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitInventorExchange.WindowsFormBusinesslayer
{
    public class InvRevMappingDataGridSourceData
    {
        private string inventorTemplate;
        private string revitFamily;      

        public string InventorTemplate { get => inventorTemplate; set => inventorTemplate = value; }
        public string RevitFamily { get => revitFamily; set => revitFamily = value; }
        
    }

    public class InvRevParamMappingDataGridSourceData
    {
        private string revitParamName;
        private string inventorParamName;

        public string RevitParamName { get => revitParamName; set => revitParamName = value; }
        public string InventorParamName { get => inventorParamName; set => inventorParamName = value; }
    }
}
