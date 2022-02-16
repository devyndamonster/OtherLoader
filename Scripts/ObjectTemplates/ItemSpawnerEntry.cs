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
        public bool IsReward;

        [HideInInspector]
        public bool IsModded;

        
        public void PopulateIDsFromObj()
        {
            if(MainObjectObj != null)
            {
                MainObjectID = MainObjectObj.ItemID;
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


        public void LegacyPopulateFromID(ItemSpawnerV2.PageMode Page, ItemSpawnerID ID, bool IsModded)
        {
            if(ID.MainObject != null)
            {
                MainObjectID = ID.MainObject.ItemID;
            }
            else
            {
                OtherLogger.LogWarning("ItemSpawnerID has a null MainObject! ItemID: " + ID.ItemID);
                MainObjectID = ID.ItemID;
            }

            if (IM.OD.ContainsKey(MainObjectID))
            {
                OtherLoader.SpawnerEntriesByID[MainObjectID] = this;
            }

            SpawnWithIDs = new List<string>();
            if(ID.SecondObject != null)
            {
                SpawnWithIDs.Add(ID.SecondObject.ItemID);
            }
            

            //Add secondary items to entry, being careful of null values!
            SecondaryObjectIDs = new List<string>();
            foreach(ItemSpawnerID secondary in ID.Secondaries)
            {
                if(secondary != null && secondary.MainObject != null)
                {
                    SecondaryObjectIDs.Add(secondary.MainObject.ItemID);
                }
                else if(secondary == null)
                {
                    OtherLogger.LogWarning("Failed to add secondary to item (" + MainObjectID + ") due to secondary item being null!");
                }
                else
                {
                    OtherLogger.LogWarning("Failed to add secondary to item (" + MainObjectID + ") due to null MainObject!, Secondary display name: " + secondary.DisplayName);
                }
            }

            if(ID.Secondaries_ByStringID != null)
            {
                SecondaryObjectIDs.AddRange(ID.Secondaries_ByStringID);
            }


            EntryPath = CreatePath(Page, ID);
            EntryIcon = ID.Sprite;
            DisplayName = ID.DisplayName;

            IsDisplayedInMainEntry = ID.IsDisplayedInMainEntry;
            UsesLargeSpawnPad = ID.UsesLargeSpawnPad;
            this.IsModded = IsModded;

            TutorialBlockIDs = new List<string>();
            TutorialBlockIDs.AddRange(ID.TutorialBlocks);

            ModTags = new List<string>();
            ModTags.AddRange(ID.ModTags);
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


    }
}
