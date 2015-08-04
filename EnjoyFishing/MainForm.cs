using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Media;
using FFACETools;
using MiscTools;
using EnjoyFishing.Properties;
using System.IO;

namespace EnjoyFishing
{
    public partial class MainForm : Form
    {
        #region Dictionary
        #region dicMoonPhaseName
        public static Dictionary<MoonPhase, string> dicMoonPhaseName = new Dictionary<MoonPhase, string>()
        {
            {MoonPhase.New, "新月"},
            {MoonPhase.WaxingCrescent, "三日月"},
            {MoonPhase.WaxingCrescent2, "七日月"},
            {MoonPhase.FirstQuarter, "上弦の月"},
            {MoonPhase.WaxingGibbous, "十日夜"},
            {MoonPhase.WaxingGibbous2, "十三夜"},
            {MoonPhase.Full, "満月"},
            {MoonPhase.WaningGibbous, "十六夜"},
            {MoonPhase.WaningGibbous2, "居待月"},
            {MoonPhase.LastQuarter, "下弦の月"},
            {MoonPhase.WaningCrescent, "二十日余月"},
            {MoonPhase.WaningCrescent2, "二十六夜"},
            {MoonPhase.Unknown, string.Empty}
        };
        #endregion
        #region dicMoonPhaseImage
        public static Dictionary<MoonPhase, Image> dicMoonPhaseImage = new Dictionary<MoonPhase, Image>()
        {
            {MoonPhase.New, Resources.IMAGE_MOON00},
            {MoonPhase.WaxingCrescent, Resources.IMAGE_MOON01},
            {MoonPhase.WaxingCrescent2, Resources.IMAGE_MOON02},
            {MoonPhase.FirstQuarter, Resources.IMAGE_MOON03},
            {MoonPhase.WaxingGibbous, Resources.IMAGE_MOON04},
            {MoonPhase.WaxingGibbous2, Resources.IMAGE_MOON05},
            {MoonPhase.Full, Resources.IMAGE_MOON06},
            {MoonPhase.WaningGibbous, Resources.IMAGE_MOON07},
            {MoonPhase.WaningGibbous2, Resources.IMAGE_MOON08},
            {MoonPhase.LastQuarter, Resources.IMAGE_MOON09},
            {MoonPhase.WaningCrescent, Resources.IMAGE_MOON10},
            {MoonPhase.WaningCrescent2, Resources.IMAGE_MOON11},
            {MoonPhase.Unknown, null}
        };
        #endregion
        #region dicWeekDayName
        public static Dictionary<Weekday, string> dicWeekDayName = new Dictionary<Weekday, string>()
        {
            {Weekday.Firesday, "火曜日"},
            {Weekday.Earthsday, "土曜日"},
            {Weekday.Watersday, "水曜日"},
            {Weekday.Windsday, "風曜日"},
            {Weekday.Iceday, "氷曜日"},
            {Weekday.Lightningday, "雷曜日"},
            {Weekday.Lightsday, "光曜日"},
            {Weekday.Darksday, "闇曜日"},
            {Weekday.Unknown, string.Empty}
        };
        #endregion
        #region dicWeekDayImage
        public static Dictionary<Weekday, Image> dicWeekDayImage = new Dictionary<Weekday, Image>()
        {
            {Weekday.Firesday, Resources.IMAGE_WEEK_FIRE},
            {Weekday.Earthsday, Resources.IMAGE_WEEK_EARTH},
            {Weekday.Watersday, Resources.IMAGE_WEEK_WATER},
            {Weekday.Windsday, Resources.IMAGE_WEEK_WIND},
            {Weekday.Iceday, Resources.IMAGE_WEEK_ICE},
            {Weekday.Lightningday, Resources.IMAGE_WEEK_LIGHTNING},
            {Weekday.Lightsday, Resources.IMAGE_WEEK_LIGHT},
            {Weekday.Darksday, Resources.IMAGE_WEEK_DARK},
            {Weekday.Unknown, null}
        };
        #endregion
        private static Dictionary<GuildTimeTableKind, TimeTable> dicTimeTable = new Dictionary<GuildTimeTableKind, TimeTable>()
        {
            {GuildTimeTableKind.WINDUST, new TimeTable(2,4)},
            {GuildTimeTableKind.SELBINA, new TimeTable(3,18)},
            {GuildTimeTableKind.BIBIKI, new TimeTable(1,18)},
            {GuildTimeTableKind.WHITEGATE, new TimeTable(1,18)},
            {GuildTimeTableKind.SHIP, new TimeTable(3,23)},
        };
        #region lstGridHistory
        private static List<GridColModel> lstGridHistory = new List<GridColModel>()
        {
            new GridColModel(typeof(DataGridViewTextBoxColumn), "EarthTime",   "地球",       0, true,  50, DataGridViewContentAlignment.MiddleLeft,   "HH:mm:ss"   ,DataGridViewTriState.False),
            new GridColModel(typeof(DataGridViewTextBoxColumn), "VanaTime",    "ヴァナ",     1, true,  50, DataGridViewContentAlignment.MiddleLeft,   string.Empty ,DataGridViewTriState.False),
            new GridColModel(typeof(DataGridViewImageColumn),   "VanaWeekDay", "曜",         2, true,  50, DataGridViewContentAlignment.MiddleCenter, string.Empty ,DataGridViewTriState.False),
            new GridColModel(typeof(DataGridViewImageColumn),   "MoonPhase",   "月",         3, true,  50, DataGridViewContentAlignment.MiddleCenter, string.Empty ,DataGridViewTriState.False),
            new GridColModel(typeof(DataGridViewTextBoxColumn), "Result",      "結果",       4, true,  50, DataGridViewContentAlignment.MiddleLeft,   string.Empty ,DataGridViewTriState.False),
            new GridColModel(typeof(DataGridViewTextBoxColumn), "ZoneName",    "エリア",     5, true,  50, DataGridViewContentAlignment.MiddleLeft,   string.Empty ,DataGridViewTriState.True),
            new GridColModel(typeof(DataGridViewTextBoxColumn), "RodName",     "竿",         6, true,  50, DataGridViewContentAlignment.MiddleLeft,   string.Empty ,DataGridViewTriState.True),
            new GridColModel(typeof(DataGridViewTextBoxColumn), "BaitName",    "エサ",       7, true,  50, DataGridViewContentAlignment.MiddleLeft,   string.Empty ,DataGridViewTriState.True),
            new GridColModel(typeof(DataGridViewTextBoxColumn), "ID",          "ID",         8, true,  50, DataGridViewContentAlignment.MiddleLeft,   string.Empty ,DataGridViewTriState.True),
            new GridColModel(typeof(DataGridViewTextBoxColumn), "FishName",    "魚",         9, true,  50, DataGridViewContentAlignment.MiddleLeft,   string.Empty ,DataGridViewTriState.True),
            new GridColModel(typeof(DataGridViewTextBoxColumn), "FishCount",   "匹",        10, true,  50, DataGridViewContentAlignment.MiddleCenter, string.Empty ,DataGridViewTriState.False),
        };
        #endregion
        #region lstGridFishingInfo
        private static List<GridColModel> lstGridFishingInfo = new List<GridColModel>()
        {
            new GridColModel(typeof(DataGridViewTextBoxColumn), "Result",     "結果",       0, true,  60, DataGridViewContentAlignment.MiddleLeft,   string.Empty ,DataGridViewTriState.False),
            new GridColModel(typeof(DataGridViewTextBoxColumn), "FishName",   "魚",         1, true, 160, DataGridViewContentAlignment.MiddleLeft,   string.Empty ,DataGridViewTriState.True),
            new GridColModel(typeof(DataGridViewTextBoxColumn), "Count",      "回",         2, true,  50, DataGridViewContentAlignment.MiddleRight,  string.Empty ,DataGridViewTriState.False),
            new GridColModel(typeof(DataGridViewTextBoxColumn), "ResultPer",  "結果%",      3, true,  50, DataGridViewContentAlignment.MiddleRight,  string.Empty ,DataGridViewTriState.False),
            new GridColModel(typeof(DataGridViewTextBoxColumn), "AllPer",     "全体%",      4, true,  50, DataGridViewContentAlignment.MiddleRight,  string.Empty ,DataGridViewTriState.False),
        };
        #endregion
        #region lstGridHarakiri
        private static List<GridColModel> lstGridHarakiri = new List<GridColModel>()
        {
            new GridColModel(typeof(DataGridViewTextBoxColumn), "FishName",   "魚",         0, true, 160, DataGridViewContentAlignment.MiddleLeft,   string.Empty ,DataGridViewTriState.True),
            new GridColModel(typeof(DataGridViewTextBoxColumn), "ItemName",   "アイテム",   1, true, 160, DataGridViewContentAlignment.MiddleLeft,   string.Empty ,DataGridViewTriState.True),
            new GridColModel(typeof(DataGridViewTextBoxColumn), "Count",      "回",         2, true,  50, DataGridViewContentAlignment.MiddleRight,  string.Empty ,DataGridViewTriState.False),
            new GridColModel(typeof(DataGridViewTextBoxColumn), "Percent",    "%",          3, true,  60, DataGridViewContentAlignment.MiddleRight,  string.Empty ,DataGridViewTriState.False),
        };
        #endregion
        #region lstGridCaughtFishes
        private static List<GridColModel> lstGridCaughtFishes = new List<GridColModel>()
        {
            new GridColModel(typeof(DataGridViewTextBoxColumn), "No",       "No", 0, true,  30, DataGridViewContentAlignment.MiddleRight,  string.Empty ,DataGridViewTriState.True),
            new GridColModel(typeof(DataGridViewTextBoxColumn), "Caught",   "☆", 1, true,  20, DataGridViewContentAlignment.MiddleLeft,   string.Empty ,DataGridViewTriState.True),
            new GridColModel(typeof(DataGridViewTextBoxColumn), "FishName", "魚", 2, true, 200, DataGridViewContentAlignment.MiddleLeft,   string.Empty ,DataGridViewTriState.True),
        };
        #endregion
        #endregion

        #region enums
        private enum GuildTimeTableKind
        {
            WINDUST,
            SELBINA,
            BIBIKI,
            WHITEGATE,
            SHIP,
        }
        private enum ShipTimeTableKind
        {
            SELBINA_MHAURA,
            MHAURA_WHITEGATE,
        }
        #endregion

        private PolTool pol;
        private FFACE fface;
        private LoggerTool logger;
        private Settings settings;
        private ChatTool chat;
        private FishingTool fishing;
        private HarakiriTool harakiri;
        private FFACEControl control;
        private FishDB fishDB;
        private FishHistoryDB fishHistoryDB;
        private HarakiriDB harakiriDB;
        private UpdateDBTool updatedb;

        private Thread thFishing;
        private Thread thHarakiri;
        private Thread thUpdateDB;
        private Thread thCaughtFishes;
        private Thread thMonitor;
        private bool loginFlg = true;//キャラクターログイン中のフラグ
        private bool startupFlg = false;//初期化中のフラグ
        private bool fishingFlg = false;//釣り中のフラグ
        private bool harakiriFlg = false;//ハラキリ中のフラグ
        private bool updatedbFlg = false;//DB更新中のフラグ
        private bool caughtFishesFlg = false;//釣った魚取得中フラグ
        private SettingsArgsModel args = new SettingsArgsModel();
        private Dictionary<string, SettingsPlayerFishListWantedModel> fishListKey = new Dictionary<string, SettingsPlayerFishListWantedModel>();

        #region Delegate
        delegate void UpdateStatusBarDelegate(FishingTool iFishing);
        delegate void InitFormDelegate();
        delegate void StartFishingDelegate();
        delegate void StopFishingDelegate(bool iShowStopMessage);
        delegate void StartHarakiriDelegate();
        delegate void StopHarakiriDelegate(bool iShowStopMessage);
        delegate void StartUpdateDBDelegate();
        delegate void StopUpdateDBDelegate(bool iShowStopMessage);
        delegate void StartCaughtFishesDelegate();
        delegate void StopCaughtFishesDelegate(bool iShowStopMessage);
        delegate void SetStatusStripBackColorDelegate();
        delegate void UpdateFishListDelegate();
        delegate void UpdateHistoryDelegate();
        delegate void UpdateFishingInfoDelegate(DataGridView iGrid, DateTime iYmd, FishResultStatusKind iResult, string iFishName);
        delegate void UpdateFishingInfoRealTimeDelegate();
        delegate void UpdateHarakiriHistoryDelegate();
        delegate void UpdateCaughtFishesDelegate();
        delegate void FishingTool_ChangeStatusDelegate(object sender, FishingTool.ChangeStatusEventArgs e);
        delegate void FishingTool_ChangeMessageDelegate(object sender, FishingTool.ChangeMessageEventArgs e);
        delegate void FishingTool_CaughtFishesUpdateDelegate(object sender, FishingTool.CaughtFishesUpdateEventArgs e);
        delegate void HarakiriTool_ChangeStatusDelegate(object sender, HarakiriTool.ChangeStatusEventArgs e);
        delegate void HarakiriTool_ChangeMessageDelegate(object sender, HarakiriTool.ChangeMessageEventArgs e);
        delegate void UpdateDBTool_ReceiveMessageDelegate(object sender, UpdateDBTool.ReceiveMessageEventArgs e);
        delegate void UpdateDBTool_NewerVersionDelegate(object sender, UpdateDBTool.NewerVersionEventArgs e);
        delegate void SaveSettingsDelegate();
        delegate void LockControlDelegate(bool iLock);
        delegate void ThreadHarakiriDelegate();
        #endregion

        #region Models
        /// <summary>
        /// グリッド設定用のモデル
        /// </summary>
        private class GridColModel
        {
            public Type Type { get; set; }
            public string DataPropertyName { get; set; }
            public string Header { get; set; }
            public int DisplayIndex { get; set; }
            public bool Visible { get; set; }
            public int Width { get; set; }
            public DataGridViewContentAlignment Alignment { get; set; }
            public string Format { get; set; }
            public DataGridViewTriState Resizable { get; set; }
            public GridColModel()
            {
                this.Type = typeof(DataGridViewTextBoxColumn);
                this.DataPropertyName = string.Empty;
                this.Header = string.Empty;
                this.DisplayIndex = 0;
                this.Visible = true;
                this.Width = 100;
                this.Alignment = DataGridViewContentAlignment.MiddleLeft;
                this.Format = string.Empty;
                this.Resizable = DataGridViewTriState.NotSet;
            }
            public GridColModel(Type iType, string iDataPropertyName, string iHeader, int iDisplayIndex, bool iVisible, int iWidth, DataGridViewContentAlignment iAlignment, string iFormat, DataGridViewTriState iResizable)
            {
                this.Type = iType;
                this.DataPropertyName = iDataPropertyName;
                this.Header = iHeader;
                this.DisplayIndex = iDisplayIndex;
                this.Visible = iVisible;
                this.Width = iWidth;
                this.Alignment = iAlignment;
                this.Format = iFormat;
                this.Resizable = iResizable;
            }
        }
        private class MoonPhaseDay
        {
            public MoonPhase MoonPhase { get; set; }
            public DateTime EarthDate { get; set; }
            public MoonPhaseDay(MoonPhase iMoonPhase, DateTime iEarthDate)
            {
                this.MoonPhase = iMoonPhase;
                this.EarthDate = iEarthDate;
            }
        }
        private class TimeTable
        {
            public byte Open { get; set; }
            public byte Close { get; set; }
            public TimeTable(byte iOpen, byte iClose)
            {
                this.Open = iOpen;
                this.Close = iClose;
            }
        }
        #endregion

        #region コンストラクタ
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainForm(PolTool iPol, SettingsArgsModel iArgs)
        {
            startupFlg = true;
            InitializeComponent();
            args = iArgs;
            constructor(iPol);
        }
        /// <summary>
        /// コンストラクタ処理部
        /// </summary>
        /// <param name="iPol"></param>
        private void constructor(PolTool iPol)
        {
            //PolTool初期設定
            pol = iPol;
            pol.ChangeStatus += new PolTool.ChangeStatusEventHandler(this.PolTool_ChangeStatus);
            //FFACE初期設定
            fface = iPol.FFACE;
            //LoggerTool初期設定
            logger = new LoggerTool(MiscTool.GetAppAssemblyName(), pol.FFACE.Player.Name);
            logger.Enable = args.LoggerEnable;
            logger.OutputLogLevel = args.LoggerLogLevel;
            logger.EnableVarDump = args.LoggerVarDumpEnable;
            logger.Output(LogLevelKind.INFO, string.Format("===== {0} {1} =====", MiscTool.GetAppAssemblyName(), MiscTool.GetAppVersion()));
            logger.Output(LogLevelKind.INFO, string.Format("デバッグログ:{0} ログレベル：{1} 変数出力：{2}", args.LoggerEnable, args.LoggerLogLevel, args.LoggerVarDumpEnable));
            logger.Output(LogLevelKind.INFO, string.Format("プロセス({0})にアタッチしました", pol.ProcessID));
            //Settings初期設定
            settings = new Settings(iPol.FFACE.Player.Name);
            //ChatTool初期設定
            chat = new ChatTool(iPol.FFACE);
            chat.ReceivedCommand += new ChatTool.ReceivedCommandEventHandler(this.ChatTool_ReceivedCommand);
            logger.Output(LogLevelKind.DEBUG, "ChatTool起動");
            //FishingTool初期設定
            fishing = new FishingTool(iPol, chat, settings, logger);
            fishing.Fished += new FishingTool.FishedEventHandler(this.FishingTool_Fished);
            fishing.ChangeMessage += new FishingTool.ChangeMessageEventHandler(this.FishingTool_ChangeMessage);
            fishing.ChangeStatus += new FishingTool.ChangeStatusEventHandler(this.FishingTool_ChangeStatus);
            fishing.CaughtFishesUpdate += new FishingTool.CaughtFishesUpdateEventHandler(this.FishingTool_CaughtFishesUpdate);
            logger.Output(LogLevelKind.DEBUG, "FishingTool起動");
            //HarakiriTool初期設定
            harakiri = new HarakiriTool(iPol, chat, settings, logger);
            harakiri.HarakiriOnce += new HarakiriTool.HarakiriOnceEventHandler(this.HarakiriTool_HarakiriOnce);
            harakiri.ChangeMessage += new HarakiriTool.ChangeMessageEventHandler(this.HarakiriTool_ChangeMessage);
            harakiri.ChangeStatus += new HarakiriTool.ChangeStatusEventHandler(this.HarakiriTool_ChangeStatus);
            logger.Output(LogLevelKind.DEBUG, "HarakiriTool起動");
            //FFACEControl初期設定
            control = new FFACEControl(pol, chat, logger);
            control.MaxLoopCount = Constants.MAX_LOOP_COUNT;
            control.UseEnternity = settings.UseEnternity;
            control.BaseWait = settings.Global.WaitBase;
            control.ChatWait = settings.Global.WaitChat;
            logger.Output(LogLevelKind.DEBUG, "FFACEControl起動");
            //監視スレッド起動
            thMonitor = new Thread(threadMonitor);
            thMonitor.Start();
            logger.Output(LogLevelKind.DEBUG, "監視スレッド起動");
            //DB
            fishDB = new FishDB(logger);
            fishHistoryDB = new FishHistoryDB(fishing.PlayerName, fishing.EarthDateTime, logger);
            harakiriDB = new HarakiriDB(logger);
            //古いデータをコンバート
            converter();
            //DB更新
            updatedb = new UpdateDBTool(settings, logger);
            updatedb.ReceiveMessage += new UpdateDBTool.ReceiveMessageEventHandler(this.UpdateDBTool_ReceiveMessage);
            updatedb.NewerVersion += new UpdateDBTool.NewerVersionEventHandler(this.UpdateDBTool_NewerVersion);
        }
        #endregion

        #region フォーム初期化
        /// <summary>
        /// フォームのLoadイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            //アドオン・プラグインの更新
            updateAddonPlugin();
            //釣り情報グリッド初期化
            initGridFishingInfo(gridFishingInfo);
            //履歴グリッド初期化
            initGridHistory();
            //履歴合計グリッド初期化
            initGridFishingInfo(gridHistorySummary);
            //ハラキリグリッド初期化
            initGridHarakiri(gridHarakiri);
            //釣った魚グリッド初期化
            initGridCaughtFishes(gridCaughtFishes);
            //装備初期化
            initGear();
            //フォーム初期化
            initForm();

            startupFlg = false;
        }
        /// <summary>
        /// フォーム Shownイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Shown(object sender, EventArgs e)
        {
            //ネットワーク接続の確認
            if (settings.Global.UpdateDB.LastUpdate == string.Empty)
            {
                SystemSounds.Asterisk.Play();
                string message = "ネットワーク接続を有効にしますか？\nネットワーク接続を有効にすると、釣りの履歴データがサーバーへアップロードされ、\n最新の魚情報を取得する事ができます。\n\nキャラクターを特定するような情報は全て削除されてアップロードされます。";
                DialogResult ret = MessageBox.Show(message, MiscTool.GetAppTitle() + " ネットワーク接続の確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                settings.Global.UpdateDB.Enable = (ret == DialogResult.Yes);
                settings.Global.UpdateDB.LastUpdate = "2000/01/01 00:00:00";
                chkUpdateDBEnable.Checked = settings.Global.UpdateDB.Enable;
            }
            //起動時に更新
            if (settings.Global.UpdateDB.Enable && settings.Global.UpdateDB.AutoUpdate)
            {
                btnExecUpdateDB_Click(this,new EventArgs());
            }
        }
        /// <summary>
        /// フォーム初期化
        /// </summary>
        private void initForm()
        {
            if (InvokeRequired)
            {
                Invoke(new InitFormDelegate(initForm), null);
            }
            else
            {
                //画面初期化
                //フォーム
                this.Text = string.Format(Constants.WINDOW_TITLE_FORMAT, MiscTool.GetAppTitle(), MiscTool.GetAppVersion(), fishing.PlayerName);
                if (settings.Form.PosX > 0) this.Left = settings.Form.PosX;
                if (settings.Form.PosY > 0) this.Top = settings.Form.PosY;
                if (settings.Form.Width > 0) this.Width = settings.Form.Width;
                if (settings.Form.Height > 0) this.Height = settings.Form.Height;
                if (settings.Form.SplitterDistance > 0) splitMain.SplitterDistance = settings.Form.SplitterDistance;
                this.TopMost = settings.Etc.WindowTopMost;
                //魚リスト
                if (settings.FishList.Mode == Settings.FishListModeKind.ID) rdoFishListModeID.Checked = true;
                else if (settings.FishList.Mode == Settings.FishListModeKind.Name) rdoFishListModeName.Checked = true;
                chkFIshListNarrowArea.Checked = settings.FishList.NarrowArea;
                chkFIshListNarrowBait.Checked = settings.FishList.NarrowBait;
                updateFishList();
                //釣り設定・動作
                chkLearning.Checked = settings.Fishing.Learning;
                chkSneakFishing.Checked = settings.Fishing.SneakFishing;
                txtSneakFishingRemain.Value = (decimal)settings.Fishing.SneakFishingRemain;
                chkHP0.Checked = settings.Fishing.HP0;
                txtHP0Min.Value = (decimal)settings.Fishing.HP0Min;
                txtHP0Max.Value = (decimal)settings.Fishing.HP0Max;
                chkIgnoreSmallFish.Checked = settings.Fishing.IgnoreSmallFish;
                chkIgnoreLargeFish.Checked = settings.Fishing.IgnoreLargeFish;
                chkIgnoreMonster.Checked = settings.Fishing.IgnoreMonster;
                chkIgnoreItem.Checked = settings.Fishing.IgnoreItem;
                chkReactionTime.Checked = settings.Fishing.ReactionTime;
                txtReactionTimeMin.Value = (decimal)settings.Fishing.ReactionTimeMin;
                txtReactionTimeMax.Value = (decimal)settings.Fishing.ReactionTimeMax;
                chkRecastTime.Checked = settings.Fishing.RecastTime;
                txtRecastTimeMin.Value = (decimal)settings.Fishing.RecastTimeMin;
                txtRecastTimeMax.Value = (decimal)settings.Fishing.RecastTimeMax;
                chkVanaTime.Checked = settings.Fishing.VanaTime;
                txtVanaTimeFrom.Value = settings.Fishing.VanaTimeFrom;
                txtVanaTimeTo.Value = settings.Fishing.VanaTimeTo;
                chkEarthTime.Checked = settings.Fishing.EarthTime;
                txtEarthTimeFrom.Value = settings.Fishing.EarthTimeFrom;
                txtEarthTimeTo.Value = settings.Fishing.EarthTimeTo;
                chkRepairRod.Checked = settings.Fishing.RepairRod;
                chkWaitTimeout.Checked = settings.Fishing.WaitTimeout;
                //釣り設定・停止条件
                chkMaxCatch.Checked = settings.Fishing.MaxCatch;
                txtMaxCatchCount.Value = settings.Fishing.MaxCatchCount;
                chkMaxNoCatch.Checked = settings.Fishing.MaxNoCatch;
                txtMaxNoCatchCount.Value = settings.Fishing.MaxNoCatchCount;
                chkMaxSkill.Checked = settings.Fishing.MaxSkill;
                txtMaxSkillValue.Value = settings.Fishing.MaxSkillValue;
                chkChatTell.Checked = settings.Fishing.ChatTell;
                chkChatSay.Checked = settings.Fishing.ChatSay;
                chkChatParty.Checked = settings.Fishing.ChatParty;
                chkChatLs.Checked = settings.Fishing.ChatLs;
                chkChatShout.Checked = settings.Fishing.ChatShout;
                chkChatEmote.Checked = settings.Fishing.ChatEmote;
                chkChatRestart.Checked = settings.Fishing.ChatRestart;
                txtChatRestartMinute.Value = settings.Fishing.ChatRestartMinute;
                chkEntryPort.Checked = settings.Fishing.EntryPort;
                chkEnemyAttackCmd.Checked = settings.Fishing.EnemyAttackCmd;
                txtEnemyAttackCmdLine.Text = settings.Fishing.EnemyAttackCmdLine;
                chkEminenceClear.Checked = settings.Fishing.EminenceClear;
                //釣り設定・鞄いっぱい
                chkInventoryFullSack.Checked = settings.Fishing.InventoryFullSack;
                chkInventoryFullSatchel.Checked = settings.Fishing.InventoryFullSatchel;
                chkInventoryFullCase.Checked = settings.Fishing.InventoryFullCase;
                chkInventoryFullCmd.Checked = settings.Fishing.InventoryFullCmd;
                txtInventoryFullCmdLine.Text = settings.Fishing.InventoryFullCmdLine;
                //釣り設定・竿エサ無し
                chkNoBaitNoRodSack.Checked = settings.Fishing.NoBaitNoRodSack;
                chkNoBaitNoRodSatchel.Checked = settings.Fishing.NoBaitNoRodSatchel;
                chkNoBaitNoRodCase.Checked = settings.Fishing.NoBaitNoRodCase;
                chkNoBaitNoRodCmd.Checked = settings.Fishing.NoBaitNoRodCmd;
                txtNoBaitNoRodCmdLine.Text = settings.Fishing.NoBaitNoRodCmdLine;
                //釣り・装備
                chkEquipEnable.Checked = settings.Fishing.EquipEnable;
                cmbEquipRod.Text = settings.Fishing.EquipRod;
                cmbEquipBait.Text = settings.Fishing.EquipBait;
                cmbEquipMain.Text = settings.Fishing.EquipMain;
                cmbEquipSub.Text = settings.Fishing.EquipSub;
                cmbEquipHead.Text = settings.Fishing.EquipHead;
                cmbEquipBody.Text = settings.Fishing.EquipBody;
                cmbEquipHands.Text = settings.Fishing.EquipHands;
                cmbEquipLegs.Text = settings.Fishing.EquipLegs;
                cmbEquipFeet.Text = settings.Fishing.EquipFeet;
                cmbEquipNeck.Text = settings.Fishing.EquipNeck;
                cmbEquipWaist.Text = settings.Fishing.EquipWaist;
                cmbEquipBack.Text = settings.Fishing.EquipBack;
                cmbEquipEarLeft.Text = settings.Fishing.EquipEarLeft;
                cmbEquipEarRight.Text = settings.Fishing.EquipEarRight;
                cmbEquipRingLeft.Text = settings.Fishing.EquipRingLeft;
                cmbEquipRingRight.Text = settings.Fishing.EquipRingRight;
                chkUseWaist.Checked = settings.Fishing.UseWaist;
                chkUseRingLeft.Checked = settings.Fishing.UseRingLeft;
                chkUseRingRight.Checked = settings.Fishing.UseRingRight;
                chkEquipEnable_CheckedChanged(this, new EventArgs());
                cmbFood.Text = settings.Fishing.Food;
                chkUseFood.Checked = settings.Fishing.UseFood;
                //釣り・情報
                updateFishingInfo(gridFishingInfo, DateTime.Now, FishResultStatusKind.Unknown, string.Empty);
                //履歴
                dateHistory.Value = DateTime.Now;
                updateHistory();
                updateFishingInfo(gridHistorySummary, dateHistory.Value, (FishResultStatusKind)cmbHistoryResult.SelectedValue, (string)cmbHistoryFishName.SelectedValue);
                //ハラキリ
                updateHarakiriFishList();
                cmbHarakiriFishname.SelectedIndex = cmbHarakiriFishname.Items.IndexOf(settings.Harakiri.FishNameSelect);
                txtHarakiriFishname.Text = settings.Harakiri.FishNameInput;
                if (settings.Harakiri.InputType == Settings.HarakiriInputTypeKind.Select)
                {
                    rdoHarakiriInputTypeSelect.Checked = true;
                    EventArgs e = new EventArgs();
                    rdoHarakiriInputTypeSelect_CheckedChanged(rdoHarakiriInputTypeSelect, e);
                }
                else if (settings.Harakiri.InputType == Settings.HarakiriInputTypeKind.Input)
                {
                    rdoHarakiriInputTypeInput.Checked = true;
                    EventArgs e = new EventArgs();
                    rdoHarakiriInputTypeInput_CheckedChanged(rdoHarakiriInputTypeInput, e);
                }
                chkHarakiriStopFound.Checked = settings.Harakiri.StopFound;
                updateHarakiriHistory();
                //釣った魚
                chkViewNotCaughtOnly.Checked = settings.CaughtFishes.ViewNotCaughtOnly;
                updateCaughtFishes();
                //DB更新
                lblLastUpdate.Text = settings.Global.UpdateDB.LastUpdate;
                chkUpdateDBEnable.Checked = settings.Global.UpdateDB.Enable;
                chkUpdateDBAutoUpdate.Checked = settings.Global.UpdateDB.AutoUpdate;
                txtUpdateDBLog.Text = string.Empty;
                //設定・一般
                chkWindowTopMost.Checked = settings.Etc.WindowTopMost;
                chkWindowFlash.Checked = settings.Etc.WindowFlash;
                chkWindowActivate.Checked = settings.Etc.WindowActivate;
                chkMessageEcho.Checked = settings.Etc.MessageEcho;
                //設定・ステータスバー表示
                chkStatusBarVisibleMoonPhase.Checked = settings.Etc.VisibleMoonPhase;
                chkStatusBarVisibleVanaTime.Checked = settings.Etc.VisibleVanaTime;
                chkStatusBarVisibleEarthTime.Checked = settings.Etc.VisibleEarthTime;
                chkStatusBarVisibleDayType.Checked = settings.Etc.VisibleDayType;
                chkStatusBarVisibleLoginStatus.Checked = settings.Etc.VisibleLoginStatus;
                chkStatusBarVisiblePlayerStatus.Checked = settings.Etc.VisiblePlayerStatus;
                chkStatusBarVisibleHpBar.Checked = settings.Etc.VisibleHpBar;
                chkStatusBarVisibleHP.Checked = settings.Etc.VisibleHP;
                chkStatusBarVisibleRemainTimeBar.Checked = settings.Etc.VisibleRemainTimeBar;
                chkStatusBarVisibleRemainTime.Checked = settings.Etc.VisibleRemainTime;
                //設定・履歴列表示
                chkHistoryVisibleEarthTime.Checked = settings.History.ColEarthTime.Visible;
                chkHistoryVisibleVanaTime.Checked = settings.History.ColVanaTime.Visible;
                chkHistoryVisibleVanaWeekDay.Checked = settings.History.ColVanaWeekDay.Visible;
                chkHistoryVisibleMoonPhase.Checked = settings.History.ColMoonPhase.Visible;
                chkHistoryVisibleZoneName.Checked = settings.History.ColZoneName.Visible;
                chkHistoryVisibleRodName.Checked = settings.History.ColRodName.Visible;
                chkHistoryVisibleBaitName.Checked = settings.History.ColBaitName.Visible;
                chkHistoryVisibleID.Checked = settings.History.ColID.Visible;
                chkHistoryVisibleFishName.Checked = settings.History.ColFishName.Visible;
                chkHistoryVisibleFishCount.Checked = settings.History.ColFishCount.Visible;
                chkHistoryVisibleResult.Checked = settings.History.ColResult.Visible;
                //設定・設定の保存
                if (settings.Global.SaveMode == Settings.SaveModeKind.Shared)
                {
                    rdoSaveModeShared.Checked = true;
                }
                else if (settings.Global.SaveMode == Settings.SaveModeKind.ByPlayer)
                {
                    rdoSaveModeByPlayer.Checked = true;
                }
                //ステータスバー
                statusStrip.BackColor = SystemColors.Control;
                lblMoonPhase.Text = string.Empty;
                lblVanaTime.Text = string.Empty;
                lblEarthTime.Text = string.Empty;
                lblDayType.Text = string.Empty;
                lblLoginStatus.Text = string.Empty;
                lblPlayerStatus.Text = string.Empty;
                lblMessage.Text = string.Empty;
                barHP.Value = 0;
                lblHP.Text = "0%";
                barRemainTime.Value = 0;
                lblRemainTime.Text = "0s";
                setStatusBarVisible();
            }
        }
        /// <summary>
        /// 履歴グリッドを初期化する
        /// </summary>
        private void initGridHistory()
        {
            //表示専用にする
            gridHistory.ReadOnly = true;
            //ユーザーが新しい行を追加できないようにする
            gridHistory.AllowUserToAddRows = false;
            //行をユーザーが削除できないようにする
            gridHistory.AllowUserToDeleteRows = false;
            //行の高さをユーザーが変更できないようにする
            gridHistory.AllowUserToResizeRows = false;
            //セルを選択すると行全体が選択されるようにする
            gridHistory.MultiSelect = false;
            gridHistory.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            //列が自動的に作成されないようにする
            gridHistory.AutoGenerateColumns = false;
            //行ヘッダを非表示にする
            gridHistory.RowHeadersVisible = false;
            //列の位置をユーザーが変更できるようにする
            gridHistory.AllowUserToOrderColumns = true;
            //列を設定する
            gridHistory.Columns.Clear();
            foreach (GridColModel col in lstGridHistory)
            {
                if (col.Type == typeof(DataGridViewTextBoxColumn))
                {
                    addGridTextColumns(gridHistory, col);
                }
                else if (col.Type == typeof(DataGridViewImageColumn))
                {
                    addGridImageColumns(gridHistory, col);
                }
            }
            //設定ファイルから列の設定を行う
            setGridColFromSettings(gridHistory, settings.History.ColEarthTime);
            setGridColFromSettings(gridHistory, settings.History.ColVanaTime);
            setGridColFromSettings(gridHistory, settings.History.ColVanaWeekDay);
            setGridColFromSettings(gridHistory, settings.History.ColMoonPhase);
            setGridColFromSettings(gridHistory, settings.History.ColZoneName);
            setGridColFromSettings(gridHistory, settings.History.ColRodName);
            setGridColFromSettings(gridHistory, settings.History.ColBaitName);
            setGridColFromSettings(gridHistory, settings.History.ColID);
            setGridColFromSettings(gridHistory, settings.History.ColFishName);
            setGridColFromSettings(gridHistory, settings.History.ColFishCount);
            setGridColFromSettings(gridHistory, settings.History.ColResult);
        }
        /// <summary>
        /// グリッドの列設定を設定ファイルから設定する
        /// </summary>
        /// <param name="iDataGridView">グリッド</param>
        /// <param name="iColSetting">設定</param>
        private void setGridColFromSettings(DataGridView iDataGridView, SettingsPlayerHistoryColModel iColSetting)
        {
            iDataGridView.Columns[iColSetting.Name].Width = iColSetting.Width;
            iDataGridView.Columns[iColSetting.Name].DisplayIndex = iColSetting.DisplayIndex;
            iDataGridView.Columns[iColSetting.Name].Visible = iColSetting.Visible;
        }
        /// <summary>
        /// 釣り情報グリッドを初期化する
        /// </summary>
        private void initGridFishingInfo(DataGridView iGrid)
        {
            //表示専用にする
            iGrid.ReadOnly = true;
            //ユーザーが新しい行を追加できないようにする
            iGrid.AllowUserToAddRows = false;
            //行をユーザーが削除できないようにする
            iGrid.AllowUserToDeleteRows = false;
            //行の高さをユーザーが変更できないようにする
            iGrid.AllowUserToResizeRows = false;
            //セルを選択すると行全体が選択されるようにする
            iGrid.MultiSelect = false;
            iGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            //列が自動的に作成されないようにする
            iGrid.AutoGenerateColumns = false;
            //行ヘッダを非表示にする
            iGrid.RowHeadersVisible = false;
            //列の位置をユーザーが変更できるようにする
            iGrid.AllowUserToOrderColumns = true;
            //列を設定する
            iGrid.Columns.Clear();
            foreach (GridColModel col in lstGridFishingInfo)
            {
                if (col.Type == typeof(DataGridViewTextBoxColumn))
                {
                    addGridTextColumns(iGrid, col);
                }
                else if (col.Type == typeof(DataGridViewImageColumn))
                {
                    addGridImageColumns(iGrid, col);
                }
            }
            //ソート無効
            for (int i = 0; i < iGrid.Columns.Count; i++)
            {
                iGrid.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }
        /// <summary>
        /// ハラキリグリッドを初期化する
        /// </summary>
        private void initGridHarakiri(DataGridView iGrid)
        {
            //表示専用にする
            iGrid.ReadOnly = true;
            //ユーザーが新しい行を追加できないようにする
            iGrid.AllowUserToAddRows = false;
            //行をユーザーが削除できないようにする
            iGrid.AllowUserToDeleteRows = false;
            //行の高さをユーザーが変更できないようにする
            iGrid.AllowUserToResizeRows = false;
            //セルを選択すると行全体が選択されるようにする
            iGrid.MultiSelect = false;
            iGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            //列が自動的に作成されないようにする
            iGrid.AutoGenerateColumns = false;
            //行ヘッダを非表示にする
            iGrid.RowHeadersVisible = false;
            //列の位置をユーザーが変更できるようにする
            iGrid.AllowUserToOrderColumns = true;
            //列を設定する
            iGrid.Columns.Clear();
            foreach (GridColModel col in lstGridHarakiri)
            {
                if (col.Type == typeof(DataGridViewTextBoxColumn))
                {
                    addGridTextColumns(iGrid, col);
                }
                else if (col.Type == typeof(DataGridViewImageColumn))
                {
                    addGridImageColumns(iGrid, col);
                }
            }
            //ソート無効
            for (int i = 0; i < iGrid.Columns.Count; i++)
            {
                iGrid.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }
        /// <summary>
        /// 釣った魚グリッドを初期化する
        /// </summary>
        /// <param name="iGrid"></param>
        private void initGridCaughtFishes(DataGridView iGrid)
        {
            //表示専用にする
            iGrid.ReadOnly = true;
            //ユーザーが新しい行を追加できないようにする
            iGrid.AllowUserToAddRows = false;
            //行をユーザーが削除できないようにする
            iGrid.AllowUserToDeleteRows = false;
            //行の高さをユーザーが変更できないようにする
            iGrid.AllowUserToResizeRows = false;
            //セルを選択すると行全体が選択されるようにする
            iGrid.MultiSelect = false;
            iGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            //列が自動的に作成されないようにする
            iGrid.AutoGenerateColumns = false;
            //行ヘッダを非表示にする
            iGrid.RowHeadersVisible = false;
            //列の位置をユーザーが変更できるようにする
            iGrid.AllowUserToOrderColumns = true;
            //列を設定する
            iGrid.Columns.Clear();
            foreach (GridColModel col in lstGridCaughtFishes)
            {
                if (col.Type == typeof(DataGridViewTextBoxColumn))
                {
                    addGridTextColumns(iGrid, col);
                }
                else if (col.Type == typeof(DataGridViewImageColumn))
                {
                    addGridImageColumns(iGrid, col);
                }
            }
        }
        /// <summary>
        /// 装備コンボボックスを初期化する
        /// </summary>
        private void initGear()
        {
            cmbEquipRod.Items.Add(string.Empty);
            cmbEquipBait.Items.Add(string.Empty);
            cmbEquipMain.Items.Add(string.Empty);
            cmbEquipSub.Items.Add(string.Empty);
            cmbEquipHead.Items.Add(string.Empty);
            cmbEquipBody.Items.Add(string.Empty);
            cmbEquipHands.Items.Add(string.Empty);
            cmbEquipLegs.Items.Add(string.Empty);
            cmbEquipFeet.Items.Add(string.Empty);
            cmbEquipNeck.Items.Add(string.Empty);
            cmbEquipWaist.Items.Add(string.Empty);
            cmbEquipBack.Items.Add(string.Empty);
            cmbEquipEarLeft.Items.Add(string.Empty);
            cmbEquipEarRight.Items.Add(string.Empty);
            cmbEquipRingLeft.Items.Add(string.Empty);
            cmbEquipRingRight.Items.Add(string.Empty);
            cmbFood.Items.Add(string.Empty);

            foreach (string rod in fishDB.Rods)
            {
                cmbEquipRod.Items.Add(rod);
            }
            foreach (string bait in fishDB.Baits)
            {
                cmbEquipBait.Items.Add(bait);
            }
            foreach (GearDBGearModel gear in fishDB.SelectGear())
            {
                switch (gear.Position)
                {
                    case GearDBPositionKind.Main:
                        cmbEquipMain.Items.Add(gear.GearName);
                        break;
                    case GearDBPositionKind.Sub:
                        cmbEquipSub.Items.Add(gear.GearName);
                        break;
                    case GearDBPositionKind.Head:
                        cmbEquipHead.Items.Add(gear.GearName);
                        break;
                    case GearDBPositionKind.Body:
                        cmbEquipBody.Items.Add(gear.GearName);
                        break;
                    case GearDBPositionKind.Hands:
                        cmbEquipHands.Items.Add(gear.GearName);
                        break;
                    case GearDBPositionKind.Legs:
                        cmbEquipLegs.Items.Add(gear.GearName);
                        break;
                    case GearDBPositionKind.Feet:
                        cmbEquipFeet.Items.Add(gear.GearName);
                        break;
                    case GearDBPositionKind.Neck:
                        cmbEquipNeck.Items.Add(gear.GearName);
                        break;
                    case GearDBPositionKind.Waist:
                        cmbEquipWaist.Items.Add(gear.GearName);
                        break;
                    case GearDBPositionKind.Back:
                        cmbEquipBack.Items.Add(gear.GearName);
                        break;
                    case GearDBPositionKind.Earrings:
                        cmbEquipEarLeft.Items.Add(gear.GearName);
                        cmbEquipEarRight.Items.Add(gear.GearName);
                        break;
                    case GearDBPositionKind.Rings:
                        cmbEquipRingLeft.Items.Add(gear.GearName);
                        cmbEquipRingRight.Items.Add(gear.GearName);
                        break;
                    case GearDBPositionKind.Foods:
                        cmbFood.Items.Add(gear.GearName);
                        break;
                }
            }
        }
        /// <summary>
        /// グリッドにテキストボックス列を追加する
        /// </summary>
        /// <param name="iDataGridView">グリッド</param>
        /// <param name="iCol">列情報</param>
        private void addGridTextColumns(DataGridView iDataGridView, GridColModel iCol)
        {
            DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
            //名前とヘッダーを設定する
            col.Name = iCol.DataPropertyName; ;
            col.HeaderText = iCol.Header;
            //表示設定
            col.Visible = iCol.Visible;
            col.Width = iCol.Width;
            col.DisplayIndex = iCol.DisplayIndex;
            col.DefaultCellStyle.Format = iCol.Format;
            col.Resizable = iCol.Resizable;
            col.DefaultCellStyle.Alignment = iCol.Alignment;
            col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            col.DefaultCellStyle.ForeColor = Color.Black;
            col.DefaultCellStyle.BackColor = Color.WhiteSmoke;
            col.DefaultCellStyle.SelectionForeColor = Color.Black;
            col.DefaultCellStyle.SelectionBackColor = Color.WhiteSmoke;
            //データソースの列名の設定
            col.DataPropertyName = iCol.DataPropertyName;
            //列を追加する
            iDataGridView.Columns.Add(col);
        }
        /// <summary>
        /// グリッドにイメージ列を追加する
        /// </summary>
        /// <param name="iDataGridView">列情報</param>
        /// <param name="iCol">列情報</param>
        private void addGridImageColumns(DataGridView iDataGridView, GridColModel iCol)
        {
            DataGridViewImageColumn col = new DataGridViewImageColumn();
            //名前とヘッダーを設定する
            col.Name = iCol.DataPropertyName; ;
            col.HeaderText = iCol.Header;
            //表示設定
            col.Visible = iCol.Visible;
            col.Width = iCol.Width;
            col.DisplayIndex = iCol.DisplayIndex;
            col.DefaultCellStyle.Format = iCol.Format;
            col.Resizable = iCol.Resizable;
            col.DefaultCellStyle.Alignment = iCol.Alignment;
            col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            col.DefaultCellStyle.ForeColor = Color.Black;
            col.DefaultCellStyle.BackColor = Color.WhiteSmoke;
            col.DefaultCellStyle.SelectionForeColor = Color.Black;
            col.DefaultCellStyle.SelectionBackColor = Color.WhiteSmoke;
            //データソースの列名の設定
            col.DataPropertyName = iCol.DataPropertyName;
            //列を追加する
            iDataGridView.Columns.Add(col);
        }
        /// <summary>
        /// グリッドのソート設定
        /// </summary>
        /// <param name="iGrid">グリッド</param>
        /// <param name="iColName">列名</param>
        /// <param name="iSortOrder">ソート順</param>
        private void setGridSort(DataGridView iGrid, string iColName, SortOrder iSortOrder)
        {
            if (iColName != string.Empty)
            {
                ListSortDirection sortDirection = ListSortDirection.Ascending;
                if (iSortOrder == SortOrder.Ascending)
                {
                    sortDirection = ListSortDirection.Ascending;
                }
                else if (iSortOrder == SortOrder.Descending)
                {
                    sortDirection = ListSortDirection.Descending;
                }
                iGrid.Sort(iGrid.Columns[iColName], sortDirection);
            }
        }
        /// <summary>
        /// ステータスバーの表示・非表示切り替え
        /// </summary>
        private void setStatusBarVisible()
        {
            lblMoonPhase.Visible = settings.Etc.VisibleMoonPhase;
            lblVanaTime.Visible = settings.Etc.VisibleVanaTime;
            lblEarthTime.Visible = settings.Etc.VisibleEarthTime;
            lblDayType.Visible = settings.Etc.VisibleDayType;
            lblLoginStatus.Visible = settings.Etc.VisibleLoginStatus;
            lblPlayerStatus.Visible = settings.Etc.VisiblePlayerStatus;
            barHP.Visible = settings.Etc.VisibleHpBar;
            lblHP.Visible = settings.Etc.VisibleHP;
            barRemainTime.Visible = settings.Etc.VisibleRemainTimeBar;
            lblRemainTime.Visible = settings.Etc.VisibleRemainTime;
        }
        #endregion

        #region フォームイベント関連
        /// <summary>
        /// フォームのFormClosingイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            unload();
            
            //PolTool停止
            pol.SystemAbort(); 
            
            System.Environment.Exit(0);//プログラム終了
        }
        /// <summary>
        /// アンロード処理
        /// </summary>
        private void unload()
        {
            //設定保存
            saveSettings();
            logger.Output(LogLevelKind.DEBUG, "設定保存終了");
            //メインスレッド停止
            if (thFishing != null && thFishing.IsAlive) thFishing.Abort();
            thFishing = null;
            logger.Output(LogLevelKind.DEBUG, "メインスレッド停止");
            //DB更新スレッド停止
            if (thUpdateDB != null && thUpdateDB.IsAlive) thUpdateDB.Abort();
            //監視スレッド停止
            if (thMonitor != null && thMonitor.IsAlive) thMonitor.Abort();
            thMonitor = null;
            logger.Output(LogLevelKind.DEBUG, "監視スレッド停止");
            //FFACEControl停止
            control = null;
            //HarakiriTool停止
            if (harakiri != null) harakiri.SystemAbort();
            harakiri = null;
            logger.Output(LogLevelKind.DEBUG, "HarakiriTool停止");
            //FishingTool停止
            if (fishing != null) fishing.SystemAbort();
            fishing = null;
            logger.Output(LogLevelKind.DEBUG, "FishingTool停止");
            //ChatTool停止
            if (chat != null) chat.SystemAbort();
            chat = null;
            logger.Output(LogLevelKind.DEBUG, "ChatTool停止");


            logger.Output(string.Format("===== {0} {1} 終了=====", MiscTool.GetAppAssemblyName(), MiscTool.GetAppVersion()));
        }
        /// <summary>
        /// 開始ボタン クリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExecFishing_Click(object sender, EventArgs e)
        {
            if (!fishingFlg)
            {
                //釣り開始
                startFishing();
            }
            else
            {
                //釣り停止
                stopFishing(true);
            }
        }
        /// <summary>
        /// 釣り開始
        /// </summary>
        private void startFishing()
        {
            if (InvokeRequired)
            {
                Invoke(new StartFishingDelegate(startFishing), null);
            }
            else
            {
                logger.Output(LogLevelKind.INFO, "釣り開始");
                fishingFlg = true;
                btnExecFishing.Text = "停　止";

                //魚リスト更新
                //updateFishList();
                //Wanted設定
                //setWantedToSettings();

                thFishing = new Thread(threadFishing);
                thFishing.Start();

                //ハラキリボタン無効化
                btnExecHarakiri.Enabled = false;
                //釣った魚初期化ボタン無効化
                btnUpdateCaughtFishes.Enabled = false;
            }
        }
        /// <summary>
        /// 釣り停止
        /// </summary>
        private void stopFishing(bool iShowStopMessage)
        {
            if (InvokeRequired)
            {
                Invoke(new StopFishingDelegate(stopFishing), iShowStopMessage);
            }
            else
            {
                logger.Output(LogLevelKind.INFO, "釣り停止");
                fishingFlg = false;
                btnExecFishing.Text = "開　始";
                bool ret = fishing.FishingAbort();
                if(iShowStopMessage) setMessage("停止しました");

                //ハラキリボタン有効化
                btnExecHarakiri.Enabled = true;
                //釣った魚初期化ボタン有効化
                btnUpdateCaughtFishes.Enabled = true;
            }
        }
        /// <summary>
        /// 釣りメインスレッド
        /// </summary>
        private void threadFishing()
        {
            FishingTool.FishingStatusKind ret = fishing.FishingStart();
            stopFishing(false);
            if (ret != FishingTool.FishingStatusKind.Error)
            {
                //正常終了
                SystemSounds.Asterisk.Play();
            }
            else
            {
                //エラー
                SystemSounds.Hand.Play();
            }
        }
        /// <summary>
        /// 魚リスト更新 Clickイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpdateFishList_Click(object sender, EventArgs e)
        {
            //ログイン中ではない場合、再稼動させる
            if (!loginFlg && fface.Player.GetLoginStatus == LoginStatus.LoggedIn)
            {
                logger.Output(string.Format("再起動開始"));
                startupFlg = true;
                constructor(pol);
                initForm();
                startupFlg = false;
                loginFlg = true;
                lockControl(false);//コントロールアンロック
            }
            //Wantedリストの更新
            if (loginFlg)
            {
                updateFishList();
            }
        }
        /// <summary>
        /// 魚リスト SelectedIndexChangedイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lstFish_SelectedIndexChanged(object sender, EventArgs e)
        {
            //竿が装備されていない場合、魚リストボックスを更新する
            if (fishing.RodName == "")
            {
                updateFishList();
                return;
            }
            //SelectedIndexをチェック
            if (lstFish.SelectedIndex == -1) return;

            //選択された行がチェックされたか判定
            bool checkedFlg = false;
            string viewFishName = (string)lstFish.Items[lstFish.SelectedIndex];
            SettingsPlayerFishListWantedModel target = fishListKey[viewFishName];
            foreach (string fishName in lstFish.CheckedItems)
            {
                if (fishName == viewFishName) checkedFlg = true;
            }
            //Wanted更新
            if (checkedFlg)
            {
                if (settings.FishList.Mode == Settings.FishListModeKind.ID)
                    settings.FishList.AddWanted(target.FishName, target.ID1, target.ID2, target.ID3, target.ID4);
                else if (settings.FishList.Mode == Settings.FishListModeKind.Name)
                    settings.FishList.AddWanted(target.FishName);
            }
            else
            {
                if (settings.FishList.Mode == Settings.FishListModeKind.ID)
                    settings.FishList.DelWanted(target.FishName, target.ID1, target.ID2, target.ID3, target.ID4);
                else if (settings.FishList.Mode == Settings.FishListModeKind.Name)
                    settings.FishList.DelWanted(target.FishName);
            }
            //選択解除
            lstFish.SelectedIndex = -1;

            logger.Output(LogLevelKind.DEBUG, "セットされてる魚");
            foreach (SettingsPlayerFishListWantedModel wanted in settings.FishList.Wanted)
            {
                logger.Output(LogLevelKind.DEBUG, string.Format("Name={0} ID1={1} ID2={2} ID3={3} ID4={4}", wanted.FishName, wanted.ID1, wanted.ID2, wanted.ID3, wanted.ID4));
            }
        }
        /// <summary>
        /// 履歴更新ボタン Clickイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnHistoryUpdate_Click(object sender, EventArgs e)
        {
            updateHistory();
        }
        /// <summary>
        /// 履歴日付テキストボックス ValueChangedイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dateHistory_ValueChanged(object sender, EventArgs e)
        {
            updateHistory();
        }
        /// <summary>
        /// 履歴結果コンボボックス SelectedIndexChangedイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbHistoryResult_SelectedIndexChanged(object sender, EventArgs e)
        {
            updateHistory();
        }
        /// <summary>
        /// 履歴魚コンボボックス SelectedIndexChangedイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbHistoryFishName_SelectedIndexChanged(object sender, EventArgs e)
        {
            updateHistory();
        }
        /// <summary>
        /// 釣り情報グリッド CellFormattingイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gridFishingInfo_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            if (dgv.Columns[e.ColumnIndex].Name == "Result" && e.Value is string)
            {
                string val = (string)e.Value;
                if (val == "全体")
                {
                    dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(0x70, 0xEF, 0x70);
                    dgv.Rows[e.RowIndex].DefaultCellStyle.SelectionBackColor = Color.FromArgb(0x70, 0xEF, 0x70);
                }
                else if (val != string.Empty)
                {
                    dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(0x80, 0xFF, 0x80);
                    dgv.Rows[e.RowIndex].DefaultCellStyle.SelectionBackColor = Color.FromArgb(0x80, 0xFF, 0x80);
                }
            }
        }
        /// <summary>
        /// 履歴グリッド Sortedイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gridHistory_Sorted(object sender, EventArgs e)
        {
            DataGridViewColumn sortCol = gridHistory.SortedColumn;
            if (sortCol != null)
            {
                settings.History.SortColName = sortCol.Name;
                settings.History.SortOrder = gridHistory.SortOrder;
            }
            else
            {
                settings.History.SortColName = string.Empty;
                settings.History.SortOrder = SortOrder.None;
            }
        }
        /// <summary>
        /// ハラキリグリッド CellFormattingイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gridHarakiri_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            if (dgv.Columns[e.ColumnIndex].Name == "FishName" && e.Value is string)
            {
                string val = (string)e.Value;
                if (val != string.Empty)
                {
                    dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(0x80, 0xFF, 0x80);
                    dgv.Rows[e.RowIndex].DefaultCellStyle.SelectionBackColor = Color.FromArgb(0x80, 0xFF, 0x80);
                }
            }
        }
        /// <summary>
        /// 釣った魚グリッド CellFormattingイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gridCaughtFishes_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            if (dgv.Columns[e.ColumnIndex].Name == "Caught" && e.Value is string)
            {
                string val = (string)e.Value;
                if (val != "★")
                {
                    dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(0xDD, 0xDD, 0xDD);
                    dgv.Rows[e.RowIndex].DefaultCellStyle.SelectionBackColor = Color.FromArgb(0xDD, 0xDD, 0xDD);
                }
            }
        }
        /// <summary>
        /// アドオン・プラグインの更新 Clickイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddonUpdate_Click(object sender, EventArgs e)
        {
            btnAddonUpdate.Enabled = false;
            updateAddonPlugin();
            btnAddonUpdate.Enabled = true;
        }
        /// <summary>
        /// ハラキリ実行 Clickイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExecHarakiri_Click(object sender, EventArgs e)
        {
            if (!harakiriFlg)
            {
                //ハラキリ開始
                startHarakiri();
            }
            else
            {
                //ハラキリ停止
                stopHarakiri(true);
            }
        }
        /// <summary>
        /// ハラキリ開始
        /// </summary>
        private void startHarakiri()
        {
            if (InvokeRequired)
            {
                Invoke(new StartHarakiriDelegate(startHarakiri), null);
            }
            else
            {
                logger.Output(LogLevelKind.INFO, "ハラキリ開始");
                harakiriFlg = true;
                btnExecHarakiri.Text = "停　止";

                if (rdoHarakiriInputTypeSelect.Checked)
                {
                    harakiri.HarakiriFishName = cmbHarakiriFishname.Text;
                }
                else if (rdoHarakiriInputTypeInput.Checked)
                {
                    harakiri.HarakiriFishName = txtHarakiriFishname.Text;
                }

                thHarakiri = new Thread(threadHarakiri);
                thHarakiri.Start();

                //釣りボタン無効化
                btnExecFishing.Enabled = false;
                //釣った魚初期化ボタン無効化
                btnUpdateCaughtFishes.Enabled = false;
            }
        }
        /// <summary>
        /// ハラキリ停止
        /// </summary>
        private void stopHarakiri(bool iShowStopMessage)
        {
            if (InvokeRequired)
            {
                Invoke(new StopHarakiriDelegate(stopHarakiri), iShowStopMessage);
            }
            else
            {
                logger.Output(LogLevelKind.INFO, "ハラキリ停止");
                harakiriFlg = false;
                btnExecHarakiri.Text = "開　始";
                bool ret = harakiri.HarakiriAbort();
                if(iShowStopMessage) setMessage("停止しました");
                
                //釣りボタン有効化
                btnExecFishing.Enabled = true;
                //釣った魚初期化ボタン有効化
                btnUpdateCaughtFishes.Enabled = true;
            }
        }
        /// <summary>
        /// ハラキリメインスレッド
        /// </summary>
        private void threadHarakiri()
        {

            HarakiriTool.HarakiriStatusKind ret = harakiri.HarakiriStart();
            stopHarakiri(false);
            if (ret != HarakiriTool.HarakiriStatusKind.Error)
            {
                //正常終了
                SystemSounds.Asterisk.Play();
            }
            else
            {
                //エラー
                SystemSounds.Hand.Play();
            }
        }
        /// <summary>
        /// ハラキリ魚更新ボタン Clickイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnHarakiriUpdate_Click(object sender, EventArgs e)
        {
            updateHarakiriFishList();
        }
        /// <summary>
        /// 釣った魚初期化ボタン Clickイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpdateCaughtFishes_Click(object sender, EventArgs e)
        {
            if (!caughtFishesFlg)
            {
                startCaughtFishes();
            }
            else
            {
                stopCaughtFishes(true);
            }
        }
        /// <summary>
        /// 釣った魚開始
        /// </summary>
        private void startCaughtFishes()
        {
            if (InvokeRequired)
            {
                Invoke(new StartCaughtFishesDelegate(startCaughtFishes), null);
            }
            else
            {
                logger.Output(LogLevelKind.INFO, "釣った魚の初期化開始");
                caughtFishesFlg = true;
                btnUpdateCaughtFishes.Text = "停　止";

                //statusStrip.BackColor = Color.FromArgb(0x80, 0xFF, 0xFF);
                //setMessage("釣った魚の初期化中");

                thCaughtFishes = new Thread(threadCaughtFishes);
                thCaughtFishes.Start();

                //釣りボタン無効化
                btnExecFishing.Enabled = false;
                //ハラキリボタン無効化
                btnExecHarakiri.Enabled = false;
            }
        }
        /// <summary>
        /// 釣った魚停止
        /// </summary>
        private void stopCaughtFishes(bool iShowStopMessage)
        {
            if (InvokeRequired)
            {
                Invoke(new StopCaughtFishesDelegate(stopCaughtFishes), iShowStopMessage);
            }
            else
            {
                logger.Output(LogLevelKind.INFO, "釣った魚の更新停止");
                caughtFishesFlg = false;
                btnUpdateCaughtFishes.Text = "初期化";


                if (thCaughtFishes != null && thCaughtFishes.IsAlive) thCaughtFishes.Abort();
                if (iShowStopMessage)
                {
                    statusStrip.BackColor = SystemColors.Control;
                    setMessage("停止しました");
                }

                //釣りボタン有効化
                btnExecFishing.Enabled = true;
                //ハラキリボタン有効化
                btnExecHarakiri.Enabled = true;
            }
        }
        /// <summary>
        /// 釣った魚メインスレッド
        /// </summary>
        private void threadCaughtFishes()
        {
            List<SettingsPlayerCaughtFishModel> caughtFishes = fishing.GetCaughtFishes();
            if (caughtFishes.Count > 0)
            {
                settings.CaughtFishes.Fishes = caughtFishes;
                updateCaughtFishes();
                saveSettings();
                setMessage("釣った魚の初期化を完了しました");
                SystemSounds.Asterisk.Play();
                stopCaughtFishes(false);
            }
            else
            {
                SystemSounds.Hand.Play();
                stopCaughtFishes(false);
            }
        }
        /// <summary>
        /// 釣った魚リセットボタン Clickイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnResetCaughtFishes_Click(object sender, EventArgs e)
        {
            DialogResult res = MessageBox.Show("釣った魚をリセットします。\rよろしいでしょうか？", "確認", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (res == DialogResult.OK)
            {
                settings.CoughtFishesReset();
                updateCaughtFishes();
            }
        }
        /// <summary>
        /// DB更新ボタンClickイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExecUpdateDB_Click(object sender, EventArgs e)
        {
            //ネットワーク有効チェック
            if (!settings.Global.UpdateDB.Enable) return;

            if (!updatedbFlg)
            {
                //DB更新開始
                startUpdateDB();
            }
            else
            {
                //DB更新停止
                stopUpdateDB(true);
            }
        }
        /// <summary>
        /// DB更新開始
        /// </summary>
        private void startUpdateDB()
        {
            //ネットワーク有効チェック
            if (!settings.Global.UpdateDB.Enable) return;

            if (InvokeRequired)
            {
                Invoke(new StartUpdateDBDelegate(startUpdateDB), null);
            }
            else
            {
                logger.Output(LogLevelKind.INFO, "DB更新開始");
                //setMessage("データベースを更新しています");
                updatedbFlg = true;
                btnExecUpdateDB.Text = "停　止";
                txtUpdateDBLog.Text = string.Empty;

                thUpdateDB = new Thread(threadUpdateDB);
                thUpdateDB.Start();
            }
        }
        /// <summary>
        /// DB更新停止
        /// </summary>
        private void stopUpdateDB(bool iShowStopMessage)
        {
            if (InvokeRequired)
            {
                Invoke(new StopUpdateDBDelegate(stopUpdateDB), iShowStopMessage);
            }
            else
            {
                logger.Output(LogLevelKind.INFO, "DB更新停止");
                updatedbFlg = false;
                btnExecUpdateDB.Text = "更　新";

                if (thUpdateDB != null && thUpdateDB.IsAlive) thUpdateDB.Abort();
                if (iShowStopMessage)
                {
                    UpdateDBTool.ReceiveMessageEventArgs args = new UpdateDBTool.ReceiveMessageEventArgs();
                    args.Message = "データベースの更新をキャンセルしました";
                    uint color = 0xFFFFFFFF;
                    args.Color = Color.FromArgb((int)color);
                    args.Bold = true;
                    UpdateDBTool_ReceiveMessage(this, args);
                }
                //最終更新日の画面表示更新
                lblLastUpdate.Text = settings.Global.UpdateDB.LastUpdate;
                //設定保存
                saveSettings();
            }
        }
        /// <summary>
        /// DB更新メインスレッド
        /// </summary>
        private void threadUpdateDB()
        {

            bool ret = updatedb.UpdateDB();
            if (!ret)
            {
                SystemSounds.Hand.Play();
            }
            stopUpdateDB(false);
        }
        /// <summary>
        /// 統計情報 Clickイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lblUpdateDBUrl_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://" + settings.Global.UpdateDB.ServerName);
        }
        /// <summary>
        /// 月齢 Clickイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lblMoonPhase_Click(object sender, EventArgs e)
        {
            MoonPhaseForm frmMoonPhase = new MoonPhaseForm(fface);
            frmMoonPhase.ShowDialog();
        }
        #region Tips
        private void lblMoonPhase_MouseHover(object sender, EventArgs e)
        {
            var pos = this.PointToClient(Cursor.Position);
            tipAnyTime.Show(getToolTipMoonPhase(fishing), this, pos.X + 20, pos.Y, 60000);
        }
        private void lblMoonPhase_MouseLeave(object sender, EventArgs e)
        {
            tipAnyTime.Hide(this);
        }
        /// <summary>
        /// 月齢のTipsを取得
        /// </summary>
        /// <param name="iFishing"></param>
        /// <returns></returns>
        private string getToolTipMoonPhase(FishingTool iFishing)
        {
            string ret = string.Format("{0}({1}%)", dicMoonPhaseName[iFishing.MoonPhase], fishing.MoonPercent);
            if (iFishing.MoonPhase == MoonPhase.New || iFishing.MoonPhase == MoonPhase.Full)
            {
                ret += "釣果が向上";
            }
            else if (iFishing.MoonPhase == MoonPhase.FirstQuarter || iFishing.MoonPhase == MoonPhase.LastQuarter)
            {
                ret += "釣果が低下";
            }
            //月齢取得
            List<MoonPhaseDay> moonPhaseList = getMoonPhaseList(iFishing);
            //次の月齢
            TimeSpan spanTime = moonPhaseList[0].EarthDate - DateTime.Now;
            ret += string.Format("\n次の月齢 {0} あと{1:00}:{2:00}", moonPhaseList[0].EarthDate.ToString("MM/dd HH:mm"), spanTime.Days * 24 + spanTime.Hours, spanTime.Minutes);
            //次の新月・満月
            foreach (var moonPhase in moonPhaseList)
            {
                if (moonPhase.MoonPhase == MoonPhase.Full || moonPhase.MoonPhase == MoonPhase.New)
                {
                    spanTime = moonPhase.EarthDate - DateTime.Now;
                    ret += string.Format("\n次の{0} {1} あと{2:00}:{3:00}", dicMoonPhaseName[moonPhase.MoonPhase], moonPhase.EarthDate.ToString("MM/dd HH:mm"), spanTime.Days * 24 + spanTime.Hours, spanTime.Minutes);
                }
            }
            return ret;
        }
        /// <summary>
        /// 月齢リストを取得
        /// </summary>
        /// <param name="iFishing"></param>
        /// <returns></returns>
        private List<MoonPhaseDay> getMoonPhaseList(FishingTool iFishing)
        {
            var ret = new List<MoonPhaseDay>();
            //var vanadate = iFishing.VanaDateTime;

            FFACE.TimerTools.VanaTime v = new FFACE.TimerTools.VanaTime();
            v.Year = iFishing.VanaDateTime.Year;
            v.Month = iFishing.VanaDateTime.Month;
            v.Day = iFishing.VanaDateTime.Day;

            MoonPhase last = FFACEControl.GetMoonPhaseFromVanaTime(v);
            for (int i = 0; i < (12 * 7); i++)
            {
                v = FFACEControl.addVanaDay(v);
                MoonPhase m = FFACEControl.GetMoonPhaseFromVanaTime(v);
                if (last != m)
                {
                    ret.Add(new MoonPhaseDay(m, FFACEControl.GetEarthTimeFromVanaTime(v)));
                }
                last = m;
            }
            return ret;
        }
        private void lblVanaTime_MouseHover(object sender, EventArgs e)
        {
            var pos = this.PointToClient(Cursor.Position);
            tipAnyTime.Show(getToolTipVanaTime(fishing), this, pos.X + 20, pos.Y, 60000);
        }
        private void lblVanaTime_MouseLeave(object sender, EventArgs e)
        {
            tipAnyTime.Hide(this);
        }
        private string getToolTipVanaTime(FishingTool iFishing)
        {
            string ret = "ヴァナ時間 " + iFishing.VanaDateTimeYmdhms + "\n";
            /*
            ret += "ギルド\n";
            ret += "ウィンダス " + getToolTipGuild(iFishing, GuildTimeTableKind.WINDUST) + "\n";
            ret += "セルビナ　 " + getToolTipGuild(iFishing, GuildTimeTableKind.SELBINA) + "\n";
            ret += "ビビキー湾 " + getToolTipGuild(iFishing, GuildTimeTableKind.BIBIKI) + "\n";
            ret += "白門　　　 " + getToolTipGuild(iFishing, GuildTimeTableKind.WHITEGATE) + "\n";
            ret += "機船　　　 " + getToolTipGuild(iFishing, GuildTimeTableKind.SHIP) + "\n";
            */ 
            return ret;
        }
        private string getToolTipGuild(FishingTool iFishing, GuildTimeTableKind iKind)
        {
            string ret = string.Empty;
            //開店・閉店
            string openClose = string.Empty;
            FFACE.TimerTools.VanaTime currVanaTime = iFishing.VanaDateTime;
            FFACE.TimerTools.VanaTime v = iFishing.VanaDateTime;
            if (currVanaTime.Hour >= dicTimeTable[iKind].Open && currVanaTime.Hour < dicTimeTable[iKind].Close)
            {
                openClose = "Open";
                if (v.Hour >= dicTimeTable[iKind].Close) v = FFACEControl.addVanaDay(v);
                v.Hour = dicTimeTable[iKind].Close;
                v.Minute = 0;
                v.Second = 0;
            }
            else
            {
                openClose = "Close";
                if (v.Hour >= dicTimeTable[iKind].Close) v = FFACEControl.addVanaDay(v);
                v.Hour = dicTimeTable[iKind].Open;
                v.Minute = 0;
                v.Second = 0;
            }
            var remain = FFACEControl.GetEarthTimeFromVanaTime(v) - DateTime.Now;

            ret = string.Format("{0,-5} {1,2}分 {2}", openClose, remain.Hours * 60 + remain.Minutes, v);
            return ret;
        }
        #endregion
        #endregion

        #region メソッド
        /// <summary>
        /// lstFishの更新
        /// </summary>
        private void updateFishList()
        {
            if (InvokeRequired)
            {
                Invoke(new UpdateFishListDelegate(updateFishList), null);
            }
            else
            {
                this.Cursor = Cursors.WaitCursor;
                logger.Output(LogLevelKind.DEBUG, "魚リストの更新");
                fishListKey = new Dictionary<string, SettingsPlayerFishListWantedModel>();
                lstFish.BeginUpdate();
                lstFish.Items.Clear();
                string area = fishing.ZoneName;
                string bait = fishing.BaitName;
                if (!settings.FishList.NarrowArea) area = string.Empty;
                if (!settings.FishList.NarrowBait) bait = string.Empty;
                List<FishDBFishModel> fishes = fishDB.SelectFishList(fishing.RodName, area, bait);
                foreach (FishDBFishModel fish in fishes)
                {
                    if (settings.FishList.Mode == Settings.FishListModeKind.ID)
                    {
                        foreach (FishDBIdModel id in fish.IDs)
                        {
                            string fishName = FishingTool.GetViewFishName(fish.FishName, fish.FishType, id.Count, id.Critical, id.ItemType);
                            fishListKey.Add(fishName, new SettingsPlayerFishListWantedModel(fish.FishName, id.ID1, id.ID2, id.ID3, id.ID4));
                            lstFish.Items.Add(fishName, settings.FishList.IsWanted(fish.FishName, id.ID1, id.ID2, id.ID3, id.ID4));
                        }
                    }
                    else if (settings.FishList.Mode == Settings.FishListModeKind.Name)
                    {
                        string fishName = FishingTool.GetViewFishName(fish.FishName, fish.FishType);
                        fishListKey.Add(fishName, new SettingsPlayerFishListWantedModel(fish.FishName, 0, 0, 0, 0));
                        lstFish.Items.Add(fishName, settings.FishList.IsWanted(fish.FishName));
                    }
                }
                //setWantedToSettings();
                lstFish.EndUpdate();
                this.Cursor = Cursors.Default;
            }
        }
        /// <summary>
        /// ハラキリリストの更新
        /// </summary>
        private void updateHarakiriFishList()
        {
            string lastFishname = cmbHarakiriFishname.Text;
            //釣ったことのある魚で、手持ちの魚をリストに追加
            cmbHarakiriFishname.Items.Clear();
            List<string> fishes = fishDB.SelectAllFishName();
            foreach (string fish in fishes)
            {
                if (harakiri.GetHarakiriRemain(fish) > 0)
                {
                    cmbHarakiriFishname.Items.Add(fish);
                }
            }
            //SelectIndex設定
            if (cmbHarakiriFishname.Items.Count > 0)
            {
                cmbHarakiriFishname.SelectedIndex = cmbHarakiriFishname.Items.IndexOf(lastFishname);
            }
        }
        /// <summary>
        /// 魚リストでチェックされている魚をSettingsのWantedにセットする。
        /// </summary>
        private void setWantedToSettings()
        {
            List<SettingsPlayerFishListWantedModel> wanted = new List<SettingsPlayerFishListWantedModel>();
            foreach (string fishName in lstFish.CheckedItems)
            {
                wanted.Add(fishListKey[fishName]);
            }
            settings.FishList.Wanted = wanted;
        }
        /// <summary>
        /// gridHistoryの更新
        /// </summary>
        private void updateHistory()
        {
            if (InvokeRequired)
            {
                Invoke(new UpdateHistoryDelegate(updateHistory), null);
            }
            else
            {
                this.Cursor = Cursors.WaitCursor;
                logger.Output(LogLevelKind.DEBUG, "履歴の更新");
                //詳細タブの設定
                //結果コンボボックスの更新
                FishResultStatusKind lastSelectedResult = FishResultStatusKind.Catch;
                if (cmbHistoryResult.SelectedValue != null)
                {
                    lastSelectedResult = (FishResultStatusKind)cmbHistoryResult.SelectedValue;
                }
                DataTable tblResult = new DataTable();
                tblResult.Columns.Add("Value", typeof(FishResultStatusKind));
                tblResult.Columns.Add("display", typeof(string));
                DataRow row = tblResult.NewRow();
                row["Value"] = FishResultStatusKind.Unknown;
                row["display"] = "(ステータス)";
                tblResult.Rows.Add(row);
                foreach (FishResultStatusKind v in Enum.GetValues(typeof(FishResultStatusKind)))
                {
                    if (v != FishResultStatusKind.Unknown)
                    {
                        row = tblResult.NewRow();
                        row["Value"] = v;
                        row["display"] = v.ToString();
                        tblResult.Rows.Add(row);
                    }
                }
                tblResult.AcceptChanges();
                cmbHistoryResult.SelectedIndexChanged -= new EventHandler(cmbHistoryResult_SelectedIndexChanged);
                cmbHistoryResult.DataSource = tblResult;
                cmbHistoryResult.ValueMember = "Value";
                cmbHistoryResult.DisplayMember = "display";
                cmbHistoryResult.SelectedValue = lastSelectedResult;
                cmbHistoryResult.SelectedIndexChanged += new EventHandler(cmbHistoryResult_SelectedIndexChanged);

                //魚コンボボックスの更新
                string lastSelectedFishName = string.Empty;
                if (cmbHistoryFishName.SelectedValue != null)
                {
                    lastSelectedFishName = (string)cmbHistoryFishName.SelectedValue;
                }
                DataTable tblFishName = new DataTable();
                tblFishName.Columns.Add("Value", typeof(string));
                tblFishName.Columns.Add("display", typeof(string));
                row = tblFishName.NewRow();
                row["Value"] = string.Empty;
                row["display"] = "(魚)";
                tblFishName.Rows.Add(row);
                List<string> fishes = fishHistoryDB.SelectDaylyUniqueFishName(fishing.PlayerName, dateHistory.Value);
                foreach (string v in fishes)
                {
                    row = tblFishName.NewRow();
                    row["Value"] = v;
                    row["display"] = v;
                    tblFishName.Rows.Add(row);
                }
                tblFishName.AcceptChanges();
                cmbHistoryFishName.SelectedIndexChanged -= new EventHandler(cmbHistoryFishName_SelectedIndexChanged);
                cmbHistoryFishName.DataSource = tblFishName;
                cmbHistoryFishName.ValueMember = "Value";
                cmbHistoryFishName.DisplayMember = "display";
                if (fishes.Contains(lastSelectedFishName))
                {
                    cmbHistoryFishName.SelectedValue = lastSelectedFishName;
                }
                else
                {
                    cmbHistoryFishName.SelectedValue = string.Empty;
                }
                cmbHistoryFishName.SelectedIndexChanged += new EventHandler(cmbHistoryFishName_SelectedIndexChanged);

                //履歴グリッドの更新
                FishHistoryDBModel history = fishHistoryDB.SelectDayly(fishing.PlayerName, dateHistory.Value, (FishResultStatusKind)cmbHistoryResult.SelectedValue, (string)cmbHistoryFishName.SelectedValue);
                DataTable tbl = new DataTable();
                tbl.Columns.Add("FishName", typeof(string));
                tbl.Columns.Add("ZoneName", typeof(string));
                tbl.Columns.Add("RodName", typeof(string));
                tbl.Columns.Add("BaitName", typeof(string));
                tbl.Columns.Add("ID", typeof(string));
                tbl.Columns.Add("FishCount", typeof(string));
                tbl.Columns.Add("Result", typeof(FishResultStatusKind));
                tbl.Columns.Add("EarthTime", typeof(DateTime));
                tbl.Columns.Add("VanaTime", typeof(string));
                tbl.Columns.Add("VanaWeekDay", typeof(Image));
                tbl.Columns.Add("MoonPhase", typeof(Image));
                foreach (FishHistoryDBFishModel fish in history.Fishes)
                {
                    row = tbl.NewRow();
                    if (fish.FishName != string.Empty)
                    {
                        row["FishName"] = FishingTool.GetViewFishName(fish.FishName, fish.FishType, fish.FishCount, fish.Critical, fish.ItemType);
                        row["ID"] = string.Format("{0:000}-{1:000}-{2:000}-{3:000}", fish.ID1, fish.ID2, fish.ID3, fish.ID4);
                        row["FishCount"] = fish.FishCount;
                    }
                    else
                    {
                        row["FishName"] = string.Empty;
                        row["ID"] = string.Empty;
                        row["FishCount"] = string.Empty;
                    }
                    row["ZoneName"] = fish.ZoneName;
                    row["RodName"] = fish.RodName;
                    row["BaitName"] = fish.BaitName;
                    row["Result"] = fish.Result;
                    row["EarthTime"] = fish.EarthTime;
                    row["VanaTime"] = fish.VanaTime;
                    row["VanaWeekDay"] = dicWeekDayImage[fish.VanaWeekDay];
                    row["MoonPhase"] = dicMoonPhaseImage[fish.MoonPhase];
                    tbl.Rows.Add(row);
                }
                tbl.AcceptChanges();
                //グリッドにデータソースを設定
                gridHistory.DataSource = tbl.DefaultView;
                //ソート設定
                setGridSort(gridHistory, settings.History.SortColName, settings.History.SortOrder);
                //合計タブの設定
                updateFishingInfo(gridHistorySummary, dateHistory.Value, (FishResultStatusKind)cmbHistoryResult.SelectedValue, (string)cmbHistoryFishName.SelectedValue);

                this.Cursor = Cursors.Default;
            }
        }
        /// <summary>
        /// 釣り情報の更新
        /// </summary>
        private void updateFishingInfo(DataGridView iGrid, DateTime iYmd, FishResultStatusKind iResult, string iFishName)
        {
            if (InvokeRequired)
            {
                Invoke(new UpdateFishingInfoDelegate(updateFishingInfo), iGrid, iYmd, iResult, iFishName);
            }
            else
            {
                this.Cursor = Cursors.WaitCursor;
                logger.Output(LogLevelKind.DEBUG, "釣り情報の更新");
                FishHistoryDBSummaryModel sum = fishHistoryDB.GetSummary(fishing.PlayerName, iYmd, iResult, iFishName);
                DataTable tbl = new DataTable();
                tbl.Columns.Add("Result", typeof(string));
                tbl.Columns.Add("FishName", typeof(string));
                tbl.Columns.Add("Count", typeof(string));
                tbl.Columns.Add("ResultPer", typeof(string));
                tbl.Columns.Add("AllPer", typeof(string));
                //全体
                DataRow row = tbl.NewRow();
                row["Result"] = "全体";
                row["FishName"] = string.Empty;
                row["Count"] = sum.Count;
                row["ResultPer"] = string.Empty;
                row["AllPer"] = "100%";
                tbl.Rows.Add(row);
                foreach (FishHistoryDBSummaryResultModel result in sum.Results)
                {
                    if (result.Count > 0)
                    {
                        row = tbl.NewRow();
                        row["Result"] = result.Result.ToString();
                        row["FishName"] = string.Empty;
                        row["Count"] = result.Count.ToString();
                        row["ResultPer"] = string.Empty;
                        row["AllPer"] = result.TotalPercent.ToString() + "%";
                        tbl.Rows.Add(row);
                        if (result.Result != FishResultStatusKind.NoBite)
                        {
                            foreach (FishHistoryDBSummaryFishModel fish in result.Fishes)
                            {
                                row = tbl.NewRow();
                                row["Result"] = string.Empty;
                                row["FishName"] = fish.FishName;
                                row["Count"] = fish.Count.ToString();
                                row["ResultPer"] = fish.Percent.ToString() + "%";
                                row["AllPer"] = fish.TotalPercent.ToString() + "%";
                                tbl.Rows.Add(row);
                            }
                        }
                    }
                }
                tbl.AcceptChanges();
                //グリッドにデータソースを設定
                iGrid.DataSource = tbl.DefaultView;

                this.Cursor = Cursors.Default;
            }
        }
        /// <summary>
        /// 釣り情報の更新(随時)
        /// </summary>        
        private void updateFishingInfoRealTime()
        {
            if (InvokeRequired)
            {
                Invoke(new UpdateFishingInfoRealTimeDelegate(updateFishingInfoRealTime), null);
            }
            else
            {
                //エリア名
                lblFishingInfoZoneName.Text = fishing.ZoneName;
                //竿名
                lblFishingInfoRodName.Text = fishing.RodNameWithRemain;
                //エサ名
                lblFishingInfoBaitName.Text = fishing.BaitNameWithRemain;
                //釣果数
                float catchCountPerHour = 0;
                if (fishing.TimeElapsed > 0)
                {
                    catchCountPerHour = ((float)fishing.CatchCount / (float)fishing.TimeElapsed) * 60f * 60f;
                }
                lblFishingInfoCatchCount.Text = string.Format("{0}({1:0.0})", fishing.CatchCount, catchCountPerHour);
                //経過時間
                DateTime timeElapsed = new DateTime(2000, 12, 31, 0, 0, 0).AddSeconds(fishing.TimeElapsed);
                lblFishingInfoTimeElapsed.Text = timeElapsed.ToString("HH:mm:ss");
                //連続釣果なし
                lblFishingInfoNoCatchCount.Text = fishing.NoCatchCount.ToString();
                //釣りスキル
                lblFishingInfoSkill.Text = fishing.FishingSkill.ToString();
                //鞄
                lblFishingInfoInventory.Text = string.Format("{0:00}/{1:00}", fishing.InventoryCount, fishing.InventoryMax);
                //サッチェル
                lblFishingInfoSatchel.Text = string.Format("{0:00}/{1:00}", fishing.SatchelCount, fishing.SatchelMax);
                //サック
                lblFishingInfoSack.Text = string.Format("{0:00}/{1:00}", fishing.SackCount, fishing.SackMax);
                //ケース
                lblFishingInfoCase.Text = string.Format("{0:00}/{1:00}", fishing.CaseCount, fishing.CaseMax);
                //ワードローブ
                lblFishingInfoWardrobe.Text = string.Format("{0:00}/{1:00}", fishing.WardrobeCount, fishing.WardrobeMax);
            }
        }
        /// <summary>
        /// ハラキリ情報の更新()
        /// </summary>             
        private void updateHarakiriHistory()
        {
            if (InvokeRequired)
            {
                Invoke(new UpdateHarakiriHistoryDelegate(updateHarakiriHistory), null);
            }
            else
            {
                this.Cursor = Cursors.WaitCursor;
                logger.Output(LogLevelKind.DEBUG, "ハラキリ情報の更新");
                HarakiriDBModel sum = harakiriDB.GetSummary();
                DataTable tbl = new DataTable();
                tbl.Columns.Add("FishName", typeof(string));
                tbl.Columns.Add("ItemName", typeof(string));
                tbl.Columns.Add("Count", typeof(string));
                tbl.Columns.Add("Percent", typeof(string));
                foreach (HarakiriDBFishModel fish in sum.Fishes)
                {
                    DataRow row = tbl.NewRow();
                    row["FishName"] = fish.FishName;
                    row["ItemName"] = string.Empty;
                    row["Count"] = fish.Count.ToString();
                    row["Percent"] = string.Empty;
                    tbl.Rows.Add(row);
                    foreach (HarakiriDBItemModel item in fish.Items)
                    {
                        row = tbl.NewRow();
                        row["FishName"] = string.Empty;
                        row["ItemName"] = item.ItemName;
                        row["Count"] = item.Count.ToString();
                        float per = (float)item.Count / (float)fish.Count;
                        row["Percent"] = per.ToString("P2");
                        tbl.Rows.Add(row);
                    }
                }
                tbl.AcceptChanges();
                //グリッドにデータソースを設定
                gridHarakiri.DataSource = tbl.DefaultView;

                this.Cursor = Cursors.Default;
            }
        }
        /// <summary>
        /// 釣った魚グリッドの更新
        /// </summary>
        private void updateCaughtFishes()
        {
            if (InvokeRequired)
            {
                Invoke(new UpdateCaughtFishesDelegate(updateCaughtFishes), null);
            }
            else
            {
                this.Cursor = Cursors.WaitCursor;
                logger.Output(LogLevelKind.DEBUG, "釣った魚情報の更新");
                DataTable tbl = new DataTable();
                tbl.Columns.Add("No", typeof(int));
                tbl.Columns.Add("Caught", typeof(string));
                tbl.Columns.Add("FishName", typeof(string));
                int no = 0;
                foreach (var fish in settings.CaughtFishes.Fishes)
                {
                    no++;
                    if (!settings.CaughtFishes.ViewNotCaughtOnly ||
                       (settings.CaughtFishes.ViewNotCaughtOnly && !fish.Caught))
                    {
                        DataRow row = tbl.NewRow();
                        row["No"] = no;
                        row["Caught"] = (fish.Caught) ? "★" : string.Empty;
                        row["FishName"] = fish.FishName;
                        tbl.Rows.Add(row);
                    }
                }
                tbl.AcceptChanges();
                //グリッドにデータソースを設定
                gridCaughtFishes.DataSource = tbl.DefaultView;
                //合計数を表示
                lblCaughtFishesCount.Text = string.Format("{0}/{1}匹", settings.GetCoughtFishesCount(), settings.CaughtFishes.Fishes.Count);
                this.Cursor = Cursors.Default;
            }
        }
        /// <summary>
        /// アドオン・プラグインの更新
        /// </summary>
        private void updateAddonPlugin()
        {
            //PlugIn
            //List<string> plugins = control.GetPlugin();
            //settings.UseItemizer = plugins.Contains("itemizer");
            //logger.Output(LogLevelKind.DEBUG, "使用中のプラグイン");
            //foreach (string plugin in plugins) logger.Output(LogLevelKind.DEBUG, plugin);
            //AddOn
            List<string> addons = control.GetAddon();
            settings.UseItemizer = addons.Contains("Itemizer");
            settings.UseEnternity = addons.Contains("enternity");
            settings.UseCancel = addons.Contains("Cancel");
            logger.Output(LogLevelKind.DEBUG, "使用中のアドオン");
            foreach (string addon in addons) logger.Output(LogLevelKind.DEBUG, addon);


            if (settings.UseItemizer)
            {
                lblPluginItemizer.ForeColor = Color.Black;
                chkInventoryFullSack.Enabled = true;
                chkInventoryFullSatchel.Enabled = true;
                chkInventoryFullCase.Enabled = true;
                txtInventoryFullCmdLine.Enabled = true;
                chkInventoryFullCmd.Enabled = true;
                chkNoBaitNoRodSack.Enabled = true;
                chkNoBaitNoRodSatchel.Enabled = true;
                chkNoBaitNoRodCase.Enabled = true;
                txtNoBaitNoRodCmdLine.Enabled = true;
                chkNoBaitNoRodCmd.Enabled = true;
                chkRepairRod.Enabled = true;
            }
            else
            {
                lblPluginItemizer.ForeColor = Color.Silver;
                chkInventoryFullSack.Enabled = false;
                chkInventoryFullSatchel.Enabled = false;
                chkInventoryFullCase.Enabled = false;
                txtInventoryFullCmdLine.Enabled = false;
                chkInventoryFullCmd.Enabled = false;
                chkNoBaitNoRodSack.Enabled = false;
                chkNoBaitNoRodSatchel.Enabled = false;
                chkNoBaitNoRodCase.Enabled = false;
                txtNoBaitNoRodCmdLine.Enabled = false;
                chkNoBaitNoRodCmd.Enabled = false;
                chkRepairRod.Enabled = false;
            }
            if (settings.UseEnternity)
            {
                lblAddonEnternity.ForeColor = Color.Black;
            }
            else
            {
                lblAddonEnternity.ForeColor = Color.Silver;
            }
            if (settings.UseCancel)
            {
                lblAddonCancel.ForeColor = Color.Black;
                chkSneakFishing.Enabled = true;
                txtSneakFishingRemain.Enabled = true;
            }
            else
            {
                lblAddonCancel.ForeColor = Color.Silver;
                chkSneakFishing.Enabled = false;
                txtSneakFishingRemain.Enabled = false;
            }
            
        }
        /// <summary>
        /// 設定保存
        /// </summary>
        private void saveSettings()
        {
            if (InvokeRequired)
            {
                Invoke(new SaveSettingsDelegate(saveSettings), null);
            }
            else
            {
                this.Cursor = Cursors.WaitCursor;
                //フォーム
                if (this.Left > 0) settings.Form.PosX = this.Left;
                if (this.Top > 0) settings.Form.PosY = this.Top;
                settings.Form.Width = this.Width;
                settings.Form.Height = this.Height;
                settings.Form.SplitterDistance = splitMain.SplitterDistance;
                //魚リスト
                if (rdoFishListModeID.Checked) settings.FishList.Mode = Settings.FishListModeKind.ID;
                else if (rdoFishListModeName.Checked) settings.FishList.Mode = Settings.FishListModeKind.Name;
                settings.FishList.NarrowArea = chkFIshListNarrowArea.Checked;
                settings.FishList.NarrowBait = chkFIshListNarrowBait.Checked;
                //釣り設定・動作
                settings.Fishing.Learning = chkLearning.Checked;
                settings.Fishing.SneakFishing = chkSneakFishing.Checked;
                settings.Fishing.SneakFishingRemain = (float)txtSneakFishingRemain.Value;
                settings.Fishing.HP0 = chkHP0.Checked;
                settings.Fishing.HP0Min = (int)txtHP0Min.Value;
                settings.Fishing.HP0Max = (int)txtHP0Max.Value;
                settings.Fishing.ReactionTime = chkReactionTime.Checked;
                settings.Fishing.ReactionTimeMin = (float)txtReactionTimeMin.Value;
                settings.Fishing.ReactionTimeMax = (float)txtReactionTimeMax.Value;
                settings.Fishing.RecastTime = chkRecastTime.Checked;
                settings.Fishing.RecastTimeMin = (float)txtRecastTimeMin.Value;
                settings.Fishing.RecastTimeMax = (float)txtRecastTimeMax.Value;
                settings.Fishing.VanaTime = chkVanaTime.Checked;
                settings.Fishing.VanaTimeFrom = (int)txtVanaTimeFrom.Value;
                settings.Fishing.VanaTimeTo = (int)txtVanaTimeTo.Value;
                settings.Fishing.EarthTime = chkEarthTime.Checked;
                settings.Fishing.EarthTimeFrom = (int)txtEarthTimeFrom.Value;
                settings.Fishing.EarthTimeTo = (int)txtEarthTimeTo.Value;
                settings.Fishing.RepairRod = chkRepairRod.Checked;
                settings.Fishing.WaitTimeout = chkWaitTimeout.Checked;
                //釣り設定・停止条件
                settings.Fishing.IgnoreSmallFish = chkIgnoreSmallFish.Checked;
                settings.Fishing.IgnoreLargeFish = chkIgnoreLargeFish.Checked;
                settings.Fishing.IgnoreMonster = chkIgnoreMonster.Checked;
                settings.Fishing.IgnoreItem = chkIgnoreItem.Checked;
                settings.Fishing.MaxCatch = chkMaxCatch.Checked;
                settings.Fishing.MaxCatchCount = (int)txtMaxCatchCount.Value;
                settings.Fishing.MaxNoCatch = chkMaxNoCatch.Checked;
                settings.Fishing.MaxNoCatchCount = (int)txtMaxNoCatchCount.Value;
                settings.Fishing.MaxSkill = chkMaxSkill.Checked;
                settings.Fishing.MaxSkillValue = (int)txtMaxSkillValue.Value;
                settings.Fishing.ChatTell = chkChatTell.Checked;
                settings.Fishing.ChatSay = chkChatSay.Checked;
                settings.Fishing.ChatParty = chkChatParty.Checked;
                settings.Fishing.ChatLs = chkChatLs.Checked;
                settings.Fishing.ChatShout = chkChatShout.Checked;
                settings.Fishing.ChatEmote = chkChatEmote.Checked;
                settings.Fishing.ChatRestart = chkChatRestart.Checked;
                settings.Fishing.ChatRestartMinute = (int)txtChatRestartMinute.Value;
                settings.Fishing.EntryPort = chkEntryPort.Checked;
                settings.Fishing.EnemyAttackCmd = chkEnemyAttackCmd.Checked;
                settings.Fishing.EnemyAttackCmdLine = txtEnemyAttackCmdLine.Text;
                settings.Fishing.EminenceClear = chkEminenceClear.Checked;
                //釣り設定・鞄いっぱい
                settings.Fishing.InventoryFullSack = chkInventoryFullSack.Checked;
                settings.Fishing.InventoryFullSatchel = chkInventoryFullSatchel.Checked;
                settings.Fishing.InventoryFullCase = chkInventoryFullCase.Checked;
                settings.Fishing.InventoryFullCmd = chkInventoryFullCmd.Checked;
                settings.Fishing.InventoryFullCmdLine = txtInventoryFullCmdLine.Text;
                //釣り設定・竿エサ無し
                settings.Fishing.NoBaitNoRodSack = chkNoBaitNoRodSack.Checked;
                settings.Fishing.NoBaitNoRodSatchel = chkNoBaitNoRodSatchel.Checked;
                settings.Fishing.NoBaitNoRodCase = chkNoBaitNoRodCase.Checked;
                settings.Fishing.NoBaitNoRodCmd = chkNoBaitNoRodCmd.Checked;
                settings.Fishing.NoBaitNoRodCmdLine = txtNoBaitNoRodCmdLine.Text;
                //釣り設定・装備
                settings.Fishing.EquipEnable = chkEquipEnable.Checked;
                settings.Fishing.EquipRod = cmbEquipRod.Text;
                settings.Fishing.EquipBait = cmbEquipBait.Text;
                settings.Fishing.EquipMain = cmbEquipMain.Text;
                settings.Fishing.EquipSub = cmbEquipSub.Text;
                settings.Fishing.EquipHead = cmbEquipHead.Text;
                settings.Fishing.EquipBody = cmbEquipBody.Text;
                settings.Fishing.EquipHands = cmbEquipHands.Text;
                settings.Fishing.EquipLegs = cmbEquipLegs.Text;
                settings.Fishing.EquipFeet = cmbEquipFeet.Text;
                settings.Fishing.EquipNeck = cmbEquipNeck.Text;
                settings.Fishing.EquipWaist = cmbEquipWaist.Text;
                settings.Fishing.EquipBack = cmbEquipBack.Text;
                settings.Fishing.EquipEarLeft = cmbEquipEarLeft.Text;
                settings.Fishing.EquipEarRight = cmbEquipEarRight.Text;
                settings.Fishing.EquipRingLeft = cmbEquipRingLeft.Text;
                settings.Fishing.EquipRingRight = cmbEquipRingRight.Text;
                settings.Fishing.UseWaist = chkUseWaist.Checked;
                settings.Fishing.UseRingLeft = chkUseRingLeft.Checked;
                settings.Fishing.UseRingRight = chkUseRingRight.Checked;
                settings.Fishing.Food = cmbFood.Text;
                settings.Fishing.UseFood = chkUseFood.Checked;
                //履歴
                DataGridViewColumn sortCol = gridHistory.SortedColumn;
                if (sortCol != null)
                {
                    settings.History.SortColName = sortCol.Name;
                    settings.History.SortOrder = gridHistory.SortOrder;
                }
                else
                {
                    settings.History.SortColName = string.Empty;
                    settings.History.SortOrder = SortOrder.None;
                }
                settings.History.ColEarthTime.DisplayIndex = gridHistory.Columns[settings.History.ColEarthTime.Name].DisplayIndex;
                settings.History.ColVanaTime.DisplayIndex = gridHistory.Columns[settings.History.ColVanaTime.Name].DisplayIndex;
                settings.History.ColVanaWeekDay.DisplayIndex = gridHistory.Columns[settings.History.ColVanaWeekDay.Name].DisplayIndex;
                settings.History.ColMoonPhase.DisplayIndex = gridHistory.Columns[settings.History.ColMoonPhase.Name].DisplayIndex;
                settings.History.ColZoneName.DisplayIndex = gridHistory.Columns[settings.History.ColZoneName.Name].DisplayIndex;
                settings.History.ColRodName.DisplayIndex = gridHistory.Columns[settings.History.ColRodName.Name].DisplayIndex;
                settings.History.ColBaitName.DisplayIndex = gridHistory.Columns[settings.History.ColBaitName.Name].DisplayIndex;
                settings.History.ColID.DisplayIndex = gridHistory.Columns[settings.History.ColID.Name].DisplayIndex;
                settings.History.ColFishName.DisplayIndex = gridHistory.Columns[settings.History.ColFishName.Name].DisplayIndex;
                settings.History.ColFishCount.DisplayIndex = gridHistory.Columns[settings.History.ColFishCount.Name].DisplayIndex;
                settings.History.ColResult.DisplayIndex = gridHistory.Columns[settings.History.ColResult.Name].DisplayIndex;
                //_Settings.History.ColEarthTime.Width = gridHistory.Columns[_Settings.History.ColEarthTime.Name].Width;
                //_Settings.History.ColVanaTime.Width = gridHistory.Columns[_Settings.History.ColVanaTime.Name].Width;
                //_Settings.History.ColVanaWeekDay.Width = gridHistory.Columns[_Settings.History.ColVanaWeekDay.Name].Width;
                //_Settings.History.ColMoonPhase.Width = gridHistory.Columns[_Settings.History.ColMoonPhase.Name].Width;
                settings.History.ColZoneName.Width = gridHistory.Columns[settings.History.ColZoneName.Name].Width;
                settings.History.ColRodName.Width = gridHistory.Columns[settings.History.ColRodName.Name].Width;
                settings.History.ColBaitName.Width = gridHistory.Columns[settings.History.ColBaitName.Name].Width;
                settings.History.ColID.Width = gridHistory.Columns[settings.History.ColID.Name].Width;
                settings.History.ColFishName.Width = gridHistory.Columns[settings.History.ColFishName.Name].Width;
                //_Settings.History.ColFishCount.Width = gridHistory.Columns[_Settings.History.ColFishCount.Name].Width;
                //_Settings.History.ColResult.Width = gridHistory.Columns[_Settings.History.ColResult.Name].Width;
                settings.History.ColEarthTime.Visible = chkHistoryVisibleEarthTime.Checked;
                settings.History.ColVanaTime.Visible = chkHistoryVisibleVanaTime.Checked;
                settings.History.ColVanaWeekDay.Visible = chkHistoryVisibleVanaWeekDay.Checked;
                settings.History.ColMoonPhase.Visible = chkHistoryVisibleMoonPhase.Checked;
                settings.History.ColZoneName.Visible = chkHistoryVisibleZoneName.Checked;
                settings.History.ColRodName.Visible = chkHistoryVisibleRodName.Checked;
                settings.History.ColBaitName.Visible = chkHistoryVisibleBaitName.Checked;
                settings.History.ColID.Visible = chkHistoryVisibleID.Checked;
                settings.History.ColFishName.Visible = chkHistoryVisibleFishName.Checked;
                settings.History.ColFishCount.Visible = chkHistoryVisibleFishCount.Checked;
                settings.History.ColResult.Visible = chkHistoryVisibleResult.Checked;
                //ハラキリ
                if (rdoHarakiriInputTypeSelect.Checked)
                {
                    settings.Harakiri.InputType = Settings.HarakiriInputTypeKind.Select;
                }
                else if (rdoHarakiriInputTypeInput.Checked)
                {
                    settings.Harakiri.InputType = Settings.HarakiriInputTypeKind.Input;
                }
                settings.Harakiri.FishNameSelect = cmbHarakiriFishname.Text;
                settings.Harakiri.FishNameInput = txtHarakiriFishname.Text;
                settings.Harakiri.StopFound = chkHarakiriStopFound.Checked;
                //釣った魚
                settings.CaughtFishes.ViewNotCaughtOnly = chkViewNotCaughtOnly.Checked;
                //DB更新
                settings.Global.UpdateDB.Enable = chkUpdateDBEnable.Checked;
                settings.Global.UpdateDB.AutoUpdate = chkUpdateDBAutoUpdate.Checked;
                //設定・一般
                settings.Etc.WindowTopMost = chkWindowTopMost.Checked;
                settings.Etc.WindowFlash = chkWindowFlash.Checked;
                settings.Etc.WindowActivate = chkWindowActivate.Checked;
                settings.Etc.MessageEcho = chkMessageEcho.Checked;
                //設定・ステータスバー表示
                settings.Etc.VisibleMoonPhase = chkStatusBarVisibleMoonPhase.Checked;
                settings.Etc.VisibleVanaTime = chkStatusBarVisibleVanaTime.Checked;
                settings.Etc.VisibleEarthTime = chkStatusBarVisibleEarthTime.Checked;
                settings.Etc.VisibleDayType = chkStatusBarVisibleDayType.Checked;
                settings.Etc.VisibleLoginStatus = chkStatusBarVisibleLoginStatus.Checked;
                settings.Etc.VisiblePlayerStatus = chkStatusBarVisiblePlayerStatus.Checked;
                settings.Etc.VisibleHpBar = chkStatusBarVisibleHpBar.Checked;
                settings.Etc.VisibleHP = chkStatusBarVisibleHP.Checked;
                settings.Etc.VisibleRemainTimeBar = chkStatusBarVisibleRemainTimeBar.Checked;
                settings.Etc.VisibleRemainTime = chkStatusBarVisibleRemainTime.Checked;
                //保存開始
                if (fishing != null)
                {
                    //キャラクター名が英語のみか？
                    string playerName = fishing.PlayerName;
                    if (MiscTool.IsRegexString(playerName, "^[a-zA-Z0-9]+$"))
                    {
                        settings.Save(playerName);
                    }
                }
                this.Cursor = Cursors.Default;
            }

        }
        /// <summary>
        /// 画面ロック
        /// </summary>
        /// <param name="iEnabled">True：ロック、False：アンロック</param>
        private void lockControl(bool iLock)
        {
            if (InvokeRequired)
            {
                Invoke(new LockControlDelegate(lockControl), iLock);
            }
            else
            {
                bool enabled = (iLock == false);
                //開始ボタン
                btnExecFishing.Enabled = enabled;
                btnUpdateFishList.Enabled = enabled;
                //魚リスト
                rdoFishListModeID.Enabled = enabled;
                rdoFishListModeName.Enabled = enabled;
                chkFIshListNarrowArea.Enabled = enabled;
                chkFIshListNarrowBait.Enabled = enabled;
                lstFish.Enabled = enabled;
                //履歴
                dateHistory.Enabled = enabled;
                btnHistoryUpdate.Enabled = enabled;
                cmbHistoryResult.Enabled = enabled;
                cmbHistoryFishName.Enabled = enabled;
                //ハラキリ
                btnExecHarakiri.Enabled = enabled;
                btnHarakiriUpdate.Enabled = enabled;
                //設定
                btnAddonUpdate.Enabled = enabled;
            }
        }
        /// <summary>
        /// 古い形式のデータがある場合コンバートする
        /// </summary>
        private void converter()
        {
            fishDB.Converter();
            fishHistoryDB.Converter();
        }
        /// <summary>
        /// 画面フラッシュ及び画面アクティブ処理
        /// </summary>
        private void flashWindow()
        {
            //フォーム点滅
            if (settings.Etc.WindowFlash)
            {
                //フォームが非アクティブの時だけ点滅する
                if (Form.ActiveForm != this)
                {
                    MiscTool.FlashWindow(this.Handle);
                }
            }
            //フォームアクティブ
            if (settings.Etc.WindowActivate)
            {
                this.WindowState = FormWindowState.Normal;
                this.Activate();
                this.TopMost = true;
                if (!settings.Etc.WindowTopMost) this.TopMost = false;
            }
        }
        /// <summary>
        /// メッセージを表示する
        /// </summary>
        private void setMessage(string iMessage)
        {
            lblMessage.Text = iMessage;
            if (settings.Etc.MessageEcho && iMessage != string.Empty)
            {
                fface.Windower.SendString(string.Format("/echo EnjoyFishing {0}", iMessage));
            }
        }
        #endregion

        #region 監視スレッド
        /// <summary>
        /// 監視メインスレッド
        /// </summary>
        private void threadMonitor()
        {
            while (true)
            {
                //釣り情報更新
                updateFishingInfoRealTime();
                //ステータスバー情報更新
                updateStatusBar(fishing);
                Thread.Sleep(settings.Global.WaitBase);   
            }
        }
        /// <summary>
        /// ステータスバー情報更新
        /// </summary>
        /// <param name="iFishing">FishingTool</param>
        private void updateStatusBar(FishingTool iFishing)
        {
            if (InvokeRequired)
            {
                Invoke(new UpdateStatusBarDelegate(updateStatusBar), iFishing);
            }
            else
            {
                FFACE.TimerTools.VanaTime vt = fface.Timer.GetVanaTime();
                //月齢
                lblMoonPhase.Image = dicMoonPhaseImage[iFishing.MoonPhase];
                //ヴァナ時間
                lblVanaTime.Text = string.Format("{0:00}:{1:00}", iFishing.VanaDateTime.Hour, iFishing.VanaDateTime.Minute);
                //地球時間
                lblEarthTime.Text = iFishing.EarthDateTime.ToString("HH:mm");
                lblEarthTime.ToolTipText = "地球時間 " + iFishing.EarthDateTime.ToString("yyyy/MM/dd HH:mm");
                //曜日
                lblDayType.ToolTipText = dicWeekDayName[iFishing.DayType];
                lblDayType.Image = dicWeekDayImage[iFishing.DayType];
                //Loginステータス
                lblLoginStatus.Text = iFishing.LoginStatus.ToString();
                //Playerステータス
                lblPlayerStatus.Text = iFishing.PlayerStatus.ToString();
                //HP
                lblHP.Text = string.Format("{0}%",iFishing.HpPercent);
                barHP.Value = iFishing.HpPercent;
                //残り時間
                lblRemainTime.Text = string.Format("{0}s", iFishing.RemainTimeCurrent);
                barRemainTime.Value = iFishing.RemainTimePercent;
            }
        }
        #endregion

        #region コントロール値変更イベント
        #region 魚リスト
        private void rdoFishListModeID_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.FishList.Mode = Settings.FishListModeKind.ID;
            settings.FishList.Wanted = new List<SettingsPlayerFishListWantedModel>();
            updateFishList();
        }
        private void rdoFishListName_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.FishList.Mode = Settings.FishListModeKind.Name;
            settings.FishList.Wanted = new List<SettingsPlayerFishListWantedModel>();
            updateFishList();
        }
        private void chkFIshListViewArea_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.FishList.NarrowArea = chkFIshListNarrowArea.Checked;
            updateFishList();
        }
        private void chkFIshListViewBait_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.FishList.NarrowBait = chkFIshListNarrowBait.Checked;
            updateFishList();
        }
        #endregion
        #region 釣り
        #region 釣り設定・動作
        private void chkLearning_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.Learning = chkLearning.Checked;
        }
        private void chkSneakFishing_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.SneakFishing = chkSneakFishing.Checked;
        }
        private void txtSneakFishingRemain_ValueChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.SneakFishingRemain = (float)txtSneakFishingRemain.Value;
        }
        private void chkHP0_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.HP0 = chkHP0.Checked;
        }
        private void txtHP0Min_ValueChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.HP0Min = (int)txtHP0Min.Value;
        }
        private void txtHP0Max_ValueChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.HP0Max = (int)txtHP0Max.Value;
        }
        private void chkIgnoreSmallFish_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.IgnoreSmallFish = chkIgnoreSmallFish.Checked;
        }
        private void chkIgnoreLargeFish_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.IgnoreLargeFish = chkIgnoreLargeFish.Checked;
        }
        private void chkIgnoreMonster_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.IgnoreMonster = chkIgnoreMonster.Checked;
        }
        private void chkIgnoreItem_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.IgnoreItem = chkIgnoreItem.Checked;
        }
        private void chkReactionTime_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.ReactionTime = chkReactionTime.Checked;
        }
        private void txtReactionTimeMin_ValueChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.ReactionTimeMin = (float)txtReactionTimeMin.Value;
        }
        private void txtReactionTimeMax_ValueChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.ReactionTimeMax = (float)txtReactionTimeMax.Value;
        }
        private void chkVanaTime_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.VanaTime = chkVanaTime.Checked;
        }
        private void txtVanaTimeFrom_ValueChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.VanaTimeFrom = (int)txtVanaTimeFrom.Value;
        }
        private void txtVanaTimeTo_ValueChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.VanaTimeTo = (int)txtVanaTimeTo.Value;
        }
        private void chkRecastTime_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.RecastTime = chkRecastTime.Checked;
        }
        private void txtRecastTimeMin_ValueChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.RecastTimeMin = (float)txtRecastTimeMin.Value;
        }
        private void txtRecastTimeMax_ValueChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.RecastTimeMax = (float)txtRecastTimeMax.Value;
        }
        private void chkEarthTime_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.EarthTime = chkEarthTime.Checked;
        }
        private void txtEarthTimeFrom_ValueChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.EarthTimeFrom = (int)txtEarthTimeFrom.Value;
        }
        private void txtEarthTimeTo_ValueChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.EarthTimeTo = (int)txtEarthTimeTo.Value;
        }
        private void chkRepairRod_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.RepairRod = chkRepairRod.Checked;
        }
        private void chkWaitTimeout_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.WaitTimeout = chkWaitTimeout.Checked;
        }
        #endregion
        #region 釣り設定・停止条件
        private void chkMaxCatch_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.MaxCatch = chkMaxCatch.Checked;
        }
        private void txtMaxCatchCount_ValueChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.MaxCatchCount = (int)txtMaxCatchCount.Value;
        }
        private void chkMaxNoCatch_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.MaxNoCatch = chkMaxNoCatch.Checked;
        }
        private void txtMaxNoCatchCount_ValueChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.MaxNoCatchCount = (int)txtMaxNoCatchCount.Value;
        }
        private void chkMaxSkill_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.MaxSkill = chkMaxSkill.Checked;
        }
        private void txtMaxSkillValue_ValueChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.MaxSkillValue = (int)txtMaxSkillValue.Value;
        }
        private void chkChatTell_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.ChatTell = chkChatTell.Checked;
        }
        private void chkChatSay_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.ChatSay = chkChatSay.Checked;
        }
        private void chkChatParty_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.ChatParty = chkChatParty.Checked;
        }
        private void chkChatLs_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.ChatLs = chkChatLs.Checked;
        }
        private void chkChatShout_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.ChatShout = chkChatShout.Checked;
        }
        private void chkChatEmote_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.ChatEmote = chkChatEmote.Checked;
        }
        private void chkChatRestart_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.ChatRestart = chkChatRestart.Checked;
        }
        private void txtChatRestartCount_ValueChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.ChatRestartMinute = (int)txtChatRestartMinute.Value;
        }
        private void chkEntryPort_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.EntryPort = chkEntryPort.Checked;
        }
        private void chkEnemyAttack_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.EnemyAttackCmd = chkEnemyAttackCmd.Checked;
        }
        private void txtEnemyAttackCmd_TextChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.EnemyAttackCmdLine = txtEnemyAttackCmdLine.Text;
        }
        private void chkEminenceClear_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.EminenceClear = chkEminenceClear.Checked;
        }
        #endregion
        #region 釣り設定・鞄いっぱい
        private void chkInventoryFullSack_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.InventoryFullSack = chkInventoryFullSack.Checked;
        }
        private void chkInventoryFullSatchel_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.InventoryFullSatchel = chkInventoryFullSatchel.Checked;
        }
        private void chkInventoryFullCase_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.InventoryFullCase = chkInventoryFullCase.Checked;
        }
        private void chkInventoryFullCmd_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.InventoryFullCmd = chkInventoryFullCmd.Checked;
        }
        private void txtInventoryFullCmdLine_TextChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.InventoryFullCmdLine = txtInventoryFullCmdLine.Text;
        }
        #endregion
        #region 釣り設定・竿エサなし
        private void chkNoBaitNoRodSack_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.NoBaitNoRodSack = chkNoBaitNoRodSack.Checked;
        }
        private void chkNoBaitNoRodSatchel_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.NoBaitNoRodSatchel = chkNoBaitNoRodSatchel.Checked;
        }
        private void chkNoBaitNoRodCase_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.NoBaitNoRodCase = chkNoBaitNoRodCase.Checked;
        }
        private void chkNoBaitNoRodCmd_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.NoBaitNoRodCmd = chkNoBaitNoRodCmd.Checked;
        }
        private void txtNoBaitNoRodCmdLine_TextChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.NoBaitNoRodCmdLine = txtNoBaitNoRodCmdLine.Text;
        }
        #endregion
        #region 釣り設定・装備
        private void chkEquipEnable_CheckedChanged(object sender, EventArgs e)
        {
            cmbEquipRod.Enabled = chkEquipEnable.Checked;
            cmbEquipBait.Enabled = chkEquipEnable.Checked;
            cmbEquipMain.Enabled = chkEquipEnable.Checked;
            cmbEquipSub.Enabled = chkEquipEnable.Checked;
            cmbEquipHead.Enabled = chkEquipEnable.Checked;
            cmbEquipBody.Enabled = chkEquipEnable.Checked;
            cmbEquipHands.Enabled = chkEquipEnable.Checked;
            cmbEquipLegs.Enabled = chkEquipEnable.Checked;
            cmbEquipFeet.Enabled = chkEquipEnable.Checked;
            cmbEquipNeck.Enabled = chkEquipEnable.Checked;
            cmbEquipWaist.Enabled = chkEquipEnable.Checked;
            cmbEquipBack.Enabled = chkEquipEnable.Checked;
            cmbEquipEarLeft.Enabled = chkEquipEnable.Checked;
            cmbEquipEarRight.Enabled = chkEquipEnable.Checked;
            cmbEquipRingLeft.Enabled = chkEquipEnable.Checked;
            cmbEquipRingRight.Enabled = chkEquipEnable.Checked;
            chkUseWaist.Enabled = chkEquipEnable.Checked;
            chkUseRingLeft.Enabled = chkEquipEnable.Checked;
            chkUseRingRight.Enabled = chkEquipEnable.Checked;
            if (startupFlg) return;
            settings.Fishing.EquipEnable = chkEquipEnable.Checked;
        }
        private void cmbEquipRod_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.EquipRod = cmbEquipRod.Text;
        }
        private void cmbEquipBait_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.EquipBait = cmbEquipBait.Text;
        }
        private void cmbEquipMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.EquipMain = cmbEquipMain.Text;
        }
        private void cmbEquipSub_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.EquipSub = cmbEquipSub.Text;
        }
        private void cmbEquipHead_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.EquipHead = cmbEquipHead.Text;
        }
        private void cmbEquipBody_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.EquipBody = cmbEquipBody.Text;
        }
        private void cmbEquipHands_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.EquipHands = cmbEquipHands.Text;
        }
        private void cmbEquipLegs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.EquipLegs = cmbEquipLegs.Text;
        }
        private void cmbEquipFeet_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.EquipFeet = cmbEquipFeet.Text;
        }
        private void cmbEquipNeck_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.EquipNeck = cmbEquipNeck.Text;
        }
        private void cmbEquipWaist_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.EquipWaist = cmbEquipWaist.Text;
        }
        private void cmbEquipBack_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.EquipBack = cmbEquipBack.Text;
        }
        private void cmbEquipEarLeft_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.EquipEarLeft = cmbEquipEarLeft.Text;
        }
        private void cmbEquipEarRight_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.EquipEarRight = cmbEquipEarRight.Text;
        }
        private void cmbEquipRingLeft_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.EquipRingLeft = cmbEquipRingLeft.Text;
        }
        private void cmbEquipRingRight_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.EquipRingRight = cmbEquipRingRight.Text;
        }
        private void chkUseWaist_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.UseWaist = chkUseWaist.Checked;
        }
        private void chkUseRingLeft_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.UseRingLeft = chkUseRingLeft.Checked;
        }
        private void chkUseRingRight_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.UseRingRight = chkUseRingRight.Checked;
        }
        private void cmbFood_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.Food = cmbFood.Text;
        }
        private void chkUseFood_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Fishing.UseFood = chkUseFood.Checked;
        }
        #endregion
        #endregion
        #region ハラキリ
        private void rdoHarakiriInputTypeSelect_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Harakiri.InputType = Settings.HarakiriInputTypeKind.Select;
            cmbHarakiriFishname.Enabled = true;
            txtHarakiriFishname.Enabled = false;
        }
        private void rdoHarakiriInputTypeInput_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Harakiri.InputType = Settings.HarakiriInputTypeKind.Input;
            cmbHarakiriFishname.Enabled = false;
            txtHarakiriFishname.Enabled = true;
        }
        private void cmbHarakiriFishname_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Harakiri.FishNameSelect = cmbHarakiriFishname.Text;
        }
        private void txtHarakiriFishname_TextChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Harakiri.FishNameInput = txtHarakiriFishname.Text;
        }
        private void chkHarakiriStopFound_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Harakiri.StopFound = chkHarakiriStopFound.Checked;
        }
        #endregion
        #region 釣った魚
        private void chkViewCaughtOnly_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.CaughtFishes.ViewNotCaughtOnly = chkViewNotCaughtOnly.Checked;
            updateCaughtFishes();
        }
        #endregion
        #region DB更新
        private void chkUpdateDBEnable_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Global.UpdateDB.Enable = chkUpdateDBEnable.Checked;
            btnExecUpdateDB.Enabled = settings.Global.UpdateDB.Enable;
        }
        private void chkAutoDBUpdate_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Global.UpdateDB.AutoUpdate = chkUpdateDBAutoUpdate.Checked;
        }
        #endregion
        #region 設定
        #region 設定・ステータスバー表示
        private void chkVisibleMoonPhase_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Etc.VisibleMoonPhase = chkStatusBarVisibleMoonPhase.Checked;
            setStatusBarVisible();
        }
        private void chkVisibleVanatime_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Etc.VisibleVanaTime = chkStatusBarVisibleVanaTime.Checked;
            setStatusBarVisible();
        }
        private void chkVisibleEarthTime_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Etc.VisibleEarthTime = chkStatusBarVisibleEarthTime.Checked;
            setStatusBarVisible();
        }
        private void chkVisibleDayType_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Etc.VisibleDayType = chkStatusBarVisibleDayType.Checked;
            setStatusBarVisible();
        }
        private void chkVisibleLoginStatus_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Etc.VisibleLoginStatus = chkStatusBarVisibleLoginStatus.Checked;
            setStatusBarVisible();
        }
        private void chkVisiblePlayerStatus_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Etc.VisiblePlayerStatus = chkStatusBarVisiblePlayerStatus.Checked;
            setStatusBarVisible();
        }
        private void chkVisibleHpBar_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Etc.VisibleHpBar = chkStatusBarVisibleHpBar.Checked;
            setStatusBarVisible();
        }
        private void chkVisibleHP_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Etc.VisibleHP = chkStatusBarVisibleHP.Checked;
            setStatusBarVisible();
        }
        private void chkVisibleRemainTimeBar_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Etc.VisibleRemainTimeBar = chkStatusBarVisibleRemainTimeBar.Checked;
            setStatusBarVisible();
        }
        private void chkVisibleRemainTime_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Etc.VisibleRemainTime = chkStatusBarVisibleRemainTime.Checked;
            setStatusBarVisible();
        }
        #endregion
        #region 設定・履歴列表示
        private void chkHistoryVisibleEarthTime_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.History.ColEarthTime.Visible = chkHistoryVisibleEarthTime.Checked;
            initGridHistory();
        }
        private void chkHistoryVisibleVanaTime_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.History.ColVanaTime.Visible = chkHistoryVisibleVanaTime.Checked;
            initGridHistory();
        }
        private void chkHistoryVisibleVanaWeekDay_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.History.ColVanaWeekDay.Visible = chkHistoryVisibleVanaWeekDay.Checked;
            initGridHistory();
        }
        private void chkHistoryVisibleMoonPhase_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.History.ColMoonPhase.Visible = chkHistoryVisibleMoonPhase.Checked;
            initGridHistory();
        }
        private void chkHistoryVisibleZoneName_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.History.ColZoneName.Visible = chkHistoryVisibleZoneName.Checked;
            initGridHistory();
        }
        private void chkHistoryVisibleRodName_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.History.ColRodName.Visible = chkHistoryVisibleRodName.Checked;
            initGridHistory();
        }
        private void chkHistoryVisibleBaitName_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.History.ColBaitName.Visible = chkHistoryVisibleBaitName.Checked;
            initGridHistory();
        }
        private void chkHistoryVisibleID_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.History.ColID.Visible = chkHistoryVisibleID.Checked;
            initGridHistory();
        }
        private void chkHistoryVisibleFishName_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.History.ColFishName.Visible = chkHistoryVisibleFishName.Checked;
            initGridHistory();
        }
        private void chkHistoryVisibleFishCount_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.History.ColFishCount.Visible = chkHistoryVisibleFishCount.Checked;
            initGridHistory();
        }
        private void chkHistoryVisibleResult_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.History.ColResult.Visible = chkHistoryVisibleResult.Checked;
            initGridHistory();
        }
        #endregion
        #region 設定・一般
        private void rdoSaveModeShared_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Global.SaveMode = Settings.SaveModeKind.Shared;
        }
        private void rdoSaveModeByPlayer_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Global.SaveMode = Settings.SaveModeKind.ByPlayer;
        }
        private void chkTopMost_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Etc.WindowTopMost = chkWindowTopMost.Checked;
            this.TopMost = chkWindowTopMost.Checked;
        }
        private void chkFlashAtFinished_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Etc.WindowFlash = chkWindowFlash.Checked;
        }
        private void chkActivateAtFinished_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Etc.WindowActivate = chkWindowActivate.Checked;
        }
        private void chkMessageEcho_CheckedChanged(object sender, EventArgs e)
        {
            if (startupFlg) return;
            settings.Etc.MessageEcho = chkMessageEcho.Checked;
        }
        #endregion
        #endregion
        #endregion

        #region PolTool/ChatTool/FishingTool/HarakiriToolイベント
        /// <summary>
        /// PolTool ChangeStatusイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PolTool_ChangeStatus(object sender, PolTool.ChangeStatusEventArgs e)
        {
            logger.Output(string.Format("POLステータスが{0}に変更された", e.PolStatus));
            if (e.PolStatus == PolTool.PolStatusKind.LoggedIn)
            {
                //プレイヤーが描画されるまで待機
                logger.Output(LogLevelKind.DEBUG, "プレイヤーのIsRendered待機");
                while (fface.NPC.IsRendered(fface.Player.ID) == false)
                {
                    Thread.Sleep(100);
                }
                Thread.Sleep(10000);
                //初期化
                logger.Output(LogLevelKind.DEBUG, "再起動開始");
                startupFlg = true;
                constructor(pol);
                initForm();
                startupFlg = false;
                //画面ロック解除
                lockControl(false);
                loginFlg = true;
                flashWindow();//画面フラッシュ
            }
            else if (e.PolStatus == PolTool.PolStatusKind.CharacterLoginScreen)
            {
                loginFlg = false;
                lockControl(true);//コントロールロック
                unload();
                flashWindow();//画面フラッシュ
            }
            else if (e.PolStatus == PolTool.PolStatusKind.Unknown)
            {

                flashWindow();//画面フラッシュ
                MessageBox.Show("FF11が終了しました。EnjoyFishingは終了します。", MiscTool.GetAppTitle(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                unload();
                System.Environment.Exit(0);//プログラム終了
            }
        }
        /// <summary>
        /// ChatTool ReceivedCommandイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChatTool_ReceivedCommand(object sender, ChatTool.ReceivedCommandEventArgs e)
        {
            List<string> cmd = e.Command;
            if (cmd.Count > 0)
            {
                switch (cmd[0])
                {
                    case "start":
                        Console.WriteLine(cmd[0]);
                        if(!fishingFlg) startFishing();
                        break;
                    case "stop":
                        Console.WriteLine(cmd[0]);
                        if (fishingFlg) stopFishing(true);
                        break;
                }
            }
        }
        /// <summary>
        /// FishingTool Fishedイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FishingTool_Fished(object sender, FishingTool.FishedEventArgs e)
        {
            //魚リストの更新
            //竿折れの場合は更新しない
            if (e.FishResultStatus != FishResultStatusKind.RodBreak) updateFishList();
            //釣り情報の更新
            updateFishingInfo(gridFishingInfo, DateTime.Now, FishResultStatusKind.Unknown, string.Empty);
        }
        /// <summary>
        /// FishingTool ChangeMessageイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FishingTool_ChangeMessage(object sender, FishingTool.ChangeMessageEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new FishingTool_ChangeMessageDelegate(FishingTool_ChangeMessage), sender, e);
            }
            else
            {
                //メッセージの更新
                setMessage(e.Message);
            }
        }
        /// <summary>
        /// FishingTool ChangeStatusイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FishingTool_ChangeStatus(object sender, FishingTool.ChangeStatusEventArgs e)
        {
            //logger.Output(string.Format("{0} {1}", e.RunningStatus, e.FishingStatus));
            if (InvokeRequired)
            {
                Invoke(new FishingTool_ChangeStatusDelegate(FishingTool_ChangeStatus), sender, e);
            }
            else
            {
                //ステータスバーの背景色を設定
                switch (e.FishingStatus)
                {
                    case FishingTool.FishingStatusKind.Normal:
                        if (e.RunningStatus == FishingTool.RunningStatusKind.Running)
                        {
                            statusStrip.BackColor = Color.FromArgb(0x80, 0xFF, 0xFF);
                        }
                        else
                        {
                            statusStrip.BackColor = SystemColors.Control;
                        }
                        break;
                    case FishingTool.FishingStatusKind.Wait:
                        statusStrip.BackColor = Color.FromArgb(0xFF, 0xFF, 0x80);
                        flashWindow();//画面フラッシュ
                        break;
                    case FishingTool.FishingStatusKind.Error:
                        statusStrip.BackColor = Color.FromArgb(0xFF, 0x80, 0x80);
                        flashWindow();//画面フラッシュ
                        break;
                    default:
                        statusStrip.BackColor = SystemColors.Control;
                        break;
                }
                switch (e.RunningStatus)
                {
                    case FishingTool.RunningStatusKind.UnderStop:
                        this.Cursor = Cursors.WaitCursor;
                        break;
                    case FishingTool.RunningStatusKind.Stop:
                        this.Cursor = Cursors.Default;
                        flashWindow();//画面フラッシュ
                        break;
                }
            }
        }
        /// <summary>
        /// FishingTool CaughtFishesUpdateイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FishingTool_CaughtFishesUpdate(object sender, FishingTool.CaughtFishesUpdateEventArgs e)
        {
            //logger.Output(string.Format("{0} {1}", e.RunningStatus, e.FishingStatus));
            if (InvokeRequired)
            {
                Invoke(new FishingTool_CaughtFishesUpdateDelegate(FishingTool_CaughtFishesUpdate), sender, e);
            }
            else
            {
                updateCaughtFishes();
                saveSettings();
            }
        }
        /// <summary>
        /// HarakiriTool HarakiriOnceイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HarakiriTool_HarakiriOnce(object sender, HarakiriTool.HarakiriOnceEventArgs e)
        {
            updateHarakiriHistory();
        }
        /// <summary>
        /// FishingTool ChangeMessageイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HarakiriTool_ChangeMessage(object sender, HarakiriTool.ChangeMessageEventArgs e)
        {
            if (InvokeRequired)
            {

                Invoke(new HarakiriTool_ChangeMessageDelegate(HarakiriTool_ChangeMessage), sender, e);
            }
            else
            {
                //メッセージの更新
                setMessage(e.Message);
            }
        }
        /// <summary>
        /// FishingTool ChangeStatusイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HarakiriTool_ChangeStatus(object sender, HarakiriTool.ChangeStatusEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new HarakiriTool_ChangeStatusDelegate(HarakiriTool_ChangeStatus), sender, e);
            }
            else
            {
                //ステータスバーの背景色を設定
                switch (e.FishingStatus)
                {
                    case HarakiriTool.HarakiriStatusKind.Normal:
                        if (e.RunningStatus == HarakiriTool.RunningStatusKind.Running)
                        {
                            statusStrip.BackColor = Color.FromArgb(0x80, 0xFF, 0xFF);
                        }
                        else
                        {
                            statusStrip.BackColor = SystemColors.Control;
                        }
                        break;
                    case HarakiriTool.HarakiriStatusKind.Wait:
                        statusStrip.BackColor = Color.FromArgb(0xFF, 0xFF, 0x80);
                        flashWindow();//画面フラッシュ
                        break;
                    case HarakiriTool.HarakiriStatusKind.Error:
                        statusStrip.BackColor = Color.FromArgb(0xFF, 0x80, 0x80);
                        flashWindow();//画面フラッシュ
                        break;
                    default:
                        statusStrip.BackColor = SystemColors.Control;
                        break;
                }
                switch (e.RunningStatus)
                {
                    case HarakiriTool.RunningStatusKind.UnderStop:
                        this.Cursor = Cursors.WaitCursor;
                        break;
                    case HarakiriTool.RunningStatusKind.Stop:
                        this.Cursor = Cursors.Default;
                        flashWindow();//画面フラッシュ
                        break;
                }
            }
        }
        /// <summary>
        /// UpdateDBTool ReceiveMessageイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateDBTool_ReceiveMessage(object sender, UpdateDBTool.ReceiveMessageEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new UpdateDBTool_ReceiveMessageDelegate(UpdateDBTool_ReceiveMessage), sender, e);
            }
            else
            {
                txtUpdateDBLog.SelectionStart = txtUpdateDBLog.Text.Length;
                //改行
                if (txtUpdateDBLog.Text != string.Empty) txtUpdateDBLog.SelectedText = Environment.NewLine;
                //日時
                txtUpdateDBLog.SelectionColor = Color.BlueViolet;
                txtUpdateDBLog.SelectionFont = new System.Drawing.Font("Tahoma", 9, FontStyle.Bold);
                txtUpdateDBLog.SelectedText = "[" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "] ";
                //メッセージ
                txtUpdateDBLog.SelectionColor = e.Color;
                if (e.Bold)
                {
                    txtUpdateDBLog.SelectionFont = new System.Drawing.Font("Meiryo UI", 9, FontStyle.Bold);
                }
                else
                {
                    txtUpdateDBLog.SelectionFont = new System.Drawing.Font("Meiryo UI", 9, FontStyle.Regular);
                }
                txtUpdateDBLog.SelectedText = e.Message;
                //最終行へスクロール
                txtUpdateDBLog.ScrollToCaret();
            }
        }
        /// <summary>
        /// UpdateDBTool NewerVersionイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateDBTool_NewerVersion(object sender, UpdateDBTool.NewerVersionEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new UpdateDBTool_NewerVersionDelegate(UpdateDBTool_NewerVersion), sender, e);
            }
            else
            {
                new Thread(new ParameterizedThreadStart(this.threadNewerVersion))
                {
                    IsBackground = true
                }.Start(e.Url);
            }
        }
        /// <summary>
        /// UpdateDBTool NewerVersionメインスレッド
        /// </summary>
        /// <param name="iUrl"></param>
        private void threadNewerVersion(object iUrl)
        {
            string url = (string)iUrl;
            DialogResult res = MessageBox.Show("新しいバージョンがリリースされています。\nリリースページを表示しますか？", MiscTool.GetAppTitle() + " 新バージョンのリリース", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res == DialogResult.Yes)
            {
                System.Diagnostics.Process.Start(url);
            }
        }
        #endregion

    }
}
