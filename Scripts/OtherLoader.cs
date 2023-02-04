
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
    [BepInPlugin("h3vr.otherloader", "OtherLoader", "1.3.8")]
    [BepInDependency(StratumRoot.GUID, StratumRoot.Version)]
    [BepInDependency(Sodalite.SodaliteConstants.Guid, Sodalite.SodaliteConstants.Version)]
    public class OtherLoader : StratumPlugin
    {
        // A dictionary of asset bundles managed by OtherLoader. The key is the UniqueAssetID, and the value is the path to that file
        public static Dictionary<string, string> ManagedBundles = new();

        public static Dictionary<string, EntryNode> SpawnerEntriesByPath = new();
        public static Dictionary<string, ItemSpawnerEntry> SpawnerEntriesByID = new();
        public static Dictionary<string, MediaPath> TutorialBlockVideos = new();
        public static Dictionary<string, ItemSpawnerID> SpawnerIDsByMainObject = new();
        public static Dictionary<string, ItemSpawnerCategoryDefinitionsV2.SpawnerPage.SpawnerTagGroup> TagGroupsByTag = new();
        public static UnlockedItemSaveData UnlockSaveData;
        public static Sprite LockIcon;

        private static ConfigEntry<int> MaxActiveLoadersConfig;
        public static ConfigEntry<bool> OptimizeMemory;
        public static ConfigEntry<bool> EnableLogging;
        public static ConfigEntry<bool> LogLoading;
        public static ConfigEntry<bool> LogItemSpawner;
        public static ConfigEntry<bool> LogMetaTagging;
        public static ConfigEntry<bool> AddUnloadButton;
        public static ConfigEntry<ItemUnlockMode> UnlockMode;

        public static int MaxActiveLoaders = 0;

        private static List<DirectLoadMod> directLoadMods = new List<DirectLoadMod>();

        public static CoroutineStarter coroutineStarter;

        private void Awake()
        {
            LoadConfigFile();
            OtherLogger.Init(EnableLogging.Value, LogLoading.Value, LogItemSpawner.Value, LogMetaTagging.Value);
            
            Harmony.CreateAndPatchAll(typeof(OtherLoader));
            Harmony.CreateAndPatchAll(typeof(ItemSpawnerPatch));
            Harmony.CreateAndPatchAll(typeof(ItemSpawnerV2Patch));
            Harmony.CreateAndPatchAll(typeof(QuickbeltPanelPatch));
            Harmony.CreateAndPatchAll(typeof(DetailsPanelPatches));
            Harmony.CreateAndPatchAll(typeof(ItemSpawningPatches));
            Harmony.CreateAndPatchAll(typeof(SimpleCanvasePatches));
            Harmony.CreateAndPatchAll(typeof(ItemDataLoadingPatches));
            
            coroutineStarter = StartCoroutine;
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

            LogItemSpawner = Config.Bind(
                "Logging",
                "LogItemSpawner",
                false,
                "When enabled, OtherLoader will log additional useful information about the item spawner. EnableLogging must be set to true for this to have an effect"
                );

            LogMetaTagging = Config.Bind(
                "Logging",
                "LogMetaTagging",
                false,
                "When enabled, OtherLoader will log additional useful information about metadata. EnableLogging must be set to true for this to have an effect"
                );

            MaxActiveLoadersConfig = Config.Bind(
                "General",
                "MaxActiveLoaders",
                6,
                "Sets the number of mods that can be loading at once. Values less than 1 will result in all mods being loaded at the same time"
                );

            UnlockMode = Config.Bind(
                "General",
                "UnlockMode",
                ItemUnlockMode.Normal,
                "When set to Unlockathon, all items will start out locked, and you must unlock items by finding them in game"
                );


            MaxActiveLoaders = MaxActiveLoadersConfig.Value;
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
        
        public static bool DoesEntryHaveChildren(ItemSpawnerEntry entry)
        {
            return SpawnerEntriesByPath[entry.EntryPath].childNodes.Count > 0;
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
