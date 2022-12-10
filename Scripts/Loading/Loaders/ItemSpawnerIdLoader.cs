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
    public class ItemSpawnerIdLoader : BaseAssetLoader
    {
        private readonly ISpawnerIdLoadingService _spawnerIdLoadingService = new SpawnerIdLoadingService(new PathService(), new MetaDataService());
        private readonly ISpawnerEntryLoadingService _spawnerEntryLoadingService = new SpawnerEntryLoadingService(new PathService());
        private readonly IMetaDataService _metaDataService = new MetaDataService();

        public override IEnumerator LoadAssetsFromBundle(AssetBundle assetBundle, string bundleId)
        {
            return LoadAssetsFromBundle<ItemSpawnerID>(assetBundle, bundleId);
        }
        
        protected override void LoadAsset(UnityEngine.Object asset, string bundleId)
        {
            ItemSpawnerID spawnerId = asset as ItemSpawnerID;

            OtherLogger.Log("Adding Itemspawner ID! Display Name: " + spawnerId.DisplayName + ", ItemID: " + spawnerId.ItemID + ", Category: " + spawnerId.Category + ", SubCategory: " + spawnerId.SubCategory + ", DisplayInMain: " + spawnerId.IsDisplayedInMainEntry, OtherLogger.LogType.Loading);

            PopulateMissingMainObject(spawnerId);

            if(spawnerId.MainObject != null)
            {
				OtherLogger.Log("MainObjectID for spawnerID: " + spawnerId.MainObject.ItemID, OtherLogger.LogType.Loading);

				UpdateUnlockCostForItem(spawnerId);
                UpdateUnlockStatusForItem(spawnerId);
				_metaDataService.RegisterSpawnerIDIntoTagSystem(spawnerId);
                
                if(_metaDataService.GetSpawnerPageForSpawnerId(spawnerId) == ItemSpawnerV2.PageMode.MainMenu)
                {
					OtherLogger.Log("Selected page for itemspawnerID is MainMenu!", OtherLogger.LogType.Loading);
					OtherLogger.Log("ItemID: " + spawnerId.ItemID, OtherLogger.LogType.Loading);
					OtherLogger.Log("Was the item added to a page? " + IM.Instance.PageItemLists.Any(o => o.Value.Contains(spawnerId.ItemID)), OtherLogger.LogType.Loading);
                }
            }

            if (CategoriesExistForSpawnerId(spawnerId))
            {
                AddSpawnerIdToGlobalDictionaries(spawnerId);

                if (!IsSpawnerIdAlreadyUsed(spawnerId))
                {
                    IM.Instance.SpawnerIDDic[spawnerId.ItemID] = spawnerId;
                    AddSpawnerIdToNewSpawner(spawnerId);
                }
            }
            else
            {
                OtherLogger.LogError("ItemSpawnerID could not be added, item will not appear in the itemspawner! Item Display Name: " + spawnerId.DisplayName + ", Item ID: " + spawnerId.ItemID);
                return;
            }

			if(spawnerId.MainObject != null)
            {
				UpdateVisibilityForItem(spawnerId);
			}
        }
        
        private void AddSpawnerIdToNewSpawner(ItemSpawnerID spawnerId)
        {
			OtherLogger.Log("Adding SpawnerID to spawner entry tree", OtherLogger.LogType.Loading);
			var spawnerEntries = _spawnerIdLoadingService.GenerateRequiredSpawnerEntriesForSpawnerId(spawnerId);
			_spawnerEntryLoadingService.AddItemSpawnerEntriesToPaths(spawnerEntries);
		}

        private bool CategoriesExistForSpawnerId(ItemSpawnerID spawnerId)
        {
            return IM.CD.ContainsKey(spawnerId.Category) && IM.SCD.ContainsKey(spawnerId.SubCategory);
        }

        private bool IsSpawnerIdAlreadyUsed(ItemSpawnerID spawnerId)
        {
            return IM.Instance.SpawnerIDDic.ContainsKey(spawnerId.ItemID);
        }
        
        private void UpdateUnlockCostForItem(ItemSpawnerID spawnerId)
        {
            if (spawnerId.UnlockCost == 0)
            {
                spawnerId.UnlockCost = spawnerId.MainObject.CreditCost;
            }
        }

        private void UpdateUnlockStatusForItem(ItemSpawnerID spawnerId)
        {
            if (!spawnerId.IsReward && OtherLoader.UnlockSaveData.AutoUnlockNonRewards)
            {
                OtherLoader.UnlockSaveData.UnlockItem(spawnerId.MainObject.ItemID);
            }
        }

        private void UpdateVisibilityForItem(ItemSpawnerID spawnerId)
        {
            if (!spawnerId.IsDisplayedInMainEntry)
            {
                HideItemFromCategories(spawnerId);
            }
        }

        private void HideItemFromCategories(ItemSpawnerID spawnerId)
        {
            foreach (List<string> pageItems in IM.Instance.PageItemLists.Values)
            {
                pageItems.Remove(spawnerId.ItemID);
            }
        }

        private void AddSpawnerIdToGlobalDictionaries(ItemSpawnerID spawnerId)
        {
            IM.CD[spawnerId.Category].Add(spawnerId);
            IM.SCD[spawnerId.SubCategory].Add(spawnerId);
        }


        private void PopulateMissingMainObject(ItemSpawnerID spawnerId)
        {
            if (spawnerId.MainObject == null)
            {
                spawnerId.MainObject = spawnerId.Secondaries.Select(o => o.MainObject).FirstOrDefault(o => o != null);

                if (spawnerId.MainObject == null) 
                {
                    throw new NullReferenceException("Could not select a secondary object for ItemSpawnerID, it will not appear in spawner: Display Name: " + spawnerId.DisplayName);
                }

				OtherLogger.Log("Assigning itemID from secondary object: " + spawnerId.MainObject.ItemID, OtherLogger.LogType.Loading);
				spawnerId.ItemID = spawnerId.MainObject.ItemID;
            }
        }
    }
}
