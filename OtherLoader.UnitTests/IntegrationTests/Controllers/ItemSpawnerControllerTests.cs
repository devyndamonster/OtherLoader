using NUnit.Framework;
using OtherLoader.Core.Controllers;
using OtherLoader.Core.Models;
using OtherLoader.Core.Services;
using FluentAssertions;

namespace OtherLoader.IntegrationTests.Controllers
{
    public class ItemSpawnerControllerTests
    {
        public class GetInitialState
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
        }

        public class PageSelected
        {
            [Test]
            public void ItWillDisplayFirearmCategories_WhenFirearmPageSelected()
            {
                var itemData = new ItemDataContainer
                {
                    ItemEntries = new SpawnerEntryData[]
                    {
                        new SpawnerEntryData
                        {
                            DisplayText = "Pistols Category",
                            Path = "Firearms/Pistols"
                        },
                        new SpawnerEntryData
                        {
                            DisplayText = "Melee Category",
                            Path = "Melee/Swords"
                        }
                    }
                };

                var state = new ItemSpawnerState
                {
                    CurrentPath = "MainMenu",
                    SimpleTileStates = {}
                };

                var expectedTileStates = new []
                {
                    new ItemSpawnerTileState
                    {
                        Path = "Firearms/Pistols",
                        DisplayText = "Pistols Category"
                    }
                };
                
                var pathService = new PathService();
                var itemSpawnerController = new ItemSpawnerController(itemData, pathService);
                
                var newState = itemSpawnerController.PageSelected(state, PageMode.Firearms);

                newState.SimpleTileStates.ShouldBeEquivalentTo(expectedTileStates);
            }
        }
    }
}
