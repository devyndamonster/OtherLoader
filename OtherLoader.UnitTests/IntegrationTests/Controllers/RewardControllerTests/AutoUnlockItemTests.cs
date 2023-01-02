using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using OtherLoader.Core.Adapters;
using OtherLoader.Core.Controllers;
using OtherLoader.Core.Models;

namespace OtherLoader.IntegrationTests.Controllers.RewardControllerTests
{
    public class AutoUnlockItemTests
    {
        [Test]
        public void ItWillUnlockItem_WhenItemIsNotReward()
        {
            var itemData = new ItemDataContainer
            {
                ItemEntries = new[]
                {
                    new SpawnerEntryData
                    {
                        MainObjectId = "TestObject",
                        IsReward = false
                    }
                }
            };

            var rewardAdapter = Substitute.For<IRewardSystemAdapter>();
            var rewardController = new RewardController(rewardAdapter, itemData);

            rewardController.AutoUnlockItem("TestObject");
            var isUnlocked = rewardController.IsItemUnlocked("TestObject");

            isUnlocked.Should().BeTrue();
        }

        [Test]
        public void ItWillNotUnlockItem_WhenItemIsReward()
        {
            var itemData = new ItemDataContainer
            {
                ItemEntries = new[]
                {
                    new SpawnerEntryData
                    {
                        MainObjectId = "TestObject",
                        IsReward = true
                    }
                }
            };

            var rewardAdapter = Substitute.For<IRewardSystemAdapter>();
            var rewardController = new RewardController(rewardAdapter, itemData);

            rewardController.AutoUnlockItem("TestObject");
            var isUnlocked = rewardController.IsItemUnlocked("TestObject");

            isUnlocked.Should().BeFalse();
        }
    }
}
