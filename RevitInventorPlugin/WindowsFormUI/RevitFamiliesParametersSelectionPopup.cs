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
using RevitInventorExchange.CoreBusinessLayer;

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
            InitializeLanguage();

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
            colRevitFam.HeaderText = LanguageHandler.GetString("dgParamsMapping_Col_RevitParam_Text");
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
                
                //  Check if selected parameter has already been assigned
                var alreadyAssigned = revitUsedParams.Any(l => l == selRevFamParam);

                if (alreadyAssigned)
                {
                    //MessageBox.Show("The selected Revit property has already been assigned. Please select another one.");
                    MessageBox.Show(LanguageHandler.GetString("msgBox_SelPropAlreadyAssigned"));

                    return;
                }

                SelectedRevitFamilyParam = selRevFamParam;
            }
            else
            {
                //MessageBox.Show("You have to select one row");
                MessageBox.Show(LanguageHandler.GetString("msgBox_SelOneRow"));

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

        private void InitializeLanguage()
        {
            this.Text = LanguageHandler.GetString("RevInventorParam_Popup_Text");
            grBoxSelParams.Text = LanguageHandler.GetString("grBoxSelParams_Text");
            btnSelect.Text = LanguageHandler.GetString("btnSelect_Text");
            btnClose.Text = LanguageHandler.GetString("btnClose_Text");
        }
    }
}
