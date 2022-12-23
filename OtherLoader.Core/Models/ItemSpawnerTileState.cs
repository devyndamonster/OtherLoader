using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Core.Models
{
    [Serializable]
    public class ItemSpawnerTileState
    {
        public bool IsVisible { get; set; }

        public string Path { get; set; }

        public string DisplayText { get; set; }
    }
}
