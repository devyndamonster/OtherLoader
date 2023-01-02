using System;
using FistVR;
using OtherLoader.Core.Adapters;

namespace OtherLoader.Adapters
{
    public class RewardSystemAdapter : IRewardSystemAdapter
    {
        private readonly RewardSystem _rewardSystem = new RewardSystem();
        
        public void InitializeFromSaveFile()
        {
            _rewardSystem.InitializeFromSaveFile();
        }

        public bool IsRewardUnlocked(string itemId)
        {
            return _rewardSystem.RewardUnlocks.IsRewardUnlocked(itemId);
        }
    }
}
