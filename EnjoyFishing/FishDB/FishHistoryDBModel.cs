using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFACETools;
using System.Xml.Serialization;
using System.Reflection;
using System.Collections;
using MiscTools;

namespace EnjoyFishing
{

    /// <summary>
    /// 釣り結果ステータス
    /// </summary>
    public enum FishResultStatusKind
    {
        Catch,
        NoBite,
        NoCatch,
        Release,
        LineBreak,
        RodBreak,
        Unknown,
    }
    /// <summary>
    /// だいじなもの有無
    /// </summary>
    public enum HasKeyItemKind
    {
        Yes,
        No,
        Unknown,
    }

    [XmlRoot("History")]
    public class FishHistoryDBModel
    {
        [XmlAttribute("version")]
        public string Version { get; set; }
        [XmlAttribute("player")]
        public string PlayerName { get; set; }
        [XmlAttribute("date")]
        public string EarthDate { get; set; }
        public int TimeElapsed { get; set; }
        [XmlArray("Fishes")]
        [XmlArrayItem("Fish")]
        public List<FishHistoryDBFishModel> Fishes { get; set; }
        [XmlArray("Harakiri")]
        [XmlArrayItem("Fish")]
        public List<FishHistoryDBHarakiriModel> Harakiri { get; set; }
        public FishHistoryDBModel()
        {
            this.Version = string.Empty;
            this.PlayerName = string.Empty;
            this.EarthDate = DateTime.Today.ToString();
            this.TimeElapsed = 0;
            this.Fishes = new List<FishHistoryDBFishModel>();
            this.Harakiri = new List<FishHistoryDBHarakiriModel>();
        }
    }
    public class FishHistoryDBFishModel
    {
        [XmlAttribute("name")]
        public string FishName { get; set; }
        [XmlAttribute("count")]
        public int FishCount { get; set; }
        [XmlAttribute("zone")]
        public string ZoneName { get; set; }
        [XmlAttribute("rod")]
        public string RodName { get; set; }
        [XmlAttribute("bait")]
        public string BaitName { get; set; }
        [XmlAttribute("id1")]
        public int ID1 { get; set; }
        [XmlAttribute("id2")]
        public int ID2 { get; set; }
        [XmlAttribute("id3")]
        public int ID3 { get; set; }
        [XmlAttribute("id4")]
        public int ID4 { get; set; }
        [XmlAttribute("critical")]
        public bool Critical { get; set; }
        [XmlAttribute("itemtype")]
        public FishDBItemTypeKind ItemType { get; set; }
        [XmlAttribute("fishtype")]
        public FishDBFishTypeKind FishType { get; set; }
        [XmlAttribute("result")]
        public FishResultStatusKind Result { get; set; }
        [XmlAttribute("earthtime")]
        public string EarthTime { get; set; }
        [XmlAttribute("vanatime")]
        public string VanaTime { get; set; }
        [XmlAttribute("weekday")]
        public Weekday VanaWeekDay { get; set; }
        [XmlAttribute("moon")]
        public MoonPhase MoonPhase { get; set; }
        [XmlAttribute("x")]
        public float X { get; set; }
        [XmlAttribute("y")]
        public float Y { get; set; }
        [XmlAttribute("z")]
        public float Z { get; set; }
        [XmlAttribute("h")]
        public float H { get; set; }
        [XmlAttribute("skill")]
        public int Skill { get; set; }
        [XmlAttribute("serpentrumors")]
        public HasKeyItemKind SerpentRumors { get; set; }
        [XmlAttribute("anglersalmanac")]
        public HasKeyItemKind AnglersAlmanac { get; set; }
        [XmlAttribute("frogfishing")]
        public HasKeyItemKind FrogFishing { get; set; }
        [XmlAttribute("mooching")]
        public HasKeyItemKind Mooching { get; set; }
        public FishHistoryDBFishModel()
        {
            this.FishName = string.Empty;
            this.ZoneName = string.Empty;
            this.RodName = string.Empty;
            this.BaitName = string.Empty;
            this.ID1 = 0;
            this.ID2 = 0;
            this.ID3 = 0;
            this.ID4 = 0;
            this.Critical = false;
            this.FishCount = 0;
            this.FishType = FishDBFishTypeKind.Unknown;
            this.Result = FishResultStatusKind.NoBite;
            this.EarthTime = DateTime.Today.ToString();
            this.VanaTime = string.Empty;
            this.VanaWeekDay = Weekday.Unknown;
            this.MoonPhase = FFACETools.MoonPhase.Unknown;
            this.X = 0.0f;
            this.Y = 0.0f;
            this.Z = 0.0f;
            this.H = 0.0f;
            this.Skill = -1;
            this.SerpentRumors = HasKeyItemKind.Unknown;
            this.AnglersAlmanac = HasKeyItemKind.Unknown;
            this.FrogFishing = HasKeyItemKind.Unknown;
            this.Mooching = HasKeyItemKind.Unknown;
        }
    }

    public class FishHistoryDBSummaryModel
    {
        public int Count { get; set; }
        public List<FishHistoryDBSummaryResultModel> Results { get; set; }
        public FishHistoryDBSummaryModel()
        {
            this.Count = 0;
            this.Results = new List<FishHistoryDBSummaryResultModel>();
            foreach (FishResultStatusKind v in Enum.GetValues(typeof(FishResultStatusKind)))
            {
                if (v != FishResultStatusKind.Unknown)
                {
                    FishHistoryDBSummaryResultModel result = new FishHistoryDBSummaryResultModel();
                    result.Result = v;
                    Results.Add(result);
                }
            }
        }
        public void Add(FishHistoryDBFishModel iFish)
        {
            this.Count += 1;
            foreach (FishHistoryDBSummaryResultModel v in this.Results)
            {
                if (v.Result == iFish.Result)
                {
                    v.Add(iFish);
                }
            }
            foreach (FishHistoryDBSummaryResultModel result in this.Results)
            {
                if (this.Count != 0) result.TotalPercent = (int)Math.Round(((double)result.Count / (double)this.Count) * 100d);
                else result.TotalPercent = 0;
                foreach (FishHistoryDBSummaryFishModel fish in result.Fishes)
                {
                    if (result.Count != 0) fish.Percent = (int)Math.Round(((double)fish.Count / (double)result.Count) * 100d);
                    else result.Percent = 0;
                    if (this.Count != 0) fish.TotalPercent = (int)Math.Round(((double)fish.Count / (double)this.Count) * 100d);
                    else result.TotalPercent = 0;
                }
            }
                        
        }
    }
    public class FishHistoryDBSummaryResultModel
    {
        public FishResultStatusKind Result { get; set; }
        public int Count { get; set; }
        public int Percent { get; set; }
        public int TotalPercent { get; set; }
        public List<FishHistoryDBSummaryFishModel> Fishes { get; set; }
        public FishHistoryDBSummaryResultModel()
        {
            this.Result = FishResultStatusKind.Unknown;
            this.Count = 0;
            this.Percent = 0;
            this.TotalPercent = 0;
            this.Fishes = new List<FishHistoryDBSummaryFishModel>();
        }
        public void Add(FishHistoryDBFishModel iFish)
        {
            this.Count += 1;

            bool foundFlg = false;
            foreach (FishHistoryDBSummaryFishModel fish in this.Fishes)
            {
                if (fish.FishName == iFish.FishName)
                {
                    foundFlg = true;
                    fish.Add(iFish);
                    break;
                }
            }
            if (!foundFlg)
            {
                FishHistoryDBSummaryFishModel fish = new FishHistoryDBSummaryFishModel(iFish);
                fish.FishName = iFish.FishName;
                fish.FishType = iFish.FishType;
                this.Fishes.Add(fish);
            }
            this.Fishes.Sort(FishHistoryDBSummaryFishModel.SortTypeName);
        }
    }
    public class FishHistoryDBHarakiriModel
    {
        [XmlAttribute("earthtime")]
        public string EarthTime { get; set; }
        [XmlAttribute("vanatime")]
        public string VanaTime { get; set; }
        [XmlAttribute("fishname")]
        public string FishName { get; set; }
        [XmlAttribute("itemname")]
        public string ItemName { get; set; }
        public FishHistoryDBHarakiriModel()
        {
            this.EarthTime = string.Empty;
            this.VanaTime = string.Empty;
            this.FishName = string.Empty;
            this.ItemName = string.Empty;
        }
        public FishHistoryDBHarakiriModel(string iEarthDate, string iVanaDate, string iFishName, string iItemName)
        {
            this.EarthTime = iEarthDate;
            this.VanaTime = iVanaDate;
            this.FishName = iFishName;
            this.ItemName = iItemName;
        }
    }
    public class FishHistoryDBSummaryFishModel
    {
        public string FishName { get; set; }
        public FishDBFishTypeKind FishType { get; set; }
        public int Count { get; set; }
        public int Percent { get; set; }
        public int TotalPercent { get; set; }
        public FishHistoryDBSummaryFishModel(FishHistoryDBFishModel iFish)
        {
            this.FishName = iFish.FishName;
            this.FishType = iFish.FishType;
            this.Count = 1;
            this.Percent = 0;
            this.TotalPercent = 0;
        }
        public void Add(FishHistoryDBFishModel iFish)
        {
            this.Count += 1;
        }
        public static int SortTypeName(FishHistoryDBSummaryFishModel iFish1, FishHistoryDBSummaryFishModel iFish2)
        {
            //1番目のキー：FishTypeでソート
            if (iFish1.FishType > iFish2.FishType)
            {
                return 1;
            }
            else if (iFish1.FishType < iFish2.FishType)
            {
                return -1;
            }
            else
            {
                //2番目のキー：FishNameでソート
                return string.Compare(iFish1.FishName, iFish2.FishName);
            }
        }

    }
}
