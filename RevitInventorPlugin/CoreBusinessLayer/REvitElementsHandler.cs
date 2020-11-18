using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitInventorExchange.CoreDataStructures;
using RevitInventorExchange.Utilities;

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

            elementStructureList.Clear();

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

                if (elemType == null)
                {
                    throw new UIRelevantException(LanguageHandler.GetString("msgBox_SelElNoRevFam"));
                }   
                
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

        //  Retrieve Revit family types in Active document
        public IList<Element> GetRevitFamilyTypesInActiveDocument(RevitFamily famType)
        {
            NLogger.LogText("Entered GetRevitFamiliesInActiveDocument");

            IList<Element> genModElements = null;

            try
            {
                Type RevitFamType = Type.GetType(famType.Type);
                                
                var genModelTypeCollector = new FilteredElementCollector(doc);
                genModelTypeCollector.OfClass(RevitFamType);

                if (!string.IsNullOrEmpty(famType.Category))
                {
                    BuiltInCategory RevitFamilyCategory = (BuiltInCategory)Enum.Parse(typeof(BuiltInCategory), famType.Category, true);
                    genModelTypeCollector.OfCategory(RevitFamilyCategory);
                }
                genModElements = genModelTypeCollector.ToElements();

            }
            catch (Exception ex)
            {
                NLogger.LogError(ex);
                NLogger.LogText("Exit GetRevitFamiliesInActiveDocument with error");

                throw (ex);
            }

            NLogger.LogText($"Extracted {genModElements.Count()} Families from Revit document");

            NLogger.LogText("Exit GetRevitFamiliesInActiveDocument");

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

                    var val = Utility.ParameterToString(doc, param);

                    elementParams.Add(new ElementOrderedParameter { ParameterId = param.Id.IntegerValue, ParameterName = name, ParameterValue = val, TransferParam = transferParam });
                }
            }
            else
            {
                NLogger.LogText("Received Revit element or its corresponding family is null");
            }

            NLogger.LogText("Exit GetParameters");

            return elementParams;
        }

        //  Extract single parameter from element
        private string GetSingleParameter(Element el, BuiltInParameter parameter)
        {
            NLogger.LogText("Entered GetSingleParameter");

            string parVal = "";

            if (el != null)
            {
                var par = el.get_Parameter(parameter);
                parVal = Utility.ParameterToString(doc, par);
            }
            else
            {
                NLogger.LogText("Received Revit element family is null");
            }

            NLogger.LogText("Exit GetSingleParameter");

            return parVal;
        }

        //  Find a list of element with given class, family type and category (optional).
        public IList<Element> FindInstancesOfType(Type targetType, ElementId idType, Nullable<BuiltInCategory> targetCategory = null)
        {
            NLogger.LogText("Entered FindInstancesOfType");

            // narrow down to the elements of the given type and category 

            var collector = new FilteredElementCollector(doc).OfClass(targetType);

            if (targetCategory.HasValue)
            {
                collector.OfCategory(targetCategory.Value);
            }

            // parse the collection for the given family type id using LINQ query here. 
            var elems =
                from element in collector
                where element.get_Parameter(BuiltInParameter.SYMBOL_ID_PARAM).
                      AsElementId().Equals(idType)
                select element;

            NLogger.LogText($"Found {elems.Count()} elements");
            NLogger.LogText("Exit FindInstancesOfType");

            // put the result as a list of element fo accessibility. 
            return elems.ToList();
        }

    }
}
