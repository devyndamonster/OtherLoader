using Anvil;
using Deli;
using Deli.Runtime;
using Deli.Setup;
using Deli.VFS;
using FistVR;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OtherLoader
{
    public class ItemLoader
    {

        public IEnumerator LoadAsset(RuntimeStage stage, Mod mod, IHandle handle)
        {
            if(handle is not IFileHandle file)
            {
                throw new ArgumentException("Could not load item, make sure you are pointing to the asset bundle correctly");
            }

            //First, we want to load the asset bundle itself
            OtherLoader.OtherLogger.LogInfo("Beginning async loading of asset bundle");
            byte[] bundleBytes = stage.ImmediateReaders.Get<byte[]>()(file);
            AnvilCallback<AssetBundle> bundle = LoadAssetBundleAsync(bundleBytes, file.Path);

            yield return bundle;

            if(bundle.Result == null)
            {
                OtherLoader.OtherLogger.LogError("Asset Bundle was null!");
                yield break;
            }


            //Now that the asset bundle is loaded, we need to load all the FVRObjects
            AssetBundleRequest fvrObjects = bundle.Result.LoadAllAssetsAsync<FVRObject>();
            yield return fvrObjects;

            foreach (FVRObject item in fvrObjects.allAssets)
            {
                if (item == null) continue;
                OtherLoader.OtherLogger.LogInfo("Loading Item: " + item.ItemID);

                item.m_anvilPrefab.Bundle = file.Path;

                IM.OD.Add(item.ItemID, item);
                ManagerSingleton<IM>.Instance.odicTagCategory.AddOrCreate(item.Category).Add(item);
                ManagerSingleton<IM>.Instance.odicTagFirearmEra.AddOrCreate(item.TagEra).Add(item);
                ManagerSingleton<IM>.Instance.odicTagFirearmSize.AddOrCreate(item.TagFirearmSize).Add(item);
                ManagerSingleton<IM>.Instance.odicTagFirearmAction.AddOrCreate(item.TagFirearmAction).Add(item);
                ManagerSingleton<IM>.Instance.odicTagAttachmentMount.AddOrCreate(item.TagAttachmentMount).Add(item);
                ManagerSingleton<IM>.Instance.odicTagAttachmentFeature.AddOrCreate(item.TagAttachmentFeature).Add(item);

                foreach (FVRObject.OTagFirearmFiringMode mode in item.TagFirearmFiringModes)
                {
                    ManagerSingleton<IM>.Instance.odicTagFirearmFiringMode.AddOrCreate(mode).Add(item);
                }
                foreach(FVRObject.OTagFirearmFeedOption feed in item.TagFirearmFeedOption)
                {
                    ManagerSingleton<IM>.Instance.odicTagFirearmFeedOption.AddOrCreate(feed).Add(item);
                }
                foreach (FVRObject.OTagFirearmMount mount in item.TagFirearmMounts)
                {
                    ManagerSingleton<IM>.Instance.odicTagFirearmMount.AddOrCreate(mount).Add(item);
                }

                //if(item.Category == FVRObject.ObjectCategory.Cartridge)
                //{
                //    FVRFireArmRound round = item.GetGameObject().GetComponent<FVRFireArmRound>();
                //}

            }

            //Now all the FVRObjects are loaded, we can load the bullet data
            AssetBundleRequest roundData = bundle.Result.LoadAllAssetsAsync<FVRFireArmRoundDisplayData>();
            yield return roundData;

            foreach (FVRFireArmRoundDisplayData data in roundData.allAssets)
            {
                if (data == null) continue;

                OtherLoader.OtherLogger.LogInfo("Loading ammo display data!");

                OtherLoader.OtherLogger.LogInfo("Type: " + data.Type);
                if (!ManagerSingleton<AM>.Instance.TypeDic.ContainsKey(data.Type))
                {
                    OtherLoader.OtherLogger.LogInfo("This is a new ammo type! Adding it to dictionary");
                    ManagerSingleton<AM>.Instance.TypeDic.Add(data.Type, new Dictionary<FireArmRoundClass, FVRFireArmRoundDisplayData.DisplayDataClass>());
                }

                if (!AM.STypeClassLists.ContainsKey(data.Type))
                {
                    AM.STypeClassLists.Add(data.Type, new List<FireArmRoundClass>());
                }

                if (!AM.SRoundDisplayDataDic.ContainsKey(data.Type))
                {
                    AM.SRoundDisplayDataDic.Add(data.Type, data);
                }

                //If this Display Data already exists, then we should add our classes to the existing display data class list
                else
                {
                    List<FVRFireArmRoundDisplayData.DisplayDataClass> classes = new List<FVRFireArmRoundDisplayData.DisplayDataClass>(AM.SRoundDisplayDataDic[data.Type].Classes);
                    classes.AddRange(data.Classes);
                    AM.SRoundDisplayDataDic[data.Type].Classes = classes.ToArray();
                }

                foreach (FVRFireArmRoundDisplayData.DisplayDataClass roundClass in data.Classes)
                {
                    OtherLoader.OtherLogger.LogInfo("Class: " + roundClass.Class);
                    if (!ManagerSingleton<AM>.Instance.TypeDic[data.Type].ContainsKey(roundClass.Class))
                    {
                        OtherLoader.OtherLogger.LogInfo("This is a new ammo class! Adding it to dictionary");
                        ManagerSingleton<AM>.Instance.TypeDic[data.Type].Add(roundClass.Class, roundClass);
                    }

                    if (!AM.STypeClassLists[data.Type].Contains(roundClass.Class))
                    {
                        AM.STypeClassLists[data.Type].Add(roundClass.Class);
                    }

                }
            }


            //Finally, add all the items to the spawner
            AssetBundleRequest spawnerIDs = bundle.Result.LoadAllAssetsAsync<ItemSpawnerID>();
            yield return spawnerIDs;

            foreach (ItemSpawnerID id in spawnerIDs.allAssets)
            {
                IM.CD[id.Category].Add(id);
                IM.SCD[id.SubCategory].Add(id);

                if (!ManagerSingleton<IM>.Instance.SpawnerIDDic.ContainsKey(id.ItemID))
                {
                    ManagerSingleton<IM>.Instance.SpawnerIDDic[id.ItemID] = id;
                }
            }
        }

        private AnvilCallback<AssetBundle> LoadAssetBundleAsync(byte[] bundleBytes, string ID)
        {
            AsyncOperation request = AssetBundle.LoadFromMemoryAsync(bundleBytes);
            AnvilCallbackBase anvilCallbackBase = new AnvilCallback<AssetBundle>(request, null);
            AnvilManager.m_bundles.Add(ID, anvilCallbackBase);
            return (AnvilCallback<AssetBundle>)anvilCallbackBase;
        }

    }

    /// <summary>
    /// Credit to BlockBuilder57 for this incredibly useful extension
    /// https://github.com/BlockBuilder57/LSIIC/blob/527927cb921c360d9c158008e24bdeaf2059440e/LSIIC/LSIIC.VirtualObjectsInjector/VirtualObjectsInjectorPlugin.cs#L146
    /// </summary>
    public static class DictionaryExtension
    {
        public static TValue AddOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
        {
            if (!dictionary.ContainsKey(key))
                dictionary.Add(key, new TValue());
            return dictionary[key];
        }
    }

}
