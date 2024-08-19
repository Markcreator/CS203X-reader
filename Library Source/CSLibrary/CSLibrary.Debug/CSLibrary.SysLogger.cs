using System;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace CSLibrary
{
    /// <summary>
    /// System Logger for Packet Paser
    /// </summary>
    class SysLogger
    {
#if CS101
        private const string model = "CS101";
#elif CS203
        private const string model = "CS203";
#endif
        private static bool m_writeToLog = true;
        private static string sLogFilePath = "";

        public static bool WriteToLog
        {
            get { return m_writeToLog; }
            set { m_writeToLog = value; }
        }
        /// <summary>
        /// Writes a formatted message to the System Application log and to the debug output (if debugging).
        /// Catches and ignores any logging errors such log full, etc.
        /// </summary>
        /// <param name="exception">Exception that is to be logged.</param>
        /// <returns>The message that was logged so it can be shown to the user if desired</returns>
        //[Conditional("DEBUG")]
        public static void LogError(System.Exception exception)
        {
            if (exception == null)
            {
                exception = new ArgumentNullException("exception", "Null exception passed to Logger.LogError()");
            }

            string message = FormatEventMessage(exception);
            if (message.Length > 32000)
                message = message.Substring(0, 32000);
            try
            {
                if (WriteToLog)
                {
                    WriteMessage(message);
                }
            }
            catch (System.Exception) { }

            System.Diagnostics.Debug.WriteLine("ERROR:: " + message);
        }
        //[Conditional("DEBUG")]
        public static void LogError(string Message)
        {
            if (Message.Length > 32000)
                Message = Message.Substring(0, 32000);
            try
            {
                if (WriteToLog)
                {
                    WriteMessage(Message);
                }
            }
            catch (System.Exception) { }

            System.Diagnostics.Debug.WriteLine("Error:: " + Message);
        }
        //[Conditional("DEBUG")]
        public static void LogWarning(string Message)
        {
            if (Message.Length > 32000)
                Message = Message.Substring(0, 32000);
            try
            {
                if (WriteToLog)
                {
                    WriteMessage(Message);
                }
            }
            catch (System.Exception) { }

            System.Diagnostics.Debug.WriteLine("Warning:: " + Message);
        }
        //[Conditional("DEBUG")]
        public static void LogMessage(string Message)
        {
            if (Message.Length > 32000)
                Message = Message.Substring(0, 32000);
            try
            {
                if (WriteToLog)
                {
                    WriteMessage(Message);
                }
            }
            catch (System.Exception) { }
            System.Diagnostics.Debug.WriteLine("Message:: " + Message);
        }
        private static string FormatEventMessage(System.Exception e)
        {
            return e == null ? "None." :
                String.Format(
                    "{0}\r\n" +
                    "{1}\r\n" +
                    "Thread Name: {2}\r\n" +
                    "Stack Trace: {3}\r\n" +
                    "InnerException: {4}\r\n",
                    DateTime.Now.ToString("yyyyMMdd-hhmmss"),
                    e.Message,
                    System.Threading.Thread.CurrentThread.Name,
                    e.StackTrace,
                    e.InnerException);
        }

        private static void WriteMessage(string message)
        {
            if (sLogFilePath == "")
            {
                /*sLogFilePath = Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);

                sLogFilePath = Path.Combine(sLogFilePath, model);*/

                sLogFilePath = Path.Combine(sLogFilePath, System.Reflection.Assembly.GetExecutingAssembly().GetName().Name.ToString());

                sLogFilePath += ".log";
            }
            StreamWriter swLog = null;
            if (File.Exists(sLogFilePath))
            {
                swLog = File.AppendText(sLogFilePath);
            }
            else
            {
                string dir = Path.GetDirectoryName(sLogFilePath);
                if (dir != null && dir.Length != 0)
                {
                    Directory.CreateDirectory(dir);
                }
                swLog = File.CreateText(sLogFilePath);
            }
            if (swLog != null)
            {
                swLog.WriteLine(message);
                swLog.Flush();
                swLog.Close();
            }
        }

    } //public static class SysLogger
}
