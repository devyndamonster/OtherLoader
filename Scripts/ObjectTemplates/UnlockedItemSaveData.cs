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
            return spawnerID.MainObject == null || 
                ShouldAutoUnlockItem(spawnerID.MainObject, spawnerID.IsReward);
        }

        public bool ShouldAutoUnlockItem(ItemSpawnerEntry spawnerEntry)
        {
            FVRObject item;
            IM.OD.TryGetValue(spawnerEntry.MainObjectID, out item);

            return item == null || ShouldAutoUnlockItem(item, spawnerEntry.IsReward);
        }

        public bool ShouldAutoUnlockItem(FVRObject item, bool isReward)
        {
            if((isReward && !IsVanillaUnlocked(item)) || 
                (!AutoUnlockNonRewards && 
                (item.Category == FVRObject.ObjectCategory.Firearm || 
                item.Category == FVRObject.ObjectCategory.Thrown || 
                item.Category == FVRObject.ObjectCategory.MeleeWeapon)))
            {
                return false;
            }

            return true;
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
