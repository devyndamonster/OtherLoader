using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader
{
    public static class OtherLogger
    {

        public static ManualLogSource BepLog;

        public static bool AllowLogging = false;
        public static bool LogLoading = false;

        public enum LogType
        {
            General,
            Loading
        }

        public static void Init()
        {
            BepLog = BepInEx.Logging.Logger.CreateLogSource("OtherLoader");
        }

        public static void Log(string log, LogType type)
        {
            if (AllowLogging)
            {
                if (type == LogType.General)
                {
                    BepLog.LogInfo(log);
                }
                else if (type == LogType.Loading && LogLoading)
                {
                    BepLog.LogInfo(log);
                }
            }
        }

        public static void LogWarning(string log)
        {
            BepLog.LogWarning(log);
        }

        public static void LogError(string log)
        {
            BepLog.LogError(log);
        }

    }
}
