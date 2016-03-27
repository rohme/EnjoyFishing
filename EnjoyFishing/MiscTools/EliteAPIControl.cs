using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using EliteMMO.API;
using EnjoyFishing;

namespace MiscTools
{
    public class EliteAPIControl
    {
        private const int DEFAULT_MAX_LOOP_COUNT = 100;
        private const bool DEFAULT_USE_ENTERNITY = true;
        private const int DEFAULT_BASE_WAIT = 300;
        private const int DEFAULT_CHAT_WAIT = 1000;
        private const string REGEX_PLUGIN = "(.*) \\(author: (.*)";
        //private const string REGEX_PLUGIN_END = "=== Done Listing Currently Loaded Plugins ===";
        private const string REGEX_PLUGIN_END = "=== Done Listing (.*)";
        private const string REGEX_ADDON = "  (.*)";
        private const string REGEX_ADDON_END = "EnjoyFishing Addon Check End";

        private PolTool pol = null;
        private EliteAPI api = null;
        private ChatTool chat = null;
        private ResourceTool resource = null;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        
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
        public EliteAPIControl(PolTool iPOL, ResourceTool iResource, ChatTool iChat)
        {
            this.pol = iPOL;
            this.api = iPOL.EliteAPI;
            this.chat = iChat;
            this.resource = iResource;
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
            logger.Trace("RegexString={0} StartChatIndex={1} WithEnter={1}", iRegexString, iStartChatIndex, iWithEnter);
            List<EliteAPI.ChatEntry> arrChatLine;
            int currChatIndex = iStartChatIndex;
            for (int i = 0; (i < this.MaxLoopCount); i++)
            {
                arrChatLine = iChatTool.GetChatLine(currChatIndex);
                foreach (var cl in arrChatLine)
                {
                    //チャットの判定
                    if (MiscTool.IsRegexString(cl.Text, iRegexString))
                    {
                        return true;
                    }

                }
                if (!this.UseEnternity && iWithEnter)
                {
                    if (api.Target.GetTargetInfo().TargetIndex != 0)
                        api.ThirdParty.KeyPress(Keys.RETURN);///Enter
                }
                System.Threading.Thread.Sleep(this.ChatWait);
            }
            logger.Warn("タイムアウトしました");
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
            logger.Trace("DialogString={0} Enter={1}", iDialogString, iEnter);
            for (int i = 0; (i < this.MaxLoopCount); i++)
            {
                Regex reg = new Regex(iDialogString, RegexOptions.IgnoreCase);
                Match ma = reg.Match(api.Dialog.GetDialog().Question);
                if (api.Menu.IsMenuOpen && ma.Success)
                {
                    return true;
                }
                if (!this.UseEnternity && iEnter)
                {
                    api.ThirdParty.KeyPress(Keys.RETURN);///Enter
                }
                System.Threading.Thread.Sleep(this.BaseWait);
            }
            logger.Warn("タイムアウトしました");
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
            logger.Trace("iIdx={0} iWithEnter={1}", iIdx, iWithEnter);
            for (int i = 0; i < this.MaxLoopCount; i++)
            {
                if (api.Dialog.DialogIndex == iIdx)
                {
                    if (iWithEnter)
                    {
                        api.ThirdParty.KeyPress(Keys.RETURN);///Enter
                    }
                    return true;
                }
                else if (api.Dialog.DialogIndex > iIdx)
                {
                    if ((api.Dialog.DialogIndex - iIdx) >= 3)
                    {
                        api.ThirdParty.KeyPress(Keys.LEFT);//左矢印
                    }
                    else
                    {
                        api.ThirdParty.KeyPress(Keys.UP);//上矢印
                    }
                }
                else if (api.Dialog.DialogIndex < iIdx)
                {
                    if ((iIdx - api.Dialog.DialogIndex) >= 3)
                    {
                        api.ThirdParty.KeyPress(Keys.RIGHT);//右矢印
                    }
                    else
                    {
                        api.ThirdParty.KeyPress(Keys.DOWN);//下矢印
                    }
                }
                System.Threading.Thread.Sleep(this.BaseWait);
            }
            logger.Warn("タイムアウトしました");
            return false;
        }
        /// <summary>
        /// 選択されたOptionIndexを返す
        /// </summary>
        /// <returns></returns>
        public ushort GetSelectedOptionIndex()
        {
            int lastDialogId = api.Dialog.DialogId;
            ushort lastOptionIndex = 0;
            while (api.Dialog.DialogId == lastDialogId)
            {
                lastOptionIndex = api.Dialog.DialogIndex;
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
                if (api.Menu.IsMenuOpen)
                {
                    api.ThirdParty.KeyPress(Keys.ESCAPE);
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

        #region メニュー関連
        public string GetMenuHelpName()
        {
            var b = Encoding.GetEncoding(1252).GetBytes(api.Menu.HelpName);
            return Encoding.GetEncoding(932).GetString(b);
        }
        public string GetMenuHelpDescription()
        {
            var b = Encoding.GetEncoding(1252).GetBytes(api.Menu.HelpDescription);
            return Encoding.GetEncoding(932).GetString(b);
        }
        #endregion

        #region Plugin Addon関連
        /// <summary>
        /// 実行中のプラグイン名を取得する
        /// </summary>
        /// <returns>プラグイン名</returns>
        public List<string> GetPlugin()
        {
            if (api.Player.LoginStatus != (int)LoginStatus.LoggedIn) return new List<string>();
            chat.Reset();
            api.ThirdParty.SendString("//plugin_list");
            List<string> ret = new List<string>();
            var cl = new EliteAPI.ChatEntry();
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
            if (api.Player.LoginStatus != (int)LoginStatus.LoggedIn) return new List<string>();
            chat.Reset();
            api.ThirdParty.SendString("//lua list");
            Thread.Sleep(this.BaseWait);
            api.ThirdParty.SendString("/echo " + REGEX_ADDON_END);
            List<string> ret = new List<string>();
            var cl = new EliteAPI.ChatEntry();
            for (int i = 0; i < this.MaxLoopCount && !MiscTool.IsRegexString(cl.Text, REGEX_ADDON_END); i++)
            {
                while (chat.GetNextChatLine(out cl))
                {
                    if (MiscTool.IsRegexString(cl.Text, REGEX_ADDON))
                    {
                        List<string> reg = MiscTool.GetRegexString(cl.Text, REGEX_ADDON);
                        ret.Add(reg[0]);
                    }
                    else if (MiscTool.IsRegexString(cl.Text, REGEX_ADDON_END))
                    {
                        break;
                    }
                }
                Thread.Sleep(this.BaseWait);
            }
            return ret;
        }
        #endregion

        #region アイテム関連
        /// <summary>
        /// 指定された倉庫タイプのアイテム数を取得する
        /// </summary>
        /// <param name="iInventoryType">倉庫タイプ</param>
        /// <returns>アイテム数</returns>
        public int GetInventoryCountByType(InventoryType iInventoryType)
        {
            return api.Inventory.GetContainerCount((int)iInventoryType);
        }
        /// <summary>
        /// 指定された倉庫のアイテムMAX数を取得する
        /// </summary>
        /// <param name="iInventoryType">倉庫タイプ</param>
        /// <returns>アイテムMAX数</returns>
        public int GetInventoryMaxByType(InventoryType iInventoryType)
        {
            return api.Inventory.GetContainerMaxCount((int)iInventoryType);
        }
        /// <summary>
        /// 指定された倉庫タイプに入っているアイテム数を取得
        /// </summary>
        /// <param name="iItemName"></param>
        /// <param name="iInventoryType"></param>
        /// <returns></returns>
        public int GetInventoryItemCount(string iItemName, InventoryType iInventoryType)
        {
            if (!resource.Items.Any(x => x.Value.Name[1] == iItemName)) return 0;
            var id = resource.Items.First(x => x.Value.Name[1] == iItemName).Key;
            return GetInventoryItemCount(id, iInventoryType);
        }
        /// <summary>
        /// 指定された倉庫タイプに入っているアイテム数を取得
        /// </summary>
        /// <param name="iItemName"></param>
        /// <param name="iInventoryType"></param>
        /// <returns></returns>
        public int GetInventoryItemCount(uint iItemID, InventoryType iInventoryType)
        {
            int ret = 0;
            for(int i = 0; i < 80; i++)
            {
                var item = api.Inventory.GetContainerItem((int)iInventoryType, i);
                if (item.Id == iItemID) ret += (int)item.Count;
            }
            return ret;
        }
        /// <summary>
        /// Itemizerでアイテムを鞄に移動する
        /// </summary>
        /// <param name="iItemName">アイテム名</param>
        /// <param name="iInventoryType">倉庫タイプ</param>
        /// <returns></returns>
        public bool GetItemizer(string iItemName, InventoryType iInventoryType)
        {
            //移動元に指定のアイテムが存在するかチェック
            if (!IsExistItem(iItemName, iInventoryType)) return false;
            //移動先に空きがあるかチェック
            if (!IsInventoryFree(InventoryType.Inventory)) return false;
            //Itemizer実行
            string scriptName = string.Format("{0}_{1}", MiscTool.GetAppAssemblyName(), api.Player.Name);
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
        public bool PutItemizer(string iItemName, InventoryType iInventoryType)
        {
            //移動元に指定のアイテムが存在するかチェック
            if (!IsExistItem(iItemName, InventoryType.Inventory)) return false;
            //移動先に空きがあるかチェック
            if (!IsInventoryFree(iInventoryType)) return false;
            //Itemizer実行
            string scriptName = string.Format("{0}_{1}", MiscTool.GetAppAssemblyName(), api.Player.Name);
            //string cmd = string.Format("input /puts \"{0}\" {1}", iItemName, iInventoryType.ToString());
            //return ExecScript(cmd, scriptName);
            string cmd = string.Format("windower.send_command(\"input //puts {0} {1}\")", iItemName, iInventoryType.ToString().ToLower());
            return ExecLua(cmd, scriptName);
        }
        /// <summary>
        /// 指定したアイテムが何処に存在するか
        /// </summary>
        /// <param name="iItemName"></param>
        /// <returns></returns>
        public InventoryType? GetInventoryTypeFromItemName(string iItemName)
        {
            if (GetInventoryItemCount(iItemName, InventoryType.Inventory) > 0) return InventoryType.Inventory;
            if (GetInventoryItemCount(iItemName, InventoryType.Sack) > 0) return InventoryType.Sack;
            if (GetInventoryItemCount(iItemName, InventoryType.Satchel) > 0) return InventoryType.Satchel;
            if (GetInventoryItemCount(iItemName, InventoryType.Case) > 0) return InventoryType.Case;
            if (GetInventoryItemCount(iItemName, InventoryType.Wardrobe) > 0) return InventoryType.Wardrobe;
            return null;
        }
        /// <summary>
        /// 指定した倉庫タイプにアイテムが存在するか否か
        /// </summary>
        /// <param name="iItemName">アイテム名</param>
        /// <param name="iInventoryType">倉庫タイプ</param>
        /// <returns>存在した場合Trueを返す</returns>
        public bool IsExistItem(string iItemName, InventoryType iInventoryType)
        {
            return GetInventoryItemCount(iItemName, iInventoryType) > 0;
        }
        /// <summary>
        /// 指定した倉庫タイプに空きがあるか否か
        /// </summary>
        /// <param name="iInventoryType">倉庫タイプ</param>
        /// <returns>空きがある場合にはTrueを返す</returns>
        public bool IsInventoryFree(InventoryType iInventoryType)
        {
            return GetInventoryCountByType(iInventoryType) < GetInventoryMaxByType(iInventoryType);
        }
        /// <summary>
        /// 指定された鞄からアイテムインデックスを取得する
        /// </summary>
        /// <param name="iItemName"></param>
        /// <param name="iInventoryType"></param>
        /// <returns></returns>
        public int GetInventoryFirstItemIndex(string iItemName,InventoryType iInventoryType)
        {
            uint id = resource.GetItem(iItemName).ItemID;
            return GetInventoryFirstItemIndex(id, iInventoryType);
        }
        /// <summary>
        /// 指定された鞄からアイテムインデックスを取得する
        /// </summary>
        /// <param name="iItemID"></param>
        /// <param name="iInventoryType"></param>
        /// <returns></returns>
        public int GetInventoryFirstItemIndex(uint iItemID, InventoryType iInventoryType)
        {
            for(int i = 1; i <= 80; i++)
            {
                var item = api.Inventory.GetContainerItem((int)iInventoryType, i);
                if (item.Id == iItemID)
                {
                    return item.Index;
                }
            }
            return 0;
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
                string fullFileName = Path.Combine(EliteAPIControl.GetWindowerPath(), "scripts", fileName);
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
                api.ThirdParty.SendString(string.Format("//exec {0}", fileName));
                //スクリプト削除
                //if (File.Exists(fullName)) File.Delete(fullName);
            }
            catch (Exception e)
            {
                logger.Error(e, "スクリプト実行エラー");
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
                string fullName = Path.Combine(EliteAPIControl.GetWindowerPath(), "scripts", string.Format("{0}.lua", iLuaName));
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
                api.ThirdParty.SendString(string.Format("//lua e {0}", iLuaName));
                //スクリプト削除
                //if (File.Exists(fullName)) File.Delete(fullName);
            }
            catch (Exception e)
            {
                logger.Error(e, "Lua実行エラー");
                return false;
            }
            return true;
        }
        #endregion

        #region ターゲット関連
        /// <summary>
        /// 指定したNPCをターゲットする
        /// </summary>
        /// <param name="iIndex">NpcID</param>
        /// <returns>True:ターゲット完了 False:ターゲット出来なかった</returns>
        public bool SetTargetFromId(int iIndex, bool iWithEnter = false)
        {
            logger.Trace("Index={0} WithEnter={1}", iIndex, iWithEnter);
            for (int i= 0; i < this.MaxLoopCount; i++)
            {
                if(api.Target.GetTargetInfo().TargetIndex == iIndex)
                    return true;
                else
                    api.Target.SetTarget(iIndex);
                Thread.Sleep(this.ChatWait);
            }
            logger.Warn("タイムアウトしました");
            return false;
        }
        #endregion

        #region プレイヤーステータス関連
        /// <summary>
        /// 指定したBUFFがかかっているあ
        /// </summary>
        /// <param name="iStatusEffect">BUFF</param>
        /// <returns>指定したBUFFがかかっていたらTRUEを返す</returns>
        public bool IsBuff(EliteMMO.API.StatusEffect iStatusEffect)
        {
            return api.Player.Buffs.Contains((short)iStatusEffect);
        }
        #endregion

        #region キー操作関連
        /// <summary>
        /// キー連打
        /// </summary>
        public void BarrageAnyKey(Keys iKeyCode, int iCount)
        {
            for (int i = 0; i < iCount; i++)
            {
                api.ThirdParty.KeyPress(iKeyCode);
                System.Threading.Thread.Sleep(100);
            }
        }
        #endregion

        #region 時間関連
        /// <summary>
        /// 地球時間からヴァナ時間を取得
        /// </summary>
        /// <param name="iEarthDate">地球時間</param>
        /// <returns>ヴァナ時間</returns>
        public static VanaTime GetVanaTimeFromEarthTime(DateTime iEarthDate)
        {
            //地球時間 2002/01/01 00:00:00 = 天晶暦 0886/01/01 00:00:00
            //一年＝３６０日 一ヶ月＝３０日 一日＝２４時間 一時間＝６０分 一分＝６０秒
            var ret = new VanaTime();
            DateTime baseDate = new DateTime(2002, 1, 1, 0, 0, 0);
            DateTime nowDate = new DateTime(iEarthDate.Year, iEarthDate.Month, iEarthDate.Day, iEarthDate.Hour, iEarthDate.Minute, iEarthDate.Second);
            long baseTicks = baseDate.Ticks / 10000000L;
            long nowTicks = nowDate.Ticks / 10000000L;
            long vanaTicks = (nowTicks - baseTicks) * 25L;
            //年
            double year = vanaTicks / (360D * 24D * 60D * 60D);
            ret.Year = (uint)(Math.Floor(year) + 886D);
            //月
            ret.Month = (byte)((vanaTicks % (360D * 24D * 60D * 60D)) / (30D * 24D * 60D * 60D) + 1);
            //日
            ret.Day = (byte)((vanaTicks % (30D * 24D * 60D * 60D)) / (24D * 60D * 60D) + 1);
            //時
            ret.Hour = (byte)((vanaTicks % (24D * 60D * 60D)) / (60D * 60D));
            //分
            ret.Minute = (byte)((vanaTicks % (60D * 60D)) / (60D));
            //秒
            ret.Second = (byte)(vanaTicks % 60D);
            //曜日
            double dayType = (byte)((vanaTicks % (8D * 24D * 60D * 60D)) / (24D * 60D * 60D));
            ret.DayType = (Weekday)dayType;
            //月齢
            double moonPhase = (byte)((vanaTicks % (12D * 7D * 24D * 60D * 60D)) / (7D * 24D * 60D * 60D));
            ret.MoonPhase = (MoonPhase)moonPhase;
            return ret;
        }
        /// <summary>
        /// ヴァナ日付より地球日付を取得
        /// </summary>
        /// <param name="iVanaDate"></param>
        /// <returns></returns>
        public static DateTime GetEarthTimeFromVanaTime(VanaTime iVanaTime)
        {
            //地球時間 2002/01/01 00:00:00 = 天晶暦 0886/01/01 00:00:00
            //一年＝３６０日 一ヶ月＝３０日 一日＝２４時間 一時間＝６０分 一分＝６０秒
            long baseTicks = (886L * 360L * 24L * 60L * 60L) + (30L * 24L * 60L * 60L) + (24L * 60L * 60L);
            long vanaTicks = (iVanaTime.Year * 12L * 30L * 24L * 60L * 60L) +
                            (iVanaTime.Month * 30L * 24L * 60L * 60L) +
                            (iVanaTime.Day * 24L * 60L * 60L) +
                            (iVanaTime.Hour * 60L * 60L) +
                            (iVanaTime.Minute * 60L) +
                            (long)iVanaTime.Second;
            long addseconds = (((vanaTicks - baseTicks) / 25L));
            DateTime ret = new DateTime(2002, 1, 1, 0, 0, 0);
            ret = ret.AddSeconds(addseconds);
            return ret;
        }
        /// <summary>
        /// ヴァナ時間より月齢を取得
        /// </summary>
        /// <param name="iVanaDate">ヴァナ日付</param>
        /// <returns>月齢</returns>
        public static MoonPhase GetMoonPhaseFromVanaTime(VanaTime iVanaTime)
        {
            long vanaTicks = getVanaTicks(iVanaTime);
            double moonPhase = (byte)((vanaTicks % (12D * 7D * 24D * 60D * 60D)) / (7D * 24D * 60D * 60D));
            return (MoonPhase)moonPhase;
        }
        /// <summary>
        /// ヴァナ日付より曜日を取得
        /// </summary>
        /// <param name="iVanaDate"></param>
        /// <returns></returns>
        public static Weekday GetWeekdayFromVanaTime(VanaTime iVanaTime)
        {
            long vanaTicks = getVanaTicks(iVanaTime);
            double dayType = (byte)((vanaTicks % (8D * 24D * 60D * 60D)) / (24D * 60D * 60D));
            return (Weekday)dayType;
        }
        /// <summary>
        /// ヴァナ時間のTicksを取得
        /// </summary>
        /// <param name="iVanaDate">ヴァナ日付</param>
        /// <returns>Ticks</returns>
        private static long getVanaTicks(VanaTime iVanaTime)
        {
            long baseTicks = (886L * 360L * 24L * 60L * 60L) + (30L * 24L * 60L * 60L) + (24L * 60L * 60L);
            long vanaTicks = (iVanaTime.Year * 12L * 30L * 24L * 60L * 60L) +
                            (iVanaTime.Month * 30L * 24L * 60L * 60L) +
                            (iVanaTime.Day * 24L * 60L * 60L) +
                            (iVanaTime.Hour * 60L * 60L) +
                            (iVanaTime.Minute * 60L) +
                            (long)iVanaTime.Second;
            return vanaTicks - baseTicks;
        }
        /// <summary>
        /// ヴァナ日付に任意の日付を足す
        /// </summary>
        /// <param name="iVanaTime"></param>
        /// <param name="iAddDays"></param>
        /// <returns></returns>
        public static VanaTime addVanaDay(VanaTime iVanaTime, int iAddDays = 1)
        {
            var ret = new VanaTime()
            {
                Year = iVanaTime.Year,
                Month = iVanaTime.Month,
                Day = iVanaTime.Day,
                Hour = iVanaTime.Hour,
                Minute = iVanaTime.Minute,
                Second = iVanaTime.Second,
            };
            for (int i = 0; i < iAddDays; i++)
            {
                ret.Day++;
                if (ret.Day > 30)
                {
                    ret.Day = 1;
                    ret.Month++;
                }
                if (ret.Month > 12)
                {
                    ret.Month = 1;
                    ret.Year++;
                }
            }
            return ret;
        }
        public VanaTime GetVanaTime()
        {
            VanaTime ret = new VanaTime();
            ret.Year = api.VanaTime.CurrentYear;
            ret.Month = api.VanaTime.CurrentMonth;
            ret.Day = api.VanaTime.CurrentDay;
            ret.Hour = api.VanaTime.CurrentHour;
            ret.Minute = api.VanaTime.CurrentMinute;
            ret.Second = api.VanaTime.CurrentSecond;
            ret.DayType = (Weekday)api.VanaTime.CurrentDay;
            ret.MoonPhase = (MoonPhase)api.VanaTime.CurrentMoonPhase;
            ret.MoonPercent = api.VanaTime.CurrentMoonPercent;
            return ret;
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

    public struct VanaTime
    {
        public uint Year { get; set; }
        public uint Month { get; set; }
        public uint Day { get; set; }
        public uint Hour { get; set; }
        public uint Minute { get; set; }
        public uint Second { get; set; }
        public Weekday DayType { get; set; }
        public uint MoonPercent { get; set; }
        public MoonPhase MoonPhase { get; set; }

        public override string ToString()
        {
            string vTime = Month + "/" + Day + "/" + Year + ", "
                         + DayType + ", "
                         + Hour + ":";

            if (10 > Minute)
                vTime += "0" + Minute + ", "
                 + MoonPhase + " (" + MoonPercent + "%)";
            else
                vTime += Minute + ", "
                 + MoonPhase + " (" + MoonPercent + "%)";

            return vTime;

        }
    }
    public class Position
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float H { get; set; }
    }
}
