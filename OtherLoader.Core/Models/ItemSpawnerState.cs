using OtherLoader.Core.SharedInterfaces;
using System;
using System.Collections.Generic;

#if DEBUG
using Newtonsoft.Json;
#else
using Valve.Newtonsoft.Json;
#endif

namespace OtherLoader.Core.Models
{
    public class ItemSpawnerState : IPrototype<ItemSpawnerState>
    {
        public IEnumerable<PageMode> VisiblePages { get; set; }

        public SearchMode SearchMode { get; set; }
        
        public SimpleModeState SimpleState { get; set; }
        
        public ItemSpawnerState Clone()
        {
            var json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<ItemSpawnerState>(json);
        }
        
        public class SimpleModeState
        {
            public string CurrentPath { get; set; }

            public IEnumerable<ItemSpawnerTileState> TileStates { get; set; }

            public int PageSize { get; set; }

            public IDictionary<string, int> SavedPathsToPages { get; set; } = new Dictionary<string, int>();

            [JsonIgnore]
            public int CurrentPage => SavedPathsToPages.ContainsKey(CurrentPath) ? SavedPathsToPages[CurrentPath] : 0;

            public bool NextPageEnabled { get; set; }

            public bool PrevPageEnabled { get; set; }

            public string PageCountText { get; set; }
        }
    }
}
