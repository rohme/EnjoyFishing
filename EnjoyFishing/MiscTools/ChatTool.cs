using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EliteMMO.API;
using NLog;
using EnjoyFishing;

namespace MiscTools
{
    public class ChatTool
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private const int MAX_CHATLINE_INDEX = 100;//保持するチャット行数
        private const int CHAT_INTERVAL = 100;//チャット取得インターバル
        private const string REGEX_EMINENCE1 = "エミネンス・レコード：『(.*)』……";
        private const string REGEX_EMINENCE2 = "進行度：([0-9]*)/([0-9]*)";

        private List<EliteAPI.ChatEntry> chatLines = new List<EliteAPI.ChatEntry>();
        private int maxIndex = 0;
        private int currentIndex = 0;
        private Thread thChat;
        private EliteAPI api;

        #region コンストラクタ
        public ChatTool(EliteAPI iEliteAPI)
        {
            api = iEliteAPI;
            Start();
        }
        #endregion

        #region メンバ
        /// <summary>
        /// CurrentIndex
        /// </summary>
        public int CurrentIndex
        {
            get { return this.currentIndex; }
            set { this.currentIndex = value; }
        }
        /// <summary>
        /// MaxIndex
        /// </summary>
        public int MaxIndex
        {
            get { return this.maxIndex; }
        }
        #endregion

        #region イベント
        #region ReceivedCommand
        /// <summary>
        /// ReceivedCommandイベントで返されるデータ
        /// </summary>
        public class ReceivedCommandEventArgs : EventArgs
        {
            public List<string> Command;
        }
        public delegate void ReceivedCommandEventHandler(object sender, ReceivedCommandEventArgs e);
        public event ReceivedCommandEventHandler ReceivedCommand;
        protected virtual void OnReceivedCommand(ReceivedCommandEventArgs e)
        {
            if (ReceivedCommand != null)
            {
                ReceivedCommand(this, e);
            }
        }
        private void EventReceivedCommand(List<string> iCommand)
        {
            //返すデータの設定
            ReceivedCommandEventArgs e = new ReceivedCommandEventArgs();
            e.Command = iCommand;
            //イベントの発生
            OnReceivedCommand(e);
        }
        #endregion
        #endregion

        #region スレッド操作など
        /// <summary>
        /// ChatTool終了処理
        /// </summary>
        public void SystemAbort()
        {
            if (thChat != null && thChat.IsAlive)
            {
                thChat.Abort();
                thChat = null;
            }
        }
        /// <summary>
        /// チャット監視の開始
        /// </summary>
        public void Start()
        {
            //スレッド開始
            if (thChat == null || !thChat.IsAlive)
            {
                thChat = new Thread(threadChat);
                thChat.IsBackground = true;
                thChat.Start();
            }
        }
        /// <summary>
        /// チャット監視の停止
        /// </summary>
        public void Stop()
        {
            if (thChat != null && thChat.IsAlive)
            {
                thChat.Abort();
                thChat = null;
            }
        }
        /// <summary>
        /// カレントインデックスをリセットする
        /// </summary>
        public void Reset()
        {
            this.currentIndex = this.maxIndex;
        }
        #endregion

        /// <summary>
        /// 指定したインデックス以降のチャットを取得
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public List<EliteAPI.ChatEntry> GetChatLine(int index, bool iAddIndex = true)
        {
            List<EliteAPI.ChatEntry> ret = new List<EliteAPI.ChatEntry>();
            for (int i = 0; i < chatLines.Count; i++)
            {
                if (chatLines[i].Index1 > index)
                {
                    if (iAddIndex) this.currentIndex = chatLines[i].Index1;
                    ret.Add(chatLines[i]);
                }
            }
            return ret;
        }
        /// <summary>
        /// 次のチャットを取得
        /// </summary>
        /// <param name="oChatLine">チャットライン</param>
        /// <returns>True：次のチャットがある場合</returns>
        public bool GetNextChatLine(out EliteAPI.ChatEntry oChatLine)
        {
            for (int i = 0; i < chatLines.Count; i++)
            {
                if (chatLines[i].Index1 > this.currentIndex)
                {
                    this.currentIndex = chatLines[i].Index1;
                    oChatLine = chatLines[i];
                    return true;
                }
            }
            oChatLine = new EliteAPI.ChatEntry();
            return false;
        }

        public void Clear()
        {
            chatLines.Clear();
        }
        /// <summary>
        /// チャット監視メインスレッド
        /// </summary>
        private void threadChat()
        {
            Thread.Sleep(2000);
            while (true)
            {
                if (api != null &&
                    api.Player.LoginStatus == (int)LoginStatus.LoggedIn)
                {
                    updateChatLine();
                }
                Thread.Sleep(CHAT_INTERVAL);
            }
        }
        /// <summary>
        /// 取得したチャットをchatLinesに設定
        /// </summary>
        private void updateChatLine()
        {
            //int currChatLineIndex = 0;
            //int lastEminenceIndex = -1;
            if (api == null || api.Player.LoginStatus != (int)LoginStatus.LoggedIn) return;

            EliteAPI.ChatEntry cl;
            EliteAPI.ChatEntry buff = null;
            while ((cl = api.Chat.GetNextChatLine()) != null)
            {
                if (buff == null)
                {
                    buff = cl;
                    continue;
                }
                if (buff.Index1 == cl.Index1 || MiscTool.IsRegexString(cl.Text, REGEX_EMINENCE2))
                {
                    buff.Text += cl.Text;
                }
                else
                {
                    AddChatLines(buff);
                    buff = cl;
                }
            }
            if (buff != null) AddChatLines(buff);

            if (chatLines.Count > MAX_CHATLINE_INDEX)
            {
                chatLines.RemoveRange(0, chatLines.Count - MAX_CHATLINE_INDEX);
            }
        }
        private void AddChatLines(EliteAPI.ChatEntry iCl)
        {
            logger.Trace("チャット追加：{0}", iCl.Text);
            chatLines.Add(iCl);

            //コマンド受信イベント処理
            if (iCl.ChatType == (int)ChatMode.Echo && iCl.Text.Length > 0)
            {
                string[] cmd = iCl.Text.Split(' ');
                if (cmd.Length >= 1 && cmd[0].ToLower() == "enjoyfishing" && cmd.Length > 1)
                {
                    EventReceivedCommand(cmd.ToList().GetRange(1, cmd.Length - 1));
                }
            }
        }
    }
}
