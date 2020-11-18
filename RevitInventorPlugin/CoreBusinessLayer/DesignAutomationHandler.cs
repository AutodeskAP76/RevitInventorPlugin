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
using RevitInventorExchange;
using RevitInventorExchange.Utilities;
using System.Threading;

namespace RevitInventorExchange.CoreBusinessLayer
{
    public class DesignAutomationHandler
    {
        private readonly int HTTPNumberOfRetries = ConfigUtilities.GetAsyncHTTPCallNumberOfRetries(); 
        private readonly int HTTPCallRetryWaitTime = ConfigUtilities.GetAsyncHTTPCallRetryWaitTime();
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
        public async Task RunDesignAutomationForgeWorkflow(string json, string invTemplFolder)
        {            
            NLogger.LogText("Entered RunDesignAutomationForgeWorkflow");

            inventorTemplatesFolder = invTemplFolder; // ConfigUtilities.GetInventorTemplateFolder();
            jsonStruct = json;

            NLogger.LogText($"Inventor templates used folder: {inventorTemplatesFolder}");
            NLogger.LogText($"Received parameters json file: {jsonStruct}");

            daEventHandler.TriggerDACurrentStepHandler("Workflow started");

            try
            {
                CheckConfigurationConsistency();

                //  build the BIM360 folders structure
                var hubId = await BuildHubStructure();
                var projId = await BuildProjectStructure(hubId);
                await BuildFoldersStructures(hubId, projId);

                daEventHandler.TriggerDACurrentStepHandler("BIM360 structure created");

                //  build the input - output files internal structure

                daStructure = GetDataFromInputJson1();
                //daStructure = GetDataFromInputJson1_for_zip_tests();

                //  Create output storage object, submit workitem and create version
                await HandleDesignAutomationFlow(projId);

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

        private async Task<string> BuildHubStructure()
        {
            NLogger.LogText("Entered BuildHubStructure");

            forgeDMClient.SetBaseURL(ConfigUtilities.GetDMBaseProjectURL());


            //var res = RetryHelper.Retry(HTTPNumberOfRetries, HTTPCallRetryWaitTime, forgeDMClient.GetHub, new Dictionary<string, string>());            



            //var ret = forgeDMClient.GetHub(new Dictionary<string, string>());
            //ret.Wait(ConfigUtilities.GetAsyncHTTPCallWaitTime());
            //var res = ret.Result;



            //var ret = await forgeDMClient.GetHub(new Dictionary<string, string>());
            //var res = ret;



            var res = await RetryHelper.Retry(HTTPNumberOfRetries, HTTPCallRetryWaitTime, forgeDMClient.GetHub, new Dictionary<string, string>());



            if (res.IsSuccessStatusCode())
            {
                var hubId = bIM360DocsStructBuilder.SetHubStructure(res);

                NLogger.LogText("Exit BuildHubStructure sucessfully");

                return hubId;
            }
            else
            {
                Utility.HandleErrorInForgeResponse("BuildHubStructure", res);
                return null;
            }
        }

        private async Task<string> BuildProjectStructure(string hubId)
        {
            NLogger.LogText("Entered BuildProjectStructure");

            forgeDMClient.SetBaseURL(ConfigUtilities.GetDMBaseProjectURL());

            var parameters = new Dictionary<string, string>() { { "hubId", hubId } };



            //var res = RetryHelper.Retry(HTTPNumberOfRetries, HTTPCallRetryWaitTime, forgeDMClient.GetProject, parameters);


            //var ret = await forgeDMClient.GetProject(parameters);
            //var res = ret;


            //var ret = forgeDMClient.GetProject(hubId);
            //ret.Wait(ConfigUtilities.GetAsyncHTTPCallWaitTime());
            //var res = ret.Result;



            var res = await RetryHelper.Retry(HTTPNumberOfRetries, HTTPCallRetryWaitTime, forgeDMClient.GetProject, parameters);


            if (res.IsSuccessStatusCode())
            {
                var projId = bIM360DocsStructBuilder.SetProjectStructure(res, hubId);

                NLogger.LogText("Exit BuildProjectStructure sucessfully");

                return projId;
            }
            else
            {
                Utility.HandleErrorInForgeResponse("BuildProjectStructure", res);
                return null;
            }
        }

        //  Navigate folder structure from project level down
        private async Task BuildFoldersStructures(string hubId, string projId)
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
            var topFolderId = await BuildTopFolderStructure(hubId, projId, folders[0]);
            var folderId = topFolderId;

            //  Handle subfolders
            for (int l = 1; l < folders.Count(); l++)
            {
                folderId = await BuildFolderStructure(projId, folderId, folders[l]);

                //  if LAST element in the configured path --> Process last folder files extraction (otherwise it would NOT be processed)
                if (l == (folders.Count() - 1))
                {
                    //  Handle the last folder in path. In this case only files are extracted
                    await BuildFolderStructure(projId, folderId, "");
                }
            }
        }

        private async Task<string> BuildTopFolderStructure(string hubId, string projectId, string folderName)
        {
            NLogger.LogText("Entered BuildTopFolderStructure");

            forgeDMClient.SetBaseURL(ConfigUtilities.GetDMBaseProjectURL());

            var parameters = new Dictionary<string, string>() { { "hubId", hubId }, { "projectId", projectId } };


            //var res = RetryHelper.Retry(HTTPNumberOfRetries, HTTPCallRetryWaitTime, forgeDMClient.GetTopFolder, parameters);


            //var ret = forgeDMClient.GetTopFolder(hubId, projectId);
            //ret.Wait(ConfigUtilities.GetAsyncHTTPCallWaitTime());
            //var res = ret.Result;


            //var ret = await forgeDMClient.GetTopFolder(parameters);
            //var res = ret;



            var res = await RetryHelper.Retry(HTTPNumberOfRetries, HTTPCallRetryWaitTime, forgeDMClient.GetTopFolder, parameters);

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
                Utility.HandleErrorInForgeResponse("BuildTopFolderStructure", res);
                return null;
            }
        }

        private async Task<string> BuildFolderStructure(string projectId, string parentFolderId, string folderName)
        {
            NLogger.LogText("Entered BuildFolderStructure");

            var forgeDMDataClient = new ForgeDMClient(ConfigUtilities.GetDMBaseDataURL(), ConfigUtilities.GetClientID(), ConfigUtilities.GetClientSecret(), "data:read");

            //  Extract parentFolder content (both subfolders and files)

            var parameters = new Dictionary<string, string>() { { "projectId", projectId }, { "parentFolderId", parentFolderId } };


            //var res = RetryHelper.Retry(HTTPNumberOfRetries, HTTPCallRetryWaitTime, forgeDMDataClient.GetFolderContent, parameters);


            //var ret = forgeDMDataClient.GetFolderContent(projectId, parentFolderId);
            //ret.Wait(ConfigUtilities.GetAsyncHTTPCallWaitTime());
            ////var res = ret.Result;


            //var ret = await forgeDMDataClient.GetFolderContent(parameters);
            //var res = ret;


            var res = await RetryHelper.Retry(HTTPNumberOfRetries, HTTPCallRetryWaitTime, forgeDMDataClient.GetFolderContent, parameters);


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
                Utility.HandleErrorInForgeResponse("BuildFolderStructure", res);
                return null;
            }
        }


        //private void SubmitWokItem_original(string inFile, string outFile)
        //{
        //    NLogger.LogText("Entered SubmitWokItem");

        //    //  Submit work items 

        //    //string payload = CreateWorkItemPayload1(inFile, outFile);
        //    string payload = CreateWorkItemPayload1_ForZip_Test(inFile, outFile);




        //    var retSubmitWotkItem = forgeDAClient.PostWorkItem(payload);
        //    retSubmitWotkItem.Wait(ConfigUtilities.GetAsyncHTTPCallWaitTime());

        //    var resSubmitWorkItem = retSubmitWotkItem.Result;

        //    //  Get Response. Check response status
        //    if (resSubmitWorkItem.IsSuccessStatusCode())
        //    {
        //        daEventHandler.TriggerDACurrentStepHandler("WorkItem submitted");

        //        JObject resSWIContent = JObject.Parse(resSubmitWorkItem.ResponseContent);

        //        var status = resSWIContent.SelectToken("$.status").ToString();
        //        var id = resSWIContent.SelectToken("$.id").ToString();

        //        NLogger.LogText($"Work Item {id} in status {status}");

        //        //  Check work Item status
        //        var ret1 = CheckWorkItemStatus(id);
        //        ret1.Wait(ConfigUtilities.GetAsyncHTTPCallWaitTime());

        //        var res2 = ret1.Result;

        //        if (res2.IsSuccessStatusCode())
        //        {
        //            JObject res3 = JObject.Parse(res2.ResponseContent);

        //            status = res3.SelectToken("$.status").ToString();
        //            id = resSWIContent.SelectToken("$.id").ToString();

        //            NLogger.LogText($"Work Item {id} in status {status}");

        //            if (status == "failedInstructions")
        //            {
        //                daEventHandler.TriggerDACurrentStepHandler("WorkItem processing completed with error. Please check logs");

        //                string errString = res2.ResponseContent;
        //                throw new Exception(errString);
        //            }

        //            daEventHandler.TriggerDACurrentStepHandler("WorkItem processing completed sucessfully");

        //            NLogger.LogText("Exit SubmitWokItem sucessfully");
        //        }
        //    }
        //    else
        //    {
        //        Utility.HandleErrorInForgeResponse("SubmitWokItem", resSubmitWorkItem);
        //    }
        //}




        //  Submit work item passing json with data extracted from Revit
        private async Task SubmitWokItem(string inFile, string outFile)
        {
            NLogger.LogText("Entered SubmitWokItem");

            //  Submit work items 
            
            string payload = CreateWorkItemPayload1(inFile, outFile);
            //string payload = CreateWorkItemPayload1_Zip_Test(inFile, outFile);




            //var retSubmitWotkItem = forgeDAClient.PostWorkItem(payload);
            //retSubmitWotkItem.Wait(ConfigUtilities.GetAsyncHTTPCallWaitTime());
            //var resSubmitWorkItem = retSubmitWotkItem.Result;




            var retSubmitWotkItem = await forgeDAClient.PostWorkItem(payload);            
            var resSubmitWorkItem = retSubmitWotkItem;




            //  Get Response. Check response status
            if (resSubmitWorkItem.IsSuccessStatusCode())
            {
                daEventHandler.TriggerDACurrentStepHandler("WorkItem submitted");

                JObject resSWIContent = JObject.Parse(resSubmitWorkItem.ResponseContent);

                var status = resSWIContent.SelectToken("$.status").ToString();
                var id = resSWIContent.SelectToken("$.id").ToString();

                NLogger.LogText($"Work Item {id} in status {status}");

                //  Check work Item status
                //var ret1 = CheckWorkItemStatus(id);
                //ret1.Wait(ConfigUtilities.GetAsyncHTTPCallWaitTime());
                //var res2 = ret1.Result;


                var ret1 = await CheckWorkItemStatus(id);
                var res2 = ret1;

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
                Utility.HandleErrorInForgeResponse("SubmitWokItem", resSubmitWorkItem);
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
                    await Task.Delay(Convert.ToInt32(ConfigUtilities.GetWorkItemCreationPollingTime()));
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

            string ret = "";

            var inputFilename = System.IO.Path.GetFileNameWithoutExtension(inFileName);
            var daStructureRow = daStructure.FilesStructure.First(p => p.InputFilename == inFileName && p.OutputFileStructurelist.Any(k => k.OutFileName == outFileName));
            string inputSignedUrl = daStructureRow.InputLink;
            string inputSignedUrlExtension = System.IO.Path.GetExtension(inputSignedUrl);
            //string outputFileName = daStructureRow.OutputFileStructurelist.First(l => l.OutFileName == outFileName).OutFileName;
            string outFileStorageObj = daStructureRow.OutputFileStructurelist.First(l => l.OutFileName == outFileName).OutFileStorageobject;
            string outputSignedUrl = GetOutputLinks(outFileStorageObj);
            string jsonParams = daStructureRow.ParamValues; // dataFromJson["paramsValues"];
            string jsonParam1 = jsonParams.Replace("\r\n", "");

            var outputSignedUrlExtension = System.IO.Path.GetExtension(outputSignedUrl);           

            var itemParamOutput = "";
            var DAActivity = "";
            var actualJsonParam = "";

            //  Based on input file extension create json
            //if (outputSignedUrlExtension == ".ipt")
            if (inputSignedUrlExtension == ".ipt")
            {
                itemParamOutput = ConfigUtilities.GetDAWorkItemParamsOutputIpt();
                DAActivity = ConfigUtilities.GetDAPartActivity();
                actualJsonParam = jsonParam1;

                ret = GetWorkItemJsonForIamIpt(DAActivity, inputSignedUrl, actualJsonParam, itemParamOutput, outputSignedUrl);
            }
            //if (outputSignedUrlExtension == ".iam")
            if (inputSignedUrlExtension == ".iam")
            {
                itemParamOutput = ConfigUtilities.GetDAWorkItemParamsOutputIam();
                DAActivity = ConfigUtilities.GetDAAssemblyActivity();
                actualJsonParam = jsonParam1;

                ret = GetWorkItemJsonForIamIpt(DAActivity, inputSignedUrl, actualJsonParam, itemParamOutput, outputSignedUrl);
            }

            //  If input file is assembly, it is expected to have a corresponding zip file with same name. Replace input file with zip extension
            //if (inputSignedUrlExtension == ".iam")
            //{
            //    itemParamOutput = ConfigUtilities.GetDAWorkItemParamsOutputIam();
            //    DAActivity = ConfigUtilities.GetDAAssemblyActivity();
            //    actualJsonParam = $"{{\"assemblyPath\":\"input\\\\{inputFilename}.iam\", \"projectPath\":\"input\\\\{inputFilename}.ipj\", \"values\": {jsonParam1}}}";

            //    inputSignedUrl = inputSignedUrl.Replace("iam", "zip");

            //    ret = GetWorkItemJsonForZip(DAActivity, inputSignedUrl, actualJsonParam, itemParamOutput, outputSignedUrl);
            //}

            //if (outputSignedUrlExtension == ".zip")
            if (inputSignedUrlExtension == ".zip")
            {                
                itemParamOutput = ConfigUtilities.GetDAWorkItemParamsOutputIam();
                DAActivity = ConfigUtilities.GetDAAssemblyActivity();
                //actualJsonParam = $"{{\"assemblyPath\":\"input\\\\Workspace\\\\Libraries_DH_Assembly_unit_frame_01\\\\Frame_Assy1.iam\", \"projectPath\":\"input\\\\Frame_Assy1.ipj\", \"values\": {jsonParam1}}}";
                actualJsonParam = $"{{\"assemblyPath\":\"input\\\\{inputFilename}.iam\", \"projectPath\":\"input\\\\{inputFilename}.ipj\", \"values\": {jsonParam1}}}";
                outputSignedUrl = outputSignedUrl.Replace("zip", "iam");

                ret = GetWorkItemJsonForZip(DAActivity, inputSignedUrl, actualJsonParam, itemParamOutput, outputSignedUrl);
            }

            //JObject payload = new JObject(
            //    new JProperty("activityId", DAActivity),
            //    new JProperty("arguments", new JObject(
            //        new JProperty(ConfigUtilities.GetDAWorkItemDocInputArgument(), new JObject(
            //            new JProperty("url", inputSignedUrl),
            //            new JProperty("localName", "input"),
            //            new JProperty("Headers", new JObject(
            //                new JProperty("Authorization", forgeDAClient.Authorization)
            //                ))
            //        )),
            //        new JProperty(ConfigUtilities.GetDAWorkItemParamsInputArgument(), new JObject(
            //            new JProperty("url", $"data:application/json, {actualJsonParam}")
            //        )),
            //        new JProperty(itemParamOutput, new JObject(
            //            new JProperty("url", outputSignedUrl),
            //            new JProperty("verb", "put"),
            //            new JProperty("Headers", new JObject(
            //                new JProperty("Authorization", forgeDAClient.Authorization),
            //                new JProperty("Content-type", "application/octet-stream")
            //            ))
            //        ))
            //    ))
            //);

            //var ret = payload.ToString();

            NLogger.LogText("Exit CreateWorkItemPayload1");

            return ret;
        }

        private string GetWorkItemJsonForZip(string DAActivity, string inputSignedUrl, string actualJsonParam, string itemParamOutput, string outputSignedUrl)
        {
            NLogger.LogText("Entered GetWorkItemJsonForZip");

            JObject payload = new JObject(
                new JProperty("activityId", DAActivity),
                new JProperty("arguments", new JObject(
                    new JProperty(ConfigUtilities.GetDAWorkItemDocInputArgument(), new JObject(
                        new JProperty("url", inputSignedUrl),
                        new JProperty("localName", "input"),
                        new JProperty("Headers", new JObject(
                            new JProperty("Authorization", forgeDAClient.Authorization)
                            ))
                    )),
                    new JProperty(ConfigUtilities.GetDAWorkItemParamsInputArgument(), new JObject(
                        new JProperty("url", $"data:application/json, {actualJsonParam}")
                    )),
                    new JProperty(itemParamOutput, new JObject(
                        new JProperty("url", outputSignedUrl),
                        new JProperty("verb", "put"),
                        new JProperty("Headers", new JObject(
                            new JProperty("Authorization", forgeDAClient.Authorization),
                            new JProperty("Content-type", "application/octet-stream")
                        ))
                    ))
                ))
            );

            var ret = payload.ToString();

            NLogger.LogText("Exit GetWorkItemJsonForZip");

            return ret;
        }

        private string GetWorkItemJsonForIamIpt(string DAActivity, string inputSignedUrl, string actualJsonParam, string itemParamOutput, string outputSignedUrl)
        {
            NLogger.LogText("Entered GetWorkItemJsonForIamIpt");

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
                        new JProperty("url", $"data:application/json, {actualJsonParam}")
                    )),
                    new JProperty(itemParamOutput, new JObject(
                        new JProperty("url", outputSignedUrl),
                        new JProperty("verb", "put"),
                        new JProperty("Headers", new JObject(
                            new JProperty("Authorization", forgeDAClient.Authorization),
                            new JProperty("Content-type", "application/octet-stream")
                        ))
                    ))
                ))
            );

            var ret = payload.ToString();

            NLogger.LogText("Exit GetWorkItemJsonForIamIpt");

            return ret;
        }

        //  Create json for Work Item submit
        //private string CreateWorkItemPayload1_Zip_Test(string inFileName, string outFileName)
        //{
        //    NLogger.LogText("Entered CreateWorkItemPayload1");

        //    var daStructureRow = daStructure.FilesStructure.First(p => p.InputFilename == inFileName);
        //    string inputSignedUrl = daStructureRow.InputLink;
        //    //string outputFileName = daStructureRow.OutputFileStructurelist.First(l => l.OutFileName == outFileName).OutFileName;
        //    string outFileStorageObj = daStructureRow.OutputFileStructurelist.First(l => l.OutFileName == outFileName).OutFileStorageobject;
        //    string outputSignedUrl = GetOutputLinks(outFileStorageObj);
        //    string jsonParams = daStructureRow.ParamValues; // dataFromJson["paramsValues"];
        //    string jsonParam1 = jsonParams.Replace("\r\n", "");


        //    var outputSignedUrlExtension = System.IO.Path.GetExtension(outputSignedUrl);


        //    var itemParamOutput = "";
        //    var DAActivity = "";

        //    //  Based on input file extension create json
        //    if (outputSignedUrlExtension == ".ipt")
        //    {
        //        itemParamOutput = ConfigUtilities.GetDAWorkItemParamsOutputIpt();
        //        DAActivity = ConfigUtilities.GetDAPartActivity();

        //    }
        //    if (outputSignedUrlExtension == ".iam" || outputSignedUrlExtension == ".zip")
        //    {
        //        itemParamOutput = ConfigUtilities.GetDAWorkItemParamsOutputIam();
        //        outputSignedUrl = outputSignedUrl.Replace("zip", "iam");

        //        DAActivity = ConfigUtilities.GetDAAssemblyActivity();
        //    }

        //    JObject payload = new JObject(
        //        new JProperty("activityId", DAActivity),
        //        new JProperty("arguments", new JObject(
        //            new JProperty(ConfigUtilities.GetDAWorkItemDocInputArgument(), new JObject(
        //                new JProperty("url", inputSignedUrl),
        //                //new JProperty("zip", "true"),
        //                new JProperty("localName", "input"),
        //                new JProperty("Headers", new JObject(
        //                    new JProperty("Authorization", forgeDAClient.Authorization)
        //                    ))
        //            )),
        //            new JProperty(ConfigUtilities.GetDAWorkItemParamsInputArgument(), new JObject(
        //                //new JProperty("localName", "params.json"),
        //                //new JProperty("url", "data:application/json,{\"assemblyPath\":\"input\\\\Workspace\\\\Libraries_DH_Assembly_wall\\\\Wall Panel.iam\", \"projectPath\":\"input\\\\Wall Panel.ipj\", \"values\": { \"Window_LeftRef\":\"750\"}}")
        //                //new JProperty("url", "data:application/json,{\"assemblyPath\":\"input\\\\Workspace\\\\Libraries_DH_Assembly_unit_frame\\\\unit_frame_assy.iam\", \"projectPath\":\"input\\\\unit_frame_assy.ipj\", \"values\": { \"UF_height\":\"2000\"}}")
        //                //new JProperty("url", "data:application/json,{\"assemblyPath\":\"input\\\\Workspace\\\\Libraries_DH_Assembly_unit_frame_01\\\\Frame_Assy1.iam\", \"projectPath\":\"input\\\\Frame_Assy1.ipj\", \"values\": { \"Height\":\"3000\" , \"Width\":\"3000\" , \"Length\":\"3000\"}}")
        //                //new JProperty("url", "data:application/json,{\"assemblyPath\":\"input\\\\Workspace\\\\Libraries_DH_Assembly_window\\\\Test_Frame Assy.iam\", \"projectPath\":\"input\\\\Test_Frame Assy.ipj\", \"values\": { \"Height\":\"3000\" , \"Width\":\"3000\" }}")

        //                new JProperty("url", $"data:application/json,{{\"assemblyPath\":\"input\\\\Workspace\\\\Libraries_DH_Assembly_unit_frame_01\\\\Frame_Assy1.iam\", \"projectPath\":\"input\\\\Frame_Assy1.ipj\", \"values\": {jsonParam1}}}")
        //            )),
        //            new JProperty(itemParamOutput, new JObject(
        //                new JProperty("url", outputSignedUrl),
        //                new JProperty("verb", "put"),
        //                new JProperty("Headers", new JObject(
        //                    new JProperty("Authorization", forgeDAClient.Authorization),
        //                    new JProperty("Content-type", "application/octet-stream")
        //                ))
        //            ))
        //        ))
        //    );

        //    var ret = payload.ToString();

        //    NLogger.LogText("Exit CreateWorkItemPayload1");

        //    return ret;
        //}


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

                    //  If input file has .zip extension, change the output extension to .iam
                    if (outputFileNameParts[1] == "zip")
                    {
                        outputFileNameParts[1] = "iam";
                    }

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


        //private DesignAutomationStructure GetDataFromInputJson1_for_zip_tests()
        //{
        //    NLogger.LogText("Entered GetDataFromInputJson1");

        //    //  Initialize internal structure keepin Forge relevant informations for output files creation

        //    NLogger.LogText("initialize internal structre for Design Automation files creation");
        //    var daStructure = new DesignAutomationStructure();

        //    JObject res = JObject.Parse(jsonStruct);
        //    var items = res.SelectTokens("$.ILogicParams").Children();

        //    foreach (var item in items)
        //    {
        //        var inventorFileName = ((string)item.SelectToken("$.InventorTemplate"));
        //        var parametersInfo = item.SelectTokens("$.ParametersInfo");

        //        foreach (var paramInfo in parametersInfo.Children())
        //        {
        //            var paramValues = paramInfo.SelectToken("$.paramsValues").ToString();
        //            var elementId = paramInfo.SelectToken("$.elementId").ToString();
        //            string inputLink = GetInputLink(inventorFileName);
        //            var outputFileNameParts = inventorFileName.Split(new char[] { '.' });

        //            if (outputFileNameParts[1] == "zip")
        //            {
        //                outputFileNameParts[1] = "iam";
        //            }

        //            var outputFileName = outputFileNameParts[0] + $"_Out_{elementId}_{DateTime.Now.ToString("yyyyMMddHHmmssffff")}." + outputFileNameParts[1];
        //            //var outputFileName = outputFileNameParts[0] + "_Out_001.iam";

        //            //  Get path from Config file where Inventor Templates are stored
        //            var relativePath = inventorTemplatesFolder;

        //            //  Split path in folders
        //            var outputFolders = relativePath.Split(new char[] { '\\' });

        //            var outputFileFolderId = bIM360DocsStructBuilder.GetFolderIdByName(outputFolders[outputFolders.Length - 1]);

        //            NLogger.LogText($"Currently processing {inventorFileName} Inventor file");
        //            NLogger.LogText($"Output file: {outputFileName}");
        //            NLogger.LogText($"Output folder: {outputFileFolderId}");


        //            //  TODO: Here only the last elemet is processed. See how to process all elements in parametersInfo
        //            daStructure.FilesStructure = new List<DesignAutomationFileStructure>() { new DesignAutomationFileStructure
        //            {
        //                InputFilename = inventorFileName,
        //                ParamValues = paramValues,
        //                InputLink = inputLink,
        //                OutputFileStructurelist = new List<DesignAutomationOutFileStructure>(){ new DesignAutomationOutFileStructure {  OutFileName = outputFileName, OutFileFolder = outputFileFolderId } }
        //            }};
        //        }
        //    }

        //    NLogger.LogText("Exit GetDataFromInputJson1");

        //    return daStructure;
        //}


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

        private async Task<string> CreateStorageObject(string projId, string inputFile, string outputFile)
        {
            NLogger.LogText("Entered CreateStorageObject");

            string OutFileStorageobjectId = "";

            forgeDMClient.SetBaseURL(ConfigUtilities.GetDMBaseDataURL());

            var parameters = new Dictionary<string, string>() { { "projectId", projId }, { "payload", CreateStorageObjectPayload1(inputFile, outputFile) } };



            //var res = RetryHelper.Retry(HTTPNumberOfRetries, HTTPCallRetryWaitTime, forgeDMClient.CreateStorageObject, parameters);


            //var ret = forgeDMClient.CreateStorageObject(projId, CreateStorageObjectPayload1(inputFile, outputFile));
            //ret.Wait(ConfigUtilities.GetAsyncHTTPCallWaitTime());
            //var res = ret.Result;


            //var ret = await forgeDMClient.CreateStorageObject(projId, CreateStorageObjectPayload1(inputFile, outputFile));
            //var res = ret;


            var res = await RetryHelper.Retry(HTTPNumberOfRetries, HTTPCallRetryWaitTime, forgeDMClient.CreateStorageObject, parameters);


            if (res.IsSuccessStatusCode())
            {
                JObject root = JObject.Parse(res.ResponseContent);

                //  Extract the created storage object identisfier. It is needed to create item versions
                OutFileStorageobjectId = root["data"]["id"].ToString();

                NLogger.LogText($"Storage object {OutFileStorageobjectId} created");

                daEventHandler.TriggerDACurrentStepHandler("Storage object created");             
            }
            else
            {
                Utility.HandleErrorInForgeResponse("CreateStorageObject", res);
            }
            
            NLogger.LogText("Exit CreateStorageObject");

            return OutFileStorageobjectId;
        }

        //  Create Storage Object for output files to be generated
        private async Task HandleDesignAutomationFlow(string projId)
        {
            NLogger.LogText("Entered HandleDesignAutomationFlow");

            forgeDMClient.SetBaseURL(ConfigUtilities.GetDMBaseDataURL());

            //  Below BL processes every input - output files. If some errors occur on a specific input-output file, the system goes on with the rest of files
            foreach (var item in daStructure.FilesStructure)
            {
                foreach (var el in item.OutputFileStructurelist)
                {
                    string inputFile = item.InputFilename;
                    string outputFile = el.OutFileName;

                    try
                    {
                        NLogger.LogText($"Started Design automation Flow for input file: {inputFile} with corresponding output file {outputFile}");
                        daEventHandler.TriggerDACurrentStepHandler($"Started Design automation Flow for input file: {inputFile} with corresponding output file {outputFile}");

                        //  Create Storage Object, Submit workItem and Create File version
                        el.OutFileStorageobject = await CreateStorageObject(projId, inputFile, outputFile);


                        await SubmitWokItem(inputFile, outputFile);
                        await CreateFileVersion(projId, inputFile, outputFile, el.OutFileFolder);


                        NLogger.LogText($"Design automation Flow for input file: {inputFile} with corresponding output file {outputFile} completed sucessfully");
                        daEventHandler.TriggerDACurrentStepHandler($"Design automation Flow for input file: {inputFile} with corresponding output file {outputFile} completed sucessfully");
                    }
                    catch (Exception ex)
                    {
                        NLogger.LogText($"Design automation Flow for input file: {inputFile} with corresponding output file {outputFile} completed with error");
                        daEventHandler.TriggerDACurrentStepHandler($"Design automation Flow for input file: {inputFile} with corresponding output file {outputFile} completed with error, please check logs");

                        NLogger.LogError(ex);
                    }
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
        private async Task CreateFileVersion(string projectId, string inFileName, string outFileName, string outFilefolder)
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


                var parameters = new Dictionary<string, string>() { { "projectId", projectId }, { "payload", payload } };


                //var resCreateFileVer = RetryHelper.Retry(HTTPNumberOfRetries, HTTPCallRetryWaitTime, forgeDMClient.CreateFileFirstVersion, parameters);


                //var retCreateFileVer = forgeDMClient.CreateFileFirstVersion(projectId, payload);
                //retCreateFileVer.Wait(ConfigUtilities.GetAsyncHTTPCallWaitTime());
                //var resCreateFileVer = retCreateFileVer.Result;


                //var retCreateFileVer = await forgeDMClient.CreateFileFirstVersion(projectId, payload);
                //var resCreateFileVer = retCreateFileVer;


                var resCreateFileVer = await RetryHelper.Retry(HTTPNumberOfRetries, HTTPCallRetryWaitTime, forgeDMClient.CreateFileFirstVersion, parameters);



                if (resCreateFileVer.IsSuccessStatusCode())
                {
                    daEventHandler.TriggerDACurrentStepHandler("CreateFileVersion processing completed sucessfully");
                    NLogger.LogText("Exit CreateFileVersion sucessfully");
                }
                else
                {
                    Utility.HandleErrorInForgeResponse("CreateFileVersion", resCreateFileVer);
                }
            }
            else
            {
                NLogger.LogText($"Create additional version of file {outFileName}");

                string payload = CreateFileAdditionalVersionPayload(inFileName, outFileName, alreadyExistingOutFiles.Id);

                forgeDMClient.SetBaseURL(ConfigUtilities.GetDMBaseDataURL());

                var parameters = new Dictionary<string, string>() { { "projectId", projectId }, { "payload", payload } };


                //var resCreateFileVer = RetryHelper.Retry(HTTPNumberOfRetries, HTTPCallRetryWaitTime, forgeDMClient.CreateFileAdditionalVersion, parameters);


                //var retCreateFileVer = forgeDMClient.CreateFileAdditionalVersion(projectId, payload);
                //retCreateFileVer.Wait(ConfigUtilities.GetAsyncHTTPCallWaitTime());
                //var resCreateFileVer = retCreateFileVer.Result;


                //var retCreateFileVer = await forgeDMClient.CreateFileAdditionalVersion(projectId, payload);
                //var resCreateFileVer = retCreateFileVer;


                var resCreateFileVer = await RetryHelper.Retry(HTTPNumberOfRetries, HTTPCallRetryWaitTime, forgeDMClient.CreateFileAdditionalVersion, parameters);



                if (resCreateFileVer.IsSuccessStatusCode())
                {
                    daEventHandler.TriggerDACurrentStepHandler("CreateFileVersion processing completed sucessfully");
                    NLogger.LogText("Exit CreateFileVersion sucessfully");
                }
                else
                {
                    Utility.HandleErrorInForgeResponse("CreateFileVersion", resCreateFileVer);
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
