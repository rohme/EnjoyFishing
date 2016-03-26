using EnjoyFishing.Properties;
using MiscTools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EliteMMO.API;

namespace EnjoyFishing
{
    public partial class MoonPhaseForm : Form
    {
        private PolTool pol;
        private EliteAPI api;
        public MoonPhaseForm(PolTool iPol)
        {
            InitializeComponent();
            pol = iPol;
            api = iPol.EliteAPI;
        }

        private void MoonPhaseForm_Load(object sender, EventArgs e)
        {
            this.Icon = Icon.FromHandle(Resources.IMAGE_MOON02.GetHicon());
            var v1 = new VanaTime()
            {
                Year = api.VanaTime.CurrentYear,
                Month = api.VanaTime.CurrentMonth,
                Day = api.VanaTime.CurrentDay,
                Hour = api.VanaTime.CurrentHour,
                Minute = api.VanaTime.CurrentMinute,
                Second = api.VanaTime.CurrentSecond,
            };

            MoonPhase lastMoonPhase = EliteAPIControl.GetMoonPhaseFromVanaTime(v1);

            for (int i = 0; i <= 4; i++)
            {
                gridMoonPhase.Columns[i].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                gridMoonPhase.Columns[i].HeaderCell.Style.Font = new Font(gridMoonPhase.Font,gridMoonPhase.Font.Style | FontStyle.Bold);
            }
            MoonPhase last = EliteAPIControl.GetMoonPhaseFromVanaTime(v1);
            for (int i = 0; i < 360; i++)
            {
                var v2 = EliteAPIControl.addVanaDay(v1, i);
                MoonPhase m = EliteAPIControl.GetMoonPhaseFromVanaTime(v2);
                if (last != m)
                {
                    gridMoonPhase.Rows.Add();
                    gridMoonPhase.Rows[gridMoonPhase.Rows.Count - 1].Cells[0].Value = EliteAPIControl.GetEarthTimeFromVanaTime(v2).ToString("yyyy/MM/dd HH:mm");
                    gridMoonPhase.Rows[gridMoonPhase.Rows.Count - 1].Cells[1].Value = string.Format("{0:0000}/{1:00}/{2:00}", v2.Year, v2.Month, v2.Day);
                    gridMoonPhase.Rows[gridMoonPhase.Rows.Count - 1].Cells[2].Value = MainForm.dicWeekDayImage[EliteAPIControl.GetWeekdayFromVanaTime(v2)];
                    gridMoonPhase.Rows[gridMoonPhase.Rows.Count - 1].Cells[3].Value = MainForm.dicMoonPhaseImage[m];
                    gridMoonPhase.Rows[gridMoonPhase.Rows.Count - 1].Cells[4].Value = MainForm.dicMoonPhaseName[m];
                    //行の色変更
                    gridMoonPhase.Rows[gridMoonPhase.Rows.Count - 1].DefaultCellStyle.ForeColor = Color.Black;
                    gridMoonPhase.Rows[gridMoonPhase.Rows.Count - 1].DefaultCellStyle.SelectionForeColor = Color.Black;
                    if (m == MoonPhase.New || m == MoonPhase.Full)
                    {
                        gridMoonPhase.Rows[gridMoonPhase.Rows.Count - 1].DefaultCellStyle.BackColor = Color.FromArgb(0x80, 0xFF, 0xFF);
                        gridMoonPhase.Rows[gridMoonPhase.Rows.Count - 1].DefaultCellStyle.SelectionBackColor = Color.FromArgb(0x80, 0xFF, 0xFF);
                    }
                    else if (m == MoonPhase.FirstQuarter || m == MoonPhase.LastQuarter)
                    {
                        gridMoonPhase.Rows[gridMoonPhase.Rows.Count - 1].DefaultCellStyle.BackColor = Color.LightGray;
                        gridMoonPhase.Rows[gridMoonPhase.Rows.Count - 1].DefaultCellStyle.SelectionBackColor = Color.LightGray;
                    }
                    else
                    {
                        gridMoonPhase.Rows[gridMoonPhase.Rows.Count - 1].DefaultCellStyle.BackColor = Color.White;
                        gridMoonPhase.Rows[gridMoonPhase.Rows.Count - 1].DefaultCellStyle.SelectionBackColor = Color.White;
                    }
                }
                last = m;
            }

        }
    }
}
