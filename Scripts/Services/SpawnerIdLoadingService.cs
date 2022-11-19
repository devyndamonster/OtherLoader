using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Scripts.Services
{
    public class SpawnerIdLoadingService : ISpawnerIdLoadingService
    {
        private readonly ISpawnerEntryLoadingService _spawnerEntryLoadingService;

        public SpawnerIdLoadingService(ISpawnerEntryLoadingService spawnerEntryLoadingService)
        {
            _spawnerEntryLoadingService = spawnerEntryLoadingService;
        }

        public void PopulateEntriesFromSpawnerId(ItemSpawnerID spawnerId)
        {
            var spawnerEntry = _spawnerEntryLoadingService.GetSpawnerEntryFromSpawnerId(spawnerId);
            PopulateEntryForPage(spawnerEntry);
            PopulateEntryForCategory(spawnerEntry);
            PopulateEntryForSpawnerId(spawnerEntry);
        }
        
        private void PopulateEntryForPage(ItemSpawnerEntry spawnerEntry)
        {
            var page = GetPage(spawnerEntry);
            if (!OtherLoader.SpawnerEntriesByPath.ContainsKey(page))
            {
                var pageNode = new EntryNode();
                pageNode.entry.EntryPath = page;
                OtherLoader.SpawnerEntriesByPath[page] = pageNode;
            }
        }
        
        private void PopulateEntryForCategory(ItemSpawnerEntry spawnerEntry)
        {
            var page = GetPage(spawnerEntry);
            var category = GetCategory(spawnerEntry);
            var path = page + "/" + category;
            var pageNode = OtherLoader.SpawnerEntriesByPath[page];
            
            if (!OtherLoader.SpawnerEntriesByPath.ContainsKey(path))
            {
                if (OtherLoader.TagGroupsByTag.TryGetValue(category, out var tagGroup))
                {
                    var categoryNode = new EntryNode();
                    categoryNode.entry.EntryIcon = tagGroup.Icon;
                    categoryNode.entry.DisplayName = tagGroup.DisplayName;
                    pageNode.childNodes.Add(categoryNode);
                    OtherLoader.SpawnerEntriesByPath[path] = categoryNode;
                }
            }
        }
        
        private void PopulateEntryForSpawnerId(ItemSpawnerEntry spawnerEntry)
        {
            var page = GetPage(spawnerEntry);
            var category = GetCategory(spawnerEntry);
            var categoryPath = page + "/" + category;
            var categoryNode = OtherLoader.SpawnerEntriesByPath[categoryPath];
            
            if (OtherLoader.SpawnerEntriesByPath.ContainsKey(spawnerEntry.EntryPath))
            {
                OtherLoader.SpawnerEntriesByPath[spawnerEntry.EntryPath].entry = spawnerEntry;
            }
            else
            {
                var itemNode = new EntryNode(spawnerEntry);
                OtherLoader.SpawnerEntriesByPath[spawnerEntry.EntryPath] = itemNode;
                categoryNode.childNodes.Add(itemNode);
            }
        }

        private string GetPage(ItemSpawnerEntry spawnerEntry)
        {
            return spawnerEntry.EntryPath.Split('/').First();
        }

        private string GetCategory(ItemSpawnerEntry spawnerEntry)
        {
            return spawnerEntry.EntryPath.Split('/').Skip(1).First();
        }
    }
}
