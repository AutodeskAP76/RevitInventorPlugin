using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitInventorExchange.Data
{
    public class PropertiesDataGridSourceData
    {
        private string propertyName;
        private string propertyValue;
        private Int32 propertyId;

        public string PropertyName { get => propertyName; set => propertyName = value; }
        public string PropertyValue { get => propertyValue; set => propertyValue = value; }
        public int PropertyId { get => propertyId; set => propertyId = value; }
    }
}
