using FistVR;
using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace OtherLoader.Loaders
{
    public class HandlingReleaseSetLoader : BaseAssetLoader
    {
        public override IEnumerator LoadAssetsFromBundle(AssetBundle assetBundle, string bundleId)
        {
            return LoadAssetsFromBundle<HandlingReleaseSet>(assetBundle, bundleId);
        }

        protected override void LoadAsset(UnityEngine.Object asset, string bundleId)
        {
            HandlingReleaseSet handlingReleaseSet = asset as HandlingReleaseSet;

            OtherLogger.Log("Loading new handling release set entry: " + handlingReleaseSet.name, OtherLogger.LogType.Loading);
            SM.Instance.m_handlingReleaseDic.Add(handlingReleaseSet.Type, handlingReleaseSet);
        }
    }
}
