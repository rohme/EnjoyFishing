using FFACETools;
using MiscTools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace EnjoyFishing
{
    public partial class FFACETestForm : Form
    {
        private PolTool pol;
        private FFACETools.FFACE fface;
        public FFACETestForm(PolTool iPol)
        {
            InitializeComponent();
            pol = iPol;
            fface = iPol.FFACE;
        }

        private void FFACETestForm_Load(object sender, EventArgs e)
        {
            timRefresh.Enabled = true;
            gridStatus.RowsDefaultCellStyle.BackColor = Color.White;
            gridStatus.AlternatingRowsDefaultCellStyle.BackColor = Color.WhiteSmoke;
        }

        private void timRefresh_Tick(object sender, EventArgs e)
        {
            Dictionary<string, string> dicStatus = new Dictionary<string, string>();
            dicStatus.Add("ProcessID", pol.ProcessID.ToString());
            //Player
            dicStatus.Add("Player.Name", fface.Player.Name);
            dicStatus.Add("Player.GetLoginStatus", fface.Player.GetLoginStatus.ToString());
            dicStatus.Add("Player.Status", fface.Player.Status.ToString());
            dicStatus.Add("Player.Zone", string.Format("{0}({1})", fface.Player.Zone.ToString("D"), FFACE.ParseResources.GetAreaName(fface.Player.Zone)));
            string statusEffectsStr = string.Empty;
            foreach(StatusEffect statusEffect in fface.Player.StatusEffects)
            {
                if (statusEffect != StatusEffect.Unknown)
                {
                    statusEffectsStr += string.Format("{0}({1}),", statusEffect.ToString("D"), statusEffect);
                }
            }
            dicStatus.Add("Player.StatusEffects", statusEffectsStr);
            dicStatus.Add("Player.GetCraftDetails(Craft.Fishing).Level", fface.Player.GetCraftDetails(Craft.Fishing).Level.ToString());
            dicStatus.Add("Player.PosX", fface.Player.PosX.ToString());
            dicStatus.Add("Player.PosY", fface.Player.PosY.ToString());
            dicStatus.Add("Player.PosZ", fface.Player.PosZ.ToString());
            dicStatus.Add("Player.PosH", fface.Player.PosH.ToString());
            //Timer
            dicStatus.Add("Timer.GetVanaTime()", fface.Timer.GetVanaTime().ToString());
            //Fish
            dicStatus.Add("Fish.HPMax", fface.Fish.HPMax.ToString());
            dicStatus.Add("Fish.HPCurrent", fface.Fish.HPCurrent.ToString());
            dicStatus.Add("Fish.Timeout", fface.Fish.Timeout.ToString());
            //Item
            dicStatus.Add("Item.InventoryMax", fface.Item.InventoryMax.ToString());
            dicStatus.Add("Item.InventoryCount", fface.Item.InventoryCount.ToString());
            //dicStatus.Add("Item.SafeMax", fface.Item.SafeMax.ToString());
            //dicStatus.Add("Item.SafeCount", fface.Item.SafeCount.ToString());
            //dicStatus.Add("Item.StorageMax", fface.Item.StorageMax.ToString());
            //dicStatus.Add("Item.StorageCount", fface.Item.StorageCount.ToString());
            //dicStatus.Add("Item.LockerMax", fface.Item.LockerMax.ToString());
            //dicStatus.Add("Item.LockerCount", fface.Item.LockerCount.ToString());
            dicStatus.Add("Item.SatchelMax", fface.Item.SatchelMax.ToString());
            dicStatus.Add("Item.SatchelCount", fface.Item.SatchelCount.ToString());
            dicStatus.Add("Item.SackMax", fface.Item.SackMax.ToString());
            dicStatus.Add("Item.SackCount", fface.Item.SackCount.ToString());
            //dicStatus.Add("Item.TemporaryMax", fface.Item.TemporaryMax.ToString());
            //dicStatus.Add("Item.TemporaryCount", fface.Item.TemporaryCount.ToString());
            dicStatus.Add("Item.CaseMax", fface.Item.CaseMax.ToString());
            dicStatus.Add("Item.CaseCount", fface.Item.CaseCount.ToString());
            //dicStatus.Add("Item.WardrobeMax", fface.Item.WardrobeMax.ToString());
            //dicStatus.Add("Item.WardrobeCount", fface.Item.WardrobeCount.ToString());
            dicStatus.Add("Item.GetEquippedItemID(EquipSlot.Range)", fface.Item.GetEquippedItemID(EquipSlot.Range).ToString());
            dicStatus.Add("Item.GetItemCount(RangeItemID, InventoryType.Inventory)", fface.Item.GetItemCount(fface.Item.GetEquippedItemID(EquipSlot.Range), InventoryType.Inventory).ToString());
            dicStatus.Add("ParseResources.GetItemName(RangeItemID)", FFACE.ParseResources.GetItemName(fface.Item.GetEquippedItemID(EquipSlot.Range)));
            dicStatus.Add("Item.GetEquippedItemID(EquipSlot.Ammo)", fface.Item.GetEquippedItemID(EquipSlot.Ammo).ToString());
            dicStatus.Add("Item.GetItemCount(AmmoItemID, InventoryType.Inventory)", fface.Item.GetItemCount(fface.Item.GetEquippedItemID(EquipSlot.Ammo), InventoryType.Inventory).ToString());
            dicStatus.Add("ParseResources.GetItemName(AmmoItemID)", FFACE.ParseResources.GetItemName(fface.Item.GetEquippedItemID(EquipSlot.Ammo)));
            //Windower
            //dicStatus.Add("Windower.ArgumentCount", fface.Windower.ArgumentCount().ToString());
            //string argumentStr = string.Empty;
            //for (short i = 0; i < fface.Windower.ArgumentCount(); i++)
            //{
            ////    argumentStr += fface.Windower.GetArgument(i) + " ";
            //}
            //dicStatus.Add("Windower.GetArgument", argumentStr);
            //dicStatus.Add("Windower.IsNewCommand", fface.Windower.IsNewCommand().ToString());

            refreshStatus(dicStatus);
        }

        private void refreshStatus(Dictionary<string, string> iStatus)
        {
            //gridStatus
            foreach (KeyValuePair<string, string> status in iStatus)
            {
                int idx = -1;
                for (int i = 0; i < gridStatus.RowCount; i++)
                {
                    if (gridStatus.Rows[i].Cells[0].Value.ToString() == status.Key)
                    {
                        idx = i;
                        break;
                    }
                }
                if (idx < 0)
                {
                    gridStatus.Rows.Add();
                    idx = gridStatus.Rows.Count - 1;
                }
                gridStatus.Rows[idx].Cells[0].Value = status.Key;
                gridStatus.Rows[idx].Cells[1].Value = status.Value;
            }
            //gridChat
            if (fface.Chat.IsNewLine)
            {
                FFACE.ChatTools.ChatLine cl = fface.Chat.GetNextLine();
                while (cl != null)
                {
                    gridChat.Rows.Add();
                    gridChat.Rows[gridChat.Rows.Count - 1].Cells[0].Value = cl.Index;
                    gridChat.Rows[gridChat.Rows.Count - 1].Cells[1].Value = cl.NowDate.ToString("hh:mm:ss");
                    gridChat.Rows[gridChat.Rows.Count - 1].Cells[2].Value = cl.Type;
                    gridChat.Rows[gridChat.Rows.Count - 1].Cells[3].Value = cl.Text;
                    cl = fface.Chat.GetNextLine();
                }
            }
        }

    }
}
