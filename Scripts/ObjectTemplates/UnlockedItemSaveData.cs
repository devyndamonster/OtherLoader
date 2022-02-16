using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader
{
    public class UnlockedItemSaveData
    {
        public bool UnlockAll = false;
        public bool AutoUnlockNonRewards = true;
        public ItemUnlockMode UnlockMode;
        public List<string> UnlockedItemIDs = new List<string>();

        public UnlockedItemSaveData() { }

        public UnlockedItemSaveData(ItemUnlockMode unlockMode)
        {
            UnlockMode = unlockMode;

            if(unlockMode == ItemUnlockMode.Unlockathon)
            {
                AutoUnlockNonRewards = false;
            }
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
            return spawnerID.MainObject == null || ShouldAutoUnlockItem(spawnerID.MainObject, spawnerID.IsReward);
        }

        public bool ShouldAutoUnlockItem(ItemSpawnerEntry spawnerEntry)
        {
            FVRObject item;
            IM.OD.TryGetValue(spawnerEntry.MainObjectID, out item);

            return item == null || ShouldAutoUnlockItem(item, spawnerEntry.IsReward);
        }

        public bool ShouldAutoUnlockItem(FVRObject item, bool isReward)
        {
            if(isReward || 
                (!AutoUnlockNonRewards && 
                (item.Category == FVRObject.ObjectCategory.Firearm || 
                item.Category == FVRObject.ObjectCategory.Thrown || 
                item.Category == FVRObject.ObjectCategory.MeleeWeapon)))
            {
                return false;
            }

            return true;
        }
    }

    public enum ItemUnlockMode
    {
        Normal,
        Unlockathon
    }
}
