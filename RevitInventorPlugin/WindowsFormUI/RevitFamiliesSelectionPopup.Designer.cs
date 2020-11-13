namespace RevitInventorExchange.WindowsFormUI
{
    partial class RevitFamiliesSelectionPopup
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
            this.grBoxSelCurrScope = new System.Windows.Forms.GroupBox();
            this.dgRevitFamilies = new System.Windows.Forms.DataGridView();
            this.btnSelect = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.grBoxSelCurrScope.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgRevitFamilies)).BeginInit();
            this.SuspendLayout();
            // 
            // grBoxSelCurrScope
            // 
            this.grBoxSelCurrScope.Controls.Add(this.dgRevitFamilies);
            this.grBoxSelCurrScope.Location = new System.Drawing.Point(13, 22);
            this.grBoxSelCurrScope.Name = "grBoxSelCurrScope";
            this.grBoxSelCurrScope.Size = new System.Drawing.Size(775, 863);
            this.grBoxSelCurrScope.TabIndex = 0;
            this.grBoxSelCurrScope.TabStop = false;
            this.grBoxSelCurrScope.Text = "Select from Current Scope";
            // 
            // dgRevitFamilies
            // 
            this.dgRevitFamilies.AllowUserToAddRows = false;
            this.dgRevitFamilies.AllowUserToDeleteRows = false;
            this.dgRevitFamilies.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgRevitFamilies.Location = new System.Drawing.Point(16, 44);
            this.dgRevitFamilies.Name = "dgRevitFamilies";
            this.dgRevitFamilies.ReadOnly = true;
            this.dgRevitFamilies.RowHeadersWidth = 82;
            this.dgRevitFamilies.RowTemplate.Height = 33;
            this.dgRevitFamilies.Size = new System.Drawing.Size(736, 790);
            this.dgRevitFamilies.TabIndex = 0;
            this.dgRevitFamilies.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.dgRevitFamilies_MouseDoubleClick);
            // 
            // btnSelect
            // 
            this.btnSelect.Location = new System.Drawing.Point(13, 932);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(159, 51);
            this.btnSelect.TabIndex = 1;
            this.btnSelect.Text = "Select";
            this.btnSelect.UseVisualStyleBackColor = true;
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(590, 932);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(198, 50);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // RevitFamiliesSelectionPopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 1008);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnSelect);
            this.Controls.Add(this.grBoxSelCurrScope);
            this.Name = "RevitFamiliesSelectionPopup";
            this.Text = "Revit Families";
            this.Load += new System.EventHandler(this.RevitFamiliesSelectionPopup_Load);
            this.grBoxSelCurrScope.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgRevitFamilies)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grBoxSelCurrScope;
        private System.Windows.Forms.DataGridView dgRevitFamilies;
        private System.Windows.Forms.Button btnSelect;
        private System.Windows.Forms.Button btnClose;
    }
}