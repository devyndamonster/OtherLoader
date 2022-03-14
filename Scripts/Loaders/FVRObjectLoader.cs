using FistVR;
using System.Linq;
using System;
using System.IO;

namespace OtherLoader.Loaders
{
    public class FVRObjectLoader : BaseAssetLoader
    {
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

            IM.Instance.odicTagCategory.AddOrCreate(fvrObject.Category).Add(fvrObject);
            IM.Instance.odicTagFirearmEra.AddOrCreate(fvrObject.TagEra).Add(fvrObject);
            IM.Instance.odicTagFirearmSize.AddOrCreate(fvrObject.TagFirearmSize).Add(fvrObject);
            IM.Instance.odicTagFirearmAction.AddOrCreate(fvrObject.TagFirearmAction).Add(fvrObject);
            IM.Instance.odicTagAttachmentMount.AddOrCreate(fvrObject.TagAttachmentMount).Add(fvrObject);
            IM.Instance.odicTagAttachmentFeature.AddOrCreate(fvrObject.TagAttachmentFeature).Add(fvrObject);

            fvrObject.TagFirearmFeedOption.ForEach(o => IM.Instance.odicTagFirearmFeedOption.AddOrCreate(o).Add(fvrObject));
            fvrObject.TagFirearmFiringModes.ForEach(o => IM.Instance.odicTagFirearmFiringMode.AddOrCreate(o).Add(fvrObject));
            fvrObject.TagFirearmMounts.ForEach(o => IM.Instance.odicTagFirearmMount.AddOrCreate(o).Add(fvrObject));
        }

    }
}
