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
        public string BundleID;
        public bool IsLoaded;
        public bool CanLoad;
        public LoadOrderType LoadOrderType;

        public BundleLoadStatus(string BundleID, bool IsLoaded, LoadOrderType LoadOrderType)
        {
            this.BundleID = BundleID;
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

        public static void AddActiveLoader(string bundleID)
        {
            if (!activeLoaders.Contains(bundleID)) activeLoaders.Add(bundleID);
        }

        public static void RemoveActiveLoader(string bundleID)
        {
            if (activeLoaders.Contains(bundleID))
            {
                activeLoaders.Remove(bundleID);

                string modPath = LoaderUtils.GetModPathFromUniqueID(bundleID);
                orderedLoadingLists[modPath].MarkBundleAsLoaded(bundleID);
                
                if (GetLoaderProgress() >= 1)
                {
                    OtherLogger.Log("All Items Loaded!", OtherLogger.LogType.General);
                }
            }
        }


        public static void TrackLoader(string bundleID, LoadOrderType loadOrderType, bool isLoadedImmediate)
        {
            //Only actively track this asset bundle if it is immediately being loaded
            if (isLoadedImmediate)
            {
                if (!trackedLoaders.ContainsKey(bundleID)) trackedLoaders.Add(bundleID, 0);
                else throw new Exception("Tried to track progress on a mod that is already being tracked! BundleID: " + bundleID);
            }

            OtherLogger.Log($"Tracking modded bundle ({bundleID}), Load Order ({loadOrderType})", OtherLogger.LogType.Loading);

            //Add bundle to load order
            string modPath = LoaderUtils.GetModPathFromUniqueID(bundleID);
            if (!orderedLoadingLists.ContainsKey(modPath))
            {
                OtherLogger.Log("Adding new load order entry for mod", OtherLogger.LogType.Loading);
                orderedLoadingLists.Add(modPath, new ModLoadOrderContainer());
            }
               
            orderedLoadingLists[modPath].AddToLoadOrder(bundleID, loadOrderType);
        }


        /// <summary>
        /// Returns true if the given modID is allowed to load based on other assetbundle dependencies
        /// </summary>
        /// <param name="modID"></param>
        /// <returns></returns>
        public static bool CanOrderedModLoad(string bundleID)
        {
            string modPath = LoaderUtils.GetModPathFromUniqueID(bundleID);

            if (!orderedLoadingLists.ContainsKey(modPath)) throw new Exception("Mod was not found in load order! BundleID: " + bundleID);

            return (OtherLoader.MaxActiveLoaders <= 0 || NumActiveLoaders < OtherLoader.MaxActiveLoaders) && orderedLoadingLists[modPath].CanBundleLoad(bundleID);
        }

        public static void UpdateProgress(string bundleID, float progress)
        {
            if (trackedLoaders.ContainsKey(bundleID)) trackedLoaders[bundleID] = progress;

            ProgressUpdated?.Invoke();
        }


        public static List<BundleLoadStatus> GetBundleDependencies(string bundleID)
        {
            string modPath = LoaderUtils.GetModPathFromUniqueID(bundleID);
            return orderedLoadingLists[modPath].GetBundleDependencies(bundleID);
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

            public void AddToLoadOrder(string bundleID, LoadOrderType loadOrderType)
            {
                BundleLoadStatus loadStatus = new BundleLoadStatus(bundleID, false, loadOrderType);

                //With this new bundle, we should decide if it is able to start being loaded immediately

                if (loadOrderType == LoadOrderType.LoadFirst)
                {
                    if (loadFirst.Count == 0) loadStatus.CanLoad = true;

                    //When adding load first bundles, there must never be unordered or load last bundles already added
                    if(loadUnordered.Count != 0 || loadLast.Count != 0)
                    {
                        OtherLogger.LogError($"Mod is set to load first, but it looks like unordered or load last mods are already loading! BundleID ({bundleID})");
                        loadUnordered.ForEach(o => OtherLogger.LogError($"Load Unordered BundleID ({o.BundleID})"));
                        loadLast.ForEach(o => OtherLogger.LogError($"Load Last BundleID ({o.BundleID})"));
                    }
                }

                if(loadOrderType == LoadOrderType.LoadUnordered)
                {
                    if (loadFirst.Count == 0) loadStatus.CanLoad = true;

                    //When adding load unordered bundles, there must never be load last bundles already added
                    if (loadLast.Count != 0)
                    {
                        OtherLogger.LogError($"Mod is set to load unordered, but it looks like load last mods are already loading! BundleID ({bundleID})");
                        loadLast.ForEach(o => OtherLogger.LogError($"Load Last BundleID ({o.BundleID})"));
                    }
                }

                if(loadOrderType == LoadOrderType.LoadLast)
                {
                    if (loadFirst.Count == 0 && loadUnordered.Count == 0 && loadLast.Count == 0) loadStatus.CanLoad = true;
                }


                if (loadOrderType == LoadOrderType.LoadFirst) loadFirst.Add(loadStatus);
                else if (loadOrderType == LoadOrderType.LoadLast) loadLast.Add(loadStatus);
                else if (loadOrderType == LoadOrderType.LoadUnordered) loadUnordered.Add(loadStatus);

                bundleStatusDic.Add(bundleID, loadStatus);
            }

            public bool CanBundleLoad(string bundleID)
            {
                BundleLoadStatus loadStatus = bundleStatusDic[bundleID];

                if (loadStatus.IsLoaded)
                {
                    OtherLogger.LogError($"Mod is already loaded, but something is still asking to load it! BundleID ({bundleID})");
                    return false;
                }

                return loadStatus.CanLoad;
            }

            public void MarkBundleAsLoaded(string bundleID)
            {
                //First, mark bundle as loaded
                BundleLoadStatus bundleStatus = bundleStatusDic[bundleID];
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


            public List<BundleLoadStatus> GetBundleDependencies(string bundleID)
            {
                List<BundleLoadStatus> depList = new List<BundleLoadStatus>();
                BundleLoadStatus bundleStatus = bundleStatusDic[bundleID];

                if(bundleStatus.LoadOrderType == LoadOrderType.LoadFirst)
                {
                    foreach(BundleLoadStatus dep in loadFirst)
                    {
                        if (dep.BundleID == bundleID) break;

                        if (!dep.IsLoaded)
                        {
                            depList.Add(dep);
                        }
                    }
                }

                else if (bundleStatus.LoadOrderType == LoadOrderType.LoadUnordered)
                {
                    foreach (BundleLoadStatus dep in loadFirst)
                    {
                        if (!dep.IsLoaded)
                        {
                            depList.Add(dep);
                        }
                    }
                }

                else if (bundleStatus.LoadOrderType == LoadOrderType.LoadLast)
                {
                    foreach (BundleLoadStatus dep in loadFirst)
                    {
                        if (!dep.IsLoaded)
                        {
                            depList.Add(dep);
                        }
                    }

                    foreach (BundleLoadStatus dep in loadUnordered)
                    {
                        if (!dep.IsLoaded)
                        {
                            depList.Add(dep);
                        }
                    }

                    foreach (BundleLoadStatus dep in loadLast)
                    {
                        if (dep.BundleID == bundleID) break;

                        if (!dep.IsLoaded)
                        {
                            depList.Add(dep);
                        }
                    }
                }

                return depList;
            }

        }

    }
}
