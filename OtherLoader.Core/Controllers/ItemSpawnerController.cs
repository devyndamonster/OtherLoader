﻿using OtherLoader.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using OtherLoader.Core.Services;

namespace OtherLoader.Core.Controllers
{
    public class ItemSpawnerController : IItemSpawnerController
    {
        private readonly ItemDataContainer _dataContainer;
        private readonly IPathService _pathService;
        private readonly IPaginationService _pageService;

        public ItemSpawnerController(ItemDataContainer dataContainer, IPathService pathService, IPaginationService pageService) 
        {
            _dataContainer = dataContainer;
            _pathService = pathService;
            _pageService = pageService;
        }
        
        public ItemSpawnerState GetInitialState()
        {
            return new ItemSpawnerState
            {
                CurrentPath = PageMode.MainMenu.ToString(),
                SimplePageSize = 18
            };
        }

        public ItemSpawnerState PageSelected(ItemSpawnerState state, PageMode page)
        {
            var newState = state.Clone();
            
            newState.CurrentPath = page.ToString();
            newState.SavedPathsToPages[newState.CurrentPath] = 0;
            var tileStatesAtPath = GetAllSimpleTileStatesForPath(newState.CurrentPath);
            var totalTiles = newState.SimplePageSize * _pageService.GetNumberOfPages(newState.SimplePageSize, tileStatesAtPath.Count());
            var startingTile = newState.SimplePageSize * newState.SimpleCurrentPage;

            newState.SimpleTileStates = Enumerable.Range(0, totalTiles)
                .Skip(startingTile)
                .Take(newState.SimplePageSize)
                .Select(index => 
                    tileStatesAtPath.ElementAtOrDefault(index) ?? new() { Path = "" });

            newState.SimpleNextPageEnabled = tileStatesAtPath.Count() > newState.SimplePageSize;

            return newState;
        }

        public ItemSpawnerState NextPageClicked(ItemSpawnerState state)
        {
            throw new NotImplementedException();
        }
        
        public ItemSpawnerState PreviousPageClicked(ItemSpawnerState state)
        {
            throw new NotImplementedException();
        }
        
        private IEnumerable<ItemSpawnerTileState> GetAllSimpleTileStatesForPath(string path)
        {
            return _dataContainer.ItemEntries
                .Where(entry => 
                    _pathService.IsImmediateParentOf(path, entry.Path) &&
                    entry.IsDisplayedInMainEntry)
                .Select(entry => new ItemSpawnerTileState
                {
                    DisplayText = entry.DisplayText,
                    Path = entry.Path
                });
        }
    }
}
