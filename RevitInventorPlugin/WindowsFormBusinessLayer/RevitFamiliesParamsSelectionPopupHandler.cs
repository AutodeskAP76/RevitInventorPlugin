using RevitInventorExchange.Data;
using RevitInventorExchange.CoreDataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RevitInventorExchange.WindowsFormBusinesslayer
{
    public class RevitFamiliesParamsSelectionPopupHandler : BaseFormHandler
    {
        //  create the datasource for datagrid
        public Dictionary<string, List<RevitFamiliesParamsDataGridSourceData>> GetRevitFamiliesDataGridSource(string selRevFamily, List<ElementStructure> elementStructureList)
        {
            var ret = new Dictionary<string, List<RevitFamiliesParamsDataGridSourceData>>();
            var sourceData = new List<RevitFamiliesParamsDataGridSourceData>();

            var selRevFamilyFileName = Utilities.GetStringForFolderName(selRevFamily);

            //  Load corresponding Revit family parameter file and set datasource for dropdown list
            var revitFamilySource = GetRevitParameters(selRevFamilyFileName);

            //  If the Revit properties filter file is present then its content is loaded, otherwise all properties are shown
            if (revitFamilySource.Count > 0)
            {
                sourceData = revitFamilySource.Select(o => new RevitFamiliesParamsDataGridSourceData { RevitFamilyParam = o }).ToList();
            }
            else
            {
                var revitEl = elementStructureList.First(o => o.ElementTypeSingleParameters.Any(l => l.ParameterName == "SYMBOL_FAMILY_AND_TYPE_NAMES_PARAM" && l.ParameterValue == selRevFamily))
                    .ElementTypeOrderedParameters;

                sourceData = revitEl.Select(o => new RevitFamiliesParamsDataGridSourceData { RevitFamilyParam = o.ParameterName }).ToList();
            }            

            ret.Add("RevitFamilyParams", sourceData);

            return ret;
        }

        public List<string> GetRevitParameters(string revitFileName)
        {
            var revitParameters = new List<string>();

            var doc = this.LoadDocument(revitFileName);

            if (doc != null)
            {
                var nodes = doc.SelectNodes("filters/property/@name");

                foreach (XmlNode node in nodes)
                {
                    revitParameters.Add(node.Value);
                }
            }

            return revitParameters;
        }
    }
}
