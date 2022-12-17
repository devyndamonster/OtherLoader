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

        private static bool AllowLogging = false;
        private static bool LogLoading = false;
        private static bool LogItemSpawner = false;
        private static bool LogMetaTagging = false;

        public enum LogType
        {
            General,
            Loading,
            ItemSpawner,
            MetaTagging
        }

        public static void Init(bool enabled, bool logLoading, bool logItemSpawner, bool logMetaTagging)
        {
            BepLog = BepInEx.Logging.Logger.CreateLogSource("OtherLoader");

            AllowLogging = enabled;
            LogLoading = logLoading;
            LogItemSpawner = logItemSpawner;
            LogMetaTagging = logMetaTagging;
        }

        public static void Log(string log, LogType type = LogType.General)
        {
            if (CanBeLogged(type))
            {
                BepLog.LogInfo(log);
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

        private static bool CanBeLogged(LogType logType)
        {
            if (!AllowLogging) return false;

            return
                logType == LogType.General ||
                (logType == LogType.Loading && LogLoading) ||
                (logType == LogType.ItemSpawner && LogItemSpawner) ||
                (logType == LogType.MetaTagging && LogMetaTagging);
        }

    }
}
