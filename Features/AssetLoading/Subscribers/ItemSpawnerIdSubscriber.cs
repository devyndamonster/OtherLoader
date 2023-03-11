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
        * - Parse them into a unified item spawner entry
        *   - Maybe should do quick parse into core spawner Id and then conversion happens in Core for testability?
        * - Pass them into an item meta data store
        * - Assets like images get connected by Id
        * - Item spawner controller hooks into item meta data store?
        */
        protected override void LoadSubscribedAssets(IEnumerable<ItemSpawnerID> assets)
        {
            foreach(var spawnerId in assets)
            {
                var convertedSpawnerId = ConvertSpawnerIdToCore(spawnerId);

                var spawnerEntry = _spawnerEntryDataService.ConvertToSpawnerEntryData(convertedSpawnerId);

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

        private SpawnerEntryData GenerateSpawnerEntryFromSpawnerId(ItemSpawnerID spawnerId)
        {
            var spawnerEntry = new SpawnerEntryData
            {
                Path = GetPathFromSpawnerId(spawnerId)
            };

            /*
            spawnerEntry.MainObjectID = GetMainObjectId(spawnerId);
            spawnerEntry.SpawnWithIDs = spawnerId.SecondObject is null ? new List<string>() : new List<string> { spawnerId.SecondObject.ItemID };

            spawnerEntry.SecondaryObjectIDs = spawnerId.Secondaries is null ? new List<string>() : spawnerId.Secondaries
                .Where(secondary => secondary != null && secondary.MainObject != null)
                .Select(secondary => secondary.MainObject.ItemID)
                .ToList();

            spawnerEntry.SecondaryObjectIDs.AddRange(spawnerId.Secondaries_ByStringID?.Where(id => !spawnerEntry.SecondaryObjectIDs.Contains(id)) ?? new List<string>());
            spawnerEntry.EntryIcon = spawnerId.Sprite;
            spawnerEntry.DisplayName = spawnerId.DisplayName;
            spawnerEntry.IsDisplayedInMainEntry = spawnerId.IsDisplayedInMainEntry;
            spawnerEntry.UsesLargeSpawnPad = spawnerId.UsesLargeSpawnPad;
            spawnerEntry.UsesHugeSpawnPad = spawnerId.UsesHugeSpawnPad;
            spawnerEntry.IsModded = IM.OD[spawnerEntry.MainObjectID].IsModContent;
            spawnerEntry.TutorialBlockIDs = spawnerId.TutorialBlocks is null ? new List<string>() : new List<string>(spawnerId.TutorialBlocks);
            spawnerEntry.ModTags = spawnerId.ModTags is null ? new List<string>() : new List<string>(spawnerId.ModTags);
            */

            return spawnerEntry;
        }

        private string GetPathFromSpawnerId(ItemSpawnerID spawnerId)
        {
            //string path = _metaDataService.GetSpawnerPageForSpawnerId(spawnerId).ToString();

            if (ShouldDisplayMainCategory(spawnerId))
            {
                //path += "/" + _metaDataService.GetTagFromCategory(spawnerId.Category);
            }

            if (ShouldDisplaySubcategory(spawnerId))
            {
                //path += "/" + _metaDataService.GetTagFromSubcategory(spawnerId.SubCategory);
            }

            //path += "/" + GetMainObjectId(spawnerId);

            //return path;

            return "";
        }

        private bool ShouldDisplayMainCategory(ItemSpawnerID spawnerId)
        {
            var isDisplayableVanillaCategory = spawnerId.Category == ItemSpawnerID.EItemCategory.MeatFortress ||
                spawnerId.Category == ItemSpawnerID.EItemCategory.Magazine ||
                spawnerId.Category == ItemSpawnerID.EItemCategory.Cartridge ||
                spawnerId.Category == ItemSpawnerID.EItemCategory.Clip ||
                spawnerId.Category == ItemSpawnerID.EItemCategory.Speedloader;

            var isModdedMainCategory = !Enum.IsDefined(typeof(ItemSpawnerID.EItemCategory), spawnerId.Category);

            return isDisplayableVanillaCategory || isModdedMainCategory;
        }

        private bool ShouldDisplaySubcategory(ItemSpawnerID spawnerId)
        {
            return spawnerId.SubCategory != ItemSpawnerID.ESubCategory.None;
        }
    }
}
