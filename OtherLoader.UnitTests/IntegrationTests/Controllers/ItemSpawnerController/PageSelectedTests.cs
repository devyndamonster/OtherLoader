using NUnit.Framework;
using OtherLoader.Core.Controllers;
using OtherLoader.Core.Models;
using OtherLoader.Core.Services;
using FluentAssertions;

namespace OtherLoader.IntegrationTests.Controllers
{
    public class PageSelectedTests
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

            var state = new ItemSpawnerState
            {
                SimplePageSize = 2
            };

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

            var state = new ItemSpawnerState
            {
                SimplePageSize = 2
            };
            
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

            var state = new ItemSpawnerState
            {
                SimplePageSize = 2
            };
            
            var pathService = new PathService();
            var itemSpawnerController = new ItemSpawnerController(itemData, pathService);

            var newState = itemSpawnerController.PageSelected(state, PageMode.Firearms);

            newState.SimpleTileStates.ShouldBeEquivalentTo(expectedTileStates);
        }
        
        [Test]
        public void ItShouldStartOnFirstPage()
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
                SimplePageSize = 2
            };

            var pathService = new PathService();
            var itemSpawnerController = new ItemSpawnerController(itemData, pathService);

            var newState = itemSpawnerController.PageSelected(state, PageMode.Firearms);

            newState.SimpleTileStates.ShouldBeEquivalentTo(expectedTileStates);
            newState.SimpleCurrentPage.Should().Be(0);
        }

        [Test]
        public void NextPageButtonDisabled_WhenFirstPageNotFull()
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
                SimplePageSize = 2
            };

            var pathService = new PathService();
            var itemSpawnerController = new ItemSpawnerController(itemData, pathService);

            var newState = itemSpawnerController.PageSelected(state, PageMode.Firearms);

            newState.SimpleNextPageEnabled.Should().BeFalse();
        }

        [Test]
        public void NextPageButtonDisabled_WhenFirstPageExactlyFull()
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
                SimplePageSize = 1
            };

            var pathService = new PathService();
            var itemSpawnerController = new ItemSpawnerController(itemData, pathService);

            var newState = itemSpawnerController.PageSelected(state, PageMode.Firearms);

            newState.SimpleNextPageEnabled.Should().BeFalse();
        }

        [Test]
        public void NextPageButtonEnabled_WhenNotEnoughRoomOnFirstPage()
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
                SimplePageSize = 1
            };

            var pathService = new PathService();
            var itemSpawnerController = new ItemSpawnerController(itemData, pathService);

            var newState = itemSpawnerController.PageSelected(state, PageMode.Firearms);

            newState.SimpleNextPageEnabled.Should().BeTrue();
        }
    }
}
