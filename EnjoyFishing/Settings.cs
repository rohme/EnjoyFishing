using MiscTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace EnjoyFishing
{
    public class Settings
    {
        //xmlファイルに保存しないメンバ
        public int PolProcID { get; set; }
        public bool UseEnternity { get; set; }
        public bool UseItemizer { get; set; }
        public bool UseCancel { get; set; }
        public SettingsArgsModel Args { get; set; }
        //xmlファイルに保存するメンバ
        public SettingsGlobalModel Global { get; set; }
        public SettingsPlayerFormModel Form { get; set; }
        public SettingsPlayerFishListModel FishList { get; set; }
        public SettingsPlayerFishingModel Fishing { get; set; }
        public SettingsPlayerEtcModel Etc { get; set; }
        public SettingsPlayerHistoryModel History { get; set; }
        public SettingsPlayerHarakiriModel Harakiri { get; set; }

        public enum FishListModeKind
        {
            ID,
            Name,
        }
        public enum SaveModeKind
        {
            Shared,
            ByPlayer,
        }
        public enum HarakiriInputTypeKind
        {
            Select,
            Input,
        }

        private const string XML_FILENAME_SETTINGS = "EnjoyFishing.xml";
        private const string SHARED_PLAYER_NAME = "_SHARED_";

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="iPlayerName"></param>
        public Settings(string iPlayerName)
        {
            this.PolProcID = 0;
            this.UseEnternity = false;
            this.UseItemizer = false;
            this.Args = new SettingsArgsModel();

            //設定読み込み
            if (iPlayerName.Length > 0) 
            { 
                Load(iPlayerName); 
            }
            else
            {
                this.Global = new SettingsGlobalModel();
                this.Form = new SettingsPlayerFormModel();
                this.FishList = new SettingsPlayerFishListModel();
                this.Fishing = new SettingsPlayerFishingModel();
                this.Etc = new SettingsPlayerEtcModel();
                this.History = new SettingsPlayerHistoryModel();
                this.Harakiri = new SettingsPlayerHarakiriModel();
            }
        }
        /// <summary>
        /// 設定読み込み
        /// </summary>
        /// <param name="iPlayerName"></param>
        /// <returns></returns>
        public bool Load(string iPlayerName)
        {
            //xmlの読み込み
            string xmlFilename = XML_FILENAME_SETTINGS;
            SettingsModel xmlSettings = new SettingsModel();
            if (File.Exists(xmlFilename))
            {
                for (int i = 0; i < Constants.FILELOCK_RETRY_COUNT; i++)
                {
                    try
                    {
                        using (FileStream fs = new FileStream(xmlFilename, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            //シリアライズ
                            XmlSerializer serializer = new XmlSerializer(typeof(SettingsModel));
                            //読み込み
                            xmlSettings = (SettingsModel)serializer.Deserialize(fs);
                            fs.Close();
                        }
                        break;
                    }
                    catch (IOException)
                    {
                        Thread.Sleep(100);
                        continue;
                    }
                }
            }
            //メンバに設定
            this.Global = xmlSettings.Global;
            bool foundFlg = false;
            if (this.Global.SaveMode == SaveModeKind.Shared)
            {
                iPlayerName = SHARED_PLAYER_NAME;
            }
            foreach (SettingsPlayerModel v in xmlSettings.Player)
            {
                if (v.PlayerName == iPlayerName)
                {
                    foundFlg = true;
                    this.Form = v.Form;
                    this.FishList = v.FishList;
                    this.Fishing = v.Fishing;
                    this.Etc = v.Etc;
                    this.History = v.History;
                    this.Harakiri = v.Harakiri;
                }
            }
            if (!foundFlg)
            {
                SettingsPlayerModel player = new SettingsPlayerModel();
                this.Form = player.Form;
                this.FishList = player.FishList;
                this.Fishing = player.Fishing;
                this.Etc = player.Etc;
                this.History = player.History;
                this.Harakiri = player.Harakiri;
            }

            return true;
        }
        /// <summary>
        /// 設定保存
        /// </summary>
        /// <param name="iPlayerName"></param>
        /// <returns></returns>
        public bool Save(string iPlayerName)
        {
            //設定の読み込み
            string xmlFilename = XML_FILENAME_SETTINGS;
            XmlSerializer serializer;
            SettingsModel xmlSettings = new SettingsModel();

            //保存データ設定
            xmlSettings.Global = this.Global;
            bool foundFlg = false;
            if (this.Global.SaveMode == SaveModeKind.Shared)
            {
                iPlayerName = SHARED_PLAYER_NAME;
            }
            foreach (SettingsPlayerModel v in xmlSettings.Player)
            {
                if (v.PlayerName == iPlayerName)
                {
                    foundFlg = true;
                    v.Form = this.Form;
                    v.FishList = this.FishList;
                    v.Fishing = this.Fishing;
                    v.Etc = this.Etc;
                    v.History = this.History;
                    v.Harakiri = this.Harakiri;
                }
            }
            if (!foundFlg)
            {
                SettingsPlayerModel player = new SettingsPlayerModel();
                player.PlayerName = iPlayerName;
                player.Form = this.Form;
                player.FishList = this.FishList;
                player.Fishing = this.Fishing;
                player.Etc = this.Etc;
                player.History = this.History;
                player.Harakiri = this.Harakiri;

                xmlSettings.Player.Add(player);
            }
            

            //設定の保存
            for (int i = 0; i < Constants.FILELOCK_RETRY_COUNT; i++)
            {
                try
                {
                    using (FileStream fs = new FileStream(xmlFilename, FileMode.Create, FileAccess.Write, FileShare.None))//ファイルロック
                    {
                        StreamWriter sw = new StreamWriter(fs, new UTF8Encoding(false));
                        //名前空間出力抑制
                        XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                        ns.Add(String.Empty, String.Empty);
                        //シリアライズ
                        serializer = new XmlSerializer(typeof(SettingsModel));
                        serializer.Serialize(sw, xmlSettings, ns);
                        //書き込み
                        sw.Flush();
                        sw.Close();
                        sw = null;
                        fs.Close();
                    }
                    break;
                }
                catch (IOException)
                {
                    Thread.Sleep(100);
                    continue;
                }
            }
            return true;
        }
    }

    [XmlRoot("EnjoyFishing")]
    public class SettingsModel
    {
        [XmlAttribute("version")]
        public string Version { get; set; }
        public SettingsGlobalModel Global { get; set; }
        [XmlArray("Players")]
        [XmlArrayItem("Player")]
        public List<SettingsPlayerModel> Player { get; set; }

        public SettingsModel()
        {
            this.Version = "1.0.0";
            this.Global = new SettingsGlobalModel();
            this.Player = new List<SettingsPlayerModel>();
        }
    }
    public class SettingsGlobalModel
    {
        public int WaitBase { get; set; }
        public int WaitChat { get; set; }
        public int WaitEquip { get; set; }
        public Settings.SaveModeKind SaveMode { get; set; }
        public SettingsGlobalModel()
        {
            this.WaitBase = 300;
            this.WaitChat = 1000;
            this.WaitEquip = 1000;
            this.SaveMode = Settings.SaveModeKind.Shared;
        }
    }
    public class SettingsPlayerModel
    {
        [XmlAttribute("name")]
        public string PlayerName { get; set; }
        public SettingsPlayerFormModel Form { get; set; }
        public SettingsPlayerFishListModel FishList { get; set; }
        public SettingsPlayerFishingModel Fishing { get; set; }
        public SettingsPlayerEtcModel Etc { get; set; }
        public SettingsPlayerHistoryModel History { get; set; }
        public SettingsPlayerHarakiriModel Harakiri { get; set; }

        public SettingsPlayerModel()
        {
            this.PlayerName = string.Empty;
            this.Form = new SettingsPlayerFormModel();
            this.FishList = new SettingsPlayerFishListModel();
            this.Fishing = new SettingsPlayerFishingModel();
            this.Etc = new SettingsPlayerEtcModel();
            this.History = new SettingsPlayerHistoryModel();
            this.Harakiri = new SettingsPlayerHarakiriModel();
        }
    }
    public class SettingsPlayerFormModel
    {
        public int PosX { get; set; }
        public int PosY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int SplitterDistance { get; set; }
        public SettingsPlayerFormModel()
        {
            this.PosX = 0;
            this.PosY = 0;
            this.Width = 0;
            this.Height = 0;
            this.SplitterDistance = 0;
        }
    }
    public class SettingsPlayerFishListModel
    {
        public Settings.FishListModeKind Mode { get; set; }
        public bool NarrowArea { get; set; }
        public bool NarrowBait { get; set; }
        [XmlArray("Wanted")]
        [XmlArrayItem("Fish")]
        public List<SettingsPlayerFishListWantedModel> Wanted { get; set; }
        public SettingsPlayerFishListModel()
        {
            this.Mode = Settings.FishListModeKind.Name;
            this.NarrowArea = true;
            this.NarrowBait = false;
            Wanted = new List<SettingsPlayerFishListWantedModel>();
        }
        public bool IsWanted(string iFishName, int iID1 = 0, int iID2 = 0, int iID3 = 0, int iID4 = 0)
        {
            foreach (SettingsPlayerFishListWantedModel w in this.Wanted)
            {
                if (iID1 == 0 && iID2 == 0 && iID3 == 0 && iID4 == 0)
                {
                    if (w.Compare(iFishName)) return true;
                }
                else
                {
                    if (w.Compare(iFishName, iID1, iID2, iID3, iID4)) return true;
                }
            }
            return false;
        }
        public void AddWanted(string iFishName, int iID1 = 0, int iID2 = 0, int iID3 = 0, int iID4 = 0)
        {
            bool foundFlg = false;
            foreach (SettingsPlayerFishListWantedModel w in this.Wanted)
            {
                if (iID1 == 0 && iID2 == 0 && iID3 == 0 && iID4 == 0)
                    foundFlg = w.Compare(iFishName);
                else
                    foundFlg = w.Compare(iFishName, iID1, iID2, iID3, iID4);
            }
            if (!foundFlg)
            {
                this.Wanted.Add(new SettingsPlayerFishListWantedModel(iFishName, iID1, iID2, iID3, iID4));
            }
        }
        public void DelWanted(string iFishName, int iID1 = 0, int iID2 = 0, int iID3 = 0, int iID4 = 0)
        {
            bool foundFlg = false;
            for (int i = 0; i < this.Wanted.Count; i++)
            {
                if (iID1 == 0 && iID2 == 0 && iID3 == 0 && iID4 == 0)
                {
                    foundFlg = this.Wanted[i].Compare(iFishName);
                }
                else
                {
                    foundFlg = this.Wanted[i].Compare(iFishName, iID1, iID2, iID3, iID4);
                }
                if (foundFlg)
                {
                    this.Wanted.RemoveAt(i);
                    break;
                }
            }
        }
    }
    public class SettingsPlayerFishListWantedModel
    {
        [XmlAttribute("name")]
        public string FishName = string.Empty;
        [XmlAttribute("id1")]
        public int ID1 = 0;
        [XmlAttribute("id2")]
        public int ID2 = 0;
        [XmlAttribute("id3")]
        public int ID3 = 0;
        [XmlAttribute("id4")]
        public int ID4 = 0;
        public SettingsPlayerFishListWantedModel() : this(string.Empty, 0, 0, 0, 0) {}
        public SettingsPlayerFishListWantedModel(string iFishName, int iID1, int iID2, int iID3, int iID4)
        {
            this.FishName = iFishName;
            this.ID1 = iID1;
            this.ID2 = iID2;
            this.ID3 = iID3;
            this.ID4 = iID4;
        }
        public bool Compare(string iFishName)
        {
            if (this.FishName == iFishName) return true;
            return false;
        }
        public bool Compare(string iFishName, int iID1, int iID2, int iID3, int iID4)
        {
            if (this.FishName == iFishName && 
                this.ID1 == iID1 && this.ID2 == iID2 && this.ID3 == iID3 && this.ID4 == iID4) return true;
            return false;
        }
    }
    public class SettingsPlayerFishingModel
    {
        public bool Learning { get; set; }
        public bool SneakFishing { get; set; }
        public float SneakFishingRemain { get; set; }
        public bool HP0 { get; set; }
        public int HP0Min { get; set; }
        public int HP0Max { get; set; }
        public bool ReactionTime { get; set; }
        public float ReactionTimeMin { get; set; }
        public float ReactionTimeMax { get; set; }
        public bool RecastTime { get; set; }
        public float RecastTimeMin { get; set; }
        public float RecastTimeMax { get; set; }
        public bool VanaTime { get; set; }
        public int VanaTimeFrom { get; set; }
        public int VanaTimeTo { get; set; }
        public bool EarthTime { get; set; }
        public int EarthTimeFrom { get; set; }
        public int EarthTimeTo { get; set; }
        public bool IgnoreSmallFish { get; set; }
        public bool IgnoreLargeFish { get; set; }
        public bool IgnoreMonster { get; set; }
        public bool IgnoreItem { get; set; }
        public bool MaxCatch { get; set; }
        public int MaxCatchCount { get; set; }
        public bool MaxNoCatch { get; set; }
        public int MaxNoCatchCount { get; set; }
        public bool MaxSkill { get; set; }
        public int MaxSkillValue { get; set; }
        public bool ChatTell { get; set; }
        public bool ChatSay { get; set; }
        public bool ChatParty { get; set; }
        public bool ChatLs { get; set; }
        public bool ChatShout { get; set; }
        public bool ChatEmote { get; set; }
        public bool ChatRestart { get; set; }
        public int ChatRestartMinute { get; set; }
        public bool EntryPort { get; set; }
        public bool InventoryFullSack { get; set; }
        public bool InventoryFullSatchel { get; set; }
        public bool InventoryFullCase { get; set; }
        public bool InventoryFullCmd { get; set; }
        public string InventoryFullCmdLine { get; set; }
        public bool NoBaitNoRodSack { get; set; }
        public bool NoBaitNoRodSatchel { get; set; }
        public bool NoBaitNoRodCase { get; set; }
        public bool NoBaitNoRodCmd { get; set; }
        public string NoBaitNoRodCmdLine { get; set; }
        public SettingsPlayerFishingModel()
        {
            this.Learning = true;
            this.SneakFishing = false;
            this.SneakFishingRemain = 1.0F;
            this.HP0 = false;
            this.HP0Min = 5;
            this.HP0Max = 10;
            this.ReactionTime = true;
            this.ReactionTimeMin = 0.5F;
            this.ReactionTimeMax = 2.5F;
            this.RecastTime = true;
            this.RecastTimeMin = 3.0F;
            this.RecastTimeMax = 4.0F;
            this.VanaTime = false;
            this.VanaTimeFrom = 18;
            this.VanaTimeTo = 5;
            this.EarthTime = false;
            this.EarthTimeFrom = 0;
            this.EarthTimeTo = 6;
            this.IgnoreSmallFish = false;
            this.IgnoreLargeFish = false;
            this.IgnoreMonster = true;
            this.IgnoreItem = true;
            this.MaxCatch = false;
            this.MaxCatchCount = 200;
            this.MaxNoCatch = true;
            this.MaxNoCatchCount = 20;
            this.MaxSkill = false;
            this.MaxSkillValue = 110;
            this.ChatTell = true;
            this.ChatSay = true;
            this.ChatParty = false;
            this.ChatLs = false;
            this.ChatShout = true;
            this.ChatEmote = true;
            this.ChatRestart = false;
            this.ChatRestartMinute = 10;
            this.EntryPort = true;
            this.InventoryFullSack = true;
            this.InventoryFullSatchel = true;
            this.InventoryFullCase = true;
            this.InventoryFullCmd = false;
            this.InventoryFullCmdLine = "/ma デジョン <me>";
            this.NoBaitNoRodSack = true;
            this.NoBaitNoRodSatchel = true;
            this.NoBaitNoRodCase = true;
            this.NoBaitNoRodCmd = false;
            this.NoBaitNoRodCmdLine = "/ma デジョン <me>";
        }
    }
    public class SettingsPlayerEtcModel
    {
        public bool WindowTopMost { get; set; }
        public bool WindowFlash { get; set; }
        public bool WindowActivate { get; set; }
        public bool VisibleMoonPhase { get; set; }
        public bool VisibleVanaTime { get; set; }
        public bool VisibleEarthTime { get; set; }
        public bool VisibleDayType { get; set; }
        public bool VisibleLoginStatus { get; set; }
        public bool VisiblePlayerStatus { get; set; }
        public bool VisibleHpBar { get; set; }
        public bool VisibleHP { get; set; }
        public bool VisibleRemainTimeBar { get; set; }
        public bool VisibleRemainTime { get; set; }
        public SettingsPlayerEtcModel()
        {
            this.WindowTopMost = false;
            this.WindowFlash = true;
            this.WindowActivate = true;
            this.VisibleMoonPhase = true;
            this.VisibleVanaTime = true;
            this.VisibleEarthTime = false;
            this.VisibleDayType = true;
            this.VisibleLoginStatus = false;
            this.VisiblePlayerStatus = false;
            this.VisibleHpBar = true;
            this.VisibleHP = true;
            this.VisibleRemainTimeBar = true;
            this.VisibleRemainTime = true;
        }
    }
    public class SettingsPlayerHistoryModel
    {
        public string SortColName { get; set; }
        public SortOrder SortOrder { get; set; }
        public SettingsPlayerHistoryColModel ColEarthTime { get; set; }
        public SettingsPlayerHistoryColModel ColVanaTime { get; set; }
        public SettingsPlayerHistoryColModel ColVanaWeekDay { get; set; }
        public SettingsPlayerHistoryColModel ColMoonPhase { get; set; }
        public SettingsPlayerHistoryColModel ColZoneName { get; set; }
        public SettingsPlayerHistoryColModel ColRodName { get; set; }
        public SettingsPlayerHistoryColModel ColBaitName { get; set; }
        public SettingsPlayerHistoryColModel ColID { get; set; }
        public SettingsPlayerHistoryColModel ColFishName { get; set; }
        public SettingsPlayerHistoryColModel ColFishCount { get; set; }
        public SettingsPlayerHistoryColModel ColResult { get; set; }
        public SettingsPlayerHistoryModel()
        {
            this.SortColName = string.Empty;
            this.SortOrder = System.Windows.Forms.SortOrder.None;
            this.ColEarthTime = new SettingsPlayerHistoryColModel("EarthTime", 0, true, 63);
            this.ColVanaTime = new SettingsPlayerHistoryColModel("VanaTime", 1, true, 130);
            this.ColVanaWeekDay = new SettingsPlayerHistoryColModel("VanaWeekDay", 2, true, 20);
            this.ColMoonPhase = new SettingsPlayerHistoryColModel("MoonPhase", 3, true, 20);
            this.ColResult = new SettingsPlayerHistoryColModel("Result", 4, true, 60);
            this.ColZoneName = new SettingsPlayerHistoryColModel("ZoneName", 5, false, 100);
            this.ColRodName = new SettingsPlayerHistoryColModel("RodName", 6, false, 100);
            this.ColBaitName = new SettingsPlayerHistoryColModel("BaitName", 7, false, 100);
            this.ColID = new SettingsPlayerHistoryColModel("ID", 8, false, 100);
            this.ColFishName = new SettingsPlayerHistoryColModel("FishName", 9, true, 160);
            this.ColFishCount = new SettingsPlayerHistoryColModel("FishCount", 10, false, 23);
        }
    }
    public class SettingsPlayerHistoryColModel
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlAttribute("index")]
        public int DisplayIndex { get; set; }
        [XmlAttribute("visible")]
        public bool Visible { get; set; }
        [XmlAttribute("width")]
        public int Width { get; set; }
        public SettingsPlayerHistoryColModel()
        {
            this.Name = string.Empty;
            this.DisplayIndex = 0;
            this.Visible = true;
            this.Width = 100;
        }
        public SettingsPlayerHistoryColModel(string iName, int iDisplayIndex, bool iVisible, int iWidth)
        {
            this.Name = iName;
            this.DisplayIndex = iDisplayIndex;
            this.Visible = iVisible;
            this.Width = iWidth;
        }
    }
    public class SettingsPlayerHarakiriModel
    {
        public Settings.HarakiriInputTypeKind InputType { get; set; }
        public string FishNameSelect { get; set; }
        public string FishNameInput { get; set; }
        public bool StopFound { get; set; }
        public SettingsPlayerHarakiriModel()
        {
            this.InputType = Settings.HarakiriInputTypeKind.Select;
            this.FishNameSelect = string.Empty;
            this.FishNameInput = string.Empty;
            this.StopFound = false;
        }
    }
    public class SettingsArgsModel
    {
        public bool LoggerEnable { get; set; }
        public LogLevelKind LoggerLogLevel { get; set; }
        public bool LoggerVarDumpEnable { get; set; }
        public SettingsArgsModel()
        {
            LoggerEnable = false;
            LoggerLogLevel = LogLevelKind.INFO;
            LoggerVarDumpEnable = false;
        }
    }
}