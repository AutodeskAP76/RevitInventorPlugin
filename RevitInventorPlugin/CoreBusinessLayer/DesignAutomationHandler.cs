using Autodesk.Revit.DB.Architecture;
using Inventor;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.Targets;
using RevitInventorExchange.CoreDataStructures;
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitInventorExchange.CoreBusinessLayer
{
    public class DesignAutomationHandler
    {
        private string jsonStruct = "";
        private BIM360StructureBuilder bIM360DocsStructBuilder = null;
        private ForgeDMClient forgeDMClient = null;
        private ForgeDAClient forgeDAClient = null;

        private string inventorTemplatesFolder = "";        

        private DesignAutomationStructure daStructure = null;
        private DAEventHandlerUtilities daEventHandler;

        public DAEventHandlerUtilities DaEventHandler { get => daEventHandler; set => daEventHandler = value; }

        public DesignAutomationHandler()
        {          
            NLogger.LogText("Entered DesignAutomationHandler contructor");
            
            bIM360DocsStructBuilder = new BIM360StructureBuilder();
            forgeDMClient = new ForgeDMClient(ConfigUtilities.GetDMBaseProjectURL(), ConfigUtilities.GetClientID(), ConfigUtilities.GetClientSecret(), "data:read data:create");
            forgeDAClient = new ForgeDAClient(ConfigUtilities.GetDABaseURL(), ConfigUtilities.GetClientID(), ConfigUtilities.GetClientSecret());
            forgeDAClient.GetToken();

            daEventHandler = new DAEventHandlerUtilities();                        

            NLogger.LogText("Exit DesignAutomationHandler contructor");

            //  WARNING: current implementation consides only one inventor template file processed at time. For multiple processing logic must be changed
        }        
               
        //  Workflow which handles the Forge API invocation
        public void RunDesignAutomationForgeWorkflow(string json, string invTemplFolder)
        {            
            NLogger.LogText("Entered RunDesignAutomationForgeWorkflow");

            inventorTemplatesFolder = invTemplFolder; // ConfigUtilities.GetInventorTemplateFolder();
            jsonStruct = json;

            NLogger.LogText($"Inventor templates used folder: {inventorTemplatesFolder}");

            daEventHandler.TriggerDACurrentStepHandler("Workflow started");

            try
            {
                CheckConfigurationConsistency();

                //  build the BIM360 folders structure
                var hubId = BuildHubStructure();
                var projId = BuildProjectStructure(hubId);
                BuildFoldersStructures(hubId, projId);

                daEventHandler.TriggerDACurrentStepHandler("BIM360 structure created");

                //  build the input - output files internal structure
                daStructure = GetDataFromInputJson1();
                //daStructure = GetDataFromInputJson1_for_zip_tests();

                //  Create output storage object, submit workitem and create version
                HandleDesignAutomationFlow(projId);

                daEventHandler.TriggerDACurrentStepHandler("Workflow completed");
            }
            catch (Exception ex)
            {
                daEventHandler.TriggerDACurrentStepHandler("Some error has occurred: please check logs");
                NLogger.LogError(ex);
            }

            NLogger.LogText("Exit RunDesignAutomationForgeWorkflow");
        }

        private void CheckConfigurationConsistency()
        {
            NLogger.LogText("Entered CheckConfigurationConsistency");

            var clientId = ConfigUtilities.GetClientID();
            var clientSecret = ConfigUtilities.GetClientSecret();
            var HUB = ConfigUtilities.GetHub();
            var project = ConfigUtilities.GetProject();
            var InventorTemplateFolder = ConfigUtilities.GetInventorTemplateFolder();

            if (string.IsNullOrEmpty(clientId))
            {
                throw new Exception("'ClientId' key must be configured");
            }

            if (string.IsNullOrEmpty(clientSecret))
            {
                throw new Exception("'ClientSecret' key must be configured");
            }

            if (string.IsNullOrEmpty(HUB))
            {
                throw new Exception("'HUB' key must be configured");
            }
            if (string.IsNullOrEmpty(project))
            {
                throw new Exception("'project' key must be configured");
            }
            if (string.IsNullOrEmpty(InventorTemplateFolder))
            {
                throw new Exception("'InventorTemplateFolder' key must be configured");
            }

            NLogger.LogText("Exit CheckConfigurationConsistency");
        }

        private string BuildHubStructure()
        {
            NLogger.LogText("Entered BuildHubStructure");

            forgeDMClient.SetBaseURL(ConfigUtilities.GetDMBaseProjectURL());
            var ret = forgeDMClient.GetHub();
            ret.Wait(ConfigUtilities.GetAsyncHTTPCallWaitTime());

            var res = ret.Result;

            if (res.IsSuccessStatusCode())
            {
                var hubId = bIM360DocsStructBuilder.SetHubStructure(res);

                NLogger.LogText("Exit BuildHubStructure sucessfully");

                return hubId;
            }
            else
            {
                Utilities.HandleErrorInForgeResponse("BuildHubStructure", res);
                return null;
            }
        }

        private string BuildProjectStructure(string hubId)
        {
            NLogger.LogText("Entered BuildProjectStructure");

            forgeDMClient.SetBaseURL(ConfigUtilities.GetDMBaseProjectURL());
            var ret = forgeDMClient.GetProject(hubId);
            ret.Wait(ConfigUtilities.GetAsyncHTTPCallWaitTime());

            var res = ret.Result;

            if (res.IsSuccessStatusCode())
            {
                var projId = bIM360DocsStructBuilder.SetProjectStructure(res, hubId);

                NLogger.LogText("Exit BuildProjectStructure sucessfully");

                return projId;
            }
            else
            {
                Utilities.HandleErrorInForgeResponse("BuildProjectStructure", res);
                return null;
            }
        }

        //  Navigate folder structure from project level down
        private void BuildFoldersStructures(string hubId, string projId)
        {
            NLogger.LogText("Entered BuildFoldersStructures");

            //  Get path where Inventor Templates are stored
            var fullPath = inventorTemplatesFolder;

            NLogger.LogText($"Selected path for Inventor file: {inventorTemplatesFolder}");
            NLogger.LogText("Extract subfolder structure from BIM 360 project down");

            var relativePath = fullPath.Split(new string[] { ConfigUtilities.GetProject() }, StringSplitOptions.None);

            if (relativePath.Length < 2)
            {
                throw new Exception($"The configured project {ConfigUtilities.GetProject()} is not part of the selected path");
            }

            //  Split path in folders
            var foldersTemp = relativePath[1].Split(new char[] { '\\' });
            List<string> folders = new List<string>();

            if (foldersTemp[0] == "")
            {
                for (int h = 1; h < foldersTemp.Length; h++)
                {
                    folders.Add(foldersTemp[h]);
                }
            }
            else
            {
                for (int h = 0; h < foldersTemp.Length; h++)
                {
                    folders.Add(foldersTemp[h]);
                }
            }            

            //  Handle Top folder
            var topFolderId = BuildTopFolderStructure(hubId, projId, folders[0]);
            var folderId = topFolderId;

            //  Handle subfolders
            for (int l = 1; l < folders.Count(); l++)
            {
                folderId = BuildFolderStructure(projId, folderId, folders[l]);

                //  if LAST element in the configured path --> Process last folder files extraction (otherwise it would NOT be processed)
                if (l == (folders.Count() - 1))
                {
                    //  Handle the last folder in path. In this case only files are extracted
                    BuildFolderStructure(projId, folderId, "");
                }
            }
        }

        private string BuildTopFolderStructure(string hubId, string projectId, string folderName)
        {
            NLogger.LogText("Entered BuildTopFolderStructure");

            forgeDMClient.SetBaseURL(ConfigUtilities.GetDMBaseProjectURL());
            var ret = forgeDMClient.GetTopFolder(hubId, projectId);
            ret.Wait(ConfigUtilities.GetAsyncHTTPCallWaitTime());

            var res = ret.Result;

            if (res.IsSuccessStatusCode())
            {
                var folderId = bIM360DocsStructBuilder.SetFolderStructure(res, projectId, folderName);

                if (!string.IsNullOrEmpty(folderId))
                {
                    NLogger.LogText("Exit BuildTopFolderStructure sucessfully");
                }
                else
                {
                    var projName = bIM360DocsStructBuilder.bIM360DocsStructure1.BIM360DataRows1.First(p => p.Id == projectId).Name;
                    
                    string errStr = $"There are no folders under project '{projName}' with name '{folderName}'";
                    NLogger.LogError($"Exit BuildTopFolderStructure with Error");

                    throw new Exception(errStr);
                }

                return folderId;
            }
            else
            {
                Utilities.HandleErrorInForgeResponse("BuildTopFolderStructure", res);
                return null;
            }
        }

        private string BuildFolderStructure(string projectId, string parentFolderId, string folderName)
        {
            NLogger.LogText("Entered BuildFolderStructure");

            var forgeDMDataClient = new ForgeDMClient(ConfigUtilities.GetDMBaseDataURL(), ConfigUtilities.GetClientID(), ConfigUtilities.GetClientSecret(), "data:read");

            //  Extract parentFolder content (both subfolders and files)
            var ret = forgeDMDataClient.GetFolderContent(projectId, parentFolderId);
            ret.Wait(ConfigUtilities.GetAsyncHTTPCallWaitTime());

            var res = ret.Result;

            if (res.IsSuccessStatusCode())
            {
                string folderId = "";
                if (!string.IsNullOrEmpty(folderName))
                {
                    //  Create the structure of subfolders contained in parent folder
                    folderId = bIM360DocsStructBuilder.SetFolderStructure(res, parentFolderId, folderName);
                   
                    if (string.IsNullOrEmpty(folderId))
                    {
                        var projName = bIM360DocsStructBuilder.bIM360DocsStructure1.BIM360DataRows1.First(p => p.Id == projectId).Name;
                        var parentFoldername = bIM360DocsStructBuilder.bIM360DocsStructure1.BIM360DataRows1.First(p => p.Id == parentFolderId).Name;

                        string errStr = $"There are no folders under project '{projName}', parent folder '{parentFoldername}' with name '{folderName}'";
                        NLogger.LogError($"Exit BuildFolderStructure with Error");

                        throw new Exception(errStr);
                    }
                }

                //  Create the structure of files contained in parent folder
                bIM360DocsStructBuilder.SetFileStructure(res, parentFolderId, "");

                NLogger.LogText("Exit BuildFolderStructure sucessfully");
                return folderId;
            }
            else
            {
                Utilities.HandleErrorInForgeResponse("BuildFolderStructure", res);
                return null;
            }
        }

        //  Submit work item passing json with data extracted from Revit
        private void SubmitWokItem(string inFile, string outFile)
        {
            NLogger.LogText("Entered SubmitWokItem");

            //  Submit work items 
            string payload = CreateWorkItemPayload1(inFile, outFile);

            //string payload = CreateWorkItemPayload1_ForZip_Test(inFile, outFile);

            var retSubmitWotkItem = forgeDAClient.PostWorkItem(payload);
            retSubmitWotkItem.Wait(ConfigUtilities.GetAsyncHTTPCallWaitTime());

            var resSubmitWorkItem = retSubmitWotkItem.Result;

            //  Get Response. Check response status
            if (resSubmitWorkItem.IsSuccessStatusCode())
            {
                daEventHandler.TriggerDACurrentStepHandler("WorkItem submitted");

                JObject resSWIContent = JObject.Parse(resSubmitWorkItem.ResponseContent);

                var status = resSWIContent.SelectToken("$.status").ToString();
                var id = resSWIContent.SelectToken("$.id").ToString();

                NLogger.LogText($"Work Item {id} in status {status}");

                //  Check work Item status
                var ret1 = CheckWorkItemStatus(id);
                ret1.Wait(ConfigUtilities.GetAsyncHTTPCallWaitTime());

                var res2 = ret1.Result;

                if (res2.IsSuccessStatusCode())
                {
                    JObject res3 = JObject.Parse(res2.ResponseContent);
                    
                    status = res3.SelectToken("$.status").ToString();
                    id = resSWIContent.SelectToken("$.id").ToString();

                    NLogger.LogText($"Work Item {id} in status {status}");

                    if (status == "failedInstructions")
                    {
                        daEventHandler.TriggerDACurrentStepHandler("WorkItem processing completed with error. Please check logs");

                        string errString = res2.ResponseContent;
                        throw new Exception(errString);
                    }

                    daEventHandler.TriggerDACurrentStepHandler("WorkItem processing completed sucessfully");

                    NLogger.LogText("Exit SubmitWokItem sucessfully");                    
                }
            }
            else
            {
                Utilities.HandleErrorInForgeResponse("SubmitWokItem", resSubmitWorkItem);
            }
        }

        //  Check workitem status
        private async Task<ForgeRestResponse> CheckWorkItemStatus(string workItemId)
        {
            NLogger.LogText("Entered CheckWorkItemStatus");

            var ret = await forgeDAClient.CheckWorkItemStatus(workItemId);
            var res = ret.ResponseContent;

            try
            {                               
                JObject res1 = JObject.Parse(res);

                var status = res1.SelectToken("$.status").ToString();
                var id = res1.SelectToken("$.id").ToString();

                NLogger.LogText($"Work Item {id} in status {status}");

                if (status == "pending" || status == "inprogress")
                {
                    //  Wait for some seconds (configured), then perform the check on status again
                    await Task.Delay(TimeSpan.FromSeconds(Convert.ToInt32(ConfigUtilities.GetWorkItemCreationPollingTime())));
                    ret = await CheckWorkItemStatus(workItemId);
                }
            }
            catch (Exception ex)
            {
                NLogger.LogText($"There has been a problem. Returned value from call: {res}");
                throw (ex);
            }

            return ret;
        }

        //  Create json for Work Item submit
        private string CreateWorkItemPayload1(string inFileName, string outFileName)
        {
            NLogger.LogText("Entered CreateWorkItemPayload1");

            var daStructureRow = daStructure.FilesStructure.First(p => p.InputFilename == inFileName && p.OutputFileStructurelist.Any(k => k.OutFileName == outFileName));
            string inputSignedUrl = daStructureRow.InputLink;
            //string outputFileName = daStructureRow.OutputFileStructurelist.First(l => l.OutFileName == outFileName).OutFileName;
            string outFileStorageObj = daStructureRow.OutputFileStructurelist.First(l => l.OutFileName == outFileName).OutFileStorageobject;
            string outputSignedUrl = GetOutputLinks(outFileStorageObj);
            string jsonParams = daStructureRow.ParamValues; // dataFromJson["paramsValues"];
            string jsonParam1 = jsonParams.Replace("\r\n", "");

            var outputSignedUrlExtension = System.IO.Path.GetExtension(outputSignedUrl);           

            var itemParamOutput = "";
            var DAActivity = "";

            //  Based on input file extension create json
            if (outputSignedUrlExtension == ".ipt")
            {
                itemParamOutput = ConfigUtilities.GetDAWorkItemParamsOutputIpt();
                DAActivity = ConfigUtilities.GetDAPartActivity();

            }
            if (outputSignedUrlExtension == ".iam")
            {
                itemParamOutput = ConfigUtilities.GetDAWorkItemParamsOutputIam();
                //outputSignedUrl = outputSignedUrl.Replace("iam", "zip");

                DAActivity = ConfigUtilities.GetDAAssemblyActivity();
            }

            JObject payload = new JObject(
                new JProperty("activityId", DAActivity),
                new JProperty("arguments", new JObject(
                    new JProperty(ConfigUtilities.GetDAWorkItemDocInputArgument(), new JObject(
                        new JProperty("url", inputSignedUrl),
                        new JProperty("Headers", new JObject(
                            new JProperty("Authorization", forgeDAClient.Authorization)
                            ))
                    )),
                    new JProperty(ConfigUtilities.GetDAWorkItemParamsInputArgument(), new JObject(
                        new JProperty("url", "data:application/json, " + jsonParam1)
                    )),
                    new JProperty(itemParamOutput, new JObject(
                        new JProperty("url", outputSignedUrl),
                        new JProperty("verb", "put"),
                        new JProperty("headers", new JObject(
                            new JProperty("Authorization", forgeDAClient.Authorization),
                            new JProperty("Content-type", "application/octet-stream")
                        ))
                    ))
                ))
            );

            var ret = payload.ToString();

            NLogger.LogText("Exit CreateWorkItemPayload1");

            return ret;
        }



        //  Create json for Work Item submit
        private string CreateWorkItemPayload1_ForZip_Test(string inFileName, string outFileName)
        {
            NLogger.LogText("Entered CreateWorkItemPayload1");

            var daStructureRow = daStructure.FilesStructure.First(p => p.InputFilename == inFileName);
            string inputSignedUrl = daStructureRow.InputLink;
            //string outputFileName = daStructureRow.OutputFileStructurelist.First(l => l.OutFileName == outFileName).OutFileName;
            string outFileStorageObj = daStructureRow.OutputFileStructurelist.First(l => l.OutFileName == outFileName).OutFileStorageobject;
            string outputSignedUrl = GetOutputLinks(outFileStorageObj);
            string jsonParams = daStructureRow.ParamValues; // dataFromJson["paramsValues"];
            string jsonParam1 = jsonParams.Replace("\r\n", "");


            var outputSignedUrlExtension = System.IO.Path.GetExtension(outputSignedUrl);


            var itemParamOutput = "";
            var DAActivity = "";

            //  Based on input file extension create json
            if (outputSignedUrlExtension == ".ipt")
            {
                itemParamOutput = ConfigUtilities.GetDAWorkItemParamsOutputIpt();
                DAActivity = ConfigUtilities.GetDAPartActivity();

            }
            if (outputSignedUrlExtension == ".iam" || outputSignedUrlExtension == ".zip")
            {
                itemParamOutput = ConfigUtilities.GetDAWorkItemParamsOutputIam();
                outputSignedUrl = outputSignedUrl.Replace("zip", "iam");

                DAActivity = ConfigUtilities.GetDAAssemblyActivity();
            }

            JObject payload = new JObject(
                new JProperty("activityId", DAActivity),
                new JProperty("arguments", new JObject(
                    new JProperty(ConfigUtilities.GetDAWorkItemDocInputArgument(), new JObject(
                        new JProperty("url", inputSignedUrl),
                        new JProperty("zip", "true"),
                        new JProperty("localName", "input"),
                        new JProperty("Headers", new JObject(
                            new JProperty("Authorization", forgeDAClient.Authorization)
                            ))
                    )),
                    new JProperty(ConfigUtilities.GetDAWorkItemParamsInputArgument(), new JObject(
                        new JProperty("localName", "params.json"),
                        //new JProperty("url", "data:application/json,{\"assemblyPath\":\"input\\\\Workspace\\\\Libraries_DH_Assembly_wall\\\\Wall Panel.iam\", \"projectPath\":\"input\\\\Wall Panel.ipj\", \"values\": { \"Window_LeftRef\":\"750\"}}")
                        new JProperty("url", "data:application/json,{\"assemblyPath\":\"input\\\\Workspace\\\\Libraries_DH_Assembly_unit_frame\\\\unit_frame_assy.iam\", \"projectPath\":\"input\\\\unit_frame_assy.ipj\", \"values\": { \"UF_height\":\"2000\"}}")
                    )),
                    new JProperty(itemParamOutput, new JObject(
                        new JProperty("url", outputSignedUrl),
                        new JProperty("verb", "put"),
                        //new JProperty("localName", "ResultSmall.iam"),
                        new JProperty("Headers", new JObject(
                            new JProperty("Authorization", forgeDAClient.Authorization),
                            new JProperty("Content-type", "application/octet-stream")
                        ))
                    ))
                ))
            );

            var ret = payload.ToString();

            NLogger.LogText("Exit CreateWorkItemPayload1");

            return ret;
        }


        //  Extract data from Parameters values json file and put them into an internal structure to keep togehter data regarding input files and output files for Forge API automation
        private DesignAutomationStructure GetDataFromInputJson1()
        {
            NLogger.LogText("Entered GetDataFromInputJson1");

            //  Initialize internal structure keepin Forge relevant informations for output files creation
            NLogger.LogText("initialize internal structre for Design Automation files creation");
            var daStructure = new DesignAutomationStructure();
            daStructure.FilesStructure = new List<DesignAutomationFileStructure>();

            JObject res = JObject.Parse(jsonStruct);
            var items = res.SelectTokens("$.ILogicParams").Children();

            foreach (var item in items)
            {
                var inventorFileName = ((string)item.SelectToken("$.InventorTemplate"));
                var parametersInfo = item.SelectTokens("$.ParametersInfo");

                foreach (var paramInfo in parametersInfo.Children())
                {
                    var paramValues = paramInfo.SelectToken("$.paramsValues").ToString();
                    var elementId = paramInfo.SelectToken("$.elementId").ToString();
                    string inputLink = GetInputLink(inventorFileName);
                    var outputFileNameParts = inventorFileName.Split(new char[] { '.' });
                    var outputFileName = outputFileNameParts[0] + $"_Out_{elementId}_{DateTime.Now.ToString("yyyyMMddHHmmssffff")}." + outputFileNameParts[1];

                    //  Get path selected from the user where Inventor Templates are stored
                    var relativePath = inventorTemplatesFolder;

                    //  Split path in folders
                    var outputFolders = relativePath.Split(new char[] { '\\' });
                    var outputFileFolderId = bIM360DocsStructBuilder.GetFolderIdByName(outputFolders[outputFolders.Length - 1]);

                    NLogger.LogText($"Currently processing {inventorFileName} Inventor file");
                    NLogger.LogText($"Output file: {outputFileName}");
                    NLogger.LogText($"Output folder: {outputFileFolderId}");

                    //daStructure.FilesStructure = new List<DesignAutomationFileStructure>() { new DesignAutomationFileStructure
                    daStructure.FilesStructure.Add(new DesignAutomationFileStructure
                    {
                        InputFilename = inventorFileName,
                        ParamValues = paramValues,
                        InputLink = inputLink,
                        OutputFileStructurelist = new List<DesignAutomationOutFileStructure>(){ new DesignAutomationOutFileStructure {  OutFileName = outputFileName, OutFileFolder = outputFileFolderId } }
                    });
                }
            }

            NLogger.LogText("Exit GetDataFromInputJson1");

            return daStructure;
        }


        private DesignAutomationStructure GetDataFromInputJson1_for_zip_tests()
        {
            NLogger.LogText("Entered GetDataFromInputJson1");

            //  Initialize internal structure keepin Forge relevant informations for output files creation

            NLogger.LogText("initialize internal structre for Design Automation files creation");
            var daStructure = new DesignAutomationStructure();

            JObject res = JObject.Parse(jsonStruct);
            var items = res.SelectTokens("$.ILogicParams").Children();

            foreach (var item in items)
            {
                var inventorFileName = ((string)item.SelectToken("$.InventorTemplate"));
                var parametersInfo = item.SelectTokens("$.ParametersInfo");

                foreach (var paramInfo in parametersInfo.Children())
                {
                    var paramValues = paramInfo.SelectToken("$.paramsValues").ToString();

                    string inputLink = GetInputLink(inventorFileName);
                    var outputFileNameParts = inventorFileName.Split(new char[] { '.' });
                    var outputFileName = outputFileNameParts[0] + "_Out_001.iam";

                    //  Get path from Config file where Inventor Templates are stored
                    var relativePath = inventorTemplatesFolder;

                    //  Split path in folders
                    var outputFolders = relativePath.Split(new char[] { '\\' });

                    var outputFileFolderId = bIM360DocsStructBuilder.GetFolderIdByName(outputFolders[outputFolders.Length - 1]);

                    NLogger.LogText($"Currently processing {inventorFileName} Inventor file");
                    NLogger.LogText($"Output file: {outputFileName}");
                    NLogger.LogText($"Output folder: {outputFileFolderId}");


                    //  TODO: Here only the last elemet is processed. See how to process all elements in parametersInfo
                    daStructure.FilesStructure = new List<DesignAutomationFileStructure>() { new DesignAutomationFileStructure
                    {
                        InputFilename = inventorFileName,
                        ParamValues = paramValues,
                        InputLink = inputLink,
                        OutputFileStructurelist = new List<DesignAutomationOutFileStructure>(){ new DesignAutomationOutFileStructure {  OutFileName = outputFileName, OutFileFolder = outputFileFolderId } }
                    }};
                }
            }

            NLogger.LogText("Exit GetDataFromInputJson1");

            return daStructure;
        }

        private string GetInputLink(string filename)
        {
            NLogger.LogText("Entered GetInputLink");

            string inputStorageId = bIM360DocsStructBuilder.GetObjectStorageByFileName(filename);
            var inputStorageIdParts = inputStorageId.Split(new char[] { '/' });

            string inputStorageObject = ConfigUtilities.GetDALinkBaseURL() + "buckets/wip.dm.prod/objects/" + inputStorageIdParts[inputStorageIdParts.Length - 1];

            NLogger.LogText("Exit GetInputLink");

            return inputStorageObject;
        }

        private string GetOutputLinks(string storageObjectId)
        {
            NLogger.LogText("Entered GetOutputLinks");

            var storageObjectIdParts = storageObjectId.Split(new char[] { '/' });

            string outLink = ConfigUtilities.GetDALinkBaseURL() + "buckets/wip.dm.prod/objects/" + storageObjectIdParts[storageObjectIdParts.Length - 1];

            NLogger.LogText("Exit GetOutputLinks");

            return outLink;
        }


        private string CreateStorageObject(string projId, string inputFile, string outputFile)
        {
            NLogger.LogText("Entered CreateStorageObject");

            string OutFileStorageobjectId = "";

            forgeDMClient.SetBaseURL(ConfigUtilities.GetDMBaseDataURL());

            var ret = forgeDMClient.CreateStorageObject(projId, CreateStorageObjectPayload1(inputFile, outputFile));
            ret.Wait(ConfigUtilities.GetAsyncHTTPCallWaitTime());

            var res = ret.Result;

            if (res.IsSuccessStatusCode())
            {
                JObject root = JObject.Parse(res.ResponseContent);

                //  Extract the created strorage object identisfier. It is needed to create item versions
                OutFileStorageobjectId = root["data"]["id"].ToString();

                NLogger.LogText($"Storage object {OutFileStorageobjectId} created");

                daEventHandler.TriggerDACurrentStepHandler("Storage object created");             
            }
            else
            {
                Utilities.HandleErrorInForgeResponse("CreateStorageObject", res);
            }
            
            NLogger.LogText("Exit CreateStorageObject");

            return OutFileStorageobjectId;
        }

        //  Create Storage Object for output files to be generated
        private void HandleDesignAutomationFlow(string projId)
        {
            NLogger.LogText("Entered HandleDesignAutomationFlow");

            forgeDMClient.SetBaseURL(ConfigUtilities.GetDMBaseDataURL());
            
            foreach (var item in daStructure.FilesStructure)
            {
                foreach (var el in item.OutputFileStructurelist)
                {
                    string inputFile = item.InputFilename;
                    string outputFile = el.OutFileName;

                    NLogger.LogText($"Started Design automation Flow for input file: {inputFile} with corresponding output file {outputFile}");

                    //  Create Storage Object, Submit workItem and Create File version
                    el.OutFileStorageobject = CreateStorageObject(projId, inputFile, outputFile);                    
                    SubmitWokItem(inputFile, outputFile);
                    CreateFileVersion(projId, inputFile, outputFile, el.OutFileFolder);
                }
            }

            NLogger.LogText("Exit HandleDesignAutomationFlow");
        }       

        //  Create json for Object storage creation
        //  TODO: Check if all pieces are generic enough
        private string CreateStorageObjectPayload1(string inFileName, string outFileName)
        {
            NLogger.LogText("Entered CreateStorageObjectPayload1");

            var daStructureRow = daStructure.FilesStructure.First(p => p.InputFilename == inFileName && p.OutputFileStructurelist.Any(k => k.OutFileName == outFileName));
            string inputFileName = daStructureRow.InputFilename;
            string outputFileName = daStructureRow.OutputFileStructurelist.First(l => l.OutFileName == outFileName).OutFileName;
            string outputFileFolderId = daStructureRow.OutputFileStructurelist.First(l => l.OutFileName == outFileName).OutFileFolder;

            JObject payload = new JObject(
                new JProperty("jsonapi", new JObject(
                    new JProperty("version", "1.0")
                )),
                new JProperty("data", new JObject(
                    new JProperty("type", "objects"),
                    new JProperty("attributes", new JObject(
                        new JProperty("name", outputFileName)
                        )),
                    new JProperty("relationships", new JObject(
                        new JProperty("target", new JObject(
                            new JProperty("data", new JObject(
                                new JProperty("type", "folders"),
                                new JProperty("id", outputFileFolderId)
                            ))
                        ))
                    ))
                )
                ));

            NLogger.LogText("Exit CreateStorageObjectPayload1");

            return payload.ToString();
        }

        //  Create first version of generated files
        private void CreateFileVersion(string projectId, string inFileName, string outFileName, string outFilefolder)
        {
            NLogger.LogText("Entered CreateFileVersion");

            //  Check if BIM 360 already contains at least one version of intended output file
            var alreadyExistingOutFiles = bIM360DocsStructBuilder.bIM360DocsStructure1.BIM360DataRows1.FirstOrDefault(l => l.Name == outFileName && l.ParentId == outFilefolder && l.Type == BIM360Type.File);
            //var alreadyExistingOutFileVersion = bIM360DocsStructBuilder.bIM360DocsStructure1.BIM360DataRows1.FirstOrDefault(l => l.ParentId == alreadyExistingOutFiles.Id && l.Type == BIM360Type.FileVersion);

            if (alreadyExistingOutFiles == null)
            {
                NLogger.LogText($"Create first version of file {outFileName}");

                string payload = CreateFileFirstVersionPayload(inFileName, outFileName);

                forgeDMClient.SetBaseURL(ConfigUtilities.GetDMBaseDataURL());

                var retCreateFileVer = forgeDMClient.CreateFileFirstVersion(projectId, payload);
                retCreateFileVer.Wait(ConfigUtilities.GetAsyncHTTPCallWaitTime());

                var resCreateFileVer = retCreateFileVer.Result;

                if (resCreateFileVer.IsSuccessStatusCode())
                {
                    daEventHandler.TriggerDACurrentStepHandler("CreateFileVersion processing completed sucessfully");
                    NLogger.LogText("Exit CreateFileVersion sucessfully");
                }
                else
                {
                    Utilities.HandleErrorInForgeResponse("CreateFileVersion", resCreateFileVer);
                }
            }
            else
            {
                NLogger.LogText($"Create additional version of file {outFileName}");

                string payload = CreateFileAdditionalVersionPayload(inFileName, outFileName, alreadyExistingOutFiles.Id);

                forgeDMClient.SetBaseURL(ConfigUtilities.GetDMBaseDataURL());

                var retCreateFileVer = forgeDMClient.CreateFileAdditionalVersion(projectId, payload);
                retCreateFileVer.Wait(ConfigUtilities.GetAsyncHTTPCallWaitTime());

                var resCreateFileVer = retCreateFileVer.Result;

                if (resCreateFileVer.IsSuccessStatusCode())
                {
                    daEventHandler.TriggerDACurrentStepHandler("CreateFileVersion processing completed sucessfully");
                    NLogger.LogText("Exit CreateFileVersion sucessfully");
                }
                else
                {
                    Utilities.HandleErrorInForgeResponse("CreateFileVersion", resCreateFileVer);
                }
            }
        }

        private string CreateFileAdditionalVersionPayload(string inFileName, string outFileName, string itemId)
        {
            NLogger.LogText("Entered CreateFileAdditionalVersionPayload");

            var daStructureRow = daStructure.FilesStructure.First(p => p.InputFilename == inFileName && p.OutputFileStructurelist.Any(k => k.OutFileName == outFileName));

            string inputFileName = daStructureRow.InputFilename;
            var daStructureRowOutputFileStruct = daStructureRow.OutputFileStructurelist.First(l => l.OutFileName == outFileName);
            string outputFileName = daStructureRowOutputFileStruct.OutFileName;
            string outputFileFolderId = daStructureRowOutputFileStruct.OutFileFolder;
            string storageObjectId = daStructureRowOutputFileStruct.OutFileStorageobject;

            JObject payload = new JObject(
               new JProperty("jsonapi", new JObject(
                   new JProperty("version", "1.0")
               )),
               new JProperty("data", new JObject(
                   new JProperty("type", "versions"),
                   new JProperty("attributes", new JObject(
                        new JProperty("name", outputFileName),
                        new JProperty("extension", new JObject(
                            new JProperty("type", "versions:autodesk.bim360:File"),
                            new JProperty("version", "1.0")
                        ))
                    )),
                    new JProperty("relationships", new JObject(
                        new JProperty("item", new JObject(
                            new JProperty("data", new JObject(
                                new JProperty("type", "items"),
                                new JProperty("id", itemId)
                            ))
                        )),
                        new JProperty("storage", new JObject(
                            new JProperty("data", new JObject(
                                new JProperty("type", "objects"),
                                new JProperty("id", storageObjectId)
                            ))
                       ))
                    ))                   
               ))
            );


            NLogger.LogText("Exit CreateFileAdditionalVersionPayload");

            return payload.ToString();
        }

        private string CreateFileFirstVersionPayload(string inFileName, string outFileName)
        {
            NLogger.LogText("Entered CreateFileFirstVersionPayload");

            var daStructureRow = daStructure.FilesStructure.First(p => p.InputFilename == inFileName && p.OutputFileStructurelist.Any(k => k.OutFileName == outFileName));

            string inputFileName = daStructureRow.InputFilename;
            var daStructureRowOutputFileStruct = daStructureRow.OutputFileStructurelist.First(l => l.OutFileName == outFileName);
            string outputFileName = daStructureRowOutputFileStruct.OutFileName;
            string outputFileFolderId = daStructureRowOutputFileStruct.OutFileFolder;
            string storageObjectId = daStructureRowOutputFileStruct.OutFileStorageobject;

            JObject payload = new JObject(
               new JProperty("jsonapi", new JObject(
                   new JProperty("version", "1.0")
               )),
               new JProperty("data", new JObject(
                   new JProperty("type", "items"),
                   new JProperty("attributes", new JObject(
                        new JProperty("displayName", outputFileName),
                        new JProperty("extension", new JObject(
                            new JProperty("type", "items:autodesk.bim360:File"),
                            new JProperty("version", "1.0")
                            ))
                        )),
                   new JProperty("relationships", new JObject(
                       new JProperty("tip", new JObject(
                            new JProperty("data", new JObject(
                                new JProperty("type", "versions"),
                                new JProperty("id", "1")
                            ))
                       )),
                       new JProperty("parent", new JObject(
                            new JProperty("data", new JObject(
                                new JProperty("type", "folders"),
                                new JProperty("id", outputFileFolderId)
                            ))
                       ))
                   ))
               )),
               new JProperty("included", new JArray(new JObject(
                   new JProperty("type", "versions"),
                   new JProperty("id", "1"),
                   new JProperty("attributes", new JObject(
                        new JProperty("name", outputFileName),
                        new JProperty("extension", new JObject(
                            new JProperty("type", "versions:autodesk.bim360:File"),
                            new JProperty("version", "1.0")
                        ))
                   )),
                   new JProperty("relationships", new JObject(
                       new JProperty("storage", new JObject(
                            new JProperty("data", new JObject(
                                new JProperty("type", "objects"),
                                new JProperty("id", storageObjectId)
                            ))
                       ))
                   ))
                )))
            );

            NLogger.LogText("Exit CreateFileFirstVersionPayload");

            return payload.ToString();
        }
    }        
}
