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

    public class BundleLoadStatus
    {
        public string ModID;
        public bool IsLoaded;
        public bool CanLoad;
        public LoadOrderType LoadOrderType;

        public BundleLoadStatus(string ModID, bool IsLoaded, LoadOrderType LoadOrderType)
        {
            this.ModID = ModID;
            this.IsLoaded = IsLoaded;
            this.CanLoad = false;
            this.LoadOrderType = LoadOrderType;
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

                string guid = modID.Split(':')[0].Trim();
                orderedLoadingLists[guid].MarkBundleAsLoaded(modID);
                
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

            OtherLogger.Log($"Tracking modded bundle ({modID}), Load Order ({loadOrderType})", OtherLogger.LogType.Loading);

            //Add bundle to load order
            string guid = modID.Split(':')[0].Trim();
            if (!orderedLoadingLists.ContainsKey(guid)) orderedLoadingLists.Add(guid, new ModLoadOrderContainer());
            orderedLoadingLists[guid].AddToLoadOrder(modID, loadOrderType);
            
        }

        /// <summary>
        /// Returns true if the given modID is allowed to load based on other assetbundle dependencies
        /// </summary>
        /// <param name="modID"></param>
        /// <returns></returns>
        public static bool CanOrderedModLoad(string modID)
        {
            string guid = modID.Split(':')[0].Trim();

            if (!orderedLoadingLists.ContainsKey(guid)) throw new Exception("Mod was not found in load order! ModID: " + modID);

            return orderedLoadingLists[guid].CanBundleLoad(modID);
        }

        public static void UpdateProgress(string modID, float progress)
        {
            if (trackedLoaders.ContainsKey(modID)) trackedLoaders[modID] = progress;

            ProgressUpdated?.Invoke();
        }


       

        private class ModLoadOrderContainer
        {
            /// <summary>
            /// A dictionary of asset bundles designated to load first. The key is the UniqueAssetID, and the value is wether the bundle is already loaded
            /// </summary>
            public List<BundleLoadStatus> loadFirst = new List<BundleLoadStatus>();

            /// <summary>
            /// A dictionary of asset bundles designated to load unordered. The key is the UniqueAssetID, and the value is wether the bundle is already loaded
            /// </summary>
            public List<BundleLoadStatus> loadUnordered = new List<BundleLoadStatus>();

            /// <summary>
            /// A dictionary of asset bundles designated to load last. The key is the UniqueAssetID, and the value is wether the bundle is already loaded
            /// </summary>
            public List<BundleLoadStatus> loadLast = new List<BundleLoadStatus>();

            public Dictionary<string, BundleLoadStatus> bundleStatusDic = new Dictionary<string, BundleLoadStatus>();

            public void AddToLoadOrder(string modID, LoadOrderType loadOrderType)
            {
                BundleLoadStatus loadStatus = new BundleLoadStatus(modID, false, loadOrderType);

                //With this new bundle, we should decide if it is able to start being loaded immediately

                if (loadOrderType == LoadOrderType.LoadFirst)
                {
                    if (loadFirst.Count == 0) loadStatus.CanLoad = true;

                    //When adding load first bundles, there must never be unordered or load last bundles already added
                    if(loadUnordered.Count != 0 || loadLast.Count != 0)
                    {
                        OtherLogger.LogError($"Mod is set to load first, but it looks like unordered or load last mods are already loading! ModID ({modID})");
                    }
                }

                if(loadOrderType == LoadOrderType.LoadUnordered)
                {
                    if (loadFirst.Count == 0) loadStatus.CanLoad = true;

                    //When adding load unordered bundles, there must never be load last bundles already added
                    if (loadLast.Count != 0)
                    {
                        OtherLogger.LogError($"Mod is set to load unordered, but it looks like load last mods are already loading! ModID ({modID})");
                    }
                }

                if(loadOrderType == LoadOrderType.LoadLast)
                {
                    if (loadFirst.Count == 0 && loadUnordered.Count == 0 && loadLast.Count == 0) loadStatus.CanLoad = true;
                }


                if (loadOrderType == LoadOrderType.LoadFirst) loadFirst.Add(loadStatus);
                else if (loadOrderType == LoadOrderType.LoadLast) loadLast.Add(loadStatus);
                else if (loadOrderType == LoadOrderType.LoadUnordered) loadUnordered.Add(loadStatus);

                bundleStatusDic.Add(modID, loadStatus);
            }

            public bool CanBundleLoad(string modID)
            {
                BundleLoadStatus loadStatus = bundleStatusDic[modID];

                if (loadStatus.IsLoaded)
                {
                    OtherLogger.LogError($"Mod is already loaded, but something is still asking to load it! ModID ({modID})");
                    return false;
                }

                return loadStatus.CanLoad;
            }

            public void MarkBundleAsLoaded(string modID)
            {
                //First, mark bundle as loaded
                BundleLoadStatus bundleStatus = bundleStatusDic[modID];
                bundleStatus.IsLoaded = true;

                //Next, mark one of the bundles that aren't yet loaded as able to load
                if(bundleStatus.LoadOrderType == LoadOrderType.LoadFirst)
                {
                    if(loadFirst.All(o => o.IsLoaded == true))
                    {
                        if(loadUnordered.Count > 0)
                        {
                            loadUnordered.ForEach(o => o.CanLoad = true);
                        }
                        else if(loadLast.Count > 0)
                        {
                            loadLast[0].CanLoad = true;
                        }
                    }

                    else
                    {
                        //It is assumed that since load first is sequential, there will always be a next bundle that hasn't started loading if they are not all loaded yet
                        loadFirst.First(o => o.CanLoad == false).CanLoad = true;
                    }
                }

                else if(bundleStatus.LoadOrderType == LoadOrderType.LoadUnordered)
                {
                    if(loadUnordered.All(o => o.IsLoaded))
                    {
                        if(loadLast.Count != 0)
                        {
                            loadLast[0].CanLoad = true;
                        }
                    }

                    //If not all of the unordered bundles have loaded yet, it is assumed that they are still currently loading, so we don't have to set them to load
                }

                else if(bundleStatus.LoadOrderType == LoadOrderType.LoadLast)
                {
                    BundleLoadStatus nextBundle = loadLast.FirstOrDefault(o => o.CanLoad == false);

                    //If there is no next bundle to load, it will be null, and all bundles are loaded
                    if(nextBundle != null)
                    {
                        nextBundle.CanLoad = true;
                    }
                }

            }
        }

    }
}
