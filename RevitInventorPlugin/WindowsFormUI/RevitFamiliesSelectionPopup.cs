using RevitInventorExchange.CoreDataStructures;
using RevitInventorExchange.WindowsFormBusinesslayer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RevitInventorExchange.WindowsFormUI
{
    public partial class RevitFamiliesSelectionPopup : Form
    {
        private IList<ElementStructure> elStructureList;
        private RevitFamiliesSelectionPopupHandler revitFamiliesSelectionHandler;
        List<RevitFamiliesDataGridSourceData> elementList = null;

        public string SelectedRevitFamily = "";

        public RevitFamiliesSelectionPopup(IList<ElementStructure> elementStructureList)
        {
            InitializeComponent();
            elStructureList = elementStructureList;
            revitFamiliesSelectionHandler = new RevitFamiliesSelectionPopupHandler();
        }

        private void RevitFamiliesSelectionPopup_Load(object sender, EventArgs e)
        {
            InitializeRevitFamiliesGrid(dgRevitFamilies);

            // Populate the Element datagrid
            var dataSources = revitFamiliesSelectionHandler.GetRevitFamiliesDataGridSource(elStructureList);

            elementList = dataSources["RevitFamilies"];

            revitFamiliesSelectionHandler.FillPropertiesGrid(dgRevitFamilies, elementList);
        }

        private void InitializeRevitFamiliesGrid(DataGridView dataGrid)
        {
            dataGrid.AutoGenerateColumns = false;

            var colRevitFam = new DataGridViewTextBoxColumn();
            colRevitFam.ReadOnly = true;
            colRevitFam.Name = "Revit Family";
            colRevitFam.DataPropertyName = "RevitFamily";
            colRevitFam.SortMode = DataGridViewColumnSortMode.Automatic;
           
            dataGrid.Columns.Add(colRevitFam);
            
            dataGrid.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        //  Pass back to the mapping form the selectd Revit family
        private void btnSelect_Click(object sender, EventArgs e)
        {
            HandleRowSelection();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        private void dgRevitFamilies_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            HandleRowSelection();
        }

        private void HandleRowSelection()
        {
            var revitFamiliesRowCount = dgRevitFamilies.Rows.GetRowCount(DataGridViewElementStates.Selected);

            if (revitFamiliesRowCount > 0)
            {
                var selRevFam = dgRevitFamilies.SelectedRows[0].Cells[0].Value.ToString();
                SelectedRevitFamily = selRevFam;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
