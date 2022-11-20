using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Services
{
    public class SpawnerEntryLoadingService : ISpawnerEntryLoadingService
    {
        private readonly IPathService _pathService;

        public SpawnerEntryLoadingService(IPathService pathService)
        {
            _pathService = pathService;
        }

        public void AddItemSpawnerEntriesToPaths(IEnumerable<ItemSpawnerEntry> spawnerEntries)
        {
            foreach (var spawnerEntry in spawnerEntries)
            {
                AddItemSpawnerEntryToPaths(spawnerEntry);
            }
        }

        public void AddItemSpawnerEntryToPaths(ItemSpawnerEntry spawnerEntry)
        {
            string previousPath = _pathService.GetParentPath(spawnerEntry.EntryPath);
            var currentPath = spawnerEntry.EntryPath;
            
            ConstructParentNodes(currentPath);
            EntryNode previousNode = OtherLoader.SpawnerEntriesByPath[previousPath];

            if (OtherLoader.SpawnerEntriesByPath.ContainsKey(currentPath))
            {
                OtherLoader.SpawnerEntriesByPath[currentPath].entry = spawnerEntry;
            }
            else
            {
                var entryNode = new EntryNode(spawnerEntry);
                OtherLoader.SpawnerEntriesByPath[currentPath] = entryNode;
                previousNode.childNodes.Add(entryNode);
            }

            if (!spawnerEntry.IsCategoryEntry())
            {
                OtherLoader.SpawnerEntriesByID[spawnerEntry.MainObjectID] = spawnerEntry;
            }
        }

        private void ConstructParentNodes(string path)
        {
            
        }
    }
}
