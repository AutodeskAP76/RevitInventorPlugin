using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitInventorExchange.CoreDataStructures
{
    /// <summary>
    /// The class contains the mapping between Inventor Templates and revit Families
    /// </summary>
    public class InventorRevitMappingStructure
    {
        private string inventorTemplate;
        private string revitFamily;
        private List<InventorRevitParameterMappingStructure> parametersMapping;

        public string InventorTemplate { get => inventorTemplate; set => inventorTemplate = value; }
        public string RevitFamily { get => revitFamily; set => revitFamily = value; }
        public List<InventorRevitParameterMappingStructure> ParametersMapping { get => parametersMapping; set => parametersMapping = value; }
    }

    /// <summary>
    /// The class contains themapping between Inventor template Parameters and REvit Families properties
    /// </summary>
    public class InventorRevitParameterMappingStructure
    {
        private string revitParamName;
        private string inventorParamName;

        public string RevitParamName { get => revitParamName; set => revitParamName = value; }
        public string InventorParamName { get => inventorParamName; set => inventorParamName = value; }
    }


}
