using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader
{
    public delegate void StatusUpdate();

    public static class LoaderStatus
    {
        private static Dictionary<string, float> trackedLoaders = new Dictionary<string, float>();
        private static List<string> activeLoaders = new List<string>();
        private static Dictionary<string, List<string>> orderedLoadingLists = new Dictionary<string, List<string>>();

        public static int NumActiveLoaders { get => activeLoaders.Count; }
        public static List<string> LoadingItems { get => new List<string>(activeLoaders); }

        public static event StatusUpdate ProgressUpdated;

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
            if (activeLoaders.Contains(modID))
            {
                activeLoaders.Remove(modID);

                if (!modID.StartsWith("Legacy"))
                {
                    string guid = modID.Split(':')[0].Trim();
                    orderedLoadingLists[guid].Remove(modID);
                }

                if (GetLoaderProgress() >= 1)
                {
                    OtherLogger.Log("All Items Loaded!", OtherLogger.LogType.General);
                }
            }
        }


        public static void TrackLoader(string modID)
        {
            if (!trackedLoaders.ContainsKey(modID)) trackedLoaders.Add(modID, 0);
            else throw new Exception("Tried to track progress on a mod that is already being tracked! ModID: " + modID);

            //If this is not a legacy assetbundle, we also track the order that it was added, so that it can be loaded in that same order
            if (!modID.StartsWith("Legacy"))
            {
                string guid = modID.Split(':')[0].Trim();
                if (!orderedLoadingLists.ContainsKey(guid)) orderedLoadingLists.Add(guid, new List<string>());
                orderedLoadingLists[guid].Add(modID);
            }
        }

        public static bool CanOrderedModLoad(string modID)
        {
            if(modID.StartsWith("Legacy")) throw new Exception("Tried to check load order for legacy mod! ModID: " + modID);

            string guid = modID.Split(':')[0].Trim();

            if (!orderedLoadingLists.ContainsKey(guid)) throw new Exception("Mod was not found in load order! ModID: " + modID);

            if (!orderedLoadingLists[guid].Contains(modID)) throw new Exception("Asset Bundle was not found in load order! ModID: " + modID);

            if (orderedLoadingLists[guid][0] == modID) return true;

            return false;
        }


        public static void UpdateProgress(string modID, float progress)
        {
            if (trackedLoaders.ContainsKey(modID)) trackedLoaders[modID] = progress;

            ProgressUpdated?.Invoke();
        }
    }
}
