using NUnit.Framework;
using OtherLoader.Core.Controllers;
using OtherLoader.Core.Models;
using OtherLoader.Core.Services;
using FluentAssertions;

namespace OtherLoader.IntegrationTests.Controllers.ItemSpawnerControllerTests
{
    public class GetInitialStateTests
    {
        [Test]
        public void ItWillStartOnMainMenu()
        {
            var itemDataContainer = new ItemDataContainer();
            var pathService = new PathService();
            var pageService = new PaginationService();
            var itemSpawnerController = new ItemSpawnerController(itemDataContainer, pathService, pageService);

            var state = itemSpawnerController.GetInitialState();
                
            state.SimpleState.CurrentPath.Should().Be(PageMode.MainMenu.ToString());
        }

        [Test]
        public void ItWillHaveCorrectPageSizes()
        {
            var itemDataContainer = new ItemDataContainer();
            var pathService = new PathService();
            var pageService = new PaginationService();
            var itemSpawnerController = new ItemSpawnerController(itemDataContainer, pathService, pageService);

            var state = itemSpawnerController.GetInitialState();
            
            state.SimpleState.PageSize.Should().Be(18);
        }
    }
}
