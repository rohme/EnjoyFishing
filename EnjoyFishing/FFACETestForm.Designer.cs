namespace EnjoyFishing
{
    partial class FFACETestForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FFACETestForm));
            this.timRefresh = new System.Windows.Forms.Timer(this.components);
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.chkTopMost = new System.Windows.Forms.CheckBox();
            this.lblPlayerName = new System.Windows.Forms.Label();
            this.lblProcID = new System.Windows.Forms.Label();
            this.trcOpacity = new System.Windows.Forms.TrackBar();
            this.gridStatus = new System.Windows.Forms.DataGridView();
            this.function = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.value = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gridChat = new System.Windows.Forms.DataGridView();
            this.ChatIndex = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ChatTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ChatType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FirstLine = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ChatText = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trcOpacity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridStatus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridChat)).BeginInit();
            this.SuspendLayout();
            // 
            // timRefresh
            // 
            this.timRefresh.Tick += new System.EventHandler(this.timRefresh_Tick);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.gridChat);
            this.splitContainer1.Size = new System.Drawing.Size(497, 565);
            this.splitContainer1.SplitterDistance = 473;
            this.splitContainer1.TabIndex = 1;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.chkTopMost);
            this.splitContainer2.Panel1.Controls.Add(this.lblPlayerName);
            this.splitContainer2.Panel1.Controls.Add(this.lblProcID);
            this.splitContainer2.Panel1.Controls.Add(this.trcOpacity);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.gridStatus);
            this.splitContainer2.Size = new System.Drawing.Size(497, 473);
            this.splitContainer2.SplitterDistance = 25;
            this.splitContainer2.TabIndex = 0;
            // 
            // chkTopMost
            // 
            this.chkTopMost.AutoSize = true;
            this.chkTopMost.Location = new System.Drawing.Point(141, 4);
            this.chkTopMost.Name = "chkTopMost";
            this.chkTopMost.Size = new System.Drawing.Size(59, 18);
            this.chkTopMost.TabIndex = 2;
            this.chkTopMost.Text = "最前面";
            this.chkTopMost.UseVisualStyleBackColor = true;
            this.chkTopMost.CheckedChanged += new System.EventHandler(this.chkTopMost_CheckedChanged);
            // 
            // lblPlayerName
            // 
            this.lblPlayerName.AutoSize = true;
            this.lblPlayerName.Font = new System.Drawing.Font("Meiryo UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblPlayerName.Location = new System.Drawing.Point(51, 5);
            this.lblPlayerName.Name = "lblPlayerName";
            this.lblPlayerName.Size = new System.Drawing.Size(78, 14);
            this.lblPlayerName.TabIndex = 1;
            this.lblPlayerName.Text = "PlayerName";
            // 
            // lblProcID
            // 
            this.lblProcID.AutoSize = true;
            this.lblProcID.Font = new System.Drawing.Font("Meiryo UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblProcID.Location = new System.Drawing.Point(3, 5);
            this.lblProcID.Name = "lblProcID";
            this.lblProcID.Size = new System.Drawing.Size(42, 14);
            this.lblProcID.TabIndex = 0;
            this.lblProcID.Text = "99999";
            this.lblProcID.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // trcOpacity
            // 
            this.trcOpacity.AutoSize = false;
            this.trcOpacity.LargeChange = 10;
            this.trcOpacity.Location = new System.Drawing.Point(206, 5);
            this.trcOpacity.Maximum = 100;
            this.trcOpacity.Minimum = 50;
            this.trcOpacity.Name = "trcOpacity";
            this.trcOpacity.Size = new System.Drawing.Size(104, 26);
            this.trcOpacity.TabIndex = 3;
            this.trcOpacity.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trcOpacity.Value = 50;
            this.trcOpacity.Scroll += new System.EventHandler(this.trcOpacity_Scroll);
            // 
            // gridStatus
            // 
            this.gridStatus.AllowUserToAddRows = false;
            this.gridStatus.AllowUserToDeleteRows = false;
            this.gridStatus.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridStatus.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.function,
            this.value});
            this.gridStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridStatus.Location = new System.Drawing.Point(0, 0);
            this.gridStatus.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.gridStatus.Name = "gridStatus";
            this.gridStatus.ReadOnly = true;
            this.gridStatus.RowHeadersWidth = 20;
            this.gridStatus.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.gridStatus.RowTemplate.Height = 21;
            this.gridStatus.Size = new System.Drawing.Size(497, 444);
            this.gridStatus.TabIndex = 0;
            // 
            // function
            // 
            this.function.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.function.HeaderText = "Function";
            this.function.Name = "function";
            this.function.ReadOnly = true;
            this.function.Width = 79;
            // 
            // value
            // 
            this.value.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.value.HeaderText = "Value";
            this.value.Name = "value";
            this.value.ReadOnly = true;
            this.value.Width = 61;
            // 
            // gridChat
            // 
            this.gridChat.AllowUserToAddRows = false;
            this.gridChat.AllowUserToDeleteRows = false;
            this.gridChat.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridChat.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ChatIndex,
            this.ChatTime,
            this.ChatType,
            this.FirstLine,
            this.ChatText});
            this.gridChat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridChat.Location = new System.Drawing.Point(0, 0);
            this.gridChat.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.gridChat.Name = "gridChat";
            this.gridChat.ReadOnly = true;
            this.gridChat.RowHeadersWidth = 20;
            this.gridChat.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.gridChat.RowTemplate.Height = 21;
            this.gridChat.Size = new System.Drawing.Size(497, 88);
            this.gridChat.TabIndex = 0;
            // 
            // ChatIndex
            // 
            this.ChatIndex.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ChatIndex.HeaderText = "Index";
            this.ChatIndex.Name = "ChatIndex";
            this.ChatIndex.ReadOnly = true;
            this.ChatIndex.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.ChatIndex.Width = 43;
            // 
            // ChatTime
            // 
            this.ChatTime.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ChatTime.HeaderText = "Time";
            this.ChatTime.Name = "ChatTime";
            this.ChatTime.ReadOnly = true;
            this.ChatTime.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.ChatTime.Width = 40;
            // 
            // ChatType
            // 
            this.ChatType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ChatType.HeaderText = "Type";
            this.ChatType.Name = "ChatType";
            this.ChatType.ReadOnly = true;
            this.ChatType.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.ChatType.Width = 38;
            // 
            // FirstLine
            // 
            this.FirstLine.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.FirstLine.HeaderText = "";
            this.FirstLine.Name = "FirstLine";
            this.FirstLine.ReadOnly = true;
            this.FirstLine.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.FirstLine.Width = 5;
            // 
            // ChatText
            // 
            this.ChatText.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ChatText.HeaderText = "Chat";
            this.ChatText.Name = "ChatText";
            this.ChatText.ReadOnly = true;
            this.ChatText.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.ChatText.Width = 37;
            // 
            // FFACETestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(497, 565);
            this.Controls.Add(this.splitContainer1);
            this.Font = new System.Drawing.Font("Meiryo UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "FFACETestForm";
            this.Text = "FFACEテスト";
            this.Load += new System.EventHandler(this.FFACETestForm_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.trcOpacity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridStatus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridChat)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer timRefresh;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataGridView gridChat;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.CheckBox chkTopMost;
        private System.Windows.Forms.Label lblPlayerName;
        private System.Windows.Forms.Label lblProcID;
        private System.Windows.Forms.TrackBar trcOpacity;
        private System.Windows.Forms.DataGridView gridStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn function;
        private System.Windows.Forms.DataGridViewTextBoxColumn value;
        private System.Windows.Forms.DataGridViewTextBoxColumn ChatIndex;
        private System.Windows.Forms.DataGridViewTextBoxColumn ChatTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn ChatType;
        private System.Windows.Forms.DataGridViewTextBoxColumn FirstLine;
        private System.Windows.Forms.DataGridViewTextBoxColumn ChatText;

    }
}