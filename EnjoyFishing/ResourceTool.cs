using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using EliteMMO.API;
using NLog;

namespace EnjoyFishing
{
    #region enums
    [Flags]
    public enum EquipSlotFlags : ushort
    {
        // Specific Slots
        None = 0x0000,
        Main = 0x0001,
        Sub = 0x0002,
        Range = 0x0004,
        Ammo = 0x0008,
        Head = 0x0010,
        Body = 0x0020,
        Hands = 0x0040,
        Legs = 0x0080,
        Feet = 0x0100,
        Neck = 0x0200,
        Waist = 0x0400,
        LEar = 0x0800,
        REar = 0x1000,
        LRing = 0x2000,
        RRing = 0x4000,
        Back = 0x8000,
        // Slot Groups
        Ears = 0x1800,
        Rings = 0x6000,
        All = 0xFFFF,
    }

    public enum ItemCategoryType
    {
        None,
        Armor,
        Automaton,
        General,
        Gil,
        Maze,
        Usable,
        Weapon,
    }

    [Flags]
    public enum ItemFlags : ushort
    {
        None = 0x0000,
        // Simple Flags - mostly assumed meanings
        Flag00 = 0x0001,
        Flag01 = 0x0002,
        Flag02 = 0x0004,
        Flag03 = 0x0008,
        CanSendPOL = 0x0010,
        Inscribable = 0x0020,
        NoAuction = 0x0040,
        Scroll = 0x0080,
        Linkshell = 0x0100,
        CanUse = 0x0200,
        CanTradeNPC = 0x0400,
        CanEquip = 0x0800,
        NoSale = 0x1000,
        NoDelivery = 0x2000,
        NoTradePC = 0x4000,
        Rare = 0x8000,
        // Combined Flags
        Ex = 0x6040, // NoAuction + NoDelivery + NoTrade
    }

    public enum ItemType : ushort
    {
        Nothing = 0x0000,
        Item = 0x0001,
        QuestItem = 0x0002,
        Fish = 0x0003,
        Weapon = 0x0004,
        Armor = 0x0005,
        Linkshell = 0x0006,
        UsableItem = 0x0007,
        Crystal = 0x0008,
        Currency = 0x0009,
        Furnishing = 0x000A,
        Plant = 0x000B,
        Flowerpot = 0x000C,
        PuppetItem = 0x000D,
        Mannequin = 0x000E,
        Book = 0x000F,
        RacingForm = 0x0010,
        BettingSlip = 0x0011,
        SoulPlate = 0x0012,
        Reflector = 0x0013,
        ItemType20 = 0x0014,
        LotteryTicket = 0x0015,
        MazeTabula_M = 0x0016,
        MazeTabula_R = 0x0017,
        MazeVoucher = 0x0018,
        MazeRune = 0x0019,
        ItemType_26 = 0x001A,
        StorageSlip = 0x001B,
    }

    [Flags]
    public enum JobFlags : uint
    {
        None = 0x00000000,
        All = 0x007FFFFE, // Masks valid jobs
        // Specific
        WAR = 0x00000002,
        MNK = 0x00000004,
        WHM = 0x00000008,
        BLM = 0x00000010,
        RDM = 0x00000020,
        THF = 0x00000040,
        PLD = 0x00000080,
        DRK = 0x00000100,
        BST = 0x00000200,
        BRD = 0x00000400,
        RNG = 0x00000800,
        SAM = 0x00001000,
        NIN = 0x00002000,
        DRG = 0x00004000,
        SMN = 0x00008000,
        BLU = 0x00010000,
        COR = 0x00020000,
        PUP = 0x00040000,
        DNC = 0x00080000,
        SCH = 0x00100000,
        GEO = 0x00200000,
        RUN = 0x00400000,
        MON = 0x00800000,
        JOB24 = 0x01000000,
        JOB25 = 0x02000000,
        JOB26 = 0x04000000,
        JOB27 = 0x08000000,
        JOB28 = 0x10000000,
        JOB29 = 0x20000000,
        JOB30 = 0x40000000,
        JOB31 = 0x80000000,
    }

    [Flags]
    public enum RaceFlags : ushort
    {
        None = 0x0000,
        All = 0x01FE,
        // Specific
        HumeMale = 0x0002,
        HumeFemale = 0x0004,
        ElvaanMale = 0x0008,
        ElvaanFemale = 0x0010,
        TarutaruMale = 0x0020,
        TarutaruFemale = 0x0040,
        Mithra = 0x0080,
        Galka = 0x0100,
        // Race Groups
        Hume = 0x0006,
        Elvaan = 0x0018,
        Tarutaru = 0x0060,
        // Gender Groups (with Mithra = female, and Galka = male)
        Male = 0x012A,
        Female = 0x00D4,
    }

    [Flags]
    public enum ValidTargetFlags : ushort
    {
        None = 0x00,
        Self = 0x01,
        Player = 0x02,
        PartyMember = 0x04,
        Ally = 0x08,
        NPC = 0x10,
        Enemy = 0x20,
        Unknown = 0x40,
        Object = 0x60,
        CorpseOnly = 0x80,
        Corpse = 0x9D // CorpseOnly + NPC + Ally + Partymember + Self
    }
    #endregion

    public class ResourceTool
    {
        private const string PATH_DB = "data";
        private const string FILE_RECIPES = "recipes.xml";
        private const string PATH_WINDOWER_RESOURCES = "res";
        private const string FILE_WINDOWER_RESOURCE_ITEMS = "items.lua";
        private const string FILE_WINDOWER_RESOURCE_ITEM_DESCRIPTIONS = "item_descriptions.lua";
        private const int DEFAULT_LANGUAGE = 1;

        private EliteAPI api;

        public ResourceTool(EliteAPI iAPI)
        {
            api = iAPI;
            string windowerPath = MiscTools.EliteAPIControl.GetWindowerPath();
            if (windowerPath.Length>0 && Directory.Exists(windowerPath))
            {
                this.WindowerPath = windowerPath;
                LoadItemNames();
                //LoadItems();
            }
        }

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string WindowerPath { get; set; }
        private SortedDictionary<uint, string> ItemNames = new SortedDictionary<uint, string>();

        public EliteAPI.IItem GetItem(string iName)
        {
            if (!this.ItemNames.ContainsValue(iName)) return new EliteAPI.IItem() { Name = new string[3] };
            return GetItem(this.ItemNames.First(x => x.Value == iName).Key);
        }
        public EliteAPI.IItem GetItem(uint iID)
        {
            if (!this.ItemNames.ContainsKey(iID) || iID == 65535) return new EliteAPI.IItem() { Name = new string[3] };
            var ret = api.Resources.GetItem(iID);
            ret.Name[1] = this.ItemNames[iID];
            return ret;
        }
        public string GetZoneName(int iID, int iLanguageID = DEFAULT_LANGUAGE)
        {
            return api.Resources.GetString("areas", (uint)iID, iLanguageID);
        }
        public string GetWeatherName(int iID, int iLanguageID = DEFAULT_LANGUAGE)
        {
            return api.Resources.GetString("weather", (uint)iID, iLanguageID);
        }
        public string GetDayName(int iID, int iLanguageID = DEFAULT_LANGUAGE)
        {
            return api.Resources.GetString("days", (uint)iID, iLanguageID);
        }
        public string GetMoonPhaseName(int iID, int iLanguageID = DEFAULT_LANGUAGE)
        {
            return api.Resources.GetString("moonphases", (uint)iID, iLanguageID);
        }
        public string GetKeyItemName(int iID, int iLanguageID = DEFAULT_LANGUAGE)
        {
            return api.Resources.GetString("keyitems", (uint)iID, iLanguageID);
        }
        public string GetStatusName(int iID, int iLanguageID = DEFAULT_LANGUAGE)
        {
            return api.Resources.GetString("statusnames", (uint)iID, iLanguageID);
        }

        private void LoadItemNames()
        {
            XmlDocument xmlDoc = new XmlDocument();
            string[] xmls = new string[] { "items_armor.xml", "items_general.xml", "items_weapons.xml" };

            foreach(var v in xmls)
            {
                string path = Path.Combine(WindowerPath, "plugins", "resources", v);
                xmlDoc.Load(path);
                XmlNodeList itemsList = xmlDoc.SelectNodes("/items/i");
                foreach (XmlNode i in itemsList)
                {
                    uint id = uint.Parse(i.Attributes["id"].Value);
                    string name = i.Attributes["jp"].Value;
                    if (!ItemNames.ContainsKey(id))
                        ItemNames.Add(id, name);
                }
            }
        }

    //    private void LoadItems()
    //    {
    //        this.Items = new Dictionary<uint, ItemModel>();
    //        string filename = Path.Combine(this.WindowerPath, PATH_WINDOWER_RESOURCES, FILE_WINDOWER_RESOURCE_ITEMS);
    //        logger.Debug("{0}の読み込み", filename);
    //        if (!File.Exists(filename)) throw new FileNotFoundException(string.Format("アイテムファイル({0})が見つかりません", filename));

    //        Lua lua = new Lua();
    //        LuaTable luaTable = (LuaTable)lua.DoFile(filename)[0];
    //        foreach (KeyValuePair<object, object> pair in luaTable)
    //        {
    //            try
    //            {
    //                LuaTable v = (LuaTable)pair.Value;
    //                ItemModel item = new ItemModel();
    //                item.ID = (v["id"] != null) ? uint.Parse(v["id"].ToString()) : (uint)0;
    //                item.Name = (v["ja"] != null) ? v["ja"].ToString() : string.Empty;
    //                item.Category = (v["category"] != null) ? (ItemCategoryType)Enum.Parse(typeof(ItemCategoryType), v["category"].ToString()) : ItemCategoryType.None;
    //                item.Flags = (v["flags"] != null) ? (ItemFlags)(double)v["flags"] : ItemFlags.None;
    //                item.Stack = (v["stack"] != null) ? (ushort)(double)v["stack"] : (ushort)0;
    //                item.Targets = (v["targets"] != null) ? (ValidTargetFlags)(double)v["targets"] : ValidTargetFlags.None;
    //                item.Type = (v["type"] != null) ? (ItemType)(double)v["type"] : ItemType.Nothing;
    //                item.CastTime = (v["cast_time"] != null) ? (uint)(double)v["cast_time"] : (uint)0;
    //                item.Jobs = (v["jobs"] != null) ? (JobFlags)(double)v["jobs"] : JobFlags.None;
    //                item.Level = (v["level"] != null) ? (ushort)(double)v["level"] : (ushort)0;
    //                item.Races = (v["races"] != null) ? (RaceFlags)(double)v["races"] : RaceFlags.None;
    //                item.EquipSlot = (v["slots"] != null) ? (EquipSlotFlags)(double)v["slots"] : EquipSlotFlags.None;
    //                item.CastDelay = (v["cast_delay"] != null) ? (uint)(double)v["cast_delay"] : (uint)0;
    //                item.MaxCharge = (v["max_charges"] != null) ? (ushort)(double)v["max_charges"] : (ushort)0;
    //                item.RecastDelay = (v["recast_delay"] != null) ? (uint)(double)v["recast_delay"] : (uint)0;
    //                item.ShieldSize = (v["shield_size"] != null) ? (ushort)(double)v["shield_size"] : (ushort)0;
    //                item.Damage = (v["damage"] != null) ? (uint)(double)v["damage"] : (uint)0;
    //                item.Delay = (v["delay"] != null) ? (uint)(double)v["delay"] : (uint)0;
    //                item.Skill = (v["skill"] != null) ? (ushort)(double)v["skill"] : (ushort)0;
    //                item.ItemLevel = (v["item_level"] != null) ? (ushort)(double)v["item_level"] : (ushort)0;
    //                item.SuperiorLevel = (v["superior_level"] != null) ? (ushort)(double)v["superior_level"] : (ushort)0;
    //                this.Items.Add(item.ID, item);
    //            }
    //            catch (Exception e)
    //            {
    //                logger.Error("{0}の読み込みエラー", filename);
    //                throw e;
    //            }
    //        }
    //        // 詳細
    //        filename = Path.Combine(this.WindowerPath, PATH_WINDOWER_RESOURCES, FILE_WINDOWER_RESOURCE_ITEM_DESCRIPTIONS);
    //        logger.Debug("{0}の読み込み", filename);
    //        if (!File.Exists(filename)) throw new FileNotFoundException(string.Format("アイテム詳細ファイル({0})が見つかりません", filename));

    //        lua = new Lua();
    //        luaTable = (LuaTable)lua.DoFile(filename)[0];
    //        foreach (KeyValuePair<object, object> pair in luaTable)
    //        {
    //            try
    //            {
    //                LuaTable v = (LuaTable)pair.Value;
    //                uint id = (v["id"] != null) ? uint.Parse(v["id"].ToString()) : (uint)0;
    //                if (this.Items.ContainsKey(id))
    //                {
    //                    this.Items[id].Description = (v["ja"] != null) ? v["ja"].ToString() : string.Empty;
    //                }
    //            }
    //            catch (Exception e)
    //            {
    //                logger.Error("{0}の読み込みエラー", filename);
    //                throw e;
    //            }
    //        }
    //    }
    }

    //public class ItemModel
    //{
    //    public uint ID { get; set; }
    //    public string Name { get; set; }
    //    public ItemCategoryType Category { get; set; }
    //    public ItemFlags Flags { get; set; }
    //    public ushort Stack { get; set; }
    //    public ValidTargetFlags Targets { get; set; }
    //    public ItemType Type { get; set; }
    //    public uint CastTime { get; set; }
    //    public JobFlags Jobs { get; set; }
    //    public ushort Level { get; set; }
    //    public RaceFlags Races { get; set; }
    //    public EquipSlotFlags EquipSlot { get; set; }
    //    public uint CastDelay { get; set; }
    //    public ushort MaxCharge { get; set; }
    //    public uint RecastDelay { get; set; }
    //    public ushort ShieldSize { get; set; }
    //    public uint Damage { get; set; }
    //    public uint Delay { get; set; }
    //    public ushort Skill { get; set; }
    //    public ushort ItemLevel { get; set; }
    //    public ushort SuperiorLevel { get; set; }
    //    public string Description { get; set; }
    //}
}
