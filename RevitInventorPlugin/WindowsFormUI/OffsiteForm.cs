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
using Inventor;
using ADSK = Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Controls;

namespace RevitInventorExchange.WindowsFormUI
{
    enum RevitFamilyHandling
    {
        ResetRevitFamily,
        SetRevitFamily
    }

    public enum SelectionMode
    {
        FromView,
        FromFilters
    }

    public partial class OffsiteForm : Form
    {
        //  Scope of Work tab
        private const string tabName = "tabMappings";
        private bool sortAscendingElementList = false;
        private IList<ElementStructure> elStructureList = null;
        private IList<ElementStructure> runtimeElStructureList = null;
        private List<ElementsDataGridSourceData> elementList = null;
        private OffsitePanelHandler offsitePanelHandler = null;
        private IList<ADSK.Element> RevitFamTypes = null;       
        private string rootPath = "";
        private string invTemplFolder = "";
        public readonly SelectionMode selMode;

        //  Initialize the form and its elements
        public OffsiteForm(IList<ADSK.Element> elementStructureList, UIApplication uiapplication)
        {
            NLogger.LogText("Entered Offsite Form constructor");
          
            //  Load Configuration file
            ConfigUtilities.LoadConfig();

            InitializeComponent();
           
            //  Initialize Offsite Panel BL handler.
            //  Here inventor process is started / attached to
            offsitePanelHandler = new OffsitePanelHandler(uiapplication);
            this.Size = new Size(1380, 800);

            //  Check if selection mode is from Viewer or from Revit Families filter
            if (elementStructureList != null)
            {
                var tempelStructureList = offsitePanelHandler.ProcessElements(elementStructureList);
                elStructureList = offsitePanelHandler.FilterElements(tempelStructureList);
                runtimeElStructureList = elStructureList;
                groupBox2.Visible = false;
                groupBox3.Visible = false;

                selMode = SelectionMode.FromView;
            }
            else
            {
                groupBox2.Visible = true;
                groupBox3.Visible = true;

                selMode = SelectionMode.FromFilters;
            }

            NLogger.LogText($"Selection mode: {selMode}");

            //  Set local BIM 360 folder as root path            
            rootPath = Utilities.GetInventorTemplateFolder(); // Utilities.GetBIM360RootPath();     
            folderBrowserDialogInventorTemplates.SelectedPath = rootPath;

            //  Initialize data grids & combobox
            InitializeSOWGrid(dgElements);
            InitializeMappingGrid(dgInvRevMapping);
            InitializeParametersMappingGrid(dgParamsMapping);

            if (selMode == SelectionMode.FromView)
            {
                btnSelectAllElements.Visible = false;
                btnUnselectAll.Visible = false;
            }

            ((System.Windows.Forms.Control)tabControl1.TabPages[tabName]).Enabled = false;
            //InitializeCombobox(comboBoxRevitFamilies);
            //InitializeCombobox(comboBoxRevitFamilyTypes);

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

            //  Load either Revit Elements grid or Revit Families dropdownlist, depending on how the user wants to select elements in Revit file
            if (elStructureList != null)
            {
                // Populate the Element datagrid
                var dataSources = offsitePanelHandler.GetElementsDataGridSource(elStructureList);

                elementList = dataSources["Elements"];

                offsitePanelHandler.FillGrid(dgElements, elementList);
            }
            else
            {
                var dataSource = offsitePanelHandler.GetRevitFamiliesList();

                this.comboBoxRevitFamilies.SelectedIndexChanged -= new EventHandler(comboBoxRevitFamilies_SelectedIndexChanged);

                offsitePanelHandler.FillComboRevitFamTypes(comboBoxRevitFamilies, dataSource, "Name");

                this.comboBoxRevitFamilies.SelectedIndexChanged += new EventHandler(comboBoxRevitFamilies_SelectedIndexChanged);

                comboBoxRevitFamilies.SelectedIndex = -1;

                //  populate filtering drowdown lists
                //var dataSource = offsitePanelHandler.GetListFamilyTypeSource(RevitFamTypes);  
                //offsitePanelHandler.FillComboRevitFamTypes(comboBoxRevitFamilyTypes, dataSource, "FamilyTypeName");
            }
            
            NLogger.LogText("Exit ElementsForm_Load");
        }

        #region SOW

        private void InitializeCombobox(System.Windows.Forms.ComboBox cmbBox)
        {
            cmbBox.SelectedIndex = -1;
        }

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

            var jsonParams = offsitePanelHandler.GetRevitPropertiesValues(runtimeElStructureList);

            //  Call Design Automation Forge APIs via HTTP calls to trigger Inventor Cloud execution engine            
            offsitePanelHandler.RunDesignAutomation(jsonParams, invTemplFolder);

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
                invTemplFolder = folderBrowserDialogInventorTemplates.SelectedPath;
                //string selectedPath = rootPath ;
                txtInventorTemplatesPath.Text = invTemplFolder;                

                //  handle internal structure creation
                var dataSource = offsitePanelHandler.GetInvRevitMappingDataGridSource(invTemplFolder);
                var elementList = dataSource["InvRevMapping"];

                NLogger.LogText("Fill InventorRevitMapping grid");
                offsitePanelHandler.FillGrid(dgInvRevMapping, elementList);

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

            HandleRowSelection(RevitFamilyHandling.SetRevitFamily);

            NLogger.LogText("Exit btnSelectFromScope_Click");
        }

        //  Handles the UI logic of the row selected + button click on the InventorRevit mapping grid
        private void HandleRowSelection(RevitFamilyHandling mode)
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

            //  Set or reset Revit Family
            var selRevFamily = GetRevitFamily(mode);

            NLogger.LogText($"Selected Revit Family {selRevFamily}");

            var dataSource = offsitePanelHandler.RefreshInvRevitMappingDataGridSource(selRevFamily, invTemplateFileName);

            var elementList = dataSource["InvRevMapping"];

            NLogger.LogText("Fill InventorRevitMapping grid");
            offsitePanelHandler.FillGrid(dgInvRevMapping, elementList);

            NLogger.LogText("Set selected rows on  InventorRevitMapping and ParametersMapping grids");
            dgInvRevMapping.ClearSelection();
            dgInvRevMapping.CurrentCell = dgInvRevMapping.Rows[selectedRow].Cells["Inventor Template"];
            dgInvRevMapping.CurrentRow.Selected = false;
            dgInvRevMapping.Rows[selectedRow].Selected = true;

            dgParamsMapping.ClearSelection();
            dgParamsMapping.Rows[0].Selected = true;

            NLogger.LogText("Exit HandleRowSelection");
        }

        private string GetRevitFamily(RevitFamilyHandling mode)
        {
            string revitFamily = "";

            switch (mode)
            {
                case RevitFamilyHandling.ResetRevitFamily:
                    revitFamily = "null";
                    break;
                case RevitFamilyHandling.SetRevitFamily:

                    //  Open Revit families popup                    
                    var revitFamiliesSelectionPopup = new RevitFamiliesSelectionPopup(runtimeElStructureList);
                    var result = revitFamiliesSelectionPopup.ShowDialog();

                    //  Update Mapping grids with data coming from popup
                    if (result == DialogResult.OK)
                    {
                        //  Update RevitInventor mapping datagrid with Revit family selected in the popup
                        revitFamily = revitFamiliesSelectionPopup.SelectedRevitFamily;
                    }
                    break;
            }

            return revitFamily;
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

            HandleRowSelectionParams(RevitFamilyHandling.SetRevitFamily);

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

                offsitePanelHandler.FillGrid(dgParamsMapping, elementList);

                //  Select the first row of the Param mapping grid by Default
                dgParamsMapping.ClearSelection();
                dgParamsMapping.Rows[0].Selected = true;
            }

            NLogger.LogText("Exit dgInvRevMapping_SelectionChanged");
        }               
        
        private void HandleRowSelectionParams(RevitFamilyHandling mode)
        {
            NLogger.LogText("Entered HandleRowSelectionParams");

            //  Force selection of an Inventor - Revit association
            var invRevMapRowCount = dgInvRevMapping.Rows.GetRowCount(DataGridViewElementStates.Selected);
            string revitFileName = "";
            string revitFamily = "";
            string inventorTemplate = "";
            int selectedRow = -1;

            //  check if a row is selected in Inventor - revit mapping and if a family is associated
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
            string currentRevitParam = "";

            if (invRevParamRowCount == 1)
            {
                selInvParam = dgParamsMapping.SelectedRows[0].Cells["Inventor Key Parameters"].Value.ToString();
                currentRevitParam = dgParamsMapping.SelectedRows[0].Cells["Revit Parameters"].Value.ToString();
                selectedRow = dgParamsMapping.SelectedRows[0].Index;
            }
            else
            {
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                MessageBox.Show("You have to select one Parameter mapping row", "", buttons);

                return;
            }

            //  Read Revit Family Param from popup or reset it, checking if the Revit parameter is already assigned
            var RevitUsedParams = offsitePanelHandler.GetRevitFamilyParamsAlreadyUsed(revitFamily);

            var selRevFamilyParam = GetRevitFamilyParameter(mode, revitFamily, currentRevitParam, RevitUsedParams);

            var dataSource = offsitePanelHandler.RefreshInvRevitParamsMappingDataGridSource(revitFamily, inventorTemplate, selInvParam, selRevFamilyParam);

            var elementList = dataSource["ParamsMapping"];

            offsitePanelHandler.FillGrid(dgParamsMapping, elementList);

            if (selectedRow < (dgParamsMapping.Rows.Count - 1))
            {
                dgParamsMapping.ClearSelection();
                dgParamsMapping.CurrentCell = dgParamsMapping.Rows[selectedRow + 1].Cells["Inventor Key Parameters"];
                dgParamsMapping.CurrentRow.Selected = false;
                dgParamsMapping.Rows[selectedRow + 1].Selected = true;

                selectedRow = selectedRow + 1;
            }
            else
            {
                dgParamsMapping.ClearSelection();
                dgParamsMapping.CurrentCell = dgParamsMapping.Rows[selectedRow].Cells["Inventor Key Parameters"];
                dgParamsMapping.CurrentRow.Selected = false;
                dgParamsMapping.Rows[selectedRow].Selected = true;
            }

            NLogger.LogText("Exit HandleRowSelectionParams");
        }

        /// <summary>
        /// Based on mode, it reset the Revit property, or return the selected one from shown popup
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="revitFamily"></param>
        /// <param name="currentRefParam"></param>
        /// <param name="RevitUsedParams"></param>
        /// <returns></returns>
        private string GetRevitFamilyParameter(RevitFamilyHandling mode, string revitFamily, string currentRefParam, IList<string> RevitUsedParams)
        {
            NLogger.LogText("Entered GetRevitFamilyParameter");

            string revitFamilyParam = "";

            switch (mode)
            {
                case RevitFamilyHandling.ResetRevitFamily:
                    revitFamilyParam = "";
                    break;
                case RevitFamilyHandling.SetRevitFamily:
                    
                    var revitFamiliesParamsSelectionPopup = new RevitFamiliesParametersSelectionPopup(revitFamily, runtimeElStructureList, RevitUsedParams);
                    var result = revitFamiliesParamsSelectionPopup.ShowDialog();

                    //  Update Mapping grids with data coming from popup or leave the original value if click on Cancel button
                    if (result == DialogResult.OK)
                    {
                        //  Update RevitInventorParameters mapping datagrid with Revit family Parameter selected in the popup
                        revitFamilyParam = revitFamiliesParamsSelectionPopup.SelectedRevitFamilyParam;
                    }
                    else
                    {
                        revitFamilyParam = currentRefParam;
                    }
                    break;
            }

            NLogger.LogText("Exit GetRevitFamilyParameter");

            return revitFamilyParam;
        }

        private void OffsiteForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            offsitePanelHandler.CloseInventorProcess();            
        }

        private void btnClearLogs_Click(object sender, EventArgs e)
        {
            richTextBoxLogs.Clear();
        }

        //  Clear Inventor - Revit mapping for a selected row
        private void btnClearSelectedMapping_Click(object sender, EventArgs e)
        {
            NLogger.LogText("Entered btnClearSelectedMapping_Click");
          
            HandleRowSelection(RevitFamilyHandling.ResetRevitFamily);
           
            NLogger.LogText("Exit btnClearSelectedMapping_Click");
        }

        //  Clear Inventor - Revit parameters mapping for a selected row
        private void btnClearSelectedParamMapping_Click(object sender, EventArgs e)
        {
            NLogger.LogText("Enter btnClearSelectedParamMapping_Click");

            HandleRowSelectionParams(RevitFamilyHandling.ResetRevitFamily);

            NLogger.LogText("Exit btnClearSelectedParamMapping_Click");
        }

        private void comboBoxRevitFamilies_SelectedIndexChanged(object sender, EventArgs e)
        {
            NLogger.LogText("Enter comboBoxRevitFamilies_SelectedIndexChanged");

            RevitFamily selectedFamily = (RevitFamily)comboBoxRevitFamilies.SelectedItem;

            if (selectedFamily != null)
            {
                //  Reset other elements depending from the selection
                offsitePanelHandler.FillGrid(dgElements, null);
                offsitePanelHandler.FillGrid(dgParamsMapping, null);
                offsitePanelHandler.FillGrid(dgInvRevMapping, null);
                txtInventorTemplatesPath.Text = "";
                offsitePanelHandler.ResetRevitInventorMappingInternalStructure();
                ((System.Windows.Forms.Control)tabControl1.TabPages[tabName]).Enabled = false;

                //  Extract Revit family from revit document, based on selected xml configured element
                NLogger.LogText($"Revit family selected: {selectedFamily.Name}");

                RevitFamTypes = offsitePanelHandler.GetRevitFamilyTypesInActiveDocument(selectedFamily);

                NLogger.LogText($"Retrieved: {RevitFamTypes.Count()} family types");

                //  populate filtering drowdown lists
                var dataSource = offsitePanelHandler.GetListFamilyTypeSource(RevitFamTypes, selectedFamily);

                this.comboBoxRevitFamilyTypes.SelectedIndexChanged -= new EventHandler(comboBoxRevitFamilyTypes_SelectedIndexChanged);
                offsitePanelHandler.FillComboRevitFamTypes(comboBoxRevitFamilyTypes, dataSource, "FamilyTypeName");                
                this.comboBoxRevitFamilyTypes.SelectedIndexChanged += new EventHandler(comboBoxRevitFamilyTypes_SelectedIndexChanged);
            }

            NLogger.LogText("Exit comboBoxRevitFamilies_SelectedIndexChanged");
        }

        private void comboBoxRevitFamilyTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            NLogger.LogText("Enter comboBoxRevitFamilies_SelectedIndexChanged");

            ComboBoxRevitFamilyTypesSourceData selectedFamilyType = (ComboBoxRevitFamilyTypesSourceData)comboBoxRevitFamilyTypes.SelectedItem;

            if (selectedFamilyType != null)
            {
                offsitePanelHandler.FillGrid(dgElements, null);
                offsitePanelHandler.FillGrid(dgParamsMapping, null);
                offsitePanelHandler.FillGrid(dgInvRevMapping, null);
                txtInventorTemplatesPath.Text = "";
                offsitePanelHandler.ResetRevitInventorMappingInternalStructure();
                ((System.Windows.Forms.Control)tabControl1.TabPages[tabName]).Enabled = false;

                //  Extract Revit family from revit document, based on selected xml configured element
                NLogger.LogText($"Revit family type selected: {selectedFamilyType.FamilyTypeName}");

                Type famType = Type.GetType(selectedFamilyType.FamilyTypeInstance); //Type.GetType("Autodesk.Revit.DB.FamilyInstance,RevitAPI");

                var filteredElements = offsitePanelHandler.FindInstancesOfType(famType, selectedFamilyType.IdType, selectedFamilyType.TargetCategory);

                var tempelStructureList = offsitePanelHandler.ProcessElements(filteredElements);
                elStructureList = offsitePanelHandler.FilterElements(tempelStructureList);

                NLogger.LogText($"Retrieved: {elStructureList.Count()} Revit elements");

                // Populate the Element datagrid
                var dataSources = offsitePanelHandler.GetElementsDataGridSource(elStructureList);

                elementList = dataSources["Elements"];

                offsitePanelHandler.FillGrid(dgElements, elementList);
            }

            NLogger.LogText("Exit comboBoxRevitFamilies_SelectedIndexChanged");
        }

        private void btnProcessElements_Click(object sender, EventArgs e)
        {
            NLogger.LogText("Enter btnProcessElements_Click");

            var rowsCount = dgElements.Rows.GetRowCount(DataGridViewElementStates.Selected);
            
            //  depending on Selection mode and if there are Revit selected elements in the grid, enable / disable tab
            ((System.Windows.Forms.Control)tabControl1.TabPages[tabName]).Enabled = false;

            if (selMode == SelectionMode.FromFilters)
            {               
                if (rowsCount > 0)
                {
                    //  Retrieve from internal structure list only selected elements
                    ((System.Windows.Forms.Control)tabControl1.TabPages[tabName]).Enabled = true;
                    tabControl1.SelectedTab = tabControl1.TabPages[tabName];

                    IList<string> selectedElementsIds = new List<string>();
                  
                    foreach (DataGridViewRow row in dgElements.SelectedRows)
                    {
                        selectedElementsIds.Add(row.Cells[0].Value.ToString());
                    }

                    runtimeElStructureList = elStructureList.Where(l => selectedElementsIds.Contains(l.Element.Id.ToString())).ToList();
                }
                else
                {
                    MessageBoxButtons buttons = MessageBoxButtons.OK;
                    MessageBox.Show("At least one Revit Element must be selected", "", buttons);
                }
            }
            
            if (selMode == SelectionMode.FromView)
            {
                ((System.Windows.Forms.Control)tabControl1.TabPages[tabName]).Enabled = true;
                tabControl1.SelectedTab = tabControl1.TabPages[tabName];
            }

            NLogger.LogText("Exit btnProcessElements_Click");
        }

        private void btnSelectAllElements_Click(object sender, EventArgs e)
        {
            dgElements.SelectAll();
        }

        private void btnUnselectAll_Click(object sender, EventArgs e)
        {
            dgElements.ClearSelection();
        }
    }
}
