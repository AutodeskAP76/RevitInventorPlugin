using RevitInventorExchange.Data;
using RevitInventorExchange.CoreBusinessLayer;
using RevitInventorExchange.CoreDataStructures;
using System.Collections.Generic;
using System.Linq;

namespace RevitInventorExchange.WindowsFormBusinesslayer
{
    public class PropertiesCollectorFormHandler : BaseFormHandler
    {
        

        public PropertiesCollectorFormHandler() : base()
        {
            
        }

        //  create the datasource for Grid
        public Dictionary<string, List<PropertiesDataGridSourceData>> GetParametersDataGridSource(ElementStructure el, bool checkEnabled)
        {
            var ret = new Dictionary<string, List<PropertiesDataGridSourceData>>();
            var filteredElements = new List<ElementOrderedParameter>();
            var filteredElementsTypes = new List<ElementOrderedParameter>();

            //  Add properties to datasource based on filter
            if (checkEnabled)
            {
                filteredElements = el.ElementOrderedParameters.Where(l => l.TransferParam == true).ToList();
                filteredElementsTypes = el.ElementTypeOrderedParameters.Where(l => l.TransferParam == true).ToList();
            }
            else
            {
                filteredElements = el.ElementOrderedParameters;
                filteredElementsTypes = el.ElementTypeOrderedParameters;
            }

            var elementParamList = ShowParameters(filteredElements);
            var elementTypeParamList = ShowParameters(filteredElementsTypes);

            ret.Add("ElementParameters", elementParamList);
            ret.Add("ElementTypeParameters", elementTypeParamList);

            return ret;
        }

        /// <summary>
        /// Show the parameter values of an element.
        /// </summary>
        public List<PropertiesDataGridSourceData> ShowParameters(List<ElementOrderedParameter> elOrdPar)
        {
            var paramList = new List<PropertiesDataGridSourceData>();

            foreach (var param in elOrdPar)
            {
                paramList.Add(new PropertiesDataGridSourceData { PropertyId = param.ParameterId, PropertyName = param.ParameterName, PropertyValue = param.ParameterValue });
            }

            return paramList;
        }                  
    }
}
