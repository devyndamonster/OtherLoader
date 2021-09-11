﻿using Deli.Setup;
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
using Deli.Runtime;
using Deli.VFS;
using Deli.Immediate;
using BepInEx.Configuration;

namespace OtherLoader
{
    public class OtherLoader : DeliBehaviour
    {
        public static string MainLegacyDirectory { get; } = Application.dataPath.Replace("/h3vr_Data", "/LegacyVirtualObjects");

        public static Dictionary<string, IFileHandle> BundleFiles = new Dictionary<string, IFileHandle>();
        public static Dictionary<string, string> LegacyBundles = new Dictionary<string, string>();
        private static ConfigEntry<int> MaxActiveLoadersConfig;
        //public static ConfigEntry<bool> OptimizeMemory;
        public static ConfigEntry<bool> EnableLogging;
        public static ConfigEntry<bool> LogLoading;

        public static int MaxActiveLoaders = 0;

        private void Awake()
        {
            LoadConfigFile();

            //CacheManager.Init();

            Harmony.CreateAndPatchAll(typeof(OtherLoader));
            Harmony.CreateAndPatchAll(typeof(ItemSpawnerPatch));
            Harmony.CreateAndPatchAll(typeof(QuickbeltPanelPatch));

            OtherLogger.Init(EnableLogging.Value, LogLoading.Value);

            Stages.Runtime += DuringRuntime;
        }

        private void LoadConfigFile()
        {
            /*
            OptimizeMemory = Source.Config.Bind(
                "General",
                "OptimizeMemory",
                false,
                "When enabled, modded assets will be loaded on-demand instead of kept in memory. Can cause small hiccups when spawning modded guns for the first time. Useful if you are low on RAM"
                );
            */

            EnableLogging = Source.Config.Bind(
                "Logging",
                "EnableLogging",
                true,
                "When enabled, OtherLoader will log more than just errors and warning to the output log"
                );

            LogLoading = Source.Config.Bind(
                "Logging",
                "LogLoading",
                false,
                "When enabled, OtherLoader will log additional useful information during the loading process. EnableLogging must be set to true for this to have an effect"
                );

            MaxActiveLoadersConfig = Source.Config.Bind(
                "General",
                "MaxActiveLoaders",
                6,
                "Sets the number of mods that can be loading at once. Values less than 1 will result in all mods being loaded at the same time"
                );

            MaxActiveLoaders = MaxActiveLoadersConfig.Value;
        }

        private void DuringRuntime(RuntimeStage stage)
        {
            LoaderUtils.ImmediateByteReader = stage.ImmediateReaders.Get<byte[]>();
            LoaderUtils.DelayedByteReader = stage.DelayedReaders.Get<byte[]>();

            ItemLoader loader = new ItemLoader();
            stage.RuntimeAssetLoaders[Source, "item"] = loader.StartAssetLoadFirst;
            stage.RuntimeAssetLoaders[Source, "item_last"] = loader.StartAssetLoadLast;
            stage.RuntimeAssetLoaders[Source, "item_unordered"] = loader.StartAssetLoadUnordered;
            loader.LoadLegacyAssets();
        }


        [HarmonyPatch(typeof(AnvilManager), "GetAssetBundleAsyncInternal")]
        [HarmonyPrefix]
        private static bool LoadModdedBundles(string bundle, ref AnvilCallback<AssetBundle> __result)
        {
            if (BundleFiles.ContainsKey(bundle))
            {
                //If this is a modded bundle, we should first check if the bundle is already loaded
                AnvilCallbackBase anvilCallbackBase;
                if (AnvilManager.m_bundles.TryGetValue(bundle, out anvilCallbackBase))
                {
                    __result = anvilCallbackBase as AnvilCallback<AssetBundle>;
                    return false;
                }

                //If the bundle is not already loaded, then load it
                else
                {
                    __result = LoaderUtils.LoadAssetBundleFromFile(BundleFiles[bundle]);
                    AnvilManager.m_bundles.Add(bundle, __result);
                    return false;
                }
            }

            else if (LegacyBundles.ContainsKey(bundle))
            {
                //If this is a modded bundle, we should first check if the bundle is already loaded
                AnvilCallbackBase anvilCallbackBase;
                if (AnvilManager.m_bundles.TryGetValue(bundle, out anvilCallbackBase))
                {
                    __result = anvilCallbackBase as AnvilCallback<AssetBundle>;
                    return false;
                }

                //If the bundle is not already loaded, then load it
                else
                {
                    __result = LoaderUtils.LoadAssetBundleFromPath(LegacyBundles[bundle]);
                    AnvilManager.m_bundles.Add(bundle, __result);
                    return false;
                }
            }

            return true;
        }






    }


    
}
