using FFACETools;
using MiscTools;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

namespace EnjoyFishing
{
    public class UpdateDBTool
    {
        private const string DUMMY_PLAYER_NAME = "DUMMY";
        private const string PATH_TEMP = "Temp";
        private const string URL_API_CHECK_VERSION = "/api/enjoyfishing/checkversion";
        private const string URL_API_ENABLE_NAME = "/api/enjoyfishing/enablename";
        private const string URL_API_STATUS = "/api/enjoyfishing/status";
        private const string URL_API_ROD =  "/api/enjoyfishing/rod";
        private const string URL_API_UPLOAD_HISTORY = "/api/enjoyfishing/uploadhistory";

        private Settings settings;
        private LoggerTool logger;
        private FishDB fishDB;
        private FishHistoryDB historyDB;
        private string serverName = string.Empty;
 
        #region メンバ
        #endregion

        #region イベント
        #region ReceiveMessage
        /// <summary>
        /// ReceiveMessageイベントで返されるデータ
        /// </summary>
        public class ReceiveMessageEventArgs : EventArgs
        {
            public string Message;
            public Color Color;
            public bool Bold;
         }
        public delegate void ReceiveMessageEventHandler(object sender, ReceiveMessageEventArgs e);
        public event ReceiveMessageEventHandler ReceiveMessage;
        protected virtual void OnReceiveMessage(ReceiveMessageEventArgs e)
        {
            if (ReceiveMessage != null)
            {
                ReceiveMessage(this, e);
            }
        }
        private void EventReceiveMessage(string iMessage, uint iArgb = 0xFFBBBBBB, bool iBold = false)
        {
            //返すデータの設定
            ReceiveMessageEventArgs e = new ReceiveMessageEventArgs();
            e.Message = iMessage;
            e.Color = Color.FromArgb((int)iArgb);
            e.Bold = iBold;
            //イベントの発生
            OnReceiveMessage(e);
        }
        #endregion
        #region NewerVersion
        /// <summary>
        /// NewerVersionイベントで返されるデータ
        /// </summary>
        public class NewerVersionEventArgs : EventArgs
        {
            public string Message;
            public string Url;
        }
        public delegate void NewerVersionEventHandler(object sender, NewerVersionEventArgs e);
        public event NewerVersionEventHandler NewerVersion;
        protected virtual void OnNewerVersion(NewerVersionEventArgs e)
        {
            if (NewerVersion != null)
            {
                NewerVersion(this, e);
            }
        }
        private void EventNewerVersion(string iMessage, string iUrl)
        {
            //返すデータの設定
            NewerVersionEventArgs e = new NewerVersionEventArgs();
            e.Message = iMessage;
            e.Url = iUrl;
            //イベントの発生
            OnNewerVersion(e);
        }
        #endregion
        #endregion

        #region コンストラクタ
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="iSettings"></param>
        /// <param name="iLogger"></param>
        public UpdateDBTool(Settings iSettings, LoggerTool iLogger)
        {
            settings = iSettings;
            logger = iLogger;
            fishDB = new FishDB(logger);
            historyDB = new FishHistoryDB(DUMMY_PLAYER_NAME, DateTime.Today, logger);

            serverName = "http://" + settings.Global.UpdateDB.ServerName;
        }
        #endregion

        #region DB更新
        /// <summary>
        /// データベース更新処理
        /// </summary>
        public bool UpdateDB()
        {
            logger.Output(LogLevelKind.INFO, "DB更新開始");
            string response = string.Empty;
            bool httpRet = false;

            //アプリケーションバージョンチェック
            EventReceiveMessage("== クライアントのバージョンチェック ==", 0xFFFFFFFF, true);
            NameValueCollection postCheckVersion = new NameValueCollection();
            postCheckVersion.Add("version", MiscTool.GetAppVersion());
            string responseCheckVersion = string.Empty;
            bool retCheckVersion = HttpPost(serverName + URL_API_CHECK_VERSION, postCheckVersion, out responseCheckVersion);
            if (!retCheckVersion)
            {
                EventReceiveMessage(responseCheckVersion, 0xFFFF0000);
                return false;
            }
            XmlSerializer seriApiCheckVersion = new XmlSerializer(typeof(UpdateDBApiCheckVersionModel));
            MemoryStream msApiCheckVersion = new MemoryStream(Encoding.UTF8.GetBytes(responseCheckVersion));
            var resCheckVersion = (UpdateDBApiCheckVersionModel)seriApiCheckVersion.Deserialize(msApiCheckVersion);
            if (resCheckVersion.Result.Success == "true")
            {
                //有効バージョンか？
                if (resCheckVersion.VersionEnable == "true")
                {
                    //新しいバージョンがリリースされているか？
                    if (resCheckVersion.NewVersionExists != "true")
                    {
                        //最新バージョンを使用
                        EventReceiveMessage(resCheckVersion.Message);
                    }
                    else
                    {
                        //新しいバージョンがある
                        EventReceiveMessage(resCheckVersion.Message, 0xFFFF0000);
                        //イベント発生
                        EventNewerVersion(resCheckVersion.Message, resCheckVersion.NewVersionUrl);
                    }
                }
                else
                {
                    EventReceiveMessage(resCheckVersion.Message, 0xFFFF0000);
                    return false;
                }
            }
            else
            {
                EventReceiveMessage(resCheckVersion.Result.Message, 0xFFFF0000);
                return false;
            }
            //有効名称データの取得
            EventReceiveMessage("== チェックデータを取得  ==", 0xFFFFFFFF, true);
            //有効名称データの受信
            response = string.Empty;
            httpRet = Http(serverName + URL_API_ENABLE_NAME, out response);
            UpdateDBApiEnableNameModel enablename = new UpdateDBApiEnableNameModel();
            if (httpRet)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(UpdateDBApiEnableNameModel));
                MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(response));
                enablename = (UpdateDBApiEnableNameModel)serializer.Deserialize(ms);
                if (enablename.Result.Success != "true")
                {
                    //イベント発生
                    EventReceiveMessage(string.Format("{0}", enablename.Result.Message), 0xFFFF0000);
                    return false;
                }
            }
            else
            {
                //イベント発生
                EventReceiveMessage(string.Format("{0}", response), 0xFFFF0000);
                return false;
            }
            
            //履歴データの送信
            EventReceiveMessage("== 履歴データの送信 ==", 0xFFFFFFFF, true);
            string[] xmlFileNames = Directory.GetFiles(FishHistoryDB.PATH_FISHHISTORYDB);
            foreach (string xmlFileName in xmlFileNames)
            {
                string filename = Path.GetFileName(xmlFileName);
                List<string> regGroupStr = new List<string>();
                if (MiscTool.GetRegexString(xmlFileName, FishHistoryDB.PATH_FISHHISTORYDB + "\\\\(.*)_([0-9][0-9][0-9][0-9])([0-9][0-9])([0-9][0-9])\\.xml$", out regGroupStr))
                {
                    string playerName = regGroupStr[0];
                    DateTime ymd = DateTime.Parse(string.Format("{0}/{1}/{2}", regGroupStr[1], regGroupStr[2], regGroupStr[3]));
                    //アップロード対象か？
                    FishHistoryDBModel history = historyDB.GetHistoryDB(playerName, ymd);
                    if (!history.Uploaded)
                    {
                        //XMLからプレイヤー情報を消去する
                        history = historyDB.GetHistoryDB(playerName, ymd);
                        history.PlayerName = DUMMY_PLAYER_NAME;
                        //名称チェック 魚
                        var uploadFishes = new List<FishHistoryDBFishModel>();
                        foreach (var fish in history.Fishes)
                        {
                            if (fish.Result == FishResultStatusKind.Catch)
                            {
                                if (
                                    enablename.Rods.Contains(new UpdateDBApiEnableNameRodModel(fish.RodName)) &&
                                    (fish.FishName.Length == 0 | enablename.Fishes.Contains(new UpdateDBApiEnableNameFishModel(fish.FishName))) &&
                                    enablename.Zones.Contains(new UpdateDBApiEnableNameZoneModel(fish.ZoneName)) &&
                                    enablename.Baits.Contains(new UpdateDBApiEnableNameBaitModel(fish.BaitName)))
                                {
                                    uploadFishes.Add(fish);
                                }
                            }
                            else
                            {
                                uploadFishes.Add(fish);
                            }
                        }
                        history.Fishes = uploadFishes;
                        //名称チェック ハラキリ
                        var uploadHarakiri = new List<FishHistoryDBHarakiriModel>();
                        foreach (var harakiri in history.Harakiri)
                        {
                            if (enablename.Fishes.Contains(new UpdateDBApiEnableNameFishModel(harakiri.FishName)) &&
                                (harakiri.ItemName.Length == 0 | enablename.HarakiriItems.Contains(new UpdateDBApiEnableNameHarakiriItemModel(harakiri.ItemName))))
                            {
                                uploadHarakiri.Add(harakiri);
                            }
                        }
                        history.Harakiri = uploadHarakiri;
                        //一時ディレクトリにXMLファイルを保存
                        historyDB.PutHistoryDB(DUMMY_PLAYER_NAME, history, PATH_TEMP);
                        //ファイルアップロード
                        string uploadFileName = historyDB.GetXmlName(DUMMY_PLAYER_NAME, ymd, PATH_TEMP);
                        logger.Output(LogLevelKind.DEBUG, string.Format("ファイル移動 {0}→{1}", filename, uploadFileName));
                        //EventReceiveMessage(string.Format("{0}のキャラクター情報を削除", filename));

                        logger.Output(LogLevelKind.INFO, string.Format("{0}を送信中", uploadFileName));
                        EventReceiveMessage(string.Format("{0}をアップロード", filename));
                        NameValueCollection nvc = new NameValueCollection();
                        response = string.Empty;
                        httpRet = HttpUploadFile(serverName + URL_API_UPLOAD_HISTORY, uploadFileName, "upfile", "application/xml", nvc, out response);
                        logger.Output(LogLevelKind.DEBUG, string.Format("Response:\r{0}", response));
                        if (httpRet)
                        {
                            //レスポンスを取得
                            XmlSerializer serializer = new XmlSerializer(typeof(UpdateDBApiUploadHistoryModel));
                            //byte[] bres = Encoding.UTF8.GetBytes(response);
                            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(response));
                            var res = (UpdateDBApiUploadHistoryModel)serializer.Deserialize(ms);
                            //HistoryDBを送信済みにする
                            FishHistoryDBModel history2 = historyDB.GetHistoryDB(playerName, ymd);
                            history2.Uploaded = true;
                            historyDB.PutHistoryDB(playerName, history2);
                            //エラー発生
                            if (res.Result.Success != "true")
                            {
                                //イベント発生
                                EventReceiveMessage(string.Format("{0}", res.Result.Message), 0xFFFF0000);
                            }
                        }
                        else
                        {
                            //イベント発生
                            EventReceiveMessage(string.Format("{0}", response), 0xFFFF0000);
                        }
                        //テンポラリファイル削除
                        if (File.Exists(uploadFileName))
                        {
                            File.Delete(uploadFileName);
                        }
                    }
                }
            }

            //魚情報を取得
            EventReceiveMessage("== 魚情報を取得  ==", 0xFFFFFFFF, true);
            //ステータスの受信
            response = string.Empty;
            httpRet = Http(serverName + URL_API_STATUS, out response);
            UpdateDBApiStatusModel status = new UpdateDBApiStatusModel();
            if (httpRet)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(UpdateDBApiStatusModel));
                MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(response));
                status = (UpdateDBApiStatusModel)serializer.Deserialize(ms);
                if (status.Result.Success == "true")
                {
                    foreach(UpdateDBApiStatusStatusModel rod in status.Status)
                    {
                        logger.Output(LogLevelKind.DEBUG, string.Format("竿:{0} 更新日:{1}", rod.RodName, rod.LastUpdate));
                        if (DateTime.Parse(rod.LastUpdate) > DateTime.Parse(settings.Global.UpdateDB.LastUpdate))
                        {
                            //竿魚情報取得
                            EventReceiveMessage(string.Format("{0}のダウンロード", rod.RodName));
                            string url = serverName + URL_API_ROD + "/" + WebUtility.HtmlEncode(rod.RodName);
                            string response2 = string.Empty;
                            logger.Output(LogLevelKind.DEBUG, string.Format("HTTP:{0}", url));
                            bool httpRet2 = Http(url, out response2);
                            //登録処理
                            if (httpRet2)
                            {
                                XmlSerializer serializer2 = new XmlSerializer(typeof(UpdateDBApiRodModel));
                                MemoryStream ms2 = new MemoryStream(Encoding.UTF8.GetBytes(response2));
                                var res2 = (UpdateDBApiRodModel)serializer2.Deserialize(ms2);
                                if (res2.Result.Success == "true")
                                {
                                    foreach (FishDBFishModel fish in res2.Rod.Fishes)
                                    {
                                        //public bool AddFish(string iRodName, string iFishName, FishDBFishTypeKind iFishType, FishDBIdModel iID, string iZoneName, string iBaitName)
                                        //IDの追加
                                        foreach (var id in fish.IDs)
                                        {
                                            FishDBIdModel idm = new FishDBIdModel(id.ID1, id.ID2, id.ID3, id.ID4, id.Count, id.Critical, id.ItemType);
                                            fishDB.AddFish(res2.Rod.RodName, fish.FishName, fish.FishType, idm, fish.ZoneNames[0], fish.BaitNames[0]);
                                            Thread.Sleep(1);
                                        }
                                        //エリアの追加
                                        foreach (var zone in fish.ZoneNames)
                                        {
                                            FishDBIdModel idm = new FishDBIdModel(fish.IDs[0].ID1, fish.IDs[0].ID2, fish.IDs[0].ID3, fish.IDs[0].ID4, fish.IDs[0].Count, fish.IDs[0].Critical, fish.IDs[0].ItemType);
                                            fishDB.AddFish(res2.Rod.RodName, fish.FishName, fish.FishType, idm, zone, fish.BaitNames[0]);
                                            Thread.Sleep(1);
                                        }
                                        //エサの追加
                                        foreach (var bait in fish.BaitNames)
                                        {
                                            FishDBIdModel idm = new FishDBIdModel(fish.IDs[0].ID1, fish.IDs[0].ID2, fish.IDs[0].ID3, fish.IDs[0].ID4, fish.IDs[0].Count, fish.IDs[0].Critical, fish.IDs[0].ItemType);
                                            fishDB.AddFish(res2.Rod.RodName, fish.FishName, fish.FishType, idm, fish.ZoneNames[0], bait);
                                            Thread.Sleep(1);
                                        }
                                        Thread.Sleep(1);
                                    }
                                }
                                else
                                {
                                    //イベント発生
                                    EventReceiveMessage(string.Format("{0}", res2.Result.Message), 0xFFFF0000);
                                }
                            }
                            else
                            {
                                //イベント発生
                                EventReceiveMessage(string.Format("{0}", response2), 0xFFFF0000);
                            }
                        }
                        Thread.Sleep(1);
                    }
                }
            }
            else
            {
                //イベント発生
                EventReceiveMessage(string.Format("{0}", response), 0xFFFF0000);
                return false;
            }

            //最終更新日の設定
            settings.Global.UpdateDB.LastUpdate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

            logger.Output(LogLevelKind.INFO, "DB更新終了");
            EventReceiveMessage("データベースの更新が完了しました", 0xFFFFFFFF, true);

            return true;
        }
        #endregion

        #region HTTP
        public static bool Http(string iUrl, out string oResponse)
        {
            try
            {
                HttpWebRequest webreq = (HttpWebRequest)WebRequest.Create(iUrl);
                webreq.Timeout = 180000;
                HttpWebResponse webres = (HttpWebResponse)webreq.GetResponse();

                Stream st = webres.GetResponseStream();
                StreamReader sr = new StreamReader(st, System.Text.Encoding.UTF8);
                string htmlSource = sr.ReadToEnd();
                sr.Close();
                st.Close();
                webres.Close();
                oResponse = htmlSource;
                return true;
            }
            catch (Exception e)
            {
                oResponse = e.Message;
                return false;
            }
        }
        public static bool HttpPost(string iUrl, NameValueCollection iPostNVC, out string oResponse)
        {
            oResponse = "";

            try
            {
                //POSTデータ作成
                bool firstTime = true;
                string postData = string.Empty;
                foreach (string key in iPostNVC.Keys)
                {
                    if (!firstTime) postData += "&";
                    postData += string.Format("{0}={1}", key, WebUtility.HtmlEncode(iPostNVC[key]));
                    firstTime = false;
                }
                byte[] postDataBytes = Encoding.ASCII.GetBytes(postData);
                //リクエスト作成
                WebRequest req = WebRequest.Create(iUrl);
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";
                req.ContentLength = postDataBytes.Length;
                //サーバーへ送信
                Stream reqStream = req.GetRequestStream();
                reqStream.Write(postDataBytes, 0, postDataBytes.Length);
                reqStream.Close();
                //レスポンス受信
                WebResponse res = req.GetResponse();
                Stream resStream = res.GetResponseStream();
                StreamReader sr = new StreamReader(resStream, Encoding.UTF8);
                oResponse = sr.ReadToEnd();
                sr.Close();
                return true;
            }
            catch (Exception e)
            {
                oResponse = e.Message;
                return false;
            }
        }
        public static bool HttpUploadFile(string iUrl, string iUploadFilename, string paramName, string iContentType, NameValueCollection iPostNVC, out string oResponse)
        {
            Console.WriteLine(string.Format("Uploading {0} to {1}", iUploadFilename, iUrl));
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(iUrl);
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";
            wr.KeepAlive = true;
            wr.Credentials = System.Net.CredentialCache.DefaultCredentials;

            Stream rs = wr.GetRequestStream();

            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            foreach (string key in iPostNVC.Keys)
            {
                rs.Write(boundarybytes, 0, boundarybytes.Length);
                string formitem = string.Format(formdataTemplate, key, iPostNVC[key]);
                byte[] formitembytes = Encoding.UTF8.GetBytes(formitem);
                rs.Write(formitembytes, 0, formitembytes.Length);
            }
            rs.Write(boundarybytes, 0, boundarybytes.Length);

            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string header = string.Format(headerTemplate, paramName, iUploadFilename, iContentType);
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            FileStream fileStream = new FileStream(iUploadFilename, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                rs.Write(buffer, 0, bytesRead);
            }
            fileStream.Close();

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();

            WebResponse wresp = null;
            try
            {
                wresp = wr.GetResponse();
                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);
                //Console.WriteLine(string.Format("File uploaded, server response is: {0}", reader2.ReadToEnd()));
                oResponse = reader2.ReadToEnd();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error uploading file", ex);
                if (wresp != null)
                {
                    wresp.Close();
                    wresp = null;
                }
                oResponse = ex.Message;
                return false;
            }
            finally
            {
                wr = null;
            }
        }
        #endregion
    }

    #region Models
    /// <summary>
    /// API CheckVersion レスポンス
    /// </summary>
    [XmlRoot("Response")]
    public class UpdateDBApiCheckVersionModel
    {
        public string VersionEnable { get; set; }
        public string NewVersionExists { get; set; }
        public string NewVersionUrl { get; set; }
        public string Message { get; set; }
        public UpdateDBApiResultModel Result { get; set; }
        public UpdateDBApiCheckVersionModel()
        {
            this.NewVersionExists = string.Empty;
            this.NewVersionUrl = string.Empty;
            this.Message = string.Empty;
            this.Result = new UpdateDBApiResultModel();
        }
    }
    /// <summary>
    /// API EnableName レスポンス
    /// </summary>
    [XmlRoot("Response")]
    public class UpdateDBApiEnableNameModel
    {
        [XmlArray("Rods")]
        [XmlArrayItem("Rod")]
        public List<UpdateDBApiEnableNameRodModel> Rods { get; set; }
        [XmlArray("Fishes")]
        [XmlArrayItem("Fish")]
        public List<UpdateDBApiEnableNameFishModel> Fishes { get; set; }
        [XmlArray("Zones")]
        [XmlArrayItem("Zone")]
        public List<UpdateDBApiEnableNameZoneModel> Zones { get; set; }
        [XmlArray("Baits")]
        [XmlArrayItem("Bait")]
        public List<UpdateDBApiEnableNameBaitModel> Baits { get; set; }
        [XmlArray("HarakiriItems")]
        [XmlArrayItem("HarakiriItem")]
        public List<UpdateDBApiEnableNameHarakiriItemModel> HarakiriItems { get; set; }
        public UpdateDBApiResultModel Result { get; set; }
        public UpdateDBApiEnableNameModel()
        {
            this.Rods = new List<UpdateDBApiEnableNameRodModel>();
            this.Fishes = new List<UpdateDBApiEnableNameFishModel>();
            this.Zones = new List<UpdateDBApiEnableNameZoneModel>();
            this.Baits = new List<UpdateDBApiEnableNameBaitModel>();
            this.HarakiriItems = new List<UpdateDBApiEnableNameHarakiriItemModel>();
            this.Result = new UpdateDBApiResultModel();
        }
    }
    public class UpdateDBApiEnableNameRodModel : IEquatable<UpdateDBApiEnableNameRodModel>
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
        public UpdateDBApiEnableNameRodModel(): this(string.Empty)
        {
        }
        public UpdateDBApiEnableNameRodModel(string iName)
        {
            this.Name = iName;
        }
        public override string ToString()
        {
            return this.Name;
        }
        bool IEquatable<UpdateDBApiEnableNameRodModel>.Equals(UpdateDBApiEnableNameRodModel other)
        {
            if (other == null) return false;
            return (this.Name == other.Name);
        }

    }
    public class UpdateDBApiEnableNameFishModel : IEquatable<UpdateDBApiEnableNameFishModel>
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
        public UpdateDBApiEnableNameFishModel(): this(string.Empty)
        {
        }
        public UpdateDBApiEnableNameFishModel(string iName)
        {
            this.Name = iName;
        }
        public override string ToString()
        {
            return this.Name;
        }
        bool IEquatable<UpdateDBApiEnableNameFishModel>.Equals(UpdateDBApiEnableNameFishModel other)
        {
            if (other == null) return false;
            return (this.Name == other.Name);
        }
    }
    public class UpdateDBApiEnableNameBaitModel : IEquatable<UpdateDBApiEnableNameBaitModel>
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
        public UpdateDBApiEnableNameBaitModel(): this(string.Empty)
        {
        }
        public UpdateDBApiEnableNameBaitModel(string iName)
        {
            this.Name = iName;
        }
        public override string ToString()
        {
            return this.Name;
        }
        bool IEquatable<UpdateDBApiEnableNameBaitModel>.Equals(UpdateDBApiEnableNameBaitModel other)
        {
            if (other == null) return false;
            return (this.Name == other.Name);
        }
    }
    public class UpdateDBApiEnableNameZoneModel : IEquatable<UpdateDBApiEnableNameZoneModel>
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
        public UpdateDBApiEnableNameZoneModel(): this(string.Empty)
        {
        }
        public UpdateDBApiEnableNameZoneModel(string iName)
        {
            this.Name = iName;
        }
        public override string ToString()
        {
            return this.Name;
        }
        bool IEquatable<UpdateDBApiEnableNameZoneModel>.Equals(UpdateDBApiEnableNameZoneModel other)
        {
            if (other == null) return false;
            return (this.Name == other.Name);
        }
    }
    public class UpdateDBApiEnableNameHarakiriItemModel : IEquatable<UpdateDBApiEnableNameHarakiriItemModel>
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
        public UpdateDBApiEnableNameHarakiriItemModel(): this(string.Empty)
        {
        }
        public UpdateDBApiEnableNameHarakiriItemModel(string iName)
        {
            this.Name = iName;
        }
        public override string ToString()
        {
            return this.Name;
        }
        bool IEquatable<UpdateDBApiEnableNameHarakiriItemModel>.Equals(UpdateDBApiEnableNameHarakiriItemModel other)
        {
            if (other == null) return false;
            return (this.Name == other.Name);
        }
    }
    /// <summary>
    /// API UploadHistory レスポンス
    /// </summary>
    [XmlRoot("Response")]
    public class UpdateDBApiUploadHistoryModel
    {
        public UpdateDBApiResultModel Result { get; set; }
        public UpdateDBApiUploadHistoryModel()
        {
            this.Result = new UpdateDBApiResultModel();
        }
    }
    /// <summary>
    /// API Status レスポンス
    /// </summary>
    [XmlRoot("Response")]
    public class UpdateDBApiStatusModel
    {
        [XmlArray("Status")]
        [XmlArrayItem("Rod")]
        public List<UpdateDBApiStatusStatusModel> Status { get; set; }
        public UpdateDBApiResultModel Result { get; set; }
        public UpdateDBApiStatusModel()
        {
            this.Status = new List<UpdateDBApiStatusStatusModel>();
            this.Result = new UpdateDBApiResultModel();
        }
    }
    public class UpdateDBApiStatusStatusModel
    {
        [XmlAttribute("name")]
        public string RodName { get; set; }
        [XmlAttribute("lastupdate")]
        public string LastUpdate { get; set; }
        public UpdateDBApiStatusStatusModel()
        {
            this.RodName = string.Empty;
            this.LastUpdate = string.Empty;
        }
    }
    /// <summary>
    /// API Rod レスポンス
    /// </summary>
    [XmlRoot("Response")]
    public class UpdateDBApiRodModel
    {
        public FishDBModel Rod { get; set; }
        public UpdateDBApiResultModel Result { get; set; }
        public UpdateDBApiRodModel()
        {
            this.Rod = new FishDBModel();
            this.Result = new UpdateDBApiResultModel();
        }
    }
    /// <summary>
    /// API共通 Result
    /// </summary>
    public class UpdateDBApiResultModel

    {
        public string Success { get; set; }
        public string Message { get; set; }
        public UpdateDBApiResultModel()
        {
            this.Success = string.Empty;
            this.Message = string.Empty;
        }
    }
    #endregion
}
