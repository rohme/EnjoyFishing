using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace EnjoyFishing
{
    /// <summary>
    /// ハラキリ結果ステータス
    /// </summary>
    public enum HarakiriResultStatusKind
    {
        Found,
        NotFound,
        Unknown,
    }
    
    [XmlRoot("Harakiri")]
    public class HarakiriDBModel
    {
        [XmlArray("Items")]
        [XmlArrayItem("Item")]
        public List<HarakiriDBHistoryModel> History { get; set; }
        public HarakiriDBModel()
        {
            this.History = new List<HarakiriDBHistoryModel>();
        }
    }
    public class HarakiriDBHistoryModel
    {
        [XmlAttribute("fishname")]
        public string FishName { get; set; }
        [XmlAttribute("itemname")]
        public string ItemName { get; set; }
        [XmlAttribute("result")]
        public HarakiriResultStatusKind Result { get; set; }
        public HarakiriDBHistoryModel()
        {
            this.FishName = string.Empty;
            this.ItemName = string.Empty;
            this.Result = HarakiriResultStatusKind.Unknown;
        }
    }
}
