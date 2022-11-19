using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Scripts.Services
{
    public interface ISpawnerEntryLoadingService
    {
        public ItemSpawnerEntry GetSpawnerEntryFromSpawnerId(ItemSpawnerID spawnerId);
    }
}
