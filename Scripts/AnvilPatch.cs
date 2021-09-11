using System;
using Anvil;
using FistVR;
using HarmonyLib;
using UnityEngine;

namespace OtherLoader
{
	public class AnvilPatch
	{

		[HarmonyPatch(typeof(AnvilAsset), "GetGameObjectAsync")]
		[HarmonyPatch(typeof(AnvilAsset), "GetGameObject")]
		[HarmonyPrefix]
		public static bool SetBundle(AnvilAsset __instance)
		{
			if (String.IsNullOrEmpty(__instance.m_anvilPrefab.Bundle))
			{
				var fvro = __instance as FVRObject;
				if (fvro != null)
				{
					FVRObject thisObject;
					if(IM.OD.TryGetValue(fvro.ItemID, out thisObject))
					{
						//Debug.Log("Fixing empty bundle with " + thisObject.m_anvilPrefab.Bundle);
						__instance.m_anvilPrefab.Bundle = thisObject.m_anvilPrefab.Bundle;
					}
				}
			}

			return true;
		}
		//all of this is just debugging crap
		 
		[HarmonyPatch(typeof(AnvilAsset), "GetGameObjectAsync")]
		[HarmonyPrefix]
		public static bool AnvilAssetPatch(AnvilAsset __instance)
		{
			Debug.Log("hmn yes today i will get gameobject async " + __instance.m_anvilPrefab.AssetName + " from bundle " + __instance.m_anvilPrefab.Bundle);
			return true;
		}
		
		[HarmonyPatch(typeof(AnvilAsset), "GetGameObject")]
		[HarmonyPrefix]
		public static bool AnvilAssetPatch2(AnvilAsset __instance)
		{
			Debug.Log("hmn yes today i will get gameobject " + __instance.m_anvilPrefab.AssetName + " from bundle " + __instance.m_anvilPrefab.Bundle);
			return true;
		}
		
		[HarmonyPatch(typeof(AnvilManager), "GetAssetBundleAsyncInternal")]
		[HarmonyPrefix]
		public static bool AnvilManagerPatch1(ref string bundle)
		{
			Debug.Log("hmn yes today i will get bundle " + bundle);
			return true;
		}
		
		[HarmonyPatch(typeof(AnvilManager), "GetCallbackRequest")]
		[HarmonyPrefix]
		public static bool AnvilManagerPatch2(ref AssetID assetID, ref AssetBundle bundle)
		{
			Debug.Log("hmn yes today i will get " + assetID.AssetName + " from bundle " + bundle);
			return true;
		}
	}
}