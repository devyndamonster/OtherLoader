using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using OtherLoader.Core.Adapters;
using OtherLoader.Core.Controllers;
using OtherLoader.Core.Models;
using OtherLoader.Core.Services;

namespace OtherLoader.IntegrationTests.Controllers
{
    [TestFixture]
    [Category("RewardController")]
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
            var applicationPathService = Substitute.For<IApplicationPathService>();
            var rewardController = new RewardController(rewardAdapter, applicationPathService, itemData);

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
            var applicationPathService = Substitute.For<IApplicationPathService>();
            var rewardController = new RewardController(rewardAdapter, applicationPathService, itemData);

            rewardController.AutoUnlockItem("TestObject");
            var isUnlocked = rewardController.IsItemUnlocked("TestObject");

            isUnlocked.Should().BeFalse();
        }
    }
}
