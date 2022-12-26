using NUnit.Framework;
using OtherLoader.Core.Controllers;
using OtherLoader.Core.Models;
using OtherLoader.Core.Services;
using FluentAssertions;

namespace OtherLoader.IntegrationTests.Controllers
{
    public class GetInitialStateTests
    {
        [Test]
        public void ItWillStartOnMainMenu()
        {
            var itemDataContainer = new ItemDataContainer();
            var pathService = new PathService();
            var itemSpawnerController = new ItemSpawnerController(itemDataContainer, pathService);

            var state = itemSpawnerController.GetInitialState();
                
            state.CurrentPath.Should().Be(PageMode.MainMenu.ToString());
        }

        [Test]
        public void ItWillHaveCorrectPageSizes()
        {
            var itemDataContainer = new ItemDataContainer();
            var pathService = new PathService();
            var itemSpawnerController = new ItemSpawnerController(itemDataContainer, pathService);

            var state = itemSpawnerController.GetInitialState();

            state.SimplePageSize.Should().Be(18);
        }
    }
}
