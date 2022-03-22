using FistVR;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace OtherLoader.Loaders
{
    public class BulletImpactSetLoader : BaseAssetLoader
    {
        public override IEnumerator LoadAssetsFromBundle(AssetBundle assetBundle, string bundleId)
        {
            return LoadAssetsFromBundle<AudioBulletImpactSet>(assetBundle, bundleId);
        }

        protected override void LoadAsset(UnityEngine.Object asset, string bundleId)
        {
            AudioBulletImpactSet impactSet = asset as AudioBulletImpactSet;

            OtherLogger.Log("Loading new bullet impact set entry: " + impactSet.name, OtherLogger.LogType.Loading);
            SM.Instance.AudioBulletImpactSets = SM.Instance.AudioBulletImpactSets.Concat(new AudioBulletImpactSet[] { impactSet }).ToArray();
            SM.Instance.m_bulletHitDic.Add(impactSet.Type, impactSet);
        }
    }
}