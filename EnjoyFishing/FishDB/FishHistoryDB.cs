using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using MiscTools;
using System.Threading;

namespace EnjoyFishing
{
    public class FishHistoryDB
    {
        private const string DIRECTORY_FISHHISTORYDB = "History";
        private const string PATH_FISHHISTORYDB = "{0}_{1}.xml";

        private LoggerTool logger;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FishHistoryDB(LoggerTool iLogger)
        {
            logger = iLogger;
        }

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
        /// 履歴に追加
        /// </summary>
        /// <param name="iPlayername">プレイヤー名</param>
        /// <param name="iFish">FishHistoryDBFishModel</param>
        /// <returns>True:成功</returns>
        public bool Add(string iPlayername, FishHistoryDBFishModel iFish)
        {
            FishHistoryDBModel historydb = getHistoryDB(iPlayername, iFish.EarthTime);

            historydb.PlayerName = iPlayername;
            historydb.EarthDate = DateTime.Parse(iFish.EarthTime.ToString("yyyy/MM/dd"));
            historydb.Fishes.Add(iFish);
            //合計数を算出
            historydb.CatchCount = 0;
            for (int i = 0; i < historydb.Fishes.Count; i++)
            {
                if (historydb.Fishes[i].Result == FishResultStatusKind.Catch) historydb.CatchCount++;
            }

            return putHistoryDB(iPlayername, historydb);
        }
        /// <summary>
        /// xmlの内容を全て取得する
        /// </summary>
        /// <returns>FishHistoryDBModel</returns>
        private FishHistoryDBModel getHistoryDB(string iPlayerName, DateTime iYmd)
        {
            string xmlFilename =getXmlName(iPlayerName,iYmd);
            FishHistoryDBModel historydb = new FishHistoryDBModel();
            if(!Directory.Exists(DIRECTORY_FISHHISTORYDB))
            {
                Directory.CreateDirectory(DIRECTORY_FISHHISTORYDB);
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
                            historydb = (FishHistoryDBModel)serializer.Deserialize(fs);
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
        /// xmlへ書き込む
        /// </summary>
        /// <param name="iPlayerName">プレイヤー名</param>
        /// <param name="iHistoryDB">FishHistoryDBModel</param>
        /// <returns>True:成功</returns>
        private bool putHistoryDB(string iPlayerName, FishHistoryDBModel iHistoryDB)
        {
            string xmlFilename = getXmlName(iPlayerName, iHistoryDB.EarthDate);
            if (!Directory.Exists(DIRECTORY_FISHHISTORYDB))
            {
                Directory.CreateDirectory(DIRECTORY_FISHHISTORYDB);
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
            return true;
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
            return DIRECTORY_FISHHISTORYDB + @"\" + string.Format(PATH_FISHHISTORYDB, iPlayerName, ymd);
        }
    }
}
