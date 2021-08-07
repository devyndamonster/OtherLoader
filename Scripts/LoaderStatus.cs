using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader
{
    public delegate void StatusUpdate();

    public enum LoadOrderType
    {
        LoadFirst,
        LoadLast,
        LoadUnordered
    }

    public class ModLoadOrderContainer
    {
        public List<string> loadFirst = new List<string>();
        public List<string> loadUnordered = new List<string>();
        public List<string> loadLast = new List<string>();

        public void AddToLoadOrder(string modID, LoadOrderType loadOrderType)
        {
            if (loadOrderType == LoadOrderType.LoadFirst) loadFirst.Add(modID);
            else if (loadOrderType == LoadOrderType.LoadLast) loadLast.Add(modID);
            else if (loadOrderType == LoadOrderType.LoadUnordered) loadUnordered.Add(modID);
        }

        public bool CanModLoad(string modID)
        {
            if (loadFirst.Contains(modID) && loadFirst[0] == modID) return true;

            else if (loadUnordered.Contains(modID) && loadFirst.Count == 0) return true;

            else if (loadLast.Contains(modID) && loadFirst.Count == 0 && loadUnordered.Count == 0 && loadLast[0] == modID) return true;

            return false;
        }

        public void RemoveFromLoadOrder(string modID)
        {
            if (loadFirst.Contains(modID)) loadFirst.Remove(modID);
            else if (loadUnordered.Contains(modID)) loadUnordered.Remove(modID);
            else if (loadLast.Contains(modID)) loadLast.Remove(modID);
        }
    }


    public static class LoaderStatus
    {
        private static Dictionary<string, float> trackedLoaders = new Dictionary<string, float>();
        private static List<string> activeLoaders = new List<string>();
        private static Dictionary<string, ModLoadOrderContainer> orderedLoadingLists = new Dictionary<string, ModLoadOrderContainer>();

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
                    orderedLoadingLists[guid].RemoveFromLoadOrder(modID);
                }

                if (GetLoaderProgress() >= 1)
                {
                    OtherLogger.Log("All Items Loaded!", OtherLogger.LogType.General);
                }
            }
        }


        public static void TrackLoader(string modID, LoadOrderType loadOrderType)
        {
            if (!trackedLoaders.ContainsKey(modID)) trackedLoaders.Add(modID, 0);
            else throw new Exception("Tried to track progress on a mod that is already being tracked! ModID: " + modID);

            //If this is not a legacy assetbundle, we also track the order that it was added, so that it can be loaded in that same order
            if (!modID.StartsWith("Legacy"))
            {
                string guid = modID.Split(':')[0].Trim();
                if (!orderedLoadingLists.ContainsKey(guid)) orderedLoadingLists.Add(guid, new ModLoadOrderContainer());
                orderedLoadingLists[guid].AddToLoadOrder(modID, loadOrderType);
            }
        }

        public static bool CanOrderedModLoad(string modID)
        {
            if (modID.StartsWith("Legacy")) return true;

            string guid = modID.Split(':')[0].Trim();

            if (!orderedLoadingLists.ContainsKey(guid)) throw new Exception("Mod was not found in load order! ModID: " + modID);

            return orderedLoadingLists[guid].CanModLoad(modID);
        }

        public static void UpdateProgress(string modID, float progress)
        {
            if (trackedLoaders.ContainsKey(modID)) trackedLoaders[modID] = progress;

            ProgressUpdated?.Invoke();
        }
    }
}
