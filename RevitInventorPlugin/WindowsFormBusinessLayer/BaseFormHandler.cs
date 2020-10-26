using RevitInventorExchange.Data;
using RevitInventorExchange.CoreBusinessLayer;
using RevitInventorExchange.CoreDataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace RevitInventorExchange.WindowsFormBusinesslayer
{
    public class BaseFormHandler
    {
        private RevitFiltersHandler revitFilterHandler = null;

        public BaseFormHandler()
        {
            revitFilterHandler = new RevitFiltersHandler();
        }

        public void FillPropertiesGrid(DataGridView dgParam, dynamic dataSource)
        {
            dgParam.DataSource = null;
            dgParam.DataSource = dataSource;
        }

        public void FillComboRevitFamTypes(ComboBox comboBoxRevitFamilies, dynamic dataSource, string displayMember)
        {
            comboBoxRevitFamilies.DataSource = dataSource;
            comboBoxRevitFamilies.DisplayMember = displayMember;
        }

        public IList<ElementStructure> FilterElements(IList<ElementStructure> elStructList)
        {
            var filteredElStruct = revitFilterHandler.FilterElements(elStructList);
            return filteredElStruct;
        }

        public ElementStructure FilterElements(ElementStructure elStruct)
        {
            List<ElementStructure> elStructList = new List<ElementStructure>();
            elStructList.Add(elStruct);

            var filteredElStruct = FilterElements(elStructList);

            return filteredElStruct.FirstOrDefault();
        }

        public XmlDocument LoadDocument(string fileName)
        {
            return revitFilterHandler.LoadFile(fileName);
        }
    }
}
