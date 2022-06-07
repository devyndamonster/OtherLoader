using FistVR;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OtherLoader.Patches
{
    public class ItemSpawningPatches
    {


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


        private static IEnumerator SpawnItems(ItemSpawnerV2 instance, ItemSpawnerEntry entry)
        {
            List<AnvilCallback<GameObject>> itemsToSpawn = new List<AnvilCallback<GameObject>>();

            itemsToSpawn.Add(IM.OD[entry.MainObjectID].GetGameObjectAsync());
            itemsToSpawn.AddRange(entry.SpawnWithIDs.Select(o => IM.OD[o].GetGameObjectAsync()));

            for (int i = 0; i < itemsToSpawn.Count; i++)
            {
                yield return itemsToSpawn[i];

                Transform spawnPoint = GetItemSpawnPoint(instance, entry, i);
                GameObject spawnedItem = UnityEngine.Object.Instantiate(itemsToSpawn[i].Result, spawnPoint.position, spawnPoint.rotation);
                FVRPhysicalObject physComponent = spawnedItem.GetComponent<FVRPhysicalObject>();

                ItemSpawnerID relatedSpawnerID;
                if (OtherLoader.SpawnerIDsByMainObject.TryGetValue(physComponent.ObjectWrapper.ItemID, out relatedSpawnerID))
                {
                    physComponent.IDSpawnedFrom = OtherLoader.SpawnerIDsByMainObject[physComponent.ObjectWrapper.ItemID];
                }
            }
        }

        private static Transform GetItemSpawnPoint(ItemSpawnerV2 instance, ItemSpawnerEntry entry, int spawnedIndex)
        {
            if (spawnedIndex == 0 && entry.UsesLargeSpawnPad)
            {
                return instance.SpawnPoint_Large;
            }
            else if (spawnedIndex == 0 && entry.UsesHugeSpawnPad)
            {
                return instance.SpawnPoint_Huge;
            }
            else
            {
                IncrementSmallSpawnPosition(instance);
                return instance.SpawnPoints_Small[instance.m_curSmallPos];
            }
        }

        private static void IncrementSmallSpawnPosition(ItemSpawnerV2 instance)
        {
            instance.m_curSmallPos = (instance.m_curSmallPos + 1) % 3;
        }
    }
}
