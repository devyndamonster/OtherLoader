using System.Collections.Generic;

namespace OtherLoader
{
    public class UnlockedItemSaveData
    {
        public bool UnlockAll { get; set; } = false;
        
        public bool AutoUnlockNonRewards { get; set; } = true;

        public ItemUnlockMode UnlockMode { get; set; }

        public List<string> UnlockedItemIDs { get; set; } = new List<string>();
    }

    public enum ItemUnlockMode
    {
        Normal,
        Unlockathon
    }
}
