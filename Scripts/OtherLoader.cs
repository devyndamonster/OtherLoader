
using UnityEngine;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using FistVR;
using Stratum;
using System.Collections;
using RenderHeads.Media.AVProVideo;
using OtherLoader.Patches;
using OtherLoader.Core.Models;
using OtherLoader.Core.Services;

namespace OtherLoader
{
    [BepInPlugin("h3vr.otherloader", "OtherLoader", "1.3.8")]
    [BepInDependency(StratumRoot.GUID, StratumRoot.Version)]
    [BepInDependency(Sodalite.SodaliteConstants.Guid, Sodalite.SodaliteConstants.Version)]
    public class OtherLoader : StratumPlugin
    {
        // A dictionary of asset bundles managed by OtherLoader. The key is the UniqueAssetID, and the value is the path to that file
        public static Dictionary<string, string> ManagedBundles = new();
        
        public static OtherLoaderConfig Configuration;

        public static Dictionary<string, EntryNode> SpawnerEntriesByPath = new();
        public static Dictionary<string, ItemSpawnerEntry> SpawnerEntriesByID = new();
        public static Dictionary<string, MediaPath> TutorialBlockVideos = new();
        public static Dictionary<string, ItemSpawnerID> SpawnerIDsByMainObject = new();
        public static Dictionary<string, ItemSpawnerCategoryDefinitionsV2.SpawnerPage.SpawnerTagGroup> TagGroupsByTag = new();
        public static UnlockedItemSaveData UnlockSaveData;
        public static Sprite LockIcon;
        
        public static int MaxActiveLoaders = 0;

        private static List<DirectLoadModData> directLoadMods = new List<DirectLoadModData>();

        public static CoroutineStarter coroutineStarter;

        private void Awake()
        {
            Configuration = OtherLoaderConfig.LoadFromFile(this);
            OtherLogger.Init(Configuration);
            CreateHarmonyPatches();
            coroutineStarter = StartCoroutine;

            //TODO: Setup services here

            
        }

        private void Start()
        {
            GM.SetRunningModded();
        }

        private void CreateHarmonyPatches()
        {
            Harmony.CreateAndPatchAll(typeof(OtherLoader));
            Harmony.CreateAndPatchAll(typeof(ItemSpawnerPatch));
            Harmony.CreateAndPatchAll(typeof(ItemSpawnerV2Patch));
            Harmony.CreateAndPatchAll(typeof(QuickbeltPanelPatch));
            Harmony.CreateAndPatchAll(typeof(DetailsPanelPatches));
            Harmony.CreateAndPatchAll(typeof(ItemSpawningPatches));
            Harmony.CreateAndPatchAll(typeof(SimpleCanvasePatches));
            Harmony.CreateAndPatchAll(typeof(ItemDataLoadingPatches));
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

            foreach(DirectLoadModData mod in directLoadMods)
            {
                //loader.LoadDirectAssets(coroutineStarter, mod.path, mod.guid, mod.dependancies.Split(','), mod.loadFirst.Split(','), mod.loadAny.Split(','), mod.loadLast.Split(','));
            }

            yield break;
        }

        public static void TestLoadContainerSetup_DeleteLater()
        {
            var assetLoadingService = new AssetLoadingService(null, null);

            BaseAssetLoadingSubscriber<TutorialBlock> tutorialSubscriber = null;

            assetLoadingService.OnAssetLoadComplete += tutorialSubscriber.LoadSubscribedAssets;

            foreach (DirectLoadModData mod in directLoadMods)
            {
                assetLoadingService.LoadDirectAssets(mod);
            }
        }
        
        public static void RegisterDirectLoad(string path, string guid, string dependancies, string loadFirst, string loadAny, string loadLast)
        {
            directLoadMods.Add(new DirectLoadModData()
            {
                FolderPath = path,
                Guid = guid,
                Dependancies = dependancies.Split(','),
                LoadFirst = loadFirst.Split(','),
                LoadAny = loadAny.Split(','),
                LoadLast = loadLast.Split(',')
            });
        }
        
        public static bool DoesEntryHaveChildren(ItemSpawnerEntry entry)
        {
            return SpawnerEntriesByPath[entry.EntryPath].childNodes.Count > 0;
        }
    }
}
