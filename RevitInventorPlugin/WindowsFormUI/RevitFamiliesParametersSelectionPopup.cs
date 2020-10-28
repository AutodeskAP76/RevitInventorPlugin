using RevitInventorExchange.Data;
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
    public partial class RevitFamiliesParametersSelectionPopup : Form
    {
        private RevitFamiliesParamsSelectionPopupHandler revitFamiliesParamsSelectionHandler;
        List<RevitFamiliesParamsDataGridSourceData> elementList = null;
        private IList<ElementStructure> elStructureList;
        private IList<string> revitUsedParams;

        private string revitFamily = "";

        public string SelectedRevitFamilyParam = "";

        public RevitFamiliesParametersSelectionPopup(string RevitFamily, IList<ElementStructure> elementStructureList, IList<string> RevitUsedParams)
        {
            InitializeComponent();

            elStructureList = elementStructureList;
            revitFamily = RevitFamily;
            revitFamiliesParamsSelectionHandler = new RevitFamiliesParamsSelectionPopupHandler();
            revitUsedParams = RevitUsedParams;
        }

        private void RevitFamiliesParametersSelectionPopup_Load(object sender, EventArgs e)
        {
            InitializeRevitFamiliesParamsGrid(dgRevitFamParams);

            // Populate the datagrid
            var dataSources = revitFamiliesParamsSelectionHandler.GetRevitFamiliesDataGridSource(revitFamily, elStructureList);

            elementList = dataSources["RevitFamilyParams"];

            revitFamiliesParamsSelectionHandler.FillGrid(dgRevitFamParams, elementList);
        }

        private void InitializeRevitFamiliesParamsGrid(DataGridView dataGrid)
        {
            dataGrid.AutoGenerateColumns = false;

            var colRevitFam = new DataGridViewTextBoxColumn();
            colRevitFam.ReadOnly = true;
            colRevitFam.Name = "Revit Family Parameters";
            colRevitFam.DataPropertyName = "RevitFamilyParam";
            colRevitFam.SortMode = DataGridViewColumnSortMode.Automatic;

            dataGrid.Columns.Add(colRevitFam);

            dataGrid.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            HandleRowSelection();
        }

        private void dgRevitFamParams_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            HandleRowSelection();
        }

        private void HandleRowSelection()
        {
            var revitFamiliesRowCount = dgRevitFamParams.Rows.GetRowCount(DataGridViewElementStates.Selected);

            if (revitFamiliesRowCount == 1)
            {
                var selRevFamParam = dgRevitFamParams.SelectedRows[0].Cells["Revit Family Parameters"].Value.ToString();
                SelectedRevitFamilyParam = selRevFamParam;

                var alreadyAssigned = revitUsedParams.Any(l => l == selRevFamParam);

                if (alreadyAssigned)
                {
                    MessageBox.Show("The selected Revit property has already been assigned. Please select another one.");

                    return;
                }
            }
            else
            {
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                MessageBox.Show("You have to select one row", "", buttons);

                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }       
    }
}
