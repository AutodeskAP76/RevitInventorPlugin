using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RevitInventorExchange.CoreDataStructures
{
    public class RevitFamiliesStructure
    {
        private IList<RevitFamily> revitFamilies = null;

        internal IList<RevitFamily> RevitFamilies { get => revitFamilies; set => revitFamilies = value; }

        static XmlDocument doc = new XmlDocument();

        public RevitFamiliesStructure()
        {
            revitFamilies = new List<RevitFamily>();
        }

        //  Load the RevitFamiliesConfig.xml containing info on RevitFamilyTypes to use for filter revit elements
        private void LoadConfig()
        {
            NLogger.LogText("Entered LoadConfig");

            try
            {
                var folderBaseLine = Utilities.GetFolderBaseline();
                var configPath = System.IO.Path.Combine(folderBaseLine, "Configuration\\RevitFamiliesConfig.xml");

                doc.Load(configPath);

                NLogger.LogText("Exit LoadConfig success");
            }
            catch (Exception ex)
            {
                NLogger.LogError(ex);
                NLogger.LogText("Exit LoadConfig with error");

                throw (ex);
            }
        }

        //  Build an internal structure from RevitFamiliesConfig.xml file
        public void LoadStructure()
        {
            NLogger.LogText("Entered LoadStructure");
            
            LoadConfig();

            try
            {
                foreach (XmlNode node in doc.SelectNodes("/Configuration/RevitFamilies/item"))
                {
                    revitFamilies.Add(new RevitFamily
                    {                        
                        Id = node.Attributes["id"].Value,
                        Name = node.Attributes["name"].Value,
                        Type = node.Attributes["type"].Value,
                        Instance = node.Attributes["instance"].Value,                                                
                        Category = node.Attributes["category"].Value
                    });
                }

                NLogger.LogText("Exit LoadStructure");
            }
            catch(Exception ex)
            {
                NLogger.LogError(ex);
                NLogger.LogText("Exit LoadStructure with error");

                throw (ex);
            }
        }
    }

    public class RevitFamily
    {
        private string id;
        private string name;
        private string type;
        private string instance;
        private string category;

        public string Id { get => id; set => id = value; }
        public string Name { get => name; set => name = value; }
        public string Type { get => type; set => type = value; }
        public string Instance { get => instance; set => instance = value; }
        public string Category { get => category; set => category = value; }
    }
}
