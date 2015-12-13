using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EnjoyFishing
{
    public enum FishDBFishTypeKind
    {
        SmallFish,
        LargeFish,
        Item,
        Monster,
        UnknownSmallFish,
        UnknownLargeFish,
        UnknownItem,
        UnknownMonster,
        Unknown,
    }
    public enum FishDBItemTypeKind
    {
        Common,
        Temporary,
        Key,
        Unknown,
    }
    public enum GearDBPositionKind
    {
        Unknown,
        Main,
        Sub,
        Head,
        Body,
        Hands,
        Legs,
        Feet,
        Neck,
        Waist,
        Back,
        Rings,
        Earrings,
        Foods,
    }

    #region FishDBModel
    [XmlRoot("Rod")]
    public class FishDBModel
    {
        [XmlAttribute("version")]
        public string Version { get; set; }
        [XmlAttribute("name")]
        public string RodName { get; set; }
        [XmlArray("Fishes")]
        [XmlArrayItem("Fish")]
        public List<FishDBFishModel> Fishes { get; set; }
        public FishDBModel() : this(string.Empty)
        {
        }
        public FishDBModel(string iRodName)
        {
            this.Version = string.Empty;
            this.RodName = iRodName;
            this.Fishes = new List<FishDBFishModel>();
        }
    }
    public class FishDBFishModel
    {
        [XmlAttribute("name")]
        public string FishName { get; set; }
        [XmlAttribute("fishtype")]
        public FishDBFishTypeKind FishType { get; set; }
        [XmlArray("Ids")]
        [XmlArrayItem("Id")]
        public List<FishDBIdModel> IDs { get; set; }
        [XmlArray("Zones")]
        [XmlArrayItem("Zone")]
        public List<string> ZoneNames { get; set; }
        [XmlArray("Baits")]
        [XmlArrayItem("Bait")]
        public List<string> BaitNames { get; set; }
        public FishDBFishModel()
        {
            this.FishName = string.Empty;
            this.FishType = FishDBFishTypeKind.Unknown;
            this.IDs = new List<FishDBIdModel>();
            this.ZoneNames = new List<string>();
            this.BaitNames = new List<string>();
        }
       public FishDBIdModel GetId(int iID1, int iID2, int iID3, int iID4)
        {
            foreach (FishDBIdModel id in this.IDs)
            {
                if (id.ID1 == iID1 && id.ID2 == iID2 && id.ID3 == iID3 && id.ID4 == iID4)
                {
                    return id;
                }
            }
            return new FishDBIdModel();
        }
        public static int SortTypeName(FishDBFishModel iFish1, FishDBFishModel iFish2)
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
    public class FishDBIdModel : IEquatable<FishDBIdModel>
    {
        [XmlAttribute("id1")]
        public int ID1 { get; set; }
        [XmlAttribute("id2")]
        public int ID2 { get; set; }
        [XmlAttribute("id3")]
        public int ID3 { get; set; }
        [XmlAttribute("id4")]
        public int ID4 { get; set; }
        [XmlAttribute("count")]
        public int Count { get; set; }
        [XmlAttribute("critical")]
        public bool Critical { get; set; }
        [XmlAttribute("itemtype")]
        public FishDBItemTypeKind ItemType { get; set; }
        public FishDBIdModel()
        {
            this.ID1 = 0;
            this.ID2 = 0;
            this.ID3 = 0;
            this.ID4 = 0;
            this.Count = 0;
            this.Critical = false;
            this.ItemType = FishDBItemTypeKind.Unknown;
        }
        public FishDBIdModel(int iID1, int iID2, int iID3, int iID4, int iCount, bool iCritical, FishDBItemTypeKind iItemType)
        {
            this.ID1 = iID1;
            this.ID2 = iID2;
            this.ID3 = iID3;
            this.ID4 = iID4;
            this.Count = iCount;
            this.Critical = iCritical;
            this.ItemType = iItemType;
        }
        public FishDBIdModel(int iID1, int iID2, int iID3, int iID4)
        {
            this.ID1 = iID1;
            this.ID2 = iID2;
            this.ID3 = iID3;
            this.ID4 = iID4;
            this.Count = 0;
            this.Critical = false;
            this.ItemType = FishDBItemTypeKind.Unknown;
        }
        public override string ToString()
        {
            return string.Format("{0:000}-{1:000}-{2:000}-{3:000}", this.ID1, this.ID2, this.ID3, this.ID4);
        }
        public override int GetHashCode()
        {
            return this.ID1.GetHashCode() ^ this.ID2.GetHashCode() ^ this.ID3.GetHashCode() ^ this.ID4.GetHashCode();
        }
        bool IEquatable<FishDBIdModel>.Equals(FishDBIdModel other)
        {
            if (other == null) return false;
            return (this.ID1 == other.ID1 && this.ID2 == other.ID2 && this.ID3 == other.ID3 && this.ID4 == other.ID4);
        }
       public static int SortCountCritical(FishDBIdModel iID1, FishDBIdModel iID2)
        {
            //1番目のキー：ItemTypeでソート
            if (iID1.ItemType > iID1.ItemType)
            {
                return 1;
            }
            else if (iID1.ItemType < iID2.ItemType)
            {
                return -1;
            }
            else
            {
                //2番目のキー：Countでソート
                if (iID1.Count > iID2.Count)
                {
                    return 1;
                }
                else if (iID1.Count < iID2.Count)
                {
                    return -1;
                }
                else
                {
                    //3番目のキー：Criticalでソート
                    if (iID1.Critical && !iID2.Critical)
                    {
                        return 1;
                    }
                    else if (!iID1.Critical && iID2.Critical)
                    {
                        return -1;
                    }
                    else
                    {
                        //4番目のキー：ID1でソート
                        if (iID1.ID1 > iID2.ID1)
                        {
                            return 1;
                        }
                        else if (iID1.ID1 < iID2.ID1)
                        {
                            return -1;
                        }
                        else
                        {
                            //5番目のキー：ID2でソート
                            if (iID1.ID2 > iID2.ID2)
                            {
                                return 1;
                            }
                            else if (iID1.ID2 < iID2.ID2)
                            {
                                return -1;
                            }
                            else
                            {
                                //6番目のキー：ID3でソート
                                if (iID1.ID3 > iID2.ID3)
                                {
                                    return 1;
                                }
                                else if (iID1.ID3 < iID2.ID3)
                                {
                                    return -1;
                                }
                                else
                                {
                                    //7番目のキー：ID4でソート
                                    if (iID1.ID4 > iID2.ID4)
                                    {
                                        return 1;
                                    }
                                    else if (iID1.ID4 < iID2.ID4)
                                    {
                                        return -1;
                                    }
                                    else
                                    {
                                        return 0;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    #endregion

    #region RodModel
    [XmlRoot("Rods")]
    public class RodDBModel
    {
        [XmlElement("Rod")]
        public List<RodDBRodModel> Rod { get; set; }
        public RodDBModel()
        {
            this.Rod = new List<RodDBRodModel>();
        }
        public static int SortTypeName(RodDBRodModel iRod1, RodDBRodModel iRod2)
        {
            //1番目のキー：RodNameでソート
            return string.Compare(iRod1.RodName, iRod2.RodName);
        }
    }
    public class RodDBRodModel
    {
        [XmlAttribute("name")]
        public string RodName { get; set; }
        [XmlAttribute("break")]
        public string BreakRodName { get; set; }
        [XmlAttribute("crystal")]
        public string RepairCrystal { get; set; }
        public RodDBRodModel()
        {
            this.RodName = string.Empty;
            this.BreakRodName = string.Empty;
            this.RepairCrystal = string.Empty;
        }
    }
    #endregion

    #region BaitModel
    [XmlRoot("Baits")]
    public class BaitDBModel
    {
        [XmlElement("Bait")]
        public List<BaitDBBaitModel> Bait { get; set; }
        public BaitDBModel()
        {
            this.Bait = new List<BaitDBBaitModel>();
        }
        public static int SortTypeName(BaitDBBaitModel iBait1, BaitDBBaitModel iBait2)
        {
            //1番目のキー：BaitNameでソート
            return string.Compare(iBait1.BaitName, iBait2.BaitName);
        }
    }
    public class BaitDBBaitModel
    {
        [XmlAttribute("name")]
        public string BaitName { get; set; }
    }
    #endregion

    #region GearModel
    [XmlRoot("Gears")]
    public class GearDBModel
    {
        [XmlElement("Gear")]
        public List<GearDBGearModel> Gear { get; set; }
        public GearDBModel()
        {
            this.Gear = new List<GearDBGearModel>();
        }
        public static int SortTypeName(GearDBGearModel iGear1, GearDBGearModel iGear2)
        {
            //1番目のキー：GearNameでソート
            return string.Compare(iGear1.GearName, iGear2.GearName);
        }
    }
    public class GearDBGearModel
    {
        [XmlAttribute("name")]
        public string GearName { get; set; }
        [XmlAttribute("position")]
        public GearDBPositionKind Position { get; set; }
        [XmlAttribute("buff")]
        public int BuffID { get; set; }
        public GearDBGearModel()
        {
            this.GearName = string.Empty;
            this.Position = GearDBPositionKind.Unknown;
            this.BuffID = -1;
        }
        public GearDBGearModel(string iGearName, GearDBPositionKind iPosition, int iGearBuff)
        {
            this.GearName = iGearName;
            this.Position = iPosition;
            this.BuffID = iGearBuff;
        }
    }
    #endregion

    #region EminenceModel
    [XmlRoot("Eminences")]
    public class EminenceDBModel
    {
        [XmlElement("Eminence")]
        public List<EminenceDBEminenceModel> Eminences { get; set; }
    }
    public class EminenceDBEminenceModel : IEquatable<EminenceDBEminenceModel>
    {
        [XmlAttribute("name")]
        public string EminenceName { get; set; }
        [XmlAttribute("stop")]
        public bool Stop { get; set; }
        public EminenceDBEminenceModel()
            : this(string.Empty, false)
        {
        }
        public EminenceDBEminenceModel(string iName, bool iStop)
        {
            this.EminenceName = iName;
            this.Stop = iStop;
        }
        public override string ToString()
        {
            return this.EminenceName;
        }
        bool IEquatable<EminenceDBEminenceModel>.Equals(EminenceDBEminenceModel other)
        {
            if (other == null) return false;
            return (this.EminenceName == other.EminenceName && this.Stop == other.Stop);
        }
    }
    #endregion
}
