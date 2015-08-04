namespace EnjoyFishing
{
    partial class MoonPhaseForm
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
            this.gridMoonPhase = new System.Windows.Forms.DataGridView();
            this.colEarthTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colVanaTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colWeekDayImage = new System.Windows.Forms.DataGridViewImageColumn();
            this.colMoonPhaseImage = new System.Windows.Forms.DataGridViewImageColumn();
            this.colMoonPhase = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.gridMoonPhase)).BeginInit();
            this.SuspendLayout();
            // 
            // gridMoonPhase
            // 
            this.gridMoonPhase.AllowUserToAddRows = false;
            this.gridMoonPhase.AllowUserToDeleteRows = false;
            this.gridMoonPhase.AllowUserToResizeColumns = false;
            this.gridMoonPhase.AllowUserToResizeRows = false;
            this.gridMoonPhase.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridMoonPhase.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colEarthTime,
            this.colVanaTime,
            this.colWeekDayImage,
            this.colMoonPhaseImage,
            this.colMoonPhase});
            this.gridMoonPhase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridMoonPhase.Location = new System.Drawing.Point(0, 0);
            this.gridMoonPhase.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.gridMoonPhase.MultiSelect = false;
            this.gridMoonPhase.Name = "gridMoonPhase";
            this.gridMoonPhase.ReadOnly = true;
            this.gridMoonPhase.RowHeadersVisible = false;
            this.gridMoonPhase.RowTemplate.Height = 21;
            this.gridMoonPhase.Size = new System.Drawing.Size(361, 375);
            this.gridMoonPhase.TabIndex = 0;
            // 
            // colEarthTime
            // 
            this.colEarthTime.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colEarthTime.HeaderText = "地球";
            this.colEarthTime.Name = "colEarthTime";
            this.colEarthTime.ReadOnly = true;
            this.colEarthTime.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.colEarthTime.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colEarthTime.Width = 40;
            // 
            // colVanaTime
            // 
            this.colVanaTime.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colVanaTime.HeaderText = "ヴァナ";
            this.colVanaTime.Name = "colVanaTime";
            this.colVanaTime.ReadOnly = true;
            this.colVanaTime.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.colVanaTime.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colVanaTime.Width = 42;
            // 
            // colWeekDayImage
            // 
            this.colWeekDayImage.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colWeekDayImage.HeaderText = "曜";
            this.colWeekDayImage.Name = "colWeekDayImage";
            this.colWeekDayImage.ReadOnly = true;
            this.colWeekDayImage.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.colWeekDayImage.Width = 27;
            // 
            // colMoonPhaseImage
            // 
            this.colMoonPhaseImage.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colMoonPhaseImage.HeaderText = "月";
            this.colMoonPhaseImage.Name = "colMoonPhaseImage";
            this.colMoonPhaseImage.ReadOnly = true;
            this.colMoonPhaseImage.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.colMoonPhaseImage.Width = 27;
            // 
            // colMoonPhase
            // 
            this.colMoonPhase.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colMoonPhase.HeaderText = "月齢";
            this.colMoonPhase.Name = "colMoonPhase";
            this.colMoonPhase.ReadOnly = true;
            this.colMoonPhase.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.colMoonPhase.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colMoonPhase.Width = 40;
            // 
            // MoonPhaseForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(361, 375);
            this.Controls.Add(this.gridMoonPhase);
            this.Font = new System.Drawing.Font("Meiryo UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MoonPhaseForm";
            this.Text = "月齢カレンダー";
            this.Load += new System.EventHandler(this.MoonPhaseForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gridMoonPhase)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView gridMoonPhase;
        private System.Windows.Forms.DataGridViewTextBoxColumn colEarthTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn colVanaTime;
        private System.Windows.Forms.DataGridViewImageColumn colWeekDayImage;
        private System.Windows.Forms.DataGridViewImageColumn colMoonPhaseImage;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMoonPhase;
    }
}