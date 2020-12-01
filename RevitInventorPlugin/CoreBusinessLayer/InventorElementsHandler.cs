using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inventor;
using System.Runtime.InteropServices;
using System.IO;
using RevitInventorExchange.CoreDataStructures;

namespace RevitInventorExchange.CoreBusinessLayer
{
    public class InventorElementsHandler
    {
        private Inventor.Application m_InventorApplication;
        private Inventor.AssemblyDocument m_AssemblyDocument;
        private Inventor.PartDocument m_PartDocument;
        private bool started = false;

        //  Load or attach to Inventor Process
        public /*async Task*/ void StartInventorApplication()
        {
            NLogger.LogText("Entered StartInventorApplication");

            try
            {
                try
                {
                    NLogger.LogText("StartInventorApplication - Try to attach to an active Inventor process");

                    // Get active inventor object
                    //await Task.Run(() =>
                    //{
                        m_InventorApplication = Marshal.GetActiveObject("Inventor.Application") as Inventor.Application;
                    //});

                    NLogger.LogText("StartInventorApplication - Attached sucessfully to an active Inventor process");
                }
                catch (COMException ex)
                {
                    NLogger.LogText("StartInventorApplication - Error in attaching to the Inventor process");

                    NLogger.LogError(ex);

                    try
                    {
                        NLogger.LogText("StartInventorApplication - Try to activate an Inventor process");

                        //  If not, start a new instance
                        Type invAppType = Type.GetTypeFromProgID("Inventor.Application");

                        //await Task.Run(() =>
                        //{
                            m_InventorApplication = (Application)Activator.CreateInstance(invAppType);
                            m_InventorApplication.Visible = false;
                            started = true;
                        //});

                        NLogger.LogText("StartInventorApplication - Inventor process activated sucessfully");

                    }
                    catch (Exception ex2)
                    {
                        NLogger.LogText("StartInventorApplication - Error in activating the Inventor process");

                        NLogger.LogError(ex2);
                    }
                }
            }
            catch (Exception e)
            {
                NLogger.LogError(e);
            }
        }

        //  Load list of Inventor Templates from user selected path, applying filter on files extension
        public List<FileInfo> GetInventorTemplates(string path)
        {
            NLogger.LogText("Entered GetInventorTemplates method with path {path}", path);

            List<FileInfo> inventorTemplates = null;
            List<string> fileExtensions = new List<string> { ".iam", ".ipt", ".zip" };

            try
            {
                var g = Directory.EnumerateFiles(path).Select(p => new FileInfo(p));

                NLogger.LogText("Retrieved Inventor files {InventorFiles}", g.ToList().Count.ToString());

                inventorTemplates = g.Where(j => fileExtensions.Contains(j.Extension)).ToList();

                NLogger.LogText("Filtered Inventor files {InventorFiles}", inventorTemplates.Count.ToString());

                NLogger.LogText("Exit GetInventorTemplates method");                
            }
            catch(Exception ex)
            {
                NLogger.LogError($"Following error occurred in GetInventorTemplates: {ex}");
                throw new UIRelevantException(LanguageHandler.GetString("msgBox_BIM360NotSync"));
            }

            return inventorTemplates;
        }

        //  Load template file and extract related parameters
        public List<InventorParameterStructure> LoadInventorTemplateParameters(string fullPath)
        {
            NLogger.LogText("Entered LoadInventorTemplateParameters method");

            var inventorParams = new List<InventorParameterStructure>();
            Parameters InventorParams = null;

            //  TODO: HANDLE THE HARDCODED PATH
            //var rootPath = Utilities.GetBIM360RootPath();
            //var fullPath = rootPath + @"ATDSK ​DEV​\Sample Project\Project Files\Libraries\" + templateName;
           
            try
            {
                NLogger.LogText($"Try opening assembly document {fullPath}");

                m_AssemblyDocument = (AssemblyDocument)m_InventorApplication.Documents.Open(fullPath, false);

                //  Load params
                NLogger.LogText("Load assembly document parameters");
                InventorParams = m_AssemblyDocument.ComponentDefinition.Parameters;
            }
            catch (Exception ex)
            {
                NLogger.LogText("An error has occurred casting to an Inventor Assembly document. Try casting to a Part document");

                try
                {
                    NLogger.LogText($"Try opening part document {fullPath}");

                    m_PartDocument = (PartDocument)m_InventorApplication.Documents.Open(fullPath, false);

                    //  Load params
                    NLogger.LogText("Load Part document parameters");

                    InventorParams = m_PartDocument.ComponentDefinition.Parameters;
                }
                catch (Exception ex1)
                {
                    NLogger.LogText("An error has occurred casting to an Inventor Part document");

                    NLogger.LogError(ex1);
                    throw (ex1);
                }
            }

            NLogger.LogText("Create 'InventorParameterStructure'");

            for (int h = 1; h <= InventorParams.Count; h++)
            {
                if (ConfigUtilities.GetInventorElementShowOnlyKeys().ToLower() == "true")
                {
                    if (InventorParams[h].IsKey)
                    {
                        inventorParams.Add(new InventorParameterStructure { Name = InventorParams[h].Name.ToString() });
                    }
                }
                else
                {
                    inventorParams.Add(new InventorParameterStructure { Name = InventorParams[h].Name.ToString() });
                }
            }

            NLogger.LogText("Remove documents from  Inventor application instance");

            m_InventorApplication.Documents.CloseAll();

            NLogger.LogText("Exit LoadInventorTemplateParameters method");

            return inventorParams;
        }

        public void CloseInventorProcess()
        {
            NLogger.LogText("Entered CloseInventorProcess");

            if (started && m_InventorApplication != null)
            {
                m_InventorApplication.Quit();

                NLogger.LogText("Inventor Process shut down");
            }

            m_InventorApplication = null;

            NLogger.LogText("Exit CloseInventorProcess");
        }
    }
}
