using OtherLoader.Core.Adapters;
using OtherLoader.Core.Models;
using OtherLoader.Core.Services;
using System;
using System.IO.Abstractions;
using System.Linq;
using System.IO;

#if DEBUG
using Newtonsoft.Json;
#else
using Valve.Newtonsoft.Json;
#endif

namespace OtherLoader.Core.Controllers
{
    public class RewardController : IRewardController
    {
        private readonly IRewardSystemAdapter _rewardSystemAdapter;
        private readonly IApplicationPathService _applicationPathService;
        private readonly ItemDataContainer _dataContainer;
        
        private UnlockedItemSaveData _unlockedItemSaveData;
        
        public RewardController(IRewardSystemAdapter rewardSystemAdapter, IApplicationPathService applicationPathService, ItemDataContainer dataContainer)
        {
            _rewardSystemAdapter = rewardSystemAdapter;
            _applicationPathService = applicationPathService;
            _dataContainer = dataContainer;

            InitializeUnlockData();
        }

        public void InitializeUnlockData()
        {
            _rewardSystemAdapter.InitializeFromSaveFile();
            _applicationPathService.InitializeApplicationPaths();
            
            if (!File.Exists(_applicationPathService.UnlockedItemSaveDataPath))
            {
                _unlockedItemSaveData = new UnlockedItemSaveData();
                SaveUnlockedItemsData(_unlockedItemSaveData);
            }

            else
            {
                _unlockedItemSaveData = LoadUnlockedItemsData();
            }
        }

        public UnlockedItemSaveData LoadUnlockedItemsData()
        {
            try
            {
                string unlockJson = File.ReadAllText(_applicationPathService.UnlockedItemSaveDataPath);
                return JsonConvert.DeserializeObject<UnlockedItemSaveData>(unlockJson);
            }
            catch (Exception ex)
            {
                //OtherLogger.LogError("Exception when loading unlocked items!\n" + ex.ToString());
                //OtherLogger.LogError("Attempting to create new unlock file");
                
                File.Delete(_applicationPathService.UnlockedItemSaveDataPath);
                return new UnlockedItemSaveData();
            }
        }
        
        public void SaveUnlockedItemsData(UnlockedItemSaveData unlockedItemSaveData)
        {
            try
            {
                string unlockJson = JsonConvert.SerializeObject(unlockedItemSaveData, Formatting.Indented);
                File.WriteAllText(_applicationPathService.UnlockedItemSaveDataPath, unlockJson);
            }
            catch (Exception ex)
            {
                //OtherLogger.LogError("Exception when saving unlocked items!\n" + ex.ToString());
            }
        }

        public bool IsItemUnlocked(string mainObjectId)
        {
            return _unlockedItemSaveData.UnlockedItemIDs.Contains(mainObjectId);
        }

        public bool ForceUnlockItem(string mainObjectId)
        {
            if (_unlockedItemSaveData.UnlockedItemIDs.Contains(mainObjectId) || !_dataContainer.ItemEntries.Any(x => x.MainObjectId == mainObjectId))
            {
                return false;
            }

            _unlockedItemSaveData.UnlockedItemIDs.Add(mainObjectId);
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
