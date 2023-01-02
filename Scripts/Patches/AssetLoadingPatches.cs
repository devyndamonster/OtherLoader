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
    public static class AssetLoadingPatches
    {
        [HarmonyPatch(typeof(AnvilManager), "GetAssetBundleAsyncInternal")]
        [HarmonyPrefix]
        private static bool LoadModdedBundlesPatch(string bundle, ref AnvilCallback<AssetBundle> __result)
        {
            if (OtherLoader.ManagedBundles.ContainsKey(bundle))
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

                    AnvilCallback<AssetBundle> mainCallback = LoaderUtils.LoadAssetBundle(OtherLoader.ManagedBundles[bundle]);
                    List<BundleInfo> dependencies = LoaderStatus.GetBundleDependencies(bundle);

                    if (dependencies.Count > 0)
                    {
                        OtherLogger.Log("Dependencies:", OtherLogger.LogType.Loading);
                        dependencies.ForEach(o => OtherLogger.Log(OtherLoader.ManagedBundles[o.BundleID], OtherLogger.LogType.Loading));

                        //Start with the last dependency, and loop through from second to last dep up to the first dep
                        //The first dep in the list is the dependency for all other dependencies, so it is the deepest
                        AnvilCallback<AssetBundle> dependency = LoaderUtils.LoadAssetBundle(OtherLoader.ManagedBundles[dependencies.Last().BundleID]);
                        mainCallback.m_dependancy = dependency;
                        AnvilManager.m_bundles.Add(dependencies.Last().BundleID, dependency);

                        for (int i = dependencies.Count - 2; i >= 0; i--)
                        {
                            dependency.m_dependancy = LoaderUtils.LoadAssetBundle(OtherLoader.ManagedBundles[dependencies[i].BundleID]);
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
    }
}
