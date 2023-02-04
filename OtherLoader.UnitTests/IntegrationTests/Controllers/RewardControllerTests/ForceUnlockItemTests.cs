using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using OtherLoader.Core.Adapters;
using OtherLoader.Core.Controllers;
using OtherLoader.Core.Models;
using OtherLoader.Core.Services;

namespace OtherLoader.IntegrationTests.Controllers.RewardControllerTests
{
    public class ForceUnlockItemTests
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
            var applicationPathService = Substitute.For<IApplicationPathService>();
            var rewardController = new RewardController(rewardAdapter, applicationPathService, itemData);

            rewardController.ForceUnlockItem("TestObject");
            var isUnlocked = rewardController.IsItemUnlocked("TestObject");

            isUnlocked.Should().BeTrue();
        }

        [Test]
        public void ItWillUnlockItem_WhenItemIsReward()
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
            var applicationPathService = Substitute.For<IApplicationPathService>();
            var rewardController = new RewardController(rewardAdapter, applicationPathService, itemData);

            rewardController.ForceUnlockItem("TestObject");
            var isUnlocked = rewardController.IsItemUnlocked("TestObject");

            isUnlocked.Should().BeTrue();
        }

        [Test]
        public void ItWillReturnTrue_WhenItemNotUnlocked()
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
            var applicationPathService = Substitute.For<IApplicationPathService>();
            var rewardController = new RewardController(rewardAdapter, applicationPathService, itemData);

            var unlocked = rewardController.ForceUnlockItem("TestObject");

            unlocked.Should().BeTrue();
        }

        [Test]
        public void ItWillReturnFalse_WhenItemAlreadyUnlocked()
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
            var applicationPathService = Substitute.For<IApplicationPathService>();
            var rewardController = new RewardController(rewardAdapter, applicationPathService, itemData);

            rewardController.ForceUnlockItem("TestObject");
            var unlocked = rewardController.ForceUnlockItem("TestObject");

            unlocked.Should().BeFalse();
        }

        [Test]
        public void ItWillReturnFalse_WhenItemDoesNotExist()
        {
            var itemData = new ItemDataContainer
            {
                ItemEntries = new SpawnerEntryData[0]
            };

            var rewardAdapter = Substitute.For<IRewardSystemAdapter>();
            var applicationPathService = Substitute.For<IApplicationPathService>();
            var rewardController = new RewardController(rewardAdapter, applicationPathService, itemData);

            var unlocked = rewardController.ForceUnlockItem("TestObject");

            unlocked.Should().BeFalse();
        }
    }
}
