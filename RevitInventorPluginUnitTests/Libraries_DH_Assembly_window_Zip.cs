using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevitInventorExchange.CoreBusinessLayer;
using RevitInventorExchange;
using System.Threading.Tasks;

namespace RevitInventorPluginUnitTests
{
    [TestClass]
    public class Libraries_DH_Assembly_window_Zip

    {
        [TestMethod]
        public void TestMethod1()
        {
            NLogger.Initialize();

            //  PLEASE NOTE:THE CONFIGURATION FILE MUST BE COPIED IN FOLDER "CONFIGURATION OF UNIT TESTS"
            ConfigUtilities.LoadConfig();

            string json = GetJson();
            string InventorFilePath = @"C:\Users\parodiaadmin\BIM 360\ATDSK DEV\Sample Project\Project Files\Libraries_DH_Assembly_window_Zip";
            //string InventorFilePath = @"C:\Users\parodiaadmin\BIM 360\ATDSK DEV\Sample Project\Project Files\Libraries_DH_Assembly_unit_frame_zip";
            DesignAutomationHandler daHandler = new DesignAutomationHandler();

            daHandler.RunDesignAutomationForgeWorkflow(json, InventorFilePath);
        }

        private string GetJson()
        {
            string json = "{ " +
                  "'ILogicParams': [" +
                    "{" +
                      //"'RevitFamily': 'Wall Panel_2.iam (Window_Ref).0001: Standard'," +
                      "'RevitFamily': 'Frame_Assy1: Standard'," +
                      //"'InventorTemplate': 'Wall Panel.iam'," +
                      //"'InventorTemplate': 'Unit_Frame_Package_01.zip'," +
                      "'InventorTemplate': 'Window_package.zip'," +

                      "'ParametersInfo': [" +
                        "{" +
                          "'elementName': ''," +
                          "'elementId': '345678'," +
                          "'paramsValues': { " +
                                "'Height': '3000'," +
                                //"'Length': '3000'," +
                                "'Width': '2000'" +
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
