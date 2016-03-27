using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using EliteMMO.API;
using EnjoyFishing;
using NLog;

namespace MiscTools
{
    public class PolTool
    {
        public enum PolStatusKind:int
        {
            Unknown,
            CharacterLoginScreen,
            LoggedIn,
        }
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private PolStatusKind _Status = PolStatusKind.Unknown;
        private EliteAPI _EliteAPI;
        private int _ProcessID = 0;
        private Thread thPol;

        #region コンストラクタ
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PolTool()
        {
            thPol = new Thread(threadPol);
            thPol.Start();
        }
        #endregion

        #region イベント
        /// <summary>
        /// ChangeStatusイベントで返されるデータ
        /// </summary>
        public class ChangeStatusEventArgs : EventArgs
        {
            public PolStatusKind PolStatus;
        }
        public delegate void ChangeStatusEventHandler(object sender, ChangeStatusEventArgs e);
        public event ChangeStatusEventHandler ChangeStatus;
        protected virtual void OnChangeStatus(ChangeStatusEventArgs e)
        {
            if (ChangeStatus != null)
            {
                ChangeStatus(this, e);
            }
        }
        private void EventChangeStatus(PolStatusKind iPolStatus)
        {
            logger.Trace("PolStatus変更:{0}", iPolStatus);
            //返すデータの設定
            ChangeStatusEventArgs e = new ChangeStatusEventArgs();
            e.PolStatus = iPolStatus;
            //イベントの発生
            OnChangeStatus(e);
        }
        #endregion

        #region メンバ
        /// <summary>
        /// EliteAPI
        /// </summary>
        public EliteAPI EliteAPI
        {
            get { return _EliteAPI; }
        }
        /// <summary>
        /// POLステータス
        /// </summary>
        public PolStatusKind Status
        {
            get { return _Status; }
        }
        /// <summary>
        /// POLプロセスID
        /// </summary>
        public int ProcessID
        {
            get { return _ProcessID; }
        }
        #endregion

        /// <summary>
        /// システム終了処理
        /// </summary>
        public void SystemAbort()
        {
            if (this.thPol != null && this.thPol.IsAlive) this.thPol.Abort();
        }
        /// <summary>
        /// POLメインスレッド
        /// </summary>
        private void threadPol()
        {
            LoginStatus lastStatus = LoginStatus.Loading;
            while (true)
            {
                if (_EliteAPI != null)
                {
                    List<Process> pols = GetPolProcess();
                    bool polFoundFlg = false;
                    foreach(Process pol in pols)
                    {
                        if (pol.Id == _ProcessID)
                        {
                            polFoundFlg = true;
                            break;
                        }
                    }
                    if (!polFoundFlg)
                    {
                        changeStatus(PolStatusKind.Unknown);
                        continue;
                    }

                    LoginStatus status = (LoginStatus)_EliteAPI.Player.LoginStatus;
                    if (status != LoginStatus.Loading)
                    {
                        if (status != lastStatus)
                        {
                            switch (status)
                            {
                                case LoginStatus.CharacterLoginScreen:
                                    changeStatus(PolStatusKind.CharacterLoginScreen);
                                    break;
                                case LoginStatus.LoggedIn:
                                    changeStatus(PolStatusKind.LoggedIn);
                                    break;
                            }
                        }
                        lastStatus = status;
                    }
                }
                Thread.Sleep(100);
            }
        }
        /// <summary>
        /// ステータス変更処理
        /// </summary>
        /// <param name="iPolStatus">POLステータス</param>
        private void changeStatus(PolStatusKind iPolStatus)
        {
            this._Status = PolStatusKind.Unknown;
            EventChangeStatus(iPolStatus);
        }
        /// <summary>
        /// POLを取得する
        /// </summary>
        /// <param name="iSelectDualBox">POLが複数存在する場合、選択ウィンドゥを出力するか否か</param>
        /// <returns></returns>
        public bool NewPol(bool iSelectDualBox = true)
        {
            List<Process> pols = GetPolProcess();
            int polId = 0;
            if (pols.Count == 1 || (pols.Count > 0 && !iSelectDualBox))
            {
                polId = pols[0].Id;
            }
            else if (pols.Count > 1)
            {
                polId = selectPolFromForm();
            }
            if (polId > 0)
            {
                _EliteAPI = new EliteAPI(polId);
                this._ProcessID = polId;
                return true;
            }
            return false;
        }
        /// <summary>
        /// POL選択ウィンドウを表示する
        /// </summary>
        /// <param name="iDefaultProcessID"></param>
        /// <returns></returns>
        private int selectPolFromForm(int iDefaultProcessID = -1)
        {
            SelectPolForm frm = new SelectPolForm();
            frm.PolId = iDefaultProcessID;
            frm.PolList = GetPolProcess();
            DialogResult dialogRes = frm.ShowDialog();
            if (frm.PolId > 0)
            {
                return frm.PolId;
            }
            return -1;
        }
        /// <summary>
        /// 実行中のPOLプロセスのリストを取得する
        /// </summary>
        /// <param name="iDefaultProcessID"></param>
        /// <returns></returns>
        public static List<Process> GetPolProcess()
        {
            Process[] pols = Process.GetProcessesByName("pol");
            List<Process> ret = new List<Process>();
            foreach (Process pol in pols)
            {
                if (pol.MainWindowTitle.IndexOf("PlayOnline") < 0) //ログインしてないPOLは弾く
                {
                    ret.Add(pol);
                }
            }
            return ret;
        }
    }
}
