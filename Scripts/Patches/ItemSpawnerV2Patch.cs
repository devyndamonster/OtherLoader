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



        [HarmonyPatch(typeof(ItemSpawnerV2), "SetPageMode")]
        [HarmonyPrefix]
        private static bool PageModePatch(ItemSpawnerV2 __instance, int i)
        {
            ItemSpawnerData data = __instance.GetComponent<ItemSpawnerData>();
            data.CurrentPath = ((ItemSpawnerV2.PageMode)i).ToString();
            data.CurrentPage = 0;

            return true;
        }



        [HarmonyPatch(typeof(ItemSpawnerV2), "SimpleGoBack")]
        [HarmonyPrefix]
        private static bool GoBackPatch(ItemSpawnerV2 __instance)
        {
            ItemSpawnerData data = __instance.GetComponent<ItemSpawnerData>();
            if (!data.CurrentPath.Contains("/")) return false;

            data.CurrentPath = data.CurrentPath.Substring(0, data.CurrentPath.LastIndexOf("/"));
            data.CurrentPage = 0;

            OtherLogger.Log("Going back to path: " + data.CurrentPath, OtherLogger.LogType.General);
            __instance.RedrawSimpleCanvas();

            return false;
        }


        [HarmonyPatch(typeof(ItemSpawnerV2), "SimpleNextPage")]
        [HarmonyPrefix]
        private static bool NextPagePatch(ItemSpawnerV2 __instance)
        {
            ItemSpawnerData data = __instance.GetComponent<ItemSpawnerData>();
            
            if(OtherLoader.SpawnerEntriesByPath[data.CurrentPath].Count / __instance.IMG_SimpleTiles.Count > data.CurrentPage)
            {
                data.CurrentPage += 1;
                __instance.RedrawSimpleCanvas();
            }

            return false;
        }


        [HarmonyPatch(typeof(ItemSpawnerV2), "SimplePrevage")]
        [HarmonyPrefix]
        private static bool PrevPagePatch(ItemSpawnerV2 __instance)
        {
            ItemSpawnerData data = __instance.GetComponent<ItemSpawnerData>();

            if (data.CurrentPage > 0)
            {
                data.CurrentPage -= 1;
                __instance.RedrawSimpleCanvas();
            }

            return false;
        }



        [HarmonyPatch(typeof(ItemSpawnerV2), "GetDetailText")]
        [HarmonyPrefix]
        private static bool DeatilTextPatch(ItemSpawnerV2 __instance, string id, ref string __result)
        {
            
            if (!IM.Instance.SpawnerIDDic.ContainsKey(id))
            {
                OtherLogger.LogError($"The ItemID was not found to have spawnerID! ItemID: {id}");
                __result = "";
                return false;
            }

            ItemSpawnerID spawnerID = IM.Instance.SpawnerIDDic[id];
            FVRObject fvrObj = IM.OD[spawnerID.MainObject.ItemID];

            string spawnerCat = spawnerID.Category.ToString();
            if (!Enum.IsDefined(typeof(ItemSpawnerID.EItemCategory), spawnerID.Category) && IM.CDefInfo.ContainsKey(spawnerID.Category))
                spawnerCat = IM.CDefInfo[spawnerID.Category].DisplayName;

            string spawnerSubcat = spawnerID.SubCategory.ToString();
            if (!Enum.IsDefined(typeof(ItemSpawnerID.ESubCategory), spawnerID.SubCategory) && IM.CDefSubInfo.ContainsKey(spawnerID.SubCategory))
                spawnerSubcat = IM.CDefSubInfo[spawnerID.SubCategory].DisplayName;


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
            if (OtherLoader.SpawnerEntriesByPath[data.VisibleEntries[i].EntryPath].Count > 0)
            {
                data.CurrentPath = data.VisibleEntries[i].EntryPath;
                data.CurrentPage = 0;
                __instance.RedrawSimpleCanvas();
            }

            else
            {
                __instance.SetSelectedID(data.VisibleEntries[i].MainObjectID);
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

            List<ItemSpawnerEntry> entries = OtherLoader.SpawnerEntriesByPath[data.CurrentPath].Where(o => o.IsDisplayedInMainEntry).ToList();

            OtherLogger.Log($"Got {entries.Count} entries for path: {data.CurrentPath}", OtherLogger.LogType.General);

            entries = entries.OrderBy(o => o.DisplayName).OrderBy(o => o.IsModded?1:0).OrderBy(o => (OtherLoader.SpawnerEntriesByPath[o.EntryPath].Count > 0)?1:0).ToList();

            int startIndex = data.CurrentPage * __instance.IMG_SimpleTiles.Count;
            for (int i = 0; i < __instance.IMG_SimpleTiles.Count; i++)
            {
                if(startIndex + i < entries.Count)
                {
                    ItemSpawnerEntry entry = entries[startIndex + i];
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

            __instance.TXT_SimpleTiles_PageNumber.text = (data.CurrentPage + 1) + " / " + (numPages);
            __instance.TXT_SimpleTiles_Showing.text = 
                "Showing " + 
                (data.CurrentPage * __instance.IMG_SimpleTiles.Count) + 
                " - " + 
                (data.CurrentPage * __instance.IMG_SimpleTiles.Count + data.VisibleEntries.Count) +
                " Of " +
                entries.Count;


            if(data.CurrentPage > 0)
            {
                __instance.GO_SimpleTiles_PrevPage.SetActive(true);
            }
            else
            {
                __instance.GO_SimpleTiles_PrevPage.SetActive(false);
            }

            if(data.CurrentPage < numPages - 1)
            {
                __instance.GO_SimpleTiles_NextPage.SetActive(true);
            }
            else
            {
                __instance.GO_SimpleTiles_NextPage.SetActive(false);
            }

            return false;
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
                if (!IM.Instance.PageItemLists[ItemSpawnerV2.PageMode.Firearms].Contains(ID.ItemID) && IM.OD.ContainsKey(ID.ItemID) && IM.OD[ID.ItemID].IsModContent)
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
                    SpawnerEntry.PopulateEntry(PageLists.Key, SpawnerID, false);
                }
            }
        }

    }
}
