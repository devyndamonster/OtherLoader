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
    }
}
