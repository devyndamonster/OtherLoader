using BepInEx.Logging;
using OtherLoader.Core.Models;
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

        public static void Init(OtherLoaderConfig config)
        {
            BepLog = BepInEx.Logging.Logger.CreateLogSource("OtherLoader");

            AllowLogging = config.EnableLogging.Value;
            LogLoading = config.LogLoading.Value;
            LogItemSpawner = config.LogItemSpawner.Value;
            LogMetaTagging = config.LogMetaTagging.Value;
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
