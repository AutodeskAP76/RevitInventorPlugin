using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevitInventorExchange.CoreBusinessLayer;
using RevitInventorExchange;
using System.Threading.Tasks;

namespace RevitInventorPluginUnitTests
{
    [TestClass]
    public class UnitTest_Libraries_DH_Assembly_Zip

    {
        [TestMethod]
        public void UnitTest_Libraries_DH_Assembly_Window_Zip_Tests()
        {
            NLogger.Initialize();

            //  PLEASE NOTE:THE CONFIGURATION FILE MUST BE COPIED IN FOLDER "CONFIGURATION OF UNIT TESTS"
            ConfigUtilities.LoadConfig();

            string json = GetJson_Window();
            string InventorFilePath = @"C:\Users\parodiaadmin\BIM 360\ATDSK DEV\Sample Project\Project Files\Libraries_DH_Assembly_window_Zip";            
            DesignAutomationHandler daHandler = new DesignAutomationHandler();
            
            var ret = daHandler.RunDesignAutomationForgeWorkflow(json, InventorFilePath);
            ret.Wait();
        }


        [TestMethod]
        public void UnitTest_Libraries_DH_Assembly_Unit_Frame_Zip_Tests()
        {
            NLogger.Initialize();

            //  PLEASE NOTE:THE CONFIGURATION FILE MUST BE COPIED IN FOLDER "CONFIGURATION OF UNIT TESTS"
            ConfigUtilities.LoadConfig();

            string json = GetJson_Unit_Frame();            
            string InventorFilePath = @"C:\Users\parodiaadmin\BIM 360\ATDSK DEV\Sample Project\Project Files\Libraries_DH_Assembly_unit_frame_zip";
            DesignAutomationHandler daHandler = new DesignAutomationHandler();

            var ret = daHandler.RunDesignAutomationForgeWorkflow(json, InventorFilePath);
            ret.Wait();
        }

        [TestMethod]
        public void UnitTest_Libraries_DH_Assembly_Floor_Panel_4_0_Zip_Tests()
        {
            NLogger.Initialize();

            //  PLEASE NOTE:THE CONFIGURATION FILE MUST BE COPIED IN FOLDER "CONFIGURATION OF UNIT TESTS"
            ConfigUtilities.LoadConfig();

            string json = GetJson_Floor_Panel();
            string InventorFilePath = @"C:\Users\parodiaadmin\BIM 360\ATDSK DEV\Sample Project\Project Files\Libraries_DH_Assembly_Floor_Panel_4.0_zip";
            DesignAutomationHandler daHandler = new DesignAutomationHandler();

            var ret = daHandler.RunDesignAutomationForgeWorkflow(json, InventorFilePath);
            ret.Wait();
        }

        [TestMethod]
        public void UnitTest_Libraries_DH_Assembly_Floor_Panel_5_0_Zip_Tests()
        {
            NLogger.Initialize();

            //  PLEASE NOTE:THE CONFIGURATION FILE MUST BE COPIED IN FOLDER "CONFIGURATION OF UNIT TESTS"
            ConfigUtilities.LoadConfig();

            string json = GetJson_Floor_Panel();
            string InventorFilePath = @"C:\Users\parodiaadmin\BIM 360\ATDSK DEV\Sample Project\Project Files\Libraries_DH_Assembly_Floor_Panel_5.0_zip";
            DesignAutomationHandler daHandler = new DesignAutomationHandler();

            var ret = daHandler.RunDesignAutomationForgeWorkflow(json, InventorFilePath);
            ret.Wait();
        }

        [TestMethod]
        public void UnitTest_Libraries_DH_Assembly_Floor_Panel_8_0_Zip_Tests()
        {
            NLogger.Initialize();

            //  PLEASE NOTE:THE CONFIGURATION FILE MUST BE COPIED IN FOLDER "CONFIGURATION OF UNIT TESTS"
            ConfigUtilities.LoadConfig();

            string json = GetJson_Floor_Panel();
            string InventorFilePath = @"C:\Users\parodiaadmin\BIM 360\ATDSK DEV\Sample Project\Project Files\Libraries_DH_Assembly_Floor_Panel_8.0_zip";
            DesignAutomationHandler daHandler = new DesignAutomationHandler();

            var ret = daHandler.RunDesignAutomationForgeWorkflow(json, InventorFilePath);
            ret.Wait();
        }

        [TestMethod]
        public void UnitTest_Libraries_DH_Assembly_Floor_Panel_9_0_Zip_Tests()
        {
            NLogger.Initialize();

            //  PLEASE NOTE:THE CONFIGURATION FILE MUST BE COPIED IN FOLDER "CONFIGURATION OF UNIT TESTS"
            ConfigUtilities.LoadConfig();

            string json = GetJson_Floor_Panel();
            string InventorFilePath = @"C:\Users\parodiaadmin\BIM 360\ATDSK DEV\Sample Project\Project Files\Libraries_DH_Assembly_Floor_Panel_9.0_zip";
            DesignAutomationHandler daHandler = new DesignAutomationHandler();

            var ret = daHandler.RunDesignAutomationForgeWorkflow(json, InventorFilePath);
            ret.Wait();
        }


        private string GetJson_Unit_Frame()
        {
            string json = "{ " +
                  "'ILogicParams': [" +
                    "{" +
                      "'RevitFamily': 'Frame_Assy1: Standard'," +
                      //"'InventorTemplate': 'Unit_Frame_Package_01.zip'," +
                      "'InventorTemplate': 'Frame_Assy1.zip'," +
                      "'ParametersInfo': [" +
                        "{" +
                          "'elementName': ''," +
                          "'elementId': '345678'," +
                          "'paramsValues': {" +
                                "'Height': '400'," +
                                "'Length': '500'," +
                                "'Width': '500'" +
                            "}" +
                          "}" +
                        "]" +
                      "}" +
                    "]" +
                "}";

            return json;
        }

        private string GetJson_Window()
        {
            string json = "{ " +
                  "'ILogicParams': [" +
                    "{" +
                      "'RevitFamily': 'Frame_Assy1: Standard'," +
                      "'InventorTemplate': 'Window_Package.zip'," +

                      "'ParametersInfo': [" +
                        "{" +
                          "'elementName': ''," +
                          "'elementId': '345678'," +
                          "'paramsValues': { " +
                                "'Height': '3000'," +
                                "'Width': '2000'" +
                            "}" +
                          "}" +
                        "]" +
                      "}" +
                    "]" +
                "}";

            return json;
        }

        private string GetJson_Floor_Panel()
        {
            string json = "{ " +
                  "'ILogicParams': [" +
                    "{" +
                      "'RevitFamily': 'Frame_Assy1: Standard'," +
                      "'InventorTemplate': 'Floor Panel.zip'," +
                      "'ParametersInfo': [" +
                        "{" +
                          "'elementName': ''," +
                          "'elementId': '627668'," +
                          "'paramsValues': {" +
                                "'Floor_X_Width': '1820'," +
                                "'Floor_Y_Length': '3840'" +
                            "}" +
                          "}" +
                        "]" +
                      "}" +
                    "]" +
                "}";

            return json;
        }
    }
}
