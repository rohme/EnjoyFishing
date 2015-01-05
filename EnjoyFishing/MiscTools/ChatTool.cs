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
        private const string REGEX_YELL = @"(.*)\[(.*)\]: (.*)";
        private const string REGEX_SHOUT = @"(.*) : (.*)";
        private const string REGEX_LINKSHELL = @"[(\[0-9\])]<(.*)> (.*)";
        private const string REGEX_PARTY = @"\((.*)\) (.*)";
        private const string REGEX_SAY = @"(.*) : (.*)";
        private const string REGEX_SENT_TELL = @"\>\>(.*) : (.*)";
        private const string REGEX_RCVD_TELL = @"(.*)\>\> (.*)";
        private const int MAX_CHATLINE_INDEX = 100;//保持するチャット行数
        private const int CHAT_INTERVAL = 100;//チャット取得インターバル

        private List<FFACE.ChatTools.ChatLine> chatLines = new List<FFACE.ChatTools.ChatLine>();
        private int maxIndex = 0;
        private int currentIndex = 0;
        private Thread thChat;
        private FFACE fface;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="iFFACE"></param>
        public ChatTool(FFACE iFFACE)
        {
            fface = iFFACE;
            Start();
        }

        #region Getter Setter
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
        /// <summary>
        /// 指定したインデックス以降のチャットを取得
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public List<FFACE.ChatTools.ChatLine> GetChatLine(int index)
        {
            List<FFACE.ChatTools.ChatLine> ret = new List<FFACE.ChatTools.ChatLine>();
            for (int i = 0; i < chatLines.Count; i++)
            {
                if (chatLines[i].Index > index)
                {
                    this.currentIndex = chatLines[i].Index;
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
                    Thread.Sleep(CHAT_INTERVAL);
                }
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
                if (fface != null && fface.Player.GetLoginStatus == LoginStatus.LoggedIn)
                {
                    if (fface.Chat.IsNewLine)
                    {
                        FFACE.ChatTools.ChatLine cl = fface.Chat.GetNextLine();
                        while (cl != null)
                        {
                            if (IsNewLine(cl))
                            {
                                chatLines.Add(cl);
                                currChatLineIndex = chatLines.Count - 1;
                            }
                            else
                            {
                                chatLines[currChatLineIndex].Text = chatLines[currChatLineIndex].Text + cl.Text;
                            }
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
        /// <summary>
        /// １行目のログか否か
        /// </summary>
        /// <param name="cl"></param>
        /// <returns></returns>
        private bool IsNewLine(FFACE.ChatTools.ChatLine cl)
        {
            if (cl.Type == ChatMode.SentYell || cl.Type == ChatMode.RcvdYell)
            {
                return MiscTool.IsRegexString(cl.Text, REGEX_YELL);
            }
            if (cl.Type == ChatMode.SentShout || cl.Type == ChatMode.RcvdShout)
            {
                return MiscTool.IsRegexString(cl.Text, REGEX_SHOUT);
            }
            if (cl.Type == ChatMode.SentLinkShell || cl.Type == ChatMode.RcvdLinkShell ||
                (short)cl.Type == 213)
            {
                return MiscTool.IsRegexString(cl.Text, REGEX_LINKSHELL);
            }
            if (cl.Type == ChatMode.SentParty || cl.Type == ChatMode.RcvdParty)
            {
                return MiscTool.IsRegexString(cl.Text, REGEX_PARTY);
            }
            if (cl.Type == ChatMode.SentSay || cl.Type == ChatMode.RcvdSay)
            {
                return MiscTool.IsRegexString(cl.Text, REGEX_SAY);
            }
            if (cl.Type == ChatMode.SentTell)
            {
                return MiscTool.IsRegexString(cl.Text, REGEX_SENT_TELL);
            }
            if (cl.Type == ChatMode.RcvdTell)
            {
                return MiscTool.IsRegexString(cl.Text, REGEX_RCVD_TELL);
            }
            return true;
        }
    }
}
