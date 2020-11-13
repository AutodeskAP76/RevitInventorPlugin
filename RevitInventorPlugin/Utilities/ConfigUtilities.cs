using RevitInventorExchange.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RevitInventorExchange
{
    public static class ConfigUtilities
    {
        static XmlDocument doc = new XmlDocument();

        public static void LoadConfig()
        {
            NLogger.LogText("Entered LoadConfig");

            try
            {
                var folderBaseLine = Utility.GetFolderBaseline();
                var configPath = System.IO.Path.Combine(folderBaseLine, "Configuration\\Config.xml");

                doc.Load(configPath);

                NLogger.LogText("Exit LoadConfig success");
            }
            catch(Exception ex)
            {
                NLogger.LogError(ex);
                NLogger.LogText("Exit LoadConfig");
            }            
        }

        public static string GetDevMode()
        {
            return doc.SelectSingleNode("/Configuration/key[@name='DEVMODE']/@value").Value;
        }

        public static string GetLanguage()
        {
            return doc.SelectSingleNode("/Configuration/key[@name='Application']/key[@name='Language']/@value").Value;
        }

        public static string GetHub()
        {
            NLogger.LogText("Entered GetHub method");

            var value = doc.SelectSingleNode("/Configuration/key[@name='BIM360']/key[@name='HUB']/@value").Value;

            NLogger.LogText("GetHub - Hub {hub}: ", value);

            return value;
        }

        public static string GetProject()
        {
            NLogger.LogText("Entered GetProject method");

            var value = doc.SelectSingleNode("/Configuration/key[@name='BIM360']/key[@name='Project']/@value").Value;

            NLogger.LogText("GetProject - Project {proj}: ", value);

            return value;
        }

        public static string GetInventorTemplateFolder()
        {
            return doc.SelectSingleNode("/Configuration/key[@name='BIM360']/key[@name='InventorTemplateFolder']/@value").Value;
        }

        public static string GetClientID()
        {
            return doc.SelectSingleNode("/Configuration/key[@name='Forge']/key[@name='ClientID']/@value").Value;
        }

        public static string GetClientSecret()
        {
            return doc.SelectSingleNode("/Configuration/key[@name='Forge']/key[@name='ClientSecret']/@value").Value;
        }


        public static int GetAsyncHTTPCallWaitTime()
        {
            return Convert.ToInt32(doc.SelectSingleNode("/Configuration/key[@name='Forge']/key[@name='HTTPAsyncCallsWaitingTime']/@value").Value);
        }


        public static int GetAsyncHTTPCallRetryWaitTime()
        {
            return Convert.ToInt32(doc.SelectSingleNode("/Configuration/key[@name='Forge']/key[@name='HTTPRetryAsyncCallsWaitingTime']/@value").Value);
        }

        public static int GetAsyncHTTPCallNumberOfRetries()
        {
            return Convert.ToInt32(doc.SelectSingleNode("/Configuration/key[@name='Forge']/key[@name='HTTPRetryAsyncCallsNumberOfRetries']/@value").Value);
        }


        public static string GetWorkItemCreationPollingTime()
        {
            return doc.SelectSingleNode("/Configuration/key[@name='Forge']/key[@name='WorkItemCreationPollingTime']/@value").Value;
        }

        public static string GetDABaseURL()
        {
            return doc.SelectSingleNode("/Configuration/key[@name='Forge']/key[@name='DABaseURL']/@value").Value;
        }

        public static string GetDALinkBaseURL()
        {
            return doc.SelectSingleNode("/Configuration/key[@name='Forge']/key[@name='DABaseLinkURL']/@value").Value;
        }

        public static string GetDMBaseProjectURL()
        {
            return doc.SelectSingleNode("/Configuration/key[@name='Forge']/key[@name='DMBaseProjectURL']/@value").Value;
        }

        public static string GetDMBaseDataURL()
        {
            return doc.SelectSingleNode("/Configuration/key[@name='Forge']/key[@name='DMBaseDataURL']/@value").Value;
        }

        public static string GetSaveParamValuesToFile()
        {
            return doc.SelectSingleNode("/Configuration/key[@name='DEVMODE']/key[@name='SaveRevitParamsValueFile']/@value").Value;
        }

        public static string GetDAPartActivity()
        {
            return doc.SelectSingleNode("/Configuration/key[@name='Forge']/key[@name='PartActivityId']/@value").Value;
        }

        public static string GetDAAssemblyActivity()
        {
            return doc.SelectSingleNode("/Configuration/key[@name='Forge']/key[@name='AssemblyActivityId']/@value").Value;
        }

        public static string GetDAWorkItemDocInputArgument()
        {
            return doc.SelectSingleNode("/Configuration/key[@name='Forge']/key[@name='RequestDocInputArgName']/@value").Value;
        }

        public static string GetDAWorkItemParamsInputArgument()
        {
            return doc.SelectSingleNode("/Configuration/key[@name='Forge']/key[@name='RequestParamInputArgName']/@value").Value;
        }

        public static string GetDAWorkItemParamsOutputIpt()
        {
            return doc.SelectSingleNode("/Configuration/key[@name='Forge']/key[@name='RequestParamOutputIpt']/@value").Value;
        }

        public static string GetDAWorkItemParamsOutputIam()
        {
            return doc.SelectSingleNode("/Configuration/key[@name='Forge']/key[@name='RequestParamOutputIam']/@value").Value;
        }
    }
}
