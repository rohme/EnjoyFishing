using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using FFACETools;
using MiscTools;
using System.IO;
using System.Threading;

namespace MiscTools
{
    public class FFACEControl
    {
        private const int DEFAULT_MAX_LOOP_COUNT = 100;
        private const bool DEFAULT_USE_ENTERNITY = true;
        private const int DEFAULT_BASE_WAIT = 300;
        private const int DEFAULT_CHAT_WAIT = 1000;
        private const string REGEX_PLUGIN = "(.*) \\(author: (.*)";
        //private const string REGEX_PLUGIN_END = "=== Done Listing Currently Loaded Plugins ===";
        private const string REGEX_PLUGIN_END = "=== Done Listing (.*)";
        private const string REGEX_ADDON = "([0-9]*):([0-9]*):([0-9]*) >   (.*)";
        private const string REGEX_ADDON_KEY = "([0-9]*):([0-9]*):([0-9]*) > {0}";

        private PolTool pol = null;
        private FFACE fface = null;
        private ChatTool chat = null;
        private LoggerTool logger = null;
        
        #region メンバ
        public int MaxLoopCount { get; set; }
        public bool UseEnternity { get; set; }
        public int BaseWait { get; set; }
        public int ChatWait { get; set; }
        #endregion

        #region コンストラクタ
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FFACEControl(PolTool iPOL, ChatTool iChat, LoggerTool iLogger)
        {
            this.pol = iPOL;
            this.fface = iPOL.FFACE;
            this.chat = iChat;
            this.logger = iLogger;
            this.MaxLoopCount = DEFAULT_MAX_LOOP_COUNT;
            this.UseEnternity = DEFAULT_USE_ENTERNITY;
            this.BaseWait = DEFAULT_BASE_WAIT;
            this.ChatWait = DEFAULT_CHAT_WAIT;
        }
        #endregion

        #region チャット関連
        /// <summary>
        /// 指定された文字列がチャットに表示されるまで待機
        /// </summary>
        /// <param name="iRegexString">検索文字列</param>
        /// <param name="iWithEnter">True:エンターキーを連打する</param>
        /// <returns>True:見つかった False:見つからなかった</returns>
        public bool WaitChat(ChatTool iChatTool, string iRegexString, int iStartChatIndex, bool iWithEnter)
        {
            logger.Output(LogLevelKind.INFO, "WaitChat", string.Format("RegexString={0} StartChatIndex={1} WithEnter={1}", iRegexString, iStartChatIndex, iWithEnter));
            List<FFACE.ChatTools.ChatLine> arrChatLine;
            int currChatIndex = iStartChatIndex;
            for (int i = 0; (i < this.MaxLoopCount); i++)
            {
                arrChatLine = iChatTool.GetChatLine(currChatIndex);
                foreach (FFACE.ChatTools.ChatLine cl in arrChatLine)
                {
                    //チャットの判定
                    if (MiscTool.IsRegexString(cl.Text, iRegexString))
                    {
                        return true;
                    }

                }
                if (!this.UseEnternity && iWithEnter)
                {
                    if (this.fface.Target.ID != 0)
                        this.fface.Windower.SendKeyPress(KeyCode.EnterKey);///Enter
                }
                System.Threading.Thread.Sleep(this.ChatWait);
            }
            logger.Output(LogLevelKind.WARN, "WaitChat", "タイムアウトしました");
            return false;
        }
        #endregion

        #region ダイアログ関連
        /// <summary>
        /// 指定されたダイアログIDのダイアログが表示されるまで待つ
        /// </summary>
        /// <param name="iDialogString">ダイアログ文字列</param>
        /// <param name="iEnter">True:待ってる間Enter連打する False:Enter連打しない</param>
        /// <returns>True:ダイアログが表示された False:ダイアログが表示されなかった</returns>
        public bool WaitOpenDialog(string iDialogString, bool iEnter)
        {
            logger.Output(LogLevelKind.INFO, "WaitOpenDialog", string.Format("DialogString={0} Enter={1}", iDialogString, iEnter));
            for (int i = 0; (i < this.MaxLoopCount); i++)
            {
                Regex reg = new Regex(iDialogString, RegexOptions.IgnoreCase);
                Match ma = reg.Match(this.fface.Menu.DialogText.Question);
                if (this.fface.Menu.IsOpen && ma.Success)
                {
                    return true;
                }
                if (!this.UseEnternity && iEnter)
                {
                    this.fface.Windower.SendKeyPress(KeyCode.EnterKey);//ENTER
                }
                System.Threading.Thread.Sleep(this.BaseWait);
            }
            logger.Output(LogLevelKind.WARN, "WaitOpenDialog", "タイムアウトしました");
            return false;
        }
        /// <summary>
        /// 指定したダイアログインデックスへカーソルを移動させる
        /// </summary>
        /// <param name="iIdx"></param>
        /// <param name="iWithEnter"></param>
        /// <returns></returns>
        public bool SetDialogOptionIndex(short iIdx, bool iWithEnter)
        {
            logger.Output(LogLevelKind.INFO, "SetDialogOptionIndex", string.Format("iIdx={0} iWithEnter={1}", iIdx, iWithEnter));
            for (int i = 0; i < this.MaxLoopCount; i++)
            {
                if (this.fface.Menu.DialogOptionIndex == iIdx)
                {
                    if (iWithEnter)
                    {
                        this.fface.Windower.SendKeyPress(KeyCode.EnterKey);///Enter
                    }
                    return true;
                }
                else if (this.fface.Menu.DialogOptionIndex > iIdx)
                {
                    if ((this.fface.Menu.DialogOptionIndex - iIdx) >= 3)
                    {
                        this.fface.Windower.SendKeyPress(KeyCode.LeftArrow);//右矢印
                    }
                    else
                    {
                        this.fface.Windower.SendKeyPress(KeyCode.UpArrow);//上矢印
                    }
                }
                else if (this.fface.Menu.DialogOptionIndex < iIdx)
                {
                    if ((iIdx - this.fface.Menu.DialogOptionIndex) >= 3)
                    {
                        this.fface.Windower.SendKeyPress(KeyCode.RightArrow);//左矢印
                    }
                    else
                    {
                        this.fface.Windower.SendKeyPress(KeyCode.DownArrow);//下矢印
                    }
                }
                System.Threading.Thread.Sleep(this.BaseWait);
            }
            logger.Output(LogLevelKind.WARN, "SetDialogOptionIndex", "タイムアウトしました");
            return false;
        }
        /// <summary>
        /// 選択されたOptionIndexを返す
        /// </summary>
        /// <returns></returns>
        public short GetSelectedOptionIndex()
        {
            short lastDialogId = this.fface.Menu.DialogID;
            short lastOptionIndex = 0;
            while (this.fface.Menu.DialogID == lastDialogId)
            {
                lastOptionIndex = this.fface.Menu.DialogOptionIndex;
                System.Threading.Thread.Sleep(10);
            }
            return lastOptionIndex;
        }
        /// <summary>
        /// メニューが閉じるまでエスケープキーを連打
        /// </summary>
        /// <returns></returns>
        public bool CloseDialog(int iTryCount = -1)
        {
            if (iTryCount == -1) iTryCount = this.MaxLoopCount;
            for (int i = 0; i < iTryCount; i++)
            {
                if (fface.Menu.IsOpen)
                {
                    fface.Windower.SendKeyPress(KeyCode.EscapeKey);
                    Thread.Sleep(this.BaseWait);
                }
                else
                {
                    return true;
                }
            }
            return false;
        }
        #endregion
        
        #region Plugin Addon関連
        /// <summary>
        /// 実行中のプラグイン名を取得する
        /// </summary>
        /// <returns>プラグイン名</returns>
        public List<string> GetPlugin()
        {
            if (pol.FFACE.Player.GetLoginStatus != LoginStatus.LoggedIn) return new List<string>();
            chat.Reset();
            fface.Windower.SendString("//plugin_list");
            List<string> ret = new List<string>();
            FFACE.ChatTools.ChatLine cl = new FFACE.ChatTools.ChatLine();
            for (int i = 0; i < this.MaxLoopCount && !MiscTool.IsRegexString(cl.Text, REGEX_PLUGIN_END); i++)
            {
                while (chat.GetNextChatLine(out cl))
                {
                    if (MiscTool.IsRegexString(cl.Text, REGEX_PLUGIN))
                    {
                        List<string> reg = MiscTool.GetRegexString(cl.Text, REGEX_PLUGIN);
                        string[] work = reg[0].Split(',');
                        ret.Add(work[work.Count() - 1]);
                    }
                    else if (MiscTool.IsRegexString(cl.Text, REGEX_PLUGIN_END))
                    {
                        break;
                    }

                }
                Thread.Sleep(this.BaseWait);
            }
            return ret;
        }
        /// <summary>
        /// 実行中のアドオン名を取得する
        /// </summary>
        /// <returns>アドオン名</returns>
        public List<string> GetAddon()
        {
            string key = "EnjoyFishing CheckAddon " + DateTime.Now.ToString("yyyyMMddhhmmss");
            List<string> addon = new List<string>();
            bool consoleLog = true;
            if (!getAddonFromKey(key, out addon))
            {
                consoleLog = false;
                fface.Windower.SendString("//console_log 1");
                getAddonFromKey(key, out addon);
            }
            if (!consoleLog)
            {
                fface.Windower.SendString("//console_log 0");
            }
            return addon;
        }
        /// <summary>
        /// 指定したキーが含まれる行以降のアドオン名を返す
        /// </summary>
        /// <param name="iKey"></param>
        /// <param name="oAddonList"></param>
        /// <returns>取得に成功した場合Trueを返す</returns>
        private bool getAddonFromKey(string iKey, out List<string> oAddonList)
        {
            string KeyStart = string.Format("{0} START", iKey);
            string KeyEnd = string.Format("{0} END", iKey);
            oAddonList = new List<string>();
            string fileNameOrg = GetWindowerPath() + "console.log";
            string fileName = GetWindowerPath() + "EnjoyFishing.log";
            if (!File.Exists(fileNameOrg)) return false;
            fface.Windower.SendString("//echo " + KeyStart);
            Thread.Sleep(this.BaseWait);
            fface.Windower.SendString("//lua list");
            Thread.Sleep(this.BaseWait);
            fface.Windower.SendString("//echo " + KeyEnd);
            Thread.Sleep(300);
            File.Copy(fileNameOrg, fileName, true);

            bool foundFlg = false;
            using (StreamReader sr = new StreamReader(fileName))
            {
                string line = string.Empty;
                while ((line = sr.ReadLine()) != null)
                {
                    if (MiscTool.IsRegexString(line, string.Format(REGEX_ADDON_KEY, KeyStart))) foundFlg = true;
                    else if (MiscTool.IsRegexString(line, string.Format(REGEX_ADDON_KEY, KeyEnd))) foundFlg = false;
                    if (foundFlg)
                    {
                        if (MiscTool.IsRegexString(line, REGEX_ADDON))
                        {
                            List<string> reg = MiscTool.GetRegexString(line, REGEX_ADDON);
                            oAddonList.Add(reg[3]);
                        }
                    }
                }
                sr.Close();
            }
            File.Delete(fileName);
            return foundFlg;
        }
        #endregion

        #region アイテム関連
        /// <summary>
        /// 指定された倉庫タイプのアイテム数を取得する
        /// </summary>
        /// <param name="iInventoryType">倉庫タイプ</param>
        /// <returns>アイテム数</returns>
        public short GetInventoryCountByType(InventoryType iInventoryType)
        {
            short cnt = 0;
            if (iInventoryType == InventoryType.Inventory) cnt = fface.Item.InventoryCount;
            else if (iInventoryType == InventoryType.Safe) cnt = fface.Item.SafeCount;
            else if (iInventoryType == InventoryType.Storage) cnt = fface.Item.StorageCount;
            else if (iInventoryType == InventoryType.Locker) cnt = fface.Item.LockerCount;
            else if (iInventoryType == InventoryType.Satchel) cnt = fface.Item.SatchelCount;
            else if (iInventoryType == InventoryType.Sack) cnt = fface.Item.SackCount;
            else if (iInventoryType == InventoryType.Temp) cnt = fface.Item.TemporaryCount;
            else if (iInventoryType == InventoryType.Case) cnt = fface.Item.CaseCount;
            else if (iInventoryType == InventoryType.Wardrobe) cnt = fface.Item.WardrobeCount;
            if (cnt > 0) return cnt;
            return 0;
        }
        /// <summary>
        /// 指定された倉庫のアイテムMAX数を取得する
        /// </summary>
        /// <param name="iInventoryType">倉庫タイプ</param>
        /// <returns>アイテムMAX数</returns>
        public short GetInventoryMaxByType(InventoryType iInventoryType)
        {
            short cnt = 0;
            if (iInventoryType == InventoryType.Inventory) cnt = fface.Item.InventoryMax;
            else if (iInventoryType == InventoryType.Safe) cnt = fface.Item.SafeMax;
            else if (iInventoryType == InventoryType.Storage) cnt = fface.Item.StorageMax;
            else if (iInventoryType == InventoryType.Locker) cnt = fface.Item.LockerMax;
            else if (iInventoryType == InventoryType.Satchel) cnt = fface.Item.SatchelMax;
            else if (iInventoryType == InventoryType.Sack) cnt = fface.Item.SackMax;
            else if (iInventoryType == InventoryType.Temp) cnt = fface.Item.TemporaryMax;
            else if (iInventoryType == InventoryType.Case) cnt = fface.Item.CaseMax;
            else if (iInventoryType == InventoryType.Wardrobe) cnt = fface.Item.WardrobeMax;
            if (cnt > 0) return cnt;
            else return 0;
        }
        /// <summary>
        /// Itemizerでアイテムを鞄に移動する
        /// </summary>
        /// <param name="iItemName">アイテム名</param>
        /// <param name="iInventoryType">倉庫タイプ</param>
        /// <returns></returns>
        public bool GetItem(string iItemName, InventoryType iInventoryType)
        {
            //移動元に指定のアイテムが存在するかチェック
            if (!IsExistItem(iItemName, iInventoryType)) return false;
            //移動先に空きがあるかチェック
            if (!IsInventoryFree(InventoryType.Inventory)) return false;
            //Itemizer実行
            string scriptName = string.Format("{0}_{1}", MiscTool.GetAppAssemblyName(), fface.Player.Name);
            //string cmd = string.Format("input /gets \"{0}\" {1}", iItemName, iInventoryType.ToString());
            //return ExecScript(cmd, scriptName);
            string cmd = string.Format("windower.send_command(\"input //get {0} {1}\")", iItemName, iInventoryType.ToString().ToLower());
            return ExecLua(cmd, scriptName);
        }
        /// <summary>
        /// Itemizerで鞄のアイテムを移動する
        /// </summary>
        /// <param name="iItemName">アイテム名</param>
        /// <param name="iInventoryType">倉庫タイプ</param>
        /// <returns>成功した場合Trueを返す</returns>
        public bool PutItem(string iItemName, InventoryType iInventoryType)
        {
            //移動元に指定のアイテムが存在するかチェック
            if (!IsExistItem(iItemName, InventoryType.Inventory)) return false;
            //移動先に空きがあるかチェック
            if (!IsInventoryFree(iInventoryType)) return false;
            //Itemizer実行
            string scriptName = string.Format("{0}_{1}", MiscTool.GetAppAssemblyName(), fface.Player.Name);
            //string cmd = string.Format("input /puts \"{0}\" {1}", iItemName, iInventoryType.ToString());
            //return ExecScript(cmd, scriptName);
            string cmd = string.Format("windower.send_command(\"input //put {0} {1}\")", iItemName, iInventoryType.ToString().ToLower());
            return ExecLua(cmd, scriptName);
        }
        /// <summary>
        /// 指定した倉庫タイプにアイテムが存在するか否か
        /// </summary>
        /// <param name="iItemName">アイテム名</param>
        /// <param name="iInventoryType">倉庫タイプ</param>
        /// <returns>存在した場合Trueを返す</returns>
        public bool IsExistItem(string iItemName, InventoryType iInventoryType)
        {
            //アイテムIDの取得
            int itemId = FFACE.ParseResources.GetItemId(iItemName);
            if (itemId == 0) return false;
            //指定のアイテムが存在するかチェック
            if (fface.Item.GetItemCount(itemId, iInventoryType) > 0) return true;
            return false;
        }
        /// <summary>
        /// 指定した倉庫タイプに空きがあるか否か
        /// </summary>
        /// <param name="iInventoryType">倉庫タイプ</param>
        /// <returns>空きがある場合にはTrueを返す</returns>
        public bool IsInventoryFree(InventoryType iInventoryType)
        {
            switch (iInventoryType)
            {
                case InventoryType.Inventory:
                    if (fface.Item.InventoryCount < fface.Item.InventoryMax) return true;
                    break;
                case InventoryType.Safe:
                    if (fface.Item.SafeCount < fface.Item.SafeMax) return true;
                    break;
                case InventoryType.Storage:
                    if (fface.Item.StorageCount < fface.Item.StorageMax) return true;
                    break;
                case InventoryType.Locker:
                    if (fface.Item.LockerCount < fface.Item.LockerMax) return true;
                    break;
                case InventoryType.Satchel:
                    if (fface.Item.SatchelCount < fface.Item.SatchelMax) return true;
                    break;
                case InventoryType.Sack:
                    if (fface.Item.SackCount < fface.Item.SackMax) return true;
                    break;
                case InventoryType.Case:
                    if (fface.Item.CaseCount < fface.Item.CaseMax) return true;
                    break;
                case InventoryType.Wardrobe:
                    if (fface.Item.WardrobeCount < fface.Item.WardrobeMax) return true;
                    break;
            }
            return false;
        }
        #endregion

        #region Script関連
        /// <summary>
        /// スクリプトを作成し実行する
        /// </summary>
        /// <param name="iScriptName">作成するスクリプト名</param>
        /// <param name="iCommand">スクリプトコマンド</param>
        /// <returns>成功した場合Trueを返す</returns>
        public bool ExecScript(string iCommand, string iScriptName = "work")
        {
            try
            {
                string fileName = string.Format("{0}.txt", iScriptName);
                string fullFileName = Path.Combine(FFACEControl.GetWindowerPath(), "scripts", fileName);
                //既存スクリプト削除
                if (File.Exists(fullFileName)) File.Delete(fullFileName);
                //スクリプトファイル作成
                using (StreamWriter sw = new StreamWriter(fullFileName, false, new UTF8Encoding(false)))
                {
                    sw.WriteLine(iCommand);
                    sw.Close();
                }
                if (!File.Exists(fullFileName)) return false;
                //スクリプトファイル実行
                fface.Windower.SendString(string.Format("//exec {0}", fileName));
                //スクリプト削除
                //if (File.Exists(fullName)) File.Delete(fullName);
            }
            catch (Exception e)
            {
                logger.Output(LogLevelKind.ERROR, e.Message);
                return false;
            }
            return true;
        }
        /// <summary>
        /// Luaを作成し実行する
        /// </summary>
        /// <param name="iLuaName">作成するLua名</param>
        /// <param name="iCommand">Luaコマンド</param>
        /// <returns>成功した場合Trueを返す</returns>
        public bool ExecLua(string iCommand, string iLuaName = "work")
        {
            try
            {
                string fullName = Path.Combine(FFACEControl.GetWindowerPath(), "scripts", string.Format("{0}.lua", iLuaName));
                //既存スクリプト削除
                if (File.Exists(fullName)) File.Delete(fullName);
                //スクリプトファイル作成
                using (StreamWriter sw = new StreamWriter(fullName, false, new UTF8Encoding(false)))
                {
                    sw.WriteLine(iCommand);
                    sw.Close();
                }
                if (!File.Exists(fullName)) return false;
                //スクリプトファイル実行
                fface.Windower.SendString(string.Format("//lua e {0}", iLuaName));
                //スクリプト削除
                //if (File.Exists(fullName)) File.Delete(fullName);
            }
            catch (Exception e)
            {
                logger.Output(LogLevelKind.ERROR, e.Message);
                return false;
            }
            return true;
        }
        #endregion

        #region ターゲット関連
        /// <summary>
        /// 指定したNPCをターゲットする
        /// </summary>
        /// <param name="iId">NpcID</param>
        /// <returns>True:ターゲット完了 False:ターゲット出来なかった</returns>
        public bool SetTargetFromId(int iId, bool iWithEnter = false)
        {
            logger.Output(LogLevelKind.DEBUG,  "SetTargetFromId", string.Format("Id={0} WithEnter={1}", iId, iWithEnter));
            for (int i = 0; i < this.MaxLoopCount; i++)
            {
                fface.Target.SetNPCTarget(iId);
                System.Threading.Thread.Sleep(this.BaseWait);
                fface.Windower.SendString("/ta <t>");
                System.Threading.Thread.Sleep(this.BaseWait);
                if (iWithEnter)
                {
                    fface.Windower.SendKeyPress(KeyCode.EnterKey);//Enter
                    System.Threading.Thread.Sleep(this.ChatWait);//Wait
                }
                if (fface.Target.ID == iId) return true;
            }
            logger.Output(LogLevelKind.WARN, "SetTargetFromId", "タイムアウトしました");
            return false;
        }
        #endregion

        #region プレイヤーステータス関連
        /// <summary>
        /// 指定したBUFFがかかっているあ
        /// </summary>
        /// <param name="iStatusEffect">BUFF</param>
        /// <returns>指定したBUFFがかかっていたらTRUEを返す</returns>
        public bool IsBuff(StatusEffect iStatusEffect)
        {
            if (fface.Player.StatusEffects.Contains(iStatusEffect)) return true;
            return false;
        }
        #endregion

        #region キー操作関連
        /// <summary>
        /// キー連打
        /// </summary>
        public void BarrageAnyKey(KeyCode iKeyCode, int iCount)
        {
            for (int i = 0; i < iCount; i++)
            {
                this.fface.Windower.SendKeyPress(iKeyCode);
                System.Threading.Thread.Sleep(100);
            }
        }
        #endregion

        #region その他
        /// <summary>
        /// Windowerがインストールされているパスの取得
        /// </summary>
        /// <returns></returns>
        public static string GetWindowerPath()
        {
            string ret = string.Empty;
            System.Diagnostics.Process[] Processes = System.Diagnostics.Process.GetProcessesByName("pol");
            if (Processes.Length > 0)
            {
                foreach (System.Diagnostics.ProcessModule mod in Processes[0].Modules)
                {
                    if (mod.ModuleName.ToLower() == "hook.dll")
                    {
                        ret = mod.FileName.Substring(0, mod.FileName.Length - 8);
                        break;
                    }
                }
            }
            return ret;
        }
        #endregion
    }
}
