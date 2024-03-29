﻿using FistVR;
using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace OtherLoader.Loaders
{
    public class MechanicalAccuracyLoader : BaseAssetLoader
    {
        public override IEnumerator LoadAssetsFromBundle(AssetBundle assetBundle, string bundleId)
        {
            return LoadAssetsFromBundle<FVRFireArmMechanicalAccuracyChart>(assetBundle, bundleId);
        }

        protected override void LoadAsset(UnityEngine.Object asset, string bundleId)
        {
            FVRFireArmMechanicalAccuracyChart chart = asset as FVRFireArmMechanicalAccuracyChart;

            foreach (FVRFireArmMechanicalAccuracyChart.MechanicalAccuracyEntry entry in chart.Entries)
            {
                LoadMechanicalAccuracyEntry(entry);
            }
        }

        private void LoadMechanicalAccuracyEntry(FVRFireArmMechanicalAccuracyChart.MechanicalAccuracyEntry entry)
        {
            if (AM.SMechanicalAccuracyDic.ContainsKey(entry.Class))
            {
                OtherLogger.LogError("Duplicate mechanical accuracy class found, will not use one of them! Make sure you're using unique mechanical accuracy classes!");
                return;
            }

            AM.SMechanicalAccuracyDic.Add(entry.Class, entry);

            OtherLogger.Log("Loaded new mechanical accuracy entry: " + entry.Class, OtherLogger.LogType.Loading);
        }

    }
}
