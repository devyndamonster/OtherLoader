using FistVR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OtherLoader.Services
{
    public class SpawnerIdLoadingService : ISpawnerIdLoadingService
    {
        private readonly IPathService _pathService;
        private readonly IMetaDataService _metaDataService;

        public SpawnerIdLoadingService(IPathService pathService, IMetaDataService metaDataService)
        {
            _pathService = pathService;
            _metaDataService = metaDataService;
        }
        
        public IEnumerable<ItemSpawnerEntry> GenerateRequiredSpawnerEntriesForSpawnerId(ItemSpawnerID spawnerId)
        {
            var spawnerEntries = new List<ItemSpawnerEntry>();
            var spawnerEntry = GenerateSpawnerEntryFromSpawnerId(spawnerId);
            var parentEntries = GenerateParentSpawnerEntries(spawnerEntry);

            spawnerEntries.AddRange(parentEntries);
            spawnerEntries.Add(spawnerEntry);
            return spawnerEntries;
        }

        public IEnumerable<ItemSpawnerEntry> GenerateParentSpawnerEntries(ItemSpawnerEntry spawnerEntry)
        {
            var spawnerEntries = new List<ItemSpawnerEntry>();
            var pagePath = _pathService.GetRootPath(spawnerEntry.EntryPath);
            var middlePaths = _pathService
                .GetParentPaths(spawnerEntry.EntryPath)
                .Skip(1);

            spawnerEntries.Add(ItemSpawnerEntry.CreateEmpty(pagePath));
            
            foreach(var middlePath in middlePaths)
            {
                var middleEntry = ItemSpawnerEntry.CreateEmpty(middlePath);
                var pathEnding = _pathService.GetEndOfPath(middlePath);

                if (OtherLoader.TagGroupsByTag.TryGetValue(pathEnding, out var tagGroup))
                {
                    middleEntry.EntryIcon = tagGroup.Icon;
                    middleEntry.DisplayName = tagGroup.DisplayName;
                    middleEntry.IsDisplayedInMainEntry = true;
                }
                else
                {
                    OtherLogger.Log("Didn't have a tag group for " + pathEnding);
                }
                
                spawnerEntries.Add(middleEntry);
            }

            return spawnerEntries;
        }

        public ItemSpawnerEntry GenerateSpawnerEntryFromSpawnerId(ItemSpawnerID spawnerId)
        {
            var spawnerEntryPath = GetSpawnerEntryPathFromSpawnerId(spawnerId);
            var spawnerEntry = ItemSpawnerEntry.CreateEmpty(spawnerEntryPath);

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

            return spawnerEntry;
        }
        
        private string GetSpawnerEntryPathFromSpawnerId(ItemSpawnerID spawnerId)
        {
            string path = _metaDataService.GetSpawnerPageForSpawnerId(spawnerId).ToString();
            
            if (ShouldDisplayMainCategory(spawnerId))
            {
                path += "/" + _metaDataService.GetTagFromCategory(spawnerId.Category);
            }
            
            if (ShouldDisplaySubcategory(spawnerId))
            {
                path += "/" + _metaDataService.GetTagFromSubcategory(spawnerId.SubCategory);
            }

            path += "/" + GetMainObjectId(spawnerId);

            return path;
        }

        private string GetMainObjectId(ItemSpawnerID spawnerId)
        {
            return spawnerId.MainObject?.ItemID ?? spawnerId.ItemID;
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
