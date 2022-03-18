using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace OtherLoader.Loaders
{
    public class ItemSpawnerIdLoader : BaseAssetLoader
    {
        public override IEnumerator LoadAssetsFromBundle(AssetBundle assetBundle, string bundleId)
        {
            return LoadAssetsFromBundle<ItemSpawnerID>(assetBundle, bundleId);
        }

        

        protected override void LoadAsset(UnityEngine.Object asset, string bundleId)
        {
            ItemSpawnerID spawnerId = asset as ItemSpawnerID;

            OtherLogger.Log("Adding Itemspawner ID! Category: " + spawnerId.Category + ", SubCategory: " + spawnerId.SubCategory, OtherLogger.LogType.Loading);

            PopulateMissingMainObject(spawnerId);
            UpdateUnlockCostForItem(spawnerId);
            UpdateUnlockStatusForItem(spawnerId);
            IM.RegisterItemIntoMetaTagSystem(spawnerId);
            UpdateVisibilityForItem(spawnerId);

            if (IM.CD.ContainsKey(spawnerId.Category) && IM.SCD.ContainsKey(spawnerId.SubCategory))
            {
                AddSpawnerIdToGlobalDictionaries(spawnerId);

                if (!IM.Instance.SpawnerIDDic.ContainsKey(spawnerId.ItemID))
                {
                    IM.Instance.SpawnerIDDic[spawnerId.ItemID] = spawnerId;
                    AddSpawnerIdToNewSpawner(spawnerId);
                }
            }

            else
            {
                OtherLogger.LogError("ItemSpawnerID could not be added, because either the main category or subcategory were not loaded! Item will not appear in the itemspawner! Item Display Name: " + id.DisplayName);
            }

        }


        private void AddSpawnerIdToNewSpawner(ItemSpawnerID spawnerId)
        {
            ItemSpawnerEntry SpawnerEntry = ScriptableObject.CreateInstance<ItemSpawnerEntry>();

            if (IsCustomCategory(spawnerId.Category))
            {
                OtherLogger.Log("Adding SpawnerID to spawner entry tree under custom category", OtherLogger.LogType.Loading);
                SpawnerEntry.LegacyPopulateFromID(ItemSpawnerV2.PageMode.Firearms, spawnerId, true);
                PopulateEntryPaths(SpawnerEntry, spawnerId);
            }

            //TODO this is where left off
            else
            {
                //TODO this should be done without having to loop through potentially all spawner entries, I bet this could become expensive
                bool added = false;
                foreach (KeyValuePair<ItemSpawnerV2.PageMode, List<string>> pageItems in IM.Instance.PageItemLists)
                {
                    if (pageItems.Value.Contains(id.ItemID))
                    {
                        OtherLogger.Log("Adding SpawnerID to spawner entry tree", OtherLogger.LogType.Loading);
                        SpawnerEntry.LegacyPopulateFromID(pageItems.Key, id, true);
                        PopulateEntryPaths(SpawnerEntry, id);
                        added = true;

                        break;
                    }
                }

                if (added) continue;

                //If we make it to this point, we failed to add the entry to the tree structure, but should still populate the entries data
                OtherLogger.Log("ItemSpawnerID could not be converted for new spawner because of metadata issues! ItemID: " + id.ItemID, OtherLogger.LogType.Loading);
                SpawnerEntry.LegacyPopulateFromID(ItemSpawnerV2.PageMode.Firearms, id, true);
            }
        }

        private bool IsCustomCategory(ItemSpawnerID.EItemCategory category)
        {
            return Enum.IsDefined(typeof(ItemSpawnerID.EItemCategory), category);
        }


        

        private void UpdateUnlockCostForItem(ItemSpawnerID spawnerId)
        {
            if (spawnerId.UnlockCost == 0)
            {
                spawnerId.UnlockCost = spawnerId.MainObject.CreditCost;
            }
        }

        private void UpdateUnlockStatusForItem(ItemSpawnerID spawnerId)
        {
            if (!spawnerId.IsReward && OtherLoader.UnlockSaveData.AutoUnlockNonRewards)
            {
                OtherLoader.UnlockSaveData.UnlockItem(spawnerId.MainObject.ItemID);
            }
        }

        private void UpdateVisibilityForItem(ItemSpawnerID spawnerId)
        {
            if (!spawnerId.IsDisplayedInMainEntry)
            {
                HideItemFromCategories(spawnerId);
            }
        }

        private void HideItemFromCategories(ItemSpawnerID spawnerId)
        {
            foreach (List<string> pageItems in IM.Instance.PageItemLists.Values)
            {
                pageItems.Remove(spawnerId.MainObject.ItemID);
            }
        }

        private void AddSpawnerIdToGlobalDictionaries(ItemSpawnerID spawnerId)
        {
            IM.CD[spawnerId.Category].Add(spawnerId);
            IM.SCD[spawnerId.SubCategory].Add(spawnerId);
        }


        private void PopulateMissingMainObject(ItemSpawnerID spawnerId)
        {
            if (spawnerId.MainObject == null)
            {
                spawnerId.MainObject = spawnerId.Secondaries.Select(o => o.MainObject).FirstOrDefault(o => o != null);

                if (spawnerId.MainObject == null) 
                {
                    throw new NullReferenceException("Could not select a secondary object for ItemSpawnerID, it will not appear in spawner: Display Name: " + spawnerId.DisplayName);
                }

                spawnerId.ItemID = spawnerId.MainObject.ItemID;
            }
        }



        public static void PopulateEntryPaths(ItemSpawnerEntry entry, ItemSpawnerID spawnerID = null)
        {
            string[] pathSegments = entry.EntryPath.Split('/');
            string currentPath = "";

            for (int i = 0; i < pathSegments.Length; i++)
            {
                //If we are at the full path length for this entry, we can just assign the entry
                if (i == pathSegments.Length - 1)
                {
                    EntryNode previousNode = OtherLoader.SpawnerEntriesByPath[currentPath];
                    currentPath += (i == 0 ? "" : "/") + pathSegments[i];

                    //If there is already an node at this path, we should just update it. Otherwise, add it as a new node
                    EntryNode node;
                    if (OtherLoader.SpawnerEntriesByPath.ContainsKey(currentPath))
                    {
                        node = OtherLoader.SpawnerEntriesByPath[currentPath];
                        node.entry = entry;
                    }
                    else
                    {
                        node = new EntryNode(entry);
                        OtherLoader.SpawnerEntriesByPath[currentPath] = node;
                        previousNode.childNodes.Add(node);
                    }
                }


                //If we are at the page level, just check to see if we need to add a page node
                else if (i == 0)
                {
                    currentPath += (i == 0 ? "" : "/") + pathSegments[i];

                    if (!OtherLoader.SpawnerEntriesByPath.ContainsKey(currentPath))
                    {
                        EntryNode pageNode = new EntryNode();
                        pageNode.entry.EntryPath = currentPath;
                        OtherLoader.SpawnerEntriesByPath[currentPath] = pageNode;
                    }
                }

                //If these are just custom categories of any depth, just add the ones that aren't already loaded
                else
                {
                    EntryNode previousNode = OtherLoader.SpawnerEntriesByPath[currentPath];
                    currentPath += (i == 0 ? "" : "/") + pathSegments[i];

                    if (!OtherLoader.SpawnerEntriesByPath.ContainsKey(currentPath))
                    {
                        EntryNode node = new EntryNode();
                        node.entry.EntryPath = currentPath;
                        node.entry.IsDisplayedInMainEntry = true;

                        //Now this section below is for legacy support
                        if (spawnerID != null)
                        {
                            //For some legacy categories, we must perform this disgustingly bad search for their icons
                            if (i == 1 &&
                                (spawnerID.Category == ItemSpawnerID.EItemCategory.MeatFortress ||
                                spawnerID.Category == ItemSpawnerID.EItemCategory.Magazine ||
                                spawnerID.Category == ItemSpawnerID.EItemCategory.Cartridge ||
                                spawnerID.Category == ItemSpawnerID.EItemCategory.Clip ||
                                spawnerID.Category == ItemSpawnerID.EItemCategory.Speedloader))
                            {

                                foreach (ItemSpawnerCategoryDefinitionsV2.SpawnerPage page in IM.CatDef.Pages)
                                {
                                    foreach (ItemSpawnerCategoryDefinitionsV2.SpawnerPage.SpawnerTagGroup tagGroup in page.TagGroups)
                                    {
                                        if (tagGroup.TagT == TagType.Category && tagGroup.Tag == spawnerID.Category.ToString())
                                        {
                                            node.entry.EntryIcon = tagGroup.Icon;
                                            node.entry.DisplayName = tagGroup.DisplayName;
                                        }
                                    }
                                }

                            }

                            //If this is a modded main category, do that
                            else if (i == 1 && !Enum.IsDefined(typeof(ItemSpawnerID.EItemCategory), spawnerID.Category))
                            {
                                if (IM.CDefInfo.ContainsKey(spawnerID.Category))
                                {
                                    node.entry.EntryIcon = IM.CDefInfo[spawnerID.Category].Sprite;
                                    node.entry.DisplayName = IM.CDefInfo[spawnerID.Category].DisplayName;
                                }
                            }

                            //If this is a subcategory (modded or not), do that
                            else if (IM.CDefSubInfo.ContainsKey(spawnerID.SubCategory))
                            {
                                node.entry.EntryIcon = IM.CDefSubInfo[spawnerID.SubCategory].Sprite;
                                node.entry.DisplayName = IM.CDefSubInfo[spawnerID.SubCategory].DisplayName;
                            }

                            node.entry.IsModded = IM.OD[spawnerID.MainObject.ItemID].IsModContent;
                        }

                        previousNode.childNodes.Add(node);
                        OtherLoader.SpawnerEntriesByPath[currentPath] = node;
                    }
                }
            }
        }

    }
}
