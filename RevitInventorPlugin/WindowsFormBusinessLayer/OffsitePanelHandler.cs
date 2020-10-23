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

        public DAEventHandlerUtilities DaEventHandler { get => daEventHandler; set => daEventHandler = value; }

        public OffsitePanelHandler(UIApplication uiapp) : base()
        {
            NLogger.LogText("Entered OffsitePanelHandler constructor");

            //  Initialize the Handler which allows writing json files on disk
            expDataHandler = new ExportDataHandler();

            //  Start an Inventor process or attach to an already existing one
            invRevMappingHandler = new InventorRevitMappingHandler();
            revFilterHandler = new RevitFiltersHandler();
            revElementHandler = new RevitElementsHandler(uiapp);

            daHandler = new DesignAutomationHandler();
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
                var elFamilyType = Utilities.GetFamilyType(el); // el.ElementTypeSingleParameters.SingleOrDefault(p => p.ParameterName == "SYMBOL_FAMILY_AND_TYPE_NAMES_PARAM").ParameterValue;

                sourceData.Add(new ElementsDataGridSourceData { ElementId = el.Element.Id.IntegerValue, ElementName = elName, ElementFamilyType = elFamilyType });
            }

            ret.Add("Elements", sourceData);

            NLogger.LogText("Exit GetElementsDataGridSource method");

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

            NLogger.LogText("Exit GetRevitPropertiesValues");

            return paramJson.ToString();
        }

        internal void RunDesignAutomation(string jsonParams, string invTemplFolder)
        {
            NLogger.LogText("Entered RunDesignAutomation");

            daHandler.RunDesignAutomationForgeWorkflow(jsonParams, invTemplFolder);

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

        public IList<ElementStructure> FilterElements(IList<ElementStructure> RevitElements)
        {
            NLogger.LogText("Entered FilterElements");

            var ret = revFilterHandler.FilterElements(RevitElements);

            NLogger.LogText("Exit FilterElements");

            return ret;
        }
    }
}
