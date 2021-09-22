using FistVR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OtherLoader
{
    public static class CacheManager
    {
        public static string CachePath;

        public const string SPAWNER_ID_PREFIX = "SpawnerID_";
        public const string FVROBJECT_PREFIX = "FVRObject_";
        public const string AMMO_DATA_PREFIX = "AmmoData_";
        public const string SPAWNER_CAT_PREFIX = "SpawnerCat_";

        public static void Init()
        {
            CachePath = Application.dataPath.Replace("/h3vr_Data", "/OtherLoaderCache");

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

            foreach (ItemSpawnerID spawnerID in itemSpawnerIDs)
            {
                yield return AnvilManager.Instance.StartCoroutine(CacheItemSpawnerID(folderPath, spawnerID));
            }

            foreach (FVRObject fvrObject in FVRObjects)
            {
                yield return AnvilManager.Instance.StartCoroutine(CacheFVRObject(folderPath, fvrObject));
            }

            foreach (ItemSpawnerCategoryDefinitions cat in itemSpawnerCats)
            {
                yield return AnvilManager.Instance.StartCoroutine(CacheSpawnerCats(folderPath, cat));
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
                    OtherLogger.Log("Caching SpawnerID (" + SPAWNER_ID_PREFIX + ID.ItemID + ")", OtherLogger.LogType.Loading);
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

        }

        public static IEnumerator CacheFVRObject(string folderPath, FVRObject fvr)
        {
            FVRObjectSerializable serfvr = new FVRObjectSerializable(fvr);

            //Save the ItemSpawnerID to the cache
            string filePath = Path.Combine(folderPath, FVROBJECT_PREFIX + fvr.ItemID + ".json");
            if (!File.Exists(filePath))
            {
                using (StreamWriter sw = File.CreateText(filePath))
                {
                    OtherLogger.Log("Caching FVRObject (" + FVROBJECT_PREFIX + fvr.ItemID + ")", OtherLogger.LogType.Loading);
                    string serData = JsonConvert.SerializeObject(serfvr, Formatting.Indented, new StringEnumConverter());
                    sw.WriteLine(serData);
                    sw.Close();
                }
            }
            else
            {
                OtherLogger.LogError("A cached FVRObject is being overwritten (" + fvr.ItemID + ")");
            }

            yield return null;
        }

        public static IEnumerator CacheSpawnerCats(string folderPath, ItemSpawnerCategoryDefinitions cat)
        {
            ItemSpawnerCategorySerializable sercat = new ItemSpawnerCategorySerializable(cat);

            //Save the ItemSpawnerID to the cache
            string filePath = Path.Combine(folderPath, SPAWNER_CAT_PREFIX + cat.Categories[0].DisplayName + ".json");
            if (!File.Exists(filePath))
            {
                using (StreamWriter sw = File.CreateText(filePath))
                {
                    OtherLogger.Log("Caching Categories (" + SPAWNER_CAT_PREFIX + cat.Categories[0].DisplayName + ")", OtherLogger.LogType.Loading);
                    string serData = JsonConvert.SerializeObject(sercat, Formatting.Indented, new StringEnumConverter());
                    sw.WriteLine(serData);
                    sw.Close();
                }
            }
            else
            {
                OtherLogger.LogError("A cached Categories is being overwritten (" + cat.Categories[0].DisplayName + ")");
            }

            yield return null;
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
