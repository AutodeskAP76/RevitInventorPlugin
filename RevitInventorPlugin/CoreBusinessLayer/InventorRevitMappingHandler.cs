
using Newtonsoft.Json.Linq;
using RevitInventorExchange.CoreDataStructures;
using RevitInventorExchange.WindowsFormBusinesslayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitInventorExchange.CoreBusinessLayer
{
    public class InventorRevitMappingHandler
    {
        private InventorElementsHandler invElHandler;
        List<InventorRevitMappingStructure> invRevMappingStructList;

        public InventorElementsHandler InvElHandler { get => invElHandler; set => invElHandler = value; }

        public InventorRevitMappingHandler()
        {
            NLogger.LogText("Entered InventorRevitMappingHandler constructor");

            //  Start Inventor process or attach to an already existing one
            InvElHandler = new InventorElementsHandler();
            InvElHandler.StartInventorApplication();

            NLogger.LogText("Exit InventorRevitMappingHandler constructor");
        }

        /// <summary>
        /// Returns the Template files for the specified the location
        /// </summary>
        /// <param name="selectedPath"></param>
        /// <returns></returns>
        public List<InventorRevitMappingStructure> SetInventorTemplateFilesInternalStructure(string selectedPath)
        {
            NLogger.LogText("Entered GetTemplateFiles method");

            //  Retrive Inventor template files
            NLogger.LogText("Retrieve Inventor Template files names");
            var inventorTemplates = InvElHandler.GetInventorTemplates(selectedPath);

            //  Initialize internal mapping structure
            NLogger.LogText("Initialize internal Inventor - Revit mapping structure");
            invRevMappingStructList = new List<InventorRevitMappingStructure>();

            foreach (var templInfo in inventorTemplates)
            {
                invRevMappingStructList.Add(new InventorRevitMappingStructure { InventorTemplate = templInfo.Name, RevitFamily = "null" });
            }

            NLogger.LogText("Exit GetTemplateFiles method");

            return invRevMappingStructList;
        }

        /// <summary>
        /// Return the cached mapped values or retrieve Inventor parameter values from template if cache is empty
        /// </summary>
        /// <param name="invTemplateFileName"></param>
        /// <returns></returns>
        internal List<InventorRevitParameterMappingStructure> GetParamsMapping(string invTemplateFileName, string invTemplatePath)
        {
            NLogger.LogText("Entered GetParamsMapping method");

            var ret = new List<InventorRevitParameterMappingStructure>();
            var selInventor = invRevMappingStructList.FirstOrDefault(o => o.InventorTemplate == invTemplateFileName);
            var paramsMapping = selInventor.ParametersMapping;

            //  Use cached values
            if (paramsMapping != null && paramsMapping.Count > 0)
            {
                ret = paramsMapping;
            }
            else  //  Retrieve values from Inventor file
            {
                var invElements = InvElHandler.LoadInventorTemplateParameters(invTemplateFileName, invTemplatePath);

                var mapping = invElements.Select(o => new InventorRevitParameterMappingStructure { InventorParamName = o.Name, RevitParamName = "" }).ToList();
                selInventor.ParametersMapping = mapping;
                ret = mapping;
            }

            NLogger.LogText("Exit GetParamsMapping method");

            return ret;
        }

        /// <summary>
        /// Set the Revit family for a given Inventor Template
        /// </summary>
        /// <param name="invTemplateFileName"></param>
        /// <param name="selRevFamily"></param>
        /// <returns></returns>
        internal List<InventorRevitMappingStructure> UpdateMappingStructure(string invTemplateFileName, string selRevFamily)
        {
            NLogger.LogText("Entered UpdateMappingStructure method");

            var selInventor = invRevMappingStructList.FirstOrDefault(o => o.InventorTemplate == invTemplateFileName);
            selInventor.RevitFamily = selRevFamily;

            //  Clear Inventor Params - revit prop association as a new Revit Family has been selected
            NLogger.LogText("Clear Inventor Parameters - Revit Properties association");

            foreach (var map in selInventor.ParametersMapping)
            {
                map.RevitParamName = "";
            }

            NLogger.LogText("Exit UpdateMappingStructure method");

            return invRevMappingStructList;
        }

        /// <summary>
        /// Set the Revit family property for a given Inventor Template parameter
        /// </summary>
        /// <param name="revitFamily"></param>
        /// <param name="selInvParam"></param>
        /// <param name="selRevFamilyParam"></param>
        /// <returns></returns>
        internal List<InventorRevitParameterMappingStructure> UpdateParametersMappingStructure(string revitFamily, string inventorTemplate, string selInvParam, string selRevFamilyParam)
        {            
            NLogger.LogText("Entered UpdateParametersMappingStructure method");

            var selInventor = invRevMappingStructList.FirstOrDefault(o => o.RevitFamily == revitFamily && o.InventorTemplate == inventorTemplate);
            var selInventorParams = selInventor.ParametersMapping;
            var l = selInventorParams.FirstOrDefault(p => p.InventorParamName == selInvParam);

            l.RevitParamName = selRevFamilyParam;

            NLogger.LogText("Exit UpdateParametersMappingStructure method");

            return selInventorParams;
        }

        /// <summary>
        /// Returns a json structure containing Revit properties values, based on Revit - Inventor mapping
        /// </summary>
        /// <param name="elStructureList"></param>
        internal JObject ExtractRevitPropertiesValues_ORIGINAL(List<ElementStructure> elStructureList)
        {
            NLogger.LogText("Entered ExtractRevitPropertiesValues method");

            //  Based on Revit prop - Inventor params mapping, extract real values from Revit selected elements
            var revitElementList = elStructureList;

            dynamic paramJson = new JObject();
            paramJson.ILogicParams = new JArray();

            //  Extract currently mapped Revit - Inventor files
            var invRevMapped = invRevMappingStructList.Where(p => !string.IsNullOrEmpty(p.RevitFamily) && p.ParametersMapping != null);

            //  Loop on Revit families in mapping structure
            foreach (var map in invRevMapped)
            {
                var revFamily = map.RevitFamily;
                var invTemplate = map.InventorTemplate;

                dynamic familyJson = new JObject();
                familyJson.RevitFamily = revFamily;
                familyJson.InventorTemplate = invTemplate;
                familyJson.paramsValues = new JObject();

                var elStructFamTypeList = Utilities.GetElementsOnFamilyType(revitElementList, revFamily);
                
                //  Loop on Revit - Inventor params mapping
                foreach (var paramMap in map.ParametersMapping)
                {
                    var revProp = paramMap.RevitParamName;
                    var invParam = paramMap.InventorParamName;

                    if (!string.IsNullOrEmpty(revProp))
                    {
                        foreach (var elStructFamType in elStructFamTypeList)
                        {
                            var elTypePropList = elStructFamType.ElementTypeOrderedParameters;

                            var propValue = elTypePropList.First(o => o.ParameterName == revProp).ParameterValue;

                            JProperty prop = new JProperty(invParam, propValue);
                            familyJson.paramsValues.Add(prop);
                        }
                    }
                }

                paramJson.ILogicParams.Add(familyJson);
            }

            NLogger.LogText("Exit ExtractRevitPropertiesValues method");

            return paramJson;
        }




        /// <summary>
        /// Returns a json structure containing Revit properties values, based on Revit - Inventor mapping
        /// </summary>
        /// <param name="elStructureList"></param>
        internal JObject ExtractRevitPropertiesValues(List<ElementStructure> elStructureList)
        {
            NLogger.LogText("Entered ExtractRevitPropertiesValues method");

            //  Based on Revit prop - Inventor params mapping, extract real values from Revit selected elements
            var revitElementList = elStructureList;

            dynamic paramJson = new JObject();
            paramJson.ILogicParams = new JArray();

            //  Extract currently mapped Revit - Inventor files
            var invRevMapped = invRevMappingStructList.Where(p => !string.IsNullOrEmpty(p.RevitFamily) && p.ParametersMapping != null);

            //  Loop on Revit families in mapping structure
            foreach (var map in invRevMapped)
            {
                var revFamily = map.RevitFamily;
                var invTemplate = map.InventorTemplate;

                dynamic familyJson = new JObject();
                familyJson.RevitFamily = revFamily;
                familyJson.InventorTemplate = invTemplate;
                familyJson.ParametersInfo = new JArray();
              
                //  get all selected Revit elements with current Revit Family type
                var elStructFamTypeList = Utilities.GetElementsOnFamilyType(revitElementList, revFamily);

                //  Loop on Revit - Inventor params mapping                
                foreach (var elStructFamType in elStructFamTypeList)
                {                    
                    dynamic parameterInfo = new JObject();
                    parameterInfo.elementName = "";
                    parameterInfo.paramsValues = new JObject();

                    foreach (var paramMap in map.ParametersMapping)
                    {
                        var revProp = paramMap.RevitParamName;
                        var invParam = paramMap.InventorParamName;

                        //  Extract Revit properties values from filtered Revit selected elements
                        if (!string.IsNullOrEmpty(revProp))
                        {

                            var elTypePropList = elStructFamType.ElementTypeOrderedParameters;

                            var propValue = elTypePropList.First(o => o.ParameterName == revProp).ParameterValue;

                            JProperty prop = new JProperty(invParam, propValue);
                            parameterInfo.paramsValues.Add(prop);
                        }
                    }

                    familyJson.ParametersInfo.Add(parameterInfo);
                }
                
                paramJson.ILogicParams.Add(familyJson);
            }

            NLogger.LogText("Exit ExtractRevitPropertiesValues method");

            return paramJson;
        }

    }
}