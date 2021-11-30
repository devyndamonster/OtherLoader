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
using UnityEngine.UI;

namespace OtherLoader
{
    public class ItemSpawnerV2Patch
    {

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
            if(tagType == TagType.SubCategory)
            {
                if (Enum.IsDefined(typeof(ItemSpawnerID.ESubCategory), value)){
                    text.text = value;
                }
                else
                {
                    text.text = IM.CDefSubInfo[(ItemSpawnerID.ESubCategory)int.Parse(value)].DisplayName;
                }
            }
            else
            {
                text.text = value;
            }
        }



        [HarmonyPatch(typeof(IM), "RegisterItemIntoMetaTagSystem")]
        [HarmonyPostfix]
        private static void MetaTagPatch(ItemSpawnerID ID)
        {
            //If this IDs items didn't get added, add it to the firearm page
            if (IM.Instance.PageItemLists.ContainsKey(ItemSpawnerV2.PageMode.Firearms)){
                if (!IM.Instance.PageItemLists[ItemSpawnerV2.PageMode.Firearms].Contains(ID.ItemID))
                {
                    OtherLogger.Log("Adding misc item to meta tag system: " + ID.ItemID, OtherLogger.LogType.General);

                    IM.AddMetaTag(ID.Category.ToString(), TagType.Category, ID.ItemID, ItemSpawnerV2.PageMode.Firearms);
                    IM.AddMetaTag(ID.SubCategory.ToString(), TagType.SubCategory, ID.ItemID, ItemSpawnerV2.PageMode.Firearms);
                }
            }

        }

    }
}
