using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Services
{
    public interface ISpawnerIdLoadingService
    {
        public ItemSpawnerEntry GenerateSpawnerEntryFromSpawnerId(ItemSpawnerID spawnerId);
        
        public IEnumerable<ItemSpawnerEntry> GenerateRequiredSpawnerEntriesForSpawnerId(ItemSpawnerID spawnerId);
    }
}
