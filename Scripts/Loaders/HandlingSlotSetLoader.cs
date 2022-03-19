using FistVR;
using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace OtherLoader.Loaders
{
    public class HandlingSlotSetLoader : BaseAssetLoader
    {
        public override IEnumerator LoadAssetsFromBundle(AssetBundle assetBundle, string bundleId)
        {
            return LoadAssetsFromBundle<HandlingReleaseIntoSlotSet>(assetBundle, bundleId);
        }

        protected override void LoadAsset(UnityEngine.Object asset, string bundleId)
        {
            HandlingReleaseIntoSlotSet handlingSlotSet = asset as HandlingReleaseIntoSlotSet;

            OtherLogger.Log("Loading new handling QB slot set entry: " + handlingSlotSet.name, OtherLogger.LogType.Loading);
            ManagerSingleton<SM>.Instance.m_handlingReleaseIntoSlotDic.Add(handlingSlotSet.Type, handlingSlotSet);
        }
    }
}