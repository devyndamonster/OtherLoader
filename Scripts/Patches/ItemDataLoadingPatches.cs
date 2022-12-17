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
        public static ISpawnerIdLoadingService _spawnerIdLoadingService = new SpawnerIdLoadingService(new PathService(), new MetaDataService(new PathService()));
        public static ISpawnerEntryLoadingService _spawnerEntryLoadingService = new SpawnerEntryLoadingService(new PathService());
        public static IMetaDataService _metaDataService = new MetaDataService(new PathService());

        
        [HarmonyPatch(typeof(IM), "RegisterItemIntoMetaTagSystem")]
        [HarmonyPostfix]
        private static void MetaTagPatch(ItemSpawnerID ID)
        {
            //If this item is not a reward, unlock it by default
            if (ID.MainObject != null)
            {
                if (OtherLoader.UnlockSaveData.ShouldAutoUnlockItem(ID))
                {
                    OtherLoader.UnlockSaveData.UnlockItem(ID.MainObject.ItemID);
                }
            }
        }

        [HarmonyPatch(typeof(IM), "GenerateItemDBs")]
        [HarmonyILManipulator]
        private static void ItemDBGenerationPatch(ILContext ctx, MethodBase orig)
        {
            ILCursor c = new ILCursor(ctx);
            
            c.GotoNext(
                i => i.MatchLdfld(AccessTools.Field(typeof(ItemSpawnerID), "MainObject"))
            );

            //Remove the check for items being unlocked to be registered
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
                    OtherLogger.Log($"Loading vanilla category {tagGroup.DisplayName} data into display data. Tag: {tagGroup.Tag}", OtherLogger.LogType.Loading);

                    OtherLoader.TagGroupsByTag[tagGroup.Tag] = tagGroup;
                }
            }
            
            foreach (KeyValuePair<ItemSpawnerV2.PageMode, List<string>> pageLists in IM.Instance.PageItemLists)
            {
                foreach (string itemId in pageLists.Value)
                {
                    OtherLogger.Log($"Loading vanilla ItemSpawnerId {itemId} into spawner entries", OtherLogger.LogType.Loading);

                    ItemSpawnerID spawnerId = OtherLoader.SpawnerIDsByMainObject[itemId];

                    var spawnerEntries = _spawnerIdLoadingService.GenerateRequiredSpawnerEntriesForSpawnerId(spawnerId);

                    OtherLogger.Log($"Loaded spawner entries:\n{string.Join("\n", spawnerEntries.Select(entry => entry.EntryPath).ToArray())}", OtherLogger.LogType.Loading);

                    _spawnerEntryLoadingService.AddItemSpawnerEntriesToPaths(spawnerEntries);
                }
            }
        }
        
        private static void RegisterItemSpawnerID(ItemSpawnerID spawnerID)
        {
            if (spawnerID.MainObject != null)
            {
                OtherLoader.SpawnerIDsByMainObject[spawnerID.MainObject.ItemID] = spawnerID;
                _metaDataService.RegisterSpawnerIDIntoTagSystem(spawnerID);
            }
        }
    }
}
