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
using BepInEx;
using UnityEngine;
using Deli.VFS.Disk;
using Valve.VR.InteractionSystem;


namespace OtherLoader
{
    public class ItemLoader
    {

        public IEnumerator StartAssetLoadUnordered(RuntimeStage stage, Mod mod, IHandle handle)
        {
            StartAssetLoad(stage, mod, handle, LoadOrderType.LoadUnordered);
            yield return null;
        }

        public IEnumerator StartAssetLoadFirst(RuntimeStage stage, Mod mod, IHandle handle)
        {
            StartAssetLoad(stage, mod, handle, LoadOrderType.LoadFirst);
            yield return null;
        }

        public IEnumerator StartAssetLoadLast(RuntimeStage stage, Mod mod, IHandle handle)
        {
            StartAssetLoad(stage, mod, handle, LoadOrderType.LoadLast);
            yield return null;
        }


        public void StartAssetLoad(RuntimeStage stage, Mod mod, IHandle handle, LoadOrderType orderType)
        {
            if (handle is not IFileHandle file)
            {
                throw new ArgumentException("Could not load item, make sure you are pointing to the asset bundle correctly");
            }

            string uniqueAssetID = mod.Info.Guid + " : " + file.Name;

            if (file is not IDiskHandle fileDisk)
            {
                IEnumerator routine = LoadAssetsFromFileAsync(stage, file, uniqueAssetID, orderType).TryCatch<Exception>(e =>
                {
                    OtherLogger.LogError("Failed to load mod (" + uniqueAssetID + ")");
                    OtherLogger.LogError(e.ToString());
                    LoaderStatus.UpdateProgress(uniqueAssetID, 1);
                    LoaderStatus.RemoveActiveLoader(uniqueAssetID);
                });

                AnvilManager.Instance.StartCoroutine(routine);
            }

            else
            {
                IEnumerator routine = LoadAssetsFromPathAsync(fileDisk.PathOnDisk, uniqueAssetID, orderType).TryCatch<Exception>(e =>
                {
                    OtherLogger.LogError("Failed to load mod (" + uniqueAssetID + ")");
                    OtherLogger.LogError(e.ToString());
                    LoaderStatus.UpdateProgress(uniqueAssetID, 1);
                    LoaderStatus.RemoveActiveLoader(uniqueAssetID);
                });

                AnvilManager.Instance.StartCoroutine(routine);
            }
        }

        public void LoadLegacyAssets()
        {

            if (!Directory.Exists(OtherLoader.MainLegacyDirectory)) Directory.CreateDirectory(OtherLoader.MainLegacyDirectory);

            OtherLogger.Log("Plugins folder found (" + Paths.PluginPath + ")", OtherLogger.LogType.General);

            List<string> legacyPaths = Directory.GetDirectories(Paths.PluginPath, "LegacyVirtualObjects", SearchOption.AllDirectories).ToList();
            legacyPaths.Add(OtherLoader.MainLegacyDirectory);

            foreach(string legacyPath in legacyPaths)
            {
                OtherLogger.Log("Legacy folder found (" + legacyPath + ")", OtherLogger.LogType.General);

                foreach (string bundlePath in Directory.GetFiles(legacyPath, "*", SearchOption.AllDirectories))
                {
                    //Only allow files without file extensions to be loaded (assumed to be an asset bundle)
                    if (Path.GetFileName(bundlePath) != Path.GetFileNameWithoutExtension(bundlePath))
                    {
                        continue;
                    }

                    string uniqueAssetID = "Legacy : " + Path.GetFileName(bundlePath);

                    IEnumerator routine = LoadAssetsFromPathAsync(bundlePath, uniqueAssetID, LoadOrderType.LoadUnordered).TryCatch<Exception>(e =>
                    {
                        OtherLogger.LogError("Failed to load mod (" + uniqueAssetID + ")");
                        OtherLogger.LogError(e.ToString());
                        LoaderStatus.UpdateProgress(uniqueAssetID, 1);
                        LoaderStatus.RemoveActiveLoader(uniqueAssetID);
                    });

                    AnvilManager.Instance.StartCoroutine(routine);
                }
            }
        }


        private IEnumerator LoadAssetsFromFileAsync(RuntimeStage stage, IFileHandle file, string uniqueAssetID, LoadOrderType orderType)
        {
            LoaderStatus.TrackLoader(uniqueAssetID, orderType);

            //If there are many active loaders at once, we should wait our turn
            while ((OtherLoader.MaxActiveLoaders > 0 && LoaderStatus.NumActiveLoaders >= OtherLoader.MaxActiveLoaders) || !LoaderStatus.CanOrderedModLoad(uniqueAssetID))
            {
                yield return null;
            }

            LoaderStatus.AddActiveLoader(uniqueAssetID);

            //First, we want to load the asset bundle itself
            OtherLogger.Log("Beginning async loading of asset bundle (" + uniqueAssetID + ") with load order type (" + orderType + ")", OtherLogger.LogType.General);
            LoaderStatus.UpdateProgress(uniqueAssetID, UnityEngine.Random.Range(.1f, .3f));

            //Load the bytes of the bundle into memory
            ResultYieldInstruction<byte[]> bundleYieldable = stage.DelayedReaders.Get<byte[]>()(file);
            yield return bundleYieldable;
            byte[] bundleBytes = bundleYieldable.Result;

            LoaderStatus.UpdateProgress(uniqueAssetID, UnityEngine.Random.Range(.4f, .7f));

            //Now get the asset bundle from those bytes
            AnvilCallback<AssetBundle> bundle = LoaderUtils.LoadAssetBundleFromBytes(bundleBytes);
            yield return bundle;

            LoaderStatus.UpdateProgress(uniqueAssetID, 0.9f);

            yield return ApplyLoadedAssetBundle(bundle, uniqueAssetID).TryCatch<Exception>(e =>
            {
                OtherLogger.LogError("Failed to load mod (" + uniqueAssetID + ")");
                OtherLogger.LogError(e.ToString());
                LoaderStatus.UpdateProgress(uniqueAssetID, 1);
                LoaderStatus.RemoveActiveLoader(uniqueAssetID);
            });

            LoaderStatus.UpdateProgress(uniqueAssetID, 1);
            LoaderStatus.RemoveActiveLoader(uniqueAssetID);
        }



        private IEnumerator LoadAssetsFromPathAsync(string path, string uniqueAssetID, LoadOrderType orderType)
        {
            LoaderStatus.TrackLoader(uniqueAssetID, orderType);

            //If there are many active loaders at once, we should wait our turn
            while (OtherLoader.MaxActiveLoaders > 0 && LoaderStatus.NumActiveLoaders >= OtherLoader.MaxActiveLoaders || !LoaderStatus.CanOrderedModLoad(uniqueAssetID))
            {
                yield return null;
            }

            LoaderStatus.AddActiveLoader(uniqueAssetID);

            //First, we want to load the asset bundle itself
            OtherLogger.Log("Beginning async loading of legacy asset bundle (" + uniqueAssetID + ") with load order type (" + orderType + ")", OtherLogger.LogType.General);
            LoaderStatus.UpdateProgress(uniqueAssetID, UnityEngine.Random.Range(.1f, .3f));

            AnvilCallback<AssetBundle> bundle = LoaderUtils.LoadAssetBundleFromPath(path);

            yield return bundle;

            LoaderStatus.UpdateProgress(uniqueAssetID, 0.9f);

            yield return ApplyLoadedAssetBundle(bundle, uniqueAssetID).TryCatch<Exception>(e =>
            {
                OtherLogger.LogError("Failed to load mod (" + uniqueAssetID + ")");
                OtherLogger.LogError(e.ToString());
                LoaderStatus.UpdateProgress(uniqueAssetID, 1);
                LoaderStatus.RemoveActiveLoader(uniqueAssetID);
            });

            OtherLoader.LegacyBundles.Add(uniqueAssetID, path);
            LoaderStatus.UpdateProgress(uniqueAssetID, 1);
            LoaderStatus.RemoveActiveLoader(uniqueAssetID);
        }



        private IEnumerator ApplyLoadedAssetBundle(AnvilCallback<AssetBundle> bundle, string uniqueAssetID)
        {
            //Load the mechanical accuracy entries
            AssetBundleRequest accuracyCharts = bundle.Result.LoadAllAssetsAsync<FVRFireArmMechanicalAccuracyChart>();
            yield return accuracyCharts;
            LoadMechanicalAccuracyEntries(accuracyCharts.allAssets);

            //Load all the FVRObjects
            AssetBundleRequest fvrObjects = bundle.Result.LoadAllAssetsAsync<FVRObject>();
            yield return fvrObjects;
            LoadFVRObjects(uniqueAssetID, fvrObjects.allAssets);

            //Now all the FVRObjects are loaded, we can load the bullet data
            AssetBundleRequest bulletData = bundle.Result.LoadAllAssetsAsync<FVRFireArmRoundDisplayData>();
            yield return bulletData;
            LoadBulletData(bulletData.allAssets);

            //Before we load the spawnerIDs, we must add any new spawner category definitions
            AssetBundleRequest spawnerCats = bundle.Result.LoadAllAssetsAsync<ItemSpawnerCategoryDefinitions>();
            yield return spawnerCats;
            LoadSpawnerCategories(spawnerCats.allAssets);

            //Finally, add all the items to the spawner
            AssetBundleRequest spawnerIDs = bundle.Result.LoadAllAssetsAsync<ItemSpawnerID>();
            yield return spawnerIDs;
            LoadSpawnerIDs(spawnerIDs.allAssets);
            
            //handle handling grab/release/slot sets
            AssetBundleRequest HandlingGrabSet = bundle.Result.LoadAllAssetsAsync<HandlingGrabSet>();
            yield return HandlingGrabSet;
            LoadHandlingGrabSetEntries(HandlingGrabSet.allAssets);
            AssetBundleRequest HandlingReleaseSet = bundle.Result.LoadAllAssetsAsync<HandlingReleaseSet>();
            yield return HandlingReleaseSet;
            LoadHandlingReleaseSetEntries(HandlingReleaseSet.allAssets);
            AssetBundleRequest HandlingSlotSet = bundle.Result.LoadAllAssetsAsync<HandlingReleaseIntoSlotSet>();
            yield return HandlingSlotSet;
            LoadHandlingSlotSetEntries(HandlingSlotSet.allAssets);
            //audio bullet impact sets; handled similarly to the ones above
            AssetBundleRequest BulletImpactSet = bundle.Result.LoadAllAssetsAsync<AudioBulletImpactSet>();
            yield return BulletImpactSet;
            LoadImpactSetEntries(BulletImpactSet.allAssets);
            AssetBundleRequest Quickbelts = bundle.Result.LoadAllAssetsAsync<GameObject>();
            yield return Quickbelts;
            LoadQuickbeltEntries(Quickbelts.allAssets);
            
            AnvilManager.m_bundles.Add(uniqueAssetID, bundle);
            OtherLogger.Log("Completed loading of asset bundle (" + uniqueAssetID + ")", OtherLogger.LogType.General);
        }
        
        private void LoadHandlingGrabSetEntries(UnityEngine.Object[] allAssets)
        { //nothing fancy; just dumps them into the lists above and logs it
            foreach (HandlingGrabSet grabSet in allAssets)
            {
                OtherLogger.Log("Loading new handling grab set entry: " + grabSet.name, OtherLogger.LogType.Loading);
                ManagerSingleton<SM>.Instance.m_handlingGrabDic.Add(grabSet.Type, grabSet);
            }
        }

        private void LoadHandlingReleaseSetEntries(UnityEngine.Object[] allAssets)
        {
            foreach (HandlingReleaseSet releaseSet in allAssets)
            {
                OtherLogger.Log("Loading new handling release set entry: " + releaseSet.name, OtherLogger.LogType.Loading);
                ManagerSingleton<SM>.Instance.m_handlingReleaseDic.Add(releaseSet.Type, releaseSet);
            }
        }

        private void LoadHandlingSlotSetEntries(UnityEngine.Object[] allAssets)
        {
            foreach (HandlingReleaseIntoSlotSet slotSet in allAssets)
            {
                OtherLogger.Log("Loading new handling QB slot set entry: " + slotSet.name, OtherLogger.LogType.Loading);
                ManagerSingleton<SM>.Instance.m_handlingReleaseIntoSlotDic.Add(slotSet.Type, slotSet);
            }
        }

        private void LoadImpactSetEntries(UnityEngine.Object[] allAssets)
        {
            foreach (AudioBulletImpactSet impactSet in allAssets)
            {
                OtherLogger.Log("Loading new bullet impact set entry: " + impactSet.name, OtherLogger.LogType.Loading);
                //this is probably the stupidest workaround, but it works and it's short. it just adds impactset to the impact sets
                ManagerSingleton<SM>.Instance.AudioBulletImpactSets.Concat(new AudioBulletImpactSet[] {impactSet});
                ManagerSingleton<SM>.Instance.m_bulletHitDic.Add(impactSet.Type, impactSet);
            }
        }

        private void LoadQuickbeltEntries(UnityEngine.Object[] allAssets)
        {
            foreach (GameObject quickbelt in allAssets)
            {
                string[] QBnameSplit = quickbelt.name.Split('_');
                if (QBnameSplit.Length > 1)
                {
                    if (QBnameSplit[QBnameSplit.Length - 2] == "QuickBelt")
                    {
                        OtherLogger.Log("Adding QuickBelt " + quickbelt.name, OtherLogger.LogType.Loading);
                        Array.Resize(ref GM.Instance.QuickbeltConfigurations,
                            GM.Instance.QuickbeltConfigurations.Length + 1);
                        GM.Instance.QuickbeltConfigurations[GM.Instance.QuickbeltConfigurations.Length - 1] = quickbelt;
                    }
                }
            }
        }


        private void LoadMechanicalAccuracyEntries(UnityEngine.Object[] allAssets)
        {
            foreach(FVRFireArmMechanicalAccuracyChart chart in allAssets)
            {
                foreach(FVRFireArmMechanicalAccuracyChart.MechanicalAccuracyEntry entry in chart.Entries)
                {
                    OtherLogger.Log("Loading new mechanical accuracy entry: " + entry.Class, OtherLogger.LogType.Loading);

                    if (!AM.SMechanicalAccuracyDic.ContainsKey(entry.Class)){
                        AM.SMechanicalAccuracyDic.Add(entry.Class, entry);
                    }
                    else
                    {
                        OtherLogger.LogError("Duplicate mechanical accuracy class found, will not use one of them! Make sure you're using unique mechanical accuracy classes!");
                    }
                }
            }
        }


        private void LoadSpawnerCategories(UnityEngine.Object[] allAssets)
        {
            foreach (ItemSpawnerCategoryDefinitions newLoadedCats in allAssets)
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

                        OtherLogger.Log("Num CDefs: " + IM.CDefs.Categories.Length, OtherLogger.LogType.Loading);

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


        private void LoadSpawnerIDs(UnityEngine.Object[] allAssets)
        {
            foreach (ItemSpawnerID id in allAssets)
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


        private void LoadFVRObjects(string bundleID, UnityEngine.Object[] allAssets)
        {
            foreach (FVRObject item in allAssets)
            {
                if (item == null) continue;

                OtherLogger.Log("Loading FVRObject: " + item.ItemID, OtherLogger.LogType.Loading);

                if (IM.OD.ContainsKey(item.ItemID))
                {
                    OtherLogger.LogError("The ItemID of FVRObject is already used! Item will not be loaded! ItemID: " + item.ItemID);
                    continue;
                }

                item.m_anvilPrefab.Bundle = bundleID;

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


        private void LoadBulletData(UnityEngine.Object[] allAssets)
        {
            foreach (FVRFireArmRoundDisplayData data in allAssets)
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
