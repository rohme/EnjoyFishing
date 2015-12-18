using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using FFACETools;
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
#if DEBUG
            // VS出力にログ表示
            var targetConsole = new ConsoleTarget();
            targetConsole.Layout = @"@${uppercase:${level:padding=-5}} ${message}";
            LogManager.Configuration.AddTarget("console", targetConsole);
            var ruleConsole = new LoggingRule("*", LogLevel.Debug, targetConsole);
            LogManager.Configuration.LoggingRules.Add(ruleConsole);
#endif

            // コマンドライン引数の処理
            try
            {
                foreach (string arg in args)
                {
                    string argLower = arg.ToLower();
                    if (MiscTool.IsRegexString(argLower, "/ll:(.+)"))
                    {
                        List<string> reg = MiscTool.GetRegexString(argLower, "/ll:(.+)");
                        LogLevel lv = LogLevel.Off;
                        if (reg[0] == "off") lv = LogLevel.Off;
                        else if (reg[0] == "trace") lv = LogLevel.Trace;
                        else if (reg[0] == "debug") lv = LogLevel.Debug;
                        else if (reg[0] == "info" || reg[0] == "information") lv = LogLevel.Info;
                        else if (reg[0] == "warn" || reg[0] == "warning") lv = LogLevel.Warn;
                        else if (reg[0] == "error") lv = LogLevel.Error;
                        else if (reg[0] == "fatal") lv = LogLevel.Fatal;
                        else throw new ArgumentException("引数の値が不正です。", arg);
                        foreach (var rule in LogManager.Configuration.LoggingRules)
                        {
                            foreach (var target in rule.Targets)
                            {
                                if (target.GetType() == typeof(FileTarget))
                                {
                                    rule.EnableLoggingForLevel(lv);
                                    break;
                                }
                            }
                        }
                        LogManager.ReconfigExistingLoggers();
                    }
                }
            }
            catch (ArgumentException e)
            {
                string msg = string.Format("{0}\r\r", e.Message);
                msg += "EnjoyFishing.exe [OPTION]\r";
                msg += "OPTIONS:\r";
                msg += "/ll:[off|trace|debug|info|warn|fatal] 出力ログレベル\r";
                MessageBox.Show(msg, MiscTool.GetAppTitle(), MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                System.Environment.Exit(1);//プログラム終了
            }

            logger.Info("===== {0} {1} =====", MiscTool.GetAppAssemblyName(), MiscTool.GetAppVersion());

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
            logger.Fatal(e, title);
            string msg = string.Format("補足されないエラーが発生しました。\r詳細はログファイルを参照してください。\r\r{0}\r{1}", e.Message, e.StackTrace);
            MessageBox.Show(msg, title, MessageBoxButtons.OK,MessageBoxIcon.Stop);
        }
    }
}
