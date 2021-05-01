using FistVR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using Deli.Newtonsoft.Json;
using Deli.Newtonsoft.Json.Converters;

namespace OtherLoader
{
    public static class CacheManager
    {
        public static string CachePath;

        private const string SPAWNER_ID_PREFIX = "SpawnerID_";
        private const string FVROBJECT_PREFIX = "FVRObject_";
        private const string AMMO_DATA_PREFIX = "AmmoData_";
        private const string SPAWNER_CAT_PREFIX = "SpawnerCat_";

        public static void Init()
        {
            CachePath = Application.dataPath.Replace("/h3vr_Data", "/OtherLoaderCache");

            if (!Directory.Exists(CachePath))
            {
                Directory.CreateDirectory(CachePath);
            }
        }

        public static bool IsModCached(string assetBundleID, UnityEngine.Object[] itemSpawnerIDs, UnityEngine.Object[] FVRObjects, UnityEngine.Object[] ammoData, UnityEngine.Object[] itemSpawnerCats)
        {
            string folderPath = ConvertIDToPath(assetBundleID);

            if (!Directory.Exists(folderPath)){
                return false;
            }

            if (Directory.GetFiles(folderPath, SPAWNER_ID_PREFIX + "*").Length != itemSpawnerIDs.Length) return false;

            if (Directory.GetFiles(folderPath, FVROBJECT_PREFIX + "*").Length != FVRObjects.Length) return false;

            if (Directory.GetFiles(folderPath, AMMO_DATA_PREFIX + "*").Length != ammoData.Length) return false;

            if (Directory.GetFiles(folderPath, SPAWNER_CAT_PREFIX + "*").Length != itemSpawnerCats.Length) return false;

            return true;
        }

        public static void CacheMod(string assetBundleID, UnityEngine.Object[] itemSpawnerIDs, UnityEngine.Object[] FVRObjects, UnityEngine.Object[] ammoData, UnityEngine.Object[] itemSpawnerCats)
        {
            OtherLogger.Log("Caching Mod (" + assetBundleID + ")", OtherLogger.LogType.General);

            string folderPath = ConvertIDToPath(assetBundleID);
            Directory.CreateDirectory(folderPath);

            //Yeah, I'm incrementing a number using a foreach loop. Cry about it
            int prefixNum = 0;
            foreach(ItemSpawnerID spawnerID in itemSpawnerIDs)
            {
                CacheItemSpawnerID(folderPath, spawnerID, prefixNum);
                prefixNum += 1;
            }
        }

        public static void DeleteCachedMod(string assetBundleID)
        {
            if (Directory.Exists(ConvertIDToPath(assetBundleID)))
            {
                Directory.Delete(ConvertIDToPath(assetBundleID), true);
            }
        }

        public static void CacheItemSpawnerID(string folderPath, ItemSpawnerID ID, int prefixNum)
        {
            ItemSpawnerIDSerializable serID = new ItemSpawnerIDSerializable(ID);

            //Save the ItemSpawnerID to the cache
            string filePath = Path.Combine(folderPath, SPAWNER_ID_PREFIX + prefixNum + ".json");
            using (StreamWriter sw = File.CreateText(filePath))
            {
                OtherLogger.Log("Caching SpawnerID (" + SPAWNER_ID_PREFIX + prefixNum + ")", OtherLogger.LogType.General);
                string serData = JsonConvert.SerializeObject(serID, Formatting.Indented, new StringEnumConverter());
                sw.WriteLine(serData);
                sw.Close();
            }

            //Save the main icon to the cache
            string iconPath = Path.Combine(folderPath, "icon_" + prefixNum + ".png");
            if(ID.Sprite != null)
            {
                OtherLogger.Log("Caching Icon (icon_" + prefixNum + ")", OtherLogger.LogType.General);
                LoaderUtils.SaveSpriteToPNG(ID.Sprite, iconPath);
            }

            //Save secondary icons to the cache
            if(ID.Secondaries != null)
            {
                for(int i = 0; i < ID.Secondaries.Length; i++)
                {
                    if (ID.Secondaries[i].Sprite == null) continue;

                    OtherLogger.Log("Caching Secondary Icon (icon_" + prefixNum + "_" + i + ")", OtherLogger.LogType.General);
                    iconPath = Path.Combine(folderPath, "icon_" + prefixNum + "_" + i + ".png");
                    LoaderUtils.SaveSpriteToPNG(ID.Secondaries[i].Sprite, iconPath);
                }
            }

            //Save infographic icons to the cache
            if(ID.Infographic != null && ID.Infographic.Poster != null)
            {
                OtherLogger.Log("Caching Infographic (info_" + prefixNum + ")", OtherLogger.LogType.General);
                iconPath = Path.Combine(folderPath, "info_" + prefixNum + ".png");
                LoaderUtils.SaveTextureToPNG(ID.Infographic.Poster, iconPath);
            }
        }

        private static string ConvertIDToPath(string assetBundleID)
        {
            return Path.Combine(CachePath, assetBundleID.Replace(" ", "").Replace(":", "_"));
        }

    }
}
