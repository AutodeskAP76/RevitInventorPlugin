﻿using RevitInventorExchange.CoreDataStructures;
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
using System.Diagnostics;
using RevitInventorExchange.Utilities;
using RevitInventorExchange.CoreBusinessLayer;

namespace RevitInventorExchange.WindowsFormUI
{
    enum RevitFamilyHandling
    {
        ResetRevitFamily,
        SetRevitFamily
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
        private readonly RevitElementSelectionMode selMode;

        private bool sortAscendingdgInvRevMapping = false;
        private bool sortAscendingdgParamsMapping = false;

        //  Initialize the form and its elements
        public OffsiteForm(IList<ADSK.Element> elementStructureList, UIApplication uiapplication, RevitElementSelectionMode RevitselMode)
        {
            NLogger.LogText("Entered Offsite Form constructor");
                      
            InitializeComponent();
           
            //  Initialize Offsite Panel BL handler.
            //  Here inventor process is started / attached to
            offsitePanelHandler = new OffsitePanelHandler(uiapplication);
            this.Size = new Size(1380, 800);

            selMode = RevitselMode;
            NLogger.LogText($"Selection mode: {selMode}");

            //  Check if selection mode is from Viewer or from Revit Families filter
            if (selMode == RevitElementSelectionMode.FromView)
            {
                var tempelStructureList = offsitePanelHandler.ProcessElements(elementStructureList);
                elStructureList = offsitePanelHandler.FilterElements(tempelStructureList);
                runtimeElStructureList = elStructureList;
                grBoxRevFamTypes.Visible = false;
                grBoxRevFamilies.Visible = false;
                btnSelectAllElements.Visible = false;
                btnUnselectAll.Visible = false;
            }
            else
            {
                grBoxRevFamTypes.Visible = true;
                grBoxRevFamilies.Visible = true;
            }
            
            //  Set local BIM 360 folder as root path            
            rootPath = Utility.GetInventorTemplateFolder(); // Utilities.GetBIM360RootPath();     
            folderBrowserDialogInventorTemplates.SelectedPath = rootPath;

            //  Initialize data grids & combobox
            InitializeSOWGrid(dgElements);
            InitializeMappingGrid(dgInvRevMapping);
            InitializeParametersMappingGrid(dgParamsMapping);
            InitializeLanguage();

            ((System.Windows.Forms.Control)tabControl1.TabPages[tabName]).Enabled = false;
            //InitializeCombobox(comboBoxRevitFamilies);
            //InitializeCombobox(comboBoxRevitFamilyTypes);

            NLogger.LogText("Exit Offsite Form constructor");
        }
        
        //  Fill logs textbox with messages coming from background
        private void DaEvHandler_DACurrentStepHandler(object sender, string e)
        {
            NLogger.LogText("Entered DaEvHandler_DACurrentStepHandler");
            
            //  This construct is needed in order to access a Win Form control from a thread different from the UI thread
            BeginInvoke(new Action(() =>
            {
               if (!string.IsNullOrWhiteSpace(richTextBoxLogs.Text))
               {
                   richTextBoxLogs.AppendText("\r\n" + e);
               }
               else
               {
                   richTextBoxLogs.AppendText(e);
               }
               richTextBoxLogs.ScrollToCaret();
            }));

            NLogger.LogText("Exit DaEvHandler_DACurrentStepHandler");
        }

        private void ElementsForm_Load(object sender, EventArgs e)
        {
            NLogger.LogText("Entered ElementsForm_Load");

            //  Load either Revit Elements grid or Revit Families dropdownlist, depending on how the user wants to select elements in Revit file
            if (elStructureList != null && elStructureList.Count != 0)
            {
                // Populate the Element datagrid
                var dataSources = offsitePanelHandler.GetElementsDataGridSource(elStructureList);

                elementList = dataSources["Elements"];

                offsitePanelHandler.FillGrid(dgElements, elementList);

                //DataGridAutoSize(dgElements);                
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
            colElId.HeaderText = LanguageHandler.GetString("dgElements_Col_ElId_Text");
            colElId.DataPropertyName = "ElementId";
            colElId.SortMode = DataGridViewColumnSortMode.Automatic;

            var colName = new DataGridViewTextBoxColumn();
            colName.ReadOnly = true;
            colName.Name = "Element Name";
            colName.HeaderText = LanguageHandler.GetString("dgElements_Col_ElName_Text");
            colName.DataPropertyName = "ElementName";
            colName.SortMode = DataGridViewColumnSortMode.Automatic;

            var colValue = new DataGridViewTextBoxColumn();
            colValue.ReadOnly = true;
            colValue.Name = "Element Family Type";
            colValue.HeaderText = LanguageHandler.GetString("dgElements_Col_ElVal_Text");
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

                var popupWindow = new PropertiesCollectorForm(elStructure, this);
                popupWindow.Show();
            }     
            else
            {
                //MessageBox.Show("You have to select one Revit Element");
                MessageBox.Show(LanguageHandler.GetString("msgBox_SelRevElement1"));

                return;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            //offsitePanelHandler.CloseInventorProcess();

            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        private async void btnExportPropVals_Click(object sender, EventArgs e)
        {
            NLogger.LogText("Entered btnExportPropVals_Click");
            
            //  Check if at least one parameters mapping has been done
            var checkConsistency = offsitePanelHandler.CheckMappingConsistency();

            if (!checkConsistency)
            {
                //MessageBox.Show("There are some missing mappings");
                MessageBox.Show(LanguageHandler.GetString("msgBox_MissingMapp"));
                return;
            }

            //  Disable the tab during DA workflow execution  
            ((System.Windows.Forms.Control)tabControl1.TabPages[tabName]).Enabled = false;
            ((System.Windows.Forms.Control)tabControl1.TabPages["TabSOW"]).Enabled = false;

            //  Handle event and build json from Revit elements + Revit - Inventor mapping
            var daEvHandler = offsitePanelHandler.DaEventHandler;
            daEvHandler.DACurrentStepHandler += DaEvHandler_DACurrentStepHandler;
            var jsonParams = offsitePanelHandler.GetRevitPropertiesValues(runtimeElStructureList);

            //  Call Design Automation Forge APIs via HTTP calls to trigger Inventor Cloud execution engine            
            /*var ret = */await offsitePanelHandler.RunDesignAutomation(jsonParams, invTemplFolder);
            //ret.Wait();

            daEvHandler.DACurrentStepHandler -= DaEvHandler_DACurrentStepHandler;

            //  Enable the tabs after DA workflow execution        
            BeginInvoke(new Action(() =>
            {
                ((System.Windows.Forms.Control)tabControl1.TabPages[tabName]).Enabled = true;
                ((System.Windows.Forms.Control)tabControl1.TabPages["TabSOW"]).Enabled = true;
            }));

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

            try
            {
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
            }
            catch(UIRelevantException ex)
            {
                MessageBox.Show(ex.Message);
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
                //MessageBox.Show("You have to select one Inventor Template");
                MessageBox.Show(LanguageHandler.GetString("msgBox_SelInvTemplate"));

                return;
            }

            //  If zip file, cannot associate parameters
            var isZipFile = System.IO.Path.GetExtension(invTemplateFileName) == ".zip";
            if (isZipFile)
            {
                //MessageBox.Show("Cannot map parameters for zip file");
                MessageBox.Show(LanguageHandler.GetString("msgBox_ZipFileParamMap"));
                return;
            }

            //  Set or reset Revit Family
            var selRevFamily = GetRevitFamily(mode);

            NLogger.LogText($"Selected Revit Family {selRevFamily}");

            if (string.IsNullOrEmpty(selRevFamily))
            {
                return;
            }

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
            colInvTempl.HeaderText = LanguageHandler.GetString("dgInvRevMapping_Col_InvTempl_Text");
            colInvTempl.DataPropertyName = "InventorTemplate";
            colInvTempl.SortMode = DataGridViewColumnSortMode.Automatic;

            var colRevitFam = new DataGridViewTextBoxColumn();
            colRevitFam.ReadOnly = true;
            colRevitFam.Name = "Revit Family";
            colRevitFam.HeaderText = LanguageHandler.GetString("dgInvRevMapping_Col_RevFam_Text");
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
            colInvParams.HeaderText = LanguageHandler.GetString("dgParamsMapping_Col_InvParam_Text");
            colInvParams.DataPropertyName = "InventorParamName";
            colInvParams.SortMode = DataGridViewColumnSortMode.Automatic;

            var colRevitParams = new DataGridViewTextBoxColumn();
            colRevitParams.ReadOnly = true;
            colRevitParams.Name = "Revit Parameters";
            colRevitParams.HeaderText = LanguageHandler.GetString("dgParamsMapping_Col_RevitParam_Text");
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

                if (dgParamsMapping.Rows.Count > 0)
                {
                    dgParamsMapping.Rows[0].Selected = true;
                }

                var mappedParams = elementList.Where(l => !string.IsNullOrEmpty(l.RevitParamName)).Count();
                lblNumbOfMappedParams.Text = mappedParams.ToString();
                lblNumberOfParams.Text = elementList.Count.ToString();

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
                    revitFileName = Utility.GetStringForFolderName(revitFamily);
                }
                else
                {
                    //MessageBox.Show("There must be a Revit Family associated with the selected inventor template");
                    MessageBox.Show(LanguageHandler.GetString("msgBox_RevitFamAssociat"));

                    return;
                }
            }
            else
            {
                //MessageBox.Show("You have to select one Inventor - Revit mapping row");
                MessageBox.Show(LanguageHandler.GetString("msgBox_SelInvRevMapRow"));

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
                //MessageBox.Show("You have to select one Parameter mapping row");
                MessageBox.Show(LanguageHandler.GetString("msgBox_SelParamMapRow"));

                return;
            }

            //  Read Revit Family Param from popup or reset it, checking if the Revit parameter is already assigned
            var RevitUsedParams = offsitePanelHandler.GetRevitFamilyParamsAlreadyUsed(revitFamily);

            var selRevFamilyParam = GetRevitFamilyParameter(mode, revitFamily, currentRevitParam, RevitUsedParams);

            var dataSource = offsitePanelHandler.RefreshInvRevitParamsMappingDataGridSource(revitFamily, inventorTemplate, selInvParam, selRevFamilyParam);

            //  Fill grid
            var elementList = dataSource["ParamsMapping"];
            offsitePanelHandler.FillGrid(dgParamsMapping, elementList);

            //  Update counter of mapped parameters
            var mappedParams = elementList.Where(l => !string.IsNullOrEmpty(l.RevitParamName)).Count();
            lblNumbOfMappedParams.Text = mappedParams.ToString();
            
            //  Set selected row:
            //  the row is changes if a real parameter mapping has been done and if the end of the grid has not been reached
            if (selectedRow < (dgParamsMapping.Rows.Count - 1) && 
                mode == RevitFamilyHandling.SetRevitFamily && 
                !(selRevFamilyParam == currentRevitParam)
                )
            {
                selectedRow = selectedRow + 1;

                UpdateRowSelection(selectedRow);                              
            }
            else
            {
                UpdateRowSelection(selectedRow);
            }
            
            NLogger.LogText("Exit HandleRowSelectionParams");
        }

        private void UpdateRowSelection(int selectedRow)
        {
            dgParamsMapping.ClearSelection();
            dgParamsMapping.CurrentCell = dgParamsMapping.Rows[selectedRow].Cells["Inventor Key Parameters"];
            dgParamsMapping.CurrentRow.Selected = false;
            dgParamsMapping.Rows[selectedRow].Selected = true;
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

            if (selMode == RevitElementSelectionMode.FromFilters)
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
                    //MessageBox.Show("At least one Revit Element must be selected");
                    MessageBox.Show(LanguageHandler.GetString("msgBox_SelRevElement"));
                }
            }
            
            if (selMode == RevitElementSelectionMode.FromView)
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

        private void button1_Click(object sender, EventArgs e)
        {
            Process.Start(txtInventorTemplatesPath.Text);
        }

        private void InitializeLanguage()
        {            
            this.Text = LanguageHandler.GetString("OffsiteForm_Text");
            ((System.Windows.Forms.Control)tabControl1.TabPages["tabSOW"]).Text = LanguageHandler.GetString("tabControl1_tabSOW_Text");
            ((System.Windows.Forms.Control)tabControl1.TabPages["tabMappings"]).Text = LanguageHandler.GetString("tabControl1_tabMapping_Text");
            grBoxRevFamilies.Text = LanguageHandler.GetString("grBoxRevFamilies_Text");
            grBoxRevFamTypes.Text = LanguageHandler.GetString("grBoxRevFamTypes_Text");
            grBoxRevElements.Text = LanguageHandler.GetString("grBoxRevElements_Text");
            btnProperties.Text = LanguageHandler.GetString("btnProperties_Text");
            btnSelectAllElements.Text = LanguageHandler.GetString("btnSelectAllElements_Text");
            btnUnselectAll.Text = LanguageHandler.GetString("btnUnselectAll_Text");
            btnProcessElements.Text = LanguageHandler.GetString("btnProcessElements_Text");
            grpBoxInventorTemplates.Text = LanguageHandler.GetString("grpBoxInventorTemplates_Text");
            btnBrowse.Text = LanguageHandler.GetString("btnBrowse_Text");
            btnSelectFromScope.Text = LanguageHandler.GetString("btnSelectFromScope_Text");
            btnClearSelectedMapping.Text = LanguageHandler.GetString("btnClearSelectedMapping_Text");
            grpBoxRevitFamilies.Text = LanguageHandler.GetString("grpBoxRevitFamilies_Text");
            lblTotalParamNr.Text = LanguageHandler.GetString("lblTotalParamNr_Text");
            lblTotalParamMappedNr.Text = LanguageHandler.GetString("lblTotalParamMappedNr_Text");
            btnRevitParametersSel.Text = LanguageHandler.GetString("btnRevitParametersSel_Text");
            btnClearSelectedParamMapping.Text = LanguageHandler.GetString("btnClearSelectedParamMapping_Text");
            btnClearLogs.Text = LanguageHandler.GetString("btnClearLogs_Text");
            btnGenerateModel.Text = LanguageHandler.GetString("btnGenerateModel_Text");
            btnCancel.Text = LanguageHandler.GetString("btnCancel_Text");
            btnGenerateBOM.Text = LanguageHandler.GetString("btnGenerateBOM_Text");
            btnGenerateDrawings.Text = LanguageHandler.GetString("btnGenerateDrawings_Text");

        }

        private void dgInvRevMapping_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {           
            var ds = (List<InvRevMappingDataGridSourceData>)dgInvRevMapping.DataSource;

            if (sortAscendingdgInvRevMapping)                
                dgInvRevMapping.DataSource = ds.OrderBy(dgInvRevMapping.Columns[e.ColumnIndex].DataPropertyName).ToList();
            else
                dgInvRevMapping.DataSource = ds.OrderBy(dgInvRevMapping.Columns[e.ColumnIndex].DataPropertyName).Reverse().ToList();

            sortAscendingdgInvRevMapping = !sortAscendingdgInvRevMapping;
        }

        private void dgParamsMapping_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {            
            var ds = (List<InvRevParamMappingDataGridSourceData>)dgParamsMapping.DataSource;

            if (sortAscendingdgParamsMapping)
                dgParamsMapping.DataSource = ds.OrderBy(dgParamsMapping.Columns[e.ColumnIndex].DataPropertyName).ToList();
            else
                dgParamsMapping.DataSource = ds.OrderBy(dgParamsMapping.Columns[e.ColumnIndex].DataPropertyName).Reverse().ToList();

            sortAscendingdgParamsMapping = !sortAscendingdgParamsMapping;
        }

        private void btnGenerateDrawings_Click(object sender, EventArgs e)
        {

        }

        private void btnGenerateBOM_Click(object sender, EventArgs e)
        {

        }


        //private void DataGridAutoSize(DataGridView dg)
        //{
        //    int sum = dg.ColumnHeadersHeight;

        //    foreach (DataGridViewRow row in dg.Rows)
        //        sum += row.Height + 1; // I dont think the height property includes the cell border size, so + 1

        //    dg.Height = sum;
        //}
    }
}
