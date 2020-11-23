using Newtonsoft.Json.Linq;
using RevitInventorExchange.CoreDataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitInventorExchange.CoreBusinessLayer
{
    public enum BIM360Type
    {
        Hub,
        Project,
        Folder,
        File,
        FileVersion,
        StorageObject
    }
    /// <summary>
    /// The class handles the creation of BIM360 structure
    /// </summary>
    public class BIM360StructureBuilder
    {        
        public BIM360DocsStructure1 bIM360DocsStructure1 = null;

        public BIM360StructureBuilder()
        {            
            bIM360DocsStructure1 = new BIM360DocsStructure1();
            bIM360DocsStructure1.BIM360DataRows1 = new List<BIM360DocsRowStructure>();
        }

        //  Extract from returned json the hub info
        internal string SetHubStructure(ForgeRestResponse ret)
        {
            NLogger.LogText("Entered SetHubStructure");

            string hubId = "";
            string configuredHub = ConfigUtilities.GetHub();

            NLogger.LogText("Parse ResponseContent");
            JObject res = JObject.Parse(ret.ResponseContent);
            var data = res.SelectTokens("$.data").Children();

            NLogger.LogText("Build bIM360DocsStructure1");
            foreach (var item in data)
            {
                var name = ((string)item.SelectToken("attributes.name"))/*.Replace("\u200B", "")*/;

                if (name == configuredHub/*.Replace("\u200B", "")*/)
                {
                    hubId = (string)item.SelectToken("id");

                    //bIM360DocsStructure.Hub = new Hub { Id = hubId, Name = name};
                    bIM360DocsStructure1.BIM360DataRows1.Add(new BIM360DocsRowStructure
                    {
                        Id = hubId,
                        Name = name,
                        Type = BIM360Type.Hub
                    });
                }
            }

            if (string.IsNullOrEmpty(hubId))
            {
                throw new Exception($"The configured Hub '{configuredHub}' is not in the returned list");
            }

            NLogger.LogText("Exit SetHubStructure");

            return hubId;
        }

        //  Extract from returned json the project info
        internal string SetProjectStructure(ForgeRestResponse ret, string parentId)
        {
            NLogger.LogText("Entered SetProjectStructure");

            string projectId = "";
            JObject res = JObject.Parse(ret.ResponseContent);
            var data = res.SelectTokens("$.data").Children();

            foreach (var item in data)
            {
                var name = ((string)item.SelectToken("attributes.name"));

                if (name == ConfigUtilities.GetProject())
                {
                    projectId = (string)item.SelectToken("id");

                    //bIM360DocsStructure.Hub.ProjectList = new List<Project> { new Project { Id = projectId, Name = name } };

                    bIM360DocsStructure1.BIM360DataRows1.Add(new BIM360DocsRowStructure
                    {
                        ParentId = parentId,
                        Id = projectId,
                        Name = name,
                        Type = BIM360Type.Project
                    });
                }
            }

            NLogger.LogText("Exit SetProjectStructure");

            return projectId;
        }

        //  Extract from returned json the subfolder info
        internal string SetFolderStructure(ForgeRestResponse ret, string parentId, string folderName)
        {
            NLogger.LogText("Entered SetFolderStructure");

            string topFolderId = "";
            JObject res = JObject.Parse(ret.ResponseContent);
            var data = res.SelectTokens("$.data").Children();

            foreach (var item in data)
            {
                var name = ((string)item.SelectToken("attributes.name"));

                if (name == folderName)
                {
                    topFolderId = (string)item.SelectToken("id");

                    //bIM360DocsStructure.Hub.ProjectList.First(o => o.Id == parentId).TopFolder = new Folder { Id = topFolderId, Name = name };

                    bIM360DocsStructure1.BIM360DataRows1.Add(new BIM360DocsRowStructure
                    {
                        ParentId = parentId,
                        Id = topFolderId,
                        Name = name,
                        Type = BIM360Type.Folder
                    });
                }
            }

            NLogger.LogText("Exit SetFolderStructure");

            return topFolderId;
        }


        internal string SetFileStructure(ForgeRestResponse ret, string parentId, string fileName)
        {
            NLogger.LogText("Enter SetFileStructure");

            string fileId = "";
            JObject res = JObject.Parse(ret.ResponseContent);
            var data = res.SelectTokens("$.data").Children();
            var included = res.SelectTokens("$.included").Children();

            //  Extract information on items
            foreach (var item in data)
            {
                var name = ((string)item.SelectToken("attributes.displayName"));
                var type = ((string)item.SelectToken("type"));

                if (type == "items")
                {
                    fileId = (string)item.SelectToken("id");

                    bIM360DocsStructure1.BIM360DataRows1.Add(new BIM360DocsRowStructure
                    {
                        ParentId = parentId,
                        Id = fileId,
                        Name = name,
                        Type = BIM360Type.File
                    });
                }
            }

            //  Extract information on item versions and corresponding Storage objects
            foreach (var item in included)
            {
                var name = ((string)item.SelectToken("attributes.name"));
                var type = ((string)item.SelectToken("type"));

                if (type == "versions")
                {
                    var parentItemId = ((string)item.SelectToken("relationships.item.data.id"));
                    var versionId = (string)item.SelectToken("id");                    

                    bIM360DocsStructure1.BIM360DataRows1.Add(new BIM360DocsRowStructure
                    {
                        ParentId = parentItemId,
                        Id = versionId,
                        Name = name,
                        Type = BIM360Type.FileVersion
                    });

                    var storageObjectId = ((string)item.SelectToken("relationships.storage.data.id"));

                    bIM360DocsStructure1.BIM360DataRows1.Add(new BIM360DocsRowStructure
                    {
                        ParentId = versionId,
                        Id = storageObjectId,
                        Type = BIM360Type.StorageObject
                    });
                }
            }

            NLogger.LogText("Exit SetFileStructure");

            return fileId;
        }

        //  Extracts the StorageObject given its file name
        internal string GetObjectStorageByFileName(string elName)
        {
            var file = bIM360DocsStructure1.BIM360DataRows1.FirstOrDefault(o => o.Name == elName);
            var fileversion = bIM360DocsStructure1.BIM360DataRows1.FirstOrDefault(o => o.ParentId == file.Id);
            var objectStorage = bIM360DocsStructure1.BIM360DataRows1.FirstOrDefault(o => o.ParentId == fileversion.Id);

            return objectStorage.Id;
        }

        //  Extract folder given its name
        internal string GetFolderIdByName(string elName)
        {
            var folder = bIM360DocsStructure1.BIM360DataRows1.FirstOrDefault(o => o.Name == elName && o.Type == BIM360Type.Folder);

            return folder.Id;
        }
    }
}
