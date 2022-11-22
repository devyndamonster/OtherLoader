using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OtherLoader
{
    public class UnlockedItemSaveData
    {
        public bool UnlockAll = false;
        public bool AutoUnlockNonRewards = true;
        public ItemUnlockMode UnlockMode;
        public List<string> UnlockedItemIDs = new List<string>();

        private RewardSystem rewardSystem = new RewardSystem();

        public UnlockedItemSaveData() 
        {
            rewardSystem.InitializeFromSaveFile();
        }

        public UnlockedItemSaveData(ItemUnlockMode unlockMode)
        {
            UnlockMode = unlockMode;

            if(unlockMode == ItemUnlockMode.Unlockathon)
            {
                AutoUnlockNonRewards = false;
            }

            rewardSystem.InitializeFromSaveFile();
        }

        public bool IsItemUnlocked(string itemID)
        {
            return UnlockAll || UnlockedItemIDs.Contains(itemID);
        }

        public bool UnlockItem(string itemID)
        {
            if (!UnlockedItemIDs.Contains(itemID))
            {
                UnlockedItemIDs.Add(itemID);
                return true;
            }

            return false;
        }

        public bool ShouldAutoUnlockItem(ItemSpawnerID spawnerID)
        {
            if (spawnerID.MainObject == null) return true;

            if ((spawnerID.IsReward && !IsVanillaUnlocked(spawnerID.MainObject)) || !ShouldAutoUnlockItem(spawnerID.MainObject))
            {
                return false;
            }

            return true;
        }

        public bool ShouldAutoUnlockItem(ItemSpawnerEntry spawnerEntry)
        {
            FVRObject item;
            IM.OD.TryGetValue(spawnerEntry.MainObjectID, out item);

            if (item == null) return true;

            if (spawnerEntry.IsReward || !ShouldAutoUnlockItem(item))
            {
                return false;
            }

            return true;
        }

        public bool ShouldAutoUnlockItem(FVRObject item)
        {
            return AutoUnlockNonRewards ||
                    (item.Category != FVRObject.ObjectCategory.Firearm &&
                    item.Category != FVRObject.ObjectCategory.Thrown &&
                    item.Category != FVRObject.ObjectCategory.MeleeWeapon);
        }

        public bool IsVanillaUnlocked(FVRObject item)
        {
            ItemSpawnerID spawnerID;
            OtherLoader.SpawnerIDsByMainObject.TryGetValue(item.ItemID, out spawnerID);

            //We must use our own version of reward system here because the GM does not always perform Awake before the IM
            return spawnerID == null || rewardSystem.RewardUnlocks.IsRewardUnlocked(spawnerID.ItemID);
        }
    }

    public enum ItemUnlockMode
    {
        Normal,
        Unlockathon
    }
}
