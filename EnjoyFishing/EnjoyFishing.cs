using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using MiscTools;
using FFACETools;
using System.Threading;
using log4net;
using System.IO;
using log4net.Repository.Hierarchy;
using log4net.Core;

namespace EnjoyFishing
{
    static class EnjoyFishing
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // エラーハンドラ
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            Thread.GetDomain().UnhandledException += new UnhandledExceptionEventHandler(Program_UnhandledException);

            // コマンドライン引数の処理
            try
            {
                foreach (string arg in args)
                {
                    string argLower = arg.ToLower();
                    if (MiscTool.IsRegexString(argLower, "/ll:(.+)"))
                    {
                        List<string> reg = MiscTool.GetRegexString(argLower, "/ll:(.+)");
                        Level lv = Level.Off;
                        if (reg[0] == "off") lv = Level.Off;
                        else if (reg[0] == "debug") lv = Level.Debug;
                        else if (reg[0] == "info" || reg[0] == "information") lv = Level.Info;
                        else if (reg[0] == "warn" || reg[0] == "warning") lv = Level.Warn;
                        else if (reg[0] == "error") lv = Level.Error;
                        else if (reg[0] == "fatal") lv = Level.Fatal;
                        else if (reg[0] == "all") lv = Level.All;
                        else throw new ArgumentException("引数の値が不正です。", arg);
                        var root = ((Hierarchy)logger.Logger.Repository).Root;
                        root.Level = lv;
                    }
                }
            }
            catch (ArgumentException e)
            {
                string msg = string.Format("{0}\r\r", e.Message);
                msg += "EnjoyFishing.exe [OPTION]\r";
                msg += "/ll:[LEVEL] 出力ログレベル(none,debug,info,warn,fatal,all)\r";
                MessageBox.Show(msg, MiscTool.GetAppTitle(), MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                System.Environment.Exit(1);//プログラム終了
            }

            logger.InfoFormat("===== {0} {1} =====", MiscTool.GetAppAssemblyName(), MiscTool.GetAppVersion());

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // POL設定
            PolTool pol = new PolTool();
            if (PolTool.GetPolProcess().Count < 1)
            {
                string msg = "FF11を起動してください。";
                logger.Error(msg);
                MessageBox.Show(msg, MiscTool.GetAppTitle(), MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                System.Environment.Exit(1); // プログラム終了
            }
            if (!pol.NewPol())
            {
                System.Environment.Exit(1); // プログラム終了
            }
            if (pol.FFACE.Player.GetLoginStatus != LoginStatus.LoggedIn)
            {
                string msg = "キャラクター選択後に起動してください。";
                logger.Error(msg);
                MessageBox.Show(msg, MiscTool.GetAppTitle(), MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                System.Environment.Exit(1); // プログラム終了
            }
            // シフトキーでテストフォーム表示
            if (Control.ModifierKeys == Keys.Shift)
            {
                logger.Info("FFACEテストモード");
                //テストモード
                FFACETestForm testForm = new FFACETestForm(pol);
                testForm.ShowDialog();
                System.Environment.Exit(0); // プログラム終了
            }
            // メインフォーム表示
            MainForm mainForm = new MainForm(pol);
            mainForm.ShowDialog();
        }

        /// <summary>
        /// 集約エラーハンドラ(UnhandledException)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Program_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            if (ex != null) ShowError(ex, "UnhandledException");
#if !DEBUG
            System.Environment.Exit(1); // プログラム終了
#endif
        }
        /// <summary>
        /// 集約エラーハンドラ(ThreadException)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            ShowError(e.Exception, "ThreadException");
#if !DEBUG
            System.Environment.Exit(1); // プログラム終了
#endif
        }
        /// <summary>
        /// エラーメッセージの表示
        /// </summary>
        /// <param name="e"></param>
        /// <param name="title"></param>
        private static void ShowError(Exception e, string title)
        {
            logger.Fatal(title, e);
            string msg = string.Format("補足されないエラーが発生しました。\r詳細はログファイルを参照してください。\r\r{0}\r{1}", e.Message, e.StackTrace);
            MessageBox.Show(msg, title, MessageBoxButtons.OK,MessageBoxIcon.Stop);
        }
    }
}
