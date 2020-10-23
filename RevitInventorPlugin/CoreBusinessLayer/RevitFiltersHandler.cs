using Autodesk.Revit.DB.Structure;
using RevitInventorExchange.CoreDataStructures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace RevitInventorExchange.CoreBusinessLayer
{    
    public class RevitFiltersHandler
    {
        Dictionary<string, XmlDocument> filters = null;
        bool filterEnabled = false;

        public bool FilterEnabled { get => filterEnabled; set => filterEnabled = value; }

        //  For each element type in the passed structure, get the configuration file and filter against it
        public IList<ElementStructure> FilterElements(IList<ElementStructure> elStruct)
        {
            var elementParams = elStruct;

            //  Get filter files: names are based on family Type
            //var filterFiles = elStruct.Select(o => Utilities.GetStringForFolderName(o.ElementTypeSingleParameters.SingleOrDefault(p => p.ParameterName == "SYMBOL_FAMILY_AND_TYPE_NAMES_PARAM").ParameterValue))
            //    .Distinct().ToList();

            var filterFiles = elStruct.Select(o => Utilities.GetFilterOnPropFile(o)).Distinct().ToList();

            LoadFilters(filterFiles);

            //  loop on all elements in the passed structure to filter each element properties againt the corresponding filter file
            foreach (var el in elStruct)
            {
                var fileName = Utilities.GetFilterOnPropFile(el);// el.ElementType.FamilyName;

                loopOnProperties(fileName, el.ElementOrderedParameters);
                loopOnProperties(fileName, el.ElementTypeOrderedParameters);               
            }                      

            return elementParams;
        }      

        //  Load filtering files
        private void LoadFilters(List<string> filterFileNames)
        {
            filterEnabled = true;

            filters = new Dictionary<string, XmlDocument>();
            
            foreach (var filterFileName in filterFileNames)
            {
                var doc = LoadFile(filterFileName);

                if (doc != null)
                {
                    filters.Add(filterFileName, doc);
                }
            }
        }

        private void loopOnProperties(string fileName, List<ElementOrderedParameter> elementOrdParams)
        {
            //  loop on all properties in the given element
            foreach (var prop in elementOrdParams)
            {
                string propName = prop.ParameterName;

                if (this.checkValue(fileName, propName))
                {
                    prop.TransferParam = true;
                }
                else
                {
                    prop.TransferParam = false;
                }
            }
        }

        //  Filter parameter value against the XMl fintering file
        public bool checkValue(string elementName, string value)
        {
            bool ret = false;

            if (filterEnabled)
            {                
                if (filters.ContainsKey(elementName))
                {
                    var f = filters[elementName].SelectNodes("filters/property[@name='" + value + "']");
                    ret = f.Count > 0;
                }
            }
            else
            { 
                ret = true; 
            }

            return ret;
        }





        public XmlDocument LoadFile(string fileName)
        {
            //  Determine the xml file path. This is needed as, using relative paths "..\..\" in some cases the opened document path is returned and not the Assemlby one
            var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            var TwoUp = Path.GetDirectoryName(Path.GetDirectoryName(assemblyFolder));


            var xmlURI = Path.Combine(TwoUp, "FilteringFiles\\" + fileName + ".xml");
            var xmlPath = new Uri(xmlURI).LocalPath;

            if (System.IO.File.Exists(xmlPath))
            {
                var doc = new XmlDocument();
                doc.Load(xmlPath);

                return doc;
            }

            return null;
        }



        // TODO: MOVE TO A MORE APPROPRIATE CLASS
        //  Create filter file based on selected properties in the Revit Plugin
        public void createParametersFilterFile(string xmlPath, Dictionary<string, DataGridViewSelectedRowCollection> rows)
        {         
            using (XmlWriter xmlWriter = XmlWriter.Create(xmlPath))
            {
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("filters");

                //  handle element grid selected rows
                var elemPropSelectedRow = rows["ElementProperties"];

                if (elemPropSelectedRow.Count > 0)
                {
                    for (int i = 0; i < elemPropSelectedRow.Count; i++)
                    {
                        var paramName = elemPropSelectedRow[i].Cells["Property Name"].Value.ToString();
                        //var paramValue = elemPropSelectedRow[i].Cells["Property Value"].Value.ToString();

                        xmlWriter.WriteStartElement("property");
                        xmlWriter.WriteAttributeString("name", paramName);
                        xmlWriter.WriteEndElement();
                    }
                }

                //  handle element type grid selected rows
                var elemTypeSelectedRow = rows["ElementTypeProperties"];

                if (elemTypeSelectedRow.Count > 0)
                {
                    for (int i = 0; i < elemTypeSelectedRow.Count; i++)
                    {
                        var paramName = elemTypeSelectedRow[i].Cells["Property Name"].Value.ToString();
                        //var paramValue = elemTypeSelectedRow[i].Cells["Property Value"].Value.ToString();
                        
                        xmlWriter.WriteStartElement("property");
                        xmlWriter.WriteAttributeString("name", paramName.ToString());
                        xmlWriter.WriteEndElement();
                    }
                }

                xmlWriter.WriteEndDocument();
                xmlWriter.Close();
            }
        }
    }
}
