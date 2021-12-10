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
        public List<string> SpawnWithIDs;

        [Tooltip("ItemIDs for items that appear in the secondary items section")]
        public List<string> SecondaryObjectIDs;


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

        [Tooltip("A list of tutorial block IDs that will appear when this entry is selected")]
        public List<string> TutorialBlockIDs;


        [Header("Misc Properties")]
        public bool UsesLargeSpawnPad;
        public bool UsesHugeSpawnPad;

        [HideInInspector]
        public bool IsModded;

        

        public void LegacyPopulateFromID(ItemSpawnerV2.PageMode Page, ItemSpawnerID ID, bool IsModded)
        {
            MainObjectID = ID.ItemID;

            SpawnWithIDs = new List<string>();
            if(ID.SecondObject != null)
            {
                SpawnWithIDs.Add(ID.SecondObject.ItemID);
            }
            
            SecondaryObjectIDs = ID.Secondaries.Select(o => o.ItemID).ToList();
            SecondaryObjectIDs.AddRange(ID.Secondaries_ByStringID);

            EntryPath = CreatePath(Page, ID);
            EntryIcon = ID.Sprite;
            DisplayName = ID.DisplayName;

            IsDisplayedInMainEntry = ID.IsDisplayedInMainEntry;
            UsesHugeSpawnPad = ID.UsesHugeSpawnPad;
            UsesLargeSpawnPad = ID.UsesLargeSpawnPad;
            this.IsModded = IsModded;

            TutorialBlockIDs = new List<string>();
            TutorialBlockIDs.AddRange(ID.TutorialBlocks);
        }



        private string CreatePath(ItemSpawnerV2.PageMode Page, ItemSpawnerID ID)
        {
            string path = Page.ToString();

            //If the category is meatfortress, include it
            if (ID.Category == ItemSpawnerID.EItemCategory.MeatFortress)
            {
                path += "/" + ID.Category.ToString();
            }

            //If the category is modded, include it
            else if (!Enum.IsDefined(typeof(ItemSpawnerID.EItemCategory), ID.Category))
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

                else
                {
                    path += "/" + IM.CDefSubInfo[ID.SubCategory].DisplayName;
                }
            }

            path += "/" + ID.ItemID;

            return path;
        }


    }
}
