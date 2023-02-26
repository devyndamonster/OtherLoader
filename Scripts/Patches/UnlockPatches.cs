using FistVR;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using OtherLoader.Loaders;
using OtherLoader.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace OtherLoader.Patches
{
    public static class UnlockPatches
    {
        [HarmonyPatch(typeof(FVRPhysicalObject), "BeginInteraction")]
        [HarmonyPrefix]
        public static bool UnlockInteractedItem(FVRPhysicalObject __instance)
        {
            if (__instance.ObjectWrapper != null)
            {
                if (OtherLoader.UnlockSaveData.UnlockItem(__instance.ObjectWrapper.ItemID))
                {
                    //OtherLoader.SaveUnlockedItemsData();
                }
            }

            return true;
        }
    }
}
