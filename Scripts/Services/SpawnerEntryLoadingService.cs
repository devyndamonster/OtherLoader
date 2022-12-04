﻿using FistVR;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OtherLoader.Services
{
    public class SpawnerEntryLoadingService : ISpawnerEntryLoadingService
    {
        private readonly IPathService _pathService;

        public SpawnerEntryLoadingService(IPathService pathService)
        {
            _pathService = pathService;
        }

        public ItemSpawnerID ConvertSpawnerEntryToSpawnerId(ItemSpawnerEntry spawnerEntry)
        {
            ItemSpawnerID.ESubCategory subcategory = spawnerEntry.GetSpawnerSubcategory();

            if (!IM.OD.ContainsKey(spawnerEntry.MainObjectID) || spawnerEntry.IsCategoryEntry())
            {
                return null;
            }

            ItemSpawnerID itemSpawnerID = ScriptableObject.CreateInstance<ItemSpawnerID>();

            foreach (ItemSpawnerCategoryDefinitions.Category category in IM.CDefs.Categories)
            {
                if (category.Subcats.Any(o => o.Subcat == subcategory))
                {
                    itemSpawnerID.Category = category.Cat;
                    itemSpawnerID.SubCategory = subcategory;
                }
            }

            itemSpawnerID.MainObject = IM.OD[spawnerEntry.MainObjectID];
            itemSpawnerID.SecondObject = spawnerEntry.SpawnWithIDs.Where(o => IM.OD.ContainsKey(o)).Select(o => IM.OD[o]).FirstOrDefault();
            itemSpawnerID.DisplayName = spawnerEntry.DisplayName;
            itemSpawnerID.IsDisplayedInMainEntry = spawnerEntry.IsDisplayedInMainEntry;
            itemSpawnerID.ItemID = spawnerEntry.MainObjectID;
            itemSpawnerID.ModTags = spawnerEntry.ModTags;
            itemSpawnerID.Secondaries_ByStringID = spawnerEntry.SecondaryObjectIDs.Where(o => IM.OD.ContainsKey(o)).ToList();
            itemSpawnerID.Secondaries = new ItemSpawnerID[] { };
            itemSpawnerID.Sprite = spawnerEntry.EntryIcon;
            itemSpawnerID.UsesLargeSpawnPad = spawnerEntry.UsesLargeSpawnPad;
            itemSpawnerID.UsesHugeSpawnPad = spawnerEntry.UsesHugeSpawnPad;
            itemSpawnerID.IsReward = spawnerEntry.IsReward;
            itemSpawnerID.TutorialBlocks = spawnerEntry.TutorialBlockIDs;

            return itemSpawnerID;
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
