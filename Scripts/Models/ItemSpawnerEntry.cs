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
        
        private string CreatePath(ItemSpawnerV2.PageMode Page, ItemSpawnerID ID)
        {
            string path = Page.ToString();

            //Some categories should still be included in the path
            if (ID.Category == ItemSpawnerID.EItemCategory.MeatFortress || 
                ID.Category == ItemSpawnerID.EItemCategory.Magazine || 
                ID.Category == ItemSpawnerID.EItemCategory.Cartridge || 
                ID.Category == ItemSpawnerID.EItemCategory.Clip || 
                ID.Category == ItemSpawnerID.EItemCategory.Speedloader)
            {
                path += "/" + ID.Category.ToString();
            }

            //If the category is modded, include it
            else if (!Enum.IsDefined(typeof(ItemSpawnerID.EItemCategory), ID.Category) && IM.CDefInfo.ContainsKey(ID.Category))
            {
                path += "/" + IM.CDefInfo[ID.Category].DisplayName;
            }

            //Include all subcategories that aren't none
            if (ID.SubCategory != ItemSpawnerID.ESubCategory.None)
            {
                if(Enum.IsDefined(typeof(ItemSpawnerID.ESubCategory), ID.SubCategory))
                {
                    path += "/" + ID.SubCategory.ToString();
                }

                else if(IM.CDefSubInfo.ContainsKey(ID.SubCategory))
                {
                    path += "/" + IM.CDefSubInfo[ID.SubCategory].DisplayName;
                }
            }

            path += "/" + MainObjectID;

            return path;
        }


        public ItemSpawnerID ConvertEntryToSpawnerID()
        {
            ItemSpawnerID.ESubCategory subcategory = GetSpawnerSubcategory();

            if (!IM.OD.ContainsKey(MainObjectID) || IsCategoryEntry())
            {
                return null;
            }

            ItemSpawnerID itemSpawnerID = CreateInstance<ItemSpawnerID>();

            foreach (ItemSpawnerCategoryDefinitions.Category category in IM.CDefs.Categories)
            {
                if (category.Subcats.Any(o => o.Subcat == subcategory))
                {
                    itemSpawnerID.Category = category.Cat;
                    itemSpawnerID.SubCategory = subcategory;
                }
            }

            itemSpawnerID.MainObject = IM.OD[MainObjectID];
            itemSpawnerID.SecondObject = SpawnWithIDs.Where(o => IM.OD.ContainsKey(o)).Select(o => IM.OD[o]).FirstOrDefault();
            itemSpawnerID.DisplayName = DisplayName;
            itemSpawnerID.IsDisplayedInMainEntry = IsDisplayedInMainEntry;
            itemSpawnerID.ItemID = MainObjectID;
            itemSpawnerID.ModTags = ModTags;
            itemSpawnerID.Secondaries_ByStringID = SecondaryObjectIDs.Where(o => IM.OD.ContainsKey(o)).ToList();
            itemSpawnerID.Secondaries = new ItemSpawnerID[] { };
            itemSpawnerID.Sprite = EntryIcon;
            itemSpawnerID.UsesLargeSpawnPad = UsesLargeSpawnPad;
            itemSpawnerID.UsesHugeSpawnPad = UsesHugeSpawnPad;
            itemSpawnerID.IsReward = IsReward;
            itemSpawnerID.TutorialBlocks = TutorialBlockIDs;

            return itemSpawnerID;
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
