namespace RevitInventorExchange.WindowsFormUI
{
    partial class RevitFamiliesParametersSelectionPopup
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
            this.grBoxSelParams = new System.Windows.Forms.GroupBox();
            this.dgRevitFamParams = new System.Windows.Forms.DataGridView();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnSelect = new System.Windows.Forms.Button();
            this.grBoxSelParams.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgRevitFamParams)).BeginInit();
            this.SuspendLayout();
            // 
            // grBoxSelParams
            // 
            this.grBoxSelParams.Controls.Add(this.dgRevitFamParams);
            this.grBoxSelParams.Location = new System.Drawing.Point(13, 22);
            this.grBoxSelParams.Name = "grBoxSelParams";
            this.grBoxSelParams.Size = new System.Drawing.Size(775, 891);
            this.grBoxSelParams.TabIndex = 0;
            this.grBoxSelParams.TabStop = false;
            this.grBoxSelParams.Text = "Select Parameters";
            // 
            // dgRevitFamParams
            // 
            this.dgRevitFamParams.AllowUserToAddRows = false;
            this.dgRevitFamParams.AllowUserToDeleteRows = false;
            this.dgRevitFamParams.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgRevitFamParams.Location = new System.Drawing.Point(16, 44);
            this.dgRevitFamParams.Name = "dgRevitFamParams";
            this.dgRevitFamParams.ReadOnly = true;
            this.dgRevitFamParams.RowHeadersWidth = 82;
            this.dgRevitFamParams.RowTemplate.Height = 33;
            this.dgRevitFamParams.Size = new System.Drawing.Size(736, 790);
            this.dgRevitFamParams.TabIndex = 0;
            this.dgRevitFamParams.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.dgRevitFamParams_MouseDoubleClick);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(590, 935);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(198, 50);
            this.btnClose.TabIndex = 4;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnSelect
            // 
            this.btnSelect.Location = new System.Drawing.Point(13, 935);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(159, 51);
            this.btnSelect.TabIndex = 3;
            this.btnSelect.Text = "Select";
            this.btnSelect.UseVisualStyleBackColor = true;
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // RevitFamiliesParametersSelectionPopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 1008);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnSelect);
            this.Controls.Add(this.grBoxSelParams);
            this.Name = "RevitFamiliesParametersSelectionPopup";
            this.Text = "Revit Families Parameters";
            this.Load += new System.EventHandler(this.RevitFamiliesParametersSelectionPopup_Load);
            this.grBoxSelParams.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgRevitFamParams)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grBoxSelParams;
        private System.Windows.Forms.DataGridView dgRevitFamParams;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnSelect;
    }
}