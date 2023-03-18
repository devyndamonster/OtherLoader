using FistVR;
using OtherLoader.Core.Features.MetaData;
using OtherLoader.Core.Features.MetaData.Models.Vanilla;
using OtherLoader.Core.Features.MetaData.Services;
using OtherLoader.Core.Models;
using OtherLoader.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OtherLoader.Features.AssetLoading.Subscribers
{
    public class ItemSpawnerIdSubscriber : BaseAssetLoadingSubscriber<ItemSpawnerID>
    {
        private readonly IItemMetaDataRepository _itemMetaDataRepository;
        private readonly ISpawnerEntryDataService _spawnerEntryDataService;

        public ItemSpawnerIdSubscriber(
            IAssetLoadingService assetLoadingService,
            IItemMetaDataRepository itemMetaDataRepository,
            ISpawnerEntryDataService spawnerEntryDataService) : base(assetLoadingService)
        {
            _itemMetaDataRepository = itemMetaDataRepository;
            _spawnerEntryDataService = spawnerEntryDataService;
        }

        /* What Do We Do With SpawnerIds?
        * [x] Parse them into a unified item spawner entry
        * [x] Pass them into an item meta data store
        * [ ] Item spawner controller hooks into item meta data store
        * [ ] Assets like images get connected by Id
        */
        protected override void LoadSubscribedAssets(IEnumerable<ItemSpawnerID> assets)
        {
            foreach(var spawnerId in assets)
            {
                var convertedSpawnerId = ConvertSpawnerIdToCore(spawnerId);
                var convertedMainObject = ConvertFVRObjectToCore(spawnerId.MainObject);

                var spawnerEntry = _spawnerEntryDataService.ConvertToSpawnerEntryData(convertedSpawnerId, convertedMainObject);

                _itemMetaDataRepository.AddItemSpawnerEntry(spawnerEntry);
            }
        }

        private ItemSpawnerId ConvertSpawnerIdToCore(ItemSpawnerID spawnerId)
        {
            return new ItemSpawnerId
            {
                ItemId = spawnerId.ItemID,
                DisplayName = spawnerId.DisplayName,
                MainObjectId = spawnerId.MainObject.ItemID,
                SecondObjectId = spawnerId.SecondObject.ItemID,
                Description = spawnerId.Description,
                SubHeading = spawnerId.SubHeading,
                Secondaries = spawnerId.Secondaries.Select(ConvertSpawnerIdToCore),
                SecondariesByString = spawnerId.Secondaries_ByStringID,
                Category = (ItemCategory)spawnerId.Category,
                SubCategory = (SubCategory)spawnerId.SubCategory,
                ModTags = spawnerId.ModTags,
                TutorialBlocks = spawnerId.TutorialBlocks,
                UsesHugeSpawnPad = spawnerId.UsesHugeSpawnPad,
                UsesLargeSpawnPad = spawnerId.UsesLargeSpawnPad,
                IsDisplayedInMainEntry = spawnerId.IsDisplayedInMainEntry,
                IsReward = spawnerId.IsReward,
                UnlockCost = spawnerId.UnlockCost,
                IsUnlockedByDefault = spawnerId.IsUnlockedByDefault,
            };
        }

        private Core.Features.MetaData.Models.Vanilla.FVRObject ConvertFVRObjectToCore(FistVR.FVRObject fvrObject)
        {
            return new Core.Features.MetaData.Models.Vanilla.FVRObject
            {
                ObjectId = fvrObject.ItemID,
                ObjectCategory = (ObjectCategory)fvrObject.Category
            };
        }
    }
}
