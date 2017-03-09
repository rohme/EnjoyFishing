using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using EliteMMO.API;
using MiscTools;

namespace EnjoyFishing
{
    public partial class EliteAPITestForm : Form
    {
        private PolTool pol;
        private EliteAPI api;
        private ResourceTool resource;
        private EliteAPIControl control;
        private ChatTool chat;
        int lastCmdTime = 0;
        string lastCmd = string.Empty;

        public EliteAPITestForm(PolTool iPol)
        {
            InitializeComponent();
            pol = iPol;
            api = iPol.EliteAPI;
            chat = new ChatTool(api);
            resource = new ResourceTool(api);
            control = new EliteAPIControl(pol, resource, chat);
        }

        private void EliteAPITestForm_Load(object sender, EventArgs e)
        {
            lblProcID.Text = pol.ProcessID.ToString();
            lblPlayerName.Text = api.Player.Name;
            chkTopMost.Checked = false;
            trcOpacity.Value = 100;

            timRefresh.Enabled = true;
            gridStatus.RowsDefaultCellStyle.BackColor = Color.White;
            gridStatus.AlternatingRowsDefaultCellStyle.BackColor = Color.WhiteSmoke;
        }

        private void timRefresh_Tick(object sender, EventArgs e)
        {
            Dictionary<string, string> dicStatus = new Dictionary<string, string>();
            //Player
            dicStatus.Add("ログインステータス", ((LoginStatus)api.Player.LoginStatus).ToString());
            dicStatus.Add("プレイヤーステータス", ((Status)api.Player.Status).ToString());
            dicStatus.Add("エリア", string.Format("{0}({1})", api.Player.ZoneId.ToString("D"), resource.GetZoneName(api.Player.ZoneId)));
            dicStatus.Add("天気", string.Format("{0}({1})", api.Weather.CurrentWeather.ToString("D"), resource.GetWeatherName(api.Weather.CurrentWeather)));
            string statusEffectsStr = string.Empty;
            foreach (var statusEffect in api.Player.Buffs)
            {
                if (statusEffect != (int)EliteMMO.API.StatusEffect.Unknown)
                {
                    statusEffectsStr += string.Format("{0}({1}),", statusEffect.ToString("D"), resource.GetStatusName(statusEffect));
                }
            }
            dicStatus.Add("強化", statusEffectsStr);
            dicStatus.Add("釣りスキル", api.Player.CraftSkills.Fishing.Skill.ToString());
            dicStatus.Add("位置X", api.Player.X.ToString());
            dicStatus.Add("位置Y", api.Player.Y.ToString());
            dicStatus.Add("位置Z", api.Player.Z.ToString());
            dicStatus.Add("方向", api.Player.H.ToString());
            dicStatus.Add("ターゲット", api.Target.GetTargetInfo().TargetIndex.ToString());
            dicStatus.Add("MP残", api.Player.MP.ToString());
            dicStatus.Add("スニーク覚えてる？", api.Player.HasSpell(137).ToString());
            dicStatus.Add("残り詠唱時間", api.CastBar.Count.ToString());
            dicStatus.Add("詠唱時間", api.CastBar.Max.ToString());
            dicStatus.Add("詠唱時間%", api.CastBar.Percent.ToString());
            //Timer
            dicStatus.Add("スニークリキャスト時間", api.Recast.GetSpellRecast(137).ToString());
            dicStatus.Add("ヴァナ時間", string.Format("{0:0000}/{1:00}/{2:00} {3:00}:{4:00}:{5:00} {6} {7}({8}%)",
                api.VanaTime.CurrentYear,
                api.VanaTime.CurrentMonth,
                api.VanaTime.CurrentDay,
                api.VanaTime.CurrentHour,
                api.VanaTime.CurrentMinute,
                api.VanaTime.CurrentSecond,
                resource.GetDayName((int)api.VanaTime.CurrentWeekDay),
                resource.GetMoonPhaseName((int)api.VanaTime.CurrentMoonPhase, 0),
                api.VanaTime.CurrentMoonPercent));
            //Menu
            dicStatus.Add("メニュー-開いてる?", api.Menu.IsMenuOpen.ToString());
            dicStatus.Add("メニュー-名称", control.GetMenuHelpName());
            //Dialog
            dicStatus.Add("ダイアログ-質問", api.Dialog.GetDialog().Question);
            dicStatus.Add("ダイアログ-選択インデックス", api.Dialog.DialogIndex.ToString());
            dicStatus.Add("ダイアログ-選択肢", string.Join("/", api.Dialog.GetDialog().Options));
            //Fish
            dicStatus.Add("魚-ID", string.Format("{0}-{1}-{2}-{3}", api.Fish.Id1, api.Fish.Id2, api.Fish.Id3, api.Fish.Id4));
            dicStatus.Add("魚-最大HP", api.Fish.MaxStamina.ToString());
            dicStatus.Add("魚-現在HP", api.Fish.Stamina.ToString());
            dicStatus.Add("魚-残り時間", api.Fish.FightTime.ToString());
            //Item
            dicStatus.Add("選択中アイテム", string.Format("{0}({1})", api.Inventory.SelectedItemId, resource.GetItem(api.Inventory.SelectedItemId).Name[1]));
            dicStatus.Add("鞄", string.Format("{0}/{1}", control.GetInventoryCountByType(StorageContainer.Inventory), control.GetInventoryMaxByType(StorageContainer.Inventory)));
            dicStatus.Add("金庫", string.Format("{0}/{1}", control.GetInventoryCountByType(StorageContainer.Safe), control.GetInventoryMaxByType(StorageContainer.Safe)));
            dicStatus.Add("金庫2", string.Format("{0}/{1}", control.GetInventoryCountByType(StorageContainer.Safe2), control.GetInventoryMaxByType(StorageContainer.Safe2)));
            dicStatus.Add("家具", string.Format("{0}/{1}", control.GetInventoryCountByType(StorageContainer.Storage), control.GetInventoryMaxByType(StorageContainer.Storage)));
            dicStatus.Add("ロッカー", string.Format("{0}/{1}", control.GetInventoryCountByType(StorageContainer.Locker), control.GetInventoryMaxByType(StorageContainer.Locker)));
            dicStatus.Add("サッチェル", string.Format("{0}/{1}", control.GetInventoryCountByType(StorageContainer.Satchel), control.GetInventoryMaxByType(StorageContainer.Satchel)));
            dicStatus.Add("サック", string.Format("{0}/{1}", control.GetInventoryCountByType(StorageContainer.Sack), control.GetInventoryMaxByType(StorageContainer.Sack)));
            dicStatus.Add("ケース", string.Format("{0}/{1}", control.GetInventoryCountByType(StorageContainer.Case), control.GetInventoryMaxByType(StorageContainer.Case)));
            dicStatus.Add("ワードローブ", string.Format("{0}/{1}", control.GetInventoryCountByType(StorageContainer.Wardrobe), control.GetInventoryMaxByType(StorageContainer.Wardrobe)));
            dicStatus.Add("ワードローブ2", string.Format("{0}/{1}", control.GetInventoryCountByType(StorageContainer.Wardrobe2), control.GetInventoryMaxByType(StorageContainer.Wardrobe2)));
            dicStatus.Add("ワードローブ3", string.Format("{0}/{1}", control.GetInventoryCountByType(StorageContainer.Wardrobe3), control.GetInventoryMaxByType(StorageContainer.Wardrobe3)));
            dicStatus.Add("ワードローブ4", string.Format("{0}/{1}", control.GetInventoryCountByType(StorageContainer.Wardrobe4), control.GetInventoryMaxByType(StorageContainer.Wardrobe4)));
            dicStatus.Add("装備-竿", string.Format("{0}({1})", api.Inventory.GetEquippedItem((int)EquipSlot.Range).Id, resource.GetItem(api.Inventory.GetEquippedItem((int)EquipSlot.Range).Id).Name[1]));
            dicStatus.Add("装備-竿-鞄残数", control.GetInventoryItemCount((uint)api.Inventory.GetEquippedItem((int)EquipSlot.Range).Id, StorageContainer.Inventory).ToString());
            dicStatus.Add("装備-竿-ワードローブ残数", control.GetInventoryItemCount((uint)api.Inventory.GetEquippedItem((int)EquipSlot.Range).Id, StorageContainer.Wardrobe).ToString());
            dicStatus.Add("装備-竿-ワードローブ2残数", control.GetInventoryItemCount((uint)api.Inventory.GetEquippedItem((int)EquipSlot.Range).Id, StorageContainer.Wardrobe2).ToString());
            dicStatus.Add("装備-竿-ワードローブ3残数", control.GetInventoryItemCount((uint)api.Inventory.GetEquippedItem((int)EquipSlot.Range).Id, StorageContainer.Wardrobe3).ToString());
            dicStatus.Add("装備-竿-ワードローブ4残数", control.GetInventoryItemCount((uint)api.Inventory.GetEquippedItem((int)EquipSlot.Range).Id, StorageContainer.Wardrobe4).ToString());
            dicStatus.Add("装備-エサ", string.Format("{0}({1})", api.Inventory.GetEquippedItem((int)EquipSlot.Ammo).Id, resource.GetItem(api.Inventory.GetEquippedItem((int)EquipSlot.Ammo).Id).Name[1]));
            dicStatus.Add("装備-エサ-鞄残数", control.GetInventoryItemCount((uint)api.Inventory.GetEquippedItem((int)EquipSlot.Ammo).Id, StorageContainer.Inventory).ToString());
            dicStatus.Add("装備-エサ-サッチェル残数", control.GetInventoryItemCount((uint)api.Inventory.GetEquippedItem((int)EquipSlot.Ammo).Id, StorageContainer.Satchel).ToString());
            dicStatus.Add("装備-エサ-サック残数", control.GetInventoryItemCount((uint)api.Inventory.GetEquippedItem((int)EquipSlot.Ammo).Id, StorageContainer.Sack).ToString());
            dicStatus.Add("装備-エサ-ケース残数", control.GetInventoryItemCount((uint)api.Inventory.GetEquippedItem((int)EquipSlot.Ammo).Id, StorageContainer.Case).ToString());
            dicStatus.Add("装備-エサ-ワードローブ残数", control.GetInventoryItemCount((uint)api.Inventory.GetEquippedItem((int)EquipSlot.Ammo).Id, StorageContainer.Wardrobe).ToString());
            dicStatus.Add("装備-エサ-ワードローブ2残数", control.GetInventoryItemCount((uint)api.Inventory.GetEquippedItem((int)EquipSlot.Ammo).Id, StorageContainer.Wardrobe2).ToString());
            dicStatus.Add("装備-エサ-ワードローブ3残数", control.GetInventoryItemCount((uint)api.Inventory.GetEquippedItem((int)EquipSlot.Ammo).Id, StorageContainer.Wardrobe3).ToString());
            dicStatus.Add("装備-エサ-ワードローブ4残数", control.GetInventoryItemCount((uint)api.Inventory.GetEquippedItem((int)EquipSlot.Ammo).Id, StorageContainer.Wardrobe4).ToString());
            dicStatus.Add("装備-メイン", string.Format("{0}({1})", api.Inventory.GetEquippedItem((int)EquipSlot.Main).Id, resource.GetItem(api.Inventory.GetEquippedItem((int)EquipSlot.Main).Id).Name[1]));
            dicStatus.Add("装備-サブ", string.Format("{0}({1})", api.Inventory.GetEquippedItem((int)EquipSlot.Shield).Id, resource.GetItem(api.Inventory.GetEquippedItem((int)EquipSlot.Shield).Id).Name[1]));
            dicStatus.Add("装備-頭", string.Format("{0}({1})", api.Inventory.GetEquippedItem((int)EquipSlot.Head).Id, resource.GetItem(api.Inventory.GetEquippedItem((int)EquipSlot.Head).Id).Name[1]));
            dicStatus.Add("装備-胴", string.Format("{0}({1})", api.Inventory.GetEquippedItem((int)EquipSlot.Body).Id, resource.GetItem(api.Inventory.GetEquippedItem((int)EquipSlot.Body).Id).Name[1]));
            dicStatus.Add("装備-手", string.Format("{0}({1})", api.Inventory.GetEquippedItem((int)EquipSlot.Hands).Id, resource.GetItem(api.Inventory.GetEquippedItem((int)EquipSlot.Hands).Id).Name[1]));
            dicStatus.Add("装備-脚", string.Format("{0}({1})", api.Inventory.GetEquippedItem((int)EquipSlot.Legs).Id, resource.GetItem(api.Inventory.GetEquippedItem((int)EquipSlot.Legs).Id).Name[1]));
            dicStatus.Add("装備-足", string.Format("{0}({1})", api.Inventory.GetEquippedItem((int)EquipSlot.Feet).Id, resource.GetItem(api.Inventory.GetEquippedItem((int)EquipSlot.Feet).Id).Name[1]));
            dicStatus.Add("装備-首", string.Format("{0}({1})", api.Inventory.GetEquippedItem((int)EquipSlot.Neck).Id, resource.GetItem(api.Inventory.GetEquippedItem((int)EquipSlot.Neck).Id).Name[1]));
            dicStatus.Add("装備-背", string.Format("{0}({1})", api.Inventory.GetEquippedItem((int)EquipSlot.Back).Id, resource.GetItem(api.Inventory.GetEquippedItem((int)EquipSlot.Back).Id).Name[1]));
            dicStatus.Add("装備-腰", string.Format("{0}({1})", api.Inventory.GetEquippedItem((int)EquipSlot.Waist).Id, resource.GetItem(api.Inventory.GetEquippedItem((int)EquipSlot.Waist).Id).Name[1]));
            dicStatus.Add("装備-左耳", string.Format("{0}({1})", api.Inventory.GetEquippedItem((int)EquipSlot.EarLeft).Id, resource.GetItem(api.Inventory.GetEquippedItem((int)EquipSlot.EarLeft).Id).Name[1]));
            dicStatus.Add("装備-右耳", string.Format("{0}({1})", api.Inventory.GetEquippedItem((int)EquipSlot.EarRight).Id, resource.GetItem(api.Inventory.GetEquippedItem((int)EquipSlot.EarRight).Id).Name[1]));
            dicStatus.Add("装備-左手", string.Format("{0}({1})", api.Inventory.GetEquippedItem((int)EquipSlot.RingLeft).Id, resource.GetItem(api.Inventory.GetEquippedItem((int)EquipSlot.RingLeft).Id).Name[1]));
            dicStatus.Add("装備-右手", string.Format("{0}({1})", api.Inventory.GetEquippedItem((int)EquipSlot.RingRight).Id, resource.GetItem(api.Inventory.GetEquippedItem((int)EquipSlot.RingRight).Id).Name[1]));
            //Command
            int cmdTime = api.ThirdParty.ConsoleIsNewCommand();
            if (lastCmdTime != cmdTime)
            {
                lastCmdTime = cmdTime;
                lastCmd = string.Empty;
                for (int i=0;i< api.ThirdParty.ConsoleGetArgCount(); i++)
                {
                    lastCmd += api.ThirdParty.ConsoleGetArg(i) + " ";
                }
            }
            dicStatus.Add("コマンド", string.Format("{0}", lastCmd));

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
            EliteAPI.ChatEntry cl;
            while (chat.GetNextChatLine(out cl))
            {
                gridChat.Rows.Add();
                gridChat.Rows[gridChat.Rows.Count - 1].Cells[0].Value = cl.Index1;
                gridChat.Rows[gridChat.Rows.Count - 1].Cells[1].Value = cl.Index2;
                gridChat.Rows[gridChat.Rows.Count - 1].Cells[2].Value = cl.Timestamp;
                gridChat.Rows[gridChat.Rows.Count - 1].Cells[3].Value = cl.Length;
                gridChat.Rows[gridChat.Rows.Count - 1].Cells[4].Value = (ChatMode)cl.ChatType;
                gridChat.Rows[gridChat.Rows.Count - 1].Cells[5].Value = cl.Text;
                gridChat.FirstDisplayedScrollingRowIndex = gridChat.RowCount - 1;
            }
        }

        private void chkTopMost_CheckedChanged(object sender, EventArgs e)
        {
            this.TopMost = chkTopMost.Checked;
        }
        private void trcOpacity_Scroll(object sender, EventArgs e)
        {
            this.Opacity = trcOpacity.Value / 100f;
        }

    }
}
