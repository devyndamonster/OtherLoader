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

namespace OtherLoader
{
    public class OtherLoader : DeliBehaviour
    {

        public static ManualLogSource OtherLogger;
        public static List<AssetBundle> LoadedBundles = new List<AssetBundle>();

        private void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(OtherLoader));

            OtherLogger = BepInEx.Logging.Logger.CreateLogSource("OtherLoader");

            Stages.Runtime += DuringRuntime;
        }

        private void DuringRuntime(RuntimeStage stage)
        {
            stage.RuntimeAssetLoaders[Source, "item"] = new ItemLoader().LoadAsset;
        }

    }
}
