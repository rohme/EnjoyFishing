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
        public const string SERVER_NAME = "http://localhost";

        private const string DUMMY_PLAYER_NAME = "DUMMY";
        private const string PATH_TEMP = "Temp";
        private const string URL_API_CHECK_VERSION = SERVER_NAME + "/api/enjoyfishing/checkversion";
        private const string URL_API_STATUS = SERVER_NAME + "/api/enjoyfishing/status";
        private const string URL_API_ROD = SERVER_NAME + "/api/enjoyfishing/rod";
        private const string URL_API_UPLOAD_HISTORY = SERVER_NAME + "/api/enjoyfishing/uploadhistory";

        private Settings settings;
        private LoggerTool logger;
        private Thread thUpdateDB;
        private FishDB fishDB;
        private FishHistoryDB historyDB;
 
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
        }
        #endregion

        #region スレッド操作など
        /// <summary>
        /// システム終了処理
        /// </summary>
        public void SystemAbort()
        {
            if (this.thUpdateDB != null && this.thUpdateDB.IsAlive) this.thUpdateDB.Abort();
        }
        /// <summary>
        /// DB更新開始
        /// </summary>
        /// <returns></returns>
        public bool UpdateDBStart()
        {
            //スレッド開始
            thUpdateDB = new Thread(threadUpdateDB);
            thUpdateDB.Start();
            thUpdateDB.Join();
            return true;
        }
        /// <summary>
        /// DB更新中止
        /// </summary>
        /// <returns></returns>
        public void UpdateDBAbort()
        {
            //スレッド停止
            if (thUpdateDB != null && thUpdateDB.IsAlive) thUpdateDB.Abort();
        }
        #endregion

        #region DB更新
        /// <summary>
        /// メインスレッド
        /// </summary>
        private void threadUpdateDB()
        {
            logger.Output(LogLevelKind.INFO, "DB更新開始");
            string response = string.Empty;
            bool httpRet = false;

            //アプリケーションバージョンチェック
            EventReceiveMessage("== クライアントのバージョンチェック ==", 0xFFFFFFFF, true);
            NameValueCollection postCheckVersion = new NameValueCollection();
            postCheckVersion.Add("version", MiscTool.GetAppVersion());
            string responseCheckVersion = string.Empty;
            bool retCheckVersion = HttpPost(URL_API_CHECK_VERSION, postCheckVersion, out responseCheckVersion);
            if (!retCheckVersion)
            {
                EventReceiveMessage(responseCheckVersion, 0xFFFF0000);
                return;
            }
            XmlSerializer seriApiCheckVersion = new XmlSerializer(typeof(UpdateDBApiCheckVersionModel));
            MemoryStream msApiCheckVersion = new MemoryStream(Encoding.UTF8.GetBytes(responseCheckVersion));
            var resCheckVersion = (UpdateDBApiCheckVersionModel)seriApiCheckVersion.Deserialize(msApiCheckVersion);
            if (resCheckVersion.Result.Success == "true")
            {
                if (resCheckVersion.NewVersionExists != "true")
                {
                    EventReceiveMessage(resCheckVersion.Message);
                }
                else
                {
                    EventReceiveMessage(resCheckVersion.Message, 0xFFFF0000);
                    return;
                }
            }
            else
            {
                EventReceiveMessage(resCheckVersion.Message, 0xFFFF0000);
                return;
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
                        //XMLからプレイヤー情報を消去し、一時ディレクトリにXMLファイルを保存
                        history = historyDB.GetHistoryDB(playerName, ymd);
                        history.PlayerName = DUMMY_PLAYER_NAME;
                        historyDB.PutHistoryDB(DUMMY_PLAYER_NAME, history, PATH_TEMP);
                        //ファイルアップロード
                        string uploadFileName = historyDB.GetXmlName(DUMMY_PLAYER_NAME, ymd, PATH_TEMP);
                        logger.Output(LogLevelKind.DEBUG, string.Format("ファイル移動 {0}→{1}", filename, uploadFileName));
                        //EventReceiveMessage(string.Format("{0}のキャラクター情報を削除", filename));

                        logger.Output(LogLevelKind.INFO, string.Format("{0}を送信中", uploadFileName));
                        EventReceiveMessage(string.Format("{0}をアップロード", filename));
                        NameValueCollection nvc = new NameValueCollection();
                        response = string.Empty;
                        httpRet = HttpUploadFile(URL_API_UPLOAD_HISTORY, uploadFileName, "upfile", "application/xml", nvc, out response);
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
                    }
                }
            }
            UpdateDBApiRodModel rr = new UpdateDBApiRodModel();
            rr.Result.Success = "true";
            rr.Result.Message = "メッセージ";
            rr.Rod.RodName = "竿名";
            rr.Rod.Version = "1.1.1";
            FishDBFishModel rm = new FishDBFishModel();
            rm.FishName = "魚名";
            rm.FishType = FishDBFishTypeKind.SmallFish;
            rm.ZoneNames.Add("エリア名");
            rm.BaitNames.Add("エサ名");
            rm.IDs.Add(new FishDBIdModel(1,2,3,4,5,true, FishDBItemTypeKind.Common));
            rr.Rod.Fishes.Add(rm);
            using (FileStream fs = new FileStream(@"c:\test.xml", FileMode.Create, FileAccess.Write, FileShare.None))//ファイルロック
            {
                StreamWriter sw = new StreamWriter(fs, new UTF8Encoding(false));
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add(String.Empty, String.Empty);
                XmlSerializer serializer = new XmlSerializer(typeof(UpdateDBApiRodModel));
                serializer.Serialize(sw, rr, ns);
                //書き込み
                sw.Flush();
                sw.Close();
                sw = null;
            }


            //魚情報を取得
            EventReceiveMessage("== 魚情報を取得  ==", 0xFFFFFFFF, true);
            //ステータスの受信
            response = string.Empty;
            httpRet = Http(URL_API_STATUS, out response);
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
                            string url = URL_API_ROD + "/" + WebUtility.HtmlEncode(rod.RodName);
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
            }

            //最終更新日の設定
            settings.Global.UpdateDB.LastUpdate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

            logger.Output(LogLevelKind.INFO, "DB更新終了");
            EventReceiveMessage("データベースの更新が完了しました", 0xFFFFFFFF, true);
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
        public string NewVersionExists { get; set; }
        public string Message { get; set; }
        public UpdateDBApiResultModel Result { get; set; }
        public UpdateDBApiCheckVersionModel()
        {
            this.NewVersionExists = string.Empty;
            this.Message = string.Empty;
            this.Result = new UpdateDBApiResultModel();
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
