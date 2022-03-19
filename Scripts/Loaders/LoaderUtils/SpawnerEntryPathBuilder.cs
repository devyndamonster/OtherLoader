using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Loaders
{
    public class SpawnerEntryPathBuilder
    {
        private string[] allPathSegments;
        private int segmentIndex;


        public void PopulateEntryPaths(ItemSpawnerEntry entry, ItemSpawnerID spawnerID = null)
        {
            allPathSegments = entry.EntryPath.Split('/').ToArray();

            for (segmentIndex = 0; segmentIndex < allPathSegments.Length; segmentIndex++)
            {
                //If we are at the full path length for this entry, we can just assign the entry
                if (IsLastSegment())
                {
                    AddSpawnerEntry(entry);
                }

                //If we are at the page level, just check to see if we need to add a page node
                else if (IsFirstSegment())
                {
                    TryAddPageNode();
                }

                //If these are just custom categories of any depth, just add the ones that aren't already loaded
                else
                {
                    TryAddMiddleNode(spawnerID);
                }
            }
        }



        private void AddSpawnerEntry(ItemSpawnerEntry entry)
        {
            string currentPath = GetCurrentPath();
            string previousPath = GetPreviousPath();
            EntryNode previousNode = OtherLoader.SpawnerEntriesByPath[previousPath];
            EntryNode finalNode;

            if (OtherLoader.SpawnerEntriesByPath.ContainsKey(currentPath))
            {
                finalNode = OtherLoader.SpawnerEntriesByPath[currentPath];
                finalNode.entry = entry;
            }
            else
            {
                finalNode = new EntryNode(entry);
                OtherLoader.SpawnerEntriesByPath[currentPath] = finalNode;
                previousNode.childNodes.Add(finalNode);
            }
        }


        private void TryAddPageNode()
        {
            string currentPath = GetCurrentPath();

            if (!OtherLoader.SpawnerEntriesByPath.ContainsKey(currentPath))
            {
                EntryNode pageNode = new EntryNode();
                pageNode.entry.EntryPath = currentPath;
                OtherLoader.SpawnerEntriesByPath[currentPath] = pageNode;
            }
        }


        private void TryAddMiddleNode(ItemSpawnerID spawnerId)
        {
            string currentPath = GetCurrentPath();
            string previousPath = GetPreviousPath();
            EntryNode previousNode = OtherLoader.SpawnerEntriesByPath[previousPath];

            if (!OtherLoader.SpawnerEntriesByPath.ContainsKey(currentPath))
            {
                //Only base information is filled out when creating middle entries
                //Assumption is that another entry will come along and fill this information out eventually
                EntryNode node = new EntryNode();
                node.entry.EntryPath = currentPath;
                node.entry.IsDisplayedInMainEntry = true;

                if (spawnerId != null)
                {
                    PopulateNodeFromSpawnerId(spawnerId, node);
                }

                previousNode.childNodes.Add(node);
                OtherLoader.SpawnerEntriesByPath[currentPath] = node;
            }
        }

        private void PopulateNodeFromSpawnerId(ItemSpawnerID spawnerId, EntryNode node)
        {
            if (IsCategorySegment() && RequiresCategorySearch(spawnerId))
            {
                PopulateNodeFromCategorySearch(spawnerId, node);
            }

            else if (IsCategorySegment() && IsModdedCategory(spawnerId.Category))
            {
                PopulateNodeFromCategory(spawnerId, node);
            }

            else 
            {
                PopulateNodeFromSubcategory(spawnerId, node);
            }

            node.entry.IsModded = IM.OD[spawnerId.MainObject.ItemID].IsModContent;
        }


        private void PopulateNodeFromCategorySearch(ItemSpawnerID spawnerId, EntryNode node)
        {
            //For some legacy categories, we must perform this disgustingly bad search for their icons
            foreach (ItemSpawnerCategoryDefinitionsV2.SpawnerPage page in IM.CatDef.Pages)
            {
                foreach (ItemSpawnerCategoryDefinitionsV2.SpawnerPage.SpawnerTagGroup tagGroup in page.TagGroups)
                {
                    if (tagGroup.TagT == TagType.Category && tagGroup.Tag == spawnerId.Category.ToString())
                    {
                        node.entry.EntryIcon = tagGroup.Icon;
                        node.entry.DisplayName = tagGroup.DisplayName;
                    }
                }
            }
        }


        private void PopulateNodeFromCategory(ItemSpawnerID spawnerId, EntryNode node)
        {
            if (IM.CDefInfo.ContainsKey(spawnerId.Category))
            {
                node.entry.EntryIcon = IM.CDefInfo[spawnerId.Category].Sprite;
                node.entry.DisplayName = IM.CDefInfo[spawnerId.Category].DisplayName;
            }
        }

        private void PopulateNodeFromSubcategory(ItemSpawnerID spawnerId, EntryNode node)
        {
            if (IM.CDefSubInfo.ContainsKey(spawnerId.SubCategory))
            {
                node.entry.EntryIcon = IM.CDefSubInfo[spawnerId.SubCategory].Sprite;
                node.entry.DisplayName = IM.CDefSubInfo[spawnerId.SubCategory].DisplayName;
            }
        }


        private bool RequiresCategorySearch(ItemSpawnerID spawnerId)
        {
            return spawnerId.Category == ItemSpawnerID.EItemCategory.MeatFortress ||
                spawnerId.Category == ItemSpawnerID.EItemCategory.Magazine ||
                spawnerId.Category == ItemSpawnerID.EItemCategory.Cartridge ||
                spawnerId.Category == ItemSpawnerID.EItemCategory.Clip ||
                spawnerId.Category == ItemSpawnerID.EItemCategory.Speedloader;
        }



        private bool IsModdedCategory(ItemSpawnerID.EItemCategory category)
        {
            return !Enum.IsDefined(typeof(ItemSpawnerID.EItemCategory), category);
        }

        private bool IsLastSegment()
        {
            return segmentIndex == allPathSegments.Length - 1;
        }

        private bool IsFirstSegment()
        {
            return segmentIndex == 0;
        }

        private bool IsCategorySegment()
        {
            return segmentIndex == 1;
        }

        private string GetCurrentPath()
        {
            return GetPathFromCount(segmentIndex + 1);
        }

        private string GetPreviousPath()
        {
            return GetPathFromCount(segmentIndex);
        }

        private string GetPathFromCount(int segmentCount)
        {
            return string.Join("/", allPathSegments.Take(segmentCount).ToArray());
        }

    }
}
