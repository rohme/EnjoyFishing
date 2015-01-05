using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MiscTools
{
    public enum LogLevelKind : int
    {
        DEBUG = 0,
        INFO = 1,
        WARN = 2,
        ERROR = 3,
        FATAL = 4,
    }
    public class LoggerTool
    {
        private const bool DEFAULT_ENABLE = true;
        private const bool DEFAULT_ENABLE_VARDUMP = false;
        private const LogLevelKind DEFAULT_LOGLEVEL = LogLevelKind.INFO;
        private const bool DEFAULT_OUTPUT_CONSOLE = true;
        private const LogLevelKind DEFAULT_OUTPUT_LOGLEVEL = LogLevelKind.INFO;
        private const int DEFAULT_ROTATION_LOG_COUNT = 5;

        private const string FORMAT_LOG_FILENAME1 = "{0}.log";
        private const string FORMAT_LOG_FILENAME2 = "{0}_{1}.log";
        private const string FORMAT_ROTATION_FILENAME1 = "{0}_{1}.log";
        private const string FORMAT_ROTATION_FILENAME2 = "{0}_{1}_{2}.log";
        private const string FORMAT_LOG1 = "{0}({1,-5}): {2} {3}";
        private const string FORMAT_LOG2 = "{0}({1,-5}): [{2}] {3} {4}";
        private const string FORMAT_MEMBER = "({0}:{1})";

        #region コンストラクタ
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="iLogFilename">ログファイル名</param>
        public LoggerTool(string iLogFilename)
            : this(iLogFilename, string.Empty)
        {
        }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="iLogFilename">ログファイル名</param>
        /// <param name="iLogFilenameSub">ログファイル名サブ</param>
        public LoggerTool(string iLogFilename, string iLogFilenameSub)
        {
            this.LogFilename = iLogFilename;
            this.LogFilenameSub = iLogFilenameSub;
        }
        #endregion

        #region メンバ
        /// <summary>
        /// 全出力可否
        /// </summary>
        public bool Enable { get; set; }
        /// <summary>
        /// 変数出力可否
        /// </summary>
        public bool EnableVarDump { get; set; }
        /// <summary>
        /// ログファイル名
        /// </summary>
        public string LogFilename
        {
            get;
            private set;
        }
        /// <summary>
        /// ログファイル名サブ
        /// </summary>
        public string LogFilenameSub
        {
            get;
            private set;
        }
        /// <summary>
        /// コンソール出力可否
        /// </summary>
        public bool OutputConsole { get; set; }
        /// <summary>
        /// デフォルトログレベル
        /// ログレベルを指定しなかったメッセージのログレベル
        /// </summary>
        public LogLevelKind DefaultLogLevel { get; set; }
        /// <summary>
        /// 出力ログレベル
        /// これよりも低いログレベルのメッセーを出力しなくなる
        /// </summary>
        public LogLevelKind OutputLogLevel { get; set; }
        /// <summary>
        /// ローテーションログ数
        /// </summary>
        public int RotationLogCount { get; set; }
        #endregion

        /// <summary>
        /// ログ出力
        /// デフォルトログレベルで出力
        /// </summary>
        /// <param name="iLogMessage">ログ内容</param>
        public void Output(string iLogMessage)
        {
            output(this.DefaultLogLevel, string.Empty, iLogMessage);
        }
        /// <summary>
        /// ログ出力
        /// </summary>
        /// <param name="iLogKindName">ログ種別名</param>
        /// <param name="iLogMessage">ログ内容</param>
        public void Output(string iLogKindName, string iLogMessage)
        {
            output(this.DefaultLogLevel, iLogKindName, iLogMessage);
        }
        /// <summary>
        /// ログ出力
        /// </summary>
        /// <param name="iLogLevel">ログレベル</param>
        /// <param name="iLogMessage">ログ内容</param>
        public void Output(LogLevelKind iLogLevel, string iLogMessage)
        {
            output(iLogLevel, string.Empty, iLogMessage);
        }
        /// <summary>
        /// ログ出力
        /// </summary>
        /// <param name="iLogLevel">ログレベル</param>
        /// <param name="iLogKindName">ログ種別</param>
        /// <param name="iLogMessage">ログ内容</param>
        public void Output(LogLevelKind iLogLevel, string iLogKindName, string iLogMessage)
        {
            output(iLogLevel, iLogKindName, iLogMessage);
        }
        /// <summary>
        /// ログ出力
        /// </summary>
        /// <param name="iLogLevel">ログレベル</param>
        /// <param name="iLogKindName">ログ種別</param>
        /// <param name="iLogMessage">ログ内容</param>
        private void output(LogLevelKind iLogLevel, string iLogKindName, string iLogMessage)
        {
            if (!this.Enable) return;
            if (iLogMessage == string.Empty && iLogKindName == string.Empty) return;
            if (iLogLevel < this.OutputLogLevel) return;

            //一つ前のスタックを取得
            StackFrame callerFrame = new StackFrame(2, true);
            MethodBase method = callerFrame.GetMethod();
            string methodName = method.Name;
            string className = method.ReflectedType.FullName;
            int lineNo = callerFrame.GetFileLineNumber();

            string outLogfileMsg = string.Empty;
            string outConsoleMsg = string.Empty;
            string strDateTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            string strConsoleDateTime = DateTime.Now.ToString("HH:mm:ss");
            string member = string.Format(FORMAT_MEMBER, className + "." + methodName, lineNo);
            if (iLogKindName == string.Empty)
            {
                outLogfileMsg = string.Format(FORMAT_LOG1, strDateTime, iLogLevel, iLogMessage, member);
                outConsoleMsg = string.Format(FORMAT_LOG1, strConsoleDateTime, iLogLevel, iLogMessage, member);
            }
            else
            {
                outLogfileMsg = string.Format(FORMAT_LOG2, strDateTime, iLogLevel, iLogKindName, iLogMessage, member);
                outConsoleMsg = string.Format(FORMAT_LOG1, strConsoleDateTime, iLogLevel, iLogMessage, member);
            }
            writeLog(getLogFilename(), outLogfileMsg);
            if (this.OutputConsole)
            {
                Console.WriteLine(outConsoleMsg);
            }
        }
        /// <summary>
        /// 変数の出力
        /// </summary>
        /// <param name="iObject">変数</param>
        /// <param name="iName">表示名</param>
        public void VarDump(object iObject, string iName = "")
        {
            if (!this.EnableVarDump || this.OutputLogLevel > LogLevelKind.DEBUG) return;
            if (iName != "")
            {
                output(LogLevelKind.DEBUG, string.Empty, string.Format("VarDump:{0}", iName));
            }
            string dmp = var_dump(iObject, 0);
            if (dmp.Length > 0)
            {
                dmp = dmp.Substring(0, dmp.Length - 1);
            }
            //string dmp = ObjectDumperExtensions.DumpToString(iObject, iName);
            writeLog(getLogFilename(), dmp);
        }
        /// <summary>
        /// ログをファイルへ出力する
        /// </summary>
        /// <param name="iFilename">ファイル名</param>
        /// <param name="iLine">ログ</param>
        private void writeLog(string iFilename, string iLine)
        {
            for (int i = 0; i < 100; i++)
            {
                try
                {
                    using (FileStream fs = new FileStream(iFilename, FileMode.Append, FileAccess.Write, FileShare.None))//ファイルロック
                    {
                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            sw.WriteLine(iLine);
                            sw.Dispose();
                        }
                        fs.Dispose();
                    }
                    break;
                }
                catch (IOException)
                {
                    Thread.Sleep(100);
                    continue;
                }
            }
        }

        /// <summary>
        /// ログファイル名の取得
        /// </summary>
        /// <returns>ログファイル名</returns>
        private string getLogFilename()
        {
            string ret = string.Empty;
            if (this.LogFilenameSub == string.Empty)
            {
                ret = string.Format(FORMAT_LOG_FILENAME1, LogFilename);
            }
            else
            {
                ret = string.Format(FORMAT_LOG_FILENAME2, this.LogFilename, this.LogFilenameSub);
            }
            return ret;
        }
        /// <summary>
        /// ローテーション用のログファイル名を取得
        /// </summary>
        /// <param name="iRotationNo">ローテーションNo</param>
        /// <returns>ローテーション用のログファイル名</returns>
        private string getRotationLogFilename(int iRotationNo)
        {
            string ret = string.Empty;
            if (this.LogFilenameSub == string.Empty)
            {
                ret = string.Format(FORMAT_ROTATION_FILENAME1, this.LogFilename, iRotationNo);
            }
            else
            {
                ret = string.Format(FORMAT_ROTATION_FILENAME2, this.LogFilename, this.LogFilenameSub, iRotationNo);
            }
            return ret;
        }

        /// <summary>
        /// ログをローテーションする
        /// </summary>
        public void RotationLog()
        {
            string filename = getLogFilename();
            if (!File.Exists(filename))
            {
                return;
            }

            string pattern = string.Empty;
            if (this.LogFilenameSub == string.Empty)
                pattern = string.Format(FORMAT_ROTATION_FILENAME1, this.LogFilename, "([0-9]*)");
            else
                pattern = string.Format(FORMAT_ROTATION_FILENAME2, this.LogFilename, this.LogFilenameSub, "([0-9]*)");

            string[] tmpFiles = Directory.GetFiles(".");
            List<string> matchFiles = new List<string>();
            foreach (string file in tmpFiles)
            {
                Regex reg = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                Match ma = reg.Match(file);
                if (ma.Groups.Count > 1)
                {
                    matchFiles.Add(file);
                }
            }
            matchFiles.Sort();
            for (int i = matchFiles.Count() - 1; i >= 0; i--)
            {
                if (i >= this.RotationLogCount - 1)
                {
                    File.Delete(matchFiles[i]);
                }
                else
                {
                    int rotationNo = getRotationNoFromFilename(matchFiles[i]);
                    File.Move(matchFiles[i], getRotationLogFilename(rotationNo + 1));
                }
            }
            File.Move(getLogFilename(), getRotationLogFilename(1));

            //空のログファイル作成
            if (!File.Exists(filename))
            {
                File.Create(filename).Close();
            }
        }
        /// <summary>
        /// ローテーションNoをファイル名より取得する
        /// </summary>
        /// <param name="iFileName">ファイル名</param>
        /// <returns>ローテーションNo　取得できなかった場合は-1</returns>
        private int getRotationNoFromFilename(string iFileName)
        {
            string pattern = string.Empty;
            if (this.LogFilenameSub == string.Empty) pattern = "(.*)" + string.Format(FORMAT_ROTATION_FILENAME1, this.LogFilename, "([0-9]*)");
            else pattern = "(.*)" + string.Format(FORMAT_ROTATION_FILENAME2, this.LogFilename, this.LogFilenameSub, "([0-9]*)");


            Regex reg = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            Match ma = reg.Match(iFileName);
            if (ma.Groups.Count > 1)
            {
                return int.Parse(ma.Groups[2].Value);
            }
            return -1;
        }

        private string var_dump(object obj, int recursion)
        {
            StringBuilder result = new StringBuilder();

            // Protect the method against endless recursion
            if (recursion < 5)
            {
                // Determine object type
                Type t = obj.GetType();

                // Get array with properties for this object
                PropertyInfo[] properties = t.GetProperties();

                foreach (PropertyInfo property in properties)
                {
                    try
                    {
                        // Get the property value
                        object value = property.GetValue(obj, null);

                        // Create indenting string to put in front of properties of a deeper level 
                        // We'll need this when we display the property name and value
                        string indent = String.Empty;
                        string spaces = "|   ";
                        string trail = "|...";

                        if (recursion > 0)
                        {
                            indent = new StringBuilder(trail).Insert(0, spaces, recursion - 1).ToString();
                        }

                        if (value != null)
                        {
                            // If the value is a string, add quotation marks
                            string displayValue = value.ToString();
                            if (value is string) displayValue = String.Concat('"', displayValue, '"');

                            // Add property name and value to return string
                            result.AppendFormat("{0}{1} = {2}\n", indent, property.Name, displayValue);

                            try
                            {
                                if (!(value is ICollection))
                                {
                                    // Call var_dump() again to list child properties
                                    // This throws an exception if the current property value 
                                    // is of an unsupported type (eg. it has not properties)
                                    result.Append(var_dump(value, recursion + 1));
                                }
                                else
                                {
                                    // 2009-07-29: added support for collections
                                    // The value is a collection (eg. it's an arraylist or generic list)
                                    // so loop through its elements and dump their properties
                                    int elementCount = 0;
                                    foreach (object element in ((ICollection)value))
                                    {
                                        string elementName = String.Format("{0}[{1}]", property.Name, elementCount);
                                        indent = new StringBuilder(trail).Insert(0, spaces, recursion).ToString();

                                        // Display the collection element name and type
                                        result.AppendFormat("{0}{1} = {2}\n", indent, elementName, element.ToString());

                                        // Display the child properties
                                        result.Append(var_dump(element, recursion + 2));
                                        elementCount++;
                                    }

                                    result.Append(var_dump(value, recursion + 1));
                                }
                            }
                            catch { }
                        }
                        else
                        {
                            // Add empty (null) property to return string
                            result.AppendFormat("{0}{1} = {2}\n", indent, property.Name, "null");
                        }
                    }
                    catch 
                    {
                        // Some properties will throw an exception on property.GetValue() 
                        // I don't know exactly why this happens, so for now i will ignore them...
                    }
                }
            }

            return result.ToString();
        }
    }
}
