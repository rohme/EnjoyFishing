using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiscTools
{
    public class MiscTool
    {
        [DllImport("user32.dll")]
        static extern Int32 FlashWindowEx(ref FLASHWINFO pwfi);

        [StructLayout(LayoutKind.Sequential)]
        public struct FLASHWINFO
        {
            public UInt32 cbSize;    // FLASHWINFO構造体のサイズ
            public IntPtr hwnd;      // 点滅対象のウィンドウ・ハンドル
            public UInt32 dwFlags;   // 以下の「FLASHW_XXX」のいずれか
            public UInt32 uCount;    // 点滅する回数
            public UInt32 dwTimeout; // 点滅する間隔（ミリ秒単位）
        }

        public const UInt32 FLASHW_STOP = 0;        // 点滅を止める
        public const UInt32 FLASHW_CAPTION = 1;     // タイトルバーを点滅させる
        public const UInt32 FLASHW_TRAY = 2;        // タスクバー・ボタンを点滅させる
        public const UInt32 FLASHW_ALL = 3;         // タスクバー・ボタンとタイトルバーを点滅させる
        public const UInt32 FLASHW_TIMER = 4;       // FLASHW_STOPが指定されるまでずっと点滅させる
        public const UInt32 FLASHW_TIMERNOFG = 12;  // ウィンドウが最前面に来るまでずっと点滅させる

        public static void DebugMessage(string iMsg)
        {
            DateTime dtNow = DateTime.Now;
            string strNow = dtNow.ToString("yyyy/MM/dd HH:mm:ss");
            Console.WriteLine(string.Format("[{0}] {1}", strNow, iMsg));
        }

        public static ArrayList LoadCsvFile(string iCsvFileName)
        {
            ArrayList retCsv = new ArrayList();
            StreamReader file = new StreamReader(iCsvFileName);
            string line;
            while ((line = file.ReadLine()) != null)
            {
                if (line.IndexOf("#") >= 0)
                {
                    line = line.Substring(0, line.IndexOf("#")).Trim();
                }
                if (line.Length > 0)
                {
                    string[] ss = line.Split(',');
                    for (int i = 0; i < ss.Length; i++)
                    {
                        ss[i] = ss[i].Trim();
                    }
                    retCsv.Add(ss);
                }
            }
            file.Close();
            return retCsv;
        }

        /// <summary>
        /// 正規表現で文字列が含まれているか
        /// </summary>
        /// <param name="iString">検索対象文字列</param>
        /// <param name="iMatchString">正規表現文字列</param>
        /// <returns>True:含まれている False:含まれていない</returns>
        public static Boolean IsRegexString(string iString, string iMatchString)
        {
            if (string.IsNullOrEmpty(iString)) return false;
            Regex reg = new Regex(iMatchString, RegexOptions.None);
            Match ma = reg.Match(iString);
            return ma.Success;
        }
        /// <summary>
        /// 正規表現で文字列が含まれているか
        /// </summary>
        /// <param name="iArrString">検索対象文字列</param>
        /// <param name="iMatchString">正規表現文字列</param>
        /// <returns>True:含まれている False:含まれていない</returns>
        public static Boolean IsRegexString(ArrayList iString, string iMatchString)
        {
            for (int i = 0; i < iString.Count; i++)
            {
                if (IsRegexString(iString[i].ToString(), iMatchString)) return true;
            }
            return false;
        }
        /// <summary>
        /// 正規表現で文字列が含まれているか
        /// </summary>
        /// <param name="iArrString">検索対象文字列</param>
        /// <param name="iMatchString">正規表現文字列</param>
        /// <returns>True:含まれている False:含まれていない</returns>
        public static Boolean IsRegexString(string iString, List<string> iMatchString)
        {
            if (string.IsNullOrEmpty(iString)) return false;
            for (int i = 0; i < iMatchString.Count; i++)
            {
                if (IsRegexString(iString, iMatchString[i].ToString())) return true;
            }
            return false;
        }
        /// <summary>
        /// 正規表現で文字列を検索する
        /// </summary>
        /// <param name="iString">検索対象文字列</param>
        /// <param name="iMatchString">正規表現文字列</param>
        /// <returns>正規表現で取得された文字列のArrayList</returns>
        public static List<string> GetRegexString(string iString, string iMatchString)
        {
            List<string> ret = new List<string>();
            GetRegexString(iString, iMatchString, out ret);
            return ret;
        }

        /// <summary>
        /// 正規表現で文字列を検索する
        /// </summary>
        /// <param name="iString">検索対象文字列</param>
        /// <param name="iMatchString">正規表現文字列</param>
        /// <param name="oGroupString">正規表現で取得された文字列のList</param>
        /// <returns>マッチした場合Trueを返す</returns>
        public static bool GetRegexString(string iString, string iMatchString, out List<string> oGroupString)
        {
            oGroupString = new List<string>();
            Regex reg = new Regex(iMatchString, RegexOptions.None);
            Match ma = reg.Match(iString);
            if (ma.Success)
            {
                if (ma.Groups.Count > 1)
                {
                    for (int i = 1; i < ma.Groups.Count; i++)
                    {
                        oGroupString.Add(ma.Groups[i].Value);
                    }
                }
            }
            return ma.Success;
        }

        /// <summary>
        /// アプリケーションタイトルの取得
        /// </summary>
        /// <returns></returns>
        public static string GetAppTitle()
        {
            AssemblyTitleAttribute asmttl = (AssemblyTitleAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyTitleAttribute));
            return asmttl.Title;
        }
        /// <summary>
        /// アプリケーションのアセンブリ名の取得
        /// </summary>
        /// <returns></returns>
        public static string GetAppAssemblyName()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            AssemblyName asmn = asm.GetName();
            return asmn.Name;
        }        
        /// <summary>
        /// アプリケーションバージョンの取得
        /// </summary>
        /// <returns></returns>
        public static string GetAppVersion()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            Version ver = asm.GetName().Version;
            string ret = string.Format("{0}.{1}.{2}", ver.Major, ver.Minor, ver.Build);
            return ret;
        }

        /// <summary>
        /// 整数の乱数を取得する
        /// </summary>
        /// <param name="iFrom">範囲From</param>
        /// <param name="iTo">範囲To</param>
        /// <returns>指定した範囲の整数</returns>
        public static int GetRandomNumber(int iFrom, int iTo)
        {
            int from = 0;
            int to = 0;
            if (iFrom > iTo)
            {
                from = iTo;
                to = iFrom;
            }
            else
            {
                from = iFrom;
                to = iTo;
            }
            Random rnd = new Random();
            return rnd.Next(from, to);
        }

        /// <summary>
        /// 指定されたウィンドウを点滅する
        /// </summary>
        /// <param name="iHandle">ウィンドウハンドル</param>
        /// <param name="iFlashMode">点滅モード</param>
        /// <param name="iFlashCount">点滅回数</param>
        public static void FlashWindow(IntPtr iHandle, UInt32 iFlashMode = FLASHW_ALL, uint iFlashCount = 5)
        {
            FLASHWINFO fInfo = new FLASHWINFO();
            fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));
            fInfo.hwnd = iHandle;
            fInfo.dwFlags = iFlashMode;
            fInfo.uCount = iFlashCount; // 点滅する回数
            fInfo.dwTimeout = 0;
            FlashWindowEx(ref fInfo);
        }

    }
}
