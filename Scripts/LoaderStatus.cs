using System;
using System.Collections.Generic;
using System.IO;
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


    public enum BundleStatus
    {
        Waiting,
        CanLoad,
        Loading,
        Loaded,
        Unloaded
    }


    public class BundleInfo
    {
        public string BundleID;
        public BundleStatus Status;
        public LoadOrderType LoadOrderType;


        public BundleInfo(string BundleID, LoadOrderType LoadOrderType)
        {
            this.BundleID = BundleID;
            this.LoadOrderType = LoadOrderType;
            Status = BundleStatus.Waiting;
        }

        public string GetBundlePath()
        {
            return Path.Combine(LoaderUtils.GetModPathFromUniqueID(BundleID), LoaderUtils.GetBundleNameFromUniqueID(BundleID));
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

        public static void RemoveActiveLoader(string bundleID, bool permanentlyLoaded)
        {
            if (activeLoaders.Contains(bundleID))
            {
                activeLoaders.Remove(bundleID);

                string modPath = LoaderUtils.GetModPathFromUniqueID(bundleID);
                orderedLoadingLists[modPath].MarkBundleAsLoaded(bundleID, permanentlyLoaded);
                
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


        public static List<BundleInfo> GetBundleDependencies(string bundleID)
        {
            string modPath = LoaderUtils.GetModPathFromUniqueID(bundleID);
            return orderedLoadingLists[modPath].GetBundleDependencies(bundleID);
        }


       

        private class ModLoadOrderContainer
        {
            /// <summary>
            /// A dictionary of asset bundles designated to load first. The key is the UniqueAssetID, and the value is wether the bundle is already loaded
            /// </summary>
            public List<BundleInfo> loadFirst = new List<BundleInfo>();

            /// <summary>
            /// A dictionary of asset bundles designated to load unordered. The key is the UniqueAssetID, and the value is wether the bundle is already loaded
            /// </summary>
            public List<BundleInfo> loadUnordered = new List<BundleInfo>();

            /// <summary>
            /// A dictionary of asset bundles designated to load last. The key is the UniqueAssetID, and the value is wether the bundle is already loaded
            /// </summary>
            public List<BundleInfo> loadLast = new List<BundleInfo>();

            public Dictionary<string, BundleInfo> bundleInfoDic = new Dictionary<string, BundleInfo>();

            public void AddToLoadOrder(string bundleID, LoadOrderType loadOrderType)
            {
                BundleInfo bundleInfo = new BundleInfo(bundleID, loadOrderType);

                //With this new bundle, we should decide if it is able to start being loaded immediately

                if (loadOrderType == LoadOrderType.LoadFirst)
                {
                    if (loadFirst.Count == 0) bundleInfo.Status = BundleStatus.CanLoad;

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
                    if (loadFirst.Count == 0) bundleInfo.Status = BundleStatus.CanLoad;

                    //When adding load unordered bundles, there must never be load last bundles already added
                    if (loadLast.Count != 0)
                    {
                        OtherLogger.LogError($"Mod is set to load unordered, but it looks like load last mods are already loading! BundleID ({bundleID})");
                        loadLast.ForEach(o => OtherLogger.LogError($"Load Last BundleID ({o.BundleID})"));
                    }
                }

                if(loadOrderType == LoadOrderType.LoadLast)
                {
                    if (loadFirst.Count == 0 && loadUnordered.Count == 0 && loadLast.Count == 0) bundleInfo.Status = BundleStatus.CanLoad;
                }


                if (loadOrderType == LoadOrderType.LoadFirst) loadFirst.Add(bundleInfo);
                else if (loadOrderType == LoadOrderType.LoadLast) loadLast.Add(bundleInfo);
                else if (loadOrderType == LoadOrderType.LoadUnordered) loadUnordered.Add(bundleInfo);

                bundleInfoDic.Add(bundleID, bundleInfo);
            }

            public bool CanBundleLoad(string bundleID)
            {
                BundleInfo loadStatus = bundleInfoDic[bundleID];

                if (loadStatus.Status == BundleStatus.Loaded || loadStatus.Status == BundleStatus.Loading)
                {
                    OtherLogger.LogError($"Mod is already loading or loaded, but something is still asking to load it! BundleID ({bundleID})");
                    return false;
                }

                return loadStatus.Status == BundleStatus.CanLoad;
            }

            public void MarkBundleAsLoaded(string bundleID, bool permanentlyLoaded)
            {
                //First, mark bundle as loaded
                BundleInfo bundleInfo = bundleInfoDic[bundleID];

                if (permanentlyLoaded)
                {
                    bundleInfo.Status = BundleStatus.Loaded;
                }
                else
                {
                    bundleInfo.Status = BundleStatus.Unloaded;
                }
                

                //Next, mark one of the bundles that aren't yet loaded as able to load
                if(bundleInfo.LoadOrderType == LoadOrderType.LoadFirst)
                {
                    BundleInfo nextBundle = loadFirst.FirstOrDefault(o => o.Status == BundleStatus.Waiting);

                    //If there is no next bundle to load, it will be null, and all bundles are loaded
                    if (nextBundle != null)
                    {
                        nextBundle.Status = BundleStatus.CanLoad;
                    }

                    else
                    {
                        if (loadUnordered.Count > 0)
                        {
                            loadUnordered.ForEach(o => o.Status = BundleStatus.CanLoad);
                        }
                        else if (loadLast.Count > 0)
                        {
                            loadLast[0].Status = BundleStatus.CanLoad;
                        }
                    }
                }

                else if(bundleInfo.LoadOrderType == LoadOrderType.LoadUnordered)
                {
                    if(loadUnordered.All(o => o.Status == BundleStatus.Loaded || o.Status == BundleStatus.Unloaded))
                    {
                        if(loadLast.Count != 0)
                        {
                            loadLast[0].Status = BundleStatus.CanLoad;
                        }
                    }

                    //If not all of the unordered bundles have loaded yet, it is assumed that they are still currently loading, so we don't have to set them to load
                }

                else if(bundleInfo.LoadOrderType == LoadOrderType.LoadLast)
                {
                    BundleInfo nextBundle = loadLast.FirstOrDefault(o => o.Status == BundleStatus.Waiting);

                    //If there is no next bundle to load, it will be null, and all bundles are loaded
                    if(nextBundle != null)
                    {
                        nextBundle.Status = BundleStatus.CanLoad;
                    }
                }
            }


            public List<BundleInfo> GetBundleDependencies(string bundleID)
            {
                List<BundleInfo> depList = new List<BundleInfo>();
                BundleInfo bundleStatus = bundleInfoDic[bundleID];

                if(bundleStatus.LoadOrderType == LoadOrderType.LoadFirst)
                {
                    foreach(BundleInfo dep in loadFirst)
                    {
                        if (dep.BundleID == bundleID) break;

                        if (!AnvilManager.m_bundles.m_lookup.ContainsKey(dep.BundleID))
                        {
                            depList.Add(dep);
                        }
                    }
                }

                else if (bundleStatus.LoadOrderType == LoadOrderType.LoadUnordered)
                {
                    foreach (BundleInfo dep in loadFirst)
                    {
                        if (!AnvilManager.m_bundles.m_lookup.ContainsKey(dep.BundleID))
                        {
                            depList.Add(dep);
                        }
                    }
                }

                else if (bundleStatus.LoadOrderType == LoadOrderType.LoadLast)
                {
                    foreach (BundleInfo dep in loadFirst)
                    {
                        if (!AnvilManager.m_bundles.m_lookup.ContainsKey(dep.BundleID))
                        {
                            depList.Add(dep);
                        }
                    }

                    foreach (BundleInfo dep in loadUnordered)
                    {
                        if (!AnvilManager.m_bundles.m_lookup.ContainsKey(dep.BundleID))
                        {
                            depList.Add(dep);
                        }
                    }

                    foreach (BundleInfo dep in loadLast)
                    {
                        if (dep.BundleID == bundleID) break;

                        if (!AnvilManager.m_bundles.m_lookup.ContainsKey(dep.BundleID))
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
