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
        public void UnitTest_Libraries_DH_Assembly_Ceiling_Panel_2_0_Zip_Tests()
        {
            NLogger.Initialize();

            //  PLEASE NOTE:THE CONFIGURATION FILE MUST BE COPIED IN FOLDER "CONFIGURATION OF UNIT TESTS"
            ConfigUtilities.LoadConfig();

            string json = GetJson_Ceiling_Panel();
            string InventorFilePath = @"C:\Users\parodiaadmin\BIM 360\ATDSK DEV\Sample Project\Project Files\Libraries_DH_Assembly_Ceiling_Panel_2.0_zip";
            DesignAutomationHandler daHandler = new DesignAutomationHandler();

            var ret = daHandler.RunDesignAutomationForgeWorkflow(json, InventorFilePath);
            ret.Wait();
        }

        [TestMethod]
        public void UnitTest_Libraries_DH_Assembly_Ceiling_Panel_3_0_Zip_Tests()
        {
            NLogger.Initialize();

            //  PLEASE NOTE:THE CONFIGURATION FILE MUST BE COPIED IN FOLDER "CONFIGURATION OF UNIT TESTS"
            ConfigUtilities.LoadConfig();

            string json = GetJson_Ceiling_Panel();
            string InventorFilePath = @"C:\Users\parodiaadmin\BIM 360\ATDSK DEV\Sample Project\Project Files\Libraries_DH_Assembly_Ceiling_Panel_3.0_zip";
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
        public void UnitTest_Libraries_DH_Assembly_Unit_Frame_Zip_MultiOutputElement_Tests()
        {
            NLogger.Initialize();

            //  PLEASE NOTE:THE CONFIGURATION FILE MUST BE COPIED IN FOLDER "CONFIGURATION OF UNIT TESTS"
            ConfigUtilities.LoadConfig();

            string json = GetJson_Unit_Frame_MultiElement();
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


        [TestMethod]
        public void UnitTest_Libraries_DH_Assembly_Wall_Panel_1_0_Zip_Tests()
        {
            NLogger.Initialize();

            //  PLEASE NOTE:THE CONFIGURATION FILE MUST BE COPIED IN FOLDER "CONFIGURATION OF UNIT TESTS"
            ConfigUtilities.LoadConfig();

            string json = GetJson_Wall();
            string InventorFilePath = @"C:\Users\parodiaadmin\BIM 360\ATDSK DEV\Sample Project\Project Files\Libraries_DH_Assembly_Wall_1.0_zip";
            DesignAutomationHandler daHandler = new DesignAutomationHandler();

            var ret = daHandler.RunDesignAutomationForgeWorkflow(json, InventorFilePath);
            ret.Wait();
        }

        [TestMethod]
        public void UnitTest_Libraries_Assembly_Wheel_1_0_zip_Tests()
        {
            NLogger.Initialize();

            //  PLEASE NOTE:THE CONFIGURATION FILE MUST BE COPIED IN FOLDER "CONFIGURATION OF UNIT TESTS"
            ConfigUtilities.LoadConfig();

            string json = GetJson_Wheel();
            string InventorFilePath = @"C:\Users\parodiaadmin\BIM 360\ATDSK DEV\Sample Project\Project Files\Libraries_Assembly_Wheel_1.0_zip";
            DesignAutomationHandler daHandler = new DesignAutomationHandler();

            var ret = daHandler.RunDesignAutomationForgeWorkflow(json, InventorFilePath);
            ret.Wait();
        }







        [TestMethod]
        public void UnitTest_Libraries_DH_Assembly_Structure_LGS_Tests()
        {
            NLogger.Initialize();

            //  PLEASE NOTE:THE CONFIGURATION FILE MUST BE COPIED IN FOLDER "CONFIGURATION OF UNIT TESTS"
            ConfigUtilities.LoadConfig();

            string json = GetJson_Structure_LGS();
            string InventorFilePath = @"C:\Users\parodiaadmin\BIM 360\ATDSK DEV\Sample Project\Project Files\Libraries_DH_structure_LGS";
            DesignAutomationHandler daHandler = new DesignAutomationHandler();

            var ret = daHandler.RunDesignAutomationForgeWorkflow(json, InventorFilePath);
            ret.Wait();
        }

        private string GetJson_Structure_LGS()
        {
            string json = "{ " +
                  "'ILogicParams': [" +
                    "{" +
                      "'RevitFamily': 'Frame_Assy1: Standard'," +
                      //"'InventorTemplate': 'Unit_Frame_Package_01.zip'," +
                      "'InventorTemplate': 'structure_LGS.zip'," +
                      "'ParametersInfo': [" +
                        "{" +
                          "'elementName': ''," +
                          "'elementId': '345678'," +
                          "'paramsValues': {" +
                                "'height': '3500'," +
                                "'length': '4500'" +
                            "}" +
                          "}" +
                        "]" +
                      "}" +
                    "]" +
                "}";

            return json;
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

        private string GetJson_Unit_Frame_MultiElement()
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
                          "}," +

                          "{" +
                          "'elementName': ''," +
                          "'elementId': '500000'," +
                          "'paramsValues': {" +
                                "'Height': '800'," +
                                "'Length': '1000'," +
                                "'Width': '1000'" +
                            "}" +
                          "}," +

                          "{" +
                          "'elementName': ''," +
                          "'elementId': '600000'," +
                          "'paramsValues': {" +
                                "'Height': '1500'," +
                                "'Length': '2000'," +
                                "'Width': '2000'" +
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

        private string GetJson_Ceiling_Panel()
        {
            string json = "{ " +
                  "'ILogicParams': [" +
                    "{" +
                      "'RevitFamily': 'Frame_Assy1: Standard'," +
                      "'InventorTemplate': 'Ceiling Panel_Assy.zip'," +
                      "'ParametersInfo': [" +
                        "{" +
                          "'elementName': ''," +
                          "'elementId': '627668'," +
                          "'paramsValues': {" +
                                "'long_length': '3030'," +
                                "'short_length': '1010'" +
                            "}" +
                          "}" +
                        "]" +
                      "}" +
                    "]" +
                "}";

            return json;
        }

        private string GetJson_Wall()
        {
            string json = "{ " +
                  "'ILogicParams': [" +
                    "{" +
                      "'RevitFamily': 'Frame_Assy1: Standard'," +
                      "'InventorTemplate': 'wall_assy.zip'," +
                      "'ParametersInfo': [" +
                        "{" +
                          "'elementName': ''," +
                          "'elementId': '627668'," +
                          "'paramsValues': {" +
                                "'length': '3500'," +
                                "'height': '2500'" +
                            "}" +
                          "}" +
                        "]" +
                      "}" +
                    "]" +
                "}";

            return json;
        }


        private string GetJson_Wheel()
        {
            string json = "{ " +
                  "'ILogicParams': [" +
                    "{" +
                      "'RevitFamily': 'Frame_Assy1: Standard'," +
                      "'InventorTemplate': 'WheelAssembly.zip'," +
                      "'ParametersInfo': [" +
                        "{" +
                          "'elementName': ''," +
                          "'elementId': '627668'," +
                          "'paramsValues': {" +
                                "'WheelSize': '18'" +                                
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
