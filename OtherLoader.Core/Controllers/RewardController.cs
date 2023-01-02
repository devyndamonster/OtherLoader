using OtherLoader.Core.Adapters;
using OtherLoader.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Core.Controllers
{
    public class RewardController : IRewardController
    {
        private readonly IRewardSystemAdapter _rewardSystemAdapter;
        private readonly ItemDataContainer _dataContainer;
        private readonly List<string> _unlockedItemIds;

        public RewardController(IRewardSystemAdapter rewardSystemAdapter, ItemDataContainer dataContainer)
        {
            _rewardSystemAdapter = rewardSystemAdapter;
            _dataContainer = dataContainer;
            _unlockedItemIds = new List<string>();
        }

        public bool IsItemUnlocked(string mainObjectId)
        {
            return _unlockedItemIds.Contains(mainObjectId);
        }

        public bool ForceUnlockItem(string mainObjectId)
        {
            if (_unlockedItemIds.Contains(mainObjectId) || !_dataContainer.ItemEntries.Any(x => x.MainObjectId == mainObjectId))
            {
                return false;
            }
            
            _unlockedItemIds.Add(mainObjectId);
            return true;
        }

        public bool AutoUnlockItem(string mainObjectId)
        {
            var entries = _dataContainer.ItemEntries.Where(x => x.MainObjectId == mainObjectId);
            
            if(entries.Any(entry => !entry.IsReward))
            {
                return ForceUnlockItem(mainObjectId);
            }

            return false;
        }
    }
}
