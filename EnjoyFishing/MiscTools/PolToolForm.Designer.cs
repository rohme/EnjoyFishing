namespace MiscTools
{
    partial class SelectPolForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectPolForm));
            this.lstPol = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // lstPol
            // 
            this.lstPol.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstPol.FormattingEnabled = true;
            this.lstPol.ItemHeight = 17;
            this.lstPol.Location = new System.Drawing.Point(0, 1);
            this.lstPol.Margin = new System.Windows.Forms.Padding(4);
            this.lstPol.Name = "lstPol";
            this.lstPol.Size = new System.Drawing.Size(237, 119);
            this.lstPol.TabIndex = 0;
            this.lstPol.DoubleClick += new System.EventHandler(this.lstPol_DoubleClick);
            // 
            // SelectPolForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(239, 120);
            this.Controls.Add(this.lstPol);
            this.Font = new System.Drawing.Font("Meiryo UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SelectPolForm";
            this.Text = "POLの選択";
            this.Load += new System.EventHandler(this.FormSelectPol_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox lstPol;
    }
}