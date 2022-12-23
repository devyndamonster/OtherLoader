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

        public ItemSpawnerController(ItemDataContainer dataContainer, IPathService pathService) 
        {
            _dataContainer = dataContainer;
            _pathService = pathService;
        }
        
        public ItemSpawnerState GetInitialState()
        {
            return new ItemSpawnerState
            {
                CurrentPath = PageMode.MainMenu.ToString()
            };
        }

        public ItemSpawnerState PageSelected(ItemSpawnerState state, PageMode page)
        {
            var newState = state.Clone();

            newState.CurrentPath = page.ToString();
            newState.SimpleTileStates = GetSimpleTileStatesForPath(newState.CurrentPath);

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
        
        private IEnumerable<ItemSpawnerTileState> GetSimpleTileStatesForPath(string path)
        {
            return _dataContainer.ItemEntries
                .Where(entry => _pathService.IsImmediateParentOf(path, entry.Path))
                .Select(entry => new ItemSpawnerTileState
                {
                    DisplayText = entry.DisplayText,
                    Path = entry.Path
                });
        }
    }
}
