using System;
using System.Collections.Generic;
using System.Threading;
using EliteMMO.API;
using MiscTools;
using NLog;

namespace EnjoyFishing
{
    /// <summary>
    /// FishingTool
    /// </summary>
    public class FishingTool
    {
        #region Dictionary
        private static Dictionary<ChatKbnKind, string> dictionaryChat = new Dictionary<ChatKbnKind, string>()
        {
            {ChatKbnKind.CanNotFishing, "ここで釣りはできません。"},
            {ChatKbnKind.NotEquipRod, "竿を装備していないので釣りができない。"},
            {ChatKbnKind.NotEquipBait, "エサを装備していないので釣りができない。"},
            {ChatKbnKind.BaitCritical, "これは……(.*)の手ごたえだっ！"},
            {ChatKbnKind.BaitSmallFish, "何かがかかったようだ！$"},
            {ChatKbnKind.BaitLargeFish, "何かがかかったようだ！！！$"},
            {ChatKbnKind.BaitItem, "何かがひっかかったようだ。"},
            {ChatKbnKind.BaitMonster, "何かが食らいついてきた！！"},
            {ChatKbnKind.CatchSingle, "{0}は(.*)を手にいれた！"},
            {ChatKbnKind.CatchMultiple, "{0}は(.*)を([0-9]*)尾手にいれた！"},
            {ChatKbnKind.CatchMonster, "{0}はモンスターを釣り上げた！"},
            {ChatKbnKind.CatchKeyItem, "だいじなもの:(.*)を手にいれた！"},
            {ChatKbnKind.CatchTempItem, "テンポラリアイテム:(.*)を手にいれた！"},
            {ChatKbnKind.LineBreak, "釣り糸が切れてしまった。"},
            {ChatKbnKind.RodBreak, "釣り竿が折れてしまった。"},
            {ChatKbnKind.Timeout, "そろそろ逃げられそうだ……！"},
            {ChatKbnKind.InventoryFull, "{0}は見事に(.*)を釣り上げたが、これ以上持てないので、仕方なくリリースした。"},
            {ChatKbnKind.NoBait, "何も釣れなかった。"},
            {ChatKbnKind.Release, "あきらめて仕掛けをたぐり寄せた。"},
            {ChatKbnKind.NoCatch, "獲物に逃げられてしまった。"},
            {ChatKbnKind.EnemyAttack1, "(.*)の攻撃→{0}に、(.*)"},
            {ChatKbnKind.EnemyAttack2, "→{0}に、([0-9]*)ダメージ。"},
            {ChatKbnKind.EnemyAttack3, "(.*)は、遠隔攻撃を実行→{0}に、ミス。(.*)"},
            {ChatKbnKind.SneakWarning1, "スニークの効果がきれそうだ。"},
            {ChatKbnKind.SneakWarning2, "{0}は、スニークの効果がきれた。"},
            {ChatKbnKind.ShipWarning1, "まもなく(.*)へ到着します。"},//汽船航路・外洋航路・銀海航路
            {ChatKbnKind.ShipWarning2, "(.*)に入港いたします。"},//汽船航路・外洋航路・銀海航路
            {ChatKbnKind.ShipWarning3, "(.*)もうすぐ夕照桟橋に着いちまうぜ。(.*)"},//マナクリッパー マリヤカレヤリーフ遊覧
            {ChatKbnKind.ShipWarning4, "(.*)Khots Chalahko : そろそろ到着だ。(.*)"},//マナクリッパー 夕照桟橋→プルゴノルゴ島
            {ChatKbnKind.ShipWarning5, "(.*)ブブリム半島が見えてきたぜ！！(.*)"},//マナクリッパー プルゴノルゴ島→夕照桟橋
            {ChatKbnKind.ShipWarning6, "(.*)Ineuteniace : そろそろ北桟橋ですな。(.*)"},//バージ 主水路(南桟橋→北桟橋)
            {ChatKbnKind.ShipWarning7, "(.*)Eunirange : そろそろ中桟橋かな？(.*)"},//バージ 主水路(北桟橋→中桟橋)
            {ChatKbnKind.ShipWarning8, "(.*)Ineuteniace : そろそろ南桟橋じゃな。(.*)"},//バージ 井守ヶ淵(中桟橋→南桟橋)
            {ChatKbnKind.ShipWarning9, "(.*)Eunirange : そろそろ中桟橋かな。(.*)"},//バージ エメフィ支水路(南桟橋→中桟橋)
            {ChatKbnKind.SkillUp, "{0}の釣りスキルが、(.*)アップ！"},
            {ChatKbnKind.SkillLvUp, "{0}の釣りスキルは、(.*)になった。"},
            {ChatKbnKind.SynthSuccess, "(.*)が([0-9]*)個、合成できた！"},
            {ChatKbnKind.SynthFailure, "合成に失敗した。クリスタルは消失した。"},
            {ChatKbnKind.SynthLostItem, "(.*)を失った…。"},
            {ChatKbnKind.SynthNotEnoughSkill, "うまく合成できない。現在のスキルでは難しすぎるようだ。"},
            {ChatKbnKind.UseItemSuccess1, "{0}は、(.*)を使用。(.*)"},
            {ChatKbnKind.UseItemSuccess2, "{0}が、(.*)を使用した。(.*)"},
            {ChatKbnKind.UseItemFailure, "（コマンドでエラーがあったようです…）"},
            {ChatKbnKind.EminenceProgress, "エミネンス・レコード：『(.*)』……進行度：([0-9]*)/([0-9]*)"},
            {ChatKbnKind.EminenceClear, "エミネンス・レコード：『(.*)』を達成しました。"},
        };
        #endregion

        #region Enum
        private enum ChatKbnKind
        {
            Tell,
            Say,
            Party,
            Linkshell,
            Shout,
            CanNotFishing,
            NotEquipRod,
            NotEquipBait,
            BaitCritical,
            BaitSmallFish,
            BaitLargeFish,
            BaitItem,
            BaitMonster,
            CatchSingle,
            CatchMultiple,
            CatchMonster,
            CatchTempItem,
            CatchKeyItem,
            LineBreak,
            RodBreak,
            Timeout,
            InventoryFull,
            NoBait,
            Release,
            NoCatch,
            Unknown,
            EnemyAttack1,
            EnemyAttack2,
            EnemyAttack3,
            SneakWarning1,
            SneakWarning2,
            ShipWarning1,
            ShipWarning2,
            ShipWarning3,
            ShipWarning4,
            ShipWarning5,
            ShipWarning6,
            ShipWarning7,
            ShipWarning8,
            ShipWarning9,
            SkillUp,
            SkillLvUp,
            SynthSuccess,
            SynthFailure,
            SynthLostItem,
            SynthNotEnoughSkill,
            UseItemSuccess1,
            UseItemSuccess2,
            UseItemFailure,
            EminenceProgress,
            EminenceClear,
        }
        public enum RunningStatusKind
        {
            Running,
            Stop,
            UnderStop
        }
        public enum FishingStatusKind
        {
            Normal,
            Error,
            Wait,
        }
        #endregion

        #region 構造体
        /// <summary>
        /// 割り込み処理用構造体
        /// </summary>
        struct FishingInterrupt
        {
            public bool ChatReceive;
            public bool EnemyAttack;
            public bool SneakWarning;
            public bool ShipWarning;
            public bool ClearEminence;
            public FishingInterrupt(bool iChatReceive, bool iEnemyAttack, bool iSneakWarning, bool iShipWarning, bool iClearEminence)
            {
                this.ChatReceive = iChatReceive;
                this.EnemyAttack = iEnemyAttack;
                this.SneakWarning = iSneakWarning;
                this.ShipWarning = iShipWarning;
                this.ClearEminence = iClearEminence;
            }
        }
        #endregion

        //釣った魚
        private const int NPCINDEX_KATSUNAGA = 37;
        private const string REGEX_FISHEDLIST_DIALOG = @"釣りあげた獲物の種類【([0-9]*)】\(ページ：([0-9]*)/([0-9]*)\)";
        private const string REGEX_FISHEDLIST_OPTIONS = "【(.*)】：(.*)";

        private PolTool pol;
        private EliteAPI api;
        private Settings settings;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private ChatTool chat;
        private EliteAPIControl control;
        private ResourceTool resource;
        private Thread thFishing;
        private Thread thSneak;
        private Thread thTimeElapsed;
        private FishHistoryDB fishHistoryDB;
        private int remainTimeMAX = 0;
        private string lastRodName = string.Empty;
        private string lastBaitName = string.Empty;
        private string lastZoneName = string.Empty;
        private DateTime lastCastDate = new DateTime(2000, 1, 1);
        private int noCatchCount = 0;
        private ushort chatFishingSkill = 0;//チャットから取得した釣りスキル
        private FishingInterrupt interrupt = new FishingInterrupt();

        #region メンバ
        /// <summary>
        /// FishDB
        /// </summary>
        public FishDB FishDB
        {
            get;
            private set;
        }
        /// <summary>
        /// 実行ステータス
        /// </summary>
        public RunningStatusKind RunningStatus
        {
            get;
            private set;
        }
        /// <summary>
        /// 動作ステータス
        /// </summary>
        public FishingStatusKind FishingStatus
        {
            get;
            private set;
        }
        /// <summary>
        /// 動作メッセージ
        /// </summary>
        public string Message
        {
            get;
            private set;
        }
        /// <summary>
        /// 連続釣果無し回数
        /// </summary>
        public int NoCatchCount
        {
            get { return this.noCatchCount; }
        }
        /// <summary>
        /// 月齢
        /// </summary>
        public MoonPhase MoonPhase { get { return (MoonPhase)api.VanaTime.CurrentMoonPhase; } }
        /// <summary>
        /// 月齢（％）
        /// </summary>
        public int MoonPercent { get { return (int)api.VanaTime.CurrentMoonPercent; } }
        /// <summary>
        /// ヴァナ時間
        /// </summary>
        public VanaTime VanaDateTime
        {
            get
            {
                return control.GetVanaTime();
            }
        }
        public uint VanaYear { get { return api.VanaTime.CurrentYear; } }
        public uint VanaMonth { get { return api.VanaTime.CurrentMonth; } }
        public uint VanaDay { get { return api.VanaTime.CurrentDay; } }
        public uint VanaHour { get { return api.VanaTime.CurrentHour; } }
        public uint VanaMinute { get { return api.VanaTime.CurrentMinute; } }
        public uint VanaSecond { get { return api.VanaTime.CurrentSecond; } }
        /// <summary>
        /// ヴァナ時間
        /// </summary>
        public String VanaDateTimeYmdhms
        {
            get
            {
                return string.Format("{0:0000}/{1:00}/{2:00} {3:00}:{4:00}:{5:00}", this.VanaYear,
                                                                                    this.VanaMonth,
                                                                                    this.VanaDay,
                                                                                    this.VanaHour,
                                                                                    this.VanaMinute,
                                                                                    this.VanaSecond);
            }
        }
        /// <summary>
        /// 地球時間
        /// </summary>
        public DateTime EarthDateTime{ get { return DateTime.Now; } }
        /// <summary>
        /// ヴァナ曜日
        /// </summary>
        public Weekday DayType{ get { return (Weekday)api.VanaTime.CurrentWeekDay; } }
        /// <summary>
        /// プレイヤーステータス
        /// </summary>
        public Status PlayerStatus { get { return (Status)api.Player.Status; } }
        /// <summary>
        /// プレイヤー名
        /// </summary>
        public string PlayerName { get { return api.Player.Name; } }
        /// <summary>
        /// ログインステータス
        /// </summary>
        public LoginStatus LoginStatus { get { return (LoginStatus)api.Player.LoginStatus; } }
        /// <summary>
        /// 魚HP（最大）
        /// </summary>
        public int HpMax
        {
            get
            {
                if (api.Player.Status == (uint)Status.FishBite)
                {
                    return api.Fish.MaxStamina;
                }
                else
                {
                    return 0;
                }
            }
        }
        /// <summary>
        /// 魚HP
        /// </summary>
        public int HpCurrent
        {
            get
            {
                if (api.Player.Status== (uint)Status.FishBite)
                {
                    return api.Fish.Stamina;
                }
                else
                {
                    return 0;
                }
            }
        }
        /// <summary>
        /// 魚HP（％）
        /// </summary>
        public int HpPercent
        {
            get
            {
                if (api.Player.Status == (uint)Status.FishBite)
                {
                    double per = 0.00;
                    if (api.Fish.MaxStamina > 0) per = (double)api.Fish.Stamina / (double)api.Fish.MaxStamina;
                    return (int)Math.Round(per * 100);
                }
                else
                {
                    return 0;
                }
            }
        }
        /// <summary>
        /// 残り時間（最大）
        /// </summary>
        public int RemainTimeMAX
        {
            get
            {
                if (this.PlayerStatus == Status.FishBite)
                {
                    if (this.remainTimeMAX < api.Fish.FightTime) this.remainTimeMAX = api.Fish.FightTime;
                    return (int)(this.remainTimeMAX / 100);
                }
                else
                {
                    this.remainTimeMAX = 0;
                    return 0;
                }
            }
        }
        /// <summary>
        /// 残り時間
        /// </summary>
        public int RemainTimeCurrent
        {
            get
            {
                if (this.PlayerStatus == Status.FishBite)
                {
                    return (int)(api.Fish.FightTime / 100);
                }
                else
                {
                    return 0;
                }
            }
        }
        /// <summary>
        /// 残り時間（％）
        /// </summary>
        public int RemainTimePercent
        {
            get
            {
                if (this.PlayerStatus == Status.FishBite)
                {
                    if (this.remainTimeMAX < api.Fish.FightTime) this.remainTimeMAX = api.Fish.FightTime;
                    double per = 0.00;
                    if (this.remainTimeMAX > 0) per = (double)api.Fish.FightTime / (double)this.remainTimeMAX;
                    return (int)Math.Round(per * 100);
                }
                else
                {
                    this.remainTimeMAX = 0;
                    return 0;
                }
            }
        }
        /// <summary>
        /// 鞄所持数
        /// </summary>
        public int InventoryCount
        {
            get { return control.GetInventoryCountByType(InventoryType.Inventory); }
        }
        /// <summary>
        /// 鞄最大所持数
        /// </summary>
        public int InventoryMax
        {
            get { return control.GetInventoryMaxByType(InventoryType.Inventory); }
        }
        /// <summary>
        /// モグサッチェル所持数
        /// </summary>
        public int SatchelCount
        {
            get { return control.GetInventoryCountByType(InventoryType.Satchel); }
        }
        /// <summary>
        /// モグサッチェル最大所持数
        /// </summary>
        public int SatchelMax
        {
            get { return control.GetInventoryMaxByType(InventoryType.Satchel); }
        }
        /// <summary>
        /// モグサック所持数
        /// </summary>
        public int SackCount
        {
            get { return control.GetInventoryCountByType(InventoryType.Sack); }
        }
        /// <summary>
        /// モグサック最大所持数
        /// </summary>
        public int SackMax
        {
            get { return control.GetInventoryMaxByType(InventoryType.Sack); }
        }
        /// <summary>
        /// モグケース所持数
        /// </summary>
        public int CaseCount
        {
            get { return control.GetInventoryCountByType(InventoryType.Case); }
        }
        /// <summary>
        /// モグケース最大所持数
        /// </summary>
        public int CaseMax
        {
            get { return control.GetInventoryMaxByType(InventoryType.Case); }
        }
        /// <summary>
        /// ワードローブ所持数
        /// </summary>
        public int WardrobeCount
        {
            get { return control.GetInventoryCountByType(InventoryType.Wardrobe); }
        }
        /// <summary>
        /// ワードローブ最大所持数
        /// </summary>
        public int WardrobeMax
        {
            get { return control.GetInventoryMaxByType(InventoryType.Wardrobe); }
        }
        /// <summary>
        /// 釣りスキル
        /// </summary>
        public ushort FishingSkill
        {
            get
            {
                if (api.Player.CraftSkills.Fishing.Skill > chatFishingSkill)
                {
                    return api.Player.CraftSkills.Fishing.Skill;
                }
                else
                {
                    return chatFishingSkill;
                }
            }
        }
        /// <summary>
        /// エリア名称
        /// </summary>
        public string ZoneName
        {
            get { return api.Resources.GetString("areas", (uint)api.Player.ZoneId, 1); }
        }
        /// <summary>
        /// 竿名称
        /// </summary>
        public string RodName
        {
            get 
            {
                var rodId = api.Inventory.GetEquippedItem((int)EquipSlot.Range).Id;
                string rodName = resource.GetItem(rodId).Name[1];
                return (rodId != 0 && FishDB.Rods.Contains(rodName)) ? rodName : string.Empty;
            }
        }
        /// <summary>
        /// 竿名称 残数付き
        /// </summary>
        public string RodNameWithRemain
        {
            get
            {
                if (!isRod(this.RodName)) return string.Empty;
                //鞄にアイテムが存在するかチェック
                uint rodId = resource.GetItem(this.RodName).ItemID;
                int remain = control.GetInventoryItemCount(rodId, InventoryType.Inventory) +
                             control.GetInventoryItemCount(rodId, InventoryType.Wardrobe);
                if (settings.UseItemizer)
                {
                    if (settings.Fishing.NoBaitNoRodSatchel) remain += control.GetInventoryItemCount(rodId, InventoryType.Satchel);
                    if (settings.Fishing.NoBaitNoRodSack) remain += control.GetInventoryItemCount(rodId, InventoryType.Sack);
                    if (settings.Fishing.NoBaitNoRodCase) remain += control.GetInventoryItemCount(rodId, InventoryType.Case);
                }
                if (remain <= 1)
                {
                    return RodName;
                }
                else 
                {
                    return string.Format("{0}[{1}]", RodName, remain);
                }
            }
        }
        /// <summary>
        /// エサ名称
        /// </summary>
        public string BaitName
        {
            get 
            {
                var baitId = api.Inventory.GetEquippedItem((int)EquipSlot.Ammo).Id;
                string baitName = resource.GetItem(baitId).Name[1];
                if (baitId != 0 && FishDB.Baits.Contains(baitName))
                {
                    return baitName;
                }
                else
                {
                    return string.Empty;
                }
            }
        }
        /// <summary>
        /// エサ名称 残数付き
        /// </summary>
        public string BaitNameWithRemain
        {
            get
            {
                if (!isBait(this.BaitName)) return string.Empty;
                //鞄にアイテムが存在するかチェック
                var baitId = resource.GetItem(this.BaitName).ItemID;
                int remain = control.GetInventoryItemCount(baitId, InventoryType.Inventory) +
                              control.GetInventoryItemCount(baitId, InventoryType.Wardrobe);
                if (settings.UseItemizer)
                {
                    if (settings.Fishing.NoBaitNoRodSatchel) remain += control.GetInventoryItemCount(baitId, InventoryType.Satchel);
                    if (settings.Fishing.NoBaitNoRodSack) remain += control.GetInventoryItemCount(baitId, InventoryType.Sack);
                    if (settings.Fishing.NoBaitNoRodCase) remain += control.GetInventoryItemCount(baitId, InventoryType.Case);
                }
                if (remain <= 1)
                {
                    return BaitName;
                }
                else
                {
                    return string.Format("{0}[{1}]", BaitName, remain);
                }
            }
        }
        /// <summary>
        /// プレイヤー位置情報
        /// </summary>
        public Position Position
        {
            get
            {
                return new Position()
                {
                    X = api.Player.X,
                    Y = api.Player.Y,
                    Z = api.Player.Z,
                    H = api.Player.H,
                };
            }
        }
        /// <summary>
        /// 経過時間
        /// </summary>
        public int TimeElapsed { get; private set; }
        /// <summary>
        /// 釣果数
        /// </summary>
        public int CatchCount
        {
            get
            {
                return fishHistoryDB.CatchCount;
            }
        }
        /// <summary>
        /// だいじなもの：サーペントの伝説を持っているか
        /// </summary>
        public HasKeyItemKind HasSerpentRumors
        {
            get
            {
                return(api.Player.HasKeyItem((uint)KeyItem.Serpent_Rumors)) ? HasKeyItemKind.Yes : HasKeyItemKind.No;
            }
        }
        /// <summary>
        /// だいじなもの：伝説の巨大魚紀聞を持っているか
        /// </summary>
        public HasKeyItemKind HasAnglersAlmanac
        {
            get
            {
                return (api.Player.HasKeyItem((uint)KeyItem.Anglers_Almanac)) ? HasKeyItemKind.Yes : HasKeyItemKind.No;
            }
        }
        /// <summary>
        /// だいじなもの：フロッグフィッシングを持っているか
        /// </summary>
        public HasKeyItemKind HasFrogFishing
        {
            get
            {
                return (api.Player.HasKeyItem((uint)KeyItem.Frog_Fishing)) ? HasKeyItemKind.Yes : HasKeyItemKind.No;
            }
        }
        /// <summary>
        /// だいじなもの：泳がせ釣りを持っているか
        /// </summary>
        public HasKeyItemKind HasMooching
        {
            get
            {
                return (api.Player.HasKeyItem((uint)KeyItem.Mooching)) ? HasKeyItemKind.Yes : HasKeyItemKind.No;
            }
        }
        #endregion

        #region イベント
        #region Fished
        /// <summary>
        /// Fishedeイベントで返されるデータ
        /// </summary>
        public class FishedEventArgs : EventArgs
        {
            public FishResultStatusKind FishResultStatus;
            public string Message;
        }
        public delegate void FishedEventHandler(object sender, FishedEventArgs e);
        public event FishedEventHandler Fished;
        protected virtual void OnFished(FishedEventArgs e)
        {
            if (Fished != null)
            {
                Fished(this, e);
            }
        }
        private void EventFished(FishResultStatusKind iFishResultStatus)
        {
            //返すデータの設定
            FishedEventArgs e = new FishedEventArgs();
            e.FishResultStatus = iFishResultStatus;
            e.Message = this.Message;
            //イベントの発生
            OnFished(e);
        }
        #endregion
        #region ChangeMessage
        /// <summary>
        /// CangeMessageイベントで返されるデータ
        /// </summary>
        public class ChangeMessageEventArgs : EventArgs
        {
            public string Message;
        }
        public delegate void ChangeMessageEventHandler(object sender, ChangeMessageEventArgs e);
        public event ChangeMessageEventHandler ChangeMessage;
        protected virtual void OnChangeMessage(ChangeMessageEventArgs e)
        {
            if (ChangeMessage != null)
            {
                ChangeMessage(this, e);
            }
        }
        private void EventChangeMessage(string iMessage)
        {
            //返すデータの設定
            ChangeMessageEventArgs e = new ChangeMessageEventArgs();
            e.Message = iMessage;
            //イベントの発生
            OnChangeMessage(e);
        }
        #endregion
        #region ChangeStatus
        /// <summary>
        /// ChangeStatusイベントで返されるデータ
        /// </summary>
        public class ChangeStatusEventArgs : EventArgs
        {
            public FishingTool.RunningStatusKind RunningStatus;
            public FishingTool.FishingStatusKind FishingStatus;
        }
        public delegate void ChangeStatusEventHandler(object sender, ChangeStatusEventArgs e);
        public event ChangeStatusEventHandler ChangeStatus;
        protected virtual void OnChangeStatus(ChangeStatusEventArgs e)
        {
            if (ChangeStatus != null)
            {
                ChangeStatus(this, e);
            }
        }
        private void EventChangeStatus(RunningStatusKind iRunningStatus, FishingStatusKind iFishingStatus)
        {
            //返すデータの設定
            ChangeStatusEventArgs e = new ChangeStatusEventArgs();
            e.RunningStatus = iRunningStatus;
            e.FishingStatus = iFishingStatus;
            //イベントの発生
            OnChangeStatus(e);
        }
        #endregion
        #region CaughtFishesUpdate
        /// <summary>
        /// CaughtFishesUpdateイベントで返されるデータ
        /// </summary>
        public class CaughtFishesUpdateEventArgs : EventArgs
        {
            public string FishName;
        }
        public delegate void CaughtFishesUpdateEventHandler(object sender, CaughtFishesUpdateEventArgs e);
        public event CaughtFishesUpdateEventHandler CaughtFishesUpdate;
        protected virtual void OnCaughtFishesUpdate(CaughtFishesUpdateEventArgs e)
        {
            if (CaughtFishesUpdate != null)
            {
                CaughtFishesUpdate(this, e);
            }
        }
        private void EventCaughtFishesUpdate(string iFishName)
        {
            //返すデータの設定
            CaughtFishesUpdateEventArgs e = new CaughtFishesUpdateEventArgs();
            e.FishName = iFishName;
            //イベントの発生
            OnCaughtFishesUpdate(e);
        }
        #endregion
        /// <summary>
        /// PolTool ChangeStatusイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PolTool_ChangeStatus(object sender, PolTool.ChangeStatusEventArgs e)
        {
            if (e.PolStatus == PolTool.PolStatusKind.LoggedIn)
            {
                setMessage(string.Format("{0}でログインしました", this.PlayerName));
            }
            else
            {
                FishingAbort();
                setFishingStatus(FishingStatusKind.Error);
                if (e.PolStatus == PolTool.PolStatusKind.CharacterLoginScreen)
                {
                    setMessage("キャラクターを選択してください");
                }
                else if (e.PolStatus == PolTool.PolStatusKind.Unknown)
                {
                    setMessage("FF11が終了しました、再起動してください");
                }
            }
        }
        #endregion

        #region コンストラクタ
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="iPol"></param>
        /// <param name="iChat"></param>
        /// <param name="iSettings"></param>
        public FishingTool(PolTool iPol, ResourceTool iResource, ChatTool iChat, Settings iSettings)
        {
            pol = iPol;
            pol.ChangeStatus += new PolTool.ChangeStatusEventHandler(this.PolTool_ChangeStatus);
            api = iPol.EliteAPI;
            chat = iChat;
            resource = iResource;
            settings = iSettings;
            FishDB = new FishDB();
            fishHistoryDB = new FishHistoryDB(this.PlayerName,this.EarthDateTime);
            FishHistoryDBModel history = fishHistoryDB.SelectDayly(this.PlayerName, this.EarthDateTime);
            this.TimeElapsed = history.TimeElapsed;
            control = new EliteAPIControl(pol, resource, chat);
            control.MaxLoopCount = Constants.MAX_LOOP_COUNT;
            control.UseEnternity = settings.UseEnternity;
            control.BaseWait = settings.Global.WaitBase;
            control.ChatWait = settings.Global.WaitChat;
            this.RunningStatus = RunningStatusKind.Stop;
            this.FishingStatus = FishingStatusKind.Normal;
        }
        #endregion

        #region スレッド操作など
        /// <summary>
        /// システム終了処理
        /// </summary>
        public void SystemAbort()
        {
            if (this.thSneak != null && this.thSneak.IsAlive) this.thSneak.Abort();
            if (this.thTimeElapsed != null && this.thTimeElapsed.IsAlive) this.thTimeElapsed.Abort();
            if (this.thFishing != null && this.thFishing.IsAlive) this.thFishing.Abort();
            chat.Stop();
        }
        /// <summary>
        /// 釣り開始
        /// </summary>
        /// <returns></returns>
        public FishingStatusKind FishingStart()
        {
            setRunningStatus(RunningStatusKind.Running);
            setFishingStatus(FishingStatusKind.Normal);
            setMessage(string.Empty);
            //スレッド開始
            thTimeElapsed = new Thread(threadthTimeElapsed);
            thTimeElapsed.Start();
            //スレッド開始
            thFishing = new Thread(threadFishing);
            thFishing.Start();
            thFishing.Join();

            return this.FishingStatus;
        }
        /// <summary>
        /// 釣り中止
        /// </summary>
        /// <returns></returns>
        public bool FishingAbort()
        {
            //ステータス変更
            setRunningStatus(RunningStatusKind.UnderStop);
            //プレイヤステータスがStandingになるまで待つ
            Thread thWaitStatusStanding = new Thread(threadWaitStatusStanding);
            thWaitStatusStanding.Start();
            thWaitStatusStanding.Join();
            //スレッド停止
            if (thTimeElapsed != null && thTimeElapsed.IsAlive) thTimeElapsed.Abort();
            if (thFishing != null && thFishing.IsAlive) thFishing.Abort();
            //ステータス変更
            setRunningStatus(RunningStatusKind.Stop);
            if (FishingStatus == FishingStatusKind.Wait) setFishingStatus(FishingStatusKind.Normal);

            return true;
        }
        /// <summary>
        /// 釣り停止メインスレッド
        /// </summary>
        private void threadWaitStatusStanding()
        {
            for (int i = 0; i < Constants.MAX_LOOP_COUNT && this.PlayerStatus != Status.Standing; i++)
            {
                api.ThirdParty.KeyPress(Keys.ESCAPE);
                Thread.Sleep(settings.Global.WaitBase);
            }
        }
        #endregion

        #region 釣り
        /// <summary>
        /// メインスレッド
        /// </summary>
        private void threadFishing()
        {
            bool firstTime = true;
            interrupt = new FishingInterrupt(false, false, false, false, false);
            noCatchCount = 0;
            lastRodName = string.Empty;
            lastBaitName = string.Empty;
            lastZoneName = string.Empty;
            chat.CurrentIndex = chat.MaxIndex;
            setFishingStatus(FishingStatusKind.Normal);
            FishHistoryDBFishModel fish = new FishHistoryDBFishModel();

            logger.Debug("釣りスレッド開始");
            setMessage("開始しました");

            //メニュー開いていたら閉じる
            if (!control.CloseDialog())
            {
                setRunningStatus(RunningStatusKind.Stop);
                setFishingStatus(FishingStatusKind.Error);
                setMessage("メニューが閉じられない");
                return;
            }

            //着替え
            setEquipGear();
            //魚リスト更新
            EventFished(FishResultStatusKind.Unknown);

            //釣りメインループ
            while (this.RunningStatus == RunningStatusKind.Running)
            {
                //日付が変わったら経過時間クリア
                if (DateTime.Now.Date != lastCastDate.Date)
                {
                    fishHistoryDB = new FishHistoryDB(this.PlayerName, this.EarthDateTime);
                    FishHistoryDBModel history = fishHistoryDB.SelectDayly(this.PlayerName, this.EarthDateTime);
                    this.TimeElapsed = history.TimeElapsed;
                }
                lastCastDate = DateTime.Now.Date;
                //チャット処理
                var cl = new EliteAPI.ChatEntry();
                while (chat.GetNextChatLine(out cl))
                {
                    List<string> chatKbnArgs = new List<string>();
                    ChatKbnKind chatKbn = getChatKbnFromChatline(cl, out chatKbnArgs);
                    logger.Debug("Chat:{0} ChatKbn:{1}", cl.Text, chatKbn);
                }
                //敵からの攻撃感知
                if (this.RunningStatus != RunningStatusKind.Running) break;
                if (interrupt.EnemyAttack)
                {
                    setRunningStatus(RunningStatusKind.Stop);
                    setFishingStatus(FishingStatusKind.Error);
                    setMessage("敵から攻撃されたので停止");
                    //コマンド実行
                    if (settings.Fishing.EnemyAttackCmd)
                    {
                        api.ThirdParty.SendString(settings.Fishing.EnemyAttackCmdLine);
                    }
                    break;
                }
                //チャット感知
                if (this.RunningStatus != RunningStatusKind.Running) break;
                if (interrupt.ChatReceive)
                {
                    if (settings.Fishing.ChatRestart)
                    {
                        setFishingStatus(FishingStatusKind.Wait);
                        double waitSec = (double)(settings.Fishing.ChatRestartMinute * 60);
                        DateTime restartTime = DateTime.Now.AddMinutes(settings.Fishing.ChatRestartMinute);
                        setMessage(string.Format("チャット感知：再始動待ち {0}(地球時間)まで待機", restartTime.ToString("HH:mm:ss")));
                        wait(waitSec, waitSec);
                        //チャットバッファをクリア
                        var waitCl = new EliteAPI.ChatEntry();
                        while (chat.GetNextChatLine(out waitCl))
                        {
                            Thread.Sleep(10);
                        }
                        //チャット受信フラグクリア
                        interrupt.ChatReceive = false;
                        setFishingStatus(FishingStatusKind.Normal);
                    }
                    else
                    {
                        setRunningStatus(RunningStatusKind.Stop);
                        setFishingStatus(FishingStatusKind.Normal);
                        setMessage("チャットを感知したので停止");
                        break;
                    }
                }
                //入港警告感知
                if (this.RunningStatus != RunningStatusKind.Running) break;
                if (settings.Fishing.EntryPort)
                {
                    if (interrupt.ShipWarning)
                    {
                        setRunningStatus(RunningStatusKind.Stop);
                        setFishingStatus(FishingStatusKind.Normal);
                        setMessage("入港するので停止");
                        break;
                    }
                }
                //エミネンスクリア
                if (this.RunningStatus != RunningStatusKind.Running) break;
                if (settings.Fishing.EminenceClear)
                {
                    if (interrupt.ClearEminence)
                    {
                        setRunningStatus(RunningStatusKind.Stop);
                        setFishingStatus(FishingStatusKind.Normal);
                        setMessage("エミネンスをクリアしたので停止");
                        break;
                    }
                }
                //エリア切り替わり感知
                if (this.RunningStatus != RunningStatusKind.Running) break;
                if (this.lastZoneName.Length > 0 && this.lastZoneName != this.ZoneName)
                {
                    setRunningStatus(RunningStatusKind.Stop);
                    setFishingStatus(FishingStatusKind.Error);
                    setMessage("エリアが切り替わったので停止");
                    break;
                }
                //釣果数
                if (this.RunningStatus != RunningStatusKind.Running) break;
                if (settings.Fishing.MaxCatch)
                {
                    if (this.fishHistoryDB.CatchCount >= settings.Fishing.MaxCatchCount)
                    {
                        setRunningStatus(RunningStatusKind.Stop);
                        setFishingStatus(FishingStatusKind.Normal);
                        setMessage("本日の釣果数が規定値になったので停止");
                        break;
                    }
                }
                //釣果無し
                if (this.RunningStatus != RunningStatusKind.Running) break;
                if (settings.Fishing.MaxNoCatch)
                {
                    if (noCatchCount >= settings.Fishing.MaxNoCatchCount)
                    {
                        setRunningStatus(RunningStatusKind.Stop);
                        setFishingStatus(FishingStatusKind.Normal);
                        setMessage("連続釣果無しが規定値になったので停止");
                        break;
                    }
                }
                //釣りスキル
                if (this.RunningStatus != RunningStatusKind.Running) break;
                if (settings.Fishing.MaxSkill)
                {
                    if (this.FishingSkill >= settings.Fishing.MaxSkillValue)
                    {
                        setRunningStatus(RunningStatusKind.Stop);
                        setFishingStatus(FishingStatusKind.Normal);
                        setMessage("釣りスキルが規定値になったので停止");
                        break;
                    }
                }
                //スニーク
                if (this.RunningStatus != RunningStatusKind.Running) break;
                if (settings.Fishing.SneakFishing && settings.UseCancel)
                {
                    if (interrupt.SneakWarning || !control.IsBuff(EliteMMO.API.StatusEffect.Sneak))
                    {
                        setMessage("スニークをかけます");
                        castSneak();
                        interrupt.SneakWarning = false;
                        chat.Clear();
                    }
                }
                //鞄1
                if (this.RunningStatus != RunningStatusKind.Running) break;
                if (this.InventoryCount >= this.InventoryMax)
                {
                    if (!putFish())
                    {
                        if (settings.Fishing.InventoryFullCmd)
                        {
                            api.ThirdParty.SendString(settings.Fishing.InventoryFullCmdLine);
                        }
                        setRunningStatus(RunningStatusKind.Stop);
                        setFishingStatus(FishingStatusKind.Normal);
                        setMessage("鞄がいっぱいなので停止");
                        break;
                    }
                }
                //竿
                if (this.RunningStatus != RunningStatusKind.Running) break;
                if (this.RodName == string.Empty)
                {
                    if (!string.IsNullOrEmpty(lastRodName) &&
                        !control.IsExistItem(lastRodName, InventoryType.Inventory) &&
                        !control.IsExistItem(lastRodName, InventoryType.Wardrobe))
                    {
                        //予備の竿を鞄へ移動
                        if (!getRodBaitItem(lastRodName))
                        {
                            //竿の修理
                            if (settings.Fishing.RepairRod && settings.UseItemizer)
                            {
                                if (!repairRod(lastRodName))
                                {
                                    setRunningStatus(RunningStatusKind.Stop);
                                    setFishingStatus(FishingStatusKind.Error);
                                    break;
                                }
                                Thread.Sleep(2000);
                            }
                        }
                    }
                    if (!setRod(lastRodName))
                    {
                        setRunningStatus(RunningStatusKind.Stop);
                        setFishingStatus(FishingStatusKind.Error);
                        setMessage("釣り竿を装備していないので停止");
                        //コマンド実行
                        if (settings.Fishing.NoBaitNoRodCmd)
                        {
                            api.ThirdParty.SendString(settings.Fishing.NoBaitNoRodCmdLine);
                        }
                        break;
                    }
                }
                //エサ
                if (this.RunningStatus != RunningStatusKind.Running) break;
                if (this.BaitName == string.Empty)
                {
                    if (!string.IsNullOrEmpty(lastBaitName) &&
                        !control.IsExistItem(lastBaitName, InventoryType.Inventory) &&
                        !control.IsExistItem(lastBaitName, InventoryType.Wardrobe))
                    {
                        getRodBaitItem(lastBaitName);
                    }
                    if (!setBait(lastBaitName))
                    {
                        setRunningStatus(RunningStatusKind.Stop);
                        setFishingStatus(FishingStatusKind.Error);
                        setMessage("エサを装備していないので停止");
                        //コマンド実行
                        if (settings.Fishing.NoBaitNoRodCmd)
                        {
                            api.ThirdParty.SendString(settings.Fishing.NoBaitNoRodCmdLine);
                        }
                        break;
                    }
                }
                //鞄2
                if (this.RunningStatus != RunningStatusKind.Running) break;
                if (this.InventoryCount >= this.InventoryMax)
                {
                    if (!putFish())
                    {
                        if (settings.Fishing.InventoryFullCmd)
                        {
                            api.ThirdParty.SendString(settings.Fishing.InventoryFullCmdLine);
                        }
                        setRunningStatus(RunningStatusKind.Stop);
                        setFishingStatus(FishingStatusKind.Normal);
                        setMessage("鞄がいっぱいなので停止");
                        break;
                    }
                }
                //エンチャントアイテム使用
                if (this.RunningStatus != RunningStatusKind.Running) break;
                if (!useEnchantedItem())
                {
                    setRunningStatus(RunningStatusKind.Stop);
                    setFishingStatus(FishingStatusKind.Error);
                    setMessage("エンチャントアイテムが使用できなかったので停止");
                    break;
                }
                //食事
                if (this.RunningStatus != RunningStatusKind.Running) break;
                if (!useFood())
                {
                    setRunningStatus(RunningStatusKind.Stop);
                    setFishingStatus(FishingStatusKind.Error);
                    setMessage("食事アイテムが使用できなかったので停止");
                    break;
                }
                //ヴァナ時間
                if (this.RunningStatus != RunningStatusKind.Running) break;
                while (settings.Fishing.VanaTime &&
                       this.RunningStatus == RunningStatusKind.Running && 
                       !isHourInRange(this.VanaDateTime, settings.Fishing.VanaTimeFrom, settings.Fishing.VanaTimeTo))
                {
                    setFishingStatus(FishingStatusKind.Wait);
                    setMessage(string.Format("{0}時(ヴァナ時間)になるまで待機中", settings.Fishing.VanaTimeFrom));
                    Thread.Sleep(settings.Global.WaitBase);
                }
                setFishingStatus(FishingStatusKind.Normal);
                //地球時間
                if (this.RunningStatus != RunningStatusKind.Running) break;
                while (settings.Fishing.EarthTime &&
                       this.RunningStatus == RunningStatusKind.Running && 
                       !isHourInRange(this.EarthDateTime, settings.Fishing.EarthTimeFrom, settings.Fishing.EarthTimeTo))
                {
                    setFishingStatus(FishingStatusKind.Wait);
                    setMessage(string.Format("{0}時(地球時間)になるまで待機中", settings.Fishing.EarthTimeFrom));
                    Thread.Sleep(settings.Global.WaitBase);
                }
                setFishingStatus(FishingStatusKind.Normal);
                //リキャスト時間待機(初回は判定しない)
                if (!firstTime && settings.Fishing.RecastTime)
                {
                    wait(settings.Fishing.RecastTimeMin, settings.Fishing.RecastTimeMax, "リキャスト待機中：{0:0.0}s");
                }
                //メニュー開いていたら閉じる
                if (this.RunningStatus != RunningStatusKind.Running) break;
                if (!control.CloseDialog())
                {
                    setRunningStatus(RunningStatusKind.Stop);
                    setFishingStatus(FishingStatusKind.Error);
                    setMessage("メニューが閉じられない");
                    break;
                }

                //魚を釣る
                if (this.RunningStatus != RunningStatusKind.Running) break;
                if (!FishingOnce(out fish))
                {
                    //エラー発生時処理
                    setRunningStatus(RunningStatusKind.Stop);
                    setFishingStatus(FishingStatusKind.Error);
                    break;
                }

                firstTime = false;
                Thread.Sleep(settings.Global.WaitBase);
            }
            logger.Debug("釣りスレッド終了");
        }
        /// <summary>
        /// 魚を１回釣る
        /// </summary>
        /// <param name="oFish"></param>
        /// <param name="oMessage"></param>
        /// <returns></returns>
        private bool FishingOnce(out FishHistoryDBFishModel oFish)
        {
            //戻り値初期化
            oFish = new FishHistoryDBFishModel();
            oFish.FishName = string.Empty;
            oFish.FishCount = 0;
            oFish.ZoneName = this.ZoneName;
            oFish.RodName = this.RodName;
            oFish.BaitName = this.BaitName;
            oFish.ID1 = 0;
            oFish.ID2 = 0;
            oFish.ID3 = 0;
            oFish.ID4 = 0;
            oFish.Critical = false;
            oFish.ItemType = FishDBItemTypeKind.Unknown;
            oFish.FishType = FishDBFishTypeKind.Unknown;
            oFish.Result = FishResultStatusKind.NoBite;
            oFish.EarthTime = this.EarthDateTime.ToString("yyyy/MM/dd HH:mm:ss");
            oFish.VanaTime = this.VanaDateTimeYmdhms;
            oFish.VanaWeekDay = this.DayType;
            oFish.MoonPhase = this.MoonPhase;
            oFish.X = (float)Math.Round(this.Position.X, 1, MidpointRounding.AwayFromZero);
            oFish.Y = (float)Math.Round(this.Position.Y, 1, MidpointRounding.AwayFromZero);
            oFish.Z = (float)Math.Round(this.Position.Z, 1, MidpointRounding.AwayFromZero);
            oFish.H = (float)Math.Round(this.Position.H, 1, MidpointRounding.AwayFromZero);
            oFish.Skill = this.FishingSkill;
            oFish.SerpentRumors = this.HasSerpentRumors;
            oFish.AnglersAlmanac = this.HasAnglersAlmanac;
            oFish.Mooching = this.HasMooching;
            oFish.FrogFishing = this.HasFrogFishing;

            bool fishedFlg = false;

            setFishingStatus(FishingStatusKind.Normal);
            setMessage(string.Format("キャスト中：{0}x{1}", this.RodNameWithRemain, this.BaitNameWithRemain));

            //キャスト
            while (this.RunningStatus == RunningStatusKind.Running && this.PlayerStatus != Status.Fishing)
            {
                api.ThirdParty.SendString("/fish");
                Thread.Sleep(2000);//wait

                var cl = new EliteAPI.ChatEntry();
                while (chat.GetNextChatLine(out cl))
                {
                    //チャット区分の取得
                    List<string> chatKbnArgs = new List<string>();
                    ChatKbnKind chatKbn = getChatKbnFromChatline(cl, out chatKbnArgs);
                    logger.Debug("Chat:{0} ChatKbn:{1}", cl.Text, chatKbn);
                    //エラーチェック
                    if (chatKbn == ChatKbnKind.CanNotFishing)
                    {
                        setFishingStatus(FishingStatusKind.Error);
                        setMessage("釣りができない場所だったので停止");
                        return false;
                    }
                    else if (chatKbn == ChatKbnKind.NotEquipRod)
                    {
                        setFishingStatus(FishingStatusKind.Error);
                        setMessage("竿を装備していないので停止");
                        return false;
                    }
                    else if (chatKbn == ChatKbnKind.NotEquipBait)
                    {
                        setFishingStatus(FishingStatusKind.Error);
                        setMessage("エサを装備していないので停止");
                        return false;
                    }
                    else if (lastZoneName.Length > 0 && lastZoneName != this.ZoneName)//エリア切り替わり感知
                    {
                        setFishingStatus(FishingStatusKind.Error);
                        setMessage("エリアが切り替わったので停止");
                        return false;
                    }
                    else if (interrupt.ChatReceive) //チャット感知
                    {
                        while (this.PlayerStatus != Status.Standing)
                        {
                            api.ThirdParty.KeyPress(Keys.ESCAPE);
                            Thread.Sleep(settings.Global.WaitBase);
                        }
                        return true;
                    }
                    else if (interrupt.EnemyAttack) //敵の攻撃感知
                    {
                        return true;
                    }
                }
            }
            this.lastRodName = this.RodName;
            this.lastBaitName = this.BaitName;
            this.lastZoneName = this.ZoneName;
            while (this.RunningStatus == RunningStatusKind.Running)
            {
                var cl = new EliteAPI.ChatEntry();
                //エリア切り替わり感知
                if (lastZoneName.Length > 0 && lastZoneName != this.ZoneName)
                {
                    setFishingStatus(FishingStatusKind.Error);
                    setMessage("エリアが切り替わったので停止");
                    return false;
                } 
                while (chat.GetNextChatLine(out cl))
                {
                    //チャット区分の取得
                    List<string> chatKbnArgs = new List<string>();
                    ChatKbnKind chatKbn = getChatKbnFromChatline(cl, out chatKbnArgs);
                    logger.Debug("Chat:{0} ChatKbn:{1}", cl.Text, chatKbn);
                    
                    if (chatKbn == ChatKbnKind.BaitSmallFish || chatKbn == ChatKbnKind.BaitLargeFish ||
                        chatKbn == ChatKbnKind.BaitItem || chatKbn == ChatKbnKind.BaitMonster)//魚がかかった
                    {
                        //プレイヤステータスがFishBiteになるまで待つ
                        while (this.PlayerStatus != Status.FishBite)
                        {
                            Thread.Sleep(settings.Global.WaitBase);
                        }
                        Thread.Sleep(500);
                        //IDの設定
                        oFish.ID1 = api.Fish.Id1;
                        oFish.ID2 = api.Fish.Id2;
                        oFish.ID3 = api.Fish.Id3;
                        oFish.ID4 = api.Fish.Id4;
                        //魚名称・タイプの設定
                        FishDBFishModel fish = FishDB.SelectFishFromIDZone(oFish.RodName, oFish.ID1, oFish.ID2, oFish.ID3, oFish.ID4, oFish.ZoneName, false);
                        if (!string.IsNullOrEmpty(fish.FishName))
                        {
                            oFish.FishName = fish.FishName;
                            oFish.FishType = fish.FishType;
                            oFish.FishCount = fish.GetId(oFish.ID1, oFish.ID2, oFish.ID3, oFish.ID4).Count;
                            oFish.Critical = fish.GetId(oFish.ID1, oFish.ID2, oFish.ID3, oFish.ID4).Critical;
                            oFish.ItemType = fish.GetId(oFish.ID1, oFish.ID2, oFish.ID3, oFish.ID4).ItemType;
                        }
                        else
                        {
                            oFish.FishType = getTmpFishTypeFromChat(cl.Text);
                            oFish.FishName = FishDB.GetTmpFishNameFromFishType(oFish.FishType, oFish.ID1, oFish.ID2, oFish.ID3, oFish.ID4);
                        }
                        setMessage(string.Format("格闘中：{0}", GetViewFishName(oFish.FishName, oFish.FishType, oFish.FishCount, oFish.Critical, oFish.ItemType)));
                        logger.Info("魚ID：{0:000}-{1:000}-{2:000}-{3:000} 魚タイプ：{4} アイテムタイプ：{5}", oFish.ID1, oFish.ID2, oFish.ID3, oFish.ID4, oFish.FishType, oFish.ItemType);
                        //日時の設定
                        oFish.EarthTime = this.EarthDateTime.ToString("yyyy/MM/dd HH:mm:ss");
                        oFish.VanaTime = this.VanaDateTimeYmdhms;
                        oFish.VanaWeekDay = this.DayType;
                        oFish.MoonPhase = this.MoonPhase;
                        //HP0の設定
                        int waitHP0 = MiscTool.GetRandomNumber(settings.Fishing.HP0Min, settings.Fishing.HP0Max);
                        //反応時間待機
                        if (settings.Fishing.ReactionTime && !settings.Fishing.WaitTimeout)
                        {
                            wait(settings.Fishing.ReactionTimeMin, settings.Fishing.ReactionTimeMax, "反応待機中：{0:0.0}s " + GetViewFishName(oFish.FishName, oFish.FishType, oFish.FishCount, oFish.Critical, oFish.ItemType));
                        }
                        else
                        {
                            //HP0になった瞬間釣り上げると、HP残ったように表示されてしまうので、ウェイトを入れる
                            Thread.Sleep(settings.Global.WaitChat); //wait
                        }
                        //リリース判定
                        if (!isWantedFish(oFish.RodName, oFish.ID1, oFish.ID2, oFish.ID3, oFish.ID4, oFish.ZoneName, oFish.FishType))
                        {
                            //リリースする
                            logger.Debug("リリースする {0}", GetViewFishName(oFish.FishName, oFish.FishType, oFish.FishCount, oFish.Critical, oFish.ItemType));
                            while (this.PlayerStatus == Status.FishBite)
                            {
                                api.ThirdParty.KeyPress(Keys.ESCAPE);
                                Thread.Sleep(settings.Global.WaitBase);
                            }
                            continue;
                        }
                        setMessage(string.Format("格闘中：{0}", GetViewFishName(oFish.FishName, oFish.FishType, oFish.FishCount, oFish.Critical, oFish.ItemType)));
                        //釣り格闘
                        
                        while (api.Fish.Stamina > 0 && this.PlayerStatus == Status.FishBite)
                        {
                            //強制HP0
                            if (settings.Fishing.HP0)
                            {
                                if (isExecHp0(DateTime.Parse(oFish.EarthTime), waitHP0))
                                {
                                    logger.Info("制限時間を過ぎたので、魚のHPを強制的にゼロにします");
                                    api.Fish.Stamina = 0;
                                    Thread.Sleep(1000);
                                }
                            }
                            //格闘
                            api.Fish.FightFish();
                            Thread.Sleep(settings.Global.WaitBase);
                        }
                        //HP0になった瞬間に釣り上げるとFFの画面上ではHPが残ったままになるのでウェイト
                        Thread.Sleep(500);
                        //チャット処理
                        //while(chat.GetNextChatLine(out cl))
                        //{
                        //    ChatKbnKind fightingChatKbn = getChatKbnFromChatline(cl, out chatKbnArgs, ref oChatReceive, ref oEnemyAttack, ref oSneakWarning, ref oShipWarning);
                        //    logger.Output(LogLevelKind.DEBUG, string.Format("Chat:{0} ChatKbn:{1}", cl.Text, fightingChatKbn));
                        //    if (fightingChatKbn == ChatKbnKind.BaitCritical)//クリティカル
                        //    {
                        //        oFish.Critical = true;
                        //    }
                        //}
                        //時間切れのログが表示されるまで待機
                        if (settings.Fishing.WaitTimeout)
                        {
                            logger.Debug("時間切れ待機中");
                            setMessage(string.Format("時間切れ待機中：{0}", GetViewFishName(oFish.FishName, oFish.FishType, oFish.FishCount, oFish.Critical, oFish.ItemType)));
                            int startIndex = chat.CurrentIndex;
                            bool timeUpOkFlg = false;
                            for (int i = 0; i < 1200; i++)//2分間チャットを監視
                            {
                                if (timeUpOkFlg) break;
                                //釣りが中止された場合、待機処理を中止する
                                if (this.PlayerStatus != Status.FishBite) break;
                                //チャット監視
                                var cl2 = chat.GetChatLine(startIndex, false);
                                foreach (var c in cl2)
                                {
                                    List<string> chatKbnTimeoutArgs = new List<string>();
                                    ChatKbnKind chatKbnTimeout = getChatKbnFromChatline(c, out chatKbnTimeoutArgs);
                                    //logger.Output(LogLevelKind.DEBUG, string.Format("Chat:{0} ChatKbn:{1}", c.Text, chatKbn));
                                    if (chatKbnTimeout == ChatKbnKind.Timeout ||
                                        chatKbnTimeout == ChatKbnKind.NoCatch)
                                    {
                                        //反応時間待機
                                        if (settings.Fishing.ReactionTime)
                                        {
                                            float reactionTimeFrom = (settings.Fishing.ReactionTimeMin <= 4.0f) ? settings.Fishing.ReactionTimeMin : 4.0f;
                                            float reactionTimeTo = (settings.Fishing.ReactionTimeMax <= 4.0f) ? settings.Fishing.ReactionTimeMax : 4.0f;
                                            wait(reactionTimeFrom, reactionTimeTo, "反応待機中：{0:0.0}s " + GetViewFishName(oFish.FishName, oFish.FishType, oFish.FishCount, oFish.Critical, oFish.ItemType));
                                        }
                                        timeUpOkFlg = true;
                                        break;
                                    }
                                    else if ( chatKbnTimeout == ChatKbnKind.CatchSingle ||  //{0}は(.*)を手にいれた！"
                                              chatKbnTimeout == ChatKbnKind.CatchMultiple ||//{0}は(.*)を([0-9]*)尾手にいれた！"
                                              chatKbnTimeout == ChatKbnKind.CatchMonster || //{0}はモンスターを釣り上げた！"
                                              chatKbnTimeout == ChatKbnKind.CatchKeyItem || //だいじなもの:(.*)を手にいれた！"
                                              chatKbnTimeout == ChatKbnKind.CatchTempItem ||//テンポラリアイテム:(.*)を手にいれた！"
                                              chatKbnTimeout == ChatKbnKind.LineBreak ||    //釣り糸が切れてしまった。"
                                              chatKbnTimeout == ChatKbnKind.RodBreak ||     //釣り竿が折れてしまった。"
                                              chatKbnTimeout == ChatKbnKind.Timeout ||      //そろそろ逃げられそうだ……！"
                                              chatKbnTimeout == ChatKbnKind.InventoryFull ||//{0}は見事に(.*)を釣り上げたが、これ以上持てないので、仕方なくリリースした。"
                                              chatKbnTimeout == ChatKbnKind.NoBait ||       //何も釣れなかった。"
                                              chatKbnTimeout == ChatKbnKind.Release ||      //あきらめて仕掛けをたぐり寄せた。"
                                              chatKbnTimeout == ChatKbnKind.NoCatch)        //獲物に逃げられてしまった。"
                                    {
                                        timeUpOkFlg = true;
                                        break;
                                    }
                                }
                                Thread.Sleep(100);//wait
                            }
                        }
                        //釣り上げる
                        //プレイヤステータスがFishBite以外になるまで待つ
                        while (this.PlayerStatus == Status.FishBite)
                        {
                            fishedFlg = true;
                            api.ThirdParty.KeyPress(Keys.RETURN);
                            Thread.Sleep(settings.Global.WaitBase);
                        } 
                    }
                    else if (chatKbn == ChatKbnKind.CatchSingle)//釣れた
                    {
                        if (!fishedFlg) continue;//釣り上げていない場合は登録しない
                        oFish.FishName = chatKbnArgs[0];
                        oFish.FishCount = 1;
                        oFish.ItemType = FishDBItemTypeKind.Common;
                        oFish.Result = FishResultStatusKind.Catch;
                        //データベースへの登録
                        if (!putDatabase(oFish)) return false;
                        //連続釣果無しカウントクリア
                        noCatchCount = 0;
                        setMessage(string.Format("釣果：{0}", GetViewFishName(oFish.FishName, oFish.FishType, oFish.FishCount, oFish.Critical, oFish.ItemType)));
                        //イベント発生
                        EventFished(oFish.Result);
                        //プレイヤステータスがStandingになるまで待つ
                        waitChangePlayerStatus(Status.Standing);
                        return true;
                    }
                    else if (chatKbn == ChatKbnKind.CatchMultiple)//複数釣れた
                    {
                        if (!fishedFlg) continue;//釣り上げていない場合は登録しない
                        oFish.FishName = chatKbnArgs[0];
                        oFish.FishCount = int.Parse(chatKbnArgs[1]);
                        oFish.ItemType = FishDBItemTypeKind.Common;
                        oFish.Result = FishResultStatusKind.Catch;
                        //データベースへの登録
                        if (!putDatabase(oFish)) return false;
                        //連続釣果無しカウントクリア
                        noCatchCount = 0;
                        setMessage(string.Format("釣果：{0}", GetViewFishName(oFish.FishName, oFish.FishType, oFish.FishCount, oFish.Critical, oFish.ItemType)));
                        //イベント発生
                        EventFished(oFish.Result);
                        //プレイヤステータスがStandingになるまで待つ
                        waitChangePlayerStatus(Status.Standing);
                        return true;
                    }
                    else if (chatKbn == ChatKbnKind.CatchMonster)//モンスター釣れた
                    {
                        //oFish.FishName = chatKbnArgs[0];
                        oFish.FishCount = 1;
                        oFish.Result = FishResultStatusKind.Catch;
                        //データベースへの登録
                        if (!putDatabase(oFish)) return false;
                        //連続釣果無しカウントクリア
                        noCatchCount = 0;
                        setMessage(string.Format("釣果：{0}", oFish.FishName));
                        //イベント発生
                        EventFished(oFish.Result);
                        //プレイヤステータスがStandingになるまで待つ
                        waitChangePlayerStatus(Status.Standing);
                        return true;
                    }
                    else if (chatKbn == ChatKbnKind.CatchTempItem)//テンポラリアイテム釣れた
                    {
                        if (!fishedFlg) continue;//釣り上げていない場合は登録しない
                        oFish.FishName = chatKbnArgs[0];
                        oFish.FishCount = 1;
                        oFish.ItemType = FishDBItemTypeKind.Temporary;
                        oFish.Result = FishResultStatusKind.Catch;
                        //データベースへの登録
                        if (!putDatabase(oFish)) return false;
                        //連続釣果無しカウントクリア
                        noCatchCount = 0;
                        setMessage(string.Format("釣果：{0}", GetViewFishName(oFish.FishName, oFish.FishType, oFish.FishCount, oFish.Critical, oFish.ItemType)));
                        //イベント発生
                        EventFished(oFish.Result);
                        //プレイヤステータスがStandingになるまで待つ
                        waitChangePlayerStatus(Status.Standing);
                        return true;
                    }
                    else if (chatKbn == ChatKbnKind.CatchKeyItem)//だいじなもの釣れた
                    {
                        if (!fishedFlg) continue;//釣り上げていない場合は登録しない
                        oFish.FishName = chatKbnArgs[0];
                        oFish.FishCount = 1;
                        oFish.ItemType = FishDBItemTypeKind.Key;
                        oFish.Result = FishResultStatusKind.Catch;
                        //データベースへの登録
                        if (!putDatabase(oFish)) return false;
                        //連続釣果無しカウントクリア
                        noCatchCount = 0;
                        setMessage(string.Format("釣果：{0}", GetViewFishName(oFish.FishName, oFish.FishType, oFish.FishCount, oFish.Critical, oFish.ItemType)));
                        //イベント発生
                        EventFished(oFish.Result);
                        //プレイヤステータスがStandingになるまで待つ
                        waitChangePlayerStatus(Status.Standing);
                        return true;
                    }
                    else if (chatKbn == ChatKbnKind.InventoryFull)//鞄いっぱい
                    {
                        oFish.FishName = chatKbnArgs[0];
                        oFish.FishCount = 1;
                        oFish.Result = FishResultStatusKind.Release;
                        //データベースへの登録
                        if (!putDatabase(oFish)) return false;
                        //連続釣果無しカウントクリア
                        noCatchCount = 0;
                        setMessage(string.Format("釣果：鞄いっぱいでリリース {0}", GetViewFishName(oFish.FishName, oFish.FishType, oFish.FishCount, oFish.Critical, oFish.ItemType)));
                        //イベント発生
                        EventFished(oFish.Result);
                        //プレイヤステータスがStandingになるまで待つ
                        waitChangePlayerStatus(Status.Standing);
                        return true;
                    }
                    else if (chatKbn == ChatKbnKind.NoBait)//何も釣れなかった
                    {
                        oFish.Result = FishResultStatusKind.NoBite;
                        //データベースへの登録
                        if (!putDatabase(oFish)) return false;
                        //連続釣果無しカウントアップ
                        noCatchCount++;
                        setMessage(string.Format("釣果：何も釣れなかった {0}連続", noCatchCount));
                        //イベント発生
                        EventFished(oFish.Result);
                        //プレイヤステータスがStandingになるまで待つ
                        waitChangePlayerStatus(Status.Standing);
                        return true;
                    }
                    else if (chatKbn == ChatKbnKind.Release)//リリース
                    {
                        oFish.Result = FishResultStatusKind.Release;
                        //データベースへの登録
                        if (!putDatabase(oFish)) return false;
                        //連続釣果無しカウントクリア
                        noCatchCount = 0;
                        setMessage(string.Format("釣果：リリース {0}", GetViewFishName(oFish.FishName, oFish.FishType, oFish.FishCount, oFish.Critical, oFish.ItemType)));
                        //イベント発生
                        EventFished(oFish.Result);
                        //プレイヤステータスがStandingになるまで待つ
                        waitChangePlayerStatus(Status.Standing);
                        return true;
                    }
                    else if (chatKbn == ChatKbnKind.NoCatch)//逃げられた
                    {
                        oFish.Result = FishResultStatusKind.NoCatch;
                        //データベースへの登録
                        if (!putDatabase(oFish)) return false;
                        //連続釣果無しカウントクリア
                        noCatchCount = 0;
                        setMessage(string.Format("釣果：逃げられた {0}", GetViewFishName(oFish.FishName, oFish.FishType, oFish.FishCount, oFish.Critical, oFish.ItemType)));
                        //イベント発生
                        EventFished(oFish.Result);
                        //プレイヤステータスがStandingになるまで待つ
                        waitChangePlayerStatus(Status.Standing);
                        return true;
                    }
                    else if (chatKbn == ChatKbnKind.LineBreak)//糸切れ
                    {
                        oFish.Result = FishResultStatusKind.LineBreak;
                        //データベースへの登録
                        if (!putDatabase(oFish)) return false;
                        //連続釣果無しカウントクリア
                        noCatchCount = 0;
                        setMessage(string.Format("釣果：糸切れ {0}", GetViewFishName(oFish.FishName, oFish.FishType, oFish.FishCount, oFish.Critical, oFish.ItemType)));
                        //イベント発生
                        EventFished(oFish.Result);
                        //プレイヤステータスがStandingになるまで待つ
                        waitChangePlayerStatus(Status.Standing);
                        return true;
                    }
                    else if (chatKbn == ChatKbnKind.RodBreak)//竿折れ
                    {
                        oFish.Result = FishResultStatusKind.RodBreak;
                        //データベースへの登録
                        if (!putDatabase(oFish)) return false;
                        //連続釣果無しカウントクリア
                        noCatchCount = 0;
                        setMessage(string.Format("釣果：竿折れ {0}", GetViewFishName(oFish.FishName, oFish.FishType, oFish.FishCount, oFish.Critical, oFish.ItemType)));
                        //イベント発生
                        EventFished(oFish.Result);
                        //プレイヤステータスがStandingになるまで待つ
                        waitChangePlayerStatus(Status.Standing);
                        return true;
                    }
                    else if (chatKbn == ChatKbnKind.BaitCritical)//クリティカル
                    {
                        oFish.Critical = true;
                    }
                    else if (interrupt.ChatReceive) //チャット感知
                    {
                        //プレイヤステータスがStandingになるまで待つ
                        while (this.PlayerStatus != Status.Standing)
                        {
                            api.ThirdParty.KeyPress(Keys.ESCAPE);
                            Thread.Sleep(settings.Global.WaitBase);
                        }
                        return true;
                    }
                    else if (interrupt.EnemyAttack) //敵の攻撃感知
                    {
                        //プレイヤステータスがStandingになるまで待つ
                        while (this.PlayerStatus != Status.Standing)
                        {
                            api.ThirdParty.KeyPress(Keys.ESCAPE);
                            Thread.Sleep(settings.Global.WaitBase);
                        }
                        return true;
                    }
                }
                Thread.Sleep(settings.Global.WaitChat);
            }

            return true;

        }

        /// <summary>
        /// FishDB・FishHistoryDBへの登録処理
        /// </summary>
        /// <param name="iFish">FishHistoryDBFishModel</param>
        /// <returns></returns>
        private bool putDatabase(FishHistoryDBFishModel iFish)
        {
            //FishDBに登録
            if (iFish.ID1 != 0 && iFish.ID2 != 0 && iFish.ID3 != 0 && iFish.ID4 != 0)
            {
                //FishTypeの設定
                if (!isTmpFishFromName(iFish.FishName))
                {
                    if (iFish.FishType == FishDBFishTypeKind.UnknownSmallFish) iFish.FishType = FishDBFishTypeKind.SmallFish;
                    if (iFish.FishType == FishDBFishTypeKind.UnknownLargeFish) iFish.FishType = FishDBFishTypeKind.LargeFish;
                    if (iFish.FishType == FishDBFishTypeKind.UnknownItem) iFish.FishType = FishDBFishTypeKind.Item;
                }
                FishDBIdModel id = new FishDBIdModel(iFish.ID1, iFish.ID2, iFish.ID3, iFish.ID4, iFish.FishCount, iFish.Critical, iFish.ItemType);
                if (!FishDB.AddFish(iFish.RodName, iFish.FishName, iFish.FishType, id, iFish.ZoneName, iFish.BaitName))
                {
                    setMessage("FishDBデータベースへの登録に失敗");
                    return false;
                } 

            }
            //FishHistoryDBに登録
            if (!fishHistoryDB.AddFish(this.PlayerName, this.TimeElapsed, iFish))
            {
                setMessage("FishHistoryDBデータベースへの登録に失敗");
                return false;
            }
            //釣れた魚を登録
            if(iFish.Result== FishResultStatusKind.Catch &&
               (iFish.FishType == FishDBFishTypeKind.SmallFish || iFish.FishType == FishDBFishTypeKind.LargeFish)){
                   settings.CaughtFishesUpdate(iFish.FishName, true);
                   EventCaughtFishesUpdate(iFish.FishName);
            }
            return true;
        }
        /// <summary>
        /// 強制HP0して良いか判定
        /// </summary>
        /// <param name="iFIshStartDatetime">釣り開始日時</param>
        /// <param name="iWaitSec">待ち時間</param>
        /// <returns></returns>
        private bool isExecHp0(DateTime iFIshStartDatetime, int iWaitSec)
        {
            DateTime chkDateTime = iFIshStartDatetime.AddSeconds(iWaitSec);
            if (DateTime.Now > chkDateTime) return true;
            return false;
        }
        /// <summary>
        /// 指定のIDが釣り上げる対象かをチェック
        /// </summary>
        /// <param name="iRodName">竿名称</param>
        /// <param name="iID1">ID1</param>
        /// <param name="iID2">ID2</param>
        /// <param name="iID3">ID3</param>
        /// <param name="iID4">ID4</param>
        /// <param name="iFishType">魚タイプ</param>
        /// <returns>釣り上げ対象の場合True</returns>
        private bool isWantedFish(string iRodName, int iID1, int iID2, int iID3, int iID4, string iZoneName, FishDBFishTypeKind iFishType)
        {
            //FishType
            if ((settings.Fishing.IgnoreSmallFish && (iFishType == FishDBFishTypeKind.SmallFish || iFishType == FishDBFishTypeKind.UnknownSmallFish)) ||
                (settings.Fishing.IgnoreLargeFish && (iFishType == FishDBFishTypeKind.LargeFish || iFishType == FishDBFishTypeKind.UnknownLargeFish)) ||
                (settings.Fishing.IgnoreItem && (iFishType == FishDBFishTypeKind.Item || iFishType == FishDBFishTypeKind.UnknownItem)) ||
                (settings.Fishing.IgnoreMonster && iFishType == FishDBFishTypeKind.UnknownMonster))
            { 
                return false;
            }
            //学習モード
            if (settings.Fishing.Learning && (iFishType == FishDBFishTypeKind.UnknownSmallFish ||
                                              iFishType == FishDBFishTypeKind.UnknownLargeFish ||
                                              iFishType == FishDBFishTypeKind.UnknownItem ||
                                              iFishType == FishDBFishTypeKind.UnknownMonster||
                                              iFishType == FishDBFishTypeKind.Unknown))
            {
                return true;
            }
            //Wanted
            FishDBFishModel fish = FishDB.SelectFishFromIDZone(iRodName, iID1, iID2, iID3, iID4, iZoneName, true);
            foreach (SettingsPlayerFishListWantedModel wantedFish in settings.FishList.Wanted)
            {
                if (settings.FishList.Mode == Settings.FishListModeKind.ID)
                {
                    if (wantedFish.FishName == fish.FishName)
                    {
                        if (wantedFish.ID1 == iID1 && wantedFish.ID2 == iID2 && wantedFish.ID3 == iID3 && wantedFish.ID4 == iID4)
                        {
                            return true;
                        }

                    }
                }
                else if (settings.FishList.Mode == Settings.FishListModeKind.Name)
                {
                    if (wantedFish.FishName == fish.FishName)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// チャット内容からChatKbnKindを取得する
        /// </summary>
        /// <param name="iCl">チャットライン</param>
        /// <returns>チャット区分</returns>
        private ChatKbnKind getChatKbnFromChatline(EliteAPI.ChatEntry iCl, out List<string> oArgs)
        {
            oArgs = new List<string>();
            switch (iCl.ChatType)
            {
                case (int)ChatMode.RcvdTell:
                //case ChatMode.SentTell:
                    if(settings.Fishing.ChatTell) interrupt.ChatReceive = true;
                    return ChatKbnKind.Tell;
                case (int)ChatMode.RcvdSay:
                case (int)ChatMode.SentSay:
                    if (settings.Fishing.ChatSay) interrupt.ChatReceive = true;
                    return ChatKbnKind.Say;
                case (int)ChatMode.RcvdParty:
                case (int)ChatMode.SentParty:
                    if (settings.Fishing.ChatParty) interrupt.ChatReceive = true;
                    return ChatKbnKind.Party;
                case (int)ChatMode.RcvdLinkShell:
                case (int)ChatMode.SentLinkShell:
                    if (settings.Fishing.ChatLs) interrupt.ChatReceive = true;
                    return ChatKbnKind.Linkshell;
                case (int)ChatMode.RcvdShout:
                case (int)ChatMode.SentShout:
                case (int)ChatMode.RcvdYell:
                case (int)ChatMode.SentYell:
                    if (settings.Fishing.ChatShout) interrupt.ChatReceive = true;
                    return ChatKbnKind.Shout;
                case (int)ChatMode.RcvdEmote:
                case (int)ChatMode.SentEmote:
                    if (settings.Fishing.ChatEmote && iCl.Text.Contains(this.PlayerName)) interrupt.ChatReceive = true;
                    return ChatKbnKind.Shout;
            }
            foreach (KeyValuePair<ChatKbnKind, string> v in dictionaryChat)
            {
                string searchStr = string.Empty;
                //プレイヤー名の置換
                if (v.Key == ChatKbnKind.CatchSingle ||
                    v.Key == ChatKbnKind.CatchMultiple ||
                    v.Key == ChatKbnKind.CatchMonster ||
                    v.Key == ChatKbnKind.InventoryFull ||
                    v.Key == ChatKbnKind.EnemyAttack1 ||
                    v.Key == ChatKbnKind.EnemyAttack2 ||
                    v.Key == ChatKbnKind.EnemyAttack3 ||
                    v.Key == ChatKbnKind.SneakWarning2 ||
                    v.Key == ChatKbnKind.SkillUp ||
                    v.Key == ChatKbnKind.SkillLvUp ||
                    v.Key == ChatKbnKind.UseItemSuccess1 ||
                    v.Key == ChatKbnKind.UseItemSuccess2)
                {
                    searchStr = string.Format(v.Value, this.PlayerName);
                }
                else
                {
                    searchStr = v.Value;
                }

                if (MiscTool.IsRegexString(iCl.Text, searchStr))
                {
                    oArgs = MiscTool.GetRegexString(iCl.Text, searchStr);
                    //敵から攻撃受けた
                    if (v.Key == ChatKbnKind.EnemyAttack1 || v.Key == ChatKbnKind.EnemyAttack2 || v.Key == ChatKbnKind.EnemyAttack3)
                    {
                        interrupt.EnemyAttack = true;
                    }
                    //スニーク切れそう
                    if (v.Key == ChatKbnKind.SneakWarning1/* || v.Key == ChatKbnKind.SneakWarning2*/)
                    {
                        interrupt.SneakWarning = true;
                    }
                    //入港
                    if (v.Key == ChatKbnKind.ShipWarning1 || v.Key == ChatKbnKind.ShipWarning2 ||
                        v.Key == ChatKbnKind.ShipWarning3 || v.Key == ChatKbnKind.ShipWarning4 ||
                        v.Key == ChatKbnKind.ShipWarning5 || v.Key == ChatKbnKind.ShipWarning6 ||
                        v.Key == ChatKbnKind.ShipWarning7 || v.Key == ChatKbnKind.ShipWarning8 ||
                        v.Key == ChatKbnKind.ShipWarning9)
                    {
                        interrupt.ShipWarning = true;
                    }
                    //エミネンスクリアー
                    if (v.Key == ChatKbnKind.EminenceClear)
                    {
                        if (FishDB.Eminences.Contains(new EminenceDBEminenceModel(oArgs[0], true)))
                        {
                            interrupt.ClearEminence = true;
                        }
                    }
                    //スキルアップ
                    if (v.Key == ChatKbnKind.SkillLvUp)
                    {
                        chatFishingSkill = ushort.Parse(oArgs[0]);
                    }
                    return v.Key;
                }
            }
            return ChatKbnKind.Unknown;
        }
        /// <summary>
        /// FishTypeをチャットテキストから判別して返す
        /// </summary>
        /// <param name="iChatText">チャットテキスト</param>
        /// <returns>FishType</returns>
        private FishDBFishTypeKind getFishTypeFromChat(string iChatText)
        {
            if (MiscTool.IsRegexString(iChatText, dictionaryChat[ChatKbnKind.BaitSmallFish])) return FishDBFishTypeKind.SmallFish;
            if (MiscTool.IsRegexString(iChatText, dictionaryChat[ChatKbnKind.BaitLargeFish])) return FishDBFishTypeKind.LargeFish;
            if (MiscTool.IsRegexString(iChatText, dictionaryChat[ChatKbnKind.BaitItem])) return FishDBFishTypeKind.Item;
            if (MiscTool.IsRegexString(iChatText, dictionaryChat[ChatKbnKind.BaitMonster])) return FishDBFishTypeKind.UnknownMonster;
            return FishDBFishTypeKind.Unknown;
        }
        /// <summary>
        /// FishType（Unknown）をチャットテキストから判別して返す
        /// </summary>
        /// <param name="iChatText">チャットテキスト</param>
        /// <returns>FishType</returns>
        private FishDBFishTypeKind getTmpFishTypeFromChat(string iChatText)
        {
            if (MiscTool.IsRegexString(iChatText, dictionaryChat[ChatKbnKind.BaitSmallFish])) return FishDBFishTypeKind.UnknownSmallFish;
            if (MiscTool.IsRegexString(iChatText, dictionaryChat[ChatKbnKind.BaitLargeFish])) return FishDBFishTypeKind.UnknownLargeFish;
            if (MiscTool.IsRegexString(iChatText, dictionaryChat[ChatKbnKind.BaitItem])) return FishDBFishTypeKind.UnknownItem;
            if (MiscTool.IsRegexString(iChatText, dictionaryChat[ChatKbnKind.BaitMonster])) return FishDBFishTypeKind.UnknownMonster;
            return FishDBFishTypeKind.Unknown;
        }
        /// <summary>
        /// 魚の名称が一時名称かどうか判定
        /// </summary>
        /// <param name="iFishName">魚名称</param>
        /// <returns>True：魚名称が一時名称だった場合</returns>
        private bool isTmpFishFromName(string iFishName)
        {
            if (iFishName.IndexOf(FishDB.FISHNAME_UNKNOWN_FISH) >= 0 ||
                iFishName.IndexOf(FishDB.FISHNAME_UNKNOWN_ITEM) >= 0 ||
                iFishName.IndexOf(FishDB.FISHNAME_UNKNOWN_MONSTER) >= 0 ||
                iFishName.IndexOf(FishDB.FISHNAME_UNKNOWN) >= 0)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 画面表示用の魚名を取得する(魚別で表示)
        /// </summary>
        /// <param name="iFishName">魚名</param>
        /// <param name="iFishType">魚タイプ</param>
        /// <returns></returns>
        public static string GetViewFishName(string iFishName, FishDBFishTypeKind iFishType)
        {
            return GetViewFishName(iFishName, iFishType, 0, false, FishDBItemTypeKind.Unknown);
        }
        /// <summary>
        /// 画面表示用の魚名を取得する
        /// </summary>
        /// <param name="iFishName">魚名</param>
        /// <param name="iFishType">魚タイプ</param>
        /// <param name="iFishCount">数</param>
        /// <param name="iCritical">クリティカル</param>
        /// <param name="iDetail">詳細モード</param>
        /// <returns>表示用の魚名</returns>
        public static string GetViewFishName(string iFishName, FishDBFishTypeKind iFishType, int iFishCount, bool iCritical, FishDBItemTypeKind iItemType)
        {
            string size = string.Empty;
            if (iFishType == FishDBFishTypeKind.SmallFish || iFishType == FishDBFishTypeKind.UnknownSmallFish)
            {
                size = "S";
            }
            else if (iFishType == FishDBFishTypeKind.LargeFish || iFishType == FishDBFishTypeKind.UnknownLargeFish)
            {
                size = "L";
            }
            else if (iFishType == FishDBFishTypeKind.Item || iFishType == FishDBFishTypeKind.UnknownItem)
            {
                size = "I";
            }
            else if (iFishType == FishDBFishTypeKind.UnknownMonster)
            {
                size = "M";
            }
            else
            {
                size = "?";
            }
            string critical = string.Empty;
            if (iCritical)
            {
                critical = "!";
            }
            string count = string.Empty;
            if (iFishCount > 1)
            {
                count = string.Format("x{0}",iFishCount);
            }
            string type = string.Empty;
            if (iItemType == FishDBItemTypeKind.Key)
            {
                type = "K";
            }
            else if (iItemType == FishDBItemTypeKind.Temporary)
            {
                type = "T";
            }
            return string.Format("{0}{1}{2}{3}{4}", iFishName, size, count, type, critical);
        }
        /// <summary>
        /// 時間が範囲内にあるかチェック
        /// </summary>
        /// <param name="iTarget">地球日時</param>
        /// <param name="iFrom">時間From</param>
        /// <param name="iTo">時間To</param>
        /// <returns></returns>
        private bool isHourInRange(DateTime iTarget, int iFrom, int iTo)
        {
            if (iFrom <= iTo)
            {
                if (iTarget.Hour >= iFrom && iTarget.Hour <= iTo)
                {
                    return true;
                }
            }
            else if (iFrom > iTo)
            {
                if ((iTarget.Hour >= iFrom && iTarget.Hour <= 23 ) ||
                    (iTarget.Hour >= 0 && iTarget.Hour <= iTo ) )
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 時間が範囲内にあるかチェック
        /// </summary>
        /// <param name="iTarget">ヴァナ日時</param>
        /// <param name="iFrom">時間From</param>
        /// <param name="iTo">時間To</param>
        /// <returns></returns>
        private bool isHourInRange(VanaTime iTarget, int iFrom, int iTo)
        {
            if (iFrom <= iTo)
            {
                if (iTarget.Hour >= iFrom && iTarget.Hour <= iTo)
                {
                    return true;
                }
            }
            else if (iFrom > iTo)
            {
                if ((iTarget.Hour >= iFrom && iTarget.Hour <= 23) ||
                    (iTarget.Hour >= 0 && iTarget.Hour <= iTo))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 範囲内のランダムな時間停止する
        /// </summary>
        /// <param name="iFrom">範囲From(ms)</param>
        /// <param name="iTo">範囲To(ms)</param>
        /// <param name="iMessage">メッセージ</param>
        private void wait(double iFrom, double iTo, string iMessage = "")
        {
            int waitSec = MiscTool.GetRandomNumber((int)(iFrom * 1000), (int)(iTo * 1000));
            int waitCnt = waitSec / 100;
            if (iMessage != string.Empty)
            {
                setMessage(string.Format(iMessage, (double)waitSec / 1000d));
            }
            for (int i = waitCnt; i >= 0 && RunningStatus == RunningStatusKind.Running; i--)
            {
                Thread.Sleep(100);
            }
        }
        /// <summary>
        /// プレイヤーステータスが指定したステータスになるまで待つ
        /// </summary>
        /// <param name="iPlayerStatus">待機を解除するプレイヤーステータス</param>
        private void waitChangePlayerStatus(Status iPlayerStatus)
        {
            while (this.PlayerStatus != iPlayerStatus)
            {
                Thread.Sleep(settings.Global.WaitBase);
            }
        }
        /// <summary>
        /// スニークを詠唱
        /// </summary>        
        public void castSneak()
        {
            //スレッド開始
            thSneak = new Thread(threadSneak);
            thSneak.Start();
            thSneak.Join();
            thSneak = null;
        }
        /// <summary>
        /// スニーク詠唱スレッド
        /// </summary>
        public void threadSneak()
        {
            //MPチェック
            if (api.Player.MP < 12) return;
            //魔法詠唱可能かチェック
            if (!api.Player.HasSpell(137)) return;
            //リキャストタイムまで待つ
            for (int i = 0; i < Constants.MAX_LOOP_COUNT && api.Recast.GetSpellRecast(137) > 0; i++) Thread.Sleep(100);
            //スニーク詠唱
            api.ThirdParty.SendString("/ma スニーク <me>");
            //詠唱開始まで待つ
            for (int i = 0; i < Constants.MAX_LOOP_COUNT && api.CastBar.Percent == 1.0f; i++) Thread.Sleep(100);
            //BUFFが残っている場合は、詠唱完了１秒前に切る
            if (control.IsBuff(EliteMMO.API.StatusEffect.Sneak))
            {
                for (int i = 0; i < Constants.MAX_LOOP_COUNT; i++)
                {
                    float remain = (api.CastBar.Max - (api.CastBar.Max - api.CastBar.Count)) / 100;
                    if (remain < settings.Fishing.SneakFishingRemain) break;
                    Thread.Sleep(100);
                }
                api.ThirdParty.SendString(string.Format("//lua c cancel {0}", StatusEffect.Sneak.ToString("D")));
            }
            //詠唱終了まで待つ
            for (int i = 0; i < Constants.MAX_LOOP_COUNT && api.CastBar.Percent < 1.0f; i++) Thread.Sleep(100);
        }
        /// <summary>
        /// 経過時間メインスレッド
        /// </summary>
        private void threadthTimeElapsed()
        {
            while (this.RunningStatus == RunningStatusKind.Running)
            {
                Thread.Sleep(1000);
                this.TimeElapsed++;
            }
        }
        /// <summary>
        /// 竿を修理する
        /// </summary>
        /// <param name="iRodName">修理する竿名称</param>
        /// <returns>成功した場合Trueを返す</returns>
        private bool repairRod(string iRodName)
        {
            //折れた竿名と修理に使用するクリスタルを取得
            List<RodDBRodModel> rods = FishDB.SelectRod(iRodName);
            if (rods.Count != 1) return false;
            string breakRodName = rods[0].BreakRodName;
            uint breakRodID = resource.GetItem(breakRodName).ItemID;
            string repairCrystal = rods[0].RepairCrystal;

            for (int i = 0; i < Constants.MAX_LOOP_COUNT; i++)
            {
                //鞄に竿が入っていない場合、鞄に移動する
                bool wardrobe = false;
                if (!control.IsExistItem(breakRodName, InventoryType.Inventory))
                {
                    wardrobe = true;
                    //ワードローブに竿があるか確認
                    if (control.GetInventoryItemCount(breakRodID,InventoryType.Wardrobe) == 0)
                    {
                        setMessage(string.Format("竿の修理：{0}が見つからなかったので停止", breakRodName));
                        return false;
                    }
                    //鞄がいっぱいの場合空きを作る
                    if (this.InventoryCount >= this.InventoryMax)
                    {
                        if (!putFish())
                        {
                            setMessage("竿の修理：鞄がいっぱいで竿の修理ができなかったので停止");
                            return false;
                        }
                    }
                    //竿を鞄に移動
                    if (!control.GetItemizer(breakRodName, InventoryType.Wardrobe))
                    {
                        setMessage(string.Format("竿の修理：{0}が鞄に移動できなかったので停止", breakRodName));
                        return false;
                    }
                    setMessage(string.Format("{0}を{1}から取り出しました", breakRodName, InventoryType.Wardrobe.ToString()));
                    Thread.Sleep(1000);
                }
                //鞄にクリスタルが入っていない場合、鞄に移動する
                if (!control.IsExistItem(repairCrystal, InventoryType.Inventory))
                {
                    //クリスタルが存在する？
                    if (control.GetInventoryTypeFromItemName(repairCrystal) == null)
                    {
                        setMessage(string.Format("竿の修理：{0}が見つからなかったので停止", repairCrystal));
                        return false;
                    }
                    //鞄がいっぱいの場合空きを作る
                    if (this.InventoryCount >= this.InventoryMax)
                    {
                        if (!putFish())
                        {
                            setMessage("竿の修理：鞄がいっぱいで竿の修理ができなかったので停止");
                            return false;
                        }
                    }
                    //クリスタルを鞄に移動
                    if (!getItem(repairCrystal))
                    {
                        setMessage(string.Format("竿の修理：{0}が鞄に移動できなかったので停止", repairCrystal));
                        return false;
                    }
                }
                //入港メッセージチェック
                if (interrupt.ShipWarning)
                {
                    setMessage("竿の修理：入港するので竿の修理を中止");
                    return false;
                }
                //合成
                setMessage(string.Format("竿の修理：{0}を修理します", breakRodName));
                bool synthSuccess = RepairRodSynthesis(repairCrystal, breakRodName);
                Thread.Sleep(1000);
                //ワードローブに竿を戻す
                if (synthSuccess && wardrobe)
                {
                    if (!putItem(iRodName, InventoryType.Wardrobe))
                    {
                        setMessage(string.Format("竿の修理：{0}が{1}に移動できなかったので停止", breakRodName, InventoryType.Wardrobe.ToString()));
                        return false;
                    }
                    Thread.Sleep(500);
                    setMessage(string.Format("竿の修理：{0}をワードローブに移動しました", iRodName));
                }
                if (synthSuccess) return true;
            }
            setMessage("竿の修理：竿の修理が出来なかったので停止");
            return false;
        }
        /// <summary>
        /// 竿を修理する(合成)
        /// </summary>
        /// <param name="iCrystalName">使用クリスタル</param>
        /// <param name="iBreakRod">修理する竿名</param>
        /// <returns>成功した場合Trueを返す</returns>
        private bool RepairRodSynthesis(string iCrystalName, string iBreakRodName)
        {
            //入力チェック
            if (!control.IsExistItem(iCrystalName, InventoryType.Inventory) ||
                !control.IsExistItem(iBreakRodName, InventoryType.Inventory)) return false;
            //メニュー閉じる
            bool maxLoop = true;
            for (int i = 0; i < Constants.MAX_LOOP_COUNT ; i++)
            {
                if (!api.Menu.IsMenuOpen)
                {
                    maxLoop = false;
                    break;
                }
                api.ThirdParty.KeyPress(Keys.ESCAPE);
                Thread.Sleep(100);
            }
            if (maxLoop) return false;
            //アイテムメニュー表示
            maxLoop = true;
            for (int i = 0; i < Constants.MAX_LOOP_COUNT; i++)
            {
                if (control.GetMenuHelpName() == "アイテム")
                {
                    maxLoop = false;
                    break;
                }
                api.ThirdParty.KeyDown(Keys.LCONTROL);
                api.ThirdParty.KeyDown(Keys.I);
                api.ThirdParty.KeyUp(Keys.LCONTROL);
                api.ThirdParty.KeyUp(Keys.I);
                Thread.Sleep(100);
            }
            if (maxLoop) return false;
            //クリスタルの選択
            maxLoop = true;
            bool firstTime = true;
            for (int i = 0; i < 200; i++)
            {
                if (resource.GetItem(api.Inventory.SelectedItemId).Name[1] == iCrystalName)
                {
                    maxLoop = false;
                    break;
                }
                else if (firstTime)
                {
                    for (int j = 0; j < 15; j++)
                    {
                        api.ThirdParty.KeyPress(Keys.RIGHT);
                        Thread.Sleep(100);
                    }
                    firstTime = false;
                }
                else
                {
                    api.ThirdParty.KeyPress(Keys.UP);
                    Thread.Sleep(100);
                }
            }
            if (maxLoop) return false;
            //クリスタルでリターン押下
            maxLoop = true;
            for (int i = 0; i < Constants.MAX_LOOP_COUNT; i++)
            {
                if (control.GetMenuHelpName() == "アイテム合成")
                {
                    maxLoop = false;
                    break;
                }
                api.ThirdParty.KeyPress(Keys.RETURN);
                Thread.Sleep(100);
            }
            if (maxLoop) return false;
            //アイテム情報取得
            uint itemID = resource.GetItem(iBreakRodName).ItemID;
            int itemIndex = control.GetInventoryFirstItemIndex(itemID, InventoryType.Inventory);
            //合成アイテムのセット
            api.CraftMenu.SetCraftItem(0, (ushort)itemID, (byte)itemIndex, 1);
            //中止ボタンへ移動
            maxLoop = true;
            for (int i = 0; i < Constants.MAX_LOOP_COUNT; i++)
            {
                if (control.GetMenuHelpDescription() == "中止します。")
                {
                    maxLoop = false;
                    break;
                }
                api.ThirdParty.KeyPress(Keys.ESCAPE);
                Thread.Sleep(200);
            }
            if (maxLoop) return false;
            //決定ボタンへ移動
            maxLoop = true;
            for (int i = 0; i < Constants.MAX_LOOP_COUNT; i++)
            {
                if (control.GetMenuHelpDescription() == "この組み合わせでアイテムを合成します。")
                {
                    maxLoop = false;
                    break;
                }
                api.ThirdParty.KeyPress(Keys.UP);
                Thread.Sleep(200);
                api.ThirdParty.KeyPress(Keys.RIGHT);
                Thread.Sleep(200);
            }
            if (maxLoop) return false;
            api.ThirdParty.KeyPress(Keys.RETURN);
            //合成終了するまで待機
            for (int i = 0; i < Constants.MAX_LOOP_COUNT; i++)
            {
                if (this.PlayerStatus == Status.Synthing) break;
                Thread.Sleep(settings.Global.WaitBase);
            }
            for (int i = 0; i < Constants.MAX_LOOP_COUNT; i++)
            {
                if (this.PlayerStatus != Status.Synthing) break;
                Thread.Sleep(settings.Global.WaitBase);
            }
            Thread.Sleep(settings.Global.WaitChat);
            //合成ログの確認
            logger.Debug("竿の修理：ログ確認");
            var cl = new EliteAPI.ChatEntry();
            for (int i = 0; i < Constants.MAX_LOOP_COUNT; i++)
            {
                if(!chat.GetNextChatLine(out cl)) break;
                //チャット区分の取得
                List<string> chatKbnArgs = new List<string>();
                ChatKbnKind chatKbn = getChatKbnFromChatline(cl, out chatKbnArgs);
                logger.Debug("Chat:{0} ChatKbn:{1}", cl.Text, chatKbn);
                //合成失敗
                if (chatKbn == ChatKbnKind.SynthFailure ||
                    chatKbn == ChatKbnKind.SynthNotEnoughSkill)
                {
                    logger.Debug("竿の修理：合成失敗");
                    return false;
                }
                //合成成功
                if (chatKbn == ChatKbnKind.SynthSuccess)
                {
                    logger.Debug("竿の修理：合成成功");
                    return true;
                }
                Thread.Sleep(settings.Global.WaitChat);
            }
            logger.Debug("竿の修理：合成失敗 タイムアウト");
            return false;
        }
        /// <summary>
        /// エンチャントアイテムの使用
        /// </summary>
        /// <returns></returns>
        public bool useEnchantedItem()
        {
            if (!settings.Fishing.EquipEnable) return true;
            //腰
            if (settings.Fishing.UseWaist && !string.IsNullOrEmpty(settings.Fishing.EquipWaist))
            {
                List<GearDBGearModel> item = FishDB.SelectGear(settings.Fishing.EquipWaist, GearDBPositionKind.Rings);
                if (item.Count > 0 && item[0].BuffID > 0)
                {
                    if (!control.IsBuff((EliteMMO.API.StatusEffect)item[0].BuffID))
                    {
                        setMessage(string.Format("強化切れ：{0}を使用", item[0].GearName));
                        useItem(item[0].GearName);
                    }
                }
            }
            //左指
            if (settings.Fishing.UseRingLeft && !string.IsNullOrEmpty(settings.Fishing.EquipRingLeft))
            {
                List<GearDBGearModel> item = FishDB.SelectGear(settings.Fishing.EquipRingLeft, GearDBPositionKind.Rings);
                if (item.Count > 0 && item[0].BuffID > 0)
                {
                    if (!control.IsBuff((EliteMMO.API.StatusEffect)item[0].BuffID))
                    {
                        setMessage(string.Format("強化切れ：{0}を使用", item[0].GearName));
                        useItem(item[0].GearName);
                    }
                }
            }
            //右指
            if (settings.Fishing.UseRingRight && !string.IsNullOrEmpty(settings.Fishing.EquipRingRight))
            {
                List<GearDBGearModel> item = FishDB.SelectGear(settings.Fishing.EquipRingRight, GearDBPositionKind.Rings);
                if (item.Count > 0 && item[0].BuffID > 0)
                {
                    if (!control.IsBuff((EliteMMO.API.StatusEffect)item[0].BuffID))
                    {
                        setMessage(string.Format("強化切れ：{0}を使用", item[0].GearName));
                        useItem(item[0].GearName);
                    }
                }
            }
            return true;
        }
        /// <summary>
        /// 食べ物を食べる
        /// </summary>
        /// <returns></returns>
        public bool useFood()
        {
            if (!settings.Fishing.UseFood) return true;
            //食べ物
            if (settings.Fishing.UseFood && !string.IsNullOrEmpty(settings.Fishing.Food))
            {
                List<GearDBGearModel> item = FishDB.SelectGear(settings.Fishing.Food, GearDBPositionKind.Foods);
                if (item.Count > 0 && item[0].BuffID > 0)
                {
                    if (!control.IsBuff((EliteMMO.API.StatusEffect)item[0].BuffID))
                    {
                        setMessage(string.Format("食事切れ：{0}を使用", item[0].GearName));
                        return useItem(item[0].GearName);
                    }
                }
            }
            return true;
        }
        /// <summary>
        /// アイテムの使用
        /// </summary>
        /// <param name="iItemname">アイテム名</param>
        /// <returns></returns>
        public bool useItem(string iItemname)
        {
            api.ThirdParty.SendString(string.Format("/item {0} <me>",iItemname));
            var cl = new EliteAPI.ChatEntry();
            for (int i = 0; i < Constants.MAX_LOOP_COUNT; i++)
            {
                chat.GetNextChatLine(out cl);
                //チャット区分の取得
                List<string> chatKbnArgs = new List<string>();
                ChatKbnKind chatKbn = getChatKbnFromChatline(cl, out chatKbnArgs);
                logger.Debug("Chat:{0} ChatKbn:{1}", cl.Text, chatKbn);
                //失敗
                if (chatKbn == ChatKbnKind.UseItemFailure)
                {
                    return false;
                }
                //成功
                if (chatKbn == ChatKbnKind.UseItemSuccess1 ||
                    chatKbn == ChatKbnKind.UseItemSuccess2)
                {
                    Thread.Sleep(5000);
                    return true;
                }
                Thread.Sleep(settings.Global.WaitBase);
            }
            return false;
        }
        #endregion

        #region 釣った魚
        public List<SettingsPlayerCaughtFishModel> GetCaughtFishes()
        {
            List<SettingsPlayerCaughtFishModel> ret = new List<SettingsPlayerCaughtFishModel>();

            setMessage("釣った魚の初期化中");
            setFishingStatus(FishingStatusKind.Normal);
            setRunningStatus(RunningStatusKind.Running);

            //Katsunagaの近くかチェック
            if (api.Player.ZoneId != (int)Zone.Mhaura ||
                (api.Entity.GetEntity(NPCINDEX_KATSUNAGA).Distance != 0f && api.Entity.GetEntity(NPCINDEX_KATSUNAGA).Distance > 6))
            {
                setMessage("マウラのKatsunagaの近くで実行してください");
                setFishingStatus(FishingStatusKind.Error);
                setRunningStatus(RunningStatusKind.Stop);
                return ret;
            }
            //メニュー開いていたら閉じる
            if (!control.CloseDialog(10))
            {
                setMessage("エラー：会話を終了させてから実行してください");
                setFishingStatus(FishingStatusKind.Error);
                setRunningStatus(RunningStatusKind.Stop);
                return ret;
            }
            //メニューを開く
            while (!api.Menu.IsMenuOpen)
            {
                //ターゲット設定
                control.SetTargetFromId(NPCINDEX_KATSUNAGA);
                Thread.Sleep(settings.Global.WaitBase);//Wait
                api.ThirdParty.KeyPress(Keys.RETURN);
            }
            control.WaitOpenDialog("何を教えてもらおう？", false);
            control.SetDialogOptionIndex(0, true);
            control.WaitOpenDialog(REGEX_FISHEDLIST_DIALOG, false);
            if (api.Menu.IsMenuOpen &&
                MiscTool.IsRegexString(api.Dialog.GetDialog().Question, REGEX_FISHEDLIST_DIALOG))
            {
                int pageCurrent = 0;
                int pageMax = 99;
                for (int i = 0; i < Constants.MAX_LOOP_COUNT; i++)
                {
                    control.WaitOpenDialog(REGEX_FISHEDLIST_DIALOG, false);
                    List<string> oArgs = MiscTool.GetRegexString(api.Dialog.GetDialog().Question, REGEX_FISHEDLIST_DIALOG);
                    pageCurrent = int.Parse(oArgs[1]);
                    pageMax = int.Parse(oArgs[2]);
                    string[] options = api.Dialog.GetDialog().Options.ToArray();
                    for (int j = 1; j < 17 - (19 - api.Dialog.DialogOptionCount); j++)
                    {
                        if (MiscTool.IsRegexString(options[j], REGEX_FISHEDLIST_OPTIONS))
                        {
                            List<string> oArgs2 = MiscTool.GetRegexString(options[j], REGEX_FISHEDLIST_OPTIONS);
                            SettingsPlayerCaughtFishModel fished = new SettingsPlayerCaughtFishModel();
                            fished.Caught = (oArgs2[0] == "★");
                            fished.FishName = oArgs2[1];
                            ret.Add(fished);
                        }
                    }
                    if (pageCurrent == pageMax) break;
                    //改ページ
                    control.SetDialogOptionIndex(18, true);
                    Thread.Sleep(settings.Global.WaitBase);
                }
            }
            //メニュー閉じる
            control.CloseDialog(10);

            setFishingStatus(FishingStatusKind.Normal);
            setRunningStatus(RunningStatusKind.Stop);
            return ret;
        }
        #endregion

        #region 装備アイテム
        /// <summary>
        /// 竿を装備する
        /// </summary>
        /// <param name="iRodName">竿名称</param>
        /// <returns>True:成功</returns>
        private bool setRod(string iRodName)
        {
            //引数チェック
            if (iRodName.Length == 0) return false;
            if (!isRod(iRodName)) return false;
            setMessage(string.Format("{0}を装備", iRodName));
            //鞄にアイテムが存在するかチェック
            uint rodId = resource.GetItem(iRodName).ItemID;
            int rodCnt = control.GetInventoryItemCount(rodId, InventoryType.Inventory) +
                         control.GetInventoryItemCount(rodId, InventoryType.Wardrobe);
            //アイテムの装備
            if (rodCnt > 0)
            {
                for (int i = 0; i < Constants.MAX_LOOP_COUNT; i++)
                {
                    if (this.RodName == iRodName)
                    {
                        //イベント発生
                        EventFished(FishResultStatusKind.Unknown);
                        return true;
                    }
                    api.ThirdParty.SendString(string.Format("/equip Range {0}", iRodName));
                    Thread.Sleep(settings.Global.WaitEquip);
                }
            }
            return false;
        }
        /// <summary>
        /// エサを装備する
        /// </summary>
        /// <param name="iBaitName">エサ名称</param>
        /// <returns>True:成功</returns>
        private bool setBait(string iBaitName)
        {
            //引数チェック
            if (iBaitName.Length == 0) return false;
            if (!isBait(iBaitName)) return false;
            setMessage(string.Format("{0}を装備", iBaitName));
            //鞄にアイテムが存在するかチェック
            uint baitId = resource.GetItem(iBaitName).ItemID;
            int baitCnt = control.GetInventoryItemCount(baitId, InventoryType.Inventory) +
                          control.GetInventoryItemCount(baitId, InventoryType.Wardrobe);
            //アイテムの装備
            if (baitCnt > 0)
            {
                for (int i = 0; i < Constants.MAX_LOOP_COUNT; i++)
                {
                    if (this.BaitName == iBaitName)
                    {
                        //イベント発生
                        EventFished(FishResultStatusKind.Unknown);
                        return true;
                    }
                    api.ThirdParty.SendString(string.Format("/equip Ammo {0}", iBaitName));
                    Thread.Sleep(settings.Global.WaitEquip);
                }
            }
            //装備のチェック
            return false;
        }
        /// <summary>
        /// 指定されたアイテム名が釣り竿かどうか判定
        /// </summary>
        /// <param name="iRodName">釣り竿名</param>
        /// <returns>True:釣り竿だった場合</returns>
        private bool isRod(string iRodName)
        {
            if (iRodName.Length == 0) return false;
            return FishDB.Rods.Contains(iRodName);
        }
        /// <summary>
        /// 指定されたアイテム名がエサかどうか判定
        /// </summary>
        /// <param name="iBaitName">エサ名</param>
        /// <returns>True:エサだった場合</returns>
        private bool isBait(string iBaitName)
        {
            if (iBaitName.Length == 0) return false;
            return FishDB.Baits.Contains(iBaitName);
        }
        /// <summary>
        /// アイテムを鞄へ移動する
        /// </summary>
        /// <param name="iItemName"></param>
        /// <returns></returns>
        private bool getItem(string iItemName)
        {
            bool moveOkFlg = false;
            if (settings.UseItemizer)
            {
                if (!moveOkFlg && control.GetItemizer(iItemName, InventoryType.Satchel))
                {
                    moveOkFlg = true;
                    setMessage(string.Format("{0}を{1}から取り出しました", iItemName, InventoryType.Satchel.ToString()));
                    Thread.Sleep(1000);
                }
                if (!moveOkFlg && control.GetItemizer(iItemName, InventoryType.Sack))
                {
                    moveOkFlg = true;
                    setMessage(string.Format("{0}を{1}から取り出しました", iItemName, InventoryType.Sack.ToString()));
                    Thread.Sleep(1000);
                }
                if (!moveOkFlg && control.GetItemizer(iItemName, InventoryType.Case))
                {
                    moveOkFlg = true;
                    setMessage(string.Format("{0}を{1}から取り出しました", iItemName, InventoryType.Case.ToString()));
                    Thread.Sleep(1000);
                }
                if (!moveOkFlg && control.GetItemizer(iItemName, InventoryType.Wardrobe))
                {
                    moveOkFlg = true;
                    setMessage(string.Format("{0}を{1}から取り出しました", iItemName, InventoryType.Wardrobe.ToString()));
                    Thread.Sleep(1000);
                }
            }
            return moveOkFlg;
        }
        /// <summary>
        /// 竿・エサを鞄へ移動する
        /// </summary>
        /// <param name="iItemName"></param>
        /// <returns></returns>
        private bool getRodBaitItem(string iItemName)
        {
            bool moveOkFlg = false;
            if (settings.UseItemizer)
            {
                if (!moveOkFlg && settings.Fishing.NoBaitNoRodSatchel)
                    if (control.GetItemizer(iItemName, InventoryType.Satchel))
                    {
                        moveOkFlg = true;
                        setMessage(string.Format("{0}を{1}から取り出しました", iItemName, InventoryType.Satchel.ToString()));
                        Thread.Sleep(1000);
                    }
                if (!moveOkFlg && settings.Fishing.NoBaitNoRodSack)
                    if (control.GetItemizer(iItemName, InventoryType.Sack))
                    {
                        moveOkFlg = true;
                        setMessage(string.Format("{0}を{1}から取り出しました", iItemName, InventoryType.Sack.ToString()));
                        Thread.Sleep(1000);
                    }
                if (!moveOkFlg && settings.Fishing.NoBaitNoRodCase)
                    if (control.GetItemizer(iItemName, InventoryType.Case))
                    {
                        moveOkFlg = true;
                        setMessage(string.Format("{0}を{1}から取り出しました", iItemName, InventoryType.Case.ToString()));
                        Thread.Sleep(1000);
                    }
            }
            return moveOkFlg;
        }
        /// <summary>
        /// アイテムを鞄から指定した場所へ移動する
        /// </summary>
        /// <param name="iItemName">アイテム名</param>
        /// <param name="iInventoryType">移動先</param>
        /// <returns>成功した場合Truwを返す</returns>
        private bool putItem(string iItemName, InventoryType iInventoryType)
        {
            if (control.GetInventoryCountByType(iInventoryType) >= control.GetInventoryMaxByType(iInventoryType)) return false;
            if (control.IsExistItem(iItemName, InventoryType.Inventory))
            {
                control.PutItemizer(iItemName, iInventoryType);
                setMessage(string.Format("{0}を{1}に移動しました", iItemName, iInventoryType.ToString()));
                Thread.Sleep(1000);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 魚を鞄から移動する
        /// </summary>
        /// <returns></returns>
        private bool putFish()
        {
            bool moveOkFlg = false;
            if (settings.UseItemizer)
            {
                if (!moveOkFlg && settings.Fishing.InventoryFullSatchel) moveOkFlg = putFish(InventoryType.Satchel);
                if (!moveOkFlg && settings.Fishing.InventoryFullSack) moveOkFlg = putFish(InventoryType.Sack);
                if (!moveOkFlg && settings.Fishing.InventoryFullCase) moveOkFlg = putFish(InventoryType.Case);
            }
            return moveOkFlg;
        }
        /// <summary>
        /// 指定した場所へ魚を移動する
        /// </summary>
        /// <param name="iInventoryType"></param>
        /// <returns></returns>
        private bool putFish(InventoryType iInventoryType)
        {
            //short lastCnt = control.GetInventoryCountByType(InventoryType.Inventory);
            if (control.GetInventoryCountByType(iInventoryType) >= control.GetInventoryMaxByType(iInventoryType)) return false;
            List<FishDBFishModel> fishes = FishDB.SelectFishList(this.RodName, string.Empty, string.Empty);
            foreach (FishDBFishModel fish in fishes)
            {
                if (control.IsExistItem(fish.FishName, InventoryType.Inventory))
                {
                    control.PutItemizer(fish.FishName, iInventoryType);
                    setMessage(string.Format("{0}を{1}に移動しました", fish.FishName, iInventoryType.ToString()));
                    Thread.Sleep(1000);
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 装備着替え
        /// </summary>
        /// <returns></returns>
        private bool setEquipGear()
        {
            if (settings.Fishing.EquipEnable)
            {
                logger.Debug("装備着替え開始");
                if (!string.IsNullOrEmpty(settings.Fishing.EquipRod))
                    api.ThirdParty.SendString(string.Format("/equip range {0}", settings.Fishing.EquipRod));
                if (!string.IsNullOrEmpty(settings.Fishing.EquipBait))
                    api.ThirdParty.SendString(string.Format("/equip ammo {0}", settings.Fishing.EquipBait));
                if (!string.IsNullOrEmpty(settings.Fishing.EquipMain))
                    api.ThirdParty.SendString(string.Format("/equip main {0}", settings.Fishing.EquipMain));
                if (!string.IsNullOrEmpty(settings.Fishing.EquipSub))
                    api.ThirdParty.SendString(string.Format("/equip sub {0}", settings.Fishing.EquipSub));
                if (!string.IsNullOrEmpty(settings.Fishing.EquipHead))
                    api.ThirdParty.SendString(string.Format("/equip head {0}", settings.Fishing.EquipHead));
                if (!string.IsNullOrEmpty(settings.Fishing.EquipBody))
                    api.ThirdParty.SendString(string.Format("/equip body {0}", settings.Fishing.EquipBody));
                if (!string.IsNullOrEmpty(settings.Fishing.EquipHands))
                    api.ThirdParty.SendString(string.Format("/equip hands {0}", settings.Fishing.EquipHands));
                if (!string.IsNullOrEmpty(settings.Fishing.EquipLegs))
                    api.ThirdParty.SendString(string.Format("/equip legs {0}", settings.Fishing.EquipLegs));
                if (!string.IsNullOrEmpty(settings.Fishing.EquipFeet))
                    api.ThirdParty.SendString(string.Format("/equip feet {0}", settings.Fishing.EquipFeet));
                if (!string.IsNullOrEmpty(settings.Fishing.EquipNeck))
                    api.ThirdParty.SendString(string.Format("/equip neck {0}", settings.Fishing.EquipNeck));
                if (!string.IsNullOrEmpty(settings.Fishing.EquipWaist))
                    api.ThirdParty.SendString(string.Format("/equip waist {0}", settings.Fishing.EquipWaist));
                if (!string.IsNullOrEmpty(settings.Fishing.EquipBack))
                    api.ThirdParty.SendString(string.Format("/equip back {0}", settings.Fishing.EquipBack));
                if (!string.IsNullOrEmpty(settings.Fishing.EquipEarLeft))
                    api.ThirdParty.SendString(string.Format("/equip ear1 {0}", settings.Fishing.EquipEarLeft));
                if (settings.Fishing.EquipEarLeft == settings.Fishing.EquipEarRight) 
                    Thread.Sleep(settings.Global.WaitBase);//wait
                if (!string.IsNullOrEmpty(settings.Fishing.EquipEarRight))
                    api.ThirdParty.SendString(string.Format("/equip ear2 {0}", settings.Fishing.EquipEarRight));
                if (!string.IsNullOrEmpty(settings.Fishing.EquipRingLeft))
                    api.ThirdParty.SendString(string.Format("/equip ring1 {0}", settings.Fishing.EquipRingLeft));
                if (settings.Fishing.EquipRingLeft == settings.Fishing.EquipRingRight) 
                    Thread.Sleep(settings.Global.WaitBase);//wait
                if (!string.IsNullOrEmpty(settings.Fishing.EquipRingRight))
                    api.ThirdParty.SendString(string.Format("/equip ring2 {0}", settings.Fishing.EquipRingRight));
                Thread.Sleep(500);
                //イベント発生
                EventFished(FishResultStatusKind.Unknown);
            }
            return true;
        }
        #endregion

        #region ステータス・メッセージ 
        private void setRunningStatus(RunningStatusKind iRunningStatus)
        {
            if (this.RunningStatus == iRunningStatus) return;
            this.RunningStatus = iRunningStatus;
            EventChangeStatus(iRunningStatus, this.FishingStatus);
        }
        private void setFishingStatus(FishingStatusKind iFishingStatus)
        {
            if (this.FishingStatus == iFishingStatus) return;
            this.FishingStatus = iFishingStatus;
            EventChangeStatus(this.RunningStatus, iFishingStatus);
        }
        private void setMessage(string iMessage)
        {
            if (this.Message == iMessage) return;
            logger.Info(iMessage);
            this.Message = iMessage;
            EventChangeMessage(iMessage);
        }
        #endregion

    }
}
