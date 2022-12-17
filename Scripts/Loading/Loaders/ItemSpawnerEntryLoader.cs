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
        private readonly IMetaDataService _metaDataService = new MetaDataService(new PathService());

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
                _metaDataService.RegisterSpawnerEntryIntoTagSystem(spawnerEntry);

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
