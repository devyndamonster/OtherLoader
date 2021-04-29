using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OtherLoader
{
    public static class CacheManager
    {
        public static string CachePath;

        public static void Init()
        {
            CachePath = Application.dataPath.Replace("/h3vr_Data", "/OtherLoaderCache/");
        }

        public static void CacheItemSpawnerID(string assetBundleID, ItemSpawnerID ID)
        {

        }


    }
}
