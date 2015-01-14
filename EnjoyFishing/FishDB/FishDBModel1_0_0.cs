using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace EnjoyFishing
{
    [XmlRoot("Rod")]
    public class FishDBModel1_0_0
    {
        [XmlAttribute("name")]
        public string RodName { get; set; }
        [XmlElement("Fish")]
        public List<FishDBFishModel1_0_0> Fishes { get; set; }
        public FishDBModel1_0_0()
            : this(string.Empty)
        {
        }
        public FishDBModel1_0_0(string iRodName)
        {
            this.RodName = iRodName;
            this.Fishes = new List<FishDBFishModel1_0_0>();
        }
    }
    public class FishDBFishModel1_0_0
    {
        [XmlAttribute("name")]
        public string FishName { get; set; }
        [XmlAttribute("type")]
        public FishDBFishTypeKind FishType { get; set; }
        [XmlArray("Ids")]
        [XmlArrayItem("Id")]
        public List<FishDBIdModel1_0_0> IDs { get; set; }
        [XmlArray("Zones")]
        [XmlArrayItem("Zone")]
        public List<string> ZoneNames { get; set; }
        [XmlArray("Baits")]
        [XmlArrayItem("Bait")]
        public List<string> BaitNames { get; set; }
        public FishDBFishModel1_0_0()
        {
            this.FishName = string.Empty;
            this.FishType = FishDBFishTypeKind.Unknown;
            this.IDs = new List<FishDBIdModel1_0_0>();
            this.ZoneNames = new List<string>();
            this.BaitNames = new List<string>();
        }
        public FishDBIdModel1_0_0 GetId(int iID1, int iID2, int iID3, int iID4)
        {
            foreach (FishDBIdModel1_0_0 id in this.IDs)
            {
                if (id.ID1 == iID1 && id.ID2 == iID2 && id.ID3 == iID3 && id.ID4 == iID4)
                {
                    return id;
                }
            }
            return new FishDBIdModel1_0_0();
        }
        public static int SortTypeName(FishDBFishModel1_0_0 iFish1, FishDBFishModel iFish2)
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
    public class FishDBIdModel1_0_0
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
        public FishDBIdModel1_0_0()
        {
            this.ID1 = 0;
            this.ID2 = 0;
            this.ID3 = 0;
            this.ID4 = 0;
            this.Count = 0;
            this.Critical = false;
        }
        public static int SortCountID(FishDBIdModel1_0_0 iID1, FishDBIdModel1_0_0 iID2)
        {
            //1番目のキー：Countでソート
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
                //2番目のキー：Criticalでソート
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
                    //3番目のキー：ID1でソート
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
                        //4番目のキー：ID2でソート
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
                            //5番目のキー：ID3でソート
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
                                //6番目のキー：ID4でソート
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
