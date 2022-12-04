using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OtherLoader
{


    [CreateAssetMenu(menuName = "MeatKit/Otherloader/SpawnerEntry", fileName = "New Spawner Entry")]
    public class ItemSpawnerEntry : ScriptableObject
    {
        [Header("Item IDs")]
        [Tooltip("ItemID for the main object that will spawn")]
        public string MainObjectID;

        [Tooltip("ItemIDs for items that will spawn alongside the main object")]
        public List<string> SpawnWithIDs = new List<string>();

        [Tooltip("ItemIDs for items that appear in the secondary items section")]
        public List<string> SecondaryObjectIDs = new List<string>();

        [Header("[OPTIONAL] Populate ItemIDs using FVRObjects directly")]
        public FVRObject MainObjectObj;
        public List<FVRObject> SpawnWithObjs = new List<FVRObject>();
        public List<FVRObject> SecondaryObjs = new List<FVRObject>();

        [Header("Entry Path Properties")]
        [Tooltip("The path for the entry")]
        public string EntryPath;

        public ItemSpawnerV2.PageMode Page;

        public ItemSpawnerID.ESubCategory SubCategory;


        [Header("Display Properties")]
        [Tooltip("The icon that will appear in the spawner for this entry")]
        public Sprite EntryIcon;

        [Tooltip("The name of the entry")]
        public string DisplayName;

        [Tooltip("Decides wether the entry will be visible in the spawner.\n Set to false if you only want the entry visible as a secondary")]
        public bool IsDisplayedInMainEntry;

        [Tooltip("A list modding tags to allow for sorting by mod groups in itemspawner")]
        public List<string> ModTags = new List<string>();

        [Tooltip("A list of tutorial block IDs that will appear when this entry is selected")]
        public List<string> TutorialBlockIDs;


        [Header("Misc Properties")]
        public bool UsesLargeSpawnPad;
        public bool UsesHugeSpawnPad;
        public bool IsReward;

        [HideInInspector]
        public bool IsModded;

        
        public static ItemSpawnerEntry CreateEmpty(string path)
        {
            var entry = ScriptableObject.CreateInstance<ItemSpawnerEntry>();
            entry.EntryPath = path;
            return entry;
        }

        public void PopulateIDsFromObj()
        {
            if(MainObjectObj != null)
            {
                MainObjectID = MainObjectObj.ItemID;
                OtherLogger.Log("Assigning ItemID from MainObject: " + MainObjectID, OtherLogger.LogType.Loading);
            }

            foreach(FVRObject obj in SpawnWithObjs)
            {
                if (!SpawnWithIDs.Contains(obj.ItemID))
                {
                    SpawnWithIDs.Add(obj.ItemID);
                }
            }

            foreach (FVRObject obj in SecondaryObjs)
            {
                if (!SecondaryObjectIDs.Contains(obj.ItemID))
                {
                    SecondaryObjectIDs.Add(obj.ItemID);
                }
            }
        }
        
        public bool IsCategoryEntry()
        {
            return string.IsNullOrEmpty(MainObjectID);
        }
        
        public ItemSpawnerID.ESubCategory GetSpawnerSubcategory()
        {
            return EntryPath
                .Split('/')
                .Where(o => Enum.IsDefined(typeof(ItemSpawnerID.ESubCategory), o))
                .Select(o => (ItemSpawnerID.ESubCategory)Enum.Parse(typeof(ItemSpawnerID.ESubCategory), o))
                .FirstOrDefault();
        }


    }
}
