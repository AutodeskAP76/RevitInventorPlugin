using RevitInventorExchange.Data;
using RevitInventorExchange.CoreBusinessLayer;
using RevitInventorExchange.CoreDataStructures;
using RevitInventorExchange.WindowsFormBusinesslayer;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Windows.Forms;
using System.Xml;
using System.Reflection;
using System.Drawing;
using Inventor;

namespace RevitInventorExchange.WindowsFormUI
{
    public partial class PropertiesCollectorForm : Form
    {
        private ElementStructure elStructureList;

        private bool sortAscendingElementParamList = false;
        private bool sortAscendingElementTypeParamList = false;
        List<PropertiesDataGridSourceData> elementParamList = null;
        List<PropertiesDataGridSourceData> elementTypeParamList = null;
        private bool enableFilter;
        private PropertiesCollectorFormHandler winFormHandler;

        public PropertiesCollectorForm(ElementStructure elementStructureList)
        {
            InitializeComponent();
            elStructureList = elementStructureList;
            enableFilter = chckBoxApplyFilters.Checked;
            winFormHandler = new PropertiesCollectorFormHandler();
            this.Size = new Size(655, 840);

            InitializeGrid(dgElementParams);
            InitializeGrid(dgElementTypeParams);

            InitBehaviour();
        }

        private void PropertiesCollectorForm_Load(object sender, EventArgs e)
        {
            var dataSources = winFormHandler.GetParametersDataGridSource(elStructureList, enableFilter);

            elementParamList = dataSources["ElementParameters"];
            elementTypeParamList = dataSources["ElementTypeParameters"];

            winFormHandler.FillGrid(dgElementParams, elementParamList);
            winFormHandler.FillGrid(dgElementTypeParams, elementTypeParamList);

            txtFamily.Text = elStructureList.ElementType.FamilyName;
            txtFamType.Text = elStructureList.ElementTypeSingleParameters.Single(p => p.ParameterName == "SYMBOL_FAMILY_AND_TYPE_NAMES_PARAM").ParameterValue.ToString();
        }       

        private void InitializeGrid(DataGridView dgParam)
        {
            dgParam.AutoGenerateColumns = false;

            var colId = new DataGridViewTextBoxColumn();
            colId.ReadOnly = true;
            colId.Name = "Property Id";
            colId.DataPropertyName = "PropertyId";
            colId.SortMode = DataGridViewColumnSortMode.Automatic;

            var colName = new DataGridViewTextBoxColumn();
            colName.ReadOnly = true;
            colName.Name = "Property Name";
            colName.DataPropertyName = "PropertyName";
            colName.SortMode = DataGridViewColumnSortMode.Automatic;

            var colValue = new DataGridViewTextBoxColumn();
            colValue.ReadOnly = true;
            colValue.Name = "Property Value";
            colValue.DataPropertyName = "PropertyValue";
            colValue.SortMode = DataGridViewColumnSortMode.Automatic;

            dgParam.Columns.Add(colId);
            dgParam.Columns.Add(colName);
            dgParam.Columns.Add(colValue);

            dgParam.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dgParam.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dgParam.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            dgParam.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
        }

        #region Form elements events

        private void btnOk_Click(object sender, EventArgs e)
        {
            //  Create the xml file path, based on selected element
            var fileName = elStructureList.ElementType.FamilyName;

            var fileName1 = elStructureList.ElementTypeSingleParameters.SingleOrDefault(p => p.ParameterName == "SYMBOL_FAMILY_AND_TYPE_NAMES_PARAM").ParameterValue;
            fileName1 = Utilities.GetStringForFolderName(fileName1);

            //var assemblyFolder = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            //var TwoUp = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(assemblyFolder));

            var folderBaseline = Utilities.GetFolderBaseline();
            var xmlPath = System.IO.Path.Combine(folderBaseline, "FilteringFiles\\" + fileName1 + ".xml");

            string localXmlPath = new Uri(xmlPath).LocalPath;

            if (System.IO.File.Exists(localXmlPath))
            {
                DialogResult dialogResult = MessageBox.Show("The filtering file for the given element already exist. Do you want to replace it?", "", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.No)
                {
                    return;
                }               
            }

            var revFilterHandler = new RevitFiltersHandler();
            var filteredRows = new Dictionary<string, DataGridViewSelectedRowCollection>();
            filteredRows.Add("ElementProperties", dgElementParams.SelectedRows);
            filteredRows.Add("ElementTypeProperties", dgElementTypeParams.SelectedRows);

            revFilterHandler.createParametersFilterFile(localXmlPath, filteredRows);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        private void dgElementParams_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (sortAscendingElementParamList)
                dgElementParams.DataSource = elementParamList.OrderBy(dgElementParams.Columns[e.ColumnIndex].DataPropertyName).ToList();
            else
                dgElementParams.DataSource = elementParamList.OrderBy(dgElementParams.Columns[e.ColumnIndex].DataPropertyName).Reverse().ToList();

            sortAscendingElementParamList = !sortAscendingElementParamList;
        }

        private void dgElementTypeParams_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (sortAscendingElementTypeParamList)
                dgElementTypeParams.DataSource = elementTypeParamList.OrderBy(dgElementParams.Columns[e.ColumnIndex].DataPropertyName).ToList();
            else
                dgElementTypeParams.DataSource = elementTypeParamList.OrderBy(dgElementParams.Columns[e.ColumnIndex].DataPropertyName).Reverse().ToList();

            sortAscendingElementTypeParamList = !sortAscendingElementTypeParamList;
        }

        #endregion

        private void btnCopyPaste_Click(object sender, EventArgs e)
        {
            if (this.dgElementTypeParams.GetCellCount(DataGridViewElementStates.Selected) > 0)
            {
                try
                {
                    // Add the selection to the clipboard.
                    Clipboard.SetDataObject(this.dgElementTypeParams.GetClipboardContent());
                }
                catch (System.Runtime.InteropServices.ExternalException)
                {
                   
                }
            }
        }

        private void chckBoxApplyFilters_CheckedChanged(object sender, EventArgs e)
        {
            enableFilter = chckBoxApplyFilters.Checked;

            var filteredElStruct = winFormHandler.FilterElements(elStructureList);
            var dataSources = winFormHandler.GetParametersDataGridSource(filteredElStruct, enableFilter);

            elementParamList = dataSources["ElementParameters"];
            elementTypeParamList = dataSources["ElementTypeParameters"];

            winFormHandler.FillGrid(dgElementParams, elementParamList);
            winFormHandler.FillGrid(dgElementTypeParams, elementTypeParamList);
        }

        private void InitBehaviour()
        {            
            var controls = new List<Control> { lblApplyFilters, chckBoxApplyFilters, btnCreateFilterFile };
            Utilities.SetRuntimeBehaviour(controls);
        }
    }
}
