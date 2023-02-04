using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using OtherLoader.Core.Adapters;
using OtherLoader.Core.Controllers;
using OtherLoader.Core.Models;
using OtherLoader.Core.Services;

namespace OtherLoader.IntegrationTests.Controllers.RewardControllerTests
{
    public class IsItemUnlockedTests
    {
        [Test]
        public void ItWillNotBeUnlocked()
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

            var isUnlocked = rewardController.IsItemUnlocked("TestObject");

            isUnlocked.Should().BeFalse();
        }

        [Test]
        public void ItWillNotBeUnlocked_WhenItemDoesNotExist()
        {
            var itemData = new ItemDataContainer
            {
                ItemEntries = new SpawnerEntryData[0]
            };

            var rewardAdapter = Substitute.For<IRewardSystemAdapter>();
            var applicationPathService = Substitute.For<IApplicationPathService>();
            var rewardController = new RewardController(rewardAdapter, applicationPathService, itemData);

            var isUnlocked = rewardController.IsItemUnlocked("TestObject");

            isUnlocked.Should().BeFalse();
        }
    }
}
