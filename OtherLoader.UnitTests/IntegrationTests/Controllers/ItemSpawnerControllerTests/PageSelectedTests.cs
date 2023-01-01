using NUnit.Framework;
using OtherLoader.Core.Controllers;
using OtherLoader.Core.Models;
using OtherLoader.Core.Services;
using FluentAssertions;

namespace OtherLoader.IntegrationTests.Controllers.ItemSpawnerControllerTests
{
    public class PageSelectedTests
    {

        [Test]
        public void ItWillDisplayCorrectDataForEntry()
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
                    }
                }
            };

            var expectedTileStates = new ItemSpawnerTileState[]
            {
                new ItemSpawnerTileState
                {
                    DisplayText = "Pistols Category",
                    Path = "Firearms/Pistols",
                }
            };

            var state = new ItemSpawnerState
            {
                SimpleState = new()
                {
                    PageSize = 1
                }
            };

            var pathService = new PathService();
            var pageService = new PaginationService();
            var itemSpawnerController = new ItemSpawnerController(itemData, pathService, pageService);

            var newState = itemSpawnerController.PageSelected(state, PageMode.Firearms);

            newState.SimpleState.TileStates.ShouldBeEquivalentTo(expectedTileStates); 
        }

        [Test]
        public void ItWillDisplayCorrectEntries()
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
            
            var state = new ItemSpawnerState
            {
                SimpleState = new()
                {
                    PageSize = 2
                }
            };
            
            var pathService = new PathService();
            var pageService = new PaginationService();
            var itemSpawnerController = new ItemSpawnerController(itemData, pathService, pageService);
                
            var newState = itemSpawnerController.PageSelected(state, PageMode.Firearms);

            newState.SimpleState.TileStates.Should().Contain(tile => tile.Path == "Firearms/Pistols");
            newState.SimpleState.TileStates.Should().NotContain(tile => tile.Path == "Melee/Swords");
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

            var state = new ItemSpawnerState
            {
                SimpleState = new()
                {
                    PageSize = 2
                }
            };
            
            var pathService = new PathService();
            var pageService = new PaginationService();
            var itemSpawnerController = new ItemSpawnerController(itemData, pathService, pageService);

            var newState = itemSpawnerController.PageSelected(state, PageMode.Firearms);

            newState.SimpleState.TileStates.Should().Contain(tile => tile.Path == "Firearms/Pistols");
            newState.SimpleState.TileStates.Should().NotContain(tile => tile.Path == "Firearms/SMGs");
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

            var state = new ItemSpawnerState
            {
                SimpleState = new()
                {
                    PageSize = 2
                }
            };
            
            var pathService = new PathService();
            var pageService = new PaginationService();
            var itemSpawnerController = new ItemSpawnerController(itemData, pathService, pageService);

            var newState = itemSpawnerController.PageSelected(state, PageMode.Firearms);

            newState.SimpleState.TileStates.Should().Contain(tile => tile.Path == "Firearms/Pistols");
            newState.SimpleState.TileStates.Should().NotContain(tile => tile.Path == "Firearms/Pistols/Glock");
        }
        
        [Test]
        public void ItWillStartOnFirstPage()
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
                        IsDisplayedInMainEntry = true
                    },
                    new SpawnerEntryData
                    {
                        Path = "Firearms/Rifles",
                        IsDisplayedInMainEntry = true
                    }
                }
            };

            var expectedTileStates = new[]
            {
                new ItemSpawnerTileState
                {
                    Path = "Firearms/Pistols"
                },
                new ItemSpawnerTileState
                {
                    Path = "Firearms/SMGs"
                }
            };
            
            var state = new ItemSpawnerState
            {
                SimpleState = new()
                {
                    PageSize = 2
                }
            };

            var pathService = new PathService();
            var pageService = new PaginationService();
            var itemSpawnerController = new ItemSpawnerController(itemData, pathService, pageService);

            var newState = itemSpawnerController.PageSelected(state, PageMode.Firearms);

            newState.SimpleState.TileStates.ShouldBeEquivalentTo(expectedTileStates);
            newState.SimpleState.CurrentPage.Should().Be(0);
        }

        [Test]
        public void ItWillDisableNextPageButton_WhenFirstPageNotFull()
        {
            var itemData = new ItemDataContainer
            {
                ItemEntries = new SpawnerEntryData[]
                {
                    new SpawnerEntryData
                    {
                        Path = "Firearms/Pistols",
                        IsDisplayedInMainEntry = true
                    }
                }
            };

            var state = new ItemSpawnerState
            {
                SimpleState = new()
                {
                    PageSize = 2
                }
            };

            var pathService = new PathService();
            var pageService = new PaginationService();
            var itemSpawnerController = new ItemSpawnerController(itemData, pathService, pageService);

            var newState = itemSpawnerController.PageSelected(state, PageMode.Firearms);

            newState.SimpleState.NextPageEnabled.Should().BeFalse();
        }

        [Test]
        public void ItWillDisableNextPageButton_WhenFirstPageExactlyFull()
        {
            var itemData = new ItemDataContainer
            {
                ItemEntries = new SpawnerEntryData[]
                {
                    new SpawnerEntryData
                    {
                        Path = "Firearms/Pistols",
                        IsDisplayedInMainEntry = true
                    }
                }
            };

            var state = new ItemSpawnerState
            {
                SimpleState = new()
                {
                    PageSize = 1
                }
            };

            var pathService = new PathService();
            var pageService = new PaginationService();
            var itemSpawnerController = new ItemSpawnerController(itemData, pathService, pageService);

            var newState = itemSpawnerController.PageSelected(state, PageMode.Firearms);

            newState.SimpleState.NextPageEnabled.Should().BeFalse();
        }

        [Test]
        public void ItWillEnableNextPageButton_WhenNotEnoughRoomOnFirstPage()
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
                        IsDisplayedInMainEntry = true
                    }
                }
            };

            var state = new ItemSpawnerState
            {
                SimpleState = new()
                {
                    PageSize = 1
                }
            };

            var pathService = new PathService();
            var pageService = new PaginationService();
            var itemSpawnerController = new ItemSpawnerController(itemData, pathService, pageService);

            var newState = itemSpawnerController.PageSelected(state, PageMode.Firearms);

            newState.SimpleState.NextPageEnabled.Should().BeTrue();
        }

        [Test]
        public void ItWillDisablePrevPageButton_OnFirstPage()
        {
            var itemData = new ItemDataContainer
            {
                ItemEntries = new SpawnerEntryData[]
                {
                    new SpawnerEntryData
                    {
                        Path = "Firearms/Pistols",
                        IsDisplayedInMainEntry = true
                    }
                }
            };

            var state = new ItemSpawnerState
            {
                SimpleState = new()
                {
                    PageSize = 1
                }
            };

            var pathService = new PathService();
            var pageService = new PaginationService();
            var itemSpawnerController = new ItemSpawnerController(itemData, pathService, pageService);

            var newState = itemSpawnerController.PageSelected(state, PageMode.Firearms);

            newState.SimpleState.PrevPageEnabled.Should().BeFalse();
        }

        [Test]
        public void ItWillHaveEmptyTilePath_WhenNotEntryForTile()
        {
            var itemData = new ItemDataContainer
            {
                ItemEntries = new SpawnerEntryData[]
                {
                    new SpawnerEntryData
                    {
                        Path = "Firearms/Pistols",
                        IsDisplayedInMainEntry = true
                    }
                }
            };

            var expectedTileStates = new[]
            {
                new ItemSpawnerTileState
                {
                    Path = "Firearms/Pistols"
                },
                new ItemSpawnerTileState
                {
                    Path = ""
                }
            };

            var state = new ItemSpawnerState
            {
                SimpleState = new()
                {
                    PageSize = 2
                }
            };

            var pathService = new PathService();
            var pageService = new PaginationService();
            var itemSpawnerController = new ItemSpawnerController(itemData, pathService, pageService);

            var newState = itemSpawnerController.PageSelected(state, PageMode.Firearms);

            newState.SimpleState.TileStates.ShouldBeEquivalentTo(expectedTileStates);
        }
    }
}
