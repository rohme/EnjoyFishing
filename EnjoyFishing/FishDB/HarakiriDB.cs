using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using NLog;

namespace EnjoyFishing
{
    public class HarakiriDB
    {
        private const string DIRECTORY_HARAKIRIDB = "FishDB";
        private const string FILENAME_HARAKIRIDB = "Harakiri.xml";
        private const string VERSION = "1.0.0";

        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public HarakiriDB()
        {
        }

        /// <summary>
        /// サマリーを取得する
        /// </summary>
        public HarakiriDBModel GetSummary()
        {
            HarakiriDBModel ret = getHarakiriDB();
            //ソート
            ret.Fishes.Sort(HarakiriDBFishModel.SortTypeCount);
            for (int i = 0; i < ret.Fishes.Count; i++)
            {
                ret.Fishes[i].Items.Sort(HarakiriDBItemModel.SortTypeCount);
            }
            return ret;
        }

        /// <summary>
        /// 履歴に追加
        /// </summary>
        /// <param name="iPlayername">プレイヤー名</param>
        /// <param name="iHistory">FishHistoryDBFishModel</param>
        /// <returns>True:成功</returns>
        public bool Add(string iPlayerName, DateTime iEarthDate, string iVanaDate, string iFishName, string iItemName)
        {
            HarakiriDBModel harakiriDB = getHarakiriDB();
            harakiriDB.Version = VERSION;
            if (harakiriDB.Fishes.Contains(new HarakiriDBFishModel(iFishName)))
            {
                HarakiriDBFishModel fish = harakiriDB.Fishes[harakiriDB.Fishes.IndexOf(new HarakiriDBFishModel(iFishName))];
                fish.Count++;
                if (iItemName != string.Empty)
                {
                    if (fish.Items.Contains(new HarakiriDBItemModel(iItemName)))
                    {
                        //Items更新
                        HarakiriDBItemModel item = fish.Items[fish.Items.IndexOf(new HarakiriDBItemModel(iItemName))];
                        item.Count++;
                    }
                    else
                    {
                        //Items追加
                        HarakiriDBItemModel item = new HarakiriDBItemModel();
                        item.ItemName = iItemName;
                        item.Count = 1;
                        fish.Items.Add(item);
                    }
                }
            }
            else
            {
                //Fishes追加
                HarakiriDBFishModel fish = new HarakiriDBFishModel();
                fish.FishName = iFishName;
                fish.Count = 1;
                if (iItemName != string.Empty)
                {
                    HarakiriDBItemModel item = new HarakiriDBItemModel();
                    item.ItemName = iItemName;
                    item.Count = 1;
                    fish.Items.Add(item);
                }
                harakiriDB.Fishes.Add(fish);
            }
            //ソート
            harakiriDB.Fishes.Sort(HarakiriDBFishModel.SortTypeCount);
            for (int i = 0; i < harakiriDB.Fishes.Count; i++)
            {
                harakiriDB.Fishes[i].Items.Sort(HarakiriDBItemModel.SortTypeCount);
            }
            //履歴DBに保存
            FishHistoryDB historyDB = new FishHistoryDB(iPlayerName, iEarthDate);
            if (historyDB.AddHarakiri(iPlayerName, new FishHistoryDBHarakiriModel(iEarthDate.ToString("yyyy/MM/dd HH:mm:ss"), iVanaDate, iFishName, iItemName)))
            {
                //ハラキリDBに保存
                return putHarakiriDB(harakiriDB);
            }
            return false;
        }

        /// <summary>
        /// xmlの内容を全て取得する
        /// </summary>
        /// <returns>HarakiriDBModel</returns>
        private HarakiriDBModel getHarakiriDB()
        {
            string xmlFilename = Path.Combine(DIRECTORY_HARAKIRIDB, FILENAME_HARAKIRIDB);
            try
            {
                HarakiriDBModel harakiridb = new HarakiriDBModel();
                if (!Directory.Exists(DIRECTORY_HARAKIRIDB))
                {
                    Directory.CreateDirectory(DIRECTORY_HARAKIRIDB);
                }
                if (File.Exists(xmlFilename))
                {
                    for (int i = 0; i < Constants.FILELOCK_RETRY_COUNT; i++)
                    {
                        try
                        {
                            using (FileStream fs = new FileStream(xmlFilename, FileMode.Open, FileAccess.Read, FileShare.Read))
                            {
                                XmlSerializer serializer = new XmlSerializer(typeof(HarakiriDBModel));
                                harakiridb = (HarakiriDBModel)serializer.Deserialize(fs);
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
                return harakiridb;
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
        /// <param name="iHarakiriDB">HarakiriDBModel</param>
        /// <returns>True:成功</returns>
        private bool putHarakiriDB(HarakiriDBModel iHarakiriDB)
        {
            string xmlFilename = Path.Combine(DIRECTORY_HARAKIRIDB, FILENAME_HARAKIRIDB);
            try
            {
                if (!Directory.Exists(DIRECTORY_HARAKIRIDB))
                {
                    Directory.CreateDirectory(DIRECTORY_HARAKIRIDB);
                }

                for (int i = 0; i < Constants.FILELOCK_RETRY_COUNT; i++)
                {
                    try
                    {
                        using (FileStream fs = new FileStream(xmlFilename, FileMode.Create, FileAccess.Write, FileShare.None))//ファイルロック
                        {
                            using (StreamWriter sw = new StreamWriter(fs, new UTF8Encoding(false)))
                            {
                                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                                ns.Add(String.Empty, String.Empty);
                                XmlSerializer serializer = new XmlSerializer(typeof(HarakiriDBModel));
                                serializer.Serialize(sw, iHarakiriDB, ns);
                                //書き込み
                                sw.Flush();
                                sw.Close();
                            }
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
            catch (Exception e)
            {
                logger.Fatal("{0}の登録中にエラーが発生しました。", xmlFilename);
                throw e;
            }
        }
    }
}
