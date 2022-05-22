using FistVR;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using OtherLoader.Loaders;
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
            //__instance.TXT_Detail.rectTransform.anchoredPosition = new Vector2(390, 500);
            //__instance.TXT_Detail.rectTransform.sizeDelta = new Vector2(310, 390);

            //Image backing = __instance.TXT_Detail.transform.GetComponentInChildren<Image>();
            //backing.rectTransform.anchoredPosition = new Vector2(0, 10);

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
                ItemSpawnerEntry secondaryEntry = data.VisibleSecondaryEntries[i];

                __instance.AddToSelectionQueue(secondaryEntry.MainObjectID);
                __instance.SetSelectedID(secondaryEntry.MainObjectID);
                __instance.RedrawDetailsCanvas();
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


        [HarmonyPatch(typeof(ItemSpawnerV2), "BTN_Details_PlayTutorialVideo")]
        [HarmonyPrefix]
        private static bool PlayTutorialPatch(ItemSpawnerV2 __instance, int i)
        {
            if (!string.IsNullOrEmpty(__instance.m_selectedID))
            {
                __instance.Boop(0);
                __instance.DetailsHelpVideoPlayer.SetActive(true);
                ItemSpawnerEntry entry = OtherLoader.SpawnerEntriesByID[__instance.m_selectedID];
                string key = entry.TutorialBlockIDs[i];
                __instance.VP_DetailsScreen.PlayTutorialBlock(IM.TutorialBlockDic[key]);
                __instance.VP_DetailsTitle.text = IM.TutorialBlockDic[key].Title;
            }
            else
            {
                __instance.Boop(2);
            }

            return false;
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


        [HarmonyPatch(typeof(ItemSpawnerV2), "SimpleSelectTile")]
        [HarmonyPrefix]
        private static bool SimpleButtonPatch(ItemSpawnerV2 __instance, int i)
        {
            ItemSpawnerData data = __instance.GetComponent<ItemSpawnerData>();

            //If the entry that was selected has child entries, we should display the child entries
            if (OtherLoader.DoesEntryHaveChildren(data.VisibleEntries[i]))
            {
                data.CurrentPath = data.VisibleEntries[i].EntryPath;
                data.CurrentDepth += 1;
                data.SavedPagePositions[__instance.PMode][data.CurrentDepth] = 0;

                __instance.RedrawSimpleCanvas();
            }

            //If the item is unlocked, allow it to be selected
            else if (OtherLoader.UnlockSaveData.IsItemUnlocked(data.VisibleEntries[i].MainObjectID))
            {
                OtherLogger.Log("Setting selected id to: " + data.VisibleEntries[i].MainObjectID, OtherLogger.LogType.General);

                __instance.AddToSelectionQueue(data.VisibleEntries[i].MainObjectID);
                __instance.SetSelectedID(data.VisibleEntries[i].MainObjectID);
                __instance.RedrawDetailsCanvas();
            }

            return false;
        }



        [HarmonyPatch(typeof(ItemSpawnerV2), "SelectItem")]
        [HarmonyPrefix]
        private static bool SelectItemPatch(ItemSpawnerV2 __instance, int i)
        {
            if (i < __instance.m_displayedItemIds.Count && OtherLoader.UnlockSaveData.IsItemUnlocked(__instance.m_displayedItemIds[i]))
            {
                __instance.SetSelectedID(__instance.m_displayedItemIds[i]);
                __instance.RedrawDetailsCanvas();
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
            string childNodePaths = String.Join("\n", OtherLoader.SpawnerEntriesByPath[data.CurrentPath].childNodes.Select(o => "Display?: " + o.entry.IsDisplayedInMainEntry + ", Path: " + o.entry.EntryPath).ToArray());

            OtherLogger.Log($"Logging all possible child entries: {childNodePaths}", OtherLogger.LogType.Loading);
            OtherLogger.Log($"Got {entries.Count} entries for path: {OtherLoader.SpawnerEntriesByPath[data.CurrentPath].childNodes}", OtherLogger.LogType.Loading);

            entries = entries.OrderBy(o => o.entry.DisplayName).OrderBy(o => o.entry.IsModded?1:0).OrderBy(o => o.childNodes.Count > 0?0:1).ToList();

            int currPage = data.SavedPagePositions[__instance.PMode][data.CurrentDepth];
            int startIndex = currPage * __instance.IMG_SimpleTiles.Count;
            for (int i = 0; i < __instance.IMG_SimpleTiles.Count; i++)
            {
                if(startIndex + i < entries.Count)
                {
                    ItemSpawnerEntry entry = entries[startIndex + i].entry;
                    data.VisibleEntries.Add(entry);

                    if (OtherLoader.DoesEntryHaveChildren(entry) || OtherLoader.UnlockSaveData.IsItemUnlocked(entry.MainObjectID))
                    {
                        __instance.IMG_SimpleTiles[i].gameObject.SetActive(true);
                        __instance.TXT_SimpleTiles[i].gameObject.SetActive(true);
                        __instance.IMG_SimpleTiles[i].sprite = entry.EntryIcon;
                        __instance.TXT_SimpleTiles[i].text = entry.DisplayName;
                    }
                    else
                    {
                        __instance.IMG_SimpleTiles[i].gameObject.SetActive(true);
                        __instance.TXT_SimpleTiles[i].gameObject.SetActive(true);
                        __instance.IMG_SimpleTiles[i].sprite = OtherLoader.LockIcon;
                        __instance.TXT_SimpleTiles[i].text = "???";
                    }
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

                        if (OtherLoader.DoesEntryHaveChildren(entry) || OtherLoader.UnlockSaveData.IsItemUnlocked(entry.MainObjectID))
                        {
                            images[m].sprite = entry.EntryIcon;
                            images[m].gameObject.SetActive(true);
                            texts[m].text = entry.DisplayName;
                            texts[m].gameObject.SetActive(true);
                        }
                        else
                        {
                            images[m].sprite = OtherLoader.LockIcon;
                            images[m].gameObject.SetActive(true);
                            texts[m].text = "???";
                            texts[m].gameObject.SetActive(true);
                        }
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
            /*
            OtherLogger.Log("Performing Tag Saerch!", OtherLogger.LogType.General);

            foreach (KeyValuePair<TagType, List<string>> pair in tagQuery)
            {
                OtherLogger.Log("Tag Type: " + pair.Key.ToString() + ", Values: " + String.Join(",", pair.Value.ToArray()), OtherLogger.LogType.General);
            }
            */

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

            if (tagQuery[TagType.Caliber].Count > 0 && (!item.UsesRoundTypeFlag || !tagQuery[TagType.Caliber].Contains(item.RoundType.ToString()))) return false;

            if (tagQuery[TagType.MagazineType].Count > 0 && !tagQuery[TagType.MagazineType].Contains(item.MagazineType.ToString())) return false;

            if (tagQuery[TagType.CountryOfOrigin].Count > 0 && !tagQuery[TagType.CountryOfOrigin].Contains(item.TagFirearmCountryOfOrigin.ToString())) return false;

            if (tagQuery[TagType.IntroductionYear].Count > 0 && !tagQuery[TagType.IntroductionYear].Contains(item.TagFirearmFirstYear.ToString())) return false;

            return true;
        }

        private static bool ShouldIncludeFirearm(FVRObject item, Dictionary<TagType, List<string>> tagQuery)
        {
            if (tagQuery[TagType.Size].Count > 0 && !tagQuery[TagType.Size].Contains(item.TagFirearmSize.ToString())) return false;

            if (tagQuery[TagType.Action].Count > 0 && !tagQuery[TagType.Action].Contains(item.TagFirearmAction.ToString())) return false;

            if (tagQuery[TagType.RoundClass].Count > 0 && !tagQuery[TagType.RoundClass].Contains(item.TagFirearmRoundPower.ToString())) return false;

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



        [HarmonyPatch(typeof(IM), "GenerateItemDBs")]
        [HarmonyILManipulator]
        private static void DisableRewardCheckPatch(ILContext ctx, MethodBase orig)
        {
            ILCursor c = new ILCursor(ctx);

            c.GotoNext(
                i => i.MatchLdfld(AccessTools.Field(typeof(ItemSpawnerID), "MainObject"))
            );

            c.RemoveRange(17);

            c.Emit(OpCodes.Call, ((Action<ItemSpawnerID>)RegisterItemSpawnerID).Method);

            c.GotoNext(
                i => i.MatchLdstr("SosigEnemyTemplates")
            );

            c.Emit(OpCodes.Call, ((Action)PopulateSpawnerEntries).Method);

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

        private static void RegisterItemSpawnerID(ItemSpawnerID spawnerID)
        {
            if(spawnerID.MainObject != null)
            {
                IM.RegisterItemIntoMetaTagSystem(spawnerID);
            }
        }


        private static void PopulateSpawnerEntries()
        {
            SpawnerEntryPathBuilder pathBuilder = new SpawnerEntryPathBuilder();

            foreach(KeyValuePair<ItemSpawnerV2.PageMode,List<string>> PageLists in IM.Instance.PageItemLists)
            {
                foreach(string ItemID in PageLists.Value)
                {
                    ItemSpawnerID SpawnerID = IM.Instance.SpawnerIDDic[ItemID];

                    ItemSpawnerEntry SpawnerEntry = ScriptableObject.CreateInstance<ItemSpawnerEntry>();
                    SpawnerEntry.LegacyPopulateFromID(PageLists.Key, SpawnerID, false);
                    pathBuilder.PopulateEntryPaths(SpawnerEntry, SpawnerID);
                }
            }
        }

    }
}
