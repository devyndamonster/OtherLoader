using FistVR;
using HarmonyLib;
using OtherLoader.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Patches
{
    public class SimpleCanvasePatches
    {

        public static IPathService PathService = new PathService();

        [HarmonyPatch(typeof(ItemSpawnerV2), "SimpleGoBack")]
        [HarmonyPrefix]
        private static bool GoBackPatch(ItemSpawnerV2 __instance)
        {
            ItemSpawnerData spawnerData = __instance.GetComponent<ItemSpawnerData>();

            if (PathService.HasParent(spawnerData.CurrentPath))
            {
                spawnerData.SavedPagePositions[spawnerData.CurrentPath] = 0;
                spawnerData.CurrentPath = PathService.GetParentPath(spawnerData.CurrentPath);
                spawnerData.CurrentDepth -= 1;
                __instance.RedrawSimpleCanvas();
            }

            return false;
        }


        [HarmonyPatch(typeof(ItemSpawnerV2), "SimpleNextPage")]
        [HarmonyPrefix]
        private static bool NextPagePatch(ItemSpawnerV2 __instance)
        {
            ItemSpawnerData spawnerData = __instance.GetComponent<ItemSpawnerData>();

            var numberOfPages = GetNumberOfPages(__instance, spawnerData);
            var currentPage = spawnerData.SavedPagePositions[spawnerData.CurrentPath];
            spawnerData.SavedPagePositions[spawnerData.CurrentPath] = Math.Min(currentPage + 1, numberOfPages);
            __instance.RedrawSimpleCanvas();

            return false;
        }


        [HarmonyPatch(typeof(ItemSpawnerV2), "SimplePrevage")]
        [HarmonyPrefix]
        private static bool PrevPagePatch(ItemSpawnerV2 __instance)
        {
            ItemSpawnerData spawnerData = __instance.GetComponent<ItemSpawnerData>();

            var currentPage = spawnerData.SavedPagePositions[spawnerData.CurrentPath];
            spawnerData.SavedPagePositions[spawnerData.CurrentPath] = Math.Max(currentPage - 1, 0);
            __instance.RedrawSimpleCanvas();
            
            return false;
        }


        [HarmonyPatch(typeof(ItemSpawnerV2), "SimpleSelectTile")]
        [HarmonyPrefix]
        private static bool SimpleButtonPatch(ItemSpawnerV2 __instance, int i)
        {
            ItemSpawnerData data = __instance.GetComponent<ItemSpawnerData>();

            if (OtherLoader.DoesEntryHaveChildren(data.VisibleEntries[i]))
            {
                SelectCategory(__instance, data, i);
            }

            else if (OtherLoader.UnlockSaveData.IsItemUnlocked(data.VisibleEntries[i].MainObjectID))
            {
                SelectItem(__instance, data, i);
            }

            return false;
        }

        private static void SelectCategory(ItemSpawnerV2 instance, ItemSpawnerData spawnerData, int itemIndex)
        {
            spawnerData.CurrentPath = spawnerData.VisibleEntries[itemIndex].EntryPath;
            spawnerData.CurrentDepth += 1;
            spawnerData.SavedPagePositions[spawnerData.CurrentPath] = 0;
            instance.RedrawSimpleCanvas();
        }

        private static void SelectItem(ItemSpawnerV2 instance, ItemSpawnerData spawnerData, int itemIndex)
        {
            instance.AddToSelectionQueue(spawnerData.VisibleEntries[itemIndex].MainObjectID);
            instance.SetSelectedID(spawnerData.VisibleEntries[itemIndex].MainObjectID);
            instance.RedrawDetailsCanvas();
        }


        [HarmonyPatch(typeof(ItemSpawnerV2), "RedrawSimpleCanvas")]
        [HarmonyPrefix]
        private static bool RedrawSimplePatch(ItemSpawnerV2 __instance)
        {
            if (__instance.PMode == ItemSpawnerV2.PageMode.MainMenu) return false;

            ItemSpawnerData spawnerData = __instance.GetComponent<ItemSpawnerData>();

            List<EntryNode> entries = OtherLoader.SpawnerEntriesByPath[spawnerData.CurrentPath].childNodes
                .Where(o => o.entry.IsDisplayedInMainEntry)
                .OrderBy(o => o.entry.IsModded)
                .ToList();

            OtherLogger.Log($"Spawner Entries visible at path:\n{entries
                .AsJoinedString(child => child.entry.EntryPath + ", IsUnlocked: " + OtherLoader.UnlockSaveData.IsItemUnlocked(child.entry.MainObjectID).ToString(), "\n")}", OtherLogger.LogType.ItemSpawner);

            int currentPage = spawnerData.SavedPagePositions[spawnerData.CurrentPath];
            int numberOfPages = GetNumberOfPages(entries.Count, __instance.IMG_SimpleTiles.Count);

            DrawTiles(__instance, entries, spawnerData, currentPage);
            DrawPageNumberDisplay(__instance, currentPage, numberOfPages, spawnerData.VisibleEntries.Count);
            DrawPageButtons(__instance, currentPage, numberOfPages);
            
            return false;
        }
        
        private static void DrawTiles(ItemSpawnerV2 instance, List<EntryNode> entries, ItemSpawnerData spawnerData, int currentPage)
        {
            int startIndex = currentPage * instance.IMG_SimpleTiles.Count;
            spawnerData.VisibleEntries.Clear();

            for (int i = 0; i < instance.IMG_SimpleTiles.Count; i++)
            {
                if (startIndex + i < entries.Count())
                {
                    ItemSpawnerEntry entry = entries[startIndex + i].entry;
                    spawnerData.VisibleEntries.Add(entry);
                    instance.IMG_SimpleTiles[i].gameObject.SetActive(true);
                    instance.TXT_SimpleTiles[i].gameObject.SetActive(true);

                    if (OtherLoader.DoesEntryHaveChildren(entry) || OtherLoader.UnlockSaveData.IsItemUnlocked(entry.MainObjectID))
                    {
                        instance.IMG_SimpleTiles[i].sprite = entry.EntryIcon;
                        instance.TXT_SimpleTiles[i].text = entry.DisplayName;
                    }
                    else
                    {
                        instance.IMG_SimpleTiles[i].sprite = OtherLoader.LockIcon;
                        instance.TXT_SimpleTiles[i].text = "???";
                    }
                }
                else
                {
                    instance.IMG_SimpleTiles[i].gameObject.SetActive(false);
                    instance.TXT_SimpleTiles[i].gameObject.SetActive(false);
                }
            }
        }

        private static int GetNumberOfPages(int numberOfItems, int pageSize)
        {
            return (int)Math.Ceiling((double)numberOfItems / pageSize);
        }

        private static int GetNumberOfPages(ItemSpawnerV2 instance, ItemSpawnerData spawnerData)
        {
            int numberOfItems = OtherLoader.SpawnerEntriesByPath[spawnerData.CurrentPath].childNodes.Count;
            return GetNumberOfPages(numberOfItems, instance.IMG_SimpleTiles.Count);
        }

        private static void DrawPageNumberDisplay(ItemSpawnerV2 instance, int currentPage, int numberOfPages, int itemsOnPage)
        {
            instance.TXT_SimpleTiles_PageNumber.text = (currentPage + 1) + " / " + (numberOfPages);
            instance.TXT_SimpleTiles_Showing.text = $"Showing {currentPage * numberOfPages} - {currentPage * numberOfPages + itemsOnPage} Of {itemsOnPage}";
        }

        private static void DrawPageButtons(ItemSpawnerV2 instance, int currentPage, int numberOfPages)
        {
            if (currentPage > 0)
            {
                instance.GO_SimpleTiles_PrevPage.SetActive(true);
            }
            else
            {
                instance.GO_SimpleTiles_PrevPage.SetActive(false);
            }

            if (currentPage < numberOfPages - 1)
            {
                instance.GO_SimpleTiles_NextPage.SetActive(true);
            }
            else
            {
                instance.GO_SimpleTiles_NextPage.SetActive(false);
            }
        }

    }
}
