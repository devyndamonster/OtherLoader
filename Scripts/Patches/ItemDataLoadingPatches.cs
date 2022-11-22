using FistVR;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using OtherLoader.Loaders;
using OtherLoader.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace OtherLoader.Patches
{
    public static class ItemDataLoadingPatches
    {
        public static ISpawnerIdLoadingService _spawnerIdLoadingService;
        public static ISpawnerEntryLoadingService _spawnerEntryLoadingService;

        
        [HarmonyPatch(typeof(IM), "RegisterItemIntoMetaTagSystem")]
        [HarmonyPostfix]
        private static void MetaTagPatch(ItemSpawnerID ID)
        {
            //If this item is not a reward, unlock it by default
            if (ID.MainObject != null)
            {
                OtherLoader.SpawnerIDsByMainObject[ID.MainObject.ItemID] = ID;
                
                if (OtherLoader.UnlockSaveData.ShouldAutoUnlockItem(ID))
                {
                    OtherLoader.UnlockSaveData.UnlockItem(ID.MainObject.ItemID);
                }
            }

            //If this IDs items didn't get added, add it to the firearm page
            if (IM.Instance.PageItemLists.ContainsKey(ItemSpawnerV2.PageMode.Firearms))
            {
                if (!IM.Instance.PageItemLists.Any(o => o.Value.Contains(ID.ItemID)) && IM.OD.ContainsKey(ID.MainObject.ItemID) && IM.OD[ID.MainObject.ItemID].IsModContent)
                {
                    OtherLogger.Log("Adding misc mod item to meta tag system: " + ID.ItemID, OtherLogger.LogType.Loading);

                    IM.AddMetaTag(ID.Category.ToString(), TagType.Category, ID.ItemID, ItemSpawnerV2.PageMode.Firearms);
                    IM.AddMetaTag(ID.SubCategory.ToString(), TagType.SubCategory, ID.ItemID, ItemSpawnerV2.PageMode.Firearms);
                }
            }
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

        private static void PopulateSpawnerEntries()
        {
            foreach (var page in IM.CatDef.Pages)
            {
                var categoryTagGroups = page.TagGroups.Where(tag => tag.TagT == TagType.Category || tag.TagT == TagType.SubCategory);

                foreach (var tagGroup in categoryTagGroups)
                {
                    OtherLoader.CategoryDisplayData[tagGroup.Tag] = new CategoryDisplayData 
                    {
                        DisplayImage = tagGroup.Icon,
                        DisplayName = tagGroup.DisplayName,
                        CategoryTag = tagGroup.Tag,
                    };
                }
            }
            
            foreach (KeyValuePair<ItemSpawnerV2.PageMode, List<string>> pageLists in IM.Instance.PageItemLists)
            {
                foreach (string itemId in pageLists.Value)
                {
                    ItemSpawnerID spawnerId = IM.Instance.SpawnerIDDic[itemId];
                    var spawnerEntries = _spawnerIdLoadingService.GenerateRequiredSpawnerEntriesForSpawnerId(spawnerId);
                    _spawnerEntryLoadingService.AddItemSpawnerEntriesToPaths(spawnerEntries);
                }
            }
        }
        
        private static void RegisterItemSpawnerID(ItemSpawnerID spawnerID)
        {
            if (spawnerID.MainObject != null)
            {
                IM.RegisterItemIntoMetaTagSystem(spawnerID);
            }
        }
    }
}
