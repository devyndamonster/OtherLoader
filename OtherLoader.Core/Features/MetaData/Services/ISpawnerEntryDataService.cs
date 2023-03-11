using OtherLoader.Core.Features.MetaData.Models.Vanilla;
using OtherLoader.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Core.Features.MetaData.Services
{
    public interface ISpawnerEntryDataService
    {

        public SpawnerEntryData ConvertToSpawnerEntryData(ItemSpawnerId spawnerId);

    }
}
