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
    }

    public enum ItemUnlockMode
    {
        Normal,
        Unlockathon
    }
}
