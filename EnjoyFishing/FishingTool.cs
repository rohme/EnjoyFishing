using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFACETools;
using MiscTools;
using System.Threading;
using System.IO;

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
            {ChatKbnKind.LineBreak, "釣り糸が切れてしまった。"},
            {ChatKbnKind.RodBreak, "釣り竿が折れてしまった。"},
            {ChatKbnKind.InventoryFull, "見事に(.*)を釣り上げたが、"},
            {ChatKbnKind.NoBait, "何も釣れなかった。"},
            {ChatKbnKind.Release, "あきらめて仕掛けをたぐり寄せた。"},
            {ChatKbnKind.NoCatch, "獲物に逃げられてしまった。"},
            {ChatKbnKind.EnemyAttack1, "(.*)の攻撃→{0}に、(.*)"},
            {ChatKbnKind.EnemyAttack2, "→{0}に、([0-9]*)ダメージ。"},
            {ChatKbnKind.SneakWarning1, "スニークの効果がきれそうだ。"},
            {ChatKbnKind.SneakWarning2, "{0}は、スニークの効果がきれた。"},
            {ChatKbnKind.ShipWarning1, "まもなく(.*)へ到着します。"},//汽船航路・外洋航路・銀海航路
            {ChatKbnKind.ShipWarning2, "(.*)に入港いたします。"},//汽船航路・外洋航路・銀海航路
            {ChatKbnKind.ShipWarning3, "もうすぐ夕照桟橋に着いちまうぜ。"},//マナクリッパー マリヤカレヤリーフ遊覧
            {ChatKbnKind.ShipWarning4, "Khots Chalahko : そろそろ到着だ。"},//マナクリッパー 夕照桟橋→プルゴノルゴ島
            {ChatKbnKind.ShipWarning5, "ブブリム半島が見えてきたぜ！！"},//マナクリッパー プルゴノルゴ島→夕照桟橋
            {ChatKbnKind.ShipWarning6, "Ineuteniace : そろそろ北桟橋ですな。"},//バージ 主水路(南桟橋→北桟橋)
            {ChatKbnKind.ShipWarning7, "Eunirange : そろそろ中桟橋かな？"},//バージ 主水路(北桟橋→中桟橋)
            {ChatKbnKind.ShipWarning8, "Ineuteniace : そろそろ南桟橋じゃな。"},//バージ 井守ヶ淵(中桟橋→南桟橋)
            {ChatKbnKind.ShipWarning9, "Eunirange : そろそろ中桟橋かな。"},//バージ エメフィ支水路(南桟橋→中桟橋)
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
            LineBreak,
            RodBreak,
            InventoryFull,
            NoBait,
            Release,
            NoCatch,
            Unknown,
            EnemyAttack1,
            EnemyAttack2,
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
        
        private PolTool pol;
        private FFACE fface;
        private Settings settings;
        private LoggerTool logger;
        private ChatTool chat;
        private FFACEControl control;
        private Thread thFishing;
        private Thread thSneak;
        private FishDB fishDB;
        private FishHistoryDB fishHistoryDB;
        private int remainTimeMAX = 0;
        private string lastRodName = string.Empty;
        private string lastBaitName = string.Empty;
        private string lastZoneName = string.Empty;
        private int noCatchCount = 0;

        #region メンバ
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
        public MoonPhase MoonPhase{ get { return fface.Timer.GetVanaTime().MoonPhase; } }
        /// <summary>
        /// 月齢（％）
        /// </summary>
        public int MoonPercent { get { return (int)fface.Timer.GetVanaTime().MoonPercent; } }
        /// <summary>
        /// ヴァナ時間
        /// </summary>
        public FFACETools.FFACE.TimerTools.VanaTime VanaDateTime
        {
            get
            {
                return fface.Timer.GetVanaTime();
            }
        }
        /// <summary>
        /// ヴァナ時間
        /// </summary>
        public String VanaDateTimeYmdhm
        {
            get
            {
                FFACE.TimerTools.VanaTime vt = fface.Timer.GetVanaTime();
                return string.Format("{0:0000}/{1:00}/{2:00} {3:00}:{4:00}", int.Parse(vt.Year.ToString()),
                                                                             int.Parse(vt.Month.ToString()),
                                                                             int.Parse(vt.Day.ToString()),
                                                                             int.Parse(vt.Hour.ToString()),
                                                                             int.Parse(vt.Minute.ToString()));
            }
        }
        /// <summary>
        /// 地球時間
        /// </summary>
        public DateTime EarthDateTime{ get { return DateTime.Now; } }
        /// <summary>
        /// ヴァナ曜日
        /// </summary>
        public Weekday DayType{ get { return fface.Timer.GetVanaTime().DayType; } }
        /// <summary>
        /// プレイヤーステータス
        /// </summary>
        public FFACETools.Status PlayerStatus { get { return fface.Player.Status; } }
        public string PlayerName { get { return fface.Player.Name; } }
        /// <summary>
        /// ログインステータス
        /// </summary>
        public LoginStatus LoginStatus { get { return fface.Player.GetLoginStatus; } }
        /// <summary>
        /// 魚HP（最大）
        /// </summary>
        public int HpMax
        {
            get
            {
                if (fface.Player.Status == FFACETools.Status.FishBite)
                {
                    return fface.Fish.HPMax;
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
                if (fface.Player.Status == FFACETools.Status.FishBite)
                {
                    return fface.Fish.HPCurrent;
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
                if (fface.Player.Status == FFACETools.Status.FishBite)
                {
                    double per = 0.00;
                    if (fface.Fish.HPMax > 0) per = (double)fface.Fish.HPCurrent / (double)fface.Fish.HPMax;
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
                if (fface.Player.Status == FFACETools.Status.FishBite)
                {
                    if (this.remainTimeMAX < fface.Fish.Timeout) this.remainTimeMAX = fface.Fish.Timeout;
                    return this.remainTimeMAX;
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
                if (fface.Player.Status == FFACETools.Status.FishBite)
                {
                    return fface.Fish.Timeout;
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
                if (fface.Player.Status == FFACETools.Status.FishBite)
                {
                    if (this.remainTimeMAX < fface.Fish.Timeout) this.remainTimeMAX = fface.Fish.Timeout;
                    double per = 0.00;
                    if (this.remainTimeMAX > 0) per = (double)fface.Fish.Timeout / (double)this.remainTimeMAX;
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
        public short InventoryCount
        {
            get { return control.GetInventoryCountByType(InventoryType.Inventory); }
        }
        /// <summary>
        /// 鞄最大所持数
        /// </summary>
        public short InventoryMax
        {
            get { return control.GetInventoryMaxByType(InventoryType.Inventory); }
        }
        /// <summary>
        /// モグサッチェル所持数
        /// </summary>
        public short SatchelCount
        {
            get { return control.GetInventoryCountByType(InventoryType.Satchel); }
        }
        /// <summary>
        /// モグサッチェル最大所持数
        /// </summary>
        public short SatchelMax
        {
            get { return control.GetInventoryMaxByType(InventoryType.Satchel); }
        }
        /// <summary>
        /// モグサック所持数
        /// </summary>
        public short SackCount
        {
            get { return control.GetInventoryCountByType(InventoryType.Sack); }
        }
        /// <summary>
        /// モグサック最大所持数
        /// </summary>
        public short SackMax
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
        /// 釣りスキル
        /// </summary>
        public int FishingSkill
        {
            get { return fface.Player.GetCraftDetails(Craft.Fishing).Level; }
        }
        /// <summary>
        /// エリア名称
        /// </summary>
        public string ZoneName
        {
            get { return FFACE.ParseResources.GetAreaName(fface.Player.Zone); }
        }
        /// <summary>
        /// 竿名称
        /// </summary>
        public string RodName
        {
            get 
            {
                int rodId = fface.Item.GetEquippedItemID(EquipSlot.Range);
                string rodName = FFACE.ParseResources.GetItemName(rodId);
                if (rodId != 0 && fishDB.Rods.Contains(rodName))
                {
                    return rodName;
                }
                else
                {
                    return string.Empty;
                }
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
                int rodId = FFACE.ParseResources.GetItemId(this.RodName);
                uint remain = fface.Item.GetItemCount(rodId, InventoryType.Inventory);
                if (settings.UseItemizer)
                {
                    if (settings.Fishing.NoBaitNoRodSatchel) remain += fface.Item.GetItemCount(rodId, InventoryType.Satchel);
                    if (settings.Fishing.NoBaitNoRodSack) remain += fface.Item.GetItemCount(rodId, InventoryType.Sack);
                    if (settings.Fishing.NoBaitNoRodCase) remain += fface.Item.GetItemCount(rodId, InventoryType.Case);
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
                int baitId = fface.Item.GetEquippedItemID(EquipSlot.Ammo);
                string baitName = FFACE.ParseResources.GetItemName(baitId);
                if (baitId != 0 && fishDB.Baits.Contains(baitName))
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
                int baitId = FFACE.ParseResources.GetItemId(this.BaitName);
                uint remain = fface.Item.GetItemCount(baitId, InventoryType.Inventory);
                if (settings.UseItemizer)
                {
                    if (settings.Fishing.NoBaitNoRodSatchel) remain += fface.Item.GetItemCount(baitId, InventoryType.Satchel);
                    if (settings.Fishing.NoBaitNoRodSack) remain += fface.Item.GetItemCount(baitId, InventoryType.Sack);
                    if (settings.Fishing.NoBaitNoRodCase) remain += fface.Item.GetItemCount(baitId, InventoryType.Case);
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
        public FFACE.Position Position
        {
            get
            {
                return fface.Player.Position;
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
        #endregion

        #region コンストラクタ
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="iFFACE"></param>
        /// <param name="iChat"></param>
        /// <param name="iSettings"></param>
        /// <param name="iLogger"></param>
        public FishingTool(PolTool iPol, ChatTool iChat, Settings iSettings, LoggerTool iLogger)
        {
            pol = iPol;
            pol.ChangeStatus += new PolTool.ChangeStatusEventHandler(this.PolTool_ChangeStatus);
            fface = iPol.FFACE;
            chat = iChat;
            settings = iSettings;
            logger = iLogger;
            fishDB = new FishDB(logger);
            fishHistoryDB = new FishHistoryDB(logger);
            control = new FFACEControl(pol, chat, logger);
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
            for (int i = 0; i < Constants.MAX_LOOP_COUNT && fface.Player.Status != FFACETools.Status.Standing; i++)
            {
                fface.Windower.SendKeyPress(KeyCode.EscapeKey);
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
            bool chatReceive = false;
            bool enemyAttack = false;
            bool sneakWaening = false;
            bool shipWaening = false;
            noCatchCount = 0;
            lastRodName = string.Empty;
            lastBaitName = string.Empty;
            lastZoneName = string.Empty;
            chat.CurrentIndex = chat.MaxIndex;
            setFishingStatus(FishingStatusKind.Normal);
            FishHistoryDBFishModel fish = new FishHistoryDBFishModel();

            logger.Output(LogLevelKind.DEBUG, "釣りスレッド開始");
            while (this.RunningStatus == RunningStatusKind.Running)
            {
                FishHistoryDBModel history = fishHistoryDB.SelectDayly(this.PlayerName, DateTime.Today);
                //敵からの攻撃感知
                if (this.RunningStatus != RunningStatusKind.Running) break;
                if (enemyAttack)
                {
                    setRunningStatus(RunningStatusKind.Stop);
                    setFishingStatus(FishingStatusKind.Error);
                    setMessage("敵から攻撃されたので停止");
                    break;
                }
                //チャット感知
                if (this.RunningStatus != RunningStatusKind.Running) break;
                if (chatReceive)
                {
                    if (settings.Fishing.ChatRestart)
                    {
                        setFishingStatus(FishingStatusKind.Wait);
                        double waitSec = (double)(settings.Fishing.ChatRestartMinute * 60);
                        DateTime restartTime = DateTime.Now.AddMinutes(settings.Fishing.ChatRestartMinute);
                        setMessage(string.Format("チャット感知：再稼働待ち {0}(地球時間)まで待機", restartTime.ToString("HH:mm:ss")));
                        wait(waitSec, waitSec);
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
                    if (shipWaening)
                    {
                        setRunningStatus(RunningStatusKind.Stop);
                        setFishingStatus(FishingStatusKind.Normal);
                        setMessage("入港するので停止");
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
                    if (history.CatchCount >= settings.Fishing.MaxCatchCount)
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
                    if (sneakWaening || !control.IsBuff(StatusEffect.Sneak))
                    {
                        setMessage("スニークをかけます");
                        castSneak();
                        sneakWaening = false;
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
                            fface.Windower.SendString(settings.Fishing.InventoryFullCmdLine);
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
                        !control.IsExistItem(lastRodName, InventoryType.Inventory))
                    {
                        getItem(lastRodName);
                    }
                    if (!setRod(lastRodName))
                    {
                        setRunningStatus(RunningStatusKind.Stop);
                        setFishingStatus(FishingStatusKind.Error);
                        setMessage("釣り竿を装備していないので停止");
                        //コマンド実行
                        if (settings.Fishing.NoBaitNoRodCmd)
                        {
                            fface.Windower.SendString(settings.Fishing.NoBaitNoRodCmdLine);
                        }
                        break;
                    }
                }
                //エサ
                if (this.RunningStatus != RunningStatusKind.Running) break;
                if (this.BaitName == string.Empty)
                {
                    if (!string.IsNullOrEmpty(lastBaitName) && 
                        !control.IsExistItem(lastBaitName, InventoryType.Inventory))
                    {
                        getItem(lastBaitName);
                    }
                    if (!setBait(lastBaitName))
                    {
                        setRunningStatus(RunningStatusKind.Stop);
                        setFishingStatus(FishingStatusKind.Error);
                        setMessage("エサを装備していないので停止");
                        //コマンド実行
                        if (settings.Fishing.NoBaitNoRodCmd)
                        {
                            fface.Windower.SendString(settings.Fishing.NoBaitNoRodCmdLine);
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
                            fface.Windower.SendString(settings.Fishing.InventoryFullCmdLine);
                        }
                        setRunningStatus(RunningStatusKind.Stop);
                        setFishingStatus(FishingStatusKind.Normal);
                        setMessage("鞄がいっぱいなので停止");
                        break;
                    }
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
                if (!FishingOnce(out fish, out chatReceive, out enemyAttack, out sneakWaening, out shipWaening))
                {
                    //エラー発生時処理
                    setRunningStatus(RunningStatusKind.Stop);
                    setFishingStatus(FishingStatusKind.Error);
                    break;
                }

                firstTime = false;
                Thread.Sleep(settings.Global.WaitBase);
            }
            logger.Output(LogLevelKind.DEBUG, "釣りスレッド終了");
        }
        /// <summary>
        /// 魚を１回釣る
        /// </summary>
        /// <param name="oFish"></param>
        /// <param name="oMessage"></param>
        /// <returns></returns>
        private bool FishingOnce(out FishHistoryDBFishModel oFish, out bool oChatReceive, out bool oEnemyAttack, out bool oSneakWarning, out bool oShipWarning)
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
            oFish.FishType = FishDBFishTypeKind.Unknown;
            oFish.Result = FishResultStatusKind.NoBite;
            oFish.EarthTime = this.EarthDateTime;
            oFish.VanaTime = this.VanaDateTimeYmdhm;
            oFish.VanaWeekDay = this.DayType;
            oFish.MoonPhase = this.MoonPhase;
            oFish.X = this.Position.X;
            oFish.Y = this.Position.Y;
            oFish.Z = this.Position.Z;
            oFish.H = this.Position.H;

            bool fishedFlg = false;
            oChatReceive = false;
            oEnemyAttack = false;
            oSneakWarning = false;
            oShipWarning = false;

            setFishingStatus(FishingStatusKind.Normal);
            setMessage(string.Format("キャスト中：{0}x{1}", this.RodNameWithRemain, this.BaitNameWithRemain));

            //キャスト
            while (this.RunningStatus == RunningStatusKind.Running && fface.Player.Status != FFACETools.Status.Fishing)
            {
                fface.Windower.SendString("/fish");
                Thread.Sleep(settings.Global.WaitChat);

                FFACE.ChatTools.ChatLine cl = new FFACE.ChatTools.ChatLine();
                while (chat.GetNextChatLine(out cl))
                {
                    //チャット区分の取得
                    List<string> chatKbnArgs = new List<string>();
                    ChatKbnKind chatKbn = getChatKbnFromChatline(cl, out chatKbnArgs, ref oChatReceive, ref oEnemyAttack, ref oSneakWarning, ref oShipWarning);
                    logger.Output(LogLevelKind.DEBUG, string.Format("Chat:{0} ChatKbn:{1}", cl.Text, chatKbn));
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
                    else if (oChatReceive) //チャット感知
                    {
                        return true;
                    }
                    else if (oEnemyAttack) //敵の攻撃感知
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
                FFACETools.FFACE.ChatTools.ChatLine cl = new FFACE.ChatTools.ChatLine();
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
                    ChatKbnKind chatKbn = getChatKbnFromChatline(cl, out chatKbnArgs, ref oChatReceive, ref oEnemyAttack, ref oSneakWarning, ref oShipWarning);
                    logger.Output(LogLevelKind.DEBUG, string.Format("Chat:{0} ChatKbn:{1}", cl.Text, chatKbn));
                    
                    if (chatKbn == ChatKbnKind.BaitSmallFish || chatKbn == ChatKbnKind.BaitLargeFish ||
                        chatKbn == ChatKbnKind.BaitItem || chatKbn == ChatKbnKind.BaitMonster)//魚がかかった
                    {
                        //プレイヤステータスがFishBiteになるまで待つ
                        while (this.PlayerStatus != FFACETools.Status.FishBite)
                        {
                            Thread.Sleep(settings.Global.WaitBase);
                        }
                        Thread.Sleep(500);
                        //IDの設定
                        oFish.ID1 = fface.Fish.ID.ID1;
                        oFish.ID2 = fface.Fish.ID.ID2;
                        oFish.ID3 = fface.Fish.ID.ID3;
                        oFish.ID4 = fface.Fish.ID.ID4;
                        //魚名称・タイプの設定
                        List<FishDBFishModel> fishList = fishDB.SelectFishFromID(oFish.RodName, oFish.ID1, oFish.ID2, oFish.ID3, oFish.ID4, false);
                        FishDBFishModel fish = new FishDBFishModel();
                        if (fishList.Count > 0)
                        {
                            fish = fishList[0];
                            oFish.FishName = fish.FishName;
                            oFish.FishType = fish.FishType;
                            oFish.FishCount = fish.GetId(oFish.ID1, oFish.ID2, oFish.ID3, oFish.ID4).Count;
                            oFish.Critical = fish.GetId(oFish.ID1, oFish.ID2, oFish.ID3, oFish.ID4).Critical;
                        }
                        else
                        {
                            oFish.FishType = getTmpFishTypeFromChat(cl.Text);
                            oFish.FishName = getTmpFishNameFromFishType(oFish.FishType, oFish.ID1, oFish.ID2, oFish.ID3, oFish.ID4);
                        }
                        setMessage(string.Format("格闘中：{0}", GetViewFishName(oFish.FishName, oFish.FishType, oFish.FishCount, oFish.Critical)));
                        logger.Output(LogLevelKind.INFO, string.Format("魚ID：{0:000}-{1:000}-{2:000}-{3:000} 魚タイプ：{4}", oFish.ID1, oFish.ID2, oFish.ID3, oFish.ID4, oFish.FishType));
                        //日時の設定
                        oFish.EarthTime = this.EarthDateTime;
                        oFish.VanaTime = this.VanaDateTimeYmdhm;
                        oFish.VanaWeekDay = this.DayType;
                        oFish.MoonPhase = this.MoonPhase;
                        //HP0の設定
                        int waitHP0 = MiscTool.GetRandomNumber(settings.Fishing.HP0Min, settings.Fishing.HP0Max);
                        //反応時間待機
                        Thread.Sleep(settings.Global.WaitChat); //wait
                        if (settings.Fishing.ReactionTime)
                        {
                            wait(settings.Fishing.ReactionTimeMin, settings.Fishing.ReactionTimeMax, "反応待機中：{0:0.0}s " + GetViewFishName(oFish.FishName, oFish.FishType, oFish.FishCount, oFish.Critical));
                        }
                        //リリース判定
                        if (!isWantedFish(oFish.RodName, oFish.ID1, oFish.ID2, oFish.ID3, oFish.ID4, oFish.FishType))
                        {
                            //リリースする
                            logger.Output(LogLevelKind.DEBUG, string.Format("リリースする {0}", GetViewFishName(oFish.FishName, oFish.FishType, oFish.FishCount, oFish.Critical)));
                            while (this.PlayerStatus == FFACETools.Status.FishBite)
                            {
                                fface.Windower.SendKeyPress(KeyCode.EscapeKey);
                                Thread.Sleep(settings.Global.WaitBase);
                            }
                            continue;
                        } 
                        setMessage(string.Format("格闘中：{0}", GetViewFishName(oFish.FishName, oFish.FishType, oFish.FishCount, oFish.Critical)));
                        //釣り格闘
                        while (fface.Fish.HPCurrent > 0 && fface.Player.Status == Status.FishBite)
                        {
                            //強制HP0
                            if (settings.Fishing.HP0)
                            {
                                if (isExecHp0(oFish.EarthTime, waitHP0))
                                {
                                    logger.Output(LogLevelKind.INFO,"制限時間を過ぎたので、魚のHPを強制的にゼロにします");
                                    fface.Fish.SetHP(0);
                                    Thread.Sleep(settings.Global.WaitBase);
                                }
                            }
                            //格闘
                            fface.Fish.FightFish();
                            Thread.Sleep(settings.Global.WaitBase);
                        }
                        //HP0になった瞬間に釣り上げるとFFの画面上ではHPが残ったままになるのでウェイト
                        Thread.Sleep(500);
                        //チャット処理
                        while(chat.GetNextChatLine(out cl))
                        {
                            ChatKbnKind fightingChatKbn = getChatKbnFromChatline(cl, out chatKbnArgs, ref oChatReceive, ref oEnemyAttack, ref oSneakWarning, ref oShipWarning);
                            logger.Output(LogLevelKind.DEBUG, string.Format("Chat:{0} ChatKbn:{1}", cl.Text, fightingChatKbn));
                            if (fightingChatKbn == ChatKbnKind.BaitCritical)//クリティカル
                            {
                                oFish.Critical = true;
                            }
                        }
                        //釣り上げる
                        //プレイヤステータスがFishBite以外になるまで待つ
                        while (this.PlayerStatus == FFACETools.Status.FishBite)
                        {
                            fishedFlg = true;
                            fface.Windower.SendKeyPress(KeyCode.EnterKey);
                            Thread.Sleep(settings.Global.WaitBase);
                        } 
                    }
                    else if (chatKbn == ChatKbnKind.CatchSingle)//釣れた
                    {
                        if (!fishedFlg) continue;//釣り上げていない場合は登録しない
                        oFish.FishName = chatKbnArgs[0];
                        oFish.FishCount = 1;
                        oFish.Result = FishResultStatusKind.Catch;
                        //データベースへの登録
                        if (!putDatabase(oFish)) return false;
                        //連続釣果無しカウントクリア
                        noCatchCount = 0;
                        setMessage(string.Format("釣果：{0}", GetViewFishName(oFish.FishName, oFish.FishType, oFish.FishCount, oFish.Critical)));
                        //イベント発生
                        EventFished(oFish.Result);
                        //プレイヤステータスがStandingになるまで待つ
                        waitChangePlayerStatus(FFACETools.Status.Standing);
                        return true;
                    }
                    else if (chatKbn == ChatKbnKind.CatchMultiple)//複数釣れた
                    {
                        if (!fishedFlg) continue;//釣り上げていない場合は登録しない
                        oFish.FishName = chatKbnArgs[0];
                        oFish.FishCount = int.Parse(chatKbnArgs[1]);
                        oFish.Result = FishResultStatusKind.Catch;
                        //データベースへの登録
                        if (!putDatabase(oFish)) return false;
                        //連続釣果無しカウントクリア
                        noCatchCount = 0;
                        setMessage(string.Format("釣果：{0}", GetViewFishName(oFish.FishName, oFish.FishType, oFish.FishCount, oFish.Critical)));
                        //イベント発生
                        EventFished(oFish.Result);
                        //プレイヤステータスがStandingになるまで待つ
                        waitChangePlayerStatus(FFACETools.Status.Standing);
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
                        waitChangePlayerStatus(FFACETools.Status.Standing);
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
                        setMessage(string.Format("釣果：鞄いっぱいでリリース {0}", GetViewFishName(oFish.FishName, oFish.FishType, oFish.FishCount, oFish.Critical)));
                        //イベント発生
                        EventFished(oFish.Result);
                        //プレイヤステータスがStandingになるまで待つ
                        waitChangePlayerStatus(FFACETools.Status.Standing);
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
                        waitChangePlayerStatus(FFACETools.Status.Standing);
                        return true;
                    }
                    else if (chatKbn == ChatKbnKind.Release)//リリース
                    {
                        oFish.Result = FishResultStatusKind.Release;
                        //データベースへの登録
                        if (!putDatabase(oFish)) return false;
                        //連続釣果無しカウントクリア
                        noCatchCount = 0;
                        setMessage(string.Format("釣果：リリース {0}", GetViewFishName(oFish.FishName, oFish.FishType, oFish.FishCount, oFish.Critical)));
                        //イベント発生
                        EventFished(oFish.Result);
                        //プレイヤステータスがStandingになるまで待つ
                        waitChangePlayerStatus(FFACETools.Status.Standing);
                        return true;
                    }
                    else if (chatKbn == ChatKbnKind.NoCatch)//逃げられた
                    {
                        oFish.Result = FishResultStatusKind.NoCatch;
                        //データベースへの登録
                        if (!putDatabase(oFish)) return false;
                        //連続釣果無しカウントクリア
                        noCatchCount = 0;
                        setMessage(string.Format("釣果：逃げられた {0}", GetViewFishName(oFish.FishName, oFish.FishType, oFish.FishCount, oFish.Critical)));
                        //イベント発生
                        EventFished(oFish.Result);
                        //プレイヤステータスがStandingになるまで待つ
                        waitChangePlayerStatus(FFACETools.Status.Standing);
                        return true;
                    }
                    else if (chatKbn == ChatKbnKind.LineBreak)//糸切れ
                    {
                        oFish.Result = FishResultStatusKind.LineBreak;
                        //データベースへの登録
                        if (!putDatabase(oFish)) return false;
                        //連続釣果無しカウントクリア
                        noCatchCount = 0;
                        setMessage(string.Format("釣果：糸切れ {0}", GetViewFishName(oFish.FishName, oFish.FishType, oFish.FishCount, oFish.Critical)));
                        //イベント発生
                        EventFished(oFish.Result);
                        //プレイヤステータスがStandingになるまで待つ
                        waitChangePlayerStatus(FFACETools.Status.Standing);
                        return true;
                    }
                    else if (chatKbn == ChatKbnKind.RodBreak)//竿折れ
                    {
                        oFish.Result = FishResultStatusKind.RodBreak;
                        //データベースへの登録
                        if (!putDatabase(oFish)) return false;
                        //連続釣果無しカウントクリア
                        noCatchCount = 0;
                        setMessage(string.Format("釣果：竿折れ {0}", GetViewFishName(oFish.FishName, oFish.FishType, oFish.FishCount, oFish.Critical)));
                        //イベント発生
                        EventFished(oFish.Result);
                        //プレイヤステータスがStandingになるまで待つ
                        waitChangePlayerStatus(FFACETools.Status.Standing);
                        return true;
                    }
                    else if (chatKbn == ChatKbnKind.BaitCritical)//クリティカル
                    {
                        oFish.Critical = true;
                    }
                    else if (oChatReceive) //チャット感知
                    {
                        //プレイヤステータスがStandingになるまで待つ
                        while (this.PlayerStatus != FFACETools.Status.Standing)
                        {
                            fface.Windower.SendKeyPress(KeyCode.EscapeKey);
                            Thread.Sleep(settings.Global.WaitBase);
                        }
                        return true;
                    }
                    else if (oEnemyAttack) //敵の攻撃感知
                    {
                        //プレイヤステータスがStandingになるまで待つ
                        while (this.PlayerStatus != FFACETools.Status.Standing)
                        {
                            fface.Windower.SendKeyPress(KeyCode.EscapeKey);
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
                //魚名の名寄せ
                string renameFishname = renameFish(iFish.FishName);
                //FishDBから魚情報取得（不明魚以外で）
                FishDBFishModel fish = fishDB.SelectFishFromIDName(iFish.RodName, iFish.ID1, iFish.ID2, iFish.ID3, iFish.ID4, renameFishname, false);
                FishDBIdModel id = fish.GetId(iFish.ID1, iFish.ID2, iFish.ID3, iFish.ID4);
                if (fish.FishName != string.Empty)
                {
                    iFish.FishCount = id.Count;
                    iFish.Critical = id.Critical;
                }
                //FishTypeの設定
                if (!isTmpFishFromName(renameFishname))
                {
                    if (iFish.FishType == FishDBFishTypeKind.UnknownSmallFish) iFish.FishType = FishDBFishTypeKind.SmallFish;
                    if (iFish.FishType == FishDBFishTypeKind.UnknownLargeFish) iFish.FishType = FishDBFishTypeKind.LargeFish;
                    if (iFish.FishType == FishDBFishTypeKind.UnknownItem) iFish.FishType = FishDBFishTypeKind.Item;
                }
                //登録情報の設定 
                FishDBFishModel fishDBFish = new FishDBFishModel();
                fishDBFish.FishName = renameFishname;
                FishDBIdModel fishDBId = new FishDBIdModel();
                fishDBId.ID1 = iFish.ID1;
                fishDBId.ID2 = iFish.ID2;
                fishDBId.ID3 = iFish.ID3;
                fishDBId.ID4 = iFish.ID4;
                fishDBId.Count = iFish.FishCount;
                fishDBId.Critical = iFish.Critical;
                fishDBFish.IDs.Add(fishDBId);
                fishDBFish.FishType = iFish.FishType;
                fishDBFish.ZoneNames.Add(iFish.ZoneName);
                fishDBFish.BaitNames.Add(iFish.BaitName);
                if (!fishDB.UpdateFish(iFish.RodName, fishDBFish))
                {
                    setMessage("FishDBデータベースへの登録に失敗");
                    return false;
                } 
            }
            //FishHistoryDBに登録
            if (!fishHistoryDB.Add(this.PlayerName, iFish))
            {
                setMessage("FishHistoryDBデータベースへの登録に失敗");
                return false;
            }
            return true;
        }
        /// <summary>
        /// 名寄せ情報に基づき、魚名を変更する
        /// </summary>
        /// <param name="iFishName">魚名</param>
        /// <returns>名寄せした魚名</returns>
        private string renameFish(string iFishName)
        {
            if (fishDB.RenameFish.ContainsKey(iFishName))
            {
                return fishDB.RenameFish[iFishName];
            }
            else
            {
                return iFishName;
            }
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
        private bool isWantedFish(string iRodName, int iID1, int iID2, int iID3, int iID4, FishDBFishTypeKind iFishType)
        {
            //FishType
            if ((settings.Fishing.IgnoreSmallFish && (iFishType == FishDBFishTypeKind.SmallFish || iFishType == FishDBFishTypeKind.UnknownSmallFish)) ||
                (settings.Fishing.IgnoreLargeFish && (iFishType == FishDBFishTypeKind.LargeFish || iFishType == FishDBFishTypeKind.UnknownLargeFish)) ||
                (settings.Fishing.IgnoreItem && (iFishType == FishDBFishTypeKind.Item || iFishType == FishDBFishTypeKind.UnknownItem)) ||
                (settings.Fishing.IgnoreMonster && iFishType == FishDBFishTypeKind.Monster))
            { 
                return false;
            }
            //学習モード
            if (settings.Fishing.Learning && (iFishType == FishDBFishTypeKind.UnknownSmallFish ||
                                              iFishType == FishDBFishTypeKind.UnknownLargeFish ||
                                              iFishType == FishDBFishTypeKind.UnknownItem ||
                                              iFishType == FishDBFishTypeKind.Monster||
                                              iFishType == FishDBFishTypeKind.Unknown))
            {
                return true;
            }
            //Wanted
            List<FishDBFishModel> fishes = fishDB.SelectFishFromID(iRodName, iID1, iID2, iID3, iID4, true);
            foreach (SettingsPlayerFishListWantedModel fish in settings.FishList.Wanted)
            {
                if (settings.FishList.Mode == Settings.FishListModeKind.ID)
                {
                    if (fish.ID1 == iID1 && fish.ID2 == iID2 && fish.ID3 == iID3 && fish.ID4 == iID4)
                    {
                        return true;
                    }
                }
                else if (settings.FishList.Mode == Settings.FishListModeKind.Name)
                {
                    if (fishes.Count > 0)
                    {
                        if (fish.FishName == fishes[0].FishName)
                        {
                            return true;
                        }
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
        private ChatKbnKind getChatKbnFromChatline(FFACE.ChatTools.ChatLine iCl, out List<string> oArgs, ref bool rChatRecieve, ref bool rEnemyAttack, ref bool rSneakWarning, ref bool rShipWarning)
        {
            oArgs = new List<string>();
            switch (iCl.Type)
            {
                case ChatMode.RcvdTell:
                case ChatMode.SentTell:
                    if(settings.Fishing.ChatTell) rChatRecieve = true;
                    return ChatKbnKind.Tell;
                case ChatMode.RcvdSay:
                case ChatMode.SentSay:
                    if(settings.Fishing.ChatSay) rChatRecieve = true;
                    return ChatKbnKind.Say;
                case ChatMode.RcvdParty:
                case ChatMode.SentParty:
                    if(settings.Fishing.ChatParty) rChatRecieve = true;
                    return ChatKbnKind.Party;
                case ChatMode.RcvdLinkShell:
                case ChatMode.SentLinkShell:
                    if(settings.Fishing.ChatLs) rChatRecieve = true;
                    return ChatKbnKind.Linkshell;
                case ChatMode.RcvdShout:
                case ChatMode.SentShout:
                case ChatMode.RcvdYell:
                case ChatMode.SentYell:
                    if(settings.Fishing.ChatShout) rChatRecieve = true;
                    return ChatKbnKind.Shout;
            }
            foreach (KeyValuePair<ChatKbnKind, string> v in dictionaryChat)
            {
                string searchStr = string.Empty;
                //プレイヤー名の置換
                if (v.Key == ChatKbnKind.CatchSingle ||
                    v.Key == ChatKbnKind.CatchMultiple ||
                    v.Key == ChatKbnKind.CatchMonster ||
                    v.Key == ChatKbnKind.EnemyAttack1 || 
                    v.Key == ChatKbnKind.EnemyAttack2 ||
                    v.Key == ChatKbnKind.SneakWarning2)
                    searchStr = string.Format(v.Value, this.PlayerName);
                else 
                    searchStr = v.Value;

                if (MiscTool.IsRegexString(iCl.Text, searchStr))
                {
                    oArgs = MiscTool.GetRegexString(iCl.Text, searchStr);
                    if (v.Key == ChatKbnKind.EnemyAttack1 || v.Key == ChatKbnKind.EnemyAttack2) rEnemyAttack = true;
                    if (v.Key == ChatKbnKind.SneakWarning1/* || v.Key == ChatKbnKind.SneakWarning2*/) rSneakWarning = true;
                    if (v.Key == ChatKbnKind.ShipWarning1 || v.Key == ChatKbnKind.ShipWarning2 ||
                        v.Key == ChatKbnKind.ShipWarning3 || v.Key == ChatKbnKind.ShipWarning4 ||
                        v.Key == ChatKbnKind.ShipWarning5 || v.Key == ChatKbnKind.ShipWarning6 ||
                        v.Key == ChatKbnKind.ShipWarning7 || v.Key == ChatKbnKind.ShipWarning8 || 
                        v.Key == ChatKbnKind.ShipWarning9) rShipWarning = true;
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
            if (MiscTool.IsRegexString(iChatText, dictionaryChat[ChatKbnKind.BaitMonster])) return FishDBFishTypeKind.Monster;
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
            if (MiscTool.IsRegexString(iChatText, dictionaryChat[ChatKbnKind.BaitMonster])) return FishDBFishTypeKind.Monster;
            return FishDBFishTypeKind.Unknown;
        }
        /// <summary>
        /// 名称不明の魚の一時名称を取得する
        /// </summary>
        /// <param name="iFishType">魚タイプ</param>
        /// <param name="iID1">ID1</param>
        /// <param name="iID2">ID2</param>
        /// <param name="iID3">ID3</param>
        /// <param name="iID4">ID4</param>
        /// <returns>一時名称</returns>
        private string getTmpFishNameFromFishType(FishDBFishTypeKind iFishType, int iID1, int iID2, int iID3, int iID4)
        {
            string tmpFishName = string.Empty;
            switch (iFishType)
            {
                case FishDBFishTypeKind.SmallFish:
                case FishDBFishTypeKind.UnknownSmallFish:
                case FishDBFishTypeKind.LargeFish:
                case FishDBFishTypeKind.UnknownLargeFish:
                    tmpFishName = FishDB.FISHNAME_UNKNOWN_FISH;
                    break;
                case FishDBFishTypeKind.Item:
                case FishDBFishTypeKind.UnknownItem:
                    tmpFishName = FishDB.FISHNAME_UNKNOWN_ITEM;
                    break;
                case FishDBFishTypeKind.Monster:
                    tmpFishName = FishDB.FISHNAME_UNKNOWN_MONSTER;
                    break;
                default:
                    tmpFishName = FishDB.FISHNAME_UNKNOWN;
                    break;
            }
            return string.Format("{0}{1:000}-{2:000}-{3:000}-{4:000}", tmpFishName, iID1, iID2, iID3, iID4);
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
            return GetViewFishName(iFishName, iFishType, 0, false, false);
        }
        /// <summary>
        /// 画面表示用の魚名を取得する(ID別で表示)
        /// </summary>
        /// <param name="iFishName">魚名</param>
        /// <param name="iFishType">魚タイプ</param>
        /// <param name="iFishCount">数</param>
        /// <param name="iCritical">クリティカル</param>
        /// <returns></returns>
        public static string GetViewFishName(string iFishName, FishDBFishTypeKind iFishType, int iFishCount, bool iCritical)
        {
            return GetViewFishName(iFishName, iFishType, iFishCount, iCritical, true);
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
        public static string GetViewFishName(string iFishName, FishDBFishTypeKind iFishType, int iFishCount, bool iCritical, bool iDetail)
        {
            string size = string.Empty;
            if (iFishType == FishDBFishTypeKind.SmallFish || iFishType == FishDBFishTypeKind.UnknownSmallFish)
            {
                size = "(S)";
            }
            else if (iFishType == FishDBFishTypeKind.LargeFish || iFishType == FishDBFishTypeKind.UnknownLargeFish)
            {
                size = "(L)";
            }
            else if (iFishType == FishDBFishTypeKind.Item || iFishType == FishDBFishTypeKind.UnknownItem)
            {
                size = "(I)";
            }
            else if (iFishType == FishDBFishTypeKind.Monster)
            {
                size = "(M)";
            }
            else
            {
                size = "(?)";
            }
            string critical = string.Empty;
            if (iCritical) critical = "!";
            string count = string.Empty;
            if (iFishCount > 1) count = "x" + iFishCount.ToString();
            return string.Format("{0}{1}{2}{3}", iFishName, count, size, critical);

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
        private bool isHourInRange(FFACETools.FFACE.TimerTools.VanaTime iTarget, int iFrom, int iTo)
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
        private void waitChangePlayerStatus(FFACETools.Status iPlayerStatus)
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
            if (fface.Player.MPCurrent < 12) return;
            //魔法詠唱可能かチェック
            //if (!fface.Player.KnowsSpell(SpellList.Sneak)) return;
            //リキャストタイムまで待つ
            for (int i = 0; i < Constants.MAX_LOOP_COUNT && fface.Timer.GetSpellRecast(SpellList.Sneak) > 0; i++) Thread.Sleep(100);
            //スニーク詠唱
            fface.Windower.SendString("/ma スニーク <me>");
            //詠唱開始まで待つ
            for (int i = 0; i < Constants.MAX_LOOP_COUNT && fface.Player.CastPercentEx == 100; i++) Thread.Sleep(100);
            for (int i = 0; i < Constants.MAX_LOOP_COUNT && fface.Player.CastMax == 0.0f; i++) Thread.Sleep(100);
            //BUFFが残っている場合は、詠唱完了１秒前に切る
            if (control.IsBuff(StatusEffect.Sneak))
            {
                for (int i = 0; i < Constants.MAX_LOOP_COUNT; i++)
                {
                    float remain = fface.Player.CastMax - (fface.Player.CastMax * fface.Player.CastPercent);
                    if (remain < settings.Fishing.SneakFishingRemain) break;
                    Thread.Sleep(100);
                }
                fface.Windower.SendString(string.Format("//lua c cancel {0}", StatusEffect.Sneak.ToString("D")));
            }
            Thread.Sleep(5000);
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
            int rodId = FFACE.ParseResources.GetItemId(iRodName);
            uint rodCnt = fface.Item.GetItemCount(rodId, InventoryType.Inventory);
            //アイテムの装備
            if (rodCnt > 0)
            {
                fface.Windower.SendString(string.Format("/equip Range {0}", iRodName));
                Thread.Sleep(settings.Global.WaitEquip);
            }
            else
            {
                return false;
            }
            //装備のチェック
            if (this.RodName != iRodName) return false;
            return true;
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
            int baitId = FFACE.ParseResources.GetItemId(iBaitName);
            uint baitCnt = fface.Item.GetItemCount(baitId, InventoryType.Inventory);
            //アイテムの装備
            if (baitCnt > 0)
            {
                fface.Windower.SendString(string.Format("/equip Ammo {0}", iBaitName));
                Thread.Sleep(settings.Global.WaitEquip);
            }
            else
            {
                return false;
            }
            //装備のチェック
            if (this.BaitName != iBaitName) return false;
            return true;
        }
        /// <summary>
        /// 指定されたアイテム名が釣り竿かどうか判定
        /// </summary>
        /// <param name="iRodName">釣り竿名</param>
        /// <returns>True:釣り竿だった場合</returns>
        private bool isRod(string iRodName)
        {
            if (iRodName.Length == 0) return false;
            return fishDB.Rods.Contains(iRodName);
        }
        /// <summary>
        /// 指定されたアイテム名がエサかどうか判定
        /// </summary>
        /// <param name="iBaitName">エサ名</param>
        /// <returns>True:エサだった場合</returns>
        private bool isBait(string iBaitName)
        {
            if (iBaitName.Length == 0) return false;
            return fishDB.Baits.Contains(iBaitName);
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
                if (!moveOkFlg && settings.Fishing.NoBaitNoRodSatchel)
                    if (control.GetItem(iItemName, InventoryType.Satchel))
                    {
                        moveOkFlg = true;
                        setMessage(string.Format("{0}を{1}から取り出しました", iItemName, InventoryType.Satchel.ToString()));
                        Thread.Sleep(1000);
                    }
                if (!moveOkFlg && settings.Fishing.NoBaitNoRodSack)
                    if (control.GetItem(iItemName, InventoryType.Sack))
                    {
                        moveOkFlg = true;
                        setMessage(string.Format("{0}を{1}から取り出しました", iItemName, InventoryType.Sack.ToString()));
                        Thread.Sleep(1000);
                    }
                if (!moveOkFlg && settings.Fishing.NoBaitNoRodCase)
                    if (control.GetItem(iItemName, InventoryType.Case))
                    {
                        moveOkFlg = true;
                        setMessage(string.Format("{0}を{1}から取り出しました", iItemName, InventoryType.Case.ToString()));
                        Thread.Sleep(1000);
                    }
            }
            return moveOkFlg;
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
            List<FishDBFishModel> fishes = fishDB.SelectFishList(this.RodName, string.Empty, string.Empty);
            foreach (FishDBFishModel fish in fishes)
            {
                if (control.IsExistItem(fish.FishName, InventoryType.Inventory))
                {
                    control.PutItem(fish.FishName, iInventoryType);
                    setMessage(string.Format("{0}を{1}に移動しました", fish.FishName, iInventoryType.ToString()));
                    Thread.Sleep(1000);
                    return true;
                }
            }
            return false;
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
            logger.Output(LogLevelKind.INFO, iMessage);
            this.Message = iMessage;
            EventChangeMessage(iMessage);
        }
        #endregion

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
    }
}
