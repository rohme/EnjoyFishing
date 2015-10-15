using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FFACETools;
namespace MiscTools
{
    public class ChatTool
    {
        private const int MAX_CHATLINE_INDEX = 100;//保持するチャット行数
        private const int CHAT_INTERVAL = 100;//チャット取得インターバル
        private const string REGEX_EMINENCE1 = "エミネンス・レコード：『(.*)』……";
        private const string REGEX_EMINENCE2 = "進行度：([0-9]*)/([0-9]*)";

        private List<FFACE.ChatTools.ChatLine> chatLines = new List<FFACE.ChatTools.ChatLine>();
        private int maxIndex = 0;
        private int currentIndex = 0;
        private Thread thChat;
        private FFACE fface;
        private string lastIndex1 = ""; //一行目か判定用

        #region コンストラクタ
        public ChatTool(FFACE iFFACE)
        {
            fface = iFFACE;

            //過去のチャットをクリア
            FFACE.ChatTools.ChatLine cl = fface.Chat.GetNextLine();
            while (cl != null)
            {
                cl = fface.Chat.GetNextLine();
            }

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
        public List<FFACE.ChatTools.ChatLine> GetChatLine(int index, bool iAddIndex = true)
        {
            List<FFACE.ChatTools.ChatLine> ret = new List<FFACE.ChatTools.ChatLine>();
            for (int i = 0; i < chatLines.Count; i++)
            {
                if (chatLines[i].Index > index)
                {
                    if (iAddIndex) this.currentIndex = chatLines[i].Index;
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
        public bool GetNextChatLine(out FFACE.ChatTools.ChatLine oChatLine)
        {
            for (int i = 0; i < chatLines.Count; i++)
            {
                if (chatLines[i].Index > this.currentIndex)
                {
                    this.currentIndex = chatLines[i].Index;
                    oChatLine = chatLines[i];
                    return true;
                }
            }
            oChatLine = new FFACE.ChatTools.ChatLine();
            return false;
        }
        /// <summary>
        /// チャット監視メインスレッド
        /// </summary>
        private void threadChat()
        {
            Thread.Sleep(2000);
            while (true)
            {
                if (fface != null && 
                    fface.Player.GetLoginStatus == LoginStatus.LoggedIn)
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
            try
            {
                int currChatLineIndex = 0;
                int lastEminenceIndex = -1;
                if (fface != null && fface.Player.GetLoginStatus == LoginStatus.LoggedIn)
                {
                    if (fface.Chat.IsNewLine)
                    {
                        FFACE.ChatTools.ChatLine cl = fface.Chat.GetNextLine();
                        while (cl != null)
                        {
                            //既に登録されているチャットか判定
                            bool foundFlg = false;
                            foreach (var chkCl in chatLines)
                            {
                                if (chkCl.RawString[4] == cl.RawString[4] &&
                                    chkCl.RawString[5] == cl.RawString[5] &&
                                    chkCl.Type == cl.Type
                                    )
                                {
                                    foundFlg = true;
                                    break;
                                }
                            }
                            if(!foundFlg)
                            {
                                //コマンド受信イベント処理
                                if(cl.Type == ChatMode.Echo && cl.Text.Length > 0){
                                    string[] cmd = cl.Text.Split(' ');
                                    if (cmd[0].ToLower() == "enjoyfishing" && cmd.Length > 1)
                                    {
                                        List<string> retcmd = new List<string>();
                                        for (int i = 1; i < cmd.Length; i++)
                                        {
                                            retcmd.Add(cmd[i].ToLower());
                                        }
                                        EventReceivedCommand(retcmd);
                                    }
                                }
                                //チャットが複数行に渡ってある場合、一行にまとめる処理
                                string[] stArrayData = cl.RawString[12].Split(',');
                                if (cl.RawString[4] != lastIndex1) //1行目か否か
                                {
                                    //エミネンス
                                    if (lastEminenceIndex > 0)
                                    {
                                        if (MiscTool.IsRegexString(cl.Text, REGEX_EMINENCE2))
                                        {
                                            chatLines[lastEminenceIndex].Text = chatLines[lastEminenceIndex].Text + cl.Text;
                                        }
                                    }
                                    else
                                    {
                                        chatLines.Add(cl);
                                        currChatLineIndex = chatLines.Count - 1;
                                    }
                                    if (MiscTool.IsRegexString(cl.Text, REGEX_EMINENCE1))
                                    {
                                        lastEminenceIndex = currChatLineIndex;
                                    }
                                    else
                                    {
                                        lastEminenceIndex = -1;
                                    }
                                }
                                else
                                {
                                    chatLines[currChatLineIndex].Text = chatLines[currChatLineIndex].Text + cl.Text;
                                }
                            }
                            lastIndex1 = cl.RawString[4];
                            maxIndex = cl.Index;
                            cl = fface.Chat.GetNextLine();
                        }
                        if (chatLines.Count > MAX_CHATLINE_INDEX)
                        {
                            chatLines.RemoveRange(0, chatLines.Count - MAX_CHATLINE_INDEX);
                        }
                    }
                }
            }
            catch(NullReferenceException e)
            {
                Console.WriteLine("ChatTool " + e.Message);
                return;
            }
        }
    }
}
