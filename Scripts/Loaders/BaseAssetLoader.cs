using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OtherLoader.Loaders
{
    public abstract class BaseAssetLoader
    {

        public IEnumerator LoadAssetsFromBundle<T>(AssetBundle assetBundle, string bundleId)
        {
            AssetBundleRequest bundleRequest = assetBundle.LoadAllAssetsAsync<T>();
            yield return bundleRequest;
            LoadAssets(bundleRequest.allAssets, bundleId);
        }

        public void LoadAssets(UnityEngine.Object[] assets, string bundleId)
        {
            foreach (UnityEngine.Object asset in assets)
            {
                TryLoadAsset(asset, bundleId);
            }
        }

        public void TryLoadAsset(UnityEngine.Object asset, string bundleId)
        {
            try
            {
                LoadAsset(asset, bundleId);
            }
            catch (Exception ex)
            {
                OtherLogger.LogError("Failed to load asset! Exception: \n" + ex.ToString());
            }
        }

        protected abstract void LoadAsset(UnityEngine.Object asset, string bundleId);


    }
}
