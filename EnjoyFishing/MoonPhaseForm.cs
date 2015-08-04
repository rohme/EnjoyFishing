using EnjoyFishing.Properties;
using FFACETools;
using MiscTools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EnjoyFishing
{
    public partial class MoonPhaseForm : Form
    {
        private FFACE fface;
        public MoonPhaseForm(FFACE iFFACE)
        {
            InitializeComponent();
            fface = iFFACE;
        }

        private void MoonPhaseForm_Load(object sender, EventArgs e)
        {
            this.Icon = Icon.FromHandle(Resources.IMAGE_MOON02.GetHicon());

            MoonPhase lastMoonPhase = FFACEControl.GetMoonPhaseFromVanaTime(fface.Timer.GetVanaTime());

            FFACE.TimerTools.VanaTime v = new FFACE.TimerTools.VanaTime();
            v.Year = fface.Timer.GetVanaTime().Year;
            v.Month = fface.Timer.GetVanaTime().Month;
            v.Day = fface.Timer.GetVanaTime().Day;

            for (int i = 0; i <= 4; i++)
            {
                gridMoonPhase.Columns[i].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                gridMoonPhase.Columns[i].HeaderCell.Style.Font = new Font(gridMoonPhase.Font,gridMoonPhase.Font.Style | FontStyle.Bold);
            }
            MoonPhase last = FFACEControl.GetMoonPhaseFromVanaTime(v);
            for (int i = 0; i < 360; i++)
            {
                v = FFACEControl.addVanaDay(v);
                MoonPhase m = FFACEControl.GetMoonPhaseFromVanaTime(v);
                if (last != m)
                {
                    gridMoonPhase.Rows.Add();
                    gridMoonPhase.Rows[gridMoonPhase.Rows.Count - 1].Cells[0].Value = FFACEControl.GetEarthTimeFromVanaTime(v).ToString("yyyy/MM/dd HH:mm");
                    gridMoonPhase.Rows[gridMoonPhase.Rows.Count - 1].Cells[1].Value = string.Format("{0:0000}/{1:00}/{2:00}", v.Year, v.Month, v.Day);
                    gridMoonPhase.Rows[gridMoonPhase.Rows.Count - 1].Cells[2].Value = MainForm.dicWeekDayImage[FFACEControl.GetWeekdayFromVanaTime(v)];
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
