using FistVR;
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
        private SpawnerEntryPathBuilder entryPathBuilder = new SpawnerEntryPathBuilder();

        public override IEnumerator LoadAssetsFromBundle(AssetBundle assetBundle, string bundleId)
        {
            return LoadAssetsFromBundle<ItemSpawnerID>(assetBundle, bundleId);
        }


        protected override void LoadAsset(UnityEngine.Object asset, string bundleId)
        {
            ItemSpawnerID spawnerId = asset as ItemSpawnerID;

            OtherLogger.Log("Adding Itemspawner ID! Display Name: " + spawnerId.DisplayName + ", ItemID: " + spawnerId.ItemID + ", Category: " + spawnerId.Category + ", SubCategory: " + spawnerId.SubCategory, OtherLogger.LogType.Loading);

            PopulateMissingMainObject(spawnerId);

            if(spawnerId.MainObject != null)
            {
                UpdateUnlockCostForItem(spawnerId);
                UpdateUnlockStatusForItem(spawnerId);
                IM.RegisterItemIntoMetaTagSystem(spawnerId);
                UpdateVisibilityForItem(spawnerId);
            }

            if (CategoriesExistForSpawnerId(spawnerId))
            {
                AddSpawnerIdToGlobalDictionaries(spawnerId);

                if (IsSpawnerIdAlreadyUsed(spawnerId))
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
        }


        private void AddSpawnerIdToNewSpawner(ItemSpawnerID spawnerId)
        {
            ItemSpawnerEntry SpawnerEntry = ScriptableObject.CreateInstance<ItemSpawnerEntry>();

            if (IsCustomCategory(spawnerId.Category))
            {
                OtherLogger.Log("Adding SpawnerID to spawner entry tree under custom category", OtherLogger.LogType.Loading);
                SpawnerEntry.LegacyPopulateFromID(ItemSpawnerV2.PageMode.Firearms, spawnerId, true);
                entryPathBuilder.PopulateEntryPaths(SpawnerEntry, spawnerId);
            }
            
            else
            {
                OtherLogger.Log("Adding SpawnerID under vanilla category", OtherLogger.LogType.Loading);
                ItemSpawnerV2.PageMode spawnerPage = GetPageForSpawnerId(spawnerId);
                SpawnerEntry.LegacyPopulateFromID(spawnerPage, spawnerId, true);
                entryPathBuilder.PopulateEntryPaths(SpawnerEntry, spawnerId);
            }
        }

        private bool CategoriesExistForSpawnerId(ItemSpawnerID spawnerId)
        {
            return IM.CD.ContainsKey(spawnerId.Category) && IM.SCD.ContainsKey(spawnerId.SubCategory);
        }

        private bool IsSpawnerIdAlreadyUsed(ItemSpawnerID spawnerId)
        {
            return !IM.Instance.SpawnerIDDic.ContainsKey(spawnerId.ItemID);
        }

        private bool IsCustomCategory(ItemSpawnerID.EItemCategory category)
        {
            return Enum.IsDefined(typeof(ItemSpawnerID.EItemCategory), category);
        }


        private ItemSpawnerV2.PageMode GetPageForSpawnerId(ItemSpawnerID spawnerId)
        {
            return IM.Instance.PageItemLists.FirstOrDefault(o => o.Value.Contains(spawnerId.ItemID)).Key;
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
                pageItems.Remove(spawnerId.MainObject.ItemID);
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

                spawnerId.ItemID = spawnerId.MainObject.ItemID;
            }
        }
    }
}
