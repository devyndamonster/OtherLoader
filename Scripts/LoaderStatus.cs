using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader
{
    public static class LoaderStatus
    {
        private static Dictionary<string, float> trackedLoaders = new Dictionary<string, float>();
        private static List<string> activeLoaders = new List<string>();

        public static int NumActiveLoaders { get => activeLoaders.Count; }
        public static List<string> LoadingItems { get => new List<string>(activeLoaders); }

        public static float GetLoaderProgress()
        {
            if (trackedLoaders.Count == 0) return 1;

            float totalProgress = 0;

            foreach (float prog in trackedLoaders.Values)
            {
                totalProgress += prog;
            }

            return totalProgress / trackedLoaders.Count;
        }

        public static void AddActiveLoader(string modID)
        {
            if (!activeLoaders.Contains(modID)) activeLoaders.Add(modID);
        }

        public static void RemoveActiveLoader(string modID)
        {
            activeLoaders.Remove(modID);

            if (GetLoaderProgress() >= 1)
            {
                OtherLogger.Log("All Items Loaded!", OtherLogger.LogType.General);
            }
        }


        public static void TrackLoader(string modID)
        {
            if (!trackedLoaders.ContainsKey(modID)) trackedLoaders.Add(modID, 0);
            else throw new Exception("Tried to track progress on a mod that is already being tracked! ModID: " + modID);
        }


        public static void UpdateProgress(string modID, float progress)
        {
            if (trackedLoaders.ContainsKey(modID)) trackedLoaders[modID] = progress;
        }
    }
}
