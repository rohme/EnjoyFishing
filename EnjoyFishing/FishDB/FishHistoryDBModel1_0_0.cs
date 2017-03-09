using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using EliteMMO.API;

namespace EnjoyFishing
{
    [XmlRoot("History")]
    public class FishHistoryDBModel1_0_0
    {
        [XmlAttribute("player")]
        public string PlayerName { get; set; }
        [XmlAttribute("date")]
        public DateTime EarthDate { get; set; }
        public int CatchCount { get; set; }
        [XmlArray("Fishes")]
        [XmlArrayItem("Fish")]
        public List<FishHistoryDBFishModel1_0_0> Fishes { get; set; }
        public FishHistoryDBModel1_0_0()
        {
            this.PlayerName = string.Empty;
            this.EarthDate = DateTime.Today;
            this.CatchCount = 0;
            this.Fishes = new List<FishHistoryDBFishModel1_0_0>();

        }
    }
    public class FishHistoryDBFishModel1_0_0
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
        [XmlAttribute("type")]
        public FishDBFishTypeKind FishType { get; set; }
        [XmlAttribute("result")]
        public FishResultStatusKind Result { get; set; }
        [XmlAttribute("earthtime")]
        public DateTime EarthTime { get; set; }
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
        public FishHistoryDBFishModel1_0_0()
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
            this.EarthTime = DateTime.Now;
            this.VanaTime = string.Empty;
            this.VanaWeekDay = Weekday.Unknown;
            this.MoonPhase = MoonPhase.Unknown;
            this.X = 0.0f;
            this.Y = 0.0f;
            this.Z = 0.0f;
            this.H = 0.0f;
        }
    }

}
