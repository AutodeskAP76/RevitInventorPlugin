using RevitInventorExchange.CoreDataStructures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RevitInventorExchange.CoreBusinessLayer
{
    public class ExportDataHandler
    {
        //  Create an xml with the content of Parameters values
        //public void ExportDataInXMLFile(string filepath, List<ElementStructure> elStructureList)
        //{
        //    //  Create the XMl file
        //    XmlWriter xmlWriter = XmlWriter.Create(filepath);

        //    //  Start add main tags
        //    xmlWriter.WriteStartDocument();
        //    xmlWriter.WriteStartElement("Properties");

        //    //  Loop on all selected elements
        //    foreach(var elStruct in elStructureList)
        //    {
        //        var Id = elStruct.Element.Id.IntegerValue;
        //        var elFamName = elStruct.ElementType.FamilyName;

        //        xmlWriter.WriteStartElement("Element");
        //        xmlWriter.WriteAttributeString("FamilyName", elFamName);
        //        xmlWriter.WriteAttributeString("Id", Id.ToString());
                
        //        //  Add element Parameters
        //        foreach (var prop in elStruct.ElementOrderedParameters.Where(k => k.TransferParam == true))
        //        {
        //            var paramName = prop.ParameterName;
        //            var paramValue = prop.ParameterValue;

        //            xmlWriter.WriteStartElement("Property");
        //            xmlWriter.WriteAttributeString("Value", paramValue);
        //            xmlWriter.WriteAttributeString("Name", paramName);
        //            xmlWriter.WriteEndElement();
        //        }

        //        //  Add Element type parameters
        //        foreach (var prop in elStruct.ElementTypeOrderedParameters.Where(k => k.TransferParam == true))
        //        {
        //            var paramName = prop.ParameterName;
        //            var paramValue = prop.ParameterValue;

        //            xmlWriter.WriteStartElement("Property");
        //            xmlWriter.WriteAttributeString("Value", paramValue);
        //            xmlWriter.WriteAttributeString("Name", paramName);
        //            xmlWriter.WriteEndElement();
        //        }

        //        xmlWriter.WriteEndElement();
        //    }

        //    xmlWriter.WriteEndDocument();
        //    xmlWriter.Close();
        //}

        public void ExportDataToJson(string path, string json)
        {
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }

            using (var tw = new StreamWriter(path, true))
            {
                tw.WriteLine(json);
                tw.Close();
            }
        }
    }
}
