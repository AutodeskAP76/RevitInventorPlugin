namespace RevitInventorExchange.WindowsFormUI
{
    partial class InventorTemplatesForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dgInvRevMapping = new System.Windows.Forms.DataGridView();
            this.grpBoxInventorTemplates = new System.Windows.Forms.GroupBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtInventorTemplatesPath = new System.Windows.Forms.TextBox();
            this.grpBoxRevitFamilies = new System.Windows.Forms.GroupBox();
            this.btnSelectFromScope = new System.Windows.Forms.Button();
            this.dgRevitFamilies = new System.Windows.Forms.DataGridView();
            this.folderBrowserDialogInventorTemplates = new System.Windows.Forms.FolderBrowserDialog();
            ((System.ComponentModel.ISupportInitialize)(this.dgInvRevMapping)).BeginInit();
            this.grpBoxInventorTemplates.SuspendLayout();
            this.grpBoxRevitFamilies.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgRevitFamilies)).BeginInit();
            this.SuspendLayout();
            // 
            // dgInvRevMapping
            // 
            this.dgInvRevMapping.AllowUserToAddRows = false;
            this.dgInvRevMapping.AllowUserToDeleteRows = false;
            this.dgInvRevMapping.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgInvRevMapping.Location = new System.Drawing.Point(16, 103);
            this.dgInvRevMapping.Name = "dgInvRevMapping";
            this.dgInvRevMapping.ReadOnly = true;
            this.dgInvRevMapping.RowHeadersWidth = 82;
            this.dgInvRevMapping.RowTemplate.Height = 33;
            this.dgInvRevMapping.Size = new System.Drawing.Size(1170, 690);
            this.dgInvRevMapping.TabIndex = 0;
            // 
            // grpBoxInventorTemplates
            // 
            this.grpBoxInventorTemplates.Controls.Add(this.btnBrowse);
            this.grpBoxInventorTemplates.Controls.Add(this.txtInventorTemplatesPath);
            this.grpBoxInventorTemplates.Controls.Add(this.dgInvRevMapping);
            this.grpBoxInventorTemplates.Location = new System.Drawing.Point(13, 13);
            this.grpBoxInventorTemplates.Name = "grpBoxInventorTemplates";
            this.grpBoxInventorTemplates.Size = new System.Drawing.Size(1210, 840);
            this.grpBoxInventorTemplates.TabIndex = 1;
            this.grpBoxInventorTemplates.TabStop = false;
            this.grpBoxInventorTemplates.Text = "Mappings";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(1007, 38);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(179, 42);
            this.btnBrowse.TabIndex = 2;
            this.btnBrowse.Text = "Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // txtInventorTemplatesPath
            // 
            this.txtInventorTemplatesPath.Location = new System.Drawing.Point(17, 39);
            this.txtInventorTemplatesPath.Name = "txtInventorTemplatesPath";
            this.txtInventorTemplatesPath.Size = new System.Drawing.Size(974, 31);
            this.txtInventorTemplatesPath.TabIndex = 1;
            // 
            // grpBoxRevitFamilies
            // 
            this.grpBoxRevitFamilies.Controls.Add(this.btnSelectFromScope);
            this.grpBoxRevitFamilies.Controls.Add(this.dgRevitFamilies);
            this.grpBoxRevitFamilies.Location = new System.Drawing.Point(1249, 13);
            this.grpBoxRevitFamilies.Name = "grpBoxRevitFamilies";
            this.grpBoxRevitFamilies.Size = new System.Drawing.Size(1210, 840);
            this.grpBoxRevitFamilies.TabIndex = 2;
            this.grpBoxRevitFamilies.TabStop = false;
            this.grpBoxRevitFamilies.Text = "Revit Familiy";
            // 
            // btnSelectFromScope
            // 
            this.btnSelectFromScope.Location = new System.Drawing.Point(979, 39);
            this.btnSelectFromScope.Name = "btnSelectFromScope";
            this.btnSelectFromScope.Size = new System.Drawing.Size(211, 41);
            this.btnSelectFromScope.TabIndex = 1;
            this.btnSelectFromScope.Text = "Select from Scope";
            this.btnSelectFromScope.UseVisualStyleBackColor = true;
            this.btnSelectFromScope.Click += new System.EventHandler(this.btnSelectFromScope_Click);
            // 
            // dgRevitFamilies
            // 
            this.dgRevitFamilies.AllowUserToAddRows = false;
            this.dgRevitFamilies.AllowUserToDeleteRows = false;
            this.dgRevitFamilies.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgRevitFamilies.Location = new System.Drawing.Point(20, 103);
            this.dgRevitFamilies.Name = "dgRevitFamilies";
            this.dgRevitFamilies.ReadOnly = true;
            this.dgRevitFamilies.RowHeadersWidth = 82;
            this.dgRevitFamilies.RowTemplate.Height = 33;
            this.dgRevitFamilies.Size = new System.Drawing.Size(1170, 690);
            this.dgRevitFamilies.TabIndex = 0;
            // 
            // InventorTemplatesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(2488, 1024);
            this.Controls.Add(this.grpBoxRevitFamilies);
            this.Controls.Add(this.grpBoxInventorTemplates);
            this.Name = "InventorTemplatesForm";
            this.Text = "Inventor Templates";
            this.Load += new System.EventHandler(this.InventorTemplatesForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgInvRevMapping)).EndInit();
            this.grpBoxInventorTemplates.ResumeLayout(false);
            this.grpBoxInventorTemplates.PerformLayout();
            this.grpBoxRevitFamilies.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgRevitFamilies)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgInvRevMapping;
        private System.Windows.Forms.GroupBox grpBoxInventorTemplates;
        private System.Windows.Forms.GroupBox grpBoxRevitFamilies;
        private System.Windows.Forms.DataGridView dgRevitFamilies;
        private System.Windows.Forms.TextBox txtInventorTemplatesPath;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialogInventorTemplates;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Button btnSelectFromScope;
    }
}