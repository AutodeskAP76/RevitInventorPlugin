
using Inventor;
using Newtonsoft.Json.Linq;
using RevitInventorExchange.CoreDataStructures;
using RevitInventorExchange.Data;
using RevitInventorExchange.WindowsFormBusinesslayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using RevitInventorExchange.Utilities;

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
                var fullPath = invTemplatePath + "\\" + invTemplateFileName;

                //  Check if extension is .iam or .ipt. In this case files are processed straight away
                var extension = System.IO.Path.GetExtension(fullPath);

                if (extension == ".iam" || extension == ".ipt")
                {
                    var invElements = InvElHandler.LoadInventorTemplateParameters(fullPath);

                    var mapping = invElements.Select(o => new InventorRevitParameterMappingStructure { InventorParamName = o.Name, RevitParamName = "" }).ToList();
                    selInventor.ParametersMapping = mapping;
                    ret = mapping;
                }
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

            if (selInventor.ParametersMapping != null)
            {
                foreach (var map in selInventor.ParametersMapping)
                {
                    map.RevitParamName = "";
                }
            }
            else
            {
                NLogger.LogText("'ParametersMapping' is null");
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
        //internal JObject ExtractRevitPropertiesValues_ORIGINAL(List<ElementStructure> elStructureList)
        //{
        //    NLogger.LogText("Entered ExtractRevitPropertiesValues method");

        //    //  Based on Revit prop - Inventor params mapping, extract real values from Revit selected elements
        //    var revitElementList = elStructureList;

        //    dynamic paramJson = new JObject();
        //    paramJson.ILogicParams = new JArray();

        //    //  Extract currently mapped Revit - Inventor files
        //    var invRevMapped = invRevMappingStructList.Where(p => !string.IsNullOrEmpty(p.RevitFamily) && p.ParametersMapping != null);

        //    //  Loop on Revit families in mapping structure
        //    foreach (var map in invRevMapped)
        //    {
        //        var revFamily = map.RevitFamily;
        //        var invTemplate = map.InventorTemplate;

        //        dynamic familyJson = new JObject();
        //        familyJson.RevitFamily = revFamily;
        //        familyJson.InventorTemplate = invTemplate;
        //        familyJson.paramsValues = new JObject();

        //        var elStructFamTypeList = Utility.GetElementsOnFamilyType(revitElementList, revFamily);
                
        //        //  Loop on Revit - Inventor params mapping
        //        foreach (var paramMap in map.ParametersMapping)
        //        {
        //            var revProp = paramMap.RevitParamName;
        //            var invParam = paramMap.InventorParamName;

        //            if (!string.IsNullOrEmpty(revProp))
        //            {
        //                foreach (var elStructFamType in elStructFamTypeList)
        //                {
        //                    var elTypePropList = elStructFamType.ElementTypeOrderedParameters;

        //                    var propValue = elTypePropList.First(o => o.ParameterName == revProp).ParameterValue;

        //                    JProperty prop = new JProperty(invParam, propValue);
        //                    familyJson.paramsValues.Add(prop);
        //                }
        //            }
        //        }

        //        paramJson.ILogicParams.Add(familyJson);
        //    }

        //    NLogger.LogText("Exit ExtractRevitPropertiesValues method");

        //    return paramJson;
        //}

        /// <summary>
        /// Returns a json structure containing Revit properties values, based on Revit - Inventor mapping
        /// </summary>
        /// <param name="elStructureList"></param>
        internal JObject ExtractRevitPropertiesValues(IList<ElementStructure> elStructureList)
        {
            NLogger.LogText("Entered ExtractRevitPropertiesValues method");

            //  Based on Revit prop - Inventor params mapping, extract real values from Revit selected elements
            var revitElementList = elStructureList;

            dynamic paramJson = new JObject();
            paramJson.ILogicParams = new JArray();

            //  Extract currently mapped Revit - Inventor files
            var invRevMapped = invRevMappingStructList.Where(p => 
                !string.IsNullOrEmpty(p.RevitFamily) 
                && p.RevitFamily != "null" 
                && p.ParametersMapping != null 
                && p.ParametersMapping.Any(n => !string.IsNullOrEmpty(n.InventorParamName))
                );

            //  Loop on Revit families in mapping structure
            foreach (var map in invRevMapped)
            {
                var revFamily = map.RevitFamily;
                var invTemplate = GetInventorTemplate(map.InventorTemplate); // map.InventorTemplate;

                dynamic familyJson = new JObject();
                familyJson.RevitFamily = revFamily;
                familyJson.InventorTemplate = invTemplate;
                familyJson.ParametersInfo = new JArray();
              
                //  get all selected Revit elements with current Revit Family type
                var elStructFamTypeList = Utility.GetElementsOnFamilyType(revitElementList, revFamily);

                //  Loop on Revit - Inventor params mapping                
                foreach (var elStructFamType in elStructFamTypeList)
                {                    
                    dynamic parameterInfo = new JObject();
                    parameterInfo.elementName = "";
                    parameterInfo.elementId = elStructFamType.Element.Id.ToString();
                    parameterInfo.paramsValues = new JObject();

                    foreach (var paramMap in map.ParametersMapping)
                    {
                        var revProp = paramMap.RevitParamName;
                        var invParam = paramMap.InventorParamName;

                        //  Extract Revit properties values from filtered Revit selected elements
                        if (!string.IsNullOrEmpty(revProp))
                        {

                            var elTypePropList = elStructFamType.ElementTypeOrderedParameters;

                            var propValue = elTypePropList.FirstOrDefault(o => o.ParameterName == revProp)?.ParameterValue;

                            if (propValue == null)
                            {
                                elTypePropList = elStructFamType.ElementOrderedParameters;

                                propValue = elTypePropList.FirstOrDefault(o => o.ParameterName == revProp)?.ParameterValue;
                            }

                            JProperty prop = new JProperty(invParam, propValue);
                            parameterInfo.paramsValues.Add(prop);
                        }
                    }

                    JProperty prop1 = new JProperty("Generate_Drawing", "True");
                    parameterInfo.paramsValues.Add(prop1);

                    familyJson.ParametersInfo.Add(parameterInfo);
                }
                
                paramJson.ILogicParams.Add(familyJson);
            }

            NLogger.LogText("Exit ExtractRevitPropertiesValues method");

            return paramJson;
        }

        //  This method checks following:
        //  - if file is an assembly and exists in the same folder a corresponding zip file with same name, then it passes the zip file as input file
        //  - if file is an assembly and does NOT exists in the same folder then it passes the assembly file as input file
        //  - if file is a part then it passes the part file as input file
        private string GetInventorTemplate(string inventorTemplate)
        {
            NLogger.LogText("Entered GetInventorTemplate method");

            var retInventorTemplate = inventorTemplate;

            var templateName = System.IO.Path.GetFileNameWithoutExtension(inventorTemplate);
            var templateExtension = System.IO.Path.GetExtension(inventorTemplate);
            var templateZip = templateName + ".zip";

            NLogger.LogText($"In Inventor template: {inventorTemplate}");

            if (templateExtension == ".iam")
            {
                var existZipFile = invRevMappingStructList.Exists(p => p.InventorTemplate == templateZip);

                if(existZipFile)
                {
                    NLogger.LogText($"A corresponding zip package: {templateZip} for the assembly input file has been found");

                    retInventorTemplate = retInventorTemplate.Replace("iam", "zip");
                }
                else
                {
                    NLogger.LogText($"A corresponding zip package for the assembly input file has not been found");
                }
            }

            NLogger.LogText($"Out Inventor template: {retInventorTemplate}");

            NLogger.LogText("Exit GetInventorTemplate method");

            return retInventorTemplate;
        }

        internal bool CheckMappingConsistency()
        {
            var consistency = false;
            
            var mappingCount = invRevMappingStructList?.Count();

            //  No Revit - Inventor mappings
            if (invRevMappingStructList == null || !(mappingCount > 0))
            {
                return consistency;
            }

            //  Check if at least one Inventor param - Revit prop mapping has been done

            var maps = new List<InventorRevitMappingStructure>();

            foreach(var map in invRevMappingStructList)
            {
                if (map.ParametersMapping != null)
                {
                    maps.Add(map);
                }
            }    
            var paramsMapping = maps.SelectMany(m => m.ParametersMapping).ToList();
            consistency = paramsMapping.Any(j => !string.IsNullOrEmpty(j.RevitParamName));

            return consistency;
        }

        /// <summary>
        /// Reset the InventorRevitMappingStructure
        /// </summary>
        internal void ResetRevitInventorMappingInternalStructure()
        {
            NLogger.LogText("Enter ResetRevitInventorMappingInternalStructure");

            invRevMappingStructList = new List<InventorRevitMappingStructure>();

            NLogger.LogText("Exit ResetRevitInventorMappingInternalStructure");
        }

        internal InventorRevitMappingStructure GetRevitInventorInternalStructureByRevitFamily(string RevitFamily)
        {
            return invRevMappingStructList.FirstOrDefault(k => k.RevitFamily == RevitFamily);
        }
    }
}