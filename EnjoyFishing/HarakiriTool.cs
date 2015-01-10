using FFACETools;
using MiscTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace EnjoyFishing
{
    public class HarakiriTool
    {
        #region Dictionary
        private static Dictionary<ChatKbnKind, string> dictionaryChat = new Dictionary<ChatKbnKind, string>()
        {
            {ChatKbnKind.Zaldon1, "Zaldon : おお、持ってきたのか。"},
            {ChatKbnKind.Zaldon2, "Zaldon : んー、残念だったな。"},
            {ChatKbnKind.Zaldon3, "腹の中から(.*)が出てきたぜ！"},
            {ChatKbnKind.Zaldon4, "まあ、あきらめずにまた持ってきてくれや。"},
            {ChatKbnKind.Zaldon5, "まあ、魚をまた持ってきてくれや。"},
            {ChatKbnKind.GetItem, "(.*)を手にいれた！"},
        };
        #endregion

        #region Enum
        private enum ChatKbnKind
        {
            Zaldon1,
            Zaldon2,
            Zaldon3,
            Zaldon4,
            Zaldon5,
            GetItem,
            Unknown,
        }
        public enum RunningStatusKind
        {
            Running,
            Stop,
            UnderStop
        }
        public enum HarakiriStatusKind
        {
            Normal,
            Error,
            Wait,
        }
        #endregion

        private const int NPCID_ZALDON = 23;

        private PolTool pol;
        private FFACE fface;
        private Settings settings;
        private LoggerTool logger;
        private ChatTool chat;
        private FFACEControl control;
        private Thread thHarakiri;
        private FishDB fishDB;
        private HarakiriDB harakiriDB;
        private string harakiriFishName;


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
        public HarakiriStatusKind HarakiriStatus
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
        #endregion

        #region イベント
        #region Fished
        /// <summary>
        /// Fishedeイベントで返されるデータ
        /// </summary>
        public class HarakiriOnceEventArgs : EventArgs
        {
            public HarakiriStatusKind HarakiriStatus;
            public string Message;
        }
        public delegate void HarakiriOnceEventHandler(object sender, HarakiriOnceEventArgs e);
        public event HarakiriOnceEventHandler HarakiriOnce;
        protected virtual void OnHarakiriOnce(HarakiriOnceEventArgs e)
        {
            if (HarakiriOnce != null)
            {
                HarakiriOnce(this, e);
            }
        }
        private void EventHarakiriOnce(HarakiriStatusKind iHarakiriStatus)
        {
            //返すデータの設定
            HarakiriOnceEventArgs e = new HarakiriOnceEventArgs();
            e.HarakiriStatus = iHarakiriStatus;
            e.Message = this.Message;
            //イベントの発生
            OnHarakiriOnce(e);
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
            public HarakiriTool.RunningStatusKind RunningStatus;
            public HarakiriTool.HarakiriStatusKind FishingStatus;
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
        private void EventChangeStatus(RunningStatusKind iRunningStatus, HarakiriStatusKind iFishingStatus)
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
        public HarakiriTool(PolTool iPol, ChatTool iChat, Settings iSettings, LoggerTool iLogger)
        {
            pol = iPol;
            fface = iPol.FFACE;
            chat = iChat;
            settings = iSettings;
            logger = iLogger;
            fishDB = new FishDB(logger);
            harakiriDB = new HarakiriDB(logger);
            control = new FFACEControl(pol, chat, logger);
            control.MaxLoopCount = Constants.MAX_LOOP_COUNT;
            control.UseEnternity = settings.UseEnternity;
            control.BaseWait = settings.Global.WaitBase;
            control.ChatWait = settings.Global.WaitChat;
            this.RunningStatus = RunningStatusKind.Stop;
            this.HarakiriStatus = HarakiriStatusKind.Normal;
        }
        #endregion

        #region スレッド操作など
        /// <summary>
        /// システム終了処理
        /// </summary>
        public void SystemAbort()
        {
            if (this.thHarakiri != null && this.thHarakiri.IsAlive) this.thHarakiri.Abort();
            chat.Stop();
        }
        /// <summary>
        /// ハラキリ開始
        /// </summary>
        /// <returns></returns>
        public HarakiriStatusKind HarakiriStart(string iFishName)
        {
            this.harakiriFishName = iFishName;

            setRunningStatus(RunningStatusKind.Running);
            setHarakiriStatus(HarakiriStatusKind.Normal);
            setMessage(string.Empty);
            //スレッド開始
            thHarakiri = new Thread(threadHarakiri);
            thHarakiri.Start();
            thHarakiri.Join();

            return this.HarakiriStatus;
        }
        /// <summary>
        /// ハラキリ中止
        /// </summary>
        /// <returns></returns>
        public bool HarakiriAbort()
        {
            //ステータス変更
            setRunningStatus(RunningStatusKind.UnderStop);
            //スレッド停止
            if (thHarakiri != null && thHarakiri.IsAlive) thHarakiri.Abort();
            //ステータス変更
            setRunningStatus(RunningStatusKind.Stop);
            if (HarakiriStatus == HarakiriStatusKind.Wait) setHarakiriStatus(HarakiriStatusKind.Normal);

            return true;
        }
        /// <summary>
        /// ハラキリ停止メインスレッド
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

        #region ハラキリ
        /// <summary>
        /// メインスレッド
        /// </summary>
        private void threadHarakiri()
        {
            chat.CurrentIndex = chat.MaxIndex;
            setHarakiriStatus(HarakiriStatusKind.Normal);
            HarakiriDBHistoryModel fish = new HarakiriDBHistoryModel();
            bool firsttime = true;

            logger.Output(LogLevelKind.DEBUG, "ハラキリスレッド開始");
            while (this.RunningStatus == RunningStatusKind.Running)
            {
                //未入力チェック
                if (this.harakiriFishName.Length == 0)
                {
                    setRunningStatus(RunningStatusKind.Stop);
                    setHarakiriStatus(HarakiriStatusKind.Error);
                    setMessage("ハラキリする魚を入力してください");
                    return;
                }
                //鞄に入っている魚の総数を取得
                int itemId = FFACE.ParseResources.GetItemId(this.harakiriFishName);
                uint remain = fface.Item.GetItemCount(itemId, InventoryType.Inventory);
                if (settings.UseItemizer)
                {
                    remain += fface.Item.GetItemCount(itemId, InventoryType.Satchel);
                    remain += fface.Item.GetItemCount(itemId, InventoryType.Sack);
                    remain += fface.Item.GetItemCount(itemId, InventoryType.Case);
                }
                if (remain <= 0)
                {
                    setRunningStatus(RunningStatusKind.Stop);
                    if (firsttime)
                    {
                        setHarakiriStatus(HarakiriStatusKind.Error);
                        setMessage("ハラキリする魚が見つかりませんでした");
                    }
                    else
                    {
                        setHarakiriStatus(HarakiriStatusKind.Normal);
                        setMessage("ハラキリが完了しました");
                    }
                    return;
                }
                firsttime = false;
                setMessage(string.Format("ハラキリ中：{0} 残り{1}匹", this.harakiriFishName, remain));
                //鞄に対象の魚を移動させる
                if (fface.Item.GetItemCount(itemId, InventoryType.Inventory) <= 0)
                {
                    if (fface.Item.GetItemCount(itemId, InventoryType.Satchel) > 0)
                    {
                        control.GetItem(this.harakiriFishName, InventoryType.Satchel);
                    }
                    else if (fface.Item.GetItemCount(itemId, InventoryType.Sack) > 0)
                    {
                        control.GetItem(this.harakiriFishName, InventoryType.Sack);
                    }
                    else if (fface.Item.GetItemCount(itemId, InventoryType.Case) > 0)
                    {
                        control.GetItem(this.harakiriFishName, InventoryType.Case);
                    }
                }
                //Zaldonの近くかチェック
                if (fface.Player.Zone != Zone.Selbina ||
                    (fface.NPC.Distance(NPCID_ZALDON) != 0f && fface.NPC.Distance(NPCID_ZALDON) > 6))
                {
                    setRunningStatus(RunningStatusKind.Stop);
                    setHarakiriStatus(HarakiriStatusKind.Error);
                    setMessage("セルビナのZaldonの近くで実行してください");
                    return;
                }
                //メニュー開いていたら閉じる
                if (!control.CloseDialog())
                {
                    setRunningStatus(RunningStatusKind.Stop);
                    setHarakiriStatus(HarakiriStatusKind.Error);
                    setMessage("メニューが閉じられない");
                    break;
                }

                //ターゲット設定
                control.SetTargetFromId(NPCID_ZALDON);
                Thread.Sleep(settings.Global.WaitChat);//Wait
                //アイテムトレード
                fface.Windower.SendString(string.Format("/item {0} <t>", this.harakiriFishName));

                HarakiriResultStatusKind result = HarakiriResultStatusKind.Unknown;
                string itemName = string.Empty;
                FFACETools.FFACE.ChatTools.ChatLine cl = new FFACE.ChatTools.ChatLine();
                while (this.RunningStatus == RunningStatusKind.Running)
                {
                    Thread.Sleep(settings.Global.WaitChat);
                    if (!chat.GetNextChatLine(out cl)) continue;
                    //チャット区分の取得
                    List<string> chatKbnArgs = new List<string>();
                    ChatKbnKind chatKbn = getChatKbnFromChatline(cl, out chatKbnArgs);
                    logger.Output(LogLevelKind.DEBUG, string.Format("Chat:{0} ChatKbn:{1}", cl.Text, chatKbn));
                    if (chatKbn == ChatKbnKind.Zaldon1)
                    {
                        if (!settings.UseEnternity)
                        {
                            fface.Windower.SendKeyPress(KeyCode.EnterKey);
                            Thread.Sleep(settings.Global.WaitChat);
                        }
                    }
                    else if (chatKbn == ChatKbnKind.Zaldon2)
                    {
                        result = HarakiriResultStatusKind.NotFound;
                        setMessage("ハラキリ結果：何も見つからなかった");
                        if (!settings.UseEnternity)
                        {
                            fface.Windower.SendKeyPress(KeyCode.EnterKey);
                            Thread.Sleep(settings.Global.WaitChat);
                        }
                    }
                    else if (chatKbn == ChatKbnKind.Zaldon3)
                    {
                        result = HarakiriResultStatusKind.Found;
                        itemName = chatKbnArgs[0];
                        setMessage(string.Format("ハラキリ結果：{0}を見つけた", itemName));
                        fface.Windower.SendKeyPress(KeyCode.EnterKey);
                        Thread.Sleep(settings.Global.WaitChat);
                    }
                    else if (chatKbn == ChatKbnKind.Zaldon4 || chatKbn == ChatKbnKind.Zaldon5)
                    {
                        HarakiriDBHistoryModel history = new HarakiriDBHistoryModel();
                        history.FishName = this.harakiriFishName;
                        history.ItemName = itemName;
                        history.Result = result;
                        harakiriDB.Add(history);
                        Thread.Sleep(settings.Global.WaitChat);
                        break;
                    }
                }

            }
            logger.Output(LogLevelKind.DEBUG, "ハラキリスレッド終了");
        }
        /// <summary>
        /// 魚を１回釣る
        /// </summary>
        /// <param name="oFish"></param>
        /// <param name="oMessage"></param>
        /// <returns></returns>
        //private bool FishingOnce(out FishHistoryDBFishModel oFish, out bool oChatReceive, out bool oEnemyAttack, out bool oSneakWarning)
        //{
        //    //戻り値初期化
        //    oFish = new FishHistoryDBFishModel();
        //    oFish.FishName = string.Empty;
        //    oFish.FishCount = 0;
        //    oFish.ZoneName = this.ZoneName;
        //    oFish.RodName = this.RodName;
        //    oFish.BaitName = this.BaitName;
        //    oFish.ID1 = 0;
        //    oFish.ID2 = 0;
        //    oFish.ID3 = 0;
        //    oFish.ID4 = 0;
        //    oFish.Critical = false;
        //    oFish.FishType = FishDBFishTypeKind.Unknown;
        //    oFish.Result = FishResultStatusKind.NoBite;
        //    oFish.EarthTime = this.EarthDateTime;
        //    oFish.VanaTime = this.VanaDateTimeYmdhm;
        //    oFish.VanaWeekDay = this.DayType;
        //    oFish.MoonPhase = this.MoonPhase;
        //    oFish.X = this.Position.X;
        //    oFish.Y = this.Position.Y;
        //    oFish.Z = this.Position.Z;
        //    oFish.H = this.Position.H;

        //    oChatReceive = false;
        //    oEnemyAttack = false;
        //    oSneakWarning = false;

        //    setHarakiriStatus(FishingStatusKind.Normal);
        //    setMessage(string.Format("キャスト中：{0}x{1}", this.RodNameWithRemain, this.BaitNameWithRemain));

        //    //キャスト
        //    while (this.RunningStatus == RunningStatusKind.Running && fface.Player.Status != FFACETools.Status.Fishing)
        //    {
        //        fface.Windower.SendString("/fish");
        //        Thread.Sleep(settings.Global.WaitChat);

        //        FFACE.ChatTools.ChatLine cl = new FFACE.ChatTools.ChatLine();
        //        while (chat.GetNextChatLine(out cl))
        //        {
        //            //チャット区分の取得
        //            List<string> chatKbnArgs = new List<string>();
        //            ChatKbnKind chatKbn = getChatKbnFromChatline(cl, out chatKbnArgs, ref oChatReceive, ref oEnemyAttack, ref oSneakWarning);
        //            logger.Output(LogLevelKind.DEBUG, string.Format("Chat:{0} ChatKbn:{1}", cl.Text, chatKbn));
        //            //エラーチェック
        //            if (chatKbn == ChatKbnKind.CanNotFishing)
        //            {
        //                setHarakiriStatus(FishingStatusKind.Error);
        //                setMessage("釣りができない場所だったので停止");
        //                return false;
        //            }
        //            else if (chatKbn == ChatKbnKind.NotEquipRod)
        //            {
        //                setHarakiriStatus(FishingStatusKind.Error);
        //                setMessage("竿を装備していないので停止");
        //                return false;
        //            }
        //            else if (chatKbn == ChatKbnKind.NotEquipBait)
        //            {
        //                setHarakiriStatus(FishingStatusKind.Error);
        //                setMessage("エサを装備していないので停止");
        //                return false;
        //            }
        //            else if (oChatReceive) //チャット感知
        //            {
        //                return true;
        //            }
        //            else if (oEnemyAttack) //敵の攻撃感知
        //            {
        //                return true;
        //            }
        //        }
        //    }
        //    this.lastRodName = this.RodName;
        //    this.lastBaitName = this.BaitName;
        //    while (this.RunningStatus == RunningStatusKind.Running)
        //    {
        //        FFACETools.FFACE.ChatTools.ChatLine cl = new FFACE.ChatTools.ChatLine();
        //        while(chat.GetNextChatLine(out cl))
        //        {
        //            //チャット区分の取得
        //            List<string> chatKbnArgs = new List<string>();
        //            ChatKbnKind chatKbn = getChatKbnFromChatline(cl, out chatKbnArgs, ref oChatReceive, ref oEnemyAttack, ref oSneakWarning);
        //            logger.Output(LogLevelKind.DEBUG, string.Format("Chat:{0} ChatKbn:{1}", cl.Text, chatKbn));
                    
        //            if (chatKbn == ChatKbnKind.BaitSmallFish || chatKbn == ChatKbnKind.BaitLargeFish ||
        //                chatKbn == ChatKbnKind.BaitItem || chatKbn == ChatKbnKind.BaitMonster)//魚がかかった
        //            {
        //                //プレイヤステータスがFishBiteになるまで待つ
        //                while (this.PlayerStatus != FFACETools.Status.FishBite)
        //                {
        //                    Thread.Sleep(settings.Global.WaitBase);
        //                }
        //                Thread.Sleep(500);
        //                //IDの設定
        //                oFish.ID1 = fface.Fish.ID.ID1;
        //                oFish.ID2 = fface.Fish.ID.ID2;
        //                oFish.ID3 = fface.Fish.ID.ID3;
        //                oFish.ID4 = fface.Fish.ID.ID4;
        //                //魚名称・タイプの設定
        //                List<FishDBFishModel> fishList = fishDB.SelectFishFromID(oFish.RodName, oFish.ID1, oFish.ID2, oFish.ID3, oFish.ID4, false);
        //                FishDBFishModel fish = new FishDBFishModel();
        //                if (fishList.Count > 0)
        //                {
        //                    fish = fishList[0];
        //                    oFish.FishName = fish.FishName;
        //                    oFish.FishType = fish.FishType;
        //                    oFish.FishCount = fish.GetId(oFish.ID1, oFish.ID2, oFish.ID3, oFish.ID4).Count;
        //                    oFish.Critical = fish.GetId(oFish.ID1, oFish.ID2, oFish.ID3, oFish.ID4).Critical;
        //                }
        //                else
        //                {
        //                    oFish.FishType = getTmpFishTypeFromChat(cl.Text);
        //                    oFish.FishName = getTmpFishNameFromFishType(oFish.FishType, oFish.ID1, oFish.ID2, oFish.ID3, oFish.ID4);
        //                }
        //                setMessage(string.Format("格闘中：{0}", GetViewFishName(oFish.FishName, oFish.FishType, oFish.FishCount, oFish.Critical)));
        //                logger.Output(LogLevelKind.INFO, string.Format("魚ID：{0:000}-{1:000}-{2:000}-{3:000} 魚タイプ：{4}", oFish.ID1, oFish.ID2, oFish.ID3, oFish.ID4, oFish.FishType));
        //                //日時の設定
        //                oFish.EarthTime = this.EarthDateTime;
        //                oFish.VanaTime = this.VanaDateTimeYmdhm;
        //                oFish.VanaWeekDay = this.DayType;
        //                oFish.MoonPhase = this.MoonPhase;
        //                //HP0の設定
        //                int waitHP0 = MiscTool.GetRandomNumber(settings.Fishing.HP0Min, settings.Fishing.HP0Max);
        //                //反応時間待機
        //                Thread.Sleep(settings.Global.WaitChat); //wait
        //                if (settings.Fishing.ReactionTime)
        //                {
        //                    wait(settings.Fishing.ReactionTimeMin, settings.Fishing.ReactionTimeMax, "反応待機中：{0:0.0}s " + GetViewFishName(oFish.FishName, oFish.FishType, oFish.FishCount, oFish.Critical));
        //                }
        //                //リリース判定
        //                if (!isWantedFish(oFish.RodName, oFish.ID1, oFish.ID2, oFish.ID3, oFish.ID4, oFish.FishType))
        //                {
        //                    //リリースする
        //                    logger.Output(LogLevelKind.DEBUG, string.Format("リリースする {0}", GetViewFishName(oFish.FishName, oFish.FishType, oFish.FishCount, oFish.Critical)));
        //                    while (this.PlayerStatus == FFACETools.Status.FishBite)
        //                    {
        //                        fface.Windower.SendKeyPress(KeyCode.EscapeKey);
        //                        Thread.Sleep(settings.Global.WaitBase);
        //                    }
        //                    continue;
        //                } 
        //                setMessage(string.Format("格闘中：{0}", GetViewFishName(oFish.FishName, oFish.FishType, oFish.FishCount, oFish.Critical)));
        //                //釣り格闘
        //                while (fface.Fish.HPCurrent > 0 && fface.Player.Status == Status.FishBite)
        //                {
        //                    //強制HP0
        //                    if (settings.Fishing.HP0)
        //                    {
        //                        if (isExecHp0(oFish.EarthTime, waitHP0))
        //                        {
        //                            logger.Output(LogLevelKind.INFO,"制限時間を過ぎたので、魚のHPを強制的にゼロにします");
        //                            fface.Fish.SetHP(0);
        //                            Thread.Sleep(settings.Global.WaitBase);
        //                        }
        //                    }
        //                    fface.Fish.FightFish();
        //                    Thread.Sleep(settings.Global.WaitBase);
        //                }
        //                //釣り上げる
        //                //プレイヤステータスがFishBite以外になるまで待つ
        //                while (this.PlayerStatus == FFACETools.Status.FishBite)
        //                {
        //                    fface.Windower.SendKeyPress(KeyCode.EnterKey);
        //                    Thread.Sleep(settings.Global.WaitBase);
        //                } 
        //            }
        //            else if (chatKbn == ChatKbnKind.CatchSingle)//釣れた
        //            {
        //                oFish.FishName = chatKbnArgs[0];
        //                oFish.FishCount = 1;
        //                oFish.Result = FishResultStatusKind.Catch;
        //                //データベースへの登録
        //                if (!putDatabase(oFish)) return false;
        //                //連続釣果無しカウントクリア
        //                noCatchCount = 0;
        //                setMessage(string.Format("釣果：{0}", GetViewFishName(oFish.FishName, oFish.FishType, oFish.FishCount, oFish.Critical)));
        //                //イベント発生
        //                EventFished(oFish.Result);
        //                //プレイヤステータスがStandingになるまで待つ
        //                waitChangePlayerStatus(FFACETools.Status.Standing);
        //                return true;
        //            }
        //            else if (chatKbn == ChatKbnKind.CatchMultiple)//複数釣れた
        //            {
        //                oFish.FishName = chatKbnArgs[0];
        //                oFish.FishCount = int.Parse(chatKbnArgs[1]);
        //                oFish.Result = FishResultStatusKind.Catch;
        //                //データベースへの登録
        //                if (!putDatabase(oFish)) return false;
        //                //連続釣果無しカウントクリア
        //                noCatchCount = 0;
        //                setMessage(string.Format("釣果：{0}", GetViewFishName(oFish.FishName, oFish.FishType, oFish.FishCount, oFish.Critical)));
        //                //イベント発生
        //                EventFished(oFish.Result);
        //                //プレイヤステータスがStandingになるまで待つ
        //                waitChangePlayerStatus(FFACETools.Status.Standing);
        //                return true;
        //            }
        //            else if (chatKbn == ChatKbnKind.CatchMonster)//モンスター釣れた
        //            {
        //                //oFish.FishName = chatKbnArgs[0];
        //                oFish.FishCount = 1;
        //                oFish.Result = FishResultStatusKind.Catch;
        //                //データベースへの登録
        //                if (!putDatabase(oFish)) return false;
        //                //連続釣果無しカウントクリア
        //                noCatchCount = 0;
        //                setMessage(string.Format("釣果：{0}", oFish.FishName));
        //                //イベント発生
        //                EventFished(oFish.Result);
        //                //プレイヤステータスがStandingになるまで待つ
        //                waitChangePlayerStatus(FFACETools.Status.Standing);
        //                return true;
        //            }
        //            else if (chatKbn == ChatKbnKind.InventoryFull)//鞄いっぱい
        //            {
        //                oFish.FishName = chatKbnArgs[0];
        //                oFish.FishCount = 1;
        //                oFish.Result = FishResultStatusKind.Release;
        //                //データベースへの登録
        //                if (!putDatabase(oFish)) return false;
        //                //連続釣果無しカウントクリア
        //                noCatchCount = 0;
        //                setMessage(string.Format("釣果：鞄いっぱいでリリース {0}", GetViewFishName(oFish.FishName, oFish.FishType, oFish.FishCount, oFish.Critical)));
        //                //イベント発生
        //                EventFished(oFish.Result);
        //                //プレイヤステータスがStandingになるまで待つ
        //                waitChangePlayerStatus(FFACETools.Status.Standing);
        //                return true;
        //            }
        //            else if (chatKbn == ChatKbnKind.NoBait)//何も釣れなかった
        //            {
        //                oFish.Result = FishResultStatusKind.NoBite;
        //                //データベースへの登録
        //                if (!putDatabase(oFish)) return false;
        //                //連続釣果無しカウントアップ
        //                noCatchCount++;
        //                setMessage(string.Format("釣果：何も釣れなかった {0}連続", noCatchCount));
        //                //イベント発生
        //                EventFished(oFish.Result);
        //                //プレイヤステータスがStandingになるまで待つ
        //                waitChangePlayerStatus(FFACETools.Status.Standing);
        //                return true;
        //            }
        //            else if (chatKbn == ChatKbnKind.Release)//リリース
        //            {
        //                oFish.Result = FishResultStatusKind.Release;
        //                //データベースへの登録
        //                if (!putDatabase(oFish)) return false;
        //                //連続釣果無しカウントクリア
        //                noCatchCount = 0;
        //                setMessage(string.Format("釣果：リリース {0}", GetViewFishName(oFish.FishName, oFish.FishType, oFish.FishCount, oFish.Critical)));
        //                //イベント発生
        //                EventFished(oFish.Result);
        //                //プレイヤステータスがStandingになるまで待つ
        //                waitChangePlayerStatus(FFACETools.Status.Standing);
        //                return true;
        //            }
        //            else if (chatKbn == ChatKbnKind.NoCatch)//逃げられた
        //            {
        //                oFish.Result = FishResultStatusKind.NoCatch;
        //                //データベースへの登録
        //                if (!putDatabase(oFish)) return false;
        //                //連続釣果無しカウントクリア
        //                noCatchCount = 0;
        //                setMessage(string.Format("釣果：逃げられた {0}", GetViewFishName(oFish.FishName, oFish.FishType, oFish.FishCount, oFish.Critical)));
        //                //イベント発生
        //                EventFished(oFish.Result);
        //                //プレイヤステータスがStandingになるまで待つ
        //                waitChangePlayerStatus(FFACETools.Status.Standing);
        //                return true;
        //            }
        //            else if (chatKbn == ChatKbnKind.LineBreak)//糸切れ
        //            {
        //                oFish.Result = FishResultStatusKind.LineBreak;
        //                //データベースへの登録
        //                if (!putDatabase(oFish)) return false;
        //                //連続釣果無しカウントクリア
        //                noCatchCount = 0;
        //                setMessage(string.Format("釣果：糸切れ {0}", GetViewFishName(oFish.FishName, oFish.FishType, oFish.FishCount, oFish.Critical)));
        //                //イベント発生
        //                EventFished(oFish.Result);
        //                //プレイヤステータスがStandingになるまで待つ
        //                waitChangePlayerStatus(FFACETools.Status.Standing);
        //                return true;
        //            }
        //            else if (chatKbn == ChatKbnKind.RodBreak)//竿折れ
        //            {
        //                oFish.Result = FishResultStatusKind.RodBreak;
        //                //データベースへの登録
        //                if (!putDatabase(oFish)) return false;
        //                //連続釣果無しカウントクリア
        //                noCatchCount = 0;
        //                setMessage(string.Format("釣果：竿折れ {0}", GetViewFishName(oFish.FishName, oFish.FishType, oFish.FishCount, oFish.Critical)));
        //                //イベント発生
        //                EventFished(oFish.Result);
        //                //プレイヤステータスがStandingになるまで待つ
        //                waitChangePlayerStatus(FFACETools.Status.Standing);
        //                return true;
        //            }
        //            else if (chatKbn == ChatKbnKind.BaitCritical)//クリティカル
        //            {
        //                oFish.Critical = true;
        //            }
        //            else if (oChatReceive) //チャット感知
        //            {
        //                //プレイヤステータスがStandingになるまで待つ
        //                while (this.PlayerStatus != FFACETools.Status.Standing)
        //                {
        //                    fface.Windower.SendKeyPress(KeyCode.EscapeKey);
        //                    Thread.Sleep(settings.Global.WaitBase);
        //                }
        //                return true;
        //            }
        //            else if (oEnemyAttack) //敵の攻撃感知
        //            {
        //                //プレイヤステータスがStandingになるまで待つ
        //                while (this.PlayerStatus != FFACETools.Status.Standing)
        //                {
        //                    fface.Windower.SendKeyPress(KeyCode.EscapeKey);
        //                    Thread.Sleep(settings.Global.WaitBase);
        //                }
        //                return true;
        //            }
        //        }
        //        Thread.Sleep(settings.Global.WaitChat);
        //    }

        //    return true;

        //}

        /// <summary>
        /// FishDB・FishHistoryDBへの登録処理
        /// </summary>
        /// <param name="iFish">FishHistoryDBFishModel</param>
        /// <returns></returns>
        private bool putDatabase(HarakiriDBHistoryModel iHistory)
        {
            ////FishDBに登録
            //if (iFish.ID1 != 0 && iFish.ID2 != 0 && iFish.ID3 != 0 && iFish.ID4 != 0)
            //{
            //    //FishDBから魚情報取得（不明魚以外で）
            //    FishDBFishModel fish = fishDB.SelectFishFromIDName(iFish.RodName, iFish.ID1, iFish.ID2, iFish.ID3, iFish.ID4, iFish.FishName, false);
            //    FishDBIdModel id = fish.GetId(iFish.ID1, iFish.ID2, iFish.ID3, iFish.ID4);
            //    //FishCount Critical Wanted
            //    if (fish.FishName != string.Empty)
            //    {
            //        iFish.FishCount = id.Count;
            //        iFish.Critical = id.Critical;
            //    }
            //    //FishTypeの設定
            //    if(!isTmpFishFromName(iFish.FishName))
            //    {
            //        if (iFish.FishType == FishDBFishTypeKind.UnknownSmallFish) iFish.FishType = FishDBFishTypeKind.SmallFish;
            //        if (iFish.FishType == FishDBFishTypeKind.UnknownLargeFish) iFish.FishType = FishDBFishTypeKind.LargeFish;
            //        if (iFish.FishType == FishDBFishTypeKind.UnknownItem) iFish.FishType = FishDBFishTypeKind.Item;
            //    }
            //    //登録情報の設定 
            //    FishDBFishModel fishDBFish = new FishDBFishModel();
            //    fishDBFish.FishName = iFish.FishName;
            //    FishDBIdModel fishDBId = new FishDBIdModel();
            //    fishDBId.ID1 = iFish.ID1;
            //    fishDBId.ID2 = iFish.ID2;
            //    fishDBId.ID3 = iFish.ID3;
            //    fishDBId.ID4 = iFish.ID4;
            //    fishDBId.Count = iFish.FishCount;
            //    fishDBId.Critical = iFish.Critical;
            //    fishDBFish.IDs.Add(fishDBId);
            //    fishDBFish.FishType = iFish.FishType;
            //    fishDBFish.ZoneNames.Add(iFish.ZoneName);
            //    fishDBFish.BaitNames.Add(iFish.BaitName);
            //    if (!fishDB.UpdateFish(iFish.RodName, fishDBFish))
            //    {
            //        setMessage("FishDBデータベースへの登録に失敗");
            //        return false;
            //    } 
            //}
            ////FishHistoryDBに登録
            //if (!harakiriDB.Add(this.PlayerName, iFish))
            //{
            //    setMessage("FishHistoryDBデータベースへの登録に失敗");
            //    return false;
            //}
            return true;
        }
        /// <summary>
        /// チャット内容からChatKbnKindを取得する
        /// </summary>
        /// <param name="iCl">チャットライン</param>
        /// <returns>チャット区分</returns>
        private ChatKbnKind getChatKbnFromChatline(FFACE.ChatTools.ChatLine iCl, out List<string> oArgs)
        {
            oArgs = new List<string>();
            foreach (KeyValuePair<ChatKbnKind, string> v in dictionaryChat)
            {
                string searchStr = v.Value;
                if (MiscTool.IsRegexString(iCl.Text, searchStr))
                {
                    oArgs = MiscTool.GetRegexString(iCl.Text, searchStr);
                    return v.Key;
                }
            }
            return ChatKbnKind.Unknown;
        }
        #endregion

        #region 装備アイテム
        /// <summary>
        /// アイテムを鞄へ移動する
        /// </summary>
        /// <param name="iItemName"></param>
        /// <returns></returns>
        private bool getItem(string iItemName)
        {
            bool moveOkFlg = false;
            //if (settings.UseItemizer)
            //{
            //    if (!moveOkFlg && settings.Fishing.NoBaitNoRodSatchel)
            //        if (control.GetItem(iItemName, InventoryType.Satchel))
            //        {
            //            moveOkFlg = true;
            //            setMessage(string.Format("{0}を{1}から取り出しました", iItemName, InventoryType.Satchel.ToString()));
            //            Thread.Sleep(1000);
            //        }
            //    if (!moveOkFlg && settings.Fishing.NoBaitNoRodSack)
            //        if (control.GetItem(iItemName, InventoryType.Sack))
            //        {
            //            moveOkFlg = true;
            //            setMessage(string.Format("{0}を{1}から取り出しました", iItemName, InventoryType.Sack.ToString()));
            //            Thread.Sleep(1000);
            //        }
            //    if (!moveOkFlg && settings.Fishing.NoBaitNoRodCase)
            //        if (control.GetItem(iItemName, InventoryType.Case))
            //        {
            //            moveOkFlg = true;
            //            setMessage(string.Format("{0}を{1}から取り出しました", iItemName, InventoryType.Case.ToString()));
            //            Thread.Sleep(1000);
            //        }
            //}
            return moveOkFlg;
        }
        /// <summary>
        /// 魚を鞄から移動する
        /// </summary>
        /// <returns></returns>
        private bool putFish()
        {
            bool moveOkFlg = false;
            //if (settings.UseItemizer)
            //{
            //    if (!moveOkFlg && settings.Fishing.InventoryFullSatchel) moveOkFlg = putFish(InventoryType.Satchel);
            //    if (!moveOkFlg && settings.Fishing.InventoryFullSack) moveOkFlg = putFish(InventoryType.Sack);
            //    if (!moveOkFlg && settings.Fishing.InventoryFullCase) moveOkFlg = putFish(InventoryType.Case);
            //}
            return moveOkFlg;
        }
        /// <summary>
        /// 指定した場所へ魚を移動する
        /// </summary>
        /// <param name="iInventoryType"></param>
        /// <returns></returns>
        private bool putFish(InventoryType iInventoryType)
        {
            //if (control.GetInventoryCountByType(iInventoryType) >= control.GetInventoryMaxByType(iInventoryType)) return false;
            //List<FishDBFishModel> fishes = fishDB.SelectFishList(this.RodName, string.Empty, string.Empty);
            //foreach (FishDBFishModel fish in fishes)
            //{
            //    if (control.IsExistItem(fish.FishName, InventoryType.Inventory))
            //    {
            //        control.PutItem(fish.FishName, iInventoryType);
            //        setMessage(string.Format("{0}を{1}に移動しました", fish.FishName, iInventoryType.ToString()));
            //        Thread.Sleep(1000);
            //        return true;
            //    }
            //}
            return false;
        }
        #endregion

        #region ステータス・メッセージ 
        private void setRunningStatus(RunningStatusKind iRunningStatus)
        {
            if (this.RunningStatus == iRunningStatus) return;
            this.RunningStatus = iRunningStatus;
            EventChangeStatus(iRunningStatus, this.HarakiriStatus);
        }
        private void setHarakiriStatus(HarakiriStatusKind iFishingStatus)
        {
            if (this.HarakiriStatus == iFishingStatus) return;
            this.HarakiriStatus = iFishingStatus;
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

    }
}
