
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
using OtherLoader.Core.Controllers;
using OtherLoader.Core.Features.DependancyInjection;
using OtherLoader.Core.Adapters;
using OtherLoader.Features.AssetLoading.Adapters;
using OtherLoader.Features.AssetLoading.Subscribers;

namespace OtherLoader
{
    [BepInPlugin("h3vr.otherloader", "OtherLoader", "2.0.0")]
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
        
        private static List<DirectLoadModData> directLoadMods = new List<DirectLoadModData>();
        private ServiceContainer _serviceContainer;
        private CoroutineStarter _coroutineStarter;

        public static int MaxActiveLoaders = 0;

        
        
        private void Awake()
        {
            Configuration = OtherLoaderConfig.LoadFromFile(this);
            OtherLogger.Init(Configuration);
            CreateHarmonyPatches();
            _coroutineStarter = StartCoroutine;
            _serviceContainer = ConstructServiceContainer();
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
            StartDirectModLoading();
            RegisterStratumModLoading(ctx);
            
            yield break;
        }

        private ServiceContainer ConstructServiceContainer()
        {
            var container = new ServiceContainer();

            container.Bind<ILoadOrderController, LoadOrderController>();
            container.Bind<IBundleLoadingAdapter, BundleLoadingAdapter>();
            container.Bind<IAssetLoadingService, AssetLoadingService>();
            
            container.Bind<TutorialBlockSubscriber>();
            container.Bind<ItemSpawnerIdSubscriber>();

            return container;
        }

        private void RegisterStratumModLoading(IStageContext<IEnumerator> ctx)
        {
            var assetLoadingService = _serviceContainer.Resolve<IAssetLoadingService>();

            ctx.Loaders.Add("item_data", assetLoadingService.StartAssetLoadData);
            ctx.Loaders.Add("item_first_late", assetLoadingService.RegisterAssetLoadFirstLate);
            ctx.Loaders.Add("item_unordered_late", assetLoadingService.RegisterAssetLoadUnorderedLate);
            ctx.Loaders.Add("item_last_late", assetLoadingService.RegisterAssetLoadLastLate);
            ctx.Loaders.Add("item", assetLoadingService.StartAssetLoadFirst);
            ctx.Loaders.Add("item_unordered", assetLoadingService.StartAssetLoadUnordered);
            ctx.Loaders.Add("item_last", assetLoadingService.StartAssetLoadLast);

        }

        private void StartDirectModLoading()
        {
            var assetLoadingService = _serviceContainer.Resolve<IAssetLoadingService>();

            foreach (DirectLoadModData mod in directLoadMods)
            {
                var assetLoadRoutines = assetLoadingService.LoadDirectAssets(mod);

                foreach (var routine in assetLoadRoutines)
                {
                    _coroutineStarter(routine);
                }
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
