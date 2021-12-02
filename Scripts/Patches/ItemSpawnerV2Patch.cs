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
            __instance.gameObject.AddComponent<ItemSpawnerData>();
            return true;
        }


        /*
        [HarmonyPatch(typeof(ItemSpawnerV2), "Awake")]
        [HarmonyPostfix]
        private static void AwakePatch(ItemSpawnerV2 __instance)
        {
            if(LoaderStatus.GetLoaderProgress() >= 1)
            {
                UpdateCatDefs(__instance);
            }

            else
            {
                __instance.StartCoroutine(WaitUntilLoadComplete(__instance));
            }

        }
        */

        private static IEnumerator WaitUntilLoadComplete(ItemSpawnerV2 __instance)
        {
            while(LoaderStatus.GetLoaderProgress() < 1)
            {
                yield return null;
            }

            UpdateCatDefs(__instance);
        }


        private static void UpdateCatDefs(ItemSpawnerV2 __instance)
        {
            OtherLogger.Log("Updating CatDefs!", OtherLogger.LogType.General);

            //Go through all subcategories and add any that aren't pre-defined
            foreach(ItemSpawnerID.ESubCategory subcat in IM.CDefSubInfo.Keys)
            {
                if (!Enum.IsDefined(typeof(ItemSpawnerID.ESubCategory), subcat)){

                    OtherLogger.Log("Adding unique subcat: " + subcat.ToString(), OtherLogger.LogType.General);

                    __instance.m_simpleSpawnerPageDic[ItemSpawnerV2.PageMode.Firearms].TagGroups.Add(new ItemSpawnerCategoryDefinitionsV2.SpawnerPage.SpawnerTagGroup
                    {
                        DisplayName = IM.CDefSubInfo[subcat].DisplayName,
                        Icon = IM.CDefSubInfo[subcat].Sprite,
                        TagT = TagType.SubCategory,
                        Tag = subcat.ToString()
                    });
                }
            }
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
            
            if(OtherLoader.SpawnerEntries[data.CurrentPath].Count / __instance.IMG_SimpleTiles.Count > data.CurrentPage)
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




        [HarmonyPatch(typeof(ItemSpawnerV2), "SimpleSelectTile")]
        [HarmonyPrefix]
        private static bool SimpleButtonPatch(ItemSpawnerV2 __instance, int i)
        {
            ItemSpawnerData data = __instance.GetComponent<ItemSpawnerData>();

            //If the entry that was selected has child entries, we should display the child entries
            if (OtherLoader.SpawnerEntries[data.VisibleEntries[i].EntryPath].Count > 0)
            {
                data.CurrentPath = data.VisibleEntries[i].EntryPath;
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

            List<ItemSpawnerEntry> entries = OtherLoader.SpawnerEntries[data.CurrentPath].Where(o => o.IsDisplayedInMainEntry).ToList();
            //List<ItemSpawnerEntry> entries = OtherLoader.SpawnerEntries[data.CurrentPath];

            OtherLogger.Log($"Got {entries.Count} entries for path: {data.CurrentPath}", OtherLogger.LogType.General);

            entries = entries.OrderBy(o => o.DisplayName).OrderBy(o => o.IsModded?1:0).OrderBy(o => (OtherLoader.SpawnerEntries[o.EntryPath].Count > 0)?1:0).ToList();

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

            int numPages = Mathf.Max(entries.Count / __instance.IMG_SimpleTiles.Count, 1);

            __instance.TXT_SimpleTiles_PageNumber.text = (data.CurrentPage + 1) + " / " + numPages;
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

            if(data.CurrentPage < numPages)
            {
                __instance.GO_SimpleTiles_NextPage.SetActive(true);
            }
            else
            {
                __instance.GO_SimpleTiles_NextPage.SetActive(false);
            }

            return false;
        }



        /*
        [HarmonyPatch(typeof(ItemSpawnerV2), "RedrawSimpleCanvas")]
        [HarmonyILManipulator]
        private static void SortingPatch(ILContext ctx, MethodBase orig)
        {
            ILCursor c = new ILCursor(ctx);

            c.GotoNext(
                i => i.MatchLdfld(AccessTools.Field(typeof(ItemSpawnerV2), "WorkingItemIDs")),
                i => i.MatchCallvirt(AccessTools.Method(typeof(List<string>), "Sort", Type.EmptyTypes))
            );

            //Remove the original call to sort the list
            c.Index += 1;
            c.Remove();

            //Add the call to sort the list with this new sorting method
            c.Emit(OpCodes.Call, ((Action<List<string>>)SortItems).Method);
        }
        */






        private static void SortItems(List<string> workingIDs)
        {
            //First sort alphabetically
            workingIDs.Sort();

            //Then sort by wether the items are modded or not
            List<string> newList = workingIDs.OrderBy(o => IM.OD.ContainsKey(o) && IM.OD[o].IsModContent).ToList();
            workingIDs.Clear();
            workingIDs.AddRange(newList);
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
