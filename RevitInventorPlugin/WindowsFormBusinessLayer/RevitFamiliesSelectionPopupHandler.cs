using RevitInventorExchange.CoreDataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RevitInventorExchange.Data;

namespace RevitInventorExchange.WindowsFormBusinesslayer
{
    public class RevitFamiliesSelectionPopupHandler : BaseFormHandler
    {
        //  create the datasource for datagrid
        public Dictionary<string, List<RevitFamiliesDataGridSourceData>> GetRevitFamiliesDataGridSource(IList<ElementStructure> elementStructureList)
        { 
            var ret = new Dictionary<string, List<RevitFamiliesDataGridSourceData>>();
            var sourceData = new List<RevitFamiliesDataGridSourceData>();

            var familyTypes = elementStructureList
                .SelectMany(l => l.ElementTypeSingleParameters)
                .Where(o => o.ParameterName == "SYMBOL_FAMILY_AND_TYPE_NAMES_PARAM")
                .Select(k => k.ParameterValue)
                .Distinct()
                .ToList();

            foreach (var el in familyTypes)
            {                
                sourceData.Add(new RevitFamiliesDataGridSourceData { RevitFamily = el });
            }

            ret.Add("RevitFamilies", sourceData);

            return ret;
        }
    }
}
