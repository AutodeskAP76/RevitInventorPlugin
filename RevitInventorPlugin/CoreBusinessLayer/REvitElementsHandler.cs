using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitInventorExchange.CoreDataStructures;

namespace RevitInventorExchange.CoreBusinessLayer
{
    public class RevitElementsHandler
    {
        List<ElementStructure> elementStructureList;
        readonly UIApplication uiApplication;
        readonly Document doc;
        RevitFiltersHandler revitFilterHandler;

        public RevitElementsHandler(UIApplication uiapp)
        {
            uiApplication = uiapp;
            elementStructureList = new List<ElementStructure>();
            doc = uiapp.ActiveUIDocument.Document;
            revitFilterHandler = new RevitFiltersHandler();
        }

        //  Extract all relevant information from selected elements and create an internal structure for passing parameters around
        public List<ElementStructure> ProcessElements(IList<Element> RevitElements)
        {
            NLogger.LogText("Entered ProcessElements");

            //  Extract information from received Revit elements
            foreach (var elem in RevitElements)
            {
                var elementOrderedParamsList = new List<ElementOrderedParameter>();
                var elementTypeOrderedParamsList = new List<ElementOrderedParameter>();
                var elementTypeSingleParamsList = new List<ElementOrderedParameter>();

                elementOrderedParamsList = GetParameters(elem);                               

                //  Extract the corresponding ElementType
                var elemTypeId = elem.GetTypeId();
                ElementType elemType = (ElementType)doc.GetElement(elemTypeId);

                elementTypeOrderedParamsList = GetParameters(elemType);
                elementTypeSingleParamsList.Add(new ElementOrderedParameter
                {
                    ParameterName = BuiltInParameter.SYMBOL_FAMILY_AND_TYPE_NAMES_PARAM.ToString(),
                    ParameterValue = GetSingleParameter(elemType, BuiltInParameter.SYMBOL_FAMILY_AND_TYPE_NAMES_PARAM)
                });              

                elementStructureList.Add(new ElementStructure { 
                    Element = elem, 
                    ElementType = elemType, 
                    ElementOrderedParameters = elementOrderedParamsList,
                    ElementTypeOrderedParameters = elementTypeOrderedParamsList,
                    ElementTypeSingleParameters = elementTypeSingleParamsList
                });
            }

            NLogger.LogText("Exit ProcessElements");

            return elementStructureList;
        }

        public IList<Element> GetRevitFamiliesInActivedocument()
        {
            var genModelTypeCollector = new FilteredElementCollector(doc);
            genModelTypeCollector.OfClass(typeof(FamilySymbol));
            genModelTypeCollector.OfCategory(BuiltInCategory.OST_GenericModel);

            var genModElements = genModelTypeCollector.ToElements();

            return genModElements;
        }

        //  extract parameters from element
        private List<ElementOrderedParameter> GetParameters(Element el)
        {
            NLogger.LogText("Entered GetParameters");

            var elementParams = new List<ElementOrderedParameter>();

            if (el != null)
            {
                var elParams = el.GetOrderedParameters();

                //  Extract all parameters for each element
                foreach (Parameter param in elParams)
                {

                    string name = param.Definition.Name;
                    bool transferParam = false;

                    //  Flag the parameter to be transferred to Inventor or not, based on a filtering file

                    //  TODO: MOVE FILTER OUTSIDE

                    //if (revitFilterHandler.checkValue(name))
                    //{
                    //    transferParam = true;
                    //}

                    var val = Utilities.ParameterToString(doc, param);

                    elementParams.Add(new ElementOrderedParameter { ParameterId = param.Id.IntegerValue, ParameterName = name, ParameterValue = val, TransferParam = transferParam });
                }
            }
            else
            {
                NLogger.LogText("Received Revit element is null");
            }

            NLogger.LogText("Exit GetParameters");

            return elementParams;
        }

        //  Extract single parameter from element
        private string GetSingleParameter(Element el, BuiltInParameter parameter)
        {
            NLogger.LogText("Entered GetSingleParameter");

            var par = el.get_Parameter(parameter);
            var parVal = Utilities.ParameterToString(doc, par);

            NLogger.LogText("Exit GetSingleParameter");

            return parVal;
        }



    }
}
