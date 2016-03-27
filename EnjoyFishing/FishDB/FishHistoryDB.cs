using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using System.Xml.XPath;
using MiscTools;
using NLog;

namespace EnjoyFishing
{
    public class FishHistoryDB
    {
        public const string PATH_FISHHISTORYDB = "History";
        public const string FILENAME_FISHHISTORYDB = "{0}_{1}.xml";
        private const string VERSION = "1.1.0";

        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FishHistoryDB(string iPlayerName, DateTime iYmd)
        {
            FishHistoryDBModel history = GetHistoryDB(iPlayerName, iYmd);
            updateCatchCount(history);
        }

        #region メンバー
        /// <summary>
        /// 釣果数(ResultがCatchの数)
        /// </summary>
        public int CatchCount
        {
            get;
            private set;
        }
        #endregion

        /// <summary>
        /// 日付別の履歴を取得
        /// </summary>
        /// <param name="iPlayerName">プレイヤー名</param>
        /// <param name="iYmd">日付</param>
        /// <returns></returns>
        public FishHistoryDBModel SelectDayly(string iPlayerName, DateTime iYmd)
        {
            return SelectDayly(iPlayerName, iYmd, FishResultStatusKind.Unknown, string.Empty);
        }
        /// <summary>
        /// 日付別の履歴を取得
        /// </summary>
        /// <param name="iPlayerName">プレイヤー名</param>
        /// <param name="iYmd">日付</param>
        /// <returns></returns>
        public FishHistoryDBModel SelectDayly(string iPlayerName, DateTime iYmd, FishResultStatusKind iResultStatus, string iFishName)
        {
            logger.Trace("Player={0} Ymd={1} ResultStatus={2} Fish={3}", iPlayerName, iYmd, iResultStatus, iFishName);
            FishHistoryDBModel tmpHistory = GetHistoryDB(iPlayerName, iYmd);
            FishHistoryDBModel ret = GetHistoryDB(iPlayerName, iYmd);
            ret.Fishes.Clear();
            foreach (FishHistoryDBFishModel fish in tmpHistory.Fishes)
            {
                bool addFlg = true;
                if (iResultStatus != FishResultStatusKind.Unknown && fish.Result != iResultStatus) addFlg = false;
                if (iFishName != string.Empty && fish.FishName != iFishName) addFlg = false;
                if (addFlg)
                {
                    ret.Fishes.Add(fish);
                }
            }
            return ret;
        }
        /// <summary>
        /// 指定された日付に釣れた魚名を取得する
        /// </summary>
        /// <param name="iPlayerName">プレイヤー名</param>
        /// <param name="iYmd">日付</param>
        /// <returns></returns>
        public List<string> SelectDaylyUniqueFishName(string iPlayerName, DateTime iYmd)
        {
            logger.Trace("Player={0} Ymd={1}", iPlayerName, iYmd);
            FishHistoryDBModel tmpHistory = GetHistoryDB(iPlayerName, iYmd);
            List<string> ret = new List<string>();
            foreach (FishHistoryDBFishModel fish in tmpHistory.Fishes)
            {
                if (fish.FishName != string.Empty && !ret.Contains(fish.FishName)) ret.Add(fish.FishName);
            }
            return ret;
        }
        /// <summary>
        /// 日別のサマリーを取得する
        /// </summary>
        /// <param name="iPlayerName">プレイヤー名</param>
        /// <param name="iYmd">日付</param>
        /// <returns>サマリー情報</returns>
        public FishHistoryDBSummaryModel GetSummary(string iPlayerName, DateTime iYmd, FishResultStatusKind iResultStatus, string iFishName)
        {
            logger.Trace("Player={0} Ymd={1} ResultStatus={2} Fish={3}", iPlayerName, iYmd, iResultStatus, iFishName);
            FishHistoryDBModel history = SelectDayly(iPlayerName, iYmd, iResultStatus, iFishName);
            FishHistoryDBSummaryModel ret = new FishHistoryDBSummaryModel();
            foreach (FishHistoryDBFishModel fish in history.Fishes)
            {
                ret.Add(fish);
            }
            return ret;
        }

        /// <summary>
        /// 魚を履歴に追加
        /// </summary>
        /// <param name="iPlayername">プレイヤー名</param>
        /// <param name="iFish">FishHistoryDBFishModel</param>
        /// <returns>True:成功</returns>
        public bool AddFish(string iPlayername, int iTimeElapsed, FishHistoryDBFishModel iFish)
        {
            FishHistoryDBModel historydb = GetHistoryDB(iPlayername, DateTime.Parse(iFish.EarthTime));

            historydb.Version = VERSION;
            historydb.PlayerName = iPlayername;
            historydb.EarthDate = DateTime.Parse(iFish.EarthTime).ToShortDateString();
            historydb.Uploaded = false;
            historydb.TimeElapsed = iTimeElapsed;
            historydb.Fishes.Add(iFish);

            return PutHistoryDB(iPlayername, historydb);
        }
        /// <summary>
        /// ハラキリを履歴に追加
        /// </summary>
        /// <param name="iPlayername">プレイヤー名</param>
        /// <param name="iFish">FishHistoryDBHarakiriModel</param>
        /// <returns>成功ならTrueを返す</returns>
        public bool AddHarakiri(string iPlayername, FishHistoryDBHarakiriModel iFish)
        {
            FishHistoryDBModel historydb = GetHistoryDB(iPlayername, DateTime.Parse(iFish.EarthTime));

            historydb.Version = VERSION;
            historydb.PlayerName = iPlayername;
            historydb.EarthDate = DateTime.Parse(iFish.EarthTime).ToShortDateString();
            historydb.Uploaded = false;
            historydb.Harakiri.Add(iFish);

            return PutHistoryDB(iPlayername, historydb);
        }
        /// <summary>
        /// ファイルコンバーター
        /// </summary>
        /// <returns></returns>
        public void Converter()
        {
            string[] xmlFileNames = Directory.GetFiles(PATH_FISHHISTORYDB);
            foreach (string xmlFileName in xmlFileNames)
            {
                //string filename = Path.GetFileName(xmlFileName);
                List<string> regGroupStr = new List<string>();
                if (MiscTool.GetRegexString(xmlFileName, PATH_FISHHISTORYDB + "\\\\(.*)_([0-9][0-9][0-9][0-9])([0-9][0-9])([0-9][0-9])\\.xml$", out regGroupStr))
                {
                    string playerName = regGroupStr[0];
                    DateTime ymd = DateTime.Parse(string.Format("{0}/{1}/{2}", regGroupStr[1], regGroupStr[2], regGroupStr[3]));
                    //最新版までコンバート
                    for (int i = 0; i < Constants.MAX_LOOP_COUNT; i++)
                    {
                        string version = GetXmlVersion(xmlFileName);
                        if (version == VERSION)
                        {
                            break;
                        }
                        else if (version == "1.0.0")////1.0.0→1.0.5
                        {
                            logger.Info("FishHistoryDBのコンバート 1.0.0→1.0.5 {0}", xmlFileName);
                            convert1_0_0to1_0_5(xmlFileName, playerName, ymd);
                        }
                        else if (version == "1.0.5")////1.0.5→1.1.0
                        {
                            logger.Info("FishHistoryDBのコンバート 1.0.5→1.1.0 {0}", xmlFileName);
                            convert1_0_5to1_1_0(xmlFileName, playerName, ymd);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// xmlファイルをコンバートする（1.0.5→1.1.0）
        /// </summary>
        /// <returns></returns>
        private void convert1_0_5to1_1_0(string iXmlFileName, string iPlayerName, DateTime iYmd)
        {
            FishHistoryDBModel1_0_5 history1_0_5 = GetHistoryDB1_0_5(iPlayerName, iYmd);
            FishHistoryDBModel history1_1_0 = new FishHistoryDBModel();
            history1_1_0.Version = "1.1.0";
            history1_1_0.PlayerName = history1_0_5.PlayerName;
            history1_1_0.EarthDate = history1_0_5.EarthDate;
            history1_1_0.Uploaded = false;
            history1_1_0.TimeElapsed = history1_0_5.TimeElapsed;
            foreach (FishHistoryDBFishModel1_0_5 fish1_0_5 in history1_0_5.Fishes)
            {
                FishHistoryDBFishModel fish1_1_0 = new FishHistoryDBFishModel();
                fish1_1_0.FishName = fish1_0_5.FishName;
                fish1_1_0.ZoneName = fish1_0_5.ZoneName;
                fish1_1_0.RodName = fish1_0_5.RodName;
                fish1_1_0.BaitName = fish1_0_5.BaitName;
                fish1_1_0.ID1 = fish1_0_5.ID1;
                fish1_1_0.ID2 = fish1_0_5.ID2;
                fish1_1_0.ID3 = fish1_0_5.ID3;
                fish1_1_0.ID4 = fish1_0_5.ID4;
                fish1_1_0.Critical = fish1_0_5.Critical;
                fish1_1_0.FishCount = fish1_0_5.FishCount;
                fish1_1_0.ItemType = fish1_0_5.ItemType;
                fish1_1_0.FishType = fish1_0_5.FishType;
                fish1_1_0.Result = fish1_0_5.Result;
                fish1_1_0.EarthTime = fish1_0_5.EarthTime;
                fish1_1_0.VanaTime = fish1_0_5.VanaTime;
                fish1_1_0.VanaWeekDay = fish1_0_5.VanaWeekDay;
                fish1_1_0.MoonPhase = fish1_0_5.MoonPhase;
                fish1_1_0.X = fish1_0_5.X;
                fish1_1_0.Y = fish1_0_5.Y;
                fish1_1_0.Z = fish1_0_5.Z;
                fish1_1_0.H = fish1_0_5.H;
                fish1_1_0.Skill = fish1_0_5.Skill;
                fish1_1_0.SerpentRumors = fish1_0_5.SerpentRumors;
                fish1_1_0.AnglersAlmanac = fish1_0_5.AnglersAlmanac;
                fish1_1_0.FrogFishing = fish1_0_5.FrogFishing;
                fish1_1_0.Mooching = fish1_0_5.Mooching;
                history1_1_0.Fishes.Add(fish1_1_0);
            }
            foreach (FishHistoryDBHarakiriModel1_0_5 harakiri1_0_5 in history1_0_5.Harakiri)
            {
                FishHistoryDBHarakiriModel harakiri1_1_0 = new FishHistoryDBHarakiriModel();
                harakiri1_1_0.EarthTime = harakiri1_0_5.EarthTime;
                harakiri1_1_0.VanaTime = harakiri1_0_5.VanaTime;
                harakiri1_1_0.FishName = harakiri1_0_5.FishName;
                harakiri1_1_0.ItemName = harakiri1_0_5.ItemName;
                history1_1_0.Harakiri.Add(harakiri1_1_0);
            }

            //バックアップ
            string backupFileName = iXmlFileName + ".bak";
            if (File.Exists(backupFileName)) File.Delete(backupFileName);
            File.Copy(iXmlFileName, backupFileName);
            //xml書き込み
            PutHistoryDB(iPlayerName, history1_1_0);
        }
        /// <summary>
        /// xmlファイルをコンバートする（1.0.0→1.0.5）
        /// </summary>
        /// <returns></returns>
        private void convert1_0_0to1_0_5(string iXmlFileName, string iPlayerName, DateTime iYmd)
        {
            FishHistoryDBModel1_0_0 history1_0_0 = getHistoryDB1_0_0(iPlayerName, iYmd);
            FishHistoryDBModel1_0_5 history1_0_5 = new FishHistoryDBModel1_0_5();
            history1_0_5.Version = "1.0.5";
            history1_0_5.PlayerName = history1_0_0.PlayerName;
            history1_0_5.EarthDate = history1_0_0.EarthDate.ToShortDateString();
            foreach (FishHistoryDBFishModel1_0_0 fish1_0_0 in history1_0_0.Fishes)
            {
                FishHistoryDBFishModel1_0_5 fish1_0_5 = new FishHistoryDBFishModel1_0_5();
                fish1_0_5.FishName = fish1_0_0.FishName;
                fish1_0_5.ZoneName = fish1_0_0.ZoneName;
                fish1_0_5.RodName = fish1_0_0.RodName;
                fish1_0_5.BaitName = fish1_0_0.BaitName;
                fish1_0_5.ID1 = fish1_0_0.ID1;
                fish1_0_5.ID2 = fish1_0_0.ID2;
                fish1_0_5.ID3 = fish1_0_0.ID3;
                fish1_0_5.ID4 = fish1_0_0.ID4;
                fish1_0_5.Critical = fish1_0_0.Critical;
                fish1_0_5.FishCount = fish1_0_0.FishCount;
                if (fish1_0_0.ID1 == 0 && fish1_0_0.ID2 == 0 && fish1_0_0.ID3 == 0 && fish1_0_0.ID4 == 0)
                {
                    fish1_0_5.ItemType = FishDBItemTypeKind.Unknown;
                }
                else
                {
                    fish1_0_5.ItemType = FishDBItemTypeKind.Common;
                }
                fish1_0_5.FishType = fish1_0_0.FishType;
                fish1_0_5.Result = fish1_0_0.Result;
                fish1_0_5.EarthTime = fish1_0_0.EarthTime.ToString();
                fish1_0_5.VanaTime = fish1_0_0.VanaTime + ":00";
                fish1_0_5.VanaWeekDay = fish1_0_0.VanaWeekDay;
                fish1_0_5.MoonPhase = fish1_0_0.MoonPhase;
                fish1_0_5.X = (float)Math.Round(fish1_0_0.X, 1, MidpointRounding.AwayFromZero);
                fish1_0_5.Y = (float)Math.Round(fish1_0_0.Y, 1, MidpointRounding.AwayFromZero);
                fish1_0_5.Z = (float)Math.Round(fish1_0_0.Z, 1, MidpointRounding.AwayFromZero);
                fish1_0_5.H = (float)Math.Round(fish1_0_0.H, 1, MidpointRounding.AwayFromZero);
                history1_0_5.Fishes.Add(fish1_0_5);
            }
            //バックアップ
            string backupFileName = iXmlFileName + ".bak";
            if (File.Exists(backupFileName)) File.Delete(backupFileName);
            File.Copy(iXmlFileName, backupFileName);
            //xml書き込み
            PutHistoryDB1_0_5(iPlayerName, history1_0_5);
        }
        /// <summary>
        /// xmlの内容を全て取得する
        /// </summary>
        /// <returns>FishHistoryDBModel</returns>
        public FishHistoryDBModel GetHistoryDB(string iPlayerName, DateTime iYmd, string iPath = PATH_FISHHISTORYDB)
        {
            string xmlFilename = GetXmlName(iPlayerName, iYmd);
            try
            {
                FishHistoryDBModel history = new FishHistoryDBModel();
                if (!Directory.Exists(iPath))
                {
                    Directory.CreateDirectory(iPath);
                }
                if (File.Exists(xmlFilename))
                {
                    for (int i = 0; i < Constants.FILELOCK_RETRY_COUNT; i++)
                    {
                        try
                        {
                            using (FileStream fs = new FileStream(xmlFilename, FileMode.Open, FileAccess.Read, FileShare.Read))
                            {
                                XmlSerializer serializer = new XmlSerializer(typeof(FishHistoryDBModel));
                                history = (FishHistoryDBModel)serializer.Deserialize(fs);
                                fs.Close();
                            }
                            break;
                        }
                        catch (IOException)
                        {
                            Thread.Sleep(100);
                            continue;
                        }
                    }
                }
                //CatchCountの更新
                updateCatchCount(history);
                return history;
            }
            catch (Exception e)
            {
                logger.Fatal("{0}の取得中にエラーが発生しました。", xmlFilename);
                throw e;
            }
        }
        /// <summary>
        /// xmlの内容を全て取得する(1.0.5)
        /// </summary>
        /// <returns>FishHistoryDBModel</returns>
        public FishHistoryDBModel1_0_5 GetHistoryDB1_0_5(string iPlayerName, DateTime iYmd, string iPath = PATH_FISHHISTORYDB)
        {
            string xmlFilename = GetXmlName(iPlayerName, iYmd);
            try
            {
                FishHistoryDBModel1_0_5 history = new FishHistoryDBModel1_0_5();
                if (!Directory.Exists(iPath))
                {
                    Directory.CreateDirectory(iPath);
                }
                if (File.Exists(xmlFilename))
                {
                    for (int i = 0; i < Constants.FILELOCK_RETRY_COUNT; i++)
                    {
                        try
                        {
                            using (FileStream fs = new FileStream(xmlFilename, FileMode.Open, FileAccess.Read, FileShare.Read))
                            {
                                XmlSerializer serializer = new XmlSerializer(typeof(FishHistoryDBModel1_0_5));
                                history = (FishHistoryDBModel1_0_5)serializer.Deserialize(fs);
                                fs.Close();
                            }
                            break;
                        }
                        catch (IOException)
                        {
                            Thread.Sleep(100);
                            continue;
                        }
                    }
                }
                //CatchCountの更新
                updateCatchCount1_0_5(history);
                return history;
            }
            catch (Exception e)
            {
                logger.Fatal("{0}の取得中にエラーが発生しました。", xmlFilename);
                throw e;
            }
        }
        /// <summary>
        /// xmlへ書き込む
        /// </summary>
        /// <param name="iPlayerName">プレイヤー名</param>
        /// <param name="iHistoryDB">FishHistoryDBModel</param>
        /// <returns>True:成功</returns>
        public bool PutHistoryDB(string iPlayerName, FishHistoryDBModel iHistoryDB, string iPath = PATH_FISHHISTORYDB)
        {
            string xmlFilename = GetXmlName(iPlayerName, DateTime.Parse(iHistoryDB.EarthDate), iPath);
            try
            {
                if (!Directory.Exists(iPath))
                {
                    Directory.CreateDirectory(iPath);
                }

                for (int i = 0; i < Constants.FILELOCK_RETRY_COUNT; i++)
                {
                    try
                    {
                        using (FileStream fs = new FileStream(xmlFilename, FileMode.Create, FileAccess.Write, FileShare.None))//ファイルロック
                        {
                            StreamWriter sw = new StreamWriter(fs, new UTF8Encoding(false));
                            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                            ns.Add(String.Empty, String.Empty);
                            XmlSerializer serializer = new XmlSerializer(typeof(FishHistoryDBModel));
                            serializer.Serialize(sw, iHistoryDB, ns);
                            //書き込み
                            sw.Flush();
                            sw.Close();
                            sw = null;
                        }
                        break;
                    }
                    catch (IOException)
                    {
                        Thread.Sleep(100);
                        continue;
                    }
                }
                //CatchCountの更新
                updateCatchCount(iHistoryDB);
                return true;
            }
            catch (Exception e)
            {
                logger.Fatal("{0}の取得中にエラーが発生しました。", xmlFilename);
                throw e;
            }
        }
        /// <summary>
        /// xmlへ書き込む(1.0.5)
        /// </summary>
        /// <param name="iPlayerName">プレイヤー名</param>
        /// <param name="iHistoryDB">FishHistoryDBModel</param>
        /// <returns>True:成功</returns>
        public bool PutHistoryDB1_0_5(string iPlayerName, FishHistoryDBModel1_0_5 iHistoryDB, string iPath = PATH_FISHHISTORYDB)
        {
            string xmlFilename = GetXmlName(iPlayerName, DateTime.Parse(iHistoryDB.EarthDate), iPath);
            try
            {
                if (!Directory.Exists(iPath))
                {
                    Directory.CreateDirectory(iPath);
                }

                for (int i = 0; i < Constants.FILELOCK_RETRY_COUNT; i++)
                {
                    try
                    {
                        using (FileStream fs = new FileStream(xmlFilename, FileMode.Create, FileAccess.Write, FileShare.None))//ファイルロック
                        {
                            StreamWriter sw = new StreamWriter(fs, new UTF8Encoding(false));
                            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                            ns.Add(String.Empty, String.Empty);
                            XmlSerializer serializer = new XmlSerializer(typeof(FishHistoryDBModel1_0_5));
                            serializer.Serialize(sw, iHistoryDB, ns);
                            //書き込み
                            sw.Flush();
                            sw.Close();
                            sw = null;
                        }
                        break;
                    }
                    catch (IOException)
                    {
                        Thread.Sleep(100);
                        continue;
                    }
                }
                //CatchCountの更新
                updateCatchCount1_0_5(iHistoryDB);
                return true;
            }
            catch (Exception e)
            {
                logger.Fatal("{0}の登録中にエラーが発生しました。", xmlFilename);
                throw e;
            }
        }
        /// <summary>
        /// xmlの内容を全て取得する(1.0.0)
        /// </summary>
        /// <returns>FishHistoryDBModel</returns>
        private FishHistoryDBModel1_0_0 getHistoryDB1_0_0(string iPlayerName, DateTime iYmd)
        {
            string xmlFilename = GetXmlName(iPlayerName, iYmd);
            try
            {
                FishHistoryDBModel1_0_0 historydb = new FishHistoryDBModel1_0_0();
                if (!Directory.Exists(PATH_FISHHISTORYDB))
                {
                    Directory.CreateDirectory(PATH_FISHHISTORYDB);
                }
                if (File.Exists(xmlFilename))
                {
                    for (int i = 0; i < Constants.FILELOCK_RETRY_COUNT; i++)
                    {
                        try
                        {
                            using (FileStream fs = new FileStream(xmlFilename, FileMode.Open, FileAccess.Read, FileShare.Read))
                            {
                                XmlSerializer serializer = new XmlSerializer(typeof(FishHistoryDBModel1_0_0));
                                historydb = (FishHistoryDBModel1_0_0)serializer.Deserialize(fs);
                                fs.Close();
                            }
                            break;
                        }
                        catch (IOException)
                        {
                            Thread.Sleep(100);
                            continue;
                        }
                    }
                }
                return historydb;
            }
            catch (Exception e)
            {
                logger.Fatal("{0}の取得中にエラーが発生しました。", xmlFilename);
                throw e;
            }
        }

        /// <summary>
        /// xmlファイル名を取得
        /// </summary>
        /// <param name="iPlayerName">プレイヤ名</param>
        /// <param name="iYmd">年月日</param>
        /// <returns>xmlファイル名</returns>
        public string GetXmlName(string iPlayerName, DateTime iYmd, string iPath = PATH_FISHHISTORYDB)
        {
            string ymd = iYmd.ToString("yyyyMMdd");
            return iPath + @"\" + string.Format(FILENAME_FISHHISTORYDB, iPlayerName, ymd);
        }
        /// <summary>
        /// xmlファイルのバージョン番号を取得する
        /// </summary>
        /// <param name="iXmlFileName"></param>
        /// <returns></returns>
        public static string GetXmlVersion(string iXmlFileName)
        {
            try
            {
                XPathDocument xmlDoc = new XPathDocument(iXmlFileName);
                XPathNavigator xNavi = xmlDoc.CreateNavigator();
                string ret = string.Empty;
                try
                {
                    ret = xNavi.SelectSingleNode("/History/@version").Value;
                }
                catch (NullReferenceException)
                {
                    ret = "1.0.0";
                }
                return ret;
            }
            catch (Exception e)
            {
                logger.Fatal("{0}のバージョン取得中にエラーが発生しました。", iXmlFileName);
                throw e;
            }
        }
        private void updateCatchCount(FishHistoryDBModel iFishHistoryDB)
        {
            int cnt = 0;
            foreach (FishHistoryDBFishModel fish in iFishHistoryDB.Fishes)
            {
                if (fish.Result == FishResultStatusKind.Catch &&
                    (fish.FishType == FishDBFishTypeKind.SmallFish || fish.FishType == FishDBFishTypeKind.LargeFish))
                {
                    cnt++;
                }
            }
            this.CatchCount = cnt;
        }
        private void updateCatchCount1_0_5(FishHistoryDBModel1_0_5 iFishHistoryDB)
        {
            int cnt = 0;
            foreach (FishHistoryDBFishModel1_0_5 fish in iFishHistoryDB.Fishes)
            {
                if (fish.Result == FishResultStatusKind.Catch &&
                    (fish.FishType == FishDBFishTypeKind.SmallFish || fish.FishType == FishDBFishTypeKind.LargeFish))
                {
                    cnt++;
                }
            }
            this.CatchCount = cnt;
        }
    }
}
