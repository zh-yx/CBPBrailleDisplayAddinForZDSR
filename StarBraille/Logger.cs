using System;
using System.IO;
using System.Runtime.InteropServices;

namespace StarBraille
{
    internal sealed class Logger
    {
        private const int MB_ICONERROR = 0x0010;
        private static string _logFilePath;

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool MessageBeep(uint uType);

        public static void SetLogFileName(string logFileName)
        {
            _logFilePath = Path.Combine(Path.GetTempPath(), logFileName);
        }

        public static void LogException(Exception ex)
        {
            if (_logFilePath == null) return;
            string text = string.Format("{0}\t{1}\t{2}\n", DateTime.Now, ex.Message, ex.StackTrace);
            File.AppendAllText(_logFilePath, text);
            MessageBeep(MB_ICONERROR);
        }
    }
}
