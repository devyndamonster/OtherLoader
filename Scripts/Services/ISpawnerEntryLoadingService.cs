using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Services
{
    public interface ISpawnerEntryLoadingService
    {
        public void AddItemSpawnerEntryToPaths(ItemSpawnerEntry spawnerEntry);

        public void AddItemSpawnerEntriesToPaths(IEnumerable<ItemSpawnerEntry> spawnerEntries);

        public ItemSpawnerID ConvertSpawnerEntryToSpawnerId(ItemSpawnerEntry spawnerEntry);
    }
}
