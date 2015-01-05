using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiscTools
{
    public partial class SelectPolForm : Form
    {
        public int PolId { get; set; }
        public List<Process> PolList { get; set; }

        private const string LIST_FORMAT = "{0,5} : {1}";
        
        public SelectPolForm()
        {
            InitializeComponent();
            PolId = 0;
            PolList = new List<Process>();
        }

        private void FormSelectPol_Load(object sender, EventArgs e)
        {
            DataTable PolTable = new DataTable();
            PolTable.Columns.Add("id", typeof(int));
            PolTable.Columns.Add("name", typeof(string));
            foreach (Process v in PolList)
            {
                DataRow row = PolTable.NewRow();
                row["id"] = v.Id;
                row["name"] = string.Format(LIST_FORMAT, v.Id, v.MainWindowTitle);
                PolTable.Rows.Add(row);
            }
            PolTable.AcceptChanges();
            lstPol.Items.Clear();
            lstPol.DataSource = PolTable;
            lstPol.ValueMember = "id";
            lstPol.DisplayMember = "name";
            if (PolId > 0)
            {
                lstPol.SelectedValue = PolId;
            }
            else
            {
                lstPol.SelectedValue = PolList[0].Id;
            }
        }

        private void lstPol_DoubleClick(object sender, EventArgs e)
        {
            PolId = (int)lstPol.SelectedValue;
            this.Hide();
        }

    }
}
