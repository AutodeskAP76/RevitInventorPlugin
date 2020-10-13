using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitInventorExchange.WindowsFormBusinesslayer
{
    public class ElementsDataGridSourceData
    {
        private string elementName;
        private string elementFamilyType;
        private Int32 elementId;

        public string ElementName { get => elementName; set => elementName = value; }
        public string ElementFamilyType { get => elementFamilyType; set => elementFamilyType = value; }
        public int ElementId { get => elementId; set => elementId = value; }
    }
}
