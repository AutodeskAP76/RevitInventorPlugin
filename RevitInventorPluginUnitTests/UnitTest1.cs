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
            ConfigUtilities.LoadConfig();

            string json = GetJson();
            DesignAutomationHandler daHandler = new DesignAutomationHandler();

            daHandler.RunDesignAutomationForgeWorkflow(json);


        }

        private string GetJson()
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
