using MiscTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

namespace EnjoyFishing
{
    public class HarakiriDB
    {
        private const string DIRECTORY_HARAKIRIDB = "History";
        private const string FILENAME_HARAKIRIDB = "Harakiri.xml";

        private LoggerTool logger;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public HarakiriDB(LoggerTool iLogger)
        {
            logger = iLogger;
        }

        /// <summary>
        /// 履歴に追加
        /// </summary>
        /// <param name="iPlayername">プレイヤー名</param>
        /// <param name="iHistory">FishHistoryDBFishModel</param>
        /// <returns>True:成功</returns>
        public bool Add(HarakiriDBHistoryModel iHistory)
        {
            HarakiriDBModel harakiridb = getHarakiriDB();
            harakiridb.History.Add(iHistory);
            return putHarakiriDB(harakiridb);
        }

        /// <summary>
        /// xmlの内容を全て取得する
        /// </summary>
        /// <returns>HarakiriDBModel</returns>
        private HarakiriDBModel getHarakiriDB()
        {
            string xmlFilename = Path.Combine(DIRECTORY_HARAKIRIDB, FILENAME_HARAKIRIDB);
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
        /// <summary>
        /// xmlへ書き込む
        /// </summary>
        /// <param name="iPlayerName">プレイヤー名</param>
        /// <param name="iHarakiriDB">HarakiriDBModel</param>
        /// <returns>True:成功</returns>
        private bool putHarakiriDB(HarakiriDBModel iHarakiriDB)
        {
            string xmlFilename = Path.Combine(DIRECTORY_HARAKIRIDB, FILENAME_HARAKIRIDB);
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
    }
}
