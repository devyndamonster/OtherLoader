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
            public void ItWillDisplayCorrectCategories_WhenPageSelected()
            {
                var itemData = new ItemDataContainer
                {
                    ItemEntries = new SpawnerEntryData[]
                    {
                        new SpawnerEntryData
                        {
                            DisplayText = "Pistols Category",
                            Path = "Firearms/Pistols",
                            IsDisplayedInMainEntry = true
                        },
                        new SpawnerEntryData
                        {
                            DisplayText = "Melee Category",
                            Path = "Melee/Swords",
                            IsDisplayedInMainEntry = true
                        }
                    }
                };
                
                var expectedTileStates = new []
                {
                    new ItemSpawnerTileState
                    {
                        Path = "Firearms/Pistols",
                        DisplayText = "Pistols Category"
                    }
                };

                var state = new ItemSpawnerState();
                var pathService = new PathService();
                var itemSpawnerController = new ItemSpawnerController(itemData, pathService);
                
                var newState = itemSpawnerController.PageSelected(state, PageMode.Firearms);

                newState.SimpleTileStates.ShouldBeEquivalentTo(expectedTileStates);
            }

            [Test]
            public void ItWontShowEntries_DisplayIsFalse()
            {
                var itemData = new ItemDataContainer
                {
                    ItemEntries = new SpawnerEntryData[]
                    {
                        new SpawnerEntryData
                        {
                            Path = "Firearms/Pistols",
                            IsDisplayedInMainEntry = true

                        },
                        new SpawnerEntryData
                        {
                            Path = "Firearms/SMGs",
                            IsDisplayedInMainEntry = false
                        }
                    }
                };

                var expectedTileStates = new[]
                {
                    new ItemSpawnerTileState
                    {
                        Path = "Firearms/Pistols"
                    }
                };

                var state = new ItemSpawnerState();
                var pathService = new PathService();
                var itemSpawnerController = new ItemSpawnerController(itemData, pathService);

                var newState = itemSpawnerController.PageSelected(state, PageMode.Firearms);

                newState.SimpleTileStates.ShouldBeEquivalentTo(expectedTileStates);
            }

            [Test]
            public void ItWontShowEntries_PathNotImmediateChild()
            {
                var itemData = new ItemDataContainer
                {
                    ItemEntries = new SpawnerEntryData[]
                    {
                        new SpawnerEntryData
                        {
                            Path = "Firearms/Pistols",
                            IsDisplayedInMainEntry = true
                        },
                        new SpawnerEntryData
                        {
                            Path = "Firearms/Pistols/Glock",
                            IsDisplayedInMainEntry = true
                        }
                    }
                };
                
                var expectedTileStates = new[]
                {
                    new ItemSpawnerTileState
                    {
                        Path = "Firearms/Pistols"
                    }
                };

                var state = new ItemSpawnerState();
                var pathService = new PathService();
                var itemSpawnerController = new ItemSpawnerController(itemData, pathService);

                var newState = itemSpawnerController.PageSelected(state, PageMode.Firearms);

                newState.SimpleTileStates.ShouldBeEquivalentTo(expectedTileStates);
            }
        }
    }
}
