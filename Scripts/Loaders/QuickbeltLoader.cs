using FistVR;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace OtherLoader.Loaders
{
    public class QuickbeltLoader : BaseAssetLoader
    {
        public override IEnumerator LoadAssetsFromBundle(AssetBundle assetBundle, string bundleId)
        {
            return LoadAssetsFromBundle<GameObject>(assetBundle, bundleId);
        }

        protected override void LoadAsset(UnityEngine.Object asset, string bundleId)
        {
            GameObject quickbeltPrefab = asset as GameObject;

            if (IsPrefabAQuickbelt(quickbeltPrefab))
            {
                OtherLogger.Log("Adding QuickBelt " + quickbeltPrefab.name, OtherLogger.LogType.Loading);
                GM.Instance.QuickbeltConfigurations = GM.Instance.QuickbeltConfigurations.Concat(new[] { quickbeltPrefab }).ToArray();
            }
        }

        private bool IsPrefabAQuickbelt(GameObject prefab)
        {
            string[] QBnameSplit = prefab.name.Split('_');

            return QBnameSplit.Length > 1 && QBnameSplit[QBnameSplit.Length - 2] == "QuickBelt";
        }
    }
}