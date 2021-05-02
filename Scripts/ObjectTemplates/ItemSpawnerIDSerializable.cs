using Deli.Newtonsoft.Json;
using FistVR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OtherLoader
{
    public class ItemSpawnerIDSerializable
    {
        public bool IsDisplayedInMainEntry;
        public string DisplayName;
        public string SubHeading;
        public string Description;
        public string InfographicName;
        public ItemSpawnerID.EItemCategory Category;
        public ItemSpawnerID.ESubCategory SubCategory;
        public string ItemID;
        public string SecondaryID;
        public List<ItemSpawnerIDSerializable> Secondaries;
        public bool UsesLargeSpawnPad;
        public bool UsesHugeSpawnPad;
        public int UnlockCost;
        public bool IsUnlockedByDefault;
        public bool IsReward;
        
        [JsonIgnore]
        public Sprite Sprite;

        public ItemSpawnerIDSerializable(ItemSpawnerID ID)
        {
            if (ID == null) return;

            IsDisplayedInMainEntry = ID.IsDisplayedInMainEntry;
            DisplayName = ID.DisplayName;
            SubHeading = ID.SubHeading;
            Description = ID.Description;
            Category = ID.Category;
            SubCategory = ID.SubCategory;
            ItemID = ID.ItemID;
            UsesLargeSpawnPad = ID.UsesLargeSpawnPad;
            UsesHugeSpawnPad = ID.UsesHugeSpawnPad;
            UnlockCost = ID.UnlockCost;
            IsUnlockedByDefault = ID.IsUnlockedByDefault;
            IsReward = ID.IsReward;

            if(ID.Secondaries != null)
            {
                Secondaries = new List<ItemSpawnerIDSerializable>();
                foreach(ItemSpawnerID secondID in ID.Secondaries)
                {
                    Secondaries.Add(new ItemSpawnerIDSerializable(secondID));
                }
            }

            if(ID.SecondObject != null) SecondaryID = ID.SecondObject.ItemID ;

            if (ID.Infographic != null && ID.Infographic.Poster != null) InfographicName = ID.Infographic.Poster.name;
        }

        public ItemSpawnerID GetItemSpawnerID(string path)
        {
            ItemSpawnerID id = new ItemSpawnerID();

            id.IsDisplayedInMainEntry = IsDisplayedInMainEntry;
            id.DisplayName = DisplayName;
            id.SubHeading = SubHeading;
            id.Description = Description;
            id.Category = Category;
            id.SubCategory = SubCategory;
            id.ItemID = ItemID;
            id.UsesLargeSpawnPad = UsesLargeSpawnPad;
            id.UsesHugeSpawnPad = UsesHugeSpawnPad;
            id.UnlockCost = UnlockCost;
            id.IsUnlockedByDefault = IsUnlockedByDefault;
            id.IsReward = IsReward;

            if (IM.OD.ContainsKey(ItemID)) id.MainObject = IM.OD[ItemID];
            else OtherLogger.LogError("FVRObject not found in object dictionary when building ItemSpawnerID from serialized data. MainObjectID: " + ItemID);

            if (IM.OD.ContainsKey(SecondaryID)) id.SecondObject = IM.OD[SecondaryID];
            else if (!string.IsNullOrEmpty(SecondaryID)) OtherLogger.LogError("FVRObject not found in object dictionary when building ItemSpawnerID from serialized data. SecondObjectID: " + SecondaryID);

            if (Secondaries != null) id.Secondaries = Secondaries.Select(o => o.GetItemSpawnerID(path)).ToArray();

            string iconPath = Path.Combine(path, CacheManager.ICON_PREFIX + ItemID + ".png");
            if (File.Exists(iconPath))
            {
                id.Sprite = LoaderUtils.LoadSprite(iconPath);
            }

            iconPath = Path.Combine(path, CacheManager.INFO_PREFIX + InfographicName + ".png");
            if (File.Exists(iconPath))
            {
                id.Infographic = new ItemSpawnerControlInfographic();
                id.Infographic.Poster = LoaderUtils.LoadTexture(iconPath);
            }

            return id;
        }


    }
}
