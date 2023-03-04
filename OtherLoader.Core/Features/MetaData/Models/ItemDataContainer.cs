using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Core.Models
{
    public class ItemDataContainer
    {
        public IEnumerable<TagGroup> TagGroups;

        public IEnumerable<SpawnerEntryData> ItemEntries;
    }
}
