using System;
using System.Collections.Generic;
using System.Threading;
using EliteMMO.API;
using MiscTools;
using NLog;

namespace EnjoyFishing
{
    public class HarakiriTool
    {
        #region Dictionary
        private static Dictionary<ChatKbnKind, string> dictionaryChat = new Dictionary<ChatKbnKind, string>()
        {
            {ChatKbnKind.Zaldon, "Zaldon : おお、持ってきたのか。"},
            {ChatKbnKind.Found, "Zaldon : おおっ！？あった、あった！腹の中から(.*)が出てきたぜ！"},
            {ChatKbnKind.NotFound, "Zaldon : んー、残念だったな。"},
        };
        #endregion

        #region Enum
        private enum ChatKbnKind
        {
            Zaldon,
            Found,
            NotFound,
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
        private EliteAPI api;
        private Settings settings;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private ChatTool chat;
        private ResourceTool resource;
        private EliteAPIControl control;
        private Thread thHarakiri;
        private FishDB fishDB;
        private HarakiriDB harakiriDB;


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
        public string HarakiriFishName 
        { 
            get; 
            set; 
        }
        #endregion

        #region イベント
        #region Fished
        /// <summary>
        /// Fishedeイベントで返されるデータ
        /// </summary>
        public class HarakiriOnceEventArgs : EventArgs
        {
            public string FishName;
            public string ItemName;
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
        private void EventHarakiriOnce(string iFishName, string iItemName)
        {
            //返すデータの設定
            HarakiriOnceEventArgs e = new HarakiriOnceEventArgs();
            e.FishName = iFishName;
            e.ItemName = iItemName;
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
        /// <param name="iPol"></param>
        /// <param name="iChat"></param>
        /// <param name="iSettings"></param>
        public HarakiriTool(PolTool iPol, ResourceTool iResource, ChatTool iChat, Settings iSettings)
        {
            pol = iPol;
            api = iPol.EliteAPI;
            chat = iChat;
            resource = iResource;
            settings = iSettings;
            fishDB = new FishDB();
            harakiriDB = new HarakiriDB();
            control = new EliteAPIControl(pol, resource, chat);
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
        public HarakiriStatusKind HarakiriStart()
        {
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
            for (int i = 0; i < Constants.MAX_LOOP_COUNT && api.Player.Status != (uint)Status.Standing; i++)
            {
                api.ThirdParty.KeyPress(Keys.ESCAPE);
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
            bool firsttime = true;

            logger.Debug("ハラキリスレッド開始");
            chat.Reset();
            while (this.RunningStatus == RunningStatusKind.Running)
            {
                //未入力チェック
                if (this.HarakiriFishName.Length == 0)
                {
                    setRunningStatus(RunningStatusKind.Stop);
                    setHarakiriStatus(HarakiriStatusKind.Error);
                    setMessage("ハラキリする魚を入力してください");
                    return;
                }
                //鞄に入っている魚の総数を取得
                uint itemId = resource.GetItem(this.HarakiriFishName).ItemID;
                int remain = GetHarakiriRemain(this.HarakiriFishName);
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
                setMessage(string.Format("ハラキリ中：{0} 残り{1}匹", this.HarakiriFishName, remain));
                //鞄に対象の魚を移動させる
                if (control.GetInventoryItemCount(itemId, InventoryType.Inventory) <= 0)
                {
                    if (control.GetInventoryItemCount(itemId, InventoryType.Satchel) > 0)
                    {
                        control.GetItemizer(this.HarakiriFishName, InventoryType.Satchel);
                    }
                    else if (control.GetInventoryItemCount(itemId, InventoryType.Sack) > 0)
                    {
                        control.GetItemizer(this.HarakiriFishName, InventoryType.Sack);
                    }
                    else if (control.GetInventoryItemCount(itemId, InventoryType.Case) > 0)
                    {
                        control.GetItemizer(this.HarakiriFishName, InventoryType.Case);
                    }
                }
                //Zaldonの近くかチェック
                if (api.Player.ZoneId != (int)Zone.Selbina ||
                    (api.Entity.GetEntity(NPCID_ZALDON).Distance != 0f && api.Entity.GetEntity(NPCID_ZALDON).Distance > 6))
                {
                    setRunningStatus(RunningStatusKind.Stop);
                    setHarakiriStatus(HarakiriStatusKind.Error);
                    setMessage("セルビナのZaldonの近くで実行してください");
                    return;
                }
                //メニュー開いていたら閉じる
                if (!control.CloseDialog(10))
                {
                    setRunningStatus(RunningStatusKind.Stop);
                    setHarakiriStatus(HarakiriStatusKind.Error);
                    setMessage("エラー：会話を終了させてから実行してください");
                    break;
                }

                //ターゲット設定
                control.SetTargetFromId(NPCID_ZALDON);
                Thread.Sleep(settings.Global.WaitChat);//Wait
                //プレイヤーステータスがstandingになるまで待機
                while(api.Player.Status != (uint)Status.Standing){
                    Thread.Sleep(settings.Global.WaitBase);//wait
                }
                //アイテムトレード
                api.ThirdParty.SendString(string.Format("/item {0} <t>", this.HarakiriFishName));

                //チャット監視開始
                string itemName = string.Empty;
                int noResponseCount = 0;
                var cl = new EliteAPI.ChatEntry();
                while (this.RunningStatus == RunningStatusKind.Running)
                {
                    if (!chat.GetNextChatLine(out cl)) 
                    {
                        noResponseCount++;
                        if (noResponseCount > 10)
                        {
                            setRunningStatus(RunningStatusKind.Stop);
                            setHarakiriStatus(HarakiriStatusKind.Error);
                            setMessage("ハラキリ対象の魚ですか？ 反応無し中止");
                            break;
                        }
                        Thread.Sleep(settings.Global.WaitChat);//wait
                        continue; 
                    } 
                    //チャット区分の取得
                    List<string> chatKbnArgs = new List<string>();
                    ChatKbnKind chatKbn = getChatKbnFromChatline(cl, out chatKbnArgs);
                    logger.Debug("Chat:{0} ChatKbn:{1}", cl.Text, chatKbn);
                    if (chatKbn == ChatKbnKind.Zaldon)
                    {
                        noResponseCount = 0;
                        if (!settings.UseEnternity)
                        {
                            api.ThirdParty.KeyPress(Keys.RETURN);
                        }
                    }
                    else if (chatKbn == ChatKbnKind.NotFound)
                    {
                        noResponseCount = 0;
                        if (!putDatabase(itemName))
                        {
                            setRunningStatus(RunningStatusKind.Stop);
                            setHarakiriStatus(HarakiriStatusKind.Error);
                            setMessage("データベースの更新に失敗しました");
                            break;
                        }
                        EventHarakiriOnce(this.HarakiriFishName, itemName);//イベント発生
                        setMessage("ハラキリ結果：何も見つからなかった");
                        if (!settings.UseEnternity)
                        {
                            api.ThirdParty.KeyPress(Keys.RETURN);
                        }
                    }
                    else if (chatKbn == ChatKbnKind.Found)
                    {
                        noResponseCount = 0;
                        itemName = chatKbnArgs[0];
                        if (!putDatabase(itemName))
                        {
                            setRunningStatus(RunningStatusKind.Stop);
                            setHarakiriStatus(HarakiriStatusKind.Error);
                            setMessage("データベースの更新に失敗しました");
                            break;
                        }
                        EventHarakiriOnce(this.HarakiriFishName, itemName);//イベント発生
                        setMessage(string.Format("ハラキリ結果：{0}を見つけた", itemName));
                        api.ThirdParty.KeyPress(Keys.RETURN);
                        Thread.Sleep(settings.Global.WaitChat);
                    }
                    if (chatKbn == ChatKbnKind.Found || chatKbn == ChatKbnKind.NotFound)
                    {
                        noResponseCount = 0;
                        Thread.Sleep(2000);
                        //発見したら停止
                        if (chatKbn == ChatKbnKind.Found && settings.Harakiri.StopFound)
                        {
                            setRunningStatus(RunningStatusKind.Stop);
                            setHarakiriStatus(HarakiriStatusKind.Normal);
                            setMessage("アイテムを発見したので停止します");
                        }
                        break;
                    }
                    Thread.Sleep(settings.Global.WaitBase);//wait
                }
                Thread.Sleep(settings.Global.WaitChat);//wait
            }
            logger.Debug("ハラキリスレッド終了");
        }

        private bool putDatabase(string iItemName)
        {
            string vanadate = string.Format("{0:0000}/{1:00}/{2:00} {3:00}:{4:00}:{5:00}",
                                                                        api.VanaTime.CurrentYear,
                                                                        api.VanaTime.CurrentMonth,
                                                                        api.VanaTime.CurrentDay,
                                                                        api.VanaTime.CurrentHour,
                                                                        api.VanaTime.CurrentMinute,
                                                                        api.VanaTime.CurrentSecond);
            return harakiriDB.Add(api.Player.Name, DateTime.Now, vanadate, this.HarakiriFishName, iItemName);
        }
        /// <summary>
        /// チャット内容からChatKbnKindを取得する
        /// </summary>
        /// <param name="iCl">チャットライン</param>
        /// <returns>チャット区分</returns>
        private ChatKbnKind getChatKbnFromChatline(EliteAPI.ChatEntry iCl, out List<string> oArgs)
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

        /// <summary>
        /// ハラキリ対象の魚の残数を取得
        /// </summary>
        /// <param name="iFishName">魚名称</param>
        /// <returns>残数</returns>
        public int GetHarakiriRemain(string iFishName)
        {
            uint itemId = resource.GetItem(iFishName).ItemID;
            int remain = control.GetInventoryItemCount(itemId, InventoryType.Inventory);
            if (settings.UseItemizer)
            {
                remain += control.GetInventoryItemCount(itemId, InventoryType.Satchel);
                remain += control.GetInventoryItemCount(itemId, InventoryType.Sack);
                remain += control.GetInventoryItemCount(itemId, InventoryType.Case);
            }
            return remain;
        }

        #region ステータス・メッセージ 
        private void setRunningStatus(RunningStatusKind iRunningStatus)
        {
            if (this.RunningStatus == iRunningStatus) return;
            this.RunningStatus = iRunningStatus;
            EventChangeStatus(iRunningStatus, this.HarakiriStatus);
        }
        private void setHarakiriStatus(HarakiriStatusKind iHarakiriStatus)
        {
            if (this.HarakiriStatus == iHarakiriStatus) return;
            this.HarakiriStatus = iHarakiriStatus;
            EventChangeStatus(this.RunningStatus, iHarakiriStatus);
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
