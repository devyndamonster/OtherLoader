using FistVR;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OtherLoader.Patches
{
    public class DetailsPanelPatches
    {

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


        [HarmonyPatch(typeof(ItemSpawnerV2), "RedrawDetailsCanvas")]
        [HarmonyPrefix]
        private static bool RedrawDetailsCanvasPatch(ItemSpawnerV2 __instance)
        {
            if (!OtherLoader.SpawnerEntriesByID.ContainsKey(__instance.m_selectedID))
            {
                return true;
            }

            else
            {
                ItemSpawnerEntry entry = OtherLoader.SpawnerEntriesByID[__instance.m_selectedID];
                ItemSpawnerData data = __instance.GetComponent<ItemSpawnerData>();

                EnableDetailElements(__instance);
                PopulateDetailText(__instance, entry);
                PopulateCurrentItemIcon(__instance, entry);
                RedrawSecondariesPanel(__instance, entry, data);
                RedrawFavButtons(__instance);

                if (__instance.m_detailsLinkedResourceMode == ItemSpawnerV2.DetailsLinkedResourceMode.Tutorials)
                {
                    RedrawTutorialBlocks(__instance, entry);
                }
                else if(__instance.m_detailsLinkedResourceMode == ItemSpawnerV2.DetailsLinkedResourceMode.RelatedVaultFiles)
                {
                    RedrawRelatedVaultFiles(__instance, entry);
                }
            }

            return false;
        }

        private static void RedrawFavButtons(ItemSpawnerV2 __instance)
        {
            for (int i = 0; i < __instance.IM_FavButtons.Count; i++)
            {
                __instance.IM_FavButtons[i].gameObject.SetActive(true);

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

        private static void EnableDetailElements(ItemSpawnerV2 __instance)
        {
            __instance.IM_Detail.gameObject.SetActive(true);
            __instance.BTN_SpawnSelectedObject.SetActive(true);
        }

        private static void PopulateDetailText(ItemSpawnerV2 __instance, ItemSpawnerEntry entry)
        {
            __instance.TXT_Title.text = entry.DisplayName;
            __instance.TXT_Detail.text = __instance.GetDetailText(__instance.m_selectedID);
        }

        private static void PopulateCurrentItemIcon(ItemSpawnerV2 __instance, ItemSpawnerEntry entry)
        {
            __instance.IM_Detail.sprite = entry.EntryIcon;
        }

        private static List<ItemSpawnerEntry> GetSecondaryEntries(ItemSpawnerEntry entry)
        {
            return entry.SecondaryObjectIDs
                .Where(o => OtherLoader.SpawnerEntriesByID.ContainsKey(o))
                .Select(o => OtherLoader.SpawnerEntriesByID[o])
                .ToList();
        }

        private static void RedrawSecondariesPanel(ItemSpawnerV2 __instance, ItemSpawnerEntry entry, ItemSpawnerData data)
        {
            List<ItemSpawnerEntry> secondaryEntries = GetSecondaryEntries(entry);
            RedrawSecondaryTiles(__instance, secondaryEntries, data);
            RedrawSecondariesPageButtons(__instance, secondaryEntries, data);
            RedrawSecondariesQueueButtons(__instance);
        }

        private static void RedrawSecondaryTiles(ItemSpawnerV2 __instance, List<ItemSpawnerEntry> secondaryEntries, ItemSpawnerData data)
        {
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
        }

        private static void RedrawSecondariesPageButtons(ItemSpawnerV2 __instance, List<ItemSpawnerEntry> secondaryEntries, ItemSpawnerData data)
        {
            int numPages = GetNumberOfPagesRequired(secondaryEntries.Count, __instance.IM_DetailRelated.Count);
            __instance.TXT_DetailsRelatedPageNum.gameObject.SetActive(true);
            __instance.TXT_DetailsRelatedPageNum.text = (__instance.m_selectedIDRelatedPage + 1).ToString() + " / " + numPages.ToString();
            __instance.BTN_DetailsRelatedPrevPage.SetActive(__instance.m_selectedIDRelatedPage > 0);
            __instance.BTN_DetailsRelatedNextPage.SetActive(__instance.m_selectedIDRelatedPage < numPages - 1);
        }

        private static void RedrawSecondariesQueueButtons(ItemSpawnerV2 __instance)
        {
            __instance.BTN_QueuePrev.SetActive(__instance.m_queuePoint > 0);
            __instance.BTN_QueueNext.SetActive(__instance.m_queuePoint >= 0 && __instance.m_queuePoint < __instance.m_selectionQueue.Count - 1);
        }

        private static void RedrawTutorialBlocks(ItemSpawnerV2 __instance, ItemSpawnerEntry entry)
        {
            int pageSize = __instance.BTNS_DetailTutorial.Count;
            int numPages = GetNumberOfPagesRequired(entry.TutorialBlockIDs.Count, pageSize);

            if (__instance.m_selectedIDTutPage >= numPages)
            {
                __instance.m_selectedIDTutPage = numPages - 1;
            }

            int startIndex = __instance.m_selectedIDTutPage * pageSize;

            for (int i = 0; i < pageSize; i++)
            {
                if (startIndex + i < entry.TutorialBlockIDs.Count)
                {
                    __instance.BTNS_DetailTutorial[i].gameObject.SetActive(true);
                    __instance.BTNS_DetailTutorial[i].text = IM.TutorialBlockDic[entry.TutorialBlockIDs[startIndex + i]].Title;
                }
                else
                {
                    __instance.BTNS_DetailTutorial[i].gameObject.SetActive(false);
                }
            }

            if (entry.TutorialBlockIDs.Count > 0)
            {
                __instance.TXT_DetailsTutVaultPageNum.text = (__instance.m_selectedIDTutPage + 1).ToString() + " / " + numPages.ToString();
                __instance.BTN_DetailsTutVaultPrevPage.SetActive(__instance.m_selectedIDTutPage > 0);
                __instance.BTN_DetailsTutVaultNextPage.SetActive(__instance.m_selectedIDTutPage < numPages - 1);
                __instance.TXT_DetailsTutVaultPageNum.gameObject.SetActive(__instance.BTN_DetailsTutVaultPrevPage.activeSelf || __instance.BTN_DetailsTutVaultNextPage.activeSelf);
            }
            else
            {
                __instance.TXT_DetailsTutVaultPageNum.gameObject.SetActive(false);
                __instance.BTN_DetailsTutVaultNextPage.SetActive(false);
                __instance.BTN_DetailsTutVaultPrevPage.SetActive(false);
            }
        }



        private static void RedrawRelatedVaultFiles(ItemSpawnerV2 __instance, ItemSpawnerEntry entry)
        {
            __instance.relatedVaultFiles = IM.GetVaultFileNamesForID(entry.MainObjectID);
            int pageSize = __instance.BTNS_DetailTutorial.Count;
            int numPages = GetNumberOfPagesRequired(__instance.relatedVaultFiles.Count, pageSize);

            OtherLogger.Log("Number of pages for vault: " + numPages, OtherLogger.LogType.General);
            OtherLogger.Log("Selected page: " + __instance.m_selectedIDVaultPage, OtherLogger.LogType.General);

            if (__instance.m_selectedIDVaultPage >= numPages)
            {
                OtherLogger.Log("Reducing!", OtherLogger.LogType.General);
                __instance.m_selectedIDVaultPage = numPages - 1;
            }

            int startIndex = __instance.m_selectedIDVaultPage * pageSize;

            for (int i = 0; i < pageSize; i++)
            {
                if (startIndex + i < __instance.relatedVaultFiles.Count)
                {
                    __instance.BTNS_DetailTutorial[i].gameObject.SetActive(true);
                    __instance.BTNS_DetailTutorial[i].text = __instance.relatedVaultFiles[startIndex + i];
                }
                else
                {
                    __instance.BTNS_DetailTutorial[i].gameObject.SetActive(false);
                }
            }

            if (__instance.relatedVaultFiles.Count > 0)
            {
                __instance.TXT_DetailsTutVaultPageNum.text = (__instance.m_selectedIDVaultPage + 1).ToString() + " / " + numPages.ToString();
                __instance.BTN_DetailsTutVaultPrevPage.SetActive(__instance.m_selectedIDVaultPage > 0);
                __instance.BTN_DetailsTutVaultNextPage.SetActive(__instance.m_selectedIDVaultPage < numPages - 1);
                __instance.TXT_DetailsTutVaultPageNum.gameObject.SetActive(__instance.BTN_DetailsTutVaultPrevPage.activeSelf || __instance.BTN_DetailsTutVaultNextPage.activeSelf);
            }
            else
            {
                __instance.TXT_DetailsTutVaultPageNum.gameObject.SetActive(false);
                __instance.BTN_DetailsTutVaultNextPage.SetActive(false);
                __instance.BTN_DetailsTutVaultPrevPage.SetActive(false);
            }
        }

        private static int GetNumberOfPagesRequired(int numberOfItems, int pageSize)
        {
            return Mathf.Max(Mathf.CeilToInt((float)numberOfItems / pageSize), 1);
        }

    }
}
