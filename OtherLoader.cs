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


        /*
        [HarmonyPatch(typeof(TNH_UIManager), "Start")] // Specify target method with HarmonyPatch attribute
        [HarmonyPrefix]
        private static void PrintPrimeCats(AmmoSpawnerV2 __instance)
        {
            OtherLogger.LogInfo("Priming Ammo Spawner Categories");

            for (int i = 0; i < 10; i++)
            {
                AmmoSpawnerV2.CartridgeCategory item = new AmmoSpawnerV2.CartridgeCategory();
                __instance.Categories.Add(item);
            }
            for (int j = 0; j < AM.STypeList.Count; j++)
            {
                FVRFireArmRoundDisplayData fvrfireArmRoundDisplayData = AM.SRoundDisplayDataDic[AM.STypeList[j]];
                OtherLogger.LogInfo("Ammo Type: " + AM.STypeList[j]);
                OtherLogger.LogInfo("Display Name: " + fvrfireArmRoundDisplayData.DisplayName);

                int num;
                if (fvrfireArmRoundDisplayData.IsMeatFortress)
                {
                    num = 8;
                }
                else
                {
                    num = fvrfireArmRoundDisplayData.RoundPower - FVRObject.OTagFirearmRoundPower.Tiny;
                }

                OtherLogger.LogInfo("Category Number: " + num);

                __instance.Categories[num].Entries.Add(fvrfireArmRoundDisplayData);
                __instance.reverseCatDic.Add(fvrfireArmRoundDisplayData.Type, num);
            }
        }
        */


    }
}
