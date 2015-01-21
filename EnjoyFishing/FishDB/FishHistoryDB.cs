using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using MiscTools;
using System.Threading;
using System.Xml.Linq;
using System.Xml.XPath;

namespace EnjoyFishing
{
    public class FishHistoryDB
    {
        public const string PATH_FISHHISTORYDB = "History";
        public const string FILENAME_FISHHISTORYDB = "{0}_{1}.xml";
        private const string VERSION = "1.0.5";

        private LoggerTool logger;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FishHistoryDB(string iPlayerName, DateTime iYmd, LoggerTool iLogger)
        {
            this.logger = iLogger;
            FishHistoryDBModel history = getHistoryDB(iPlayerName, iYmd);
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
            return SelectDayly(iPlayerName,iYmd, FishResultStatusKind.Unknown, string.Empty);
        }
        /// <summary>
        /// 日付別の履歴を取得
        /// </summary>
        /// <param name="iPlayerName">プレイヤー名</param>
        /// <param name="iYmd">日付</param>
        /// <returns></returns>
        public FishHistoryDBModel SelectDayly(string iPlayerName, DateTime iYmd, FishResultStatusKind iResultStatus, string iFishName)
        {
            FishHistoryDBModel tmpHistory = getHistoryDB(iPlayerName, iYmd);
            FishHistoryDBModel ret = getHistoryDB(iPlayerName, iYmd);
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
            FishHistoryDBModel tmpHistory = getHistoryDB(iPlayerName, iYmd);
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
        public FishHistoryDBSummaryModel GetSummary(string iPlayerName, DateTime iYmd, FishResultStatusKind iResult, string iFishName)
        {
            FishHistoryDBModel history = SelectDayly(iPlayerName, iYmd, iResult, iFishName);
            FishHistoryDBSummaryModel ret = new FishHistoryDBSummaryModel();
            foreach(FishHistoryDBFishModel fish in history.Fishes)
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
            FishHistoryDBModel historydb = getHistoryDB(iPlayername, DateTime.Parse(iFish.EarthTime));

            historydb.Version = VERSION;
            historydb.PlayerName = iPlayername;
            historydb.EarthDate = DateTime.Parse(iFish.EarthTime).ToShortDateString();
            historydb.TimeElapsed = iTimeElapsed;
            historydb.Fishes.Add(iFish);

            return putHistoryDB(iPlayername, historydb);
        }
        /// <summary>
        /// ハラキリを履歴に追加
        /// </summary>
        /// <param name="iPlayername">プレイヤー名</param>
        /// <param name="iFish">FishHistoryDBHarakiriModel</param>
        /// <returns>成功ならTrueを返す</returns>
        public bool AddHarakiri(string iPlayername, FishHistoryDBHarakiriModel iFish)
        {
            FishHistoryDBModel historydb = getHistoryDB(iPlayername, DateTime.Parse(iFish.EarthTime));

            historydb.Version = VERSION;
            historydb.PlayerName = iPlayername;
            historydb.EarthDate = DateTime.Parse(iFish.EarthTime).ToShortDateString();
            historydb.Harakiri.Add(iFish);

            return putHistoryDB(iPlayername, historydb);
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
                        string version = getXmlVersion(xmlFileName);
                        if (version == VERSION)
                        {
                            break;
                        }
                        if (version == "1.0.0")////1.0.0→1.0.5
                        {
                            logger.Output(LogLevelKind.INFO, string.Format("FishHistoryDBのコンバート 1.0.0→1.0.5 {0}", xmlFileName));
                            convert1_0_0to1_0_5(xmlFileName, playerName, ymd);
                        }
                    }
                }
            } 
        }
        /// <summary>
        /// xmlファイルをコンバートする（1.0.0→1.0.5）
        /// </summary>
        /// <returns></returns>
        private void convert1_0_0to1_0_5(string iXmlFileName, string iPlayerName, DateTime iYmd)
        {
            FishHistoryDBModel1_0_0 history1_0_0 = getHistoryDB1_0_0(iPlayerName, iYmd);
            FishHistoryDBModel history1_0_5 = new FishHistoryDBModel();
            history1_0_5.Version = "1.0.5";
            history1_0_5.PlayerName = history1_0_0.PlayerName;
            history1_0_5.EarthDate = history1_0_0.EarthDate.ToShortDateString();
            foreach (FishHistoryDBFishModel1_0_0 fish1_0_0 in history1_0_0.Fishes)
            {
                FishHistoryDBFishModel fish1_0_5 = new FishHistoryDBFishModel();
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
            putHistoryDB(iPlayerName, history1_0_5);
        }

        /// <summary>
        /// xmlの内容を全て取得する
        /// </summary>
        /// <returns>FishHistoryDBModel</returns>
        private FishHistoryDBModel getHistoryDB(string iPlayerName, DateTime iYmd)
        {
            string xmlFilename =getXmlName(iPlayerName,iYmd);
            FishHistoryDBModel history = new FishHistoryDBModel();
            if(!Directory.Exists(PATH_FISHHISTORYDB))
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
        /// <summary>
        /// xmlへ書き込む
        /// </summary>
        /// <param name="iPlayerName">プレイヤー名</param>
        /// <param name="iHistoryDB">FishHistoryDBModel</param>
        /// <returns>True:成功</returns>
        private bool putHistoryDB(string iPlayerName, FishHistoryDBModel iHistoryDB)
        {
            string xmlFilename = getXmlName(iPlayerName, DateTime.Parse(iHistoryDB.EarthDate));
            if (!Directory.Exists(PATH_FISHHISTORYDB))
            {
                Directory.CreateDirectory(PATH_FISHHISTORYDB);
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
        /// <summary>
        /// xmlの内容を全て取得する(1.0.0)
        /// </summary>
        /// <returns>FishHistoryDBModel</returns>
        private FishHistoryDBModel1_0_0 getHistoryDB1_0_0(string iPlayerName, DateTime iYmd)
        {
            string xmlFilename = getXmlName(iPlayerName, iYmd);
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

        /// <summary>
        /// xmlファイル名を取得
        /// </summary>
        /// <param name="iPlayerName">プレイヤ名</param>
        /// <param name="iYmd">年月日</param>
        /// <returns>xmlファイル名</returns>
        private string getXmlName(string iPlayerName, DateTime iYmd)
        {
            string ymd = iYmd.ToString("yyyyMMdd");
            return PATH_FISHHISTORYDB + @"\" + string.Format(FILENAME_FISHHISTORYDB, iPlayerName, ymd);
        }
        /// <summary>
        /// xmlファイルのバージョン番号を取得する
        /// </summary>
        /// <param name="iXmlFileName"></param>
        /// <returns></returns>
        private string getXmlVersion(string iXmlFileName)
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
    }
}
