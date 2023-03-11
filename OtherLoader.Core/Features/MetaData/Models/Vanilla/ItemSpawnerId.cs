using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Core.Features.MetaData.Models.Vanilla
{
    public class ItemSpawnerId
    {

        public string ItemId { get; set; }

        public string MainObjectId { get; set; }

        public string SecondObjectId { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public string SubHeading { get; set; }

        public ItemCategory Category { get; set; }

        public SubCategory SubCategory { get; set; }

        public IEnumerable<ItemSpawnerId> Secondaries { get; set; }

        public IEnumerable<string> SecondariesByString { get; set; }

        public IEnumerable<string> ModTags { get; set; }

        public IEnumerable<string> TutorialBlocks { get; set; }

        public bool UsesLargeSpawnPad { get; set; }

        public bool UsesHugeSpawnPad { get; set; }

        public int UnlockCost { get; set; }

        public bool IsUnlockedByDefault { get; set; }

        public bool IsReward { get; set; }

        public bool IsDisplayedInMainEntry { get; set; }

    }
}
