using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using MiscTools;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace EnjoyFishing
{
    public class FishDB
    {
        public const string FISHNAME_UNKNOWN_FISH = "魚_";
        public const string FISHNAME_UNKNOWN_ITEM = "外道_";
        public const string FISHNAME_UNKNOWN_MONSTER = "モンスター_";
        public const string FISHNAME_UNKNOWN = "不明_";
        private const string PATH_FISHDB = "FishDB";
        private const string FILENAME_RODDB = "Rod.xml";
        private const string FILENAME_BAITDB = "Bait.xml";
        private const string FILENAME_GEARDB = "Gear.xml";

        private LoggerTool logger;
        private List<string> _Rods = new List<string>();
        private List<string> _Baits = new List<string>();
        private List<string> _Gears = new List<string>();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="iLogger">LoggerTool</param>
        public FishDB(LoggerTool iLogger)
        {
            logger = iLogger;
            foreach(RodDBRodModel rod in SelectRod())
            {
                _Rods.Add(rod.RodName);
            }
            foreach(BaitDBBaitModel bait in SelectBait())
            {
                _Baits.Add(bait.BaitName);
            }
            foreach(GearDBGearModel gear in SelectGear())
            {
                _Gears.Add(gear.GearName);
            }
        }

        #region メンバ
        public List<string> Rods{ get {return _Rods; }}
        public List<string> Baits { get { return _Baits; } }
        public List<string> Gears { get { return _Gears; } }
        #endregion
        
        #region FishDB
        /// <summary>
        /// 魚リストの取得
        /// </summary>
        /// <param name="iRodName">竿名称</param>
        /// <param name="iZoneName">エリア名称</param>
        /// <param name="iBaitName">エサ名称</param>
        /// <returns></returns>
        public List<FishDBFishModel> SelectFishList(string iRodName, string iZoneName, string iBaitName)
        {
            logger.Output(LogLevelKind.DEBUG, string.Format("{0} RodName={1} ZoneName={2} BaitName={3}", MethodBase.GetCurrentMethod().Name ,iRodName, iZoneName, iBaitName));
            List<FishDBFishModel> ret = new List<FishDBFishModel>();
            FishDBModel fishDB = getFishDB(iRodName);
            foreach (FishDBFishModel fish in fishDB.Fishes)
            {
                bool foundZoneFlg = false;
                if (iZoneName != string.Empty)
                {
                    foundZoneFlg = fish.ZoneNames.Contains(iZoneName);
                }
                else
                {
                    foundZoneFlg = true;
                }
                bool foundBaitFlg = false;
                if (iBaitName != string.Empty)
                {
                    foundBaitFlg = fish.BaitNames.Contains(iBaitName);
                }
                else
                {
                    foundBaitFlg = true;
                }
                if (foundZoneFlg && foundBaitFlg)
                {
                    ret.Add(fish);
                }
            }
            ret.Sort(FishDBFishModel.SortTypeName);
            foreach (FishDBFishModel v in ret)
            {
                logger.VarDump(v);
            }
            return ret;
        }
        /// <summary>
        /// 魚を取得する（魚名称）
        /// </summary>
        /// <param name="iRodName">竿名称</param>
        /// <param name="iFishName">魚名称</param>
        /// <returns></returns>
        public FishDBFishModel SelectFishFromName(string iRodName, string iFishName)
        {
            logger.Output(LogLevelKind.DEBUG, string.Format("{0} RodName={1} FishName={2}", MethodBase.GetCurrentMethod().Name, iRodName, iFishName));
            FishDBFishModel ret = new FishDBFishModel();
            FishDBModel fishDB = getFishDB(iRodName);
            foreach (FishDBFishModel fish in fishDB.Fishes)
            {
                if (fish.FishName == iFishName)
                {
                    ret = fish;
                    break;
                }
            }
            logger.VarDump(ret);
            return ret;
        }
        /// <summary>
        /// 魚を取得する（ID,Name）
        /// </summary>
        /// <param name="iRodName">竿名称</param>
        /// <param name="iID1">ID1</param>
        /// <param name="iID2">ID2</param>
        /// <param name="iID3">ID3</param>
        /// <param name="iID4">ID4</param>
        /// <param name="iWithUnknownFish">不明魚も返す場合Trueを指定</param>
        /// <returns></returns>
        public FishDBFishModel SelectFishFromIDName(string iRodName, int iID1, int iID2, int iID3, int iID4, string iFishName, bool iWithUnknownFish)
        {
            logger.Output(LogLevelKind.DEBUG, string.Format("{0} RodName={1} ID1={2} ID2={3} ID3={4} ID4={5} WithUnknownFish={6}", MethodBase.GetCurrentMethod().Name, iRodName, iID1, iID2, iID3, iID4, iWithUnknownFish));
            FishDBModel fishDB = getFishDB(iRodName);
            foreach (FishDBFishModel fish in fishDB.Fishes)
            {
                if (iWithUnknownFish == true ||
                   (iWithUnknownFish == false && !MiscTool.IsRegexString(fish.FishName, FISHNAME_UNKNOWN_FISH + "(.*)") &&
                                                 !MiscTool.IsRegexString(fish.FishName, FISHNAME_UNKNOWN_ITEM + "(.*)") &&
                                                 !MiscTool.IsRegexString(fish.FishName, FISHNAME_UNKNOWN_MONSTER + "(.*)") &&
                                                 !MiscTool.IsRegexString(fish.FishName, FISHNAME_UNKNOWN + "(.*)")))
                {
                    foreach (FishDBIdModel id in fish.IDs)
                    {
                        if (fish.FishName == iFishName && id.ID1 == iID1 && id.ID2 == iID2 && id.ID3 == iID3 && id.ID4 == iID4)
                        {
                            logger.VarDump(fish);
                            return fish;
                        }
                    }
                }
            }
            return new FishDBFishModel();
        }
        /// <summary>
        /// 魚を取得する（ID）
        /// </summary>
        /// <param name="iRodName">竿名称</param>
        /// <param name="iID1">ID1</param>
        /// <param name="iID2">ID2</param>
        /// <param name="iID3">ID3</param>
        /// <param name="iID4">ID4</param>
        /// <param name="iWithUnknownFish">不明魚も返す場合Trueを指定</param>
        /// <returns></returns>
        public List<FishDBFishModel> SelectFishFromID(string iRodName, int iID1, int iID2, int iID3, int iID4, bool iWithUnknownFish)
        {
            logger.Output(LogLevelKind.DEBUG, string.Format("{0} RodName={1} ID1={2} ID2={3} ID3={4} ID4={5} WithUnknownFish={6}", MethodBase.GetCurrentMethod().Name, iRodName, iID1, iID2, iID3, iID4, iWithUnknownFish));
            List<FishDBFishModel> ret = new List<FishDBFishModel>();
            FishDBModel fishDB = getFishDB(iRodName);
            foreach (FishDBFishModel fish in fishDB.Fishes)
            {
                if (iWithUnknownFish == true ||
                   (iWithUnknownFish == false && !MiscTool.IsRegexString(fish.FishName, FISHNAME_UNKNOWN_FISH + "(.*)") &&
                                                 !MiscTool.IsRegexString(fish.FishName, FISHNAME_UNKNOWN_ITEM + "(.*)") &&
                                                 !MiscTool.IsRegexString(fish.FishName, FISHNAME_UNKNOWN_MONSTER + "(.*)") &&
                                                 !MiscTool.IsRegexString(fish.FishName, FISHNAME_UNKNOWN + "(.*)")))
                {
                    foreach (FishDBIdModel id in fish.IDs)
                    {
                        if (id.ID1 == iID1 && id.ID2 == iID2 && id.ID3 == iID3 && id.ID4 == iID4)
                        {
                            ret.Add(fish);
                        }
                    }
                }
            }
            logger.VarDump(ret);
            return ret;
        }
        /// <summary>
        /// 魚を取得する（エリア・竿）
        /// </summary>
        /// <param name="iZoneName">エリア名称</param>
        /// <param name="iRodName">竿名称</param>
        /// <returns></returns>
        public List<FishDBFishModel> SelectFishFromZoneRod(string iZoneName, string iRodName)
        {
            logger.Output(LogLevelKind.DEBUG, string.Format("{0} RodName={1} ZoneName={2}", MethodBase.GetCurrentMethod().Name, iRodName, iZoneName));
            List<FishDBFishModel> ret = new List<FishDBFishModel>();
            FishDBModel fishDB = getFishDB(iRodName);
            foreach (FishDBFishModel fish in fishDB.Fishes)
            {
                if (fish.ZoneNames.Contains(iZoneName))
                {
                    ret.Add(fish);
                }
            }
            logger.VarDump(ret);
            return ret;
        }
        /// <summary>
        /// 魚を取得する（エリア・竿・餌）
        /// </summary>
        /// <param name="iZoneName">エリア名称</param>
        /// <param name="iRodName">竿名称</param>
        /// <param name="iBaitName">餌名称</param>
        /// <returns></returns>
        public List<FishDBFishModel> SelectFishFromZoneRodBait(string iZoneName, string iRodName, string iBaitName)
        {
            logger.Output(LogLevelKind.DEBUG, string.Format("{0} RodName={1} ZoneName={2} BaitName={3}", MethodBase.GetCurrentMethod().Name, iRodName, iZoneName, iBaitName));
            List<FishDBFishModel> ret = new List<FishDBFishModel>();
            FishDBModel fishDB = getFishDB(iRodName);
            foreach (FishDBFishModel fish in fishDB.Fishes)
            {
                if (fish.ZoneNames.Contains(iZoneName) && fish.BaitNames.Contains(iBaitName))
                {
                    ret.Add(fish);
                }
            }
            logger.VarDump(ret);
            return ret;
        }
        /// <summary>
        /// 魚を更新する
        /// </summary>
        /// <param name="iRodName">竿名称</param>
        /// <param name="iFish">魚</param>
        /// <returns>True:成功</returns>
        public bool UpdateFish(string iRodName, FishDBFishModel iFish)
        {
            logger.Output(LogLevelKind.DEBUG, string.Format("{0} RodName={1}", MethodBase.GetCurrentMethod().Name, iRodName));
            logger.VarDump(iFish, "引数");
            FishDBModel fishDB = getFishDB(iRodName);
            bool foundFishFlg = false;

            //不明魚として登録されている魚を削除する
            if (!MiscTool.IsRegexString(iFish.FishName, FISHNAME_UNKNOWN_FISH + "(.*)") &&
                !MiscTool.IsRegexString(iFish.FishName, FISHNAME_UNKNOWN_ITEM + "(.*)") &&
                !MiscTool.IsRegexString(iFish.FishName, FISHNAME_UNKNOWN_MONSTER + "(.*)") &&
                !MiscTool.IsRegexString(iFish.FishName, FISHNAME_UNKNOWN + "(.*)"))
            {
                int deleteIdx = -1;
                for (int i = 0; i < fishDB.Fishes.Count; i++)
                {
                    if (MiscTool.IsRegexString(fishDB.Fishes[i].FishName, FISHNAME_UNKNOWN_FISH + "(.*)") ||
                        MiscTool.IsRegexString(fishDB.Fishes[i].FishName, FISHNAME_UNKNOWN_ITEM + "(.*)") ||
                        MiscTool.IsRegexString(fishDB.Fishes[i].FishName, FISHNAME_UNKNOWN_MONSTER + "(.*)") ||
                        MiscTool.IsRegexString(fishDB.Fishes[i].FishName, FISHNAME_UNKNOWN + "(.*)"))
                    {
                        for (int j = 0; j < fishDB.Fishes[i].IDs.Count; j++)
                        {
                            if (iFish.IDs.Count > 0 &&
                                iFish.IDs[0].ID1 == fishDB.Fishes[i].IDs[j].ID1 &&
                                iFish.IDs[0].ID2 == fishDB.Fishes[i].IDs[j].ID2 &&
                                iFish.IDs[0].ID3 == fishDB.Fishes[i].IDs[j].ID3 &&
                                iFish.IDs[0].ID4 == fishDB.Fishes[i].IDs[j].ID4)
                            {
                                deleteIdx = i;
                            }
                        }
                    }
                        
                }
                if (deleteIdx >= 0)
                {
                    fishDB.Fishes.RemoveAt(deleteIdx);
                }
            }

            //xmlに存在すれば更新する
            for (int i = 0; i < fishDB.Fishes.Count; i++)
            {
                if (fishDB.Fishes[i].FishName == iFish.FishName)
                {
                    fishDB.Fishes[i].FishType = iFish.FishType;
                    if (iFish.IDs.Count > 0)
                    {
                        bool foundFlg = false;
                        for (int ii = 0; ii < fishDB.Fishes[i].IDs.Count; ii++)
                        {
                            if (fishDB.Fishes[i].IDs[ii].ID1 == iFish.IDs[0].ID1 &&
                                fishDB.Fishes[i].IDs[ii].ID2 == iFish.IDs[0].ID2 &&
                                fishDB.Fishes[i].IDs[ii].ID3 == iFish.IDs[0].ID3 &&
                                fishDB.Fishes[i].IDs[ii].ID4 == iFish.IDs[0].ID4)
                            {
                                foundFlg = true;
                                fishDB.Fishes[i].IDs[ii].Count = iFish.IDs[0].Count;
                                fishDB.Fishes[i].IDs[ii].Critical = iFish.IDs[0].Critical;
                                break;
                            }
                        }
                        if (!foundFlg)
                        {
                            fishDB.Fishes[i].IDs.Add(iFish.IDs[0]);
                        }
                    }
                    if (iFish.ZoneNames.Count > 0)
                    {
                        if (!fishDB.Fishes[i].ZoneNames.Contains(iFish.ZoneNames[0]))
                        {
                            fishDB.Fishes[i].ZoneNames.Add(iFish.ZoneNames[0]);
                        }
                    }
                    if (iFish.BaitNames.Count > 0)
                    {
                        if (!fishDB.Fishes[i].BaitNames.Contains(iFish.BaitNames[0]))
                        {
                            fishDB.Fishes[i].BaitNames.Add(iFish.BaitNames[0]);
                        }
                    }
                    foundFishFlg = true;
                }
            }
            //xmlに存在しない場合は追加する
            if (!foundFishFlg)
            {
                fishDB.Fishes.Add(iFish);
            }
            //ソート
            fishDB.Fishes.Sort(FishDBFishModel.SortTypeName);
            for (int i = 0; i < fishDB.Fishes.Count; i++)
            {
                fishDB.Fishes[i].IDs.Sort(FishDBIdModel.SortCountID);
            }
            //Rod.xmlへ出力する
            if (!putFishDB(iRodName, fishDB)) return false;
            return true;
        }
        /// <summary>
        /// 魚を削除する
        /// </summary>
        /// <param name="iRod">竿</param>
        /// <returns>True:成功</returns>
        public bool DeleteFish(string iRodName, string iFishName)
        {
            logger.Output(LogLevelKind.DEBUG, string.Format("{0} RodName={1} FishName={2}", MethodBase.GetCurrentMethod().Name, iRodName, iFishName));
            FishDBModel fishDB = getFishDB(iRodName);
            //xmlに存在すれば削除する
            for (int i = 0; i < fishDB.Fishes.Count; i++)
            {
                if (fishDB.Fishes[i].FishName == iFishName)
                {
                    fishDB.Fishes.RemoveAt(i);
                    break;
                }
            }
            //Rod.xmlへ出力する
            if (!putFishDB(iRodName, fishDB)) return false;
            return true;
        }
        /// <summary>
        /// xmlの内容を全て取得する
        /// </summary>
        /// <returns>RodDBModel</returns>
        private FishDBModel getFishDB(string iRodName)
        {
            string xmlFilename = PATH_FISHDB + @"\" + iRodName + ".xml";
            FishDBModel fishdb = new FishDBModel(iRodName);
            if (File.Exists(xmlFilename))
            {
                for (int i = 0; i < Constants.FILELOCK_RETRY_COUNT; i++)
                {
                    try
                    {
                        using (FileStream fs = new FileStream(xmlFilename, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(FishDBModel));
                            fishdb = (FishDBModel)serializer.Deserialize(fs);
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
            return fishdb;
        }
        /// <summary>
        /// xmlに登録内容を書き出す
        /// </summary>
        /// <param name="iFishDB">RodDBModel</param>
        /// <returns>True：成功</returns>
        private bool putFishDB(string iRodName, FishDBModel iFishDB)
        {
            string xmlFilename = PATH_FISHDB + @"\" + iRodName + ".xml";
            for (int i = 0; i < Constants.FILELOCK_RETRY_COUNT; i++)
            {
                try 
                {
                    using (FileStream fs = new FileStream(xmlFilename, FileMode.Create, FileAccess.Write, FileShare.None))//ファイルロック
                    {
                        StreamWriter sw = new StreamWriter(fs, new UTF8Encoding(false));
                        XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                        ns.Add(String.Empty, String.Empty);
                        XmlSerializer serializer = new XmlSerializer(typeof(FishDBModel));
                        serializer.Serialize(sw, iFishDB, ns);
                        //書き込み
                        sw.Flush();
                        sw = null;
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

            return true;
        }
        #endregion

        #region RodDB
        /// <summary>
        /// 竿を取得する（引数無し）
        /// </summary>
        /// <returns>竿の一覧</returns>
        public List<RodDBRodModel> SelectRod()
        {
            return SelectRod(string.Empty);
        }
        /// <summary>
        /// 竿を取得する
        /// </summary>
        /// <param name="iSearchString">竿名称（正規表現）</param>
        /// <returns>竿の一覧</returns>
        public List<RodDBRodModel> SelectRod(string iSearchString)
        {
            logger.Output(LogLevelKind.DEBUG, string.Format("{0} SearchString={1}", MethodBase.GetCurrentMethod().Name, iSearchString));
            List<RodDBRodModel> ret = new List<RodDBRodModel>();
            RodDBModel roddb = getRodDB();
            if (iSearchString == string.Empty)
            {
                ret = roddb.Rod;
            }
            else
            {
                foreach (RodDBRodModel rod in roddb.Rod)
                {
                    if (MiscTool.IsRegexString(rod.RodName, iSearchString))
                    {
                        ret.Add(rod);
                    }
                }
            }
            logger.VarDump(ret);
            return ret;
        }
        /// <summary>
        /// Rod.xmlの内容を全て取得する
        /// </summary>
        /// <returns>RodDBModel</returns>
        private RodDBModel getRodDB()
        {
            string xmlFilename = PATH_FISHDB + @"\" + FILENAME_RODDB;
            RodDBModel roddb = new RodDBModel();
            if (File.Exists(xmlFilename))
            {
                FileStream fs = new FileStream(xmlFilename, System.IO.FileMode.Open);
                XmlSerializer serializer = new XmlSerializer(typeof(RodDBModel));
                roddb = (RodDBModel)serializer.Deserialize(fs);
                fs.Close();
            }
            return roddb;
        }
        /// <summary>
        /// Rod.xmlに登録内容を書き出す
        /// </summary>
        /// <param name="iRodDB">RodDBModel</param>
        /// <returns>True：成功</returns>
        private bool putRodDB(RodDBModel iRodDB)
        {
            string xmlFilename = PATH_FISHDB + @"\" + FILENAME_RODDB;
            StreamWriter sw = new StreamWriter(xmlFilename, false, new UTF8Encoding(false));
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add(String.Empty, String.Empty);
            XmlSerializer serializer = new XmlSerializer(typeof(RodDBModel));
            serializer.Serialize(sw, iRodDB, ns);
            //書き込み
            sw.Flush();
            sw.Close();
            sw = null;
            return true;
        }
        #endregion

        #region BaitDB
        /// <summary>
        /// 餌を取得する（引数無し）
        /// </summary>
        /// <returns>餌の一覧</returns>
        public List<BaitDBBaitModel> SelectBait()
        {
            return SelectBait(string.Empty);
        }
        /// <summary>
        /// 餌を取得する
        /// </summary>
        /// <param name="iSearchString">餌名称（正規表現）</param>
        /// <returns>餌の一覧</returns>
        public List<BaitDBBaitModel> SelectBait(string iSearchString)
        {
            logger.Output(LogLevelKind.DEBUG, string.Format("{0} SearchString={1}", MethodBase.GetCurrentMethod().Name, iSearchString));
            List<BaitDBBaitModel> ret = new List<BaitDBBaitModel>();
            BaitDBModel Baitdb = getBaitDB();
            if (iSearchString == string.Empty)
            {
                ret = Baitdb.Bait;
            }
            else
            {
                foreach (BaitDBBaitModel Bait in Baitdb.Bait)
                {
                    if (MiscTool.IsRegexString(Bait.BaitName, iSearchString))
                    {
                        ret.Add(Bait);
                    }
                }
            }
            logger.VarDump(ret);
            return ret;
        }
        /// <summary>
        /// Bait.xmlの内容を全て取得する
        /// </summary>
        /// <returns>BaitDBModel</returns>
        private BaitDBModel getBaitDB()
        {
            string xmlFilename = PATH_FISHDB + @"\" + FILENAME_BAITDB;
            BaitDBModel Baitdb = new BaitDBModel();
            if (File.Exists(xmlFilename))
            {
                FileStream fs = new FileStream(xmlFilename, System.IO.FileMode.Open);
                XmlSerializer serializer = new XmlSerializer(typeof(BaitDBModel));
                Baitdb = (BaitDBModel)serializer.Deserialize(fs);
                fs.Close();
            }
            return Baitdb;
        }
        /// <summary>
        /// Bait.xmlに登録内容を書き出す
        /// </summary>
        /// <param name="iBaitDB">BaitDBModel</param>
        /// <returns>True：成功</returns>
        private bool putBaitDB(BaitDBModel iBaitDB)
        {
            string xmlFilename = PATH_FISHDB + @"\" + FILENAME_BAITDB;
            StreamWriter sw = new StreamWriter(xmlFilename, false, new UTF8Encoding(false));
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add(String.Empty, String.Empty);
            XmlSerializer serializer = new XmlSerializer(typeof(BaitDBModel));
            serializer.Serialize(sw, iBaitDB, ns);
            //書き込み
            sw.Flush();
            sw.Close();
            sw = null;
            return true;
        }
        #endregion

        #region GearDB
        /// <summary>
        /// 装備を取得する（引数無し）
        /// </summary>
        /// <returns>餌の一覧</returns>
        public List<GearDBGearModel> SelectGear()
        {
            return SelectGear(string.Empty);
        }
        /// <summary>
        /// 装備を取得する
        /// </summary>
        /// <param name="iSearchString">餌名称（正規表現）</param>
        /// <returns>餌の一覧</returns>
        public List<GearDBGearModel> SelectGear(string iSearchString)
        {
            logger.Output(LogLevelKind.DEBUG, string.Format("{0} SearchString={1}", MethodBase.GetCurrentMethod().Name, iSearchString));
            List<GearDBGearModel> ret = new List<GearDBGearModel>();
            GearDBModel gearDB = getGearDB();
            if (iSearchString == string.Empty)
            {
                ret = gearDB.Gear;
            }
            else
            {
                foreach (GearDBGearModel Gear in gearDB.Gear)
                {
                    if (MiscTool.IsRegexString(Gear.GearName, iSearchString))
                    {
                        ret.Add(Gear);
                    }
                }
            }
            logger.VarDump(ret);
            return ret;
        }
        /// <summary>
        /// Gear.xmlの内容を全て取得する
        /// </summary>
        /// <returns>GearDBModel</returns>
        private GearDBModel getGearDB()
        {
            string xmlFilename = PATH_FISHDB + @"\" + FILENAME_GEARDB;
            GearDBModel gearDB = new GearDBModel();
            if (File.Exists(xmlFilename))
            {
                FileStream fs = new FileStream(xmlFilename, System.IO.FileMode.Open);
                XmlSerializer serializer = new XmlSerializer(typeof(GearDBModel));
                gearDB = (GearDBModel)serializer.Deserialize(fs);
                fs.Close();
            }
            return gearDB;
        }
        #endregion

        /// <summary>
        /// FishTypeが一時登録魚が判定
        /// </summary>
        /// <param name="iFishType">FishType</param>
        /// <returns>一時登録魚の場合Trueを返す</returns>
        public static bool IsTempFish(FishDBFishTypeKind iFishType)
        {
            if (iFishType == FishDBFishTypeKind.Unknown ||
                iFishType == FishDBFishTypeKind.UnknownSmallFish ||
                iFishType == FishDBFishTypeKind.UnknownLargeFish ||
                iFishType == FishDBFishTypeKind.UnknownItem)
            {
                return true;
            }
            return false;
        }
    }
}
