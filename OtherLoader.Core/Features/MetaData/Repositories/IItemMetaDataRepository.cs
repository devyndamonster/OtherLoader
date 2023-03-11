using OtherLoader.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Core.Features.MetaData
{
    public interface IItemMetaDataRepository
    {
        public void AddItemSpawnerEntry(SpawnerEntryData spawnerEntryData);
    }
}
