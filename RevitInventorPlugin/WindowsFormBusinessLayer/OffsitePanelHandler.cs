using Autodesk.Revit.DB.Events;
using RevitInventorExchange.Data;
using RevitInventorExchange.CoreBusinessLayer;
using RevitInventorExchange.CoreDataStructures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json.Linq;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using RevitInventorExchange.Utilities;

namespace RevitInventorExchange.WindowsFormBusinesslayer
{
    public class OffsitePanelHandler : BaseFormHandler
    {
        private ExportDataHandler expDataHandler = null;
        private InventorRevitMappingHandler invRevMappingHandler = null;
        private RevitFiltersHandler revFilterHandler = null;
        private RevitElementsHandler revElementHandler = null;
        private DAEventHandlerUtilities daEventHandler;
        private DesignAutomationHandler daHandler;
        private RevitFamiliesStructure revitFamilyStructure = null;

        public DAEventHandlerUtilities DaEventHandler { get => daEventHandler; set => daEventHandler = value; }

        public OffsitePanelHandler(UIApplication uiapp) : base()
        {
            NLogger.LogText("Entered OffsitePanelHandler constructor");

            //  Initialize the Handler which allows writing json files on disk
            expDataHandler = new ExportDataHandler();

            //  Start an Inventor process or attach to an already existing one
            invRevMappingHandler = new InventorRevitMappingHandler();

            //  Initialize a set of internal classes needed to handle Revit elements
            revFilterHandler = new RevitFiltersHandler();
            revElementHandler = new RevitElementsHandler(uiapp);
            revitFamilyStructure = new RevitFamiliesStructure();
            
            //  Initialize hte class handling the interaction with Forge
            daHandler = new DesignAutomationHandler();

            //  Get the event handler used to log on the UI
            daEventHandler = daHandler.DaEventHandler;

            NLogger.LogText("Exit OffsitePanelHandler constructor");
        }

        //  Create the datasource for datagrid, from elementStructurelist, based on fields shown in the UI
        public Dictionary<string, List<ElementsDataGridSourceData>> GetElementsDataGridSource(IList<ElementStructure> elementStructureList)
        {
            NLogger.LogText("Entered GetElementsDataGridSource method");

            var ret = new Dictionary<string, List<ElementsDataGridSourceData>>();
            var sourceData = new List<ElementsDataGridSourceData>();

            foreach (var el in elementStructureList)
            {
                var elName = el.Element.Name;
                var elFamilyType = Utility.GetFamilyType(el); // el.ElementTypeSingleParameters.SingleOrDefault(p => p.ParameterName == "SYMBOL_FAMILY_AND_TYPE_NAMES_PARAM").ParameterValue;

                sourceData.Add(new ElementsDataGridSourceData { ElementId = el.Element.Id.IntegerValue, ElementName = elName, ElementFamilyType = elFamilyType });
            }

            ret.Add("Elements", sourceData);

            NLogger.LogText("Exit GetElementsDataGridSource method");

            return ret;
        }

        internal IList<Element> GetRevitFamilyTypesInActiveDocument(RevitFamily selectedFamily)
        {
            var ret = revElementHandler.GetRevitFamilyTypesInActiveDocument(selectedFamily);
            return ret;
        }       

        /// <summary>
        /// Create json file with exported properties values
        /// </summary>
        /// <param name="elStructureList"></param>
        public void ExportPropertiesValues(string json)
        {
            NLogger.LogText("Entered ExportPropertiesValues method");

            //  Check if folder exists. If not it is created automatically
            var tempFolder = "C:\\Temp\\ElemProperties\\";
            Directory.CreateDirectory(tempFolder);

            var filePath = tempFolder + "\\ElementPropertiesValue.json";

            //  Export data creating an xml file
            expDataHandler.ExportDataToJson(filePath, json);

            NLogger.LogText("Exit ExportPropertiesValues method");
        }

        internal Dictionary<string, List<InvRevMappingDataGridSourceData>> GetInvRevitMappingDataGridSource(string selectedPath)
        {
            NLogger.LogText("Entered GetInvRevitMappingDataGridSource method");

            //  Build DataGrid datasource structure keeping mapping between Inventor and revit files, and related parameters
            var ret = new Dictionary<string, List<InvRevMappingDataGridSourceData>>();
            var sourceData = new List<InvRevMappingDataGridSourceData>();

            var inventorTemplates = invRevMappingHandler.SetInventorTemplateFilesInternalStructure(selectedPath);

            NLogger.LogText("Create datagrid datasource");
            foreach (var el in inventorTemplates)
            {
                sourceData.Add(new InvRevMappingDataGridSourceData { InventorTemplate = el.InventorTemplate });
            }

            ret.Add("InvRevMapping", sourceData);

            NLogger.LogText("Exit GetInvRevitMappingDataGridSource method");

            return ret;
        }

        internal IList<RevitFamily> GetRevitFamiliesList()
        {
            NLogger.LogText("Entered GetRevitFamiliesList");

            //  Load Revit families from configuration file "RevitFamiliesConfig.xml"
            revitFamilyStructure.LoadStructure();

            NLogger.LogText("Entered GetRevitFamiliesList");
            return revitFamilyStructure.RevitFamilies;            
        }

        //  Build the datasource for Revit family types drop down list
        internal IList<ComboBoxRevitFamilyTypesSourceData> GetListFamilyTypeSource(IList<Element> RevitFamTypes, RevitFamily selectedFamily)
        {
            NLogger.LogText("Entered GetListFamilyTypeSource");

            IList<ComboBoxRevitFamilyTypesSourceData> ret = new List<ComboBoxRevitFamilyTypesSourceData>();

            foreach(var el in RevitFamTypes.Distinct())
            {
                string name = "";

                Parameter param = el.get_Parameter(BuiltInParameter.SYMBOL_FAMILY_AND_TYPE_NAMES_PARAM);
                if (param != null)
                {
                    name = param.AsString();
                }

                //  See https://docs.microsoft.com/en-us/dotnet/api/system.enum.parse?view=netcore-3.1
                BuiltInCategory targetCategory = (BuiltInCategory)Enum.Parse(typeof(BuiltInCategory), el.Category.Id.ToString(), true);

                ret.Add(new ComboBoxRevitFamilyTypesSourceData { FamilyTypeName = name, IdType = el.Id, TargetCategory = targetCategory, TargetType = el.GetType(), FamilyTypeInstance = selectedFamily.Instance } );
            }

            NLogger.LogText("Exit GetListFamilyTypeSource");

            return ret;
        }

        internal Dictionary<string, List<InvRevMappingDataGridSourceData>> RefreshInvRevitMappingDataGridSource(string selRevFamily, string invTemplateFileName)
        {
            NLogger.LogText("Entered RefreshInvRevitMappingDataGridSource");

            var ret = new Dictionary<string, List<InvRevMappingDataGridSourceData>>();
            var sourceData = new List<InvRevMappingDataGridSourceData>();

            var inventorTemplates = invRevMappingHandler.UpdateMappingStructure(invTemplateFileName, selRevFamily);

            foreach (var el in inventorTemplates)
            {
                sourceData.Add(new InvRevMappingDataGridSourceData { InventorTemplate = el.InventorTemplate, RevitFamily = el.RevitFamily });
            }

            ret.Add("InvRevMapping", sourceData);

            NLogger.LogText("Exit RefreshInvRevitMappingDataGridSource");

            return ret;
        }

        internal Dictionary<string, List<InvRevParamMappingDataGridSourceData>> GetInvRevitParamsMappingDataGridSource(string invTemplateFileName, string invTemplatePath)
        {
            NLogger.LogText("Entered GetInvRevitParamsMappingDataGridSource");

            var ret = new Dictionary<string, List<InvRevParamMappingDataGridSourceData>>();
            var sourceData = new List<InvRevParamMappingDataGridSourceData>();

            var paramsMapping = invRevMappingHandler.GetParamsMapping(invTemplateFileName, invTemplatePath);

            foreach (var el in paramsMapping)
            {
                sourceData.Add(new InvRevParamMappingDataGridSourceData { InventorParamName = el.InventorParamName, RevitParamName = el.RevitParamName });
            }

            ret.Add("ParamsMapping", sourceData);

            NLogger.LogText("Exit GetInvRevitParamsMappingDataGridSource");

            return ret;
        }

        internal bool CheckMappingConsistency()
        {
            NLogger.LogText("Entered CheckMappingConsistency");

            var consistency = invRevMappingHandler.CheckMappingConsistency();

            NLogger.LogText($"Revit - Inventor mapping consistency: {consistency.ToString()}");

            NLogger.LogText("Entered CheckMappingConsistency");

            return consistency;
        }

        /// <summary>
        /// Updates Inventor Revit Parameters mapping grid
        /// </summary>
        /// <param name="revitFamily"></param>
        /// <param name="inventorTemplate"></param>
        /// <param name="selInvParam"></param>
        /// <param name="selRevFamilyParam"></param>
        /// <returns></returns>
        internal Dictionary<string, List<InvRevParamMappingDataGridSourceData>> RefreshInvRevitParamsMappingDataGridSource(string revitFamily, string inventorTemplate, string selInvParam, string selRevFamilyParam)
        {
            NLogger.LogText("Entered RefreshInvRevitParamsMappingDataGridSource");

            var ret = new Dictionary<string, List<InvRevParamMappingDataGridSourceData>>();
            var sourceData = new List<InvRevParamMappingDataGridSourceData>();

            var paramsMapping = invRevMappingHandler.UpdateParametersMappingStructure(revitFamily, inventorTemplate, selInvParam, selRevFamilyParam);

            foreach (var el in paramsMapping)
            {
                sourceData.Add(new InvRevParamMappingDataGridSourceData { InventorParamName = el.InventorParamName, RevitParamName = el.RevitParamName });
            }

            ret.Add("ParamsMapping", sourceData);

            NLogger.LogText("Exit RefreshInvRevitParamsMappingDataGridSource");

            return ret;
        }

        /// <summary>
        /// Extract Revit properties values from selected elements
        /// </summary>
        /// <param name="elStructureList"></param>
        /// <returns></returns>
        internal string GetRevitPropertiesValues(IList<ElementStructure> elStructureList)
        {
            NLogger.LogText("Entered GetRevitPropertiesValues");

            var paramJson = invRevMappingHandler.ExtractRevitPropertiesValues(elStructureList);

            //  Create json file containing Revid mapped values id "dev mode" and "Save to file" are enabled
            var devEnabled = Convert.ToBoolean(ConfigUtilities.GetDevMode());
            var enabledSave = Convert.ToBoolean(ConfigUtilities.GetSaveParamValuesToFile());

            if (devEnabled && enabledSave)
            {
                ExportPropertiesValues(paramJson.ToString());
            }

            var jsonString = paramJson.ToString();
            
            NLogger.LogText("Exit GetRevitPropertiesValues");

            return jsonString;
        }

        internal async Task RunDesignAutomation(string jsonParams, string invTemplFolder)
        {
            NLogger.LogText("Entered RunDesignAutomation");

            await daHandler.RunDesignAutomationForgeWorkflow(jsonParams, invTemplFolder);

            NLogger.LogText("Exit RunDesignAutomation");
        }

        internal void CloseInventorProcess()
        {
            var InvElHandler = invRevMappingHandler.InvElHandler;
            InvElHandler.CloseInventorProcess();
        }

        public List<ElementStructure> ProcessElements(IList<Element> RevitElements)
        {
            NLogger.LogText("Entered ProcessElements");

            var ret = revElementHandler.ProcessElements(RevitElements);
                
            NLogger.LogText("Exit ProcessElements");

            return ret;
        }

        /// <summary>
        /// this method is a bridge for corresponding method which extracts Revit elements from active document, based on Revit family types seleced by the user
        /// </summary>
        /// <param name="targetType"></param>
        /// <param name="idType"></param>
        /// <param name="targetCategory"></param>
        /// <returns></returns>
        public IList<Element> FindInstancesOfType(Type targetType, ElementId idType, Nullable<BuiltInCategory> targetCategory = null)
        {
            NLogger.LogText("Entered FindInstancesOfType");

            var ret = revElementHandler.FindInstancesOfType(targetType, idType, targetCategory);

            NLogger.LogText("Exit FindInstancesOfType");

            // put the result as a list of element fo accessibility. 
            return ret.ToList();
        }

        /// <summary>
        /// Bridge method for Reset the InventorRevitMappingStructure
        /// </summary>
        internal void ResetRevitInventorMappingInternalStructure()
        {
            invRevMappingHandler.ResetRevitInventorMappingInternalStructure();
        }

        /// <summary>
        /// Given a Revit Family, it return the properties already mapped to corresponding inventor parameters
        /// </summary>
        /// <param name="RevitFamily"></param>
        internal IList<string> GetRevitFamilyParamsAlreadyUsed(string RevitFamily)
        {
            var internalStructRow = invRevMappingHandler.GetRevitInventorInternalStructureByRevitFamily(RevitFamily);

            IList<string> RevitUsedProperties = internalStructRow.ParametersMapping.Select(m => m.RevitParamName).ToList();
            return RevitUsedProperties;
        }
    }
}
