using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace OtherLoader.Loaders
{
    public class AudioImpactSetLoader : BaseAssetLoader
    {
        public override IEnumerator LoadAssetsFromBundle(AssetBundle assetBundle, string bundleId)
        {
            return LoadAssetsFromBundle<AudioImpactSet>(assetBundle, bundleId);
        }

        protected override void LoadAsset(UnityEngine.Object asset, string bundleId)
        {
            AudioImpactSet impactSet = asset as AudioImpactSet;

            OtherLogger.Log("Loading new Audio Impact Set: " + impactSet.name, OtherLogger.LogType.Loading);
            SM.Instance.AudioImpactSets = SM.Instance.AudioImpactSets.Concat(new[] { impactSet }).ToArray();
            SM.Instance.m_impactDic = new Dictionary<ImpactType, Dictionary<MatSoundType, Dictionary<AudioImpactIntensity, AudioEvent>>>();
            SM.Instance.generateImpactDictionary();
        }
    }
}
