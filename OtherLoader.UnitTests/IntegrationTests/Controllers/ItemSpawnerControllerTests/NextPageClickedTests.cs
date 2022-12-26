using FluentAssertions;
using NUnit.Framework;
using OtherLoader.Core.Controllers;
using OtherLoader.Core.Models;
using OtherLoader.Core.Services;
using System.Collections.Generic;

namespace OtherLoader.IntegrationTests.Controllers.ItemSpawnerControllerTests
{
    public class NextPageClickedTests
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
                    },
                    new SpawnerEntryData
                    {
                        DisplayText = "SMGs Category",
                        Path = "Firearms/SMGs",
                        IsDisplayedInMainEntry = true
                    }
                }
            };

            var expectedTileStates = new ItemSpawnerTileState[]
            {
                new ItemSpawnerTileState
                {
                    DisplayText = "SMGs Category",
                    Path = "Firearms/SMGs",
                }
            };

            var state = new ItemSpawnerState
            {
                SimplePageSize = 1,
                CurrentPath = "Firearms",
                SavedPathsToPages = new Dictionary<string, int>
                {
                    { "Firearms", 0 }
                }
            };

            var pathService = new PathService();
            var pageService = new PaginationService();
            var itemSpawnerController = new ItemSpawnerController(itemData, pathService, pageService);

            var newState = itemSpawnerController.NextPageClicked(state);

            newState.SimpleTileStates.ShouldBeEquivalentTo(expectedTileStates);
        }


        [Test]
        public void ItWillUpdateCurrentPage()
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
                SimplePageSize = 1,
                CurrentPath = "Firearms",
                SavedPathsToPages = new Dictionary<string, int>
                {
                    { "Firearms", 0 }
                }
            };

            var pathService = new PathService();
            var pageService = new PaginationService();
            var itemSpawnerController = new ItemSpawnerController(itemData, pathService, pageService);

            var newState = itemSpawnerController.NextPageClicked(state);

            newState.SimpleCurrentPage.Should().Be(1);
        }

        [Test]
        public void ItWillDisplayNextPagesEntries()
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
                SimplePageSize = 1,
                CurrentPath = "Firearms",
                SavedPathsToPages = new Dictionary<string, int>
                {
                    { "Firearms", 0 }
                }
            };

            var pathService = new PathService();
            var pageService = new PaginationService();
            var itemSpawnerController = new ItemSpawnerController(itemData, pathService, pageService);

            var newState = itemSpawnerController.NextPageClicked(state);

            newState.SimpleTileStates.Should().ContainSingle(tile => tile.Path == "Firearms/SMGs");
        }

        [Test]
        public void ItWillDisableNextPageButton_WhenNoNextPage()
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
                SimplePageSize = 1,
                CurrentPath = "Firearms",
                SavedPathsToPages = new Dictionary<string, int>
                {
                    { "Firearms", 0 }
                }
            };

            var pathService = new PathService();
            var pageService = new PaginationService();
            var itemSpawnerController = new ItemSpawnerController(itemData, pathService, pageService);

            var newState = itemSpawnerController.NextPageClicked(state);

            newState.SimpleNextPageEnabled.Should().BeFalse();
        }

        [Test]
        public void ItWillEnableNextPageButton_WhenNextPage()
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

            var state = new ItemSpawnerState
            {
                SimplePageSize = 1,
                CurrentPath = "Firearms",
                SavedPathsToPages = new Dictionary<string, int>
                {
                    { "Firearms", 0 }
                }
            };

            var pathService = new PathService();
            var pageService = new PaginationService();
            var itemSpawnerController = new ItemSpawnerController(itemData, pathService, pageService);

            var newState = itemSpawnerController.NextPageClicked(state);

            newState.SimpleNextPageEnabled.Should().BeTrue();
        }

        [Test]
        public void ItWillEnablePrevPageButton_WhenHasPrevPage()
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
                SimplePageSize = 1,
                CurrentPath = "Firearms",
                SavedPathsToPages = new Dictionary<string, int>
                {
                    { "Firearms", 0 }
                }
            };

            var pathService = new PathService();
            var pageService = new PaginationService();
            var itemSpawnerController = new ItemSpawnerController(itemData, pathService, pageService);

            var newState = itemSpawnerController.NextPageClicked(state);

            newState.SimplePrevPageEnabled.Should().BeTrue();
        }
        

    }
}
