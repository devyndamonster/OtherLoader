using FistVR;
using System.Linq;
using System;
using System.IO;
using System.Collections;
using UnityEngine;

namespace OtherLoader.Loaders
{
    public class FVRObjectLoader : BaseAssetLoader
    {

        public override IEnumerator LoadAssetsFromBundle(AssetBundle assetBundle, string bundleId)
        {
            return LoadAssetsFromBundle<FVRObject>(assetBundle, bundleId);
        }

        protected override void LoadAsset(UnityEngine.Object asset, string bundleId)
        {
            FVRObject fvrObject = asset as FVRObject;

            if (IM.OD.ContainsKey(fvrObject.ItemID))
            {
                OtherLogger.LogError("The ItemID of FVRObject is already used! Item will not be loaded! ItemID: " + fvrObject.ItemID);
                return;
            }
            
            SetAnvilBundle(fvrObject, bundleId);
            CalcCreditCostIfUnassigned(fvrObject);
            LoadIntoObjectDatabase(fvrObject);

            fvrObject.IsModContent = true;

            OtherLogger.Log("Loaded FVRObject: " + fvrObject.ItemID, OtherLogger.LogType.Loading);
        }

        private void SetAnvilBundle(FVRObject fvrObject, string bundleId)
        {
            fvrObject.m_anvilPrefab.Bundle = bundleId;
        }

        private void CalcCreditCostIfUnassigned(FVRObject fvrObject)
        {
            if (fvrObject.CreditCost == 0) fvrObject.CalcCreditCost();
        }

        private void LoadIntoObjectDatabase(FVRObject fvrObject)
        {
            IM.OD.Add(fvrObject.ItemID, fvrObject);

            IM.Instance.odicTagCategory.CreateValueIfNewKey(fvrObject.Category).Add(fvrObject);
            IM.Instance.odicTagFirearmEra.CreateValueIfNewKey(fvrObject.TagEra).Add(fvrObject);
            IM.Instance.odicTagFirearmSize.CreateValueIfNewKey(fvrObject.TagFirearmSize).Add(fvrObject);
            IM.Instance.odicTagFirearmAction.CreateValueIfNewKey(fvrObject.TagFirearmAction).Add(fvrObject);
            IM.Instance.odicTagAttachmentMount.CreateValueIfNewKey(fvrObject.TagAttachmentMount).Add(fvrObject);
            IM.Instance.odicTagAttachmentFeature.CreateValueIfNewKey(fvrObject.TagAttachmentFeature).Add(fvrObject);

            fvrObject.TagFirearmFeedOption.ForEach(o => IM.Instance.odicTagFirearmFeedOption.CreateValueIfNewKey(o).Add(fvrObject));
            fvrObject.TagFirearmFiringModes.ForEach(o => IM.Instance.odicTagFirearmFiringMode.CreateValueIfNewKey(o).Add(fvrObject));
            fvrObject.TagFirearmMounts.ForEach(o => IM.Instance.odicTagFirearmMount.CreateValueIfNewKey(o).Add(fvrObject));
        }

    }
}
