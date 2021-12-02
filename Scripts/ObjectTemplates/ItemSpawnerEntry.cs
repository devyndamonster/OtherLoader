using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OtherLoader
{
    public class ItemSpawnerEntry : ScriptableObject
    {
        public string MainObjectID;
        public List<string> SpawnWithIDs;
        public List<string> SecondaryObjectIDs;

        public string EntryPath;
        public Sprite EntryIcon;
        public string DisplayName;
        public bool IsDisplayedInMainEntry;
        public bool IsModded;
        public List<string> TutorialBlockIDs;

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
            if (!OtherLoader.SpawnerEntries.ContainsKey(currentPath))
            {
                OtherLoader.SpawnerEntries.Add(currentPath, new List<ItemSpawnerEntry>());
            }



            //If this is a modded ID that uses custom categories (or if it's meat fortress), we also include the category
            previousPath = currentPath;
            if (!Enum.IsDefined(typeof(ItemSpawnerID.EItemCategory), ID.Category) || ID.Category == ItemSpawnerID.EItemCategory.MeatFortress)
            {
                //Check if this category is already added, and if it's not then add it
                currentPath += "/" + IM.CDefInfo[ID.Category].DisplayName;
                if (!OtherLoader.SpawnerEntries.ContainsKey(currentPath))
                {
                    //First, add the current path to the entry dictionary
                    OtherLoader.SpawnerEntries.Add(currentPath, new List<ItemSpawnerEntry>());

                    //Then, add this entry to the previous path (page)
                    ItemSpawnerEntry categoryEntry = CreateInstance<ItemSpawnerEntry>();
                    categoryEntry.EntryPath = currentPath;
                    categoryEntry.EntryIcon = IM.CDefInfo[ID.Category].Sprite;
                    categoryEntry.DisplayName = IM.CDefInfo[ID.Category].DisplayName;
                    categoryEntry.IsDisplayedInMainEntry = true;
                    categoryEntry.IsModded = !Enum.IsDefined(typeof(ItemSpawnerID.EItemCategory), ID.Category);
                    OtherLoader.SpawnerEntries[previousPath].Add(categoryEntry);
                }
            }



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
            if (!OtherLoader.SpawnerEntries.ContainsKey(currentPath))
            {
                OtherLoader.SpawnerEntries.Add(currentPath, new List<ItemSpawnerEntry>());

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

                OtherLoader.SpawnerEntries[previousPath].Add(subcategoryEntry);
            }

            

            //Finally add the itemspawnerID as an entry to the dictionary
            previousPath = currentPath;
            currentPath += "/" + ID.MainObject.ItemID;
            if (!OtherLoader.SpawnerEntries.ContainsKey(currentPath))
            {
                OtherLoader.SpawnerEntries.Add(currentPath, new List<ItemSpawnerEntry>());

                //Then, add this entry to the previous path (subcategory)
                EntryPath = currentPath;
                OtherLoader.SpawnerEntries[previousPath].Add(this);
            }
        }

    }
}
