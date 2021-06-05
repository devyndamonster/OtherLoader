using FistVR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using Deli.Newtonsoft.Json;
using Deli.Newtonsoft.Json.Converters;
using System.Collections;

namespace OtherLoader
{
    public static class CacheManager
    {
        public static string CachePath;

        public const string SPAWNER_ID_PREFIX = "SpawnerID_";
        public const string FVROBJECT_PREFIX = "FVRObject_";
        public const string AMMO_DATA_PREFIX = "AmmoData_";
        public const string SPAWNER_CAT_PREFIX = "SpawnerCat_";
        public const string ICON_PREFIX = "Icon_";
        public const string INFO_PREFIX = "Info_";

        public static void Init()
        {
            CachePath = Path.Combine(OtherLoader.Directory, "cache");

            if (!Directory.Exists(CachePath))
            {
                Directory.CreateDirectory(CachePath);
            }
        }

        public static bool IsModCached(string assetBundleID, int fileSize)
        {
            string folderPath = ConvertIDToPath(assetBundleID);
            if (!Directory.Exists(folderPath)){
                return false;
            }

            string dataPath = Path.Combine(folderPath, "CacheData.json");
            if (!File.Exists(dataPath))
            {
                return false;
            }

            string dataString = File.ReadAllText(dataPath);
            CacheData data = JsonConvert.DeserializeObject<CacheData>(dataString);

            if (data.FileSize != fileSize) return false;

            return true;
        }

        public static IEnumerator CacheMod(string assetBundleID, int fileSize, UnityEngine.Object[] itemSpawnerIDs, UnityEngine.Object[] FVRObjects, UnityEngine.Object[] ammoData, UnityEngine.Object[] itemSpawnerCats)
        {
            OtherLogger.Log("Caching Mod (" + assetBundleID + ")", OtherLogger.LogType.General);

            string folderPath = ConvertIDToPath(assetBundleID);
            Directory.CreateDirectory(folderPath);

            //Yeah, I'm incrementing a number using a foreach loop. Cry about it
            foreach (ItemSpawnerID spawnerID in itemSpawnerIDs)
            {
                yield return AnvilManager.Instance.StartCoroutine(CacheItemSpawnerID(folderPath, spawnerID));
            }

            //After everything is cached, we can save the cache data
            string filePath = Path.Combine(folderPath, "CacheData.json");
            using (StreamWriter sw = File.CreateText(filePath))
            {
                CacheData data = new CacheData();
                data.FileSize = fileSize;

                string serData = JsonConvert.SerializeObject(data, Formatting.Indented, new StringEnumConverter());
                sw.WriteLine(serData);
                sw.Close();
            }
        }

        public static void DeleteCachedMod(string assetBundleID)
        {
            if (Directory.Exists(ConvertIDToPath(assetBundleID)))
            {
                Directory.Delete(ConvertIDToPath(assetBundleID), true);
            }
        }

        public static IEnumerator CacheItemSpawnerID(string folderPath, ItemSpawnerID ID)
        {
            ItemSpawnerIDSerializable serID = new ItemSpawnerIDSerializable(ID);

            //Save the ItemSpawnerID to the cache
            string filePath = Path.Combine(folderPath, SPAWNER_ID_PREFIX + ID.ItemID + ".json");
            if (!File.Exists(filePath))
            {
                using (StreamWriter sw = File.CreateText(filePath))
                {
                    OtherLogger.Log("Caching SpawnerID (" + SPAWNER_ID_PREFIX + ID.ItemID + ")", OtherLogger.LogType.General);
                    string serData = JsonConvert.SerializeObject(serID, Formatting.Indented, new StringEnumConverter());
                    sw.WriteLine(serData);
                    sw.Close();
                }
            }
            else
            {
                OtherLogger.LogError("A cached ItemSpawnerID is being overwritten (" + ID.ItemID + ")");
            }

            yield return null;

            //Save the main icon to the cache
            string iconPath = Path.Combine(folderPath, ICON_PREFIX + ID.ItemID + ".png");
            if (ID.Sprite != null)
            {
                if (!File.Exists(iconPath))
                {
                    OtherLogger.Log("Caching Icon (" + ICON_PREFIX + ID.ItemID + ")", OtherLogger.LogType.General);
                    LoaderUtils.ForceSaveSpriteToPNG(ID.Sprite, iconPath);
                }
            }

            yield return null;

            //Save the main infographic to cache
            if (ID.Infographic != null && ID.Infographic.Poster != null)
            {
                iconPath = Path.Combine(folderPath, INFO_PREFIX + ID.Infographic.Poster.name + ".png");
                if (!File.Exists(iconPath))
                {
                    OtherLogger.Log("Caching Infographic (" + INFO_PREFIX + ID.Infographic.Poster.name + ")", OtherLogger.LogType.General);
                    LoaderUtils.ForceSaveTextureToPNG(ID.Infographic.Poster, iconPath);
                }
            }

            yield return null;

            //Save secondary icons and infographics to cache
            if (ID.Secondaries != null)
            {
                for(int i = 0; i < ID.Secondaries.Length; i++)
                {
                    if (ID.Secondaries[i].Sprite != null)
                    {
                        iconPath = Path.Combine(folderPath, ICON_PREFIX + ID.Secondaries[i].ItemID + ".png");
                        if (!File.Exists(iconPath))
                        {
                            OtherLogger.Log("Caching Secondary Icon (" + ICON_PREFIX + ID.Secondaries[i].ItemID + ")", OtherLogger.LogType.General);
                            LoaderUtils.ForceSaveSpriteToPNG(ID.Secondaries[i].Sprite, iconPath);
                        }
                    }

                    yield return null;

                    if (ID.Secondaries[i].Infographic != null && ID.Secondaries[i].Infographic.Poster != null)
                    {
                        iconPath = Path.Combine(folderPath, INFO_PREFIX + ID.Secondaries[i].Infographic.Poster.name + ".png");
                        if (!File.Exists(iconPath))
                        {
                            OtherLogger.Log("Caching Secondary Infographic (" + INFO_PREFIX + ID.Secondaries[i].Infographic.Poster.name + ")", OtherLogger.LogType.General);
                            LoaderUtils.ForceSaveTextureToPNG(ID.Secondaries[i].Infographic.Poster, iconPath);
                        }
                    }

                    yield return null;
                }
            }
        }

        private static string ConvertIDToPath(string assetBundleID)
        {
            return Path.Combine(CachePath, assetBundleID.Replace(" ", "").Replace(":", "_"));
        }

    }

    public class CacheData
    {
        public int FileSize;
    }

}
