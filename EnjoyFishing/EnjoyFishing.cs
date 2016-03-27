using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using MiscTools;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace EnjoyFishing
{
    static class EnjoyFishing
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // エラーハンドラ
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            Thread.GetDomain().UnhandledException += new UnhandledExceptionEventHandler(Program_UnhandledException);

            logger.Info("===== {0} {1} =====", MiscTool.GetAppAssemblyName(), MiscTool.GetAppVersion());

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // POL設定
            PolTool pol = new PolTool();
            if (PolTool.GetPolProcess().Count < 1)
            {
                string msg = "FF11を起動してください。";
                logger.Warn(msg);
                MessageBox.Show(msg, MiscTool.GetAppTitle(), MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                System.Environment.Exit(1); // プログラム終了
            }
            if (!pol.NewPol())
            {
                System.Environment.Exit(1); // プログラム終了
            }
            if (pol.EliteAPI.Player.LoginStatus != (int)LoginStatus.LoggedIn)
            {
                string msg = "キャラクター選択後に起動してください。";
                logger.Warn(msg);
                MessageBox.Show(msg, MiscTool.GetAppTitle(), MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                System.Environment.Exit(1); // プログラム終了
            }
            // シフトキーでテストフォーム表示
            if (Control.ModifierKeys == Keys.Shift)
            {
                logger.Info("EliteAPIテストモードで起動");
                //テストモード
                EliteAPITestForm testForm = new EliteAPITestForm(pol);
                testForm.ShowDialog();
                System.Environment.Exit(0); // プログラム終了
            }
            // ResourceTool
            var resource = new ResourceTool(pol.EliteAPI);
            // メインフォーム表示
            MainForm mainForm = new MainForm(pol, resource);
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
            System.Environment.Exit(1); // プログラム終了
        }
        /// <summary>
        /// 集約エラーハンドラ(ThreadException)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            ShowError(e.Exception, "ThreadException");
            System.Environment.Exit(1); // プログラム終了
        }
        /// <summary>
        /// エラーメッセージの表示
        /// </summary>
        /// <param name="e"></param>
        /// <param name="title"></param>
        private static void ShowError(Exception e, string title)
        {
            logger.Fatal(e, title);
            string msg = string.Format("補足されないエラーが発生しました。\r詳細はログファイルを参照してください。\r\r{0}\r{1}", e.Message, e.StackTrace);
            MessageBox.Show(msg, title, MessageBoxButtons.OK,MessageBoxIcon.Stop);
        }
    }
}
