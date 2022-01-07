using FistVR;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace OtherLoader
{
    public class ItemSpawnerV2Patch
    {

        [HarmonyPatch(typeof(ItemSpawnerV2), "Awake")]
        [HarmonyPrefix]
        private static bool BeforeAwakePatch(ItemSpawnerV2 __instance)
        {
            __instance.StartCoroutine(HandleLoadingText(__instance));

            __instance.gameObject.AddComponent<ItemSpawnerData>();

            __instance.TXT_Detail.resizeTextForBestFit = true;
            __instance.TXT_Detail.resizeTextMaxSize = __instance.TXT_Detail.fontSize;
            __instance.TXT_Detail.resizeTextMinSize = 2;
            __instance.TXT_Detail.verticalOverflow = VerticalWrapMode.Truncate;
            __instance.TXT_Detail.rectTransform.anchoredPosition = new Vector2(390, 500);
            __instance.TXT_Detail.rectTransform.sizeDelta = new Vector2(310, 390);

            Image backing = __instance.TXT_Detail.transform.GetComponentInChildren<Image>();
            backing.rectTransform.anchoredPosition = new Vector2(0, 10);

            return true;
        }



        private static IEnumerator HandleLoadingText(ItemSpawnerV2 instance)
        {
            //First create the loading text
            Text text = CreateLoadingText(instance);

            //Now loop until all items are loaded, while updating the text
            float progress = LoaderStatus.GetLoaderProgress();
            while (progress < 1)
            {
                string progressBar = new string('I', (int)(progress * 120));
                text.text = "Loading Mods\n" + progressBar;

                yield return new WaitForSeconds(1);

                progress = LoaderStatus.GetLoaderProgress();
            }

            //Finally destroy the text
            GameObject.Destroy(text.transform.parent.gameObject);
        }


        private static Text CreateLoadingText(ItemSpawnerV2 instance)
        {
            GameObject loadingCanvas = new GameObject("LoadingTextCanvas");
            loadingCanvas.transform.SetParent(instance.transform);
            loadingCanvas.transform.rotation = instance.transform.rotation;
            loadingCanvas.transform.localPosition = new Vector3(-0.47f, 0.492f, 0);

            Canvas canvasComp = loadingCanvas.AddComponent<Canvas>();
            RectTransform rect = canvasComp.GetComponent<RectTransform>();
            canvasComp.renderMode = RenderMode.WorldSpace;
            rect.sizeDelta = new Vector2(1, 1);

            GameObject text = new GameObject("LoadingText");
            text.transform.SetParent(loadingCanvas.transform);
            text.transform.rotation = instance.transform.rotation;
            text.transform.localPosition = new Vector3(-0.25f, 0.4f, 0);

            text.AddComponent<CanvasRenderer>();
            Text textComp = text.AddComponent<Text>();
            Font ArialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");

            textComp.text = "Loading Mods\n";
            textComp.alignment = TextAnchor.MiddleLeft;
            textComp.fontSize = 32;
            text.transform.localScale = new Vector3(0.0015f, 0.0015f, 0.0015f);
            textComp.font = ArialFont;
            textComp.horizontalOverflow = HorizontalWrapMode.Overflow;

            return textComp;
        }


        [HarmonyPatch(typeof(ItemSpawnerV2), "BTN_Details_Spawn")]
        [HarmonyPrefix]
        private static bool SpawnItemDetails(ItemSpawnerV2 __instance)
        {
            OtherLogger.Log("Trying to spawn: " + __instance.m_selectedID, OtherLogger.LogType.General);

            //If the selected item has a spawner entry, use that
            if (OtherLoader.SpawnerEntriesByID.ContainsKey(__instance.m_selectedID))
            {
                OtherLogger.Log("Using normal spawn", OtherLogger.LogType.General);

                __instance.Boop(1);
                AnvilManager.Run(SpawnItems(__instance, OtherLoader.SpawnerEntriesByID[__instance.m_selectedID]));
            }
            
            //Otherwise try to use legacy spawner ID
            else if (IM.HasSpawnedID(__instance.m_selectedID))
            {
                OtherLogger.Log("Using legacy spawn", OtherLogger.LogType.General);

                return true;
            }

            else
            {
                __instance.Boop(2);
            }

            return false;
        }


        [HarmonyPatch(typeof(ItemSpawnerV2), "BTN_Details_SpawnRelated")]
        [HarmonyPrefix]
        private static bool SpawnItemRelated(ItemSpawnerV2 __instance, int i)
        {
            //If the selected item has a spawner entry, use that
            if (OtherLoader.SpawnerEntriesByID.ContainsKey(__instance.m_selectedID))
            {
                __instance.Boop(1);

                ItemSpawnerData data = __instance.GetComponent<ItemSpawnerData>();
                AnvilManager.Run(SpawnItems(__instance, data.VisibleSecondaryEntries[i]));
            }

            //Otherwise try to use legacy spawner ID
            else if (IM.HasSpawnedID(__instance.m_selectedID))
            {
                return true;
            }

            else
            {
                __instance.Boop(2);
            }

            return false;
        }




        [HarmonyPatch(typeof(ItemSpawnerV2), "SetPageMode")]
        [HarmonyPrefix]
        private static bool PageModePatch(ItemSpawnerV2 __instance, int i)
        {
            ItemSpawnerData data = __instance.GetComponent<ItemSpawnerData>();
            data.CurrentPath = ((ItemSpawnerV2.PageMode)i).ToString();
            data.CurrentDepth = 0;
            data.SavedPagePositions[(ItemSpawnerV2.PageMode)i] = new Dictionary<int, int>();

            data.SavedPagePositions[(ItemSpawnerV2.PageMode)i][data.CurrentDepth] = 0;

            return true;
        }



        [HarmonyPatch(typeof(ItemSpawnerV2), "SimpleGoBack")]
        [HarmonyPrefix]
        private static bool GoBackPatch(ItemSpawnerV2 __instance)
        {
            ItemSpawnerData data = __instance.GetComponent<ItemSpawnerData>();
            if (!data.CurrentPath.Contains("/")) return false;

            data.CurrentPath = data.CurrentPath.Substring(0, data.CurrentPath.LastIndexOf("/"));
            data.SavedPagePositions[__instance.PMode][data.CurrentDepth] = 0;
            data.CurrentDepth -= 1;

            OtherLogger.Log("Going back to path: " + data.CurrentPath, OtherLogger.LogType.General);
            __instance.RedrawSimpleCanvas();

            return false;
        }


        [HarmonyPatch(typeof(ItemSpawnerV2), "SimpleNextPage")]
        [HarmonyPrefix]
        private static bool NextPagePatch(ItemSpawnerV2 __instance)
        {
            ItemSpawnerData data = __instance.GetComponent<ItemSpawnerData>();
            
            if(OtherLoader.SpawnerEntriesByPath[data.CurrentPath].childNodes.Count / __instance.IMG_SimpleTiles.Count > data.SavedPagePositions[__instance.PMode][data.CurrentDepth])
            {
                data.SavedPagePositions[__instance.PMode][data.CurrentDepth] += 1;
                __instance.RedrawSimpleCanvas();
            }

            return false;
        }


        [HarmonyPatch(typeof(ItemSpawnerV2), "SimplePrevage")]
        [HarmonyPrefix]
        private static bool PrevPagePatch(ItemSpawnerV2 __instance)
        {
            ItemSpawnerData data = __instance.GetComponent<ItemSpawnerData>();

            if (data.SavedPagePositions[__instance.PMode][data.CurrentDepth] > 0)
            {
                data.SavedPagePositions[__instance.PMode][data.CurrentDepth] -= 1;
                __instance.RedrawSimpleCanvas();
            }

            return false;
        }



        [HarmonyPatch(typeof(ItemSpawnerV2), "GetDetailText")]
        [HarmonyPrefix]
        private static bool DetailTextPatch(ItemSpawnerV2 __instance, string id, ref string __result)
        {
            FVRObject fvrObj;
            string spawnerCat;
            string spawnerSubcat;

            if (IM.Instance.SpawnerIDDic.ContainsKey(id))
            {
                OtherLogger.Log("Getting ID from spawnerID", OtherLogger.LogType.General);

                ItemSpawnerID spawnerID = IM.Instance.SpawnerIDDic[id];
                fvrObj = IM.OD[spawnerID.MainObject.ItemID];

                spawnerCat = spawnerID.Category.ToString();
                if (!Enum.IsDefined(typeof(ItemSpawnerID.EItemCategory), spawnerID.Category) && IM.CDefInfo.ContainsKey(spawnerID.Category))
                    spawnerCat = IM.CDefInfo[spawnerID.Category].DisplayName;

                spawnerSubcat = spawnerID.SubCategory.ToString();
                if (!Enum.IsDefined(typeof(ItemSpawnerID.ESubCategory), spawnerID.SubCategory) && IM.CDefSubInfo.ContainsKey(spawnerID.SubCategory))
                    spawnerSubcat = IM.CDefSubInfo[spawnerID.SubCategory].DisplayName;
            }

            else if (OtherLoader.SpawnerEntriesByID.ContainsKey(id))
            {
                OtherLogger.Log("Getting ID from otherloader", OtherLogger.LogType.General);

                spawnerCat = "None";
                spawnerSubcat = "None";

                fvrObj = IM.OD[id];
            }

            else
            {
                OtherLogger.LogError($"The ItemID was not found to have spawner entry! ItemID: {id}");
                __result = "";
                return false;
            }

            
            string text = 
                "Spawner Category: " + spawnerCat + "\n" +
                "Spawner Subcategory: " + spawnerSubcat + "\n" +
                "Object Category: " + fvrObj.Category.ToString() + "\n" +
                "Set: " + fvrObj.TagSet.ToString() + "\n" +
                "Size: " + fvrObj.TagFirearmSize.ToString() + "\n" +
                "Era: " + fvrObj.TagEra.ToString() + "\n" +
                "Action: " + fvrObj.TagFirearmAction.ToString() + "\n" +
                "Round Power: " + fvrObj.TagFirearmRoundPower.ToString() + "\n" +
                "Country: " + fvrObj.TagFirearmCountryOfOrigin.ToString() + "\n" +
                "Introduction Year: " + fvrObj.TagFirearmFirstYear.ToString() + "\n" +
                "Magazine Type: " + fvrObj.MagazineType.ToString() + "\n" +
                "Round Type: " + fvrObj.RoundType.ToString() + "\n" +
                "Firing Modes: " + string.Join(",", fvrObj.TagFirearmFiringModes.Select(o => o.ToString()).ToArray()) + "\n" +
                "Feed Options: " + string.Join(",", fvrObj.TagFirearmFeedOption.Select(o => o.ToString()).ToArray()) + "\n" +
                "Mounts: " + string.Join(",", fvrObj.TagFirearmMounts.Select(o => o.ToString()).ToArray()) + "\n" +
                "Attachment Mount: " + fvrObj.TagAttachmentMount.ToString() + "\n" +
                "Attachment Feature: " + fvrObj.TagAttachmentFeature.ToString();

            __result = text;

            return false;
        }




        [HarmonyPatch(typeof(ItemSpawnerV2), "SimpleSelectTile")]
        [HarmonyPrefix]
        private static bool SimpleButtonPatch(ItemSpawnerV2 __instance, int i)
        {
            ItemSpawnerData data = __instance.GetComponent<ItemSpawnerData>();

            //If the entry that was selected has child entries, we should display the child entries
            if (OtherLoader.SpawnerEntriesByPath[data.VisibleEntries[i].EntryPath].childNodes.Count > 0)
            {
                data.CurrentPath = data.VisibleEntries[i].EntryPath;
                data.CurrentDepth += 1;
                data.SavedPagePositions[__instance.PMode][data.CurrentDepth] = 0;
                
                __instance.RedrawSimpleCanvas();
            }

            else
            {
                OtherLogger.Log("Setting selected id to: " + data.VisibleEntries[i].MainObjectID, OtherLogger.LogType.General);

                __instance.SetSelectedID(data.VisibleEntries[i].MainObjectID);
                __instance.RedrawDetailsCanvas();
            }

            return false;
        }


        /// <summary>
        /// This method adapts the code for drawing the details canvas to use the new spawner entry system
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPatch(typeof(ItemSpawnerV2), "RedrawDetailsCanvas")]
        [HarmonyPrefix]
        private static bool RedrawDetailsCanvasPatch(ItemSpawnerV2 __instance)
        {
            OtherLogger.Log("Selected ID: " + __instance.m_selectedID, OtherLogger.LogType.General);

            //If there is no spawner entry for the selected ID, set everything to blank
            if (!OtherLoader.SpawnerEntriesByID.ContainsKey(__instance.m_selectedID))
            {
                return true;
            }


            else
            {
                ItemSpawnerEntry entry = OtherLoader.SpawnerEntriesByID[__instance.m_selectedID];
                ItemSpawnerData data = __instance.GetComponent<ItemSpawnerData>();

                OtherLogger.Log("We found an entry for it!", OtherLogger.LogType.General);

                //First, fill activate some of the detail and populate it with info
                for (int l = 0; l < __instance.IM_FavButtons.Count; l++)
                {
                    __instance.IM_FavButtons[l].gameObject.SetActive(true);
                }

                __instance.IM_Detail.gameObject.SetActive(true);
                __instance.IM_Detail.sprite = entry.EntryIcon;
                __instance.TXT_Title.text = entry.DisplayName;
                __instance.BTN_SpawnSelectedObject.SetActive(true);
                __instance.TXT_Detail.text = __instance.GetDetailText(__instance.m_selectedID);



                //Now get all the secondary entries
                List<ItemSpawnerEntry> secondaryEntries = new List<ItemSpawnerEntry>();
                for (int m = 0; m < entry.SecondaryObjectIDs.Count; m++)
                {
                    if (!OtherLoader.SpawnerEntriesByID.ContainsKey(entry.SecondaryObjectIDs[m])){
                        OtherLogger.LogWarning($"Secondary ID for ({entry.MainObjectID}) was not in entry dictionary! It will not appear! Secondary ID ({entry.SecondaryObjectIDs[m]})");
                        continue;
                    }

                    ItemSpawnerEntry secondary = OtherLoader.SpawnerEntriesByID[entry.SecondaryObjectIDs[m]];
                    if(!secondary.IsReward || GM.Rewards.RewardUnlocks.Rewards.Contains(secondary.MainObjectID))
                    {
                        secondaryEntries.Add(secondary);
                    }
                }


                //Now we create the secondaries page
                //Start by drawing the tiles
                data.VisibleSecondaryEntries.Clear();
                int startIndex = __instance.m_selectedIDRelatedPage * __instance.IM_DetailRelated.Count;
                for (int i = 0; i < __instance.IM_DetailRelated.Count; i++)
                {
                    if (startIndex + i < secondaryEntries.Count)
                    {
                        ItemSpawnerEntry secondaryEntry = secondaryEntries[startIndex + i];
                        data.VisibleSecondaryEntries.Add(secondaryEntry);

                        __instance.IM_DetailRelated[i].gameObject.SetActive(true);
                        __instance.IM_DetailRelated[i].sprite = secondaryEntry.EntryIcon;
                    }
                    else
                    {
                        __instance.IM_DetailRelated[i].gameObject.SetActive(false);
                    }
                }

                //Now handle the page selectors
                int numPages = (int)Math.Ceiling((double)secondaryEntries.Count / __instance.IM_DetailRelated.Count);
                __instance.TXT_DetailsRelatedPageNum.gameObject.SetActive(true);
                __instance.TXT_DetailsRelatedPageNum.text = (__instance.m_selectedIDRelatedPage + 1).ToString() + " / " + numPages.ToString();
                
                if (__instance.m_selectedIDRelatedPage > 0)
                {
                    __instance.BTN_DetailsRelatedPrevPage.SetActive(true);
                }
                else
                {
                    __instance.BTN_DetailsRelatedPrevPage.SetActive(false);
                }

                if (__instance.m_selectedIDRelatedPage < numPages - 1)
                {
                    __instance.BTN_DetailsRelatedNextPage.SetActive(true);
                }
                else
                {
                    __instance.BTN_DetailsRelatedNextPage.SetActive(false);
                }



                //Setup the tutorials panel
                for (int i = 0; i < __instance.BTNS_DetailTutorial.Count; i++)
                {
                    if (i < entry.TutorialBlockIDs.Count)
                    {
                        if (IM.TutorialBlockDic.ContainsKey(entry.TutorialBlockIDs[i]))
                        {
                            __instance.BTNS_DetailTutorial[i].gameObject.SetActive(true);
                            __instance.BTNS_DetailTutorial[i].text = IM.TutorialBlockDic[entry.TutorialBlockIDs[i]].Title;
                        }
                        else
                        {
                            __instance.BTNS_DetailTutorial[i].gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        __instance.BTNS_DetailTutorial[i].gameObject.SetActive(false);
                    }
                }



                //Setup the favorites icons
                for (int i = 0; i < __instance.IM_FavButtons.Count; i++)
                {
                    if (ManagerSingleton<IM>.Instance.ItemMetaDic.ContainsKey(__instance.m_selectedID) && ManagerSingleton<IM>.Instance.ItemMetaDic[__instance.m_selectedID].ContainsKey(TagType.Favorites) && ManagerSingleton<IM>.Instance.ItemMetaDic[__instance.m_selectedID][TagType.Favorites].Contains(__instance.FaveTags[i]))
                    {
                        __instance.IM_FavButtons[i].sprite = __instance.IM_FavButton_Faved[i];
                    }
                    else
                    {
                        __instance.IM_FavButtons[i].sprite = __instance.IM_FavButton_UnFaved[i];
                    }
                }
            }


            return false;
        }



        [HarmonyPatch(typeof(ItemSpawnerV2), "RedrawSimpleCanvas")]
        [HarmonyPrefix]
        private static bool RedrawSimplePatch(ItemSpawnerV2 __instance)
        {
            if (__instance.PMode == ItemSpawnerV2.PageMode.MainMenu) return false;

            ItemSpawnerData data = __instance.GetComponent<ItemSpawnerData>();
            data.VisibleEntries.Clear();

            List<EntryNode> entries = OtherLoader.SpawnerEntriesByPath[data.CurrentPath].childNodes.Where(o => o.entry.IsDisplayedInMainEntry).ToList();

            OtherLogger.Log($"Got {entries.Count} entries for path: {data.CurrentPath}", OtherLogger.LogType.General);

            entries = entries.OrderBy(o => o.entry.DisplayName).OrderBy(o => o.entry.IsModded?1:0).OrderBy(o => o.childNodes.Count > 0?0:1).ToList();

            int currPage = data.SavedPagePositions[__instance.PMode][data.CurrentDepth];
            int startIndex = currPage * __instance.IMG_SimpleTiles.Count;
            for (int i = 0; i < __instance.IMG_SimpleTiles.Count; i++)
            {
                if(startIndex + i < entries.Count)
                {
                    ItemSpawnerEntry entry = entries[startIndex + i].entry;
                    data.VisibleEntries.Add(entry);

                    __instance.IMG_SimpleTiles[i].gameObject.SetActive(true);
                    __instance.TXT_SimpleTiles[i].gameObject.SetActive(true);
                    __instance.IMG_SimpleTiles[i].sprite = entry.EntryIcon;
                    __instance.TXT_SimpleTiles[i].text = entry.DisplayName;
                }
                else
                {
                    __instance.IMG_SimpleTiles[i].gameObject.SetActive(false);
                    __instance.TXT_SimpleTiles[i].gameObject.SetActive(false);
                }
            }

            int numPages = (int) Math.Ceiling((double) entries.Count / __instance.IMG_SimpleTiles.Count);

            OtherLogger.Log($"There are {numPages} pages for this entry", OtherLogger.LogType.General);

            __instance.TXT_SimpleTiles_PageNumber.text = (currPage + 1) + " / " + (numPages);
            __instance.TXT_SimpleTiles_Showing.text = 
                "Showing " + 
                (currPage * __instance.IMG_SimpleTiles.Count) + 
                " - " + 
                (currPage * __instance.IMG_SimpleTiles.Count + data.VisibleEntries.Count) +
                " Of " +
                entries.Count;


            if(currPage > 0)
            {
                __instance.GO_SimpleTiles_PrevPage.SetActive(true);
            }
            else
            {
                __instance.GO_SimpleTiles_PrevPage.SetActive(false);
            }

            if(currPage < numPages - 1)
            {
                __instance.GO_SimpleTiles_NextPage.SetActive(true);
            }
            else
            {
                __instance.GO_SimpleTiles_NextPage.SetActive(false);
            }

            return false;
        }



        [HarmonyPatch(typeof(ItemSpawnerV2), "RedrawListCanvas")]
        [HarmonyPrefix]
        private static bool RedrawListPatch(ItemSpawnerV2 __instance)
        {
            if (__instance.PMode == ItemSpawnerV2.PageMode.MainMenu) return false;

            //Ensure that the selected tags list contains values for every tag type
            //TODO this should be done in the place where the selected tags list is assigned, but that will require a new patch
            if (!__instance.m_selectedTags.ContainsKey(__instance.PMode)) __instance.m_selectedTags[__instance.PMode] = new Dictionary<TagType, List<string>>();
            foreach (TagType tagType in Enum.GetValues(typeof(TagType)))
            {
                if (!__instance.m_selectedTags[__instance.PMode].ContainsKey(tagType)) __instance.m_selectedTags[__instance.PMode][tagType] = new List<string>();
            }

            Dictionary<FVRObject, ItemSpawnerEntry> entries = GetEntryPairsFromQuery(IM.Instance.PageItemLists[__instance.PMode], __instance.m_selectedTags[__instance.PMode]);

            __instance.WorkingItemIDs.Clear();
            __instance.WorkingItemIDs.AddRange(entries.Keys.Select(o => o.ItemID));
            __instance.WorkingItemIDs.Sort();
            __instance.m_displayedItemIds.Clear();

            //Assign values based on the display mode of the spawner
            int numItems = __instance.WorkingItemIDs.Count;
            int numPages = 1;
            int entriesPerPage;
            List<Image> images;
            List<Text> texts;

            if (__instance.LDMode == ItemSpawnerV2.ListDisplayMode.Text)
            {
                __instance.GO_List.SetActive(true);
                __instance.GO_Grid.SetActive(false);

                entriesPerPage = 22;
                images = __instance.IM_List;
                texts = __instance.TXT_List;
            }
            else
            {
                __instance.GO_List.SetActive(false);
                __instance.GO_Grid.SetActive(true);

                entriesPerPage = 12;
                images = __instance.IM_Grid;
                texts = __instance.TXT_Grid;
            }


            //If there are no items from our query, let the use know
            if (numItems == 0)
            {
                for (int l = 0; l < images.Count; l++)
                {
                    __instance.m_curListPageNum[__instance.PMode] = 0;
                    images[l].gameObject.SetActive(false);
                    texts[l].gameObject.SetActive(false);
                }
                __instance.TXT_ListPage.text = string.Empty;
                __instance.TXT_ListShowing.text = "No Items Match All Selected Tags";
            }

            else
            {
                numPages = Mathf.Max(Mathf.CeilToInt((float)numItems / entriesPerPage), 1);

                if (__instance.m_curListPageNum[__instance.PMode] >= numPages)
                {
                    __instance.m_curListPageNum[__instance.PMode] = numPages - 1;
                }

                int startIndex = __instance.m_curListPageNum[__instance.PMode] * entriesPerPage;
                int nextPageStartIndex = (__instance.m_curListPageNum[__instance.PMode] + 1) * entriesPerPage;
                int currentIndex = startIndex;

                for (int m = 0; m < images.Count; m++)
                {
                    if (currentIndex >= numItems)
                    {
                        images[m].gameObject.SetActive(false);
                        texts[m].gameObject.SetActive(false);
                    }
                    else
                    {
                        ItemSpawnerEntry entry = OtherLoader.SpawnerEntriesByID[__instance.WorkingItemIDs[currentIndex]];
                        __instance.m_displayedItemIds.Add(entry.MainObjectID);
                        images[m].sprite = entry.EntryIcon;
                        images[m].gameObject.SetActive(true);
                        texts[m].text = entry.DisplayName;
                        texts[m].gameObject.SetActive(true);
                    }

                    currentIndex++;
                }

                __instance.TXT_ListShowing.text = string.Concat(new object[]
                {
                    "Showing ",
                    startIndex,
                    " - ",
                    Mathf.Min(nextPageStartIndex, numItems),
                    " of ",
                    numItems
                });
            }


            //Now perform logic for displaying page buttons
            __instance.TXT_ListPage.text = (__instance.m_curListPageNum[__instance.PMode] + 1).ToString() + " / " + numPages.ToString();
            if (__instance.m_curListPageNum[__instance.PMode] > 0)
            {
                __instance.BTN_ListPagePrev.SetActive(true);
            }
            else
            {
                __instance.BTN_ListPagePrev.SetActive(false);
            }
            if (__instance.m_curListPageNum[__instance.PMode] < numPages - 1)
            {
                __instance.BTN_ListPageNext.SetActive(true);
            }
            else
            {
                __instance.BTN_ListPageNext.SetActive(false);
            }

            return false;
        }




        private static Dictionary<FVRObject, ItemSpawnerEntry> GetEntryPairsFromQuery(List<string> itemIDs, Dictionary<TagType, List<string>> tagQuery)
        {
            //Create a dictionary and populate it with items that have an FVRObject and a spawner entry
            Dictionary<FVRObject, ItemSpawnerEntry> entryDic = new Dictionary<FVRObject, ItemSpawnerEntry>();
            for (int i = 0; i < itemIDs.Count; i++)
            {
                string itemID = itemIDs[i];


                //First, ensure that this item ID has all necessary information (FVRObject and SpawnerEntry
                FVRObject item;
                IM.OD.TryGetValue(itemID, out item);

                //For vanilla items, the itemID might not be the object ID (out of my control)
                //So if the item is still null, try to get it through an itemspawnerID
                if(item == null)
                {
                    ItemSpawnerID ID;
                    IM.Instance.SpawnerIDDic.TryGetValue(itemID, out ID);
                    if(ID != null)
                    {
                        item = ID.MainObject;
                    }
                }
                if (item == null) continue;


                ItemSpawnerEntry entry;
                OtherLoader.SpawnerEntriesByID.TryGetValue(item.ItemID, out entry);
                if (entry == null) continue;


                //Now, Decide wether this item will be displayed
                if (!entry.IsDisplayedInMainEntry) continue;

                else if(!ShouldIncludeGeneral(item, entry, tagQuery)) continue;

                else if (item.Category == FVRObject.ObjectCategory.Firearm && !ShouldIncludeFirearm(item, tagQuery)) continue;

                else if (item.Category == FVRObject.ObjectCategory.Attachment && !ShouldIncludeAttachment(item, tagQuery)) continue;

                else if (item.Category == FVRObject.ObjectCategory.Magazine && !ShouldIncludeMagazine(item, tagQuery)) continue;

                entryDic[item] = entry;
            }

            return entryDic;
        }


        private static bool ShouldIncludeGeneral(FVRObject item, ItemSpawnerEntry entry, Dictionary<TagType, List<string>> tagQuery)
        {
            if (tagQuery[TagType.SubCategory].Count > 0 && !tagQuery[TagType.SubCategory].Contains(entry.EntryPath.Split('/')[1])) return false;

            if (tagQuery[TagType.Set].Count > 0 && !tagQuery[TagType.Set].Contains(item.TagSet.ToString())) return false;

            if (tagQuery[TagType.Era].Count > 0 && !tagQuery[TagType.Era].Contains(item.TagEra.ToString())) return false;

            if (tagQuery[TagType.ModTag].Count > 0 && !entry.ModTags.Any(o => tagQuery[TagType.ModTag].Contains(o.ToString()))) return false;

            return true;
        }

        private static bool ShouldIncludeFirearm(FVRObject item, Dictionary<TagType, List<string>> tagQuery)
        {
            if (tagQuery[TagType.Size].Count > 0 && !tagQuery[TagType.Size].Contains(item.TagFirearmSize.ToString())) return false;

            if (tagQuery[TagType.Action].Count > 0 && !tagQuery[TagType.Action].Contains(item.TagFirearmAction.ToString())) return false;

            if (tagQuery[TagType.RoundClass].Count > 0 && !tagQuery[TagType.RoundClass].Contains(item.TagFirearmRoundPower.ToString())) return false;

            if (tagQuery[TagType.CountryOfOrigin].Count > 0 && !tagQuery[TagType.CountryOfOrigin].Contains(item.TagFirearmCountryOfOrigin.ToString())) return false;

            if (tagQuery[TagType.IntroductionYear].Count > 0 && !tagQuery[TagType.IntroductionYear].Contains(item.TagFirearmFirstYear.ToString())) return false;

            if (tagQuery[TagType.MagazineType].Count > 0 && !tagQuery[TagType.MagazineType].Contains(item.MagazineType.ToString())) return false;

            if (tagQuery[TagType.Caliber].Count > 0 && (!item.UsesRoundTypeFlag || !tagQuery[TagType.Caliber].Contains(item.RoundType.ToString()))) return false;

            if (tagQuery[TagType.FiringMode].Count > 0 && !item.TagFirearmFiringModes.Any(o => tagQuery[TagType.FiringMode].Contains(o.ToString()))) return false;

            if (tagQuery[TagType.FeedOption].Count > 0 && !item.TagFirearmFeedOption.Any(o => tagQuery[TagType.FeedOption].Contains(o.ToString()))) return false;

            if (tagQuery[TagType.AttachmentMount].Count > 0 && !item.TagFirearmMounts.Any(o => tagQuery[TagType.AttachmentMount].Contains(o.ToString()))) return false;

            return true;
        }


        private static bool ShouldIncludeAttachment(FVRObject item, Dictionary<TagType, List<string>> tagQuery)
        {
            if (tagQuery[TagType.AttachmentFeature].Count > 0 && !tagQuery[TagType.AttachmentFeature].Contains(item.TagAttachmentFeature.ToString())) return false;

            if (tagQuery[TagType.AttachmentMount].Count > 0 && !tagQuery[TagType.AttachmentMount].Contains(item.TagAttachmentMount.ToString())) return false;

            return true;
        }


        private static bool ShouldIncludeMagazine(FVRObject item, Dictionary<TagType, List<string>> tagQuery)
        {
            if (tagQuery[TagType.MagazineType].Count > 0 && !tagQuery[TagType.MagazineType].Contains(item.MagazineType.ToString())) return false;

            return true;
        }






        [HarmonyPatch(typeof(ItemSpawnerV2), "RedrawTagsCanvas")]
        [HarmonyILManipulator]
        private static void TagNamingPatch(ILContext ctx, MethodBase orig)
        {
            ILCursor c = new ILCursor(ctx);

            c.GotoNext(
                i => i.MatchLdarg(0),
                i => i.MatchLdfld(AccessTools.Field(typeof(ItemSpawnerV2), "TXT_Tags")),
                i => i.MatchLdloc(14),
                i => i.MatchCallvirt(AccessTools.Method(typeof(List<Text>), "get_Item", new Type[] { typeof(int) })),
                i => i.MatchLdloc(8),
                i => i.MatchLdloc(13),
                i => i.MatchCallvirt(AccessTools.Method(typeof(List<string>), "get_Item", new Type[] { typeof(int) })),
                i => i.MatchCallvirt(AccessTools.Method(typeof(Text), "set_text", new Type[] { typeof(string) }))
            );

            //Insert the ldarg it expects 
            c.Emit(OpCodes.Ldarg, 0);

            //Now move the cursor so we can insert code between getting the string and setting the text
            c.Index += 7;
            c.Emit(OpCodes.Ldarg, 0);
            c.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(ItemSpawnerV2), "m_tagType"));

            //Remove the call to set text
            c.Remove();

            //Now add our new call for setting the text
            c.Emit(OpCodes.Call, ((Action<Text, string, TagType>)CategoricalSetText).Method);
        }
        


        private static IEnumerator SpawnItems(ItemSpawnerV2 instance, ItemSpawnerEntry entry)
        {
            List<AnvilCallback<GameObject>> itemsToSpawn = new List<AnvilCallback<GameObject>>();

            itemsToSpawn.Add(IM.OD[entry.MainObjectID].GetGameObjectAsync());
            itemsToSpawn.AddRange(entry.SpawnWithIDs.Select(o => IM.OD[o].GetGameObjectAsync()));

            for(int i = 0; i < itemsToSpawn.Count; i++)
            {
                yield return itemsToSpawn[i];

                if (i == 0 && entry.UsesLargeSpawnPad)
                {
                    UnityEngine.Object.Instantiate(itemsToSpawn[i].Result, instance.SpawnPoint_Large.position, instance.SpawnPoint_Large.rotation);
                }
                else
                {
                    if (instance.m_curSmallPos >= instance.SpawnPoints_Small.Count)
                    {
                        instance.m_curSmallPos = 0;
                    }

                    UnityEngine.Object.Instantiate(itemsToSpawn[i].Result, instance.SpawnPoints_Small[instance.m_curSmallPos].position, instance.SpawnPoints_Small[instance.m_curSmallPos].rotation);

                    instance.m_curSmallPos += 1;
                }
            }
        }



        private static void CategoricalSetText(Text text, string value, TagType tagType)
        {
            if (tagType == TagType.Category && !Enum.IsDefined(typeof(ItemSpawnerID.EItemCategory), value))
            {
                text.text = IM.CDefInfo[(ItemSpawnerID.EItemCategory)int.Parse(value)].DisplayName;
                return;
            }

            else if (tagType == TagType.SubCategory && !Enum.IsDefined(typeof(ItemSpawnerID.ESubCategory), value))
            {
                text.text = IM.CDefSubInfo[(ItemSpawnerID.ESubCategory)int.Parse(value)].DisplayName;
                return;
            }

            text.text = value;
        }



        [HarmonyPatch(typeof(IM), "RegisterItemIntoMetaTagSystem")]
        [HarmonyPostfix]
        private static void MetaTagPatch(ItemSpawnerID ID)
        {
            //If this IDs items didn't get added, add it to the firearm page
            if (IM.Instance.PageItemLists.ContainsKey(ItemSpawnerV2.PageMode.Firearms)){
                if (!IM.Instance.PageItemLists.Any(o => o.Value.Contains(ID.ItemID)) && IM.OD.ContainsKey(ID.ItemID) && IM.OD[ID.ItemID].IsModContent)
                {
                    OtherLogger.Log("Adding misc mod item to meta tag system: " + ID.ItemID, OtherLogger.LogType.Loading);

                    IM.AddMetaTag(ID.Category.ToString(), TagType.Category, ID.ItemID, ItemSpawnerV2.PageMode.Firearms);
                    IM.AddMetaTag(ID.SubCategory.ToString(), TagType.SubCategory, ID.ItemID, ItemSpawnerV2.PageMode.Firearms);
                }
            }
        }

        [HarmonyPatch(typeof(IM), "GenerateItemDBs")]
        [HarmonyPostfix]
        private static void PopulateSpawnerEntries()
        {
            foreach(KeyValuePair<ItemSpawnerV2.PageMode,List<string>> PageLists in IM.Instance.PageItemLists)
            {
                foreach(string ItemID in PageLists.Value)
                {
                    ItemSpawnerID SpawnerID = IM.Instance.SpawnerIDDic[ItemID];

                    ItemSpawnerEntry SpawnerEntry = ScriptableObject.CreateInstance<ItemSpawnerEntry>();
                    SpawnerEntry.LegacyPopulateFromID(PageLists.Key, SpawnerID, false);
                    ItemLoader.PopulateEntryPaths(SpawnerEntry, SpawnerID);
                }
            }
        }

    }
}
