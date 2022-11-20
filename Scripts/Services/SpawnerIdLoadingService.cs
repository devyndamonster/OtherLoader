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
        
        public SpawnerIdLoadingService(IPathService pathService)
        {
            _pathService = pathService;
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
                .GetSpreadOfPath(_pathService.GetParentPath(spawnerEntry.EntryPath))
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
                }

                spawnerEntries.Add(middleEntry);
            }

            return spawnerEntries;
        }

        public ItemSpawnerEntry GenerateSpawnerEntryFromSpawnerId(ItemSpawnerID spawnerId)
        {
            throw new NotImplementedException();
        }
    }
}
