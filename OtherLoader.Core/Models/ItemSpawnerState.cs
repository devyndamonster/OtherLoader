using OtherLoader.Core.SharedInterfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace OtherLoader.Core.Models
{
    [Serializable]
    public class ItemSpawnerState : IPrototype<ItemSpawnerState>
    {
        public IEnumerable<PageMode> VisiblePages { get; set; }

        public SearchMode SearchMode { get; set; }
        
        public SimpleModeState SimpleState { get; set; }


        public ItemSpawnerState Clone()
        {
            using (var ms = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, this);
                ms.Seek(0, SeekOrigin.Begin);
                return (ItemSpawnerState)formatter.Deserialize(ms);
            }
        }

        [Serializable]
        public class SimpleModeState
        {
            public string CurrentPath { get; set; }

            public IEnumerable<ItemSpawnerTileState> TileStates { get; set; }

            public int PageSize { get; set; }

            public IDictionary<string, int> SavedPathsToPages { get; set; } = new Dictionary<string, int>();

            public int CurrentPage => SavedPathsToPages.ContainsKey(CurrentPath) ? SavedPathsToPages[CurrentPath] : 0;

            public bool NextPageEnabled { get; set; }

            public bool PrevPageEnabled { get; set; }
        }
    }
}
