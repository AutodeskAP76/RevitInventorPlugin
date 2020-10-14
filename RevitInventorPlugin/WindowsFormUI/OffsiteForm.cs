using RevitInventorExchange.CoreDataStructures;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RevitInventorExchange.WindowsFormBusinesslayer;
using RevitInventorExchange.Data;
using System.Linq.Dynamic;
using System.IO;
using NLog;

namespace RevitInventorExchange.WindowsFormUI
{
    public partial class OffsiteForm : Form
    {
        //  Scope of Work tab
        private bool sortAscendingElementList = false;
        private List<ElementStructure> elStructureList = null;
        private List<ElementsDataGridSourceData> elementList = null;
        private OffsitePanelHandler offsitePanelHandler = null;
        private string rootPath = "";        

        //  Initialize the form and its elements
        public OffsiteForm(List<ElementStructure> elementStructureList)
        {
            NLogger.LogText("Entered Offsite Form constructor");
          
            //  Load Configuration file
            ConfigUtilities.LoadConfig();

            InitializeComponent();

            elStructureList = elementStructureList;

            //  Initialize Offsite Panel BL handler.
            //  Here inventor process is started / attached to
            offsitePanelHandler = new OffsitePanelHandler();
            this.Size = new Size(1380, 750);
                        
            //  Set local BIM 360 folder as root path            
            rootPath = Utilities.GetInventorTemplateFolder(); // Utilities.GetBIM360RootPath();     
            folderBrowserDialogInventorTemplates.SelectedPath = rootPath;

            //  Iniotialize data grids
            InitializeSOWGrid(dgElements);
            InitializeMappingGrid(dgInvRevMapping);
            InitializeParametersMappingGrid(dgParamsMapping);

            NLogger.LogText("Exit Offsite Form constructor");
        }

        //  Fill logs textbox with messages coming from background
        private void DaEvHandler_DACurrentStepHandler(object sender, string e)
        {
            NLogger.LogText("Entered DaEvHandler_DACurrentStepHandler");

            if (!string.IsNullOrWhiteSpace(richTextBoxLogs.Text))
            {
                richTextBoxLogs.AppendText("\r\n" + e);
            }
            else
            {
                richTextBoxLogs.AppendText(e);
            }
            richTextBoxLogs.ScrollToCaret();

            NLogger.LogText("Exit DaEvHandler_DACurrentStepHandler");
        }

        private void ElementsForm_Load(object sender, EventArgs e)
        {
            NLogger.LogText("Entered ElementsForm_Load");

            // Populate the Element datagrid
            var dataSources = offsitePanelHandler.GetElementsDataGridSource(elStructureList);

            elementList = dataSources["Elements"];

            offsitePanelHandler.FillPropertiesGrid(dgElements, elementList);

            NLogger.LogText("Exit ElementsForm_Load");
        }

        #region SOW
        private void InitializeSOWGrid(DataGridView dataGrid)
        {
            dataGrid.AutoGenerateColumns = false;

            var colElId= new DataGridViewTextBoxColumn();
            colElId.ReadOnly = true;
            colElId.Name = "Element Identifier";
            colElId.DataPropertyName = "ElementId";
            colElId.SortMode = DataGridViewColumnSortMode.Automatic;

            var colName = new DataGridViewTextBoxColumn();
            colName.ReadOnly = true;
            colName.Name = "Element Name";
            colName.DataPropertyName = "ElementName";
            colName.SortMode = DataGridViewColumnSortMode.Automatic;

            var colValue = new DataGridViewTextBoxColumn();
            colValue.ReadOnly = true;
            colValue.Name = "Element Family Type";
            colValue.DataPropertyName = "ElementFamilyType";
            colValue.SortMode = DataGridViewColumnSortMode.Automatic;

            dataGrid.Columns.Add(colElId);
            dataGrid.Columns.Add(colName);
            dataGrid.Columns.Add(colValue);

            dataGrid.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGrid.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGrid.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            dataGrid.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
        }        

        //  Open the properties window for the selected Revit element
        private void btnProperties_Click(object sender, EventArgs e)
        {
            var elemSelectedRowCount = dgElements.Rows.GetRowCount(DataGridViewElementStates.Selected);

            if (elemSelectedRowCount == 1)
            {
                var elementId = Convert.ToInt32(dgElements.SelectedRows[0].Cells[0].Value);
                var elStructure = elStructureList.SingleOrDefault(p => p.Element.Id.IntegerValue == elementId);

                var popupWindow = new PropertiesCollectorForm(elStructure);
                popupWindow.Show();
            }     
            else
            {
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                MessageBox.Show("You have to select one Revit Element", "", buttons);

                return;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            //offsitePanelHandler.CloseInventorProcess();

            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnExportPropVals_Click(object sender, EventArgs e)
        {
            NLogger.LogText("Entered btnExportPropVals_Click");

            var daEvHandler = offsitePanelHandler.DaEventHandler;
            daEvHandler.DACurrentStepHandler += DaEvHandler_DACurrentStepHandler;

            var jsonParams = offsitePanelHandler.GetRevitPropertiesValues(elStructureList);

            //  Call Design Automation Forge APIs via HTTP calls to trigger Inventor Cloud execution engine
            
            offsitePanelHandler.RunDesignAutomation(jsonParams);

            daEvHandler.DACurrentStepHandler -= DaEvHandler_DACurrentStepHandler;

            NLogger.LogText("Exit btnExportPropVals_Click");
        }

        private void dgElements_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (sortAscendingElementList)
                dgElements.DataSource = elementList.OrderBy(dgElements.Columns[e.ColumnIndex].DataPropertyName).ToList();
            else
                dgElements.DataSource = elementList.OrderBy(dgElements.Columns[e.ColumnIndex].DataPropertyName).Reverse().ToList();

            sortAscendingElementList = !sortAscendingElementList;
        }

#endregion

#region Mapping tab

        //  Load the list of all Inventor files found at specified location and load them into grid
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            NLogger.LogText("Entered btnBrowse_Click");           

            DialogResult result = folderBrowserDialogInventorTemplates.ShowDialog();

            if (result == DialogResult.OK)
            {
                //  Handle selected path
                string selectedPath = folderBrowserDialogInventorTemplates.SelectedPath;
                //string selectedPath = rootPath ;
                txtInventorTemplatesPath.Text = selectedPath;                

                //  handle internal structure creation
                var dataSource = offsitePanelHandler.GetInvRevitMappingDataGridSource(selectedPath);
                var elementList = dataSource["InvRevMapping"];

                NLogger.LogText("Fill InventorRevitMapping grid");
                offsitePanelHandler.FillPropertiesGrid(dgInvRevMapping, elementList);

                //  Select first row of inventor - Revit mapping grid by default
                if (dgInvRevMapping.Rows.Count > 0)
                {
                    dgInvRevMapping.ClearSelection();
                    dgInvRevMapping.Rows[0].Selected = true;
                }
            }

            if (result == DialogResult.Cancel)
            {

            }

            NLogger.LogText("Exit btnBrowse_Click");
        }

        private void btnSelectFromScope_Click(object sender, EventArgs e)
        {
            NLogger.LogText("Entered btnSelectFromScope_Click");

            HandleRowSelection();

            NLogger.LogText("Exit btnSelectFromScope_Click");
        }       

        //  Handles the UI logic of the row selected + button click on the InventorRevit mapping grid
        private void HandleRowSelection()
        {
            NLogger.LogText("Entered HandleRowSelection");

            var invTemplRowCount = dgInvRevMapping.Rows.GetRowCount(DataGridViewElementStates.Selected);
            string invTemplateFileName = "";
            int selectedRow = -1;

            if (invTemplRowCount == 1)
            {
                invTemplateFileName = dgInvRevMapping.SelectedRows[0].Cells["Inventor Template"].Value.ToString();
                selectedRow = dgInvRevMapping.SelectedRows[0].Index;
            }
            else
            {
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                MessageBox.Show("You have to select one Inventor Template", "", buttons);

                return;
            }

            //  Open Revit families popup
            var revitFamiliesSelectionPopup = new RevitFamiliesSelectionPopup(elStructureList);
            var result = revitFamiliesSelectionPopup.ShowDialog();

            //  Update Mapping grids with data coming from popup
            if (result == DialogResult.OK)
            {
                //  Update RevitInventor mapping datagrid with Revit family selected in the popup
                var selRevFamily = revitFamiliesSelectionPopup.SelectedRevitFamily;

                NLogger.LogText($"Selected Revit Family {selRevFamily}");

                var dataSource = offsitePanelHandler.RefreshInvRevitMappingDataGridSource(selRevFamily, invTemplateFileName);

                var elementList = dataSource["InvRevMapping"];

                NLogger.LogText("Fill InventorRevitMapping grid");
                offsitePanelHandler.FillPropertiesGrid(dgInvRevMapping, elementList);

                NLogger.LogText("Set selected rows on  InventorRevitMapping and ParametersMapping grids");
                dgInvRevMapping.ClearSelection();
                dgInvRevMapping.CurrentCell = dgInvRevMapping.Rows[selectedRow].Cells["Inventor Template"];
                dgInvRevMapping.CurrentRow.Selected = false;
                dgInvRevMapping.Rows[selectedRow].Selected = true;

                dgParamsMapping.ClearSelection();
                dgParamsMapping.Rows[0].Selected = true;
            }

            NLogger.LogText("Exit HandleRowSelection");
        }

        private void InitializeMappingGrid(DataGridView dataGrid)
        {
            dataGrid.AutoGenerateColumns = false;

            var colInvTempl = new DataGridViewTextBoxColumn();
            colInvTempl.ReadOnly = true;
            colInvTempl.Name = "Inventor Template";
            colInvTempl.DataPropertyName = "InventorTemplate";
            colInvTempl.SortMode = DataGridViewColumnSortMode.Automatic;

            var colRevitFam = new DataGridViewTextBoxColumn();
            colRevitFam.ReadOnly = true;
            colRevitFam.Name = "Revit Family";
            colRevitFam.DataPropertyName = "RevitFamily";
            colRevitFam.SortMode = DataGridViewColumnSortMode.Automatic;

            dataGrid.Columns.Add(colInvTempl);
            dataGrid.Columns.Add(colRevitFam);

            dataGrid.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGrid.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        private void InitializeParametersMappingGrid(DataGridView dataGrid)
        {
            dataGrid.AutoGenerateColumns = false;

            var colInvParams = new DataGridViewTextBoxColumn();
            colInvParams.ReadOnly = true;
            colInvParams.Name = "Inventor Key Parameters";
            colInvParams.DataPropertyName = "InventorParamName";
            colInvParams.SortMode = DataGridViewColumnSortMode.Automatic;

            var colRevitParams = new DataGridViewTextBoxColumn();
            colRevitParams.ReadOnly = true;
            colRevitParams.Name = "Revit Parameters";
            colRevitParams.DataPropertyName = "RevitParamName";
            colRevitParams.SortMode = DataGridViewColumnSortMode.Automatic;

            dataGrid.Columns.Add(colInvParams);
            dataGrid.Columns.Add(colRevitParams);

            dataGrid.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGrid.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        #endregion

        private void btnRevitParametersSel_Click(object sender, EventArgs e)
        {
            NLogger.LogText("Entered btnRevitParametersSel_Click");

            HandleRowSelectionParams();

            NLogger.LogText("Exit btnRevitParametersSel_Click");
        }

        private void dgInvRevMapping_SelectionChanged(object sender, EventArgs e)
        {
            NLogger.LogText("Entered dgInvRevMapping_SelectionChanged");

            var rowsCount = dgInvRevMapping.Rows.GetRowCount(DataGridViewElementStates.Selected);
            var inventorTemplatePath = txtInventorTemplatesPath.Text;

            if (rowsCount > 0)
            {
                var invTemplateFileName = dgInvRevMapping.SelectedRows[0].Cells["Inventor Template"].Value.ToString();

                // Populate the Properties datagrid
                var dataSources = offsitePanelHandler.GetInvRevitParamsMappingDataGridSource(invTemplateFileName, inventorTemplatePath);

                var elementList = dataSources["ParamsMapping"];

                offsitePanelHandler.FillPropertiesGrid(dgParamsMapping, elementList);

                //  Select the first row of the Param mapping grid by Default
                dgParamsMapping.ClearSelection();
                dgParamsMapping.Rows[0].Selected = true;
            }

            NLogger.LogText("Exit dgInvRevMapping_SelectionChanged");
        }               

        private void HandleRowSelectionParams()
        {
            NLogger.LogText("Entered HandleRowSelectionParams");

            //  Force selection of an Inventor - Revit association
            var invRevMapRowCount = dgInvRevMapping.Rows.GetRowCount(DataGridViewElementStates.Selected);
            string revitFileName = "";
            string revitFamily = "";
            string inventorTemplate = "";
            int selectedRow = -1;

            if (invRevMapRowCount == 1)
            {
                revitFamily = dgInvRevMapping.SelectedRows[0].Cells["Revit Family"].Value?.ToString();
                inventorTemplate = dgInvRevMapping.SelectedRows[0].Cells["Inventor Template"].Value?.ToString();

                if (!string.IsNullOrEmpty(revitFamily) && !(revitFamily == "null"))
                {
                    revitFileName = Utilities.GetStringForFolderName(revitFamily);
                }
                else
                {
                    MessageBoxButtons buttons = MessageBoxButtons.OK;
                    MessageBox.Show("There must be a Revit Family associated with the selected inventor template", "", buttons);

                    return;
                }
            }
            else
            {
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                MessageBox.Show("You have to select one Inventor - Revit mapping row", "", buttons);

                return;
            }

            //  Force selection of a parameters
            var invRevParamRowCount = dgParamsMapping.Rows.GetRowCount(DataGridViewElementStates.Selected);
            string selInvParam = "";

            if (invRevParamRowCount == 1)
            {
                selInvParam = dgParamsMapping.SelectedRows[0].Cells["Inventor Key Parameters"].Value.ToString();
                selectedRow = dgParamsMapping.SelectedRows[0].Index;
            }
            else
            {
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                MessageBox.Show("You have to select one Parameter mapping row", "", buttons);

                return;
            }

            //  Open Revit families popup
            var revitFamiliesParamsSelectionPopup = new RevitFamiliesParametersSelectionPopup(revitFamily, elStructureList);
            var result = revitFamiliesParamsSelectionPopup.ShowDialog();

            //  Update Mapping grids with data coming from popup
            if (result == DialogResult.OK)
            {
                //  Update RevitInventorParameters mapping datagrid with Revit family Parameter selected in the popup
                var selRevFamilyParam = revitFamiliesParamsSelectionPopup.SelectedRevitFamilyParam;

                var dataSource = offsitePanelHandler.RefreshInvRevitParamsMappingDataGridSource(revitFamily, inventorTemplate, selInvParam, selRevFamilyParam);

                var elementList = dataSource["ParamsMapping"];

                offsitePanelHandler.FillPropertiesGrid(dgParamsMapping, elementList);

                if (selectedRow < (dgParamsMapping.Rows.Count - 1))
                {
                    dgParamsMapping.ClearSelection();
                    dgParamsMapping.CurrentCell = dgParamsMapping.Rows[selectedRow + 1].Cells["Inventor Key Parameters"];
                    dgParamsMapping.CurrentRow.Selected = false;
                    dgParamsMapping.Rows[selectedRow + 1].Selected = true;

                    selectedRow = selectedRow + 1;
                }
            }

            NLogger.LogText("Exit HandleRowSelectionParams");
        }

        private void OffsiteForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            offsitePanelHandler.CloseInventorProcess();            
        }
    }
}
