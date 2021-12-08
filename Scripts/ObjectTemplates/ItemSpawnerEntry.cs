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

        

        public void PopulateEntry(ItemSpawnerV2.PageMode Page, ItemSpawnerID ID, bool IsModded)
        {
            MainObjectID = ID.ItemID;

            SpawnWithIDs = new List<string>();
            if(ID.SecondObject != null)
            {
                SpawnWithIDs.Add(ID.SecondObject.ItemID);
            }
            
            
            SecondaryObjectIDs = ID.Secondaries.Select(o => o.ItemID).ToList();
            SecondaryObjectIDs.AddRange(ID.Secondaries_ByStringID);

            EntryIcon = ID.Sprite;
            DisplayName = ID.DisplayName;

            IsDisplayedInMainEntry = ID.IsDisplayedInMainEntry;
            UsesHugeSpawnPad = ID.UsesHugeSpawnPad;
            UsesLargeSpawnPad = ID.UsesLargeSpawnPad;
            this.IsModded = IsModded;

            TutorialBlockIDs = new List<string>();
            TutorialBlockIDs.AddRange(ID.TutorialBlocks);

            PopulatePaths(Page, ID, IsModded);
        }




        /// <summary>
        /// Converts legacy ItemSpawnerIDs into a new tree based format, and adds this converted info to a global dictionary
        /// </summary>
        /// <param name="Page"></param>
        /// <param name="ID"></param>
        private void PopulatePaths(ItemSpawnerV2.PageMode Page, ItemSpawnerID ID, bool IsModded)
        {
            //Add the page entry to the dictionary
            string currentPath = Page.ToString();
            string previousPath = currentPath;
            if (!OtherLoader.SpawnerEntriesByPath.ContainsKey(currentPath))
            {
                OtherLoader.SpawnerEntriesByPath.Add(currentPath, new List<ItemSpawnerEntry>());
            }



            //If this is a modded ID that uses custom categories (or if it's meat fortress), we also include the category
            previousPath = currentPath;
            if ((!Enum.IsDefined(typeof(ItemSpawnerID.EItemCategory), ID.Category) && IM.CDefInfo.ContainsKey(ID.Category)) || ID.Category == ItemSpawnerID.EItemCategory.MeatFortress)
            {
                //Check if this category is already added, and if it's not then add it
                currentPath += "/" + IM.CDefInfo[ID.Category].DisplayName;
                if (!OtherLoader.SpawnerEntriesByPath.ContainsKey(currentPath))
                {
                    //First, add the current path to the entry dictionary
                    OtherLoader.SpawnerEntriesByPath.Add(currentPath, new List<ItemSpawnerEntry>());

                    //Then, add this entry to the previous path (page)
                    ItemSpawnerEntry categoryEntry = CreateInstance<ItemSpawnerEntry>();
                    categoryEntry.EntryPath = currentPath;
                    categoryEntry.EntryIcon = IM.CDefInfo[ID.Category].Sprite;
                    categoryEntry.DisplayName = IM.CDefInfo[ID.Category].DisplayName;
                    categoryEntry.IsDisplayedInMainEntry = true;
                    categoryEntry.IsModded = !Enum.IsDefined(typeof(ItemSpawnerID.EItemCategory), ID.Category);
                    OtherLoader.SpawnerEntriesByPath[previousPath].Add(categoryEntry);
                }
            }



            //If the subcategory is not 'none', we inlcude the subcategory
            if(ID.SubCategory != ItemSpawnerID.ESubCategory.None)
            {
                //Update the path
                previousPath = currentPath;
                if (IM.CDefSubInfo.ContainsKey(ID.SubCategory))
                {
                    currentPath += "/" + IM.CDefSubInfo[ID.SubCategory].DisplayName;
                }
                else
                {
                    currentPath += "/" + ID.SubCategory.ToString();
                }

                //Check if this subcategory is already added, and if it's not then add it
                if (!OtherLoader.SpawnerEntriesByPath.ContainsKey(currentPath))
                {
                    OtherLoader.SpawnerEntriesByPath.Add(currentPath, new List<ItemSpawnerEntry>());

                    //Then, add this entry to the previous path (category)
                    ItemSpawnerEntry subcategoryEntry = CreateInstance<ItemSpawnerEntry>();
                    subcategoryEntry.EntryPath = currentPath;
                    subcategoryEntry.IsDisplayedInMainEntry = true;
                    subcategoryEntry.IsModded = !Enum.IsDefined(typeof(ItemSpawnerID.ESubCategory), ID.SubCategory);

                    if (IM.CDefSubInfo.ContainsKey(ID.SubCategory))
                    {
                        subcategoryEntry.EntryIcon = IM.CDefSubInfo[ID.SubCategory].Sprite;
                        subcategoryEntry.DisplayName = IM.CDefSubInfo[ID.SubCategory].DisplayName;
                    }
                    else
                    {
                        subcategoryEntry.DisplayName = ID.SubCategory.ToString();
                    }

                    OtherLoader.SpawnerEntriesByPath[previousPath].Add(subcategoryEntry);
                }
            }
            

            

            //Finally add the itemspawnerID as an entry to the dictionary
            previousPath = currentPath;
            currentPath += "/" + ID.ItemID;
            if (!OtherLoader.SpawnerEntriesByPath.ContainsKey(currentPath))
            {
                OtherLoader.SpawnerEntriesByPath.Add(currentPath, new List<ItemSpawnerEntry>());

                //Then, add this entry to the previous path (subcategory)
                EntryPath = currentPath;
                OtherLoader.SpawnerEntriesByPath[previousPath].Add(this);
            }
        }

    }
}
