using System;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization.Configuration;
using Inventor;
using RevitInventorExchange.WindowsFormBusinesslayer;
using RevitInventorExchange.RevitDataStructures;

namespace RevitInventorExchange.WindowsFormUI
{
    public partial class InventorTemplatesForm : Form
    {
        
        List<InvRevMappingDataGridSourceData> invRevMappingDataSource = new List<InvRevMappingDataGridSourceData>();
        private List<ElementStructure> elStructureList;                

        public InventorTemplatesForm(List<ElementStructure> elementStructureList)
        {
            InitializeComponent();
            elStructureList = elementStructureList;
            
            this.Size = new Size(1257, 548);

            //  Set local BIM 360 folder as root path            
            var rootPath = Utilities.GetBIM360RootPath();   //System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile) + @"\BIM 360\";                       
            folderBrowserDialogInventorTemplates.SelectedPath = rootPath;

            InitializeInventorTemplatesGrid(dgInvRevMapping);            
        }

        private void InventorTemplatesForm_Load(object sender, EventArgs e)
        {
            
        }

        private void InitializeInventorTemplatesGrid(DataGridView dataGrid)
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

        //  Get list of Inventor Templates, browsing them
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialogInventorTemplates.ShowDialog();
            string selectedPath = "";
            string[] inventorTemplates;

            if (result == DialogResult.OK)
            {
                selectedPath = folderBrowserDialogInventorTemplates.SelectedPath;
                txtInventorTemplatesPath.Text = selectedPath;

                inventorTemplates = Directory.GetFiles(selectedPath);

                foreach (var path in inventorTemplates)
                {
                    invRevMappingDataSource.Add(new InvRevMappingDataGridSourceData { InventorTemplate = System.IO.Path.GetFileName(path), RevitFamily = "null" });
                }

                dgInvRevMapping.DataSource = invRevMappingDataSource;
            }
        }

        //  Open modal popup for Revit family selection
        private void btnSelectFromScope_Click(object sender, EventArgs e)
        {
            var invTemplRowCount = dgInvRevMapping.Rows.GetRowCount(DataGridViewElementStates.Selected);
            string invTemplate = "";

            if (invTemplRowCount == 1)
            {
                invTemplate = dgInvRevMapping.SelectedRows[0].Cells[0].Value.ToString();
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

            //  Update Mapping grid with data comiung from popup
            if (result == DialogResult.OK)
            {
                var selRevFamily = revitFamiliesSelectionPopup.SelectedRevitFamily;

                var invElement = invRevMappingDataSource.Single(k => k.InventorTemplate == invTemplate);
                invElement.RevitFamily = selRevFamily;

                dgInvRevMapping.DataSource = null;
                dgInvRevMapping.DataSource = invRevMappingDataSource;
            }
        }
    }
}
