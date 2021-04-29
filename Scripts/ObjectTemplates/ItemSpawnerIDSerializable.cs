using Deli.Newtonsoft.Json;
using FistVR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OtherLoader.Scripts.ObjectTemplates
{
    public class ItemSpawnerIDSerializable
    {
        public bool IsDisplayedInMainEntry;
        public string DisplayName;
        public string SpriteName;
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
        private ItemSpawnerID id;

        public ItemSpawnerIDSerializable(ItemSpawnerID ID)
        {
            id = ID;

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

            if (ID.Sprite != null) SpriteName = ID.Sprite.name + ".png";

            if (ID.Infographic != null) InfographicName = ID.Infographic.name + ".png";

        }

        public ItemSpawnerID GetItemSpawnerID(string path)
        {
            if(id == null)
            {
                id = new ItemSpawnerID();

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

                if(File.Exists(path + SpriteName + ".png"))
                {
                    id.Sprite = LoaderUtils.LoadSprite(path + SpriteName + ".png");
                }

                if(File.Exists(path + InfographicName + ".png"))
                {
                    id.Infographic = new ItemSpawnerControlInfographic();
                    id.Infographic.Poster = LoaderUtils.LoadTexture(path + InfographicName + ".png");
                }

            }

            return id;
        }


    }
}
