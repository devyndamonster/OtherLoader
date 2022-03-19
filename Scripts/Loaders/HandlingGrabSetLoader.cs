using FistVR;
using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace OtherLoader.Loaders
{
    public class HandlingGrabSetLoader : BaseAssetLoader
    {
        public override IEnumerator LoadAssetsFromBundle(AssetBundle assetBundle, string bundleId)
        {
            return LoadAssetsFromBundle<HandlingGrabSet>(assetBundle, bundleId);
        }

        protected override void LoadAsset(UnityEngine.Object asset, string bundleId)
        {
            HandlingGrabSet handlingGrabSet = asset as HandlingGrabSet;

            OtherLogger.Log("Loading new handling grab set entry: " + handlingGrabSet.name, OtherLogger.LogType.Loading);
            SM.Instance.m_handlingGrabDic.Add(handlingGrabSet.Type, handlingGrabSet);
        }
    }
}
