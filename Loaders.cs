using Anvil;
using Deli;
using Deli.Runtime;
using Deli.Runtime.Yielding;
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
            OtherLogger.Log("Beginning async loading of mod: " + mod.Info.Name, OtherLogger.LogType.Loading);
            LoaderStatus.AddLoader(mod.Info.Guid);

            //Load the bytes of the bundle into memory
            ResultYieldInstruction<byte[]> bundleYieldable = stage.DelayedReaders.Get<byte[]>()(file);
            yield return bundleYieldable;
            byte[] bundleBytes = bundleYieldable.Result;

            LoaderStatus.UpdateProgress(mod.Info.Guid, 0.25f);

            //Now get the asset bundle from those bytes
            AnvilCallback<AssetBundle> bundle = LoaderUtils.LoadAssetBundleFromBytes(bundleBytes);
            yield return bundle;

            LoaderStatus.UpdateProgress(mod.Info.Guid, 0.5f);

            if (bundle.Result == null)
            {
                OtherLogger.LogError("Asset Bundle was null!");
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

            LoaderStatus.UpdateProgress(mod.Info.Guid, 0.75f);

            //Before we load the spawnerIDs, we must add any new spawner category definitions
            AssetBundleRequest spawnerCats = bundle.Result.LoadAllAssetsAsync<ItemSpawnerCategoryDefinitions>();
            yield return spawnerCats;
            LoadSpawnerCategories(spawnerCats);

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

            LoaderStatus.RemoveLoader(mod.Info.Guid);
        }


        private void LoadSpawnerCategories(AssetBundleRequest IDAssets)
        {
            foreach (ItemSpawnerCategoryDefinitions newLoadedCats in IDAssets.allAssets)
            {
                foreach (ItemSpawnerCategoryDefinitions.Category newCategory in newLoadedCats.Categories)
                {

                    OtherLogger.Log("Loading New ItemSpawner Category: " + newCategory.DisplayName, OtherLogger.LogType.Loading);

                    //If the loaded categories already contains this new category, we want to add subcategories
                    if (IM.CD.ContainsKey(newCategory.Cat))
                    {
                        OtherLogger.Log("Category already exists! Adding subcategories", OtherLogger.LogType.Loading);

                        foreach (ItemSpawnerCategoryDefinitions.Category currentCat in IM.CDefs.Categories)
                        {
                            if(currentCat.Cat == newCategory.Cat)
                            {
                                foreach(ItemSpawnerCategoryDefinitions.SubCategory newSubCat in newCategory.Subcats)
                                {
                                    //Only add this new subcategory if it is unique
                                    if(!IM.CDefSubInfo.ContainsKey(newSubCat.Subcat))
                                    {
                                        OtherLogger.Log("Adding subcategory: " + newSubCat.DisplayName, OtherLogger.LogType.Loading);

                                        List<ItemSpawnerCategoryDefinitions.SubCategory> currSubCatList = currentCat.Subcats.ToList();
                                        currSubCatList.Add(newSubCat);
                                        currentCat.Subcats = currSubCatList.ToArray();

                                        IM.CDefSubs[currentCat.Cat].Add(newSubCat);

                                        if (!IM.CDefSubInfo.ContainsKey(newSubCat.Subcat)) IM.CDefSubInfo.Add(newSubCat.Subcat, newSubCat);
                                        if (!IM.SCD.ContainsKey(newSubCat.Subcat)) IM.SCD.Add(newSubCat.Subcat, new List<ItemSpawnerID>());
                                    }

                                    else
                                    {
                                        OtherLogger.LogError("SubCategory type is already being used, and SubCategory will not be added! Make sure your subcategory is using a unique type! SubCategory Type: " + newSubCat.Subcat);
                                    }
                                }
                            }
                        }
                    }

                    //If new category, we can just add the whole thing
                    else
                    {
                        OtherLogger.Log("This is a new primary category", OtherLogger.LogType.Loading);

                        List<ItemSpawnerCategoryDefinitions.Category> currentCatsList = IM.CDefs.Categories.ToList();
                        currentCatsList.Add(newCategory);
                        IM.CDefs.Categories = currentCatsList.ToArray();

                        if (!IM.CDefSubs.ContainsKey(newCategory.Cat)) IM.CDefSubs.Add(newCategory.Cat, new List<ItemSpawnerCategoryDefinitions.SubCategory>());
                        if (!IM.CDefInfo.ContainsKey(newCategory.Cat)) IM.CDefInfo.Add(newCategory.Cat, newCategory);
                        if (!IM.CD.ContainsKey(newCategory.Cat)) IM.CD.Add(newCategory.Cat, new List<ItemSpawnerID>());

                        foreach(ItemSpawnerCategoryDefinitions.SubCategory newSubCat in newCategory.Subcats)
                        {
                            IM.CDefSubs[newCategory.Cat].Add(newSubCat);

                            if (!IM.CDefSubInfo.ContainsKey(newSubCat.Subcat)) IM.CDefSubInfo.Add(newSubCat.Subcat, newSubCat);
                            if (!IM.SCD.ContainsKey(newSubCat.Subcat)) IM.SCD.Add(newSubCat.Subcat, new List<ItemSpawnerID>());
                        }
                    }
                }

                

                
            }
        }


        private void LoadSpawnerIDs(AssetBundleRequest IDAssets)
        {
            foreach (ItemSpawnerID id in IDAssets.allAssets)
            {
                OtherLogger.Log("Adding Itemspawner ID! Category: " + id.Category + ", SubCategory: " + id.SubCategory, OtherLogger.LogType.Loading);

                if (IM.CD.ContainsKey(id.Category) && IM.SCD.ContainsKey(id.SubCategory)) {
                    IM.CD[id.Category].Add(id);
                    IM.SCD[id.SubCategory].Add(id);

                    if (!ManagerSingleton<IM>.Instance.SpawnerIDDic.ContainsKey(id.ItemID))
                    {
                        ManagerSingleton<IM>.Instance.SpawnerIDDic[id.ItemID] = id;
                    }
                }

                else
                {
                    OtherLogger.LogError("ItemSpawnerID could not be added, because either the main category or subcategory were not loaded! Item will not appear in the itemspawner! Item Display Name: " + id.DisplayName);
                }
            }
        }


        private void LoadFVRObjects(IFileHandle file, AssetBundleRequest fvrObjects)
        {
            foreach (FVRObject item in fvrObjects.allAssets)
            {
                if (item == null) continue;

                OtherLogger.Log("Loading FVRObject: " + item.ItemID, OtherLogger.LogType.Loading);

                if (IM.OD.ContainsKey(item.ItemID))
                {
                    OtherLogger.LogError("The ItemID of FVRObject is already used! Item will not be loaded! ItemID: " + item.ItemID);
                    continue;
                }

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

                OtherLogger.Log("Loading ammo type: " + data.Type, OtherLogger.LogType.Loading);

                if (!AM.STypeDic.ContainsKey(data.Type))
                {
                    OtherLogger.Log("This is a new ammo type! Adding it to dictionary", OtherLogger.LogType.Loading);
                    AM.STypeDic.Add(data.Type, new Dictionary<FireArmRoundClass, FVRFireArmRoundDisplayData.DisplayDataClass>());
                }
                else
                {
                    OtherLogger.Log("This is an existing ammo type, will add subclasses to this type", OtherLogger.LogType.Loading);
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
                    OtherLogger.Log("Loading ammo class: " + roundClass.Class, OtherLogger.LogType.Loading);
                    if (!AM.STypeDic[data.Type].ContainsKey(roundClass.Class))
                    {
                        OtherLogger.Log("This is a new ammo class! Adding it to dictionary", OtherLogger.LogType.Loading);
                        AM.STypeDic[data.Type].Add(roundClass.Class, roundClass);
                    }
                    else
                    {
                        OtherLogger.LogError("Ammo class already exists for bullet type! Bullet will not be loaded! Type: " + data.Type + ", Class: " + roundClass.Class);
                        return;
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
