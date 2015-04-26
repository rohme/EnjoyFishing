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
using System.Xml.XPath;

namespace EnjoyFishing
{
    public class FishDB
    {
        public const string FISHNAME_UNKNOWN_FISH = "魚_";
        public const string FISHNAME_UNKNOWN_ITEM = "外_";
        public const string FISHNAME_UNKNOWN_MONSTER = "モ_";
        public const string FISHNAME_UNKNOWN = "？_";
        public const string PATH_FISHDB = "FishDB";
        public const string FILENAME_RODDB = "Rod.xml";
        public const string FILENAME_BAITDB = "Bait.xml";
        public const string FILENAME_GEARDB = "Gear.xml";
        private const string VERSION = "1.0.5";

        private LoggerTool logger;
        private List<string> _Rods = new List<string>();
        private List<string> _Baits = new List<string>();

        #region コンストラクタ
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="iLogger">LoggerTool</param>
        public FishDB(LoggerTool iLogger)
        {
            logger = iLogger;
            foreach(RodDBRodModel rod in SelectRod())
            {
                this._Rods.Add(rod.RodName);
            }
            foreach(BaitDBBaitModel bait in SelectBait())
            {
                this._Baits.Add(bait.BaitName);
            }
        }
        #endregion

        #region メンバ
        public List<string> Rods { get { return _Rods; } }
        public List<string> Baits { get { return _Baits; } }
        #endregion
        
        #region FishDB
        /// <summary>
        /// 指定された竿の魚情報を取得する
        /// </summary>
        /// <param name="iRodName"></param>
        /// <returns></returns>
        public FishDBModel SelectAll(string iRodName)
        {
            return getFishDB(iRodName);
        }
        /// <summary>
        /// 全竿に登録されている魚名を取得する
        /// </summary>
        /// <returns>魚名称</returns>
        public List<string> SelectAllFishName()
        {
            logger.Output(LogLevelKind.DEBUG, string.Format("{0}", MethodBase.GetCurrentMethod().Name));
            List<string> ret = new List<string>();
            foreach (string rod in _Rods)
            {
                FishDBModel fishDB = getFishDB(rod);
                foreach (FishDBFishModel fish in fishDB.Fishes)
                {
                    if (!ret.Contains(fish.FishName))
                    {
                        ret.Add(fish.FishName);
                    }
                }
            }
            ret.Sort();
            return ret;
        }
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
        /// 魚を取得する（ID,Zone）
        /// </summary>
        /// <param name="iRodName">竿名称</param>
        /// <param name="iID1">ID1</param>
        /// <param name="iID2">ID2</param>
        /// <param name="iID3">ID3</param>
        /// <param name="iID4">ID4</param>
        /// <param name="iWithUnknownFish">不明魚も返す場合Trueを指定</param>
        /// <returns></returns>
        public FishDBFishModel SelectFishFromIDZone(string iRodName, int iID1, int iID2, int iID3, int iID4, string iZoneName, bool iWithUnknownFish)
        {
            logger.Output(LogLevelKind.DEBUG, string.Format("{0} RodName={1} ID1={2} ID2={3} ID3={4} ID4={5} ZoneName={6} WithUnknownFish={7}", MethodBase.GetCurrentMethod().Name, iRodName, iID1, iID2, iID3, iID4, iZoneName, iWithUnknownFish));
            FishDBModel fishDB = getFishDB(iRodName);
            foreach (FishDBFishModel fish in fishDB.Fishes)
            {
                if (iWithUnknownFish == true ||
                   (iWithUnknownFish == false && fish.FishType != FishDBFishTypeKind.UnknownSmallFish &&
                                                 fish.FishType != FishDBFishTypeKind.UnknownLargeFish &&
                                                 fish.FishType != FishDBFishTypeKind.UnknownItem &&
                                                 fish.FishType != FishDBFishTypeKind.UnknownMonster &&
                                                 fish.FishType != FishDBFishTypeKind.Unknown))
                {
                    if (fish.IDs.Contains(new FishDBIdModel(iID1, iID2, iID3, iID4)) &&
                        fish.ZoneNames.Contains(iZoneName))
                    {
                        logger.VarDump(fish);
                        return fish;
                    }
                }
            }
            return new FishDBFishModel();
        }
        public bool AddFish(string iRodName, string iFishName, FishDBFishTypeKind iFishType, FishDBIdModel iID, string iZoneName, string iBaitName)
        {
            logger.Output(LogLevelKind.DEBUG, string.Format("{0} RodName={1} FishName={2} FishType={3} ID={4} ZoneName={5} BaitName={6}", 
                MethodBase.GetCurrentMethod().Name, iRodName, iFishName, iFishType, iID, iZoneName, iBaitName));
            FishDBModel fishDB = getFishDB(iRodName);
            fishDB.Version = VERSION;
            fishDB.RodName = iRodName;

            //不明魚として登録されている場合、削除する
            for (int fishIdx = 0; fishIdx < fishDB.Fishes.Count; fishIdx++)
            {
                if (fishDB.Fishes[fishIdx].IDs.Contains(iID) &&
                    fishDB.Fishes[fishIdx].ZoneNames.Contains(iZoneName) &&
                    (fishDB.Fishes[fishIdx].FishType == FishDBFishTypeKind.UnknownSmallFish ||
                     fishDB.Fishes[fishIdx].FishType == FishDBFishTypeKind.UnknownLargeFish ||
                     fishDB.Fishes[fishIdx].FishType == FishDBFishTypeKind.UnknownItem ||
                     fishDB.Fishes[fishIdx].FishType == FishDBFishTypeKind.UnknownMonster ||
                     fishDB.Fishes[fishIdx].FishType == FishDBFishTypeKind.Unknown))
                {
                    fishDB.Fishes[fishIdx].ZoneNames.Remove(iZoneName);
                    //エリア情報が無くなっった場合、魚情報を削除する
                    if (fishDB.Fishes[fishIdx].ZoneNames.Count == 0)
                    {
                        fishDB.Fishes.RemoveAt(fishIdx);
                    }
                    break;
                }

            }

            //更新処理
            bool foundFlg = false;
            for (int fishIdx = 0; fishIdx < fishDB.Fishes.Count; fishIdx++)
            {
                if (fishDB.Fishes[fishIdx].FishName == iFishName)
                {
                    foundFlg = true;
                    fishDB.Fishes[fishIdx].FishName = iFishName;
                    fishDB.Fishes[fishIdx].FishType = iFishType;
                    if (!fishDB.Fishes[fishIdx].IDs.Contains(iID)) fishDB.Fishes[fishIdx].IDs.Add(iID);
                    if (!fishDB.Fishes[fishIdx].ZoneNames.Contains(iZoneName)) fishDB.Fishes[fishIdx].ZoneNames.Add(iZoneName);
                    if (!fishDB.Fishes[fishIdx].BaitNames.Contains(iBaitName)) fishDB.Fishes[fishIdx].BaitNames.Add(iBaitName);
                }
            }
            //新規追加処理
            if (!foundFlg)
            {
                FishDBFishModel fish = new FishDBFishModel();
                fish.FishName = iFishName;
                fish.FishType = iFishType;
                fish.IDs.Add(iID);
                fish.ZoneNames.Add(iZoneName);
                fish.BaitNames.Add(iBaitName);
                fishDB.Fishes.Add(fish);
            }
            //ソート
            fishDB.Fishes.Sort(FishDBFishModel.SortTypeName);
            for (int i = 0; i < fishDB.Fishes.Count; i++)
            {
                fishDB.Fishes[i].IDs.Sort(FishDBIdModel.SortCountCritical);
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
        /// ファイルコンバーター
        /// </summary>
        /// <returns></returns>
        public void Converter()
        {
            string[] xmlFileNames = Directory.GetFiles(PATH_FISHDB);
            foreach (string xmlFileName in xmlFileNames)
            {
                //string filename = Path.GetFileName(xmlFileName);
                List<string> regGroupStr = new List<string>();
                if (MiscTool.GetRegexString(xmlFileName, PATH_FISHDB + "\\\\(.*)\\.xml$", out regGroupStr))
                {
                    string rodName = regGroupStr[0];
                    if (!_Rods.Contains(rodName)) continue;
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
                            logger.Output(LogLevelKind.INFO, string.Format("FishDBのコンバート 1.0.0→1.0.5 {0}", xmlFileName));
                            convert1_0_0to1_0_5(xmlFileName, rodName);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// xmlファイルをコンバートする（1.0.0→1.0.5）
        /// </summary>
        private void convert1_0_0to1_0_5(string iXmlFileName, string iRodName)
        {
            FishDBModel1_0_0 fishdb1_0_0 = getFishDB1_0_0(iRodName);
            FishDBModel fishdb1_0_5 = new FishDBModel();
            fishdb1_0_5.Version = "1.0.5";
            fishdb1_0_5.RodName = fishdb1_0_0.RodName;
            foreach (FishDBFishModel1_0_0 fish1_0_0 in fishdb1_0_0.Fishes)
            {
                FishDBFishModel fish1_0_5 = new FishDBFishModel();
                //FishDBFishTypeKind.Monster→FishDBFishTypeKind.UnknownMonster
                if (fish1_0_0.FishType == FishDBFishTypeKind.Monster)
                {
                    fish1_0_0.FishType = FishDBFishTypeKind.UnknownMonster;
                }
                fish1_0_5.FishType = fish1_0_0.FishType;
                //FishName
                if (fish1_0_0.FishType == FishDBFishTypeKind.UnknownSmallFish ||
                    fish1_0_0.FishType == FishDBFishTypeKind.UnknownLargeFish ||
                    fish1_0_0.FishType == FishDBFishTypeKind.UnknownItem ||
                    fish1_0_0.FishType == FishDBFishTypeKind.UnknownMonster ||
                    fish1_0_0.FishType == FishDBFishTypeKind.Unknown)
                {
                    if (fish1_0_0.IDs.Count > 0)
                    {
                        fish1_0_5.FishName = FishDB.GetTmpFishNameFromFishType(fish1_0_0.FishType, fish1_0_0.IDs[0].ID1, fish1_0_0.IDs[0].ID2, fish1_0_0.IDs[0].ID3, fish1_0_0.IDs[0].ID4);
                    }
                    else
                    {
                        fish1_0_5.FishName = fish1_0_0.FishName;
                    }
                }
                else
                {
                    fish1_0_5.FishName = fish1_0_0.FishName;
                }
                //ID
                foreach (FishDBIdModel1_0_0 id1_0_0 in fish1_0_0.IDs)
                {
                    FishDBIdModel id1_0_5 = new FishDBIdModel();
                    id1_0_5.ID1 = id1_0_0.ID1;
                    id1_0_5.ID2 = id1_0_0.ID2;
                    id1_0_5.ID3 = id1_0_0.ID3;
                    id1_0_5.ID4 = id1_0_0.ID4;
                    id1_0_5.Count = id1_0_0.Count;
                    id1_0_5.Critical = id1_0_0.Critical;
                    id1_0_5.ItemType = FishDBItemTypeKind.Common;
                    fish1_0_5.IDs.Add(id1_0_5);
                }
                //エリア 初期化する
                fish1_0_5.ZoneNames = new List<string>();
                //エサ
                fish1_0_5.BaitNames = fish1_0_0.BaitNames;

                fishdb1_0_5.Fishes.Add(fish1_0_5);
            }

            //バックアップ
            string backupFileName = iXmlFileName + ".bak";
            if (File.Exists(backupFileName)) File.Delete(backupFileName);
            File.Copy(iXmlFileName, backupFileName);
            //xml書き込み
            putFishDB(iRodName, fishdb1_0_5);
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
        /// xmlの内容を全て取得する(1.0.0)
        /// </summary>
        /// <returns>RodDBModel</returns>
        private FishDBModel1_0_0 getFishDB1_0_0(string iRodName)
        {
            string xmlFilename = PATH_FISHDB + @"\" + iRodName + ".xml";
            FishDBModel1_0_0 fishdb = new FishDBModel1_0_0(iRodName);
            if (File.Exists(xmlFilename))
            {
                for (int i = 0; i < Constants.FILELOCK_RETRY_COUNT; i++)
                {
                    try
                    {
                        using (FileStream fs = new FileStream(xmlFilename, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(FishDBModel1_0_0));
                            fishdb = (FishDBModel1_0_0)serializer.Deserialize(fs);
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
            ret.Sort(RodDBModel.SortTypeName);
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
            ret.Sort(BaitDBModel.SortTypeName);
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
        /// <returns>装備の一覧</returns>
        public List<GearDBGearModel> SelectGear()
        {
            return SelectGear(string.Empty, GearDBPositionKind.Unknown);
        }
        /// <summary>
        /// 装備を取得する
        /// </summary>
        /// <param name="iSearchString">餌名称（正規表現）</param>
        /// <returns>装備の一覧</returns>
        public List<GearDBGearModel> SelectGear(string iSearchString, GearDBPositionKind iPosition)
        {
            logger.Output(LogLevelKind.DEBUG, string.Format("{0} SearchString={1}", MethodBase.GetCurrentMethod().Name, iSearchString));
            List<GearDBGearModel> ret = new List<GearDBGearModel>();
            GearDBModel gearDB = getGearDB();
            if (iSearchString == string.Empty && iPosition == GearDBPositionKind.Unknown)
            {
                ret = gearDB.Gear;
            }
            else
            {
                foreach (GearDBGearModel gear in gearDB.Gear)
                {
                    if ((iSearchString != string.Empty && iPosition != GearDBPositionKind.Unknown && MiscTool.IsRegexString(gear.GearName, iSearchString) && gear.Position == iPosition) ||
                       (iSearchString != string.Empty && iPosition == GearDBPositionKind.Unknown && MiscTool.IsRegexString(gear.GearName, iSearchString)) ||
                       (iSearchString == string.Empty && iPosition != GearDBPositionKind.Unknown && gear.Position == iPosition))
                    {
                        ret.Add(gear);
                    }
                }
            }
            ret.Sort(GearDBModel.SortTypeName);
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
        /// <summary>
        /// 名称不明の魚の一時名称を取得する
        /// </summary>
        /// <param name="iFishType">魚タイプ</param>
        /// <param name="iID1">ID1</param>
        /// <param name="iID2">ID2</param>
        /// <param name="iID3">ID3</param>
        /// <param name="iID4">ID4</param>
        /// <returns>一時名称</returns>
        public static string GetTmpFishNameFromFishType(FishDBFishTypeKind iFishType, int iID1, int iID2, int iID3, int iID4)
        {
            string tmpFishName = string.Empty;
            switch (iFishType)
            {
                case FishDBFishTypeKind.SmallFish:
                case FishDBFishTypeKind.UnknownSmallFish:
                case FishDBFishTypeKind.LargeFish:
                case FishDBFishTypeKind.UnknownLargeFish:
                    tmpFishName = FishDB.FISHNAME_UNKNOWN_FISH;
                    break;
                case FishDBFishTypeKind.Item:
                case FishDBFishTypeKind.UnknownItem:
                    tmpFishName = FishDB.FISHNAME_UNKNOWN_ITEM;
                    break;
                case FishDBFishTypeKind.UnknownMonster:
                    tmpFishName = FishDB.FISHNAME_UNKNOWN_MONSTER;
                    break;
                default:
                    tmpFishName = FishDB.FISHNAME_UNKNOWN;
                    break;
            }
            return string.Format("{0}{1:000}-{2:000}-{3:000}-{4:000}", tmpFishName, iID1, iID2, iID3, iID4);
        }        /// <summary>
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
                ret = xNavi.SelectSingleNode("/Rod/@version").Value;
            }
            catch (NullReferenceException)
            {
                ret = "1.0.0";
            }
            return ret;
        }
    }
}
