using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Services
{
    public interface IMetaDataService
    {
        public void RegisterSpawnerIDIntoTagSystem(ItemSpawnerID spawnerID);

        public void RegisterSpawnerEntryIntoTagSystem(ItemSpawnerEntry spawnerEntry);

        public ItemSpawnerV2.PageMode GetSpawnerPageForFVRObject(FVRObject fvrObject);

        public ItemSpawnerV2.PageMode GetSpawnerPageForSpawnerId(ItemSpawnerID spawnerId);

        public string GetTagFromCategory(ItemSpawnerID.EItemCategory category);

        public string GetTagFromSubcategory(ItemSpawnerID.ESubCategory subcategory);
    }
}
