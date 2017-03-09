using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using EliteMMO.API;

namespace EnjoyFishing
{
    [XmlRoot("History")]
    public class FishHistoryDBModel1_0_5
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
        public List<FishHistoryDBFishModel1_0_5> Fishes { get; set; }
        [XmlArray("Harakiri")]
        [XmlArrayItem("Fish")]
        public List<FishHistoryDBHarakiriModel1_0_5> Harakiri { get; set; }
        public FishHistoryDBModel1_0_5()
        {
            this.Version = string.Empty;
            this.PlayerName = string.Empty;
            this.EarthDate = DateTime.Today.ToString();
            this.TimeElapsed = 0;
            this.Fishes = new List<FishHistoryDBFishModel1_0_5>();
            this.Harakiri = new List<FishHistoryDBHarakiriModel1_0_5>();
        }
    }

    public class FishHistoryDBFishModel1_0_5
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
        public FishHistoryDBFishModel1_0_5()
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
            this.MoonPhase = MoonPhase.Unknown;
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

    public class FishHistoryDBHarakiriModel1_0_5
    {
        [XmlAttribute("earthtime")]
        public string EarthTime { get; set; }
        [XmlAttribute("vanatime")]
        public string VanaTime { get; set; }
        [XmlAttribute("fishname")]
        public string FishName { get; set; }
        [XmlAttribute("itemname")]
        public string ItemName { get; set; }
        public FishHistoryDBHarakiriModel1_0_5()
        {
            this.EarthTime = string.Empty;
            this.VanaTime = string.Empty;
            this.FishName = string.Empty;
            this.ItemName = string.Empty;
        }
        public FishHistoryDBHarakiriModel1_0_5(string iEarthDate, string iVanaDate, string iFishName, string iItemName)
        {
            this.EarthTime = iEarthDate;
            this.VanaTime = iVanaDate;
            this.FishName = iFishName;
            this.ItemName = iItemName;
        }
    }
}
