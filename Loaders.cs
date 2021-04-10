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
            if (handle is not IFileHandle file)
            {
                throw new ArgumentException("Could not load item, make sure you are pointing to the asset bundle correctly");
            }

            //First, we want to load the asset bundle itself
            OtherLoader.OtherLogger.LogInfo("Beginning async loading of asset bundle");
            AnvilCallback<AssetBundle> bundle = LoaderUtils.LoadAssetBundleFromFile(file);

            yield return bundle;

            if (bundle.Result == null)
            {
                OtherLoader.OtherLogger.LogError("Asset Bundle was null!");
                yield break;
            }

            //Now that the asset bundle is loaded, we need to load all the FVRObjects
            AssetBundleRequest fvrObjects = bundle.Result.LoadAllAssetsAsync<FVRObject>();
            yield return fvrObjects;
            LoadFVRObjects(file, fvrObjects);

            //Now all the FVRObjects are loaded, we can load the bullet data
            AssetBundleRequest bulletData = bundle.Result.LoadAllAssetsAsync<FVRFireArmRoundDisplayData>();
            yield return bulletData;
            LoadBulletData(bulletData);

            //Finally, add all the items to the spawner
            AssetBundleRequest spawnerIDs = bundle.Result.LoadAllAssetsAsync<ItemSpawnerID>();
            yield return spawnerIDs;
            LoadSpawnerIDs(spawnerIDs);
            
            //If OptimizeMemory is true, we unload the asset bundle. If it's not true, the asset bundle will remain loaded, and the reference to it will be kept in the AnvilManager
            OtherLoader.BundleFiles.Add(file.Path, file);
            if (OtherLoader.OptimizeMemory.Value)
            {
                bundle.Result.Unload(false);
            }
            else
            {
                AnvilManager.m_bundles.Add(file.Path, bundle);
            }
            
        }

        
        private void LoadSpawnerIDs(AssetBundleRequest IDAssets)
        {
            foreach (ItemSpawnerID id in IDAssets.allAssets)
            {
                IM.CD[id.Category].Add(id);
                IM.SCD[id.SubCategory].Add(id);

                if (!ManagerSingleton<IM>.Instance.SpawnerIDDic.ContainsKey(id.ItemID))
                {
                    ManagerSingleton<IM>.Instance.SpawnerIDDic[id.ItemID] = id;
                }
            }
        }


        private void LoadFVRObjects(IFileHandle file, AssetBundleRequest fvrObjects)
        {
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
                foreach (FVRObject.OTagFirearmFeedOption feed in item.TagFirearmFeedOption)
                {
                    ManagerSingleton<IM>.Instance.odicTagFirearmFeedOption.AddOrCreate(feed).Add(item);
                }
                foreach (FVRObject.OTagFirearmMount mount in item.TagFirearmMounts)
                {
                    ManagerSingleton<IM>.Instance.odicTagFirearmMount.AddOrCreate(mount).Add(item);
                }
            }
        }


        private void LoadBulletData(AssetBundleRequest bulletAssets)
        {
            foreach (FVRFireArmRoundDisplayData data in bulletAssets.allAssets)
            {
                if (data == null) continue;

                OtherLoader.OtherLogger.LogInfo("Loading ammo display data!");

                OtherLoader.OtherLogger.LogInfo("Type: " + data.Type);
                if (!AM.STypeDic.ContainsKey(data.Type))
                {
                    OtherLoader.OtherLogger.LogInfo("This is a new ammo type! Adding it to dictionary");
                    AM.STypeDic.Add(data.Type, new Dictionary<FireArmRoundClass, FVRFireArmRoundDisplayData.DisplayDataClass>());
                }

                if (!AM.STypeList.Contains(data.Type))
                {
                    AM.STypeList.Add(data.Type);
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

                if (!AM.STypeClassLists.ContainsKey(data.Type))
                {
                    AM.STypeClassLists.Add(data.Type, new List<FireArmRoundClass>());
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
        }

    }

}
