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
    public class ResourceTool
    {
        private const int DEFAULT_LANGUAGE = 1;

        private EliteAPI api;

        public ResourceTool(EliteAPI iAPI)
        {
            api = iAPI;
            string windowerPath = MiscTools.EliteAPIControl.GetWindowerPath();
            if (windowerPath.Length > 0 && Directory.Exists(windowerPath))
            {
                this.WindowerPath = windowerPath;
                LoadItemNames();
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

            foreach (var v in xmls)
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
    }
}
