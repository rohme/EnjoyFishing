using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using MiscTools;
using FFACETools;

namespace EnjoyFishing
{
    static class EnjoyFishing
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new MainForm());

            //コマンドライン引数の処理
            SettingsArgsModel ARGS = new SettingsArgsModel();
            foreach (string arg in args)
            {
                string argLower = arg.ToLower();
                if (argLower.Equals("/d"))
                {
                    ARGS.LoggerEnable = true;
                    ARGS.LoggerLogLevel = LogLevelKind.INFO;
                }
                if (argLower.Equals("/vd"))
                {
                    ARGS.LoggerVarDumpEnable = true;
                }
                if (MiscTool.IsRegexString(argLower, "/ll:([0-4])"))
                {
                    List<string> reg = MiscTool.GetRegexString(argLower, "/ll:([0-4])");
                    ARGS.LoggerLogLevel = (LogLevelKind)int.Parse(reg[0]);
                }
            }

            //POL設定
            PolTool pol = new PolTool();
            if (PolTool.GetPolProcess().Count < 1)
            {
                MessageBox.Show("FF11を起動してください。", MiscTool.GetAppTitle(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Environment.Exit(0);//プログラム終了
            }
            if (!pol.NewPol())
            {
                System.Environment.Exit(0);//プログラム終了
            }
            if (pol.FFACE.Player.GetLoginStatus != LoginStatus.LoggedIn)
            {
                MessageBox.Show("キャラクター選択後に起動してください。", MiscTool.GetAppTitle(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Environment.Exit(0);//プログラム終了
            }

            //フォーム表示
            MainForm mainForm = new MainForm(pol, ARGS);
            mainForm.ShowDialog();
        }
    }
}
