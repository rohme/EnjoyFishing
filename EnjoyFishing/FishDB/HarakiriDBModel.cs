using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace EnjoyFishing
{
    [XmlRoot("Harakiri")]
    public class HarakiriDBModel
    {
        [XmlAttribute("version")]
        public string Version { get; set; }
        [XmlArray("Fishes")]
        [XmlArrayItem("Fish")]
        public List<HarakiriDBFishModel> Fishes { get; set; }
        public HarakiriDBModel()
        {
            this.Version = string.Empty;
            this.Fishes = new List<HarakiriDBFishModel>();
        }
    }
    public class HarakiriDBFishModel : IEquatable<HarakiriDBFishModel>
    {
        [XmlAttribute("fishname")]
        public string FishName { get; set; }
        [XmlAttribute("count")]
        public int Count { get; set; }
        [XmlArray("Items")]
        [XmlArrayItem("Item")]
        public List<HarakiriDBItemModel> Items { get; set; }
        public HarakiriDBFishModel()
        {
            this.FishName = string.Empty;
            this.Count = 0;
            this.Items = new List<HarakiriDBItemModel>();
        }
        public HarakiriDBFishModel(string iFishName)
        {
            this.FishName = iFishName;
        }
        public override int GetHashCode()
        {
            return this.FishName.GetHashCode();
        }
        bool IEquatable<HarakiriDBFishModel>.Equals(HarakiriDBFishModel other)
        {
            if (other == null) return false;
            return (this.FishName == other.FishName);
        }
    }
    public class HarakiriDBItemModel : IEquatable<HarakiriDBItemModel>
    {
        [XmlAttribute("itemname")]
        public string ItemName { get; set; }
        [XmlAttribute("count")]
        public int Count { get; set; }
        public HarakiriDBItemModel()
        {
            this.ItemName = string.Empty;
            this.Count = 0;
        }
        public HarakiriDBItemModel(string iItemName)
        {
            this.ItemName = iItemName;
        }
        public override int GetHashCode()
        {
            return this.ItemName.GetHashCode();
        }
        bool IEquatable<HarakiriDBItemModel>.Equals(HarakiriDBItemModel other)
        {
            if (other == null) return false;
            return (this.ItemName == other.ItemName);
        }
    }
}
