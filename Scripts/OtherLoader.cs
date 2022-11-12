
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx;
using HarmonyLib;
using BepInEx.Logging;
using FistVR;
using BepInEx.Configuration;
using Stratum;
using System.Collections;
using Anvil;
using RenderHeads.Media.AVProVideo;
using OtherLoader.Patches;
using Valve.Newtonsoft.Json;

namespace OtherLoader
{
    [BepInPlugin("h3vr.otherloader", "OtherLoader", "1.3.7")]
    [BepInDependency(StratumRoot.GUID, StratumRoot.Version)]
    [BepInDependency(Sodalite.SodaliteConstants.Guid, Sodalite.SodaliteConstants.Version)]
    public class OtherLoader : StratumPlugin
    {
        public static string MainLegacyDirectory;
        public static string OtherLoaderSaveDirectory;
        public static string UnlockedItemSaveDataPath;

        // A dictionary of asset bundles managed by OtherLoader. The key is the UniqueAssetID, and the value is the path to that file
        public static Dictionary<string, string> ManagedBundles = new Dictionary<string, string>();

        public static Dictionary<string, EntryNode> SpawnerEntriesByPath = new Dictionary<string, EntryNode>();
        public static Dictionary<string, ItemSpawnerEntry> SpawnerEntriesByID = new Dictionary<string, ItemSpawnerEntry>();
        public static Dictionary<string, MediaPath> TutorialBlockVideos = new Dictionary<string, MediaPath>();
        public static Dictionary<string, ItemSpawnerID> SpawnerIDsByMainObject = new Dictionary<string, ItemSpawnerID>();
        public static UnlockedItemSaveData UnlockSaveData;
        public static Sprite LockIcon;

        private static ConfigEntry<int> MaxActiveLoadersConfig;
        public static ConfigEntry<bool> OptimizeMemory;
        public static ConfigEntry<bool> EnableLogging;
        public static ConfigEntry<bool> LogLoading;
        public static ConfigEntry<bool> AddUnloadButton;
        public static ConfigEntry<ItemUnlockMode> UnlockMode;

        public static int MaxActiveLoaders = 0;

        private static List<DirectLoadMod> directLoadMods = new List<DirectLoadMod>();

        public static CoroutineStarter coroutineStarter;

        private void Awake()
        {
            LoadConfigFile();
            OtherLogger.Init(EnableLogging.Value, LogLoading.Value);

            InitPaths();
            InitUnlockSaveData();

            Harmony.CreateAndPatchAll(typeof(OtherLoader));
            Harmony.CreateAndPatchAll(typeof(ItemSpawnerPatch));
            Harmony.CreateAndPatchAll(typeof(ItemSpawnerV2Patch));
            Harmony.CreateAndPatchAll(typeof(QuickbeltPanelPatch));
            Harmony.CreateAndPatchAll(typeof(DetailsPanelPatches));
            Harmony.CreateAndPatchAll(typeof(ItemSpawningPatches));
            Harmony.CreateAndPatchAll(typeof(SimpleCanvasePatches));

            if (AddUnloadButton.Value)
            {
                AddUnloadWristMenuButton();
            }

            coroutineStarter = StartCoroutine;
        }

        private void InitPaths()
        {
            MainLegacyDirectory = Application.dataPath.Replace("/h3vr_Data", "/LegacyVirtualObjects");
            OtherLoaderSaveDirectory = Application.dataPath.Replace("/h3vr_Data", "/OtherLoader");
            UnlockedItemSaveDataPath = Path.Combine(OtherLoaderSaveDirectory, "UnlockedItems.json");
        }

        private void Start()
        {
            GM.SetRunningModded();
        }

        private void LoadConfigFile()
        {
            
            OptimizeMemory = Config.Bind(
                "General",
                "OptimizeMemory",
                false,
                "When enabled, modded assets will be loaded on-demand instead of kept in memory. Can cause small hiccups when spawning modded guns for the first time. Useful if you are low on RAM"
                );
            

            EnableLogging = Config.Bind(
                "Logging",
                "EnableLogging",
                true,
                "When enabled, OtherLoader will log more than just errors and warning to the output log"
                );

            LogLoading = Config.Bind(
                "Logging",
                "LogLoading",
                false,
                "When enabled, OtherLoader will log additional useful information during the loading process. EnableLogging must be set to true for this to have an effect"
                );

            MaxActiveLoadersConfig = Config.Bind(
                "General",
                "MaxActiveLoaders",
                6,
                "Sets the number of mods that can be loading at once. Values less than 1 will result in all mods being loaded at the same time"
                );

            AddUnloadButton = Config.Bind(
                "Debug",
                "AddUnloadButton",
                false,
                "When true, and sodalite is installed, you'll have a wristmenu button that unloads all modded asset bundles for testing"
                );

            UnlockMode = Config.Bind(
                "General",
                "UnlockMode",
                ItemUnlockMode.Normal,
                "When set to Unlockathon, all items will start out locked, and you must unlock items by finding them in game"
                );


            MaxActiveLoaders = MaxActiveLoadersConfig.Value;
        }


        private void InitUnlockSaveData()
        {
            if (!Directory.Exists(OtherLoaderSaveDirectory))
            {
                Directory.CreateDirectory(OtherLoaderSaveDirectory);
            }

            if (!File.Exists(UnlockedItemSaveDataPath))
            {
                UnlockSaveData = new UnlockedItemSaveData(UnlockMode.Value);

                SaveUnlockedItemsData();
            }

            else
            {
                LoadUnlockedItemsData();
            }
        }


        public static void LoadUnlockedItemsData()
        {
            try
            {
                string unlockJson = File.ReadAllText(UnlockedItemSaveDataPath);
                UnlockSaveData = JsonConvert.DeserializeObject<UnlockedItemSaveData>(unlockJson);

                if(UnlockSaveData.UnlockMode != UnlockMode.Value)
                {
                    UnlockSaveData = new UnlockedItemSaveData(UnlockMode.Value);
                }
            } 
            catch (Exception ex)
            {
                OtherLogger.LogError("Exception when loading unlocked items!\n" + ex.ToString());

                OtherLogger.LogError("Attempting to create new unlock file");
                File.Delete(UnlockedItemSaveDataPath);
                UnlockSaveData = new UnlockedItemSaveData(UnlockMode.Value);
                SaveUnlockedItemsData();
            }
        }


        public static void SaveUnlockedItemsData()
        {
            try
            {
                string unlockJson = JsonConvert.SerializeObject(UnlockSaveData, Formatting.Indented);
                File.WriteAllText(UnlockedItemSaveDataPath, unlockJson);
            }
            catch (Exception ex)
            {
                OtherLogger.LogError("Exception when saving unlocked items!\n" + ex.ToString());
            }
        }


        public override void OnSetup(IStageContext<Empty> ctx)
        {
            ItemLoader loader = new ItemLoader();

            //Setup Loaders
            ctx.Loaders.Add("assembly", loader.LoadAssembly);
            ctx.Loaders.Add("icon", loader.LoadLockIcon);
        }

        public override IEnumerator OnRuntime(IStageContext<IEnumerator> ctx)
        {
            ItemLoader loader = new ItemLoader();
            
            //On-Demand Loaders
            ctx.Loaders.Add("item_data", loader.StartAssetDataLoad);
            ctx.Loaders.Add("item_first_late", loader.RegisterAssetLoadFirstLate);
            ctx.Loaders.Add("item_unordered_late", loader.RegisterAssetLoadUnorderedLate);
            ctx.Loaders.Add("item_last_late", loader.RegisterAssetLoadLastLate);

            //Immediate Loaders
            ctx.Loaders.Add("item", loader.StartAssetLoadFirst);
            ctx.Loaders.Add("item_unordered", loader.StartAssetLoadUnordered);
            ctx.Loaders.Add("item_last", loader.StartAssetLoadLast);
            
            loader.LoadLegacyAssets(coroutineStarter);

            foreach(DirectLoadMod mod in directLoadMods)
            {
                loader.LoadDirectAssets(coroutineStarter, mod.path, mod.guid, mod.dependancies.Split(','), mod.loadFirst.Split(','), mod.loadAny.Split(','), mod.loadLast.Split(','));
            }

            yield break;
        }


        
        public static void RegisterDirectLoad(string path, string guid, string dependancies, string loadFirst, string loadAny, string loadLast)
        {
            directLoadMods.Add(new DirectLoadMod()
            {
                path = path,
                guid = guid,
                dependancies = dependancies,
                loadFirst = loadFirst,
                loadAny = loadAny,
                loadLast = loadLast
            });
        }


        private void AddUnloadWristMenuButton()
        {
            Sodalite.Api.WristMenuAPI.Buttons.Add(new Sodalite.Api.WristMenuButton("Unload Bundles", UnloadAllModdedBundlesButton));
        }


        private void UnloadAllModdedBundlesButton(object sender, Sodalite.ButtonClickEventArgs args){
            UnloadAllModdedBundles();
        }


        private void UnloadAllModdedBundles()
        {
            foreach(string bundleID in ManagedBundles.Keys)
            {
                if (!AnvilManager.m_bundles.m_lookup.ContainsKey(bundleID)) continue;

                OtherLogger.Log("Unloading bundle: " + bundleID, OtherLogger.LogType.General);

                //Get the bundle container
                AnvilCallback<AssetBundle> bundleCallback = (AnvilCallback<AssetBundle>)AnvilManager.m_bundles.m_lookup[bundleID];

                //Remove Instances of this bundle from the anvil manager
                AnvilManager.m_bundles.m_loading.Remove(AnvilManager.m_bundles.m_lookup[bundleID]);
                AnvilManager.m_bundles.m_lookup.Remove(bundleID);

                //Unload the bundle
                bundleCallback.Result.Unload(false);
            }
        }

        public static bool DoesEntryHaveChildren(ItemSpawnerEntry entry)
        {
            return SpawnerEntriesByPath[entry.EntryPath].childNodes.Count > 0;
        }




        [HarmonyPatch(typeof(AnvilManager), "GetAssetBundleAsyncInternal")]
        [HarmonyPrefix]
        private static bool LoadModdedBundlesPatch(string bundle, ref AnvilCallback<AssetBundle> __result)
        {
            if (ManagedBundles.ContainsKey(bundle))
            {
                //If this is a modded bundle, we should first check if the bundle is already loaded
                AnvilCallbackBase anvilCallbackBase;
                if (AnvilManager.m_bundles.TryGetValue(bundle, out anvilCallbackBase))
                {
                    OtherLogger.Log("Tried to load modded asset bundle, and it's already loaded : " + bundle, OtherLogger.LogType.Loading);
                    __result = anvilCallbackBase as AnvilCallback<AssetBundle>;
                    return false;
                }

                //If the bundle is not already loaded, then load it
                else
                {
                    OtherLogger.Log("Tried to load modded asset bundle, and it's not yet loaded : " + bundle, OtherLogger.LogType.Loading);
                    
                    AnvilCallback<AssetBundle> mainCallback = LoaderUtils.LoadAssetBundle(ManagedBundles[bundle]);
                    List<BundleInfo> dependencies = LoaderStatus.GetBundleDependencies(bundle);

                    if (dependencies.Count > 0)
                    {
                        OtherLogger.Log("Dependencies:", OtherLogger.LogType.Loading);
                        dependencies.ForEach(o => OtherLogger.Log(ManagedBundles[o.BundleID], OtherLogger.LogType.Loading));

                        //Start with the last dependency, and loop through from second to last dep up to the first dep
                        //The first dep in the list is the dependency for all other dependencies, so it is the deepest
                        AnvilCallback<AssetBundle> dependency = LoaderUtils.LoadAssetBundle(ManagedBundles[dependencies.Last().BundleID]);
                        mainCallback.m_dependancy = dependency;
                        AnvilManager.m_bundles.Add(dependencies.Last().BundleID, dependency);

                        for (int i = dependencies.Count - 2; i >= 0; i--)
                        {
                            dependency.m_dependancy = LoaderUtils.LoadAssetBundle(ManagedBundles[dependencies[i].BundleID]);
                            dependency = dependency.m_dependancy;
                            AnvilManager.m_bundles.Add(dependencies[i].BundleID, dependency);
                        }
                    }

                    __result = mainCallback;
                    AnvilManager.m_bundles.Add(bundle, __result);
                    return false;
                }
            }

            return true;
        }


        //This patch courtesy of Potatoes
        [HarmonyPatch(typeof(AnvilAsset), "GetGameObjectAsync")]
        [HarmonyPatch(typeof(AnvilAsset), "GetGameObject")]
        [HarmonyPrefix]
        public static bool SetBundlePatch(AnvilAsset __instance)
        {
            if (string.IsNullOrEmpty(__instance.m_anvilPrefab.Bundle))
            {
                var fvro = __instance as FVRObject;
                if (fvro != null)
                {
                    FVRObject thisObject;
                    if (IM.OD.TryGetValue(fvro.ItemID, out thisObject))
                    {
                        __instance.m_anvilPrefab.Bundle = thisObject.m_anvilPrefab.Bundle;
                    }
                }
            }
            return true;
        }


        [HarmonyPatch(typeof(FVRPhysicalObject), "BeginInteraction")]
        [HarmonyPrefix]
        public static bool UnlockInteractedItem(FVRPhysicalObject __instance)
        {
            if(__instance.ObjectWrapper != null)
            {
                if (UnlockSaveData.UnlockItem(__instance.ObjectWrapper.ItemID))
                {
                    SaveUnlockedItemsData();
                }
            }

            return true;
        }


        [HarmonyPatch(typeof(IM), "RegisterItemIntoMetaTagSystem")]
        [HarmonyPostfix] 
        private static void MetaTagPatch(ItemSpawnerID ID)
        {
            //If this item is not a reward, unlock it by default
            if (ID.MainObject != null)
            {
                SpawnerIDsByMainObject[ID.MainObject.ItemID] = ID;

                if (UnlockSaveData.ShouldAutoUnlockItem(ID))
                {
                    UnlockSaveData.UnlockItem(ID.MainObject.ItemID);
                }
            }

            //If this IDs items didn't get added, add it to the firearm page
            if (IM.Instance.PageItemLists.ContainsKey(ItemSpawnerV2.PageMode.Firearms))
            {
                if (!IM.Instance.PageItemLists.Any(o => o.Value.Contains(ID.ItemID)) && IM.OD.ContainsKey(ID.MainObject.ItemID) && IM.OD[ID.MainObject.ItemID].IsModContent)
                {
                    OtherLogger.Log("Adding misc mod item to meta tag system: " + ID.ItemID, OtherLogger.LogType.Loading);

                    IM.AddMetaTag(ID.Category.ToString(), TagType.Category, ID.ItemID, ItemSpawnerV2.PageMode.Firearms);
                    IM.AddMetaTag(ID.SubCategory.ToString(), TagType.SubCategory, ID.ItemID, ItemSpawnerV2.PageMode.Firearms);
                }
            }
        }


        private class DirectLoadMod
        {
            public string path;
            public string guid;
            public string dependancies;
            public string loadFirst;
            public string loadAny;
            public string loadLast;
        }

    }


    
}
