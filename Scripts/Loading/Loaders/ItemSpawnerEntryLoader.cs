using FistVR;
using OtherLoader.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace OtherLoader.Loaders
{
    public class ItemSpawnerEntryLoader : BaseAssetLoader
    {
        private readonly ISpawnerEntryLoadingService _spawnerEntryLoadingService = new SpawnerEntryLoadingService(new PathService());

        private List<ItemSpawnerID> convertedSpawnerIDs;

        public override IEnumerator LoadAssetsFromBundle(AssetBundle assetBundle, string bundleId)
        {
            convertedSpawnerIDs = new List<ItemSpawnerID>();

            return LoadAssetsFromBundle<ItemSpawnerEntry>(assetBundle, bundleId);
        }

        protected override void LoadAsset(UnityEngine.Object asset, string bundleId)
        {
            ItemSpawnerEntry spawnerEntry = asset as ItemSpawnerEntry;

            OtherLogger.Log("Loading new item spawner entry: " + spawnerEntry.EntryPath, OtherLogger.LogType.Loading);
            OtherLogger.Log("Is Displayed in menu?: " + spawnerEntry.IsDisplayedInMainEntry, OtherLogger.LogType.Loading);

            spawnerEntry.IsModded = true;
            spawnerEntry.PopulateIDsFromObj();
            _spawnerEntryLoadingService.AddItemSpawnerEntryToPaths(spawnerEntry);

            if (!spawnerEntry.IsCategoryEntry())
            {
                OtherLogger.Log("Spawner Entry is not a category", OtherLogger.LogType.Loading);

                UpdateUnlockStatusForItem(spawnerEntry);
                RegisterItemIntoMetaTagSystem(spawnerEntry);

                ItemSpawnerID convertedSpawnerId = AddEntryToLegacySpawner(spawnerEntry);
                if(convertedSpawnerId != null)
                {
                    convertedSpawnerIDs.Add(convertedSpawnerId);
                }
            }
            else
            {
                OtherLogger.Log("Spawner Entry is a category", OtherLogger.LogType.Loading);
            }
        }

        protected override void AfterLoad()
        {
            PopulateSpawnerIdSecondaries();
        }

        private void PopulateSpawnerIdSecondaries()
        {
            foreach (ItemSpawnerID converted in convertedSpawnerIDs)
            {
                converted.Secondaries = converted.Secondaries_ByStringID
                    .Where(o => IM.Instance.SpawnerIDDic.ContainsKey(o))
                    .Select(o => IM.Instance.SpawnerIDDic[o])
                    .ToArray();
            }
        }

        private void UpdateUnlockStatusForItem(ItemSpawnerEntry spawnerEntry)
        {
            if (OtherLoader.UnlockSaveData.ShouldAutoUnlockItem(spawnerEntry))
            {
                OtherLoader.UnlockSaveData.UnlockItem(spawnerEntry.MainObjectID);
            }
        }

        private void RegisterItemIntoMetaTagSystem(ItemSpawnerEntry entry)
        {
            if (!IM.OD.ContainsKey(entry.MainObjectID)) return;

            FVRObject mainObject = IM.OD[entry.MainObjectID];
            ItemSpawnerV2.PageMode page = GetSpawnerPage(entry);
            
            RegisterSubcategory(entry, page);
            RegisterModTags(entry, page);
            RegisterItemIntoMetaTagSystem(mainObject, page);
        }

        private ItemSpawnerV2.PageMode GetSpawnerPage(ItemSpawnerEntry entry)
        {
            ItemSpawnerV2.PageMode page = ItemSpawnerV2.PageMode.Firearms;
            string pageString = entry.EntryPath.Split('/')[0];

            if (Enum.IsDefined(typeof(ItemSpawnerV2.PageMode), pageString))
            {
                page = (ItemSpawnerV2.PageMode)Enum.Parse(typeof(ItemSpawnerV2.PageMode), pageString);
            }

            return page;
        }

        private string GetSpawnerSubcategoryTag(ItemSpawnerEntry entry)
        {
            return entry.EntryPath.Split('/')[1];
        }

        

        private void RegisterModTags(ItemSpawnerEntry entry, ItemSpawnerV2.PageMode page)
        {
            foreach(string tag in entry.ModTags)
            {
                IM.AddMetaTag(tag, TagType.ModTag, entry.MainObjectID, page);
            }  
        }

        private void RegisterSubcategory(ItemSpawnerEntry entry, ItemSpawnerV2.PageMode page)
        {
            string subcategory = GetSpawnerSubcategoryTag(entry);
            IM.AddMetaTag(subcategory, TagType.SubCategory, entry.MainObjectID, page);
        }


        private void RegisterItemIntoMetaTagSystem(FVRObject mainObject, ItemSpawnerV2.PageMode page)
        {
            if (mainObject.Category == FVRObject.ObjectCategory.Firearm)
            {
                RegisterFirearmIntoMetaTagSystem(mainObject, page);
            }

            else if (mainObject.Category == FVRObject.ObjectCategory.Attachment)
            {
                RegisterAttachmentIntoMetaTagSystem(mainObject, page);
            }
        }

        private void RegisterFirearmIntoMetaTagSystem(FVRObject mainObject, ItemSpawnerV2.PageMode page)
        {
            IM.AddMetaTag(mainObject.TagSet.ToString(), TagType.Set, mainObject.ItemID, page);
            IM.AddMetaTag(mainObject.TagEra.ToString(), TagType.Era, mainObject.ItemID, page);
            IM.AddMetaTag(mainObject.TagFirearmSize.ToString(), TagType.Size, mainObject.ItemID, page);
            IM.AddMetaTag(mainObject.TagFirearmAction.ToString(), TagType.Action, mainObject.ItemID, page);
            IM.AddMetaTag(mainObject.TagFirearmRoundPower.ToString(), TagType.RoundClass, mainObject.ItemID, page);
            IM.AddMetaTag(mainObject.TagFirearmCountryOfOrigin.ToString(), TagType.CountryOfOrigin, mainObject.ItemID, page);
            IM.AddMetaTag(mainObject.TagFirearmFirstYear.ToString(), TagType.IntroductionYear, mainObject.ItemID, page);
            IM.AddMetaTag(mainObject.MagazineType.ToString(), TagType.MagazineType, mainObject.ItemID, page);

            if (mainObject.UsesRoundTypeFlag)
                IM.AddMetaTag(mainObject.RoundType.ToString(), TagType.Caliber, mainObject.ItemID, page);

            mainObject.TagFirearmFiringModes.ForEach(tag =>
                IM.AddMetaTag(tag.ToString(), TagType.FiringMode, mainObject.ItemID, page));

            mainObject.TagFirearmFeedOption.ForEach(tag =>
                IM.AddMetaTag(tag.ToString(), TagType.FeedOption, mainObject.ItemID, page));

            mainObject.TagFirearmMounts.ForEach(mode =>
                IM.AddMetaTag(mode.ToString(), TagType.AttachmentMount, mainObject.ItemID, page));
        }

        private void RegisterAttachmentIntoMetaTagSystem(FVRObject mainObject, ItemSpawnerV2.PageMode page)
        {
            IM.AddMetaTag(mainObject.TagSet.ToString(), TagType.Set, mainObject.ItemID, page);
            IM.AddMetaTag(mainObject.TagEra.ToString(), TagType.Era, mainObject.ItemID, page);
            IM.AddMetaTag(mainObject.TagAttachmentFeature.ToString(), TagType.AttachmentFeature, mainObject.ItemID, page);
            IM.AddMetaTag(mainObject.TagAttachmentMount.ToString(), TagType.AttachmentMount, mainObject.ItemID, page);
        }


        private ItemSpawnerID AddEntryToLegacySpawner(ItemSpawnerEntry entry)
        {
            if (!entry.IsCategoryEntry() && IM.OD.ContainsKey(entry.MainObjectID))
            {
                OtherLogger.Log("Adding spawner entry to legacy spawner", OtherLogger.LogType.Loading);

                ItemSpawnerID itemSpawnerID = _spawnerEntryLoadingService.ConvertSpawnerEntryToSpawnerId(entry);

                if (itemSpawnerID == null) return null;

                IM.Instance.SpawnerIDDic[itemSpawnerID.MainObject.ItemID] = itemSpawnerID;
                OtherLoader.SpawnerIDsByMainObject[itemSpawnerID.MainObject.ItemID] = itemSpawnerID;

                if (itemSpawnerID.SubCategory != ItemSpawnerID.ESubCategory.None)
                {
                    IM.CD[itemSpawnerID.Category].Add(itemSpawnerID);
                    IM.SCD[itemSpawnerID.SubCategory].Add(itemSpawnerID);
                }
                else
                {
                    OtherLogger.Log("Created ItemSpawnerID will not appear in legacy spawner because subcategory could not be found", OtherLogger.LogType.Loading);
                }

                return itemSpawnerID;
            }

            return null;
        }


        
    }
}
