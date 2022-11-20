using System.Collections.Generic;

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
            var entryPath = spawnerEntry.EntryPath;
            ConstructParentNodes(entryPath);
            
            if (OtherLoader.SpawnerEntriesByPath.ContainsKey(entryPath))
            {
                OtherLoader.SpawnerEntriesByPath[entryPath].entry = spawnerEntry;
            }
            else
            {
                var entryNode = new EntryNode(spawnerEntry);
                AddEntryNodeToPaths(entryNode);
            }

            if (!spawnerEntry.IsCategoryEntry())
            {
                OtherLoader.SpawnerEntriesByID[spawnerEntry.MainObjectID] = spawnerEntry;
            }
        }

        private void ConstructParentNodes(string path)
        {
            var childPaths = _pathService.GetChildPaths(path);
            foreach(var childPath in childPaths)
            {
                if (!OtherLoader.SpawnerEntriesByPath.ContainsKey(childPath))
                {
                    var entryNode = new EntryNode(path);
                    AddEntryNodeToPaths(entryNode);
                }
            }
        }
        
        private void AddEntryNodeToPaths(EntryNode entryNode)
        {
            var path = entryNode.entry.EntryPath;
            OtherLoader.SpawnerEntriesByPath[path] = entryNode;

            if (_pathService.HasParent(path))
            {
                var parentNode = OtherLoader.SpawnerEntriesByPath[_pathService.GetParentPath(path)];
                parentNode.childNodes.Add(entryNode);
            }
        }
    }
}
