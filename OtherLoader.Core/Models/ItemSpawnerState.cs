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
        public string CurrentPath { get; set; }
        
        public IEnumerable<PageMode> VisiblePages { get; set; }

        public SearchMode SearchMode { get; set; }

        public IEnumerable<ItemSpawnerTileState> SimpleTileStates { get; set; }

        public int SimplePageSize { get; set; }

        public IDictionary<string, int> SavedPathsToPages { get; set; } = new Dictionary<string, int>();

        public int SimpleCurrentPage => SavedPathsToPages.ContainsKey(CurrentPath) ? SavedPathsToPages[CurrentPath] : 0;

        public bool SimpleNextPageEnabled { get; set; }

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
    }
}
