using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevitInventorExchange.CoreBusinessLayer;
using RevitInventorExchange;
using System.Threading.Tasks;

namespace RevitInventorPluginUnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            NLogger.Initialize();

            //  PLEASE NOTE:THE CONFIGURATION FILE MUST BE COPIED IN FOLDER "CONFIGURATION OF UNIT TESTS"
            ConfigUtilities.LoadConfig();

            string json = GetJson();
            string InventorFilePath = @"C:\Users\parodiaadmin\BIM 360\ATDSK DEV\Sample Project\Project Files\Libraries_DH_Assembly_wall";
            DesignAutomationHandler daHandler = new DesignAutomationHandler();

            daHandler.RunDesignAutomationForgeWorkflow(json, InventorFilePath);
        }

        private string GetJson()
        {
            string json = "{ " +
                  "'ILogicParams': [" +
                    "{" +
                      "'RevitFamily': 'Wall Panel_2.iam (Window_Ref).0001: Standard'," +
                      "'InventorTemplate': 'P Board.ipt'," +
                      //"'InventorTemplate': 'Wall Panel.iam'," +
                      //"'InventorTemplate': 'Wall_Assembly_Package.zip'," +

                      "'ParametersInfo': [" +
                        "{" +
                          "'elementName': ''," +
                          "'elementId': '345678'," +
                          "'paramsValues': { " +
                                "'Window_LeftRef': '750'" +
                            "}" +
                          "}" +
                        "]" +
                      "}" +
                    "]" +
                "}";

            return json;
        }

        private string GetJson_OLD()
        {
            string json = "{ " +
                  "'ILogicParams': [" +
                    "{" +
                      "'RevitFamily': 'DH_開口貫通_4P: TYPE_一般壁_2P'," +
                      "'InventorTemplate': 'box.ipt'," +
                      "'paramsValues': { " +
                            "'length': '150'," +
                            "'width': '150'," +
                            "'height': '100'" +
                      "}" +
                     "}" +
                  "]" +
                "}";

            return json;
        }
    }
}
