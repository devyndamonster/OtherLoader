using FistVR;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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


        [HarmonyPatch(typeof(IM), "RegisterItemIntoMetaTagSystem")]
        [HarmonyPostfix]
        private static void MetaTagPatch(ItemSpawnerID ID)
        {
            //If this IDs items didn't get added, add it to the firearm page
            if (ID.MainObject != null && IM.Instance.PageItemLists.ContainsKey(ItemSpawnerV2.PageMode.Firearms)){
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
