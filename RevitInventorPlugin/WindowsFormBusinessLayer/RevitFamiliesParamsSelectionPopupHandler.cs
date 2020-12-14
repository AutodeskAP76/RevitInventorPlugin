using RevitInventorExchange.Data;
using RevitInventorExchange.CoreDataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using RevitInventorExchange.Utilities;

namespace RevitInventorExchange.WindowsFormBusinesslayer
{
    public class RevitFamiliesParamsSelectionPopupHandler : BaseFormHandler
    {
        //  create the datasource for datagrid
        public Dictionary<string, List<RevitFamiliesParamsDataGridSourceData>> GetRevitFamiliesDataGridSource(string selRevFamily, IList<ElementStructure> elementStructureList)
        {
            var ret = new Dictionary<string, List<RevitFamiliesParamsDataGridSourceData>>();
            var sourceData = new List<RevitFamiliesParamsDataGridSourceData>();

            var selRevFamilyFileName = Utility.GetStringForFolderName(selRevFamily);

            //  Load corresponding Revit family parameter file and set datasource for dropdown list
            var revitFamilySource = GetRevitParameters(selRevFamilyFileName);

            //  If the Revit properties filter file is present then its content is loaded, otherwise all properties are shown
            if (revitFamilySource.Count > 0)
            {
                sourceData = revitFamilySource.Select(o => new RevitFamiliesParamsDataGridSourceData { RevitFamilyParam = o }).ToList();
            }
            else
            {               
                //  ADDED CODE FOR INCLUDING ALSO ELEMENT PARAMETERS
                var revitElParams = elementStructureList.First(o => o.ElementTypeSingleParameters.Any(l => l.ParameterName == "SYMBOL_FAMILY_AND_TYPE_NAMES_PARAM" && l.ParameterValue == selRevFamily))
                    .ElementOrderedParameters;

                sourceData.AddRange(revitElParams.Select(o => new RevitFamiliesParamsDataGridSourceData { RevitFamilyParam = o.ParameterName }).ToList());

                var revitElTypeParams = elementStructureList.First(o => o.ElementTypeSingleParameters.Any(l => l.ParameterName == "SYMBOL_FAMILY_AND_TYPE_NAMES_PARAM" && l.ParameterValue == selRevFamily))
                   .ElementTypeOrderedParameters;

                //sourceData = revitElTypeParams.Select(o => new RevitFamiliesParamsDataGridSourceData { RevitFamilyParam = o.ParameterName }).ToList();
                sourceData.AddRange(revitElTypeParams.Select(o => new RevitFamiliesParamsDataGridSourceData { RevitFamilyParam = o.ParameterName }).ToList());
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
