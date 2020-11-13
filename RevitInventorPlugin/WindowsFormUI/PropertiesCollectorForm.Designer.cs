namespace RevitInventorExchange.WindowsFormUI
{
    partial class PropertiesCollectorForm
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
            this.btnCreateFilterFile = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.dgElementParams = new System.Windows.Forms.DataGridView();
            this.grBoxProp = new System.Windows.Forms.GroupBox();
            this.grBoxElTypeProps = new System.Windows.Forms.GroupBox();
            this.dgElementTypeParams = new System.Windows.Forms.DataGridView();
            this.btnCopyPaste = new System.Windows.Forms.Button();
            this.chckBoxApplyFilters = new System.Windows.Forms.CheckBox();
            this.lblApplyFilters = new System.Windows.Forms.Label();
            this.lblFamType = new System.Windows.Forms.Label();
            this.txtFamType = new System.Windows.Forms.TextBox();
            this.grBoxElTypeInfo = new System.Windows.Forms.GroupBox();
            this.txtFamily = new System.Windows.Forms.TextBox();
            this.lblFamily = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgElementParams)).BeginInit();
            this.grBoxElTypeProps.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgElementTypeParams)).BeginInit();
            this.grBoxElTypeInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCreateFilterFile
            // 
            this.btnCreateFilterFile.Location = new System.Drawing.Point(24, 1442);
            this.btnCreateFilterFile.Margin = new System.Windows.Forms.Padding(6);
            this.btnCreateFilterFile.Name = "btnCreateFilterFile";
            this.btnCreateFilterFile.Size = new System.Drawing.Size(256, 44);
            this.btnCreateFilterFile.TabIndex = 0;
            this.btnCreateFilterFile.Text = "Create Filter File";
            this.btnCreateFilterFile.UseVisualStyleBackColor = true;
            this.btnCreateFilterFile.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(1110, 1442);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(6);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(150, 44);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Close";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // dgElementParams
            // 
            this.dgElementParams.AllowUserToAddRows = false;
            this.dgElementParams.AllowUserToDeleteRows = false;
            this.dgElementParams.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgElementParams.Location = new System.Drawing.Point(58, 319);
            this.dgElementParams.Margin = new System.Windows.Forms.Padding(6);
            this.dgElementParams.Name = "dgElementParams";
            this.dgElementParams.ReadOnly = true;
            this.dgElementParams.RowHeadersWidth = 102;
            this.dgElementParams.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgElementParams.Size = new System.Drawing.Size(1168, 423);
            this.dgElementParams.TabIndex = 2;
            this.dgElementParams.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgElementParams_ColumnHeaderMouseClick);
            // 
            // grBoxProp
            // 
            this.grBoxProp.Location = new System.Drawing.Point(24, 277);
            this.grBoxProp.Margin = new System.Windows.Forms.Padding(6);
            this.grBoxProp.Name = "grBoxProp";
            this.grBoxProp.Padding = new System.Windows.Forms.Padding(6);
            this.grBoxProp.Size = new System.Drawing.Size(1236, 488);
            this.grBoxProp.TabIndex = 3;
            this.grBoxProp.TabStop = false;
            this.grBoxProp.Text = "Element properties";
            // 
            // grBoxElTypeProps
            // 
            this.grBoxElTypeProps.Controls.Add(this.dgElementTypeParams);
            this.grBoxElTypeProps.Location = new System.Drawing.Point(24, 781);
            this.grBoxElTypeProps.Margin = new System.Windows.Forms.Padding(6);
            this.grBoxElTypeProps.Name = "grBoxElTypeProps";
            this.grBoxElTypeProps.Padding = new System.Windows.Forms.Padding(6);
            this.grBoxElTypeProps.Size = new System.Drawing.Size(1236, 650);
            this.grBoxElTypeProps.TabIndex = 4;
            this.grBoxElTypeProps.TabStop = false;
            this.grBoxElTypeProps.Text = "Element Type properties";
            // 
            // dgElementTypeParams
            // 
            this.dgElementTypeParams.AllowUserToAddRows = false;
            this.dgElementTypeParams.AllowUserToDeleteRows = false;
            this.dgElementTypeParams.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgElementTypeParams.Location = new System.Drawing.Point(34, 37);
            this.dgElementTypeParams.Margin = new System.Windows.Forms.Padding(6);
            this.dgElementTypeParams.Name = "dgElementTypeParams";
            this.dgElementTypeParams.ReadOnly = true;
            this.dgElementTypeParams.RowHeadersWidth = 102;
            this.dgElementTypeParams.Size = new System.Drawing.Size(1168, 596);
            this.dgElementTypeParams.TabIndex = 0;
            this.dgElementTypeParams.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgElementTypeParams_ColumnHeaderMouseClick);
            // 
            // btnCopyPaste
            // 
            this.btnCopyPaste.Location = new System.Drawing.Point(558, 1442);
            this.btnCopyPaste.Margin = new System.Windows.Forms.Padding(2);
            this.btnCopyPaste.Name = "btnCopyPaste";
            this.btnCopyPaste.Size = new System.Drawing.Size(184, 44);
            this.btnCopyPaste.TabIndex = 5;
            this.btnCopyPaste.Text = "Copy / paste";
            this.btnCopyPaste.UseVisualStyleBackColor = true;
            this.btnCopyPaste.Visible = false;
            this.btnCopyPaste.Click += new System.EventHandler(this.btnCopyPaste_Click);
            // 
            // chckBoxApplyFilters
            // 
            this.chckBoxApplyFilters.AutoSize = true;
            this.chckBoxApplyFilters.Location = new System.Drawing.Point(168, 225);
            this.chckBoxApplyFilters.Margin = new System.Windows.Forms.Padding(2);
            this.chckBoxApplyFilters.Name = "chckBoxApplyFilters";
            this.chckBoxApplyFilters.Size = new System.Drawing.Size(28, 27);
            this.chckBoxApplyFilters.TabIndex = 1;
            this.chckBoxApplyFilters.UseVisualStyleBackColor = true;
            this.chckBoxApplyFilters.CheckedChanged += new System.EventHandler(this.chckBoxApplyFilters_CheckedChanged);
            // 
            // lblApplyFilters
            // 
            this.lblApplyFilters.AutoSize = true;
            this.lblApplyFilters.Location = new System.Drawing.Point(32, 225);
            this.lblApplyFilters.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblApplyFilters.Name = "lblApplyFilters";
            this.lblApplyFilters.Size = new System.Drawing.Size(130, 25);
            this.lblApplyFilters.TabIndex = 2;
            this.lblApplyFilters.Text = "Apply filters:";
            // 
            // lblFamType
            // 
            this.lblFamType.AutoSize = true;
            this.lblFamType.Location = new System.Drawing.Point(16, 120);
            this.lblFamType.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblFamType.Name = "lblFamType";
            this.lblFamType.Size = new System.Drawing.Size(129, 25);
            this.lblFamType.TabIndex = 6;
            this.lblFamType.Text = "FamilyType:";
            // 
            // txtFamType
            // 
            this.txtFamType.Location = new System.Drawing.Point(150, 117);
            this.txtFamType.Margin = new System.Windows.Forms.Padding(2);
            this.txtFamType.Name = "txtFamType";
            this.txtFamType.ReadOnly = true;
            this.txtFamType.Size = new System.Drawing.Size(1052, 31);
            this.txtFamType.TabIndex = 7;
            // 
            // grBoxElTypeInfo
            // 
            this.grBoxElTypeInfo.Controls.Add(this.txtFamily);
            this.grBoxElTypeInfo.Controls.Add(this.lblFamily);
            this.grBoxElTypeInfo.Controls.Add(this.lblFamType);
            this.grBoxElTypeInfo.Controls.Add(this.txtFamType);
            this.grBoxElTypeInfo.Location = new System.Drawing.Point(24, 38);
            this.grBoxElTypeInfo.Margin = new System.Windows.Forms.Padding(2);
            this.grBoxElTypeInfo.Name = "grBoxElTypeInfo";
            this.grBoxElTypeInfo.Padding = new System.Windows.Forms.Padding(2);
            this.grBoxElTypeInfo.Size = new System.Drawing.Size(1236, 169);
            this.grBoxElTypeInfo.TabIndex = 8;
            this.grBoxElTypeInfo.TabStop = false;
            this.grBoxElTypeInfo.Text = "Element type information";
            // 
            // txtFamily
            // 
            this.txtFamily.Location = new System.Drawing.Point(150, 54);
            this.txtFamily.Name = "txtFamily";
            this.txtFamily.ReadOnly = true;
            this.txtFamily.Size = new System.Drawing.Size(1052, 31);
            this.txtFamily.TabIndex = 9;
            // 
            // lblFamily
            // 
            this.lblFamily.AutoSize = true;
            this.lblFamily.Location = new System.Drawing.Point(16, 54);
            this.lblFamily.Name = "lblFamily";
            this.lblFamily.Size = new System.Drawing.Size(81, 25);
            this.lblFamily.TabIndex = 8;
            this.lblFamily.Text = "Family:";
            // 
            // PropertiesCollectorForm
            // 
            this.AcceptButton = this.btnCreateFilterFile;
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(1284, 1506);
            this.Controls.Add(this.grBoxElTypeInfo);
            this.Controls.Add(this.lblApplyFilters);
            this.Controls.Add(this.chckBoxApplyFilters);
            this.Controls.Add(this.btnCopyPaste);
            this.Controls.Add(this.grBoxElTypeProps);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.dgElementParams);
            this.Controls.Add(this.btnCreateFilterFile);
            this.Controls.Add(this.grBoxProp);
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "PropertiesCollectorForm";
            this.Text = "Properties Collector";
            this.Load += new System.EventHandler(this.PropertiesCollectorForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgElementParams)).EndInit();
            this.grBoxElTypeProps.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgElementTypeParams)).EndInit();
            this.grBoxElTypeInfo.ResumeLayout(false);
            this.grBoxElTypeInfo.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCreateFilterFile;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.DataGridView dgElementParams;
        private System.Windows.Forms.GroupBox grBoxProp;
        private System.Windows.Forms.GroupBox grBoxElTypeProps;
        private System.Windows.Forms.DataGridView dgElementTypeParams;
        private System.Windows.Forms.Button btnCopyPaste;
        private System.Windows.Forms.Label lblApplyFilters;
        private System.Windows.Forms.CheckBox chckBoxApplyFilters;
        private System.Windows.Forms.Label lblFamType;
        private System.Windows.Forms.TextBox txtFamType;
        private System.Windows.Forms.GroupBox grBoxElTypeInfo;
        private System.Windows.Forms.TextBox txtFamily;
        private System.Windows.Forms.Label lblFamily;
    }
}