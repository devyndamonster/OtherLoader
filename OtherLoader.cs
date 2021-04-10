using Deli.Setup;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using BepInEx.Logging;
using FistVR;
using Deli.Runtime;
using Deli.VFS;
using Deli.Immediate;

namespace OtherLoader
{
    public class OtherLoader : DeliBehaviour
    {

        public static ManualLogSource OtherLogger;
        public static Dictionary<string, IFileHandle> BundleFiles = new Dictionary<string, IFileHandle>();
        

        private void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(OtherLoader));

            OtherLogger = BepInEx.Logging.Logger.CreateLogSource("OtherLoader");

            Stages.Runtime += DuringRuntime;
        }

        private void DuringRuntime(RuntimeStage stage)
        {
            LoaderUtils.DelayedByteReader = stage.DelayedReaders.Get<byte[]>();
            LoaderUtils.ImmediateByteReader = stage.ImmediateReaders.Get<byte[]>();
            stage.RuntimeAssetLoaders[Source, "item"] = new ItemLoader().LoadAsset;
        }

        [HarmonyPatch(typeof(AnvilManager), "GetAssetBundleAsyncInternal")]
        [HarmonyPrefix]
        private static bool LoadModdedBundles(string bundle, ref AnvilCallback<AssetBundle> __result)
        {
            if (BundleFiles.ContainsKey(bundle))
            {
                __result = LoaderUtils.LoadAssetBundleFromFile(BundleFiles[bundle]);
                AnvilManager.m_bundles.Add(bundle, __result);
                return false;
            }

            return true;
        }

    }


    
}
