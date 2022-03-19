using Anvil;
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
using Valve.VR.InteractionSystem;
using Stratum.Extensions;
using Stratum;
using System.Reflection;
using RenderHeads.Media.AVProVideo;
using OtherLoader.Loaders;

namespace OtherLoader
{
    public class ItemLoader
    {

        //Anatomy of a BundleID
        // [Mod Path] : [Bundle Name]
        // Combining these two gives you the path to the asset bundle


        public Empty LoadAssembly(FileSystemInfo handle)
        {
            OtherLogger.Log("Loading Assembly: " + handle.FullName, OtherLogger.LogType.Loading);
            Assembly.LoadFile(handle.FullName);
            return new Empty();
        }

        public Empty LoadLockIcon(FileSystemInfo handle)
        {
            OtherLogger.Log("Loading Lock Icon: " + handle.FullName, OtherLogger.LogType.Loading);

            OtherLoader.LockIcon = LoadSprite(handle);

            return new Empty();
        }

        //Immediate Loaders
        public IEnumerator StartAssetLoadUnordered(FileSystemInfo handle)
        {
            return StartAssetLoad(handle, LoadOrderType.LoadUnordered, true);
        }

        public IEnumerator StartAssetLoadFirst(FileSystemInfo handle)
        {
            return StartAssetLoad(handle, LoadOrderType.LoadFirst, true);
        }

        public IEnumerator StartAssetLoadLast(FileSystemInfo handle)
        {
            return StartAssetLoad(handle, LoadOrderType.LoadLast, true);
        }

        //On-Demand Loaders
        public IEnumerator StartAssetDataLoad(FileSystemInfo handle)
        {
            return StartAssetLoad(handle, LoadOrderType.LoadFirst, false);
        }

        public IEnumerator RegisterAssetLoadFirstLate(FileSystemInfo handle)
        {
            return RegisterAssetLoadLate(handle, LoadOrderType.LoadFirst);
        }

        public IEnumerator RegisterAssetLoadUnorderedLate(FileSystemInfo handle)
        {
            return RegisterAssetLoadLate(handle, LoadOrderType.LoadUnordered);
        }

        public IEnumerator RegisterAssetLoadLastLate(FileSystemInfo handle)
        {
            return RegisterAssetLoadLate(handle, LoadOrderType.LoadLast);
        }


        public static Sprite LoadSprite(FileSystemInfo file)
        {
            Texture2D spriteTexture = LoadTexture(file);
            if (spriteTexture == null) return null;
            Sprite sprite = Sprite.Create(spriteTexture, new Rect(0, 0, spriteTexture.width, spriteTexture.height), new Vector2(0, 0), 100f);
            sprite.name = file.Name;
            return sprite;
        }


        public static Texture2D LoadTexture(FileSystemInfo file)
        {
            // Load a PNG or JPG file from disk to a Texture2D
            // Returns null if load fails

            byte[] fileData = File.ReadAllBytes(file.FullName);

            Texture2D tex2D = new Texture2D(2, 2);
            if (tex2D.LoadImage(fileData)) return tex2D;

            return null;
        }


        public void LoadDirectAssets(CoroutineStarter starter, string folderPath, string guid, string[] dependancies, string[] loadFirst, string[] loadAny, string[] loadLast)
        {
            foreach (string bundleFirst in loadFirst)
            {
                if (!string.IsNullOrEmpty(bundleFirst))
                {
                    starter(StartAssetLoadDirect(folderPath, bundleFirst, guid, dependancies, LoadOrderType.LoadFirst, false));
                }
                
            }

            foreach (string bundleAny in loadAny)
            {
                if (!string.IsNullOrEmpty(bundleAny))
                {
                    starter(StartAssetLoadDirect(folderPath, bundleAny, guid, dependancies, LoadOrderType.LoadUnordered, false));
                }
            }

            foreach (string bundleLast in loadLast)
            {
                if (!string.IsNullOrEmpty(bundleLast))
                {
                    starter(StartAssetLoadDirect(folderPath, bundleLast, guid, dependancies, LoadOrderType.LoadLast, false));
                }
            }
        }

        public IEnumerator StartAssetLoad(FileSystemInfo handle, LoadOrderType loadOrder, bool allowUnload)
        {
            FileInfo file = handle.ConsumeFile();

            string bundleID = file.FullName.Replace(file.Name, "") + " : " + file.Name;

            return LoadAssetsFromPathAsync(file.FullName, bundleID, "", new string[] { }, loadOrder, allowUnload).TryCatch(e =>
            {
                OtherLogger.LogError("Failed to load mod (" + bundleID + ")");
                OtherLogger.LogError(e.ToString());
                LoaderStatus.UpdateProgress(bundleID, 1);
                LoaderStatus.RemoveActiveLoader(bundleID, true);
            });
        }


        public IEnumerator StartAssetLoadDirect(string folderPath, string bundleName, string guid, string[] dependancies, LoadOrderType loadOrder, bool allowUnload)
        {
            OtherLogger.Log("Direct Loading Bundle (" + bundleName + ")", OtherLogger.LogType.General);

            string bundlePath = Path.Combine(folderPath, bundleName);
            string lateName = "late_" + bundleName;
            string latePath = Path.Combine(folderPath, lateName);
            string bundleID = bundlePath.Replace(bundleName, "") + " : " + bundleName;
            IEnumerator afterLoad = null;

            if (File.Exists(latePath))
            {
                afterLoad = RegisterAssetLoadLate(latePath, lateName, loadOrder);
            }

            return LoadAssetsFromPathAsync(bundlePath, bundleID, guid, dependancies, loadOrder, allowUnload, afterLoad).TryCatch(e =>
            {
                OtherLogger.LogError("Failed to load mod (" + bundleID + ")");
                OtherLogger.LogError(e.ToString());
                LoaderStatus.UpdateProgress(bundleID, 1);
                LoaderStatus.RemoveActiveLoader(bundleID, true);
            });
        }


        public IEnumerator RegisterAssetLoadLate(FileSystemInfo handle, LoadOrderType loadOrder)
        {
            FileInfo file = handle.ConsumeFile();

            return RegisterAssetLoadLate(file.FullName, file.Name, loadOrder);
        }


        public IEnumerator RegisterAssetLoadLate(string bundlePath, string bundleName, LoadOrderType loadOrder)
        {
            //In order to get this bundle to load later, we want to replace the file path for the already loaded FVRObject
            string bundleID = bundlePath.Replace(bundleName, "") + " : " + bundleName.Replace("late_", "");
            OtherLoader.ManagedBundles[bundleID] = bundlePath;
            LoaderStatus.TrackLoader(bundleID, loadOrder);

            AnvilCallbackBase anvilCallbackBase;
            if (AnvilManager.m_bundles.TryGetValue(bundleID, out anvilCallbackBase))
            {
                AnvilManager.m_bundles.m_lookup.Remove(bundleID);
                AnvilManager.m_bundles.m_loading.Remove(anvilCallbackBase);

                if (OtherLoader.LogLoading.Value)
                {
                    OtherLogger.Log("Registered asset bundle to load later (" + bundlePath + ")", OtherLogger.LogType.General);
                    OtherLogger.Log("This bundle will replace the data bundle (" + bundleID + ")", OtherLogger.LogType.Loading);
                }
                else
                {
                    OtherLogger.Log("Registered asset bundle to load later (" + bundleName + ")", OtherLogger.LogType.General);
                    OtherLogger.Log("This bundle will replace the data bundle (" + LoaderUtils.GetBundleNameFromUniqueID(bundleID) + ")", OtherLogger.LogType.Loading);
                }
            }
            else
            {
                OtherLogger.LogError("Tried to register bundle to load later, but pre-bundle had not yet been loaded! (" + bundleID + ")");
            }

            yield return null;
        }


        public void LoadLegacyAssets(CoroutineStarter starter)
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

                    string bundleID = bundlePath.Replace(Path.GetFileName(bundlePath), "") + " : " + Path.GetFileName(bundlePath);

                    IEnumerator routine = LoadAssetsFromPathAsync(bundlePath, bundleID, "", new string[] { }, LoadOrderType.LoadUnordered, true).TryCatch<Exception>(e =>
                    {
                        OtherLogger.LogError("Failed to load mod (" + bundleID + ")");
                        OtherLogger.LogError(e.ToString());
                        LoaderStatus.UpdateProgress(bundleID, 1);
                        LoaderStatus.RemoveActiveLoader(bundleID, true);
                    });

                    starter(routine);
                }
            }
        }


        


        private IEnumerator LoadAssetsFromPathAsync(string path, string bundleID, string guid, string[] dependancies, LoadOrderType loadOrder, bool allowUnload, IEnumerator afterLoad = null)
        {
            //Start tracking this bundle and then wait a frame for everything else to be tracked
            LoaderStatus.TrackLoader(bundleID, loadOrder);
            yield return null;

            //If there are many active loaders at once, we should wait our turn
            bool overTime = false;
            while (!LoaderStatus.CanOrderedModLoad(bundleID))
            {
                if(!overTime && Time.time - LoaderStatus.LastLoadEventTime > 30)
                {
                    OtherLogger.Log("Bundle has been waiting a long time to load! (" + bundleID + ")", OtherLogger.LogType.General);
                    LoaderStatus.PrintWaitingBundles(bundleID);
                    overTime = true;
                }

                yield return null;
            }

            //Begin the loading process
            LoaderStatus.AddActiveLoader(bundleID);

            if (OtherLoader.LogLoading.Value)
                OtherLogger.Log("Beginning async loading of asset bundle (" + bundleID + ")", OtherLogger.LogType.General);


            //Load the bundle and apply it's contents
            float time = Time.time;
            LoaderStatus.UpdateProgress(bundleID, UnityEngine.Random.Range(.1f, .3f));

            AnvilCallback<AssetBundle> bundle = LoaderUtils.LoadAssetBundle(path);
            yield return bundle;

            LoaderStatus.UpdateProgress(bundleID, 0.9f);

            yield return ApplyLoadedAssetBundleAsync(bundle, bundleID).TryCatch(e =>
            {
                OtherLogger.LogError("Failed to load mod (" + bundleID + ")");
                OtherLogger.LogError(e.ToString());
                LoaderStatus.UpdateProgress(bundleID, 1);
                LoaderStatus.RemoveActiveLoader(bundleID, true);
            });

            //Log that the bundle is loaded
            if (OtherLoader.LogLoading.Value)
                OtherLogger.Log($"[{(Time.time - time).ToString("0.000")} s] Completed loading bundle ({bundleID})", OtherLogger.LogType.General);
            else
                OtherLogger.Log($"[{(Time.time - time).ToString("0.000")} s] Completed loading bundle ({LoaderUtils.GetBundleNameFromUniqueID(bundleID)})", OtherLogger.LogType.General);



            if (allowUnload && OtherLoader.OptimizeMemory.Value)
            {
                OtherLogger.Log("Unloading asset bundle (Optimize Memory is true)", OtherLogger.LogType.Loading);
                bundle.Result.Unload(false);
            }
            else
            {
                AnvilManager.m_bundles.Add(bundleID, bundle);
            }

            OtherLoader.ManagedBundles.Add(bundleID, path);
            LoaderStatus.UpdateProgress(bundleID, 1);
            LoaderStatus.RemoveActiveLoader(bundleID, !(OtherLoader.OptimizeMemory.Value && allowUnload));

            if(afterLoad != null)
            {
                yield return afterLoad;
            }
        }



        




        private IEnumerator ApplyLoadedAssetBundleAsync(AnvilCallback<AssetBundle> bundle, string bundleID)
        {
            MechanicalAccuracyLoader mechanicalAccuracyLoader = new MechanicalAccuracyLoader();
            yield return mechanicalAccuracyLoader.LoadAssetsFromBundle(bundle.Result, bundleID);

            FVRObjectLoader fvrObjectLoader = new FVRObjectLoader();
            yield return fvrObjectLoader.LoadAssetsFromBundle(bundle.Result, bundleID);

            RoundDisplayDataLoader roundDisplayDataLoader = new RoundDisplayDataLoader();
            yield return roundDisplayDataLoader.LoadAssetsFromBundle(bundle.Result, bundleID);

            //Before we load the spawnerIDs, we must add any new spawner category definitions
            CategoryDefinitionLoader categoryDefinitionLoader = new CategoryDefinitionLoader();
            yield return categoryDefinitionLoader.LoadAssetsFromBundle(bundle.Result, bundleID);

            ItemSpawnerIdLoader itemSpawnerIdLoader = new ItemSpawnerIdLoader();
            yield return itemSpawnerIdLoader.LoadAssetsFromBundle(bundle.Result, bundleID);

            //Load the spawner entries for the new spawner
            AssetBundleRequest spawnerEntries = bundle.Result.LoadAllAssetsAsync<ItemSpawnerEntry>();
            yield return spawnerEntries;
            LoadSpawnerEntries(spawnerEntries.allAssets);

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

            AssetBundleRequest AudioImpactSet = bundle.Result.LoadAllAssetsAsync<AudioImpactSet>();
            yield return AudioImpactSet;
            LoadAudioImpactSetEntries(AudioImpactSet.allAssets);

            TutorialBlockLoader tutorialBlockLoader = new TutorialBlockLoader();
            yield return tutorialBlockLoader.LoadAssetsFromBundle(bundle.Result, bundleID);

            AssetBundleRequest Quickbelts = bundle.Result.LoadAllAssetsAsync<GameObject>();
            yield return Quickbelts;
            LoadQuickbeltEntries(Quickbelts.allAssets);
        }


        private void LoadSpawnerEntries(UnityEngine.Object[] allAssets)
        {
            List<ItemSpawnerID> convertedSpawnerIDs = new List<ItemSpawnerID>();

            foreach (ItemSpawnerEntry entry in allAssets)
            {
                OtherLogger.Log("Loading new item spawner entry: " + entry.EntryPath, OtherLogger.LogType.Loading);

                entry.IsModded = true;

                entry.PopulateIDsFromObj();
                PopulateEntryPaths(entry);
                OtherLoader.SpawnerEntriesByID[entry.MainObjectID] = entry;

                if (OtherLoader.UnlockSaveData.ShouldAutoUnlockItem(entry)) OtherLoader.UnlockSaveData.UnlockItem(entry.MainObjectID);
                RegisterItemIntoMetaTagSystem(entry);

                convertedSpawnerIDs.Add(AddEntryToLegacySpawner(entry));
            }

            foreach(ItemSpawnerID spawnerID in convertedSpawnerIDs)
            {
                spawnerID.Secondaries = spawnerID.Secondaries_ByStringID.Where(o => IM.Instance.SpawnerIDDic.ContainsKey(o)).Select(o => IM.Instance.SpawnerIDDic[o]).ToArray();
            }
        }

        private void LoadHandlingGrabSetEntries(UnityEngine.Object[] allAssets)
        {
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

        private void LoadAudioImpactSetEntries(UnityEngine.Object[] allAssets)
        {
            foreach (AudioImpactSet AIS in allAssets)
            {
                //resize SM's AIS list to its length + 1, insert AIS into list
                OtherLogger.Log("Loading new Audio Impact Set: " + AIS.name, OtherLogger.LogType.Loading);
                Array.Resize(ref ManagerSingleton<SM>.Instance.AudioImpactSets, ManagerSingleton<SM>.Instance.AudioImpactSets.Length + 1);
                ManagerSingleton<SM>.Instance.AudioImpactSets[ManagerSingleton<SM>.Instance.AudioImpactSets.Length - 1] = AIS;
                //clears impactdic
                ManagerSingleton<SM>.Instance.m_impactDic = new Dictionary<ImpactType, Dictionary<MatSoundType, Dictionary<AudioImpactIntensity, AudioEvent>>>();
                //remakes impactdic
                ManagerSingleton<SM>.Instance.generateImpactDictionary(); //TODO: this is an inefficient method. pls dont remake
                //the dictionary every time a new one is added! oh well. it works
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
                        Array.Resize(ref GM.Instance.QuickbeltConfigurations, GM.Instance.QuickbeltConfigurations.Length + 1);
                        GM.Instance.QuickbeltConfigurations[GM.Instance.QuickbeltConfigurations.Length - 1] = quickbelt;
                    }
                }
            }
        }
        

        private void RegisterItemIntoMetaTagSystem(ItemSpawnerEntry entry)
        {
            if (!IM.OD.ContainsKey(entry.MainObjectID)) return;

            FVRObject mainObject = IM.OD[entry.MainObjectID];
            string pageString = entry.EntryPath.Split('/')[0];
            string subCategoryString = entry.EntryPath.Split('/')[1];

            ItemSpawnerV2.PageMode page = ItemSpawnerV2.PageMode.Firearms;
            if(Enum.IsDefined(typeof(ItemSpawnerV2.PageMode), pageString))
            {
                page = (ItemSpawnerV2.PageMode)Enum.Parse(typeof(ItemSpawnerV2.PageMode), pageString);
            }

            ItemSpawnerID.ESubCategory subCategory = ItemSpawnerID.ESubCategory.None;
            if (Enum.IsDefined(typeof(ItemSpawnerID.ESubCategory), subCategoryString))
            {
                subCategory = (ItemSpawnerID.ESubCategory)Enum.Parse(typeof(ItemSpawnerID.ESubCategory), subCategoryString);
            }

            IM.AddMetaTag(subCategoryString, TagType.SubCategory, entry.MainObjectID, page);

            entry.ModTags.ForEach(tag =>
                IM.AddMetaTag(tag, TagType.ModTag, entry.MainObjectID, page));

            RegisterItemIntoMetaTagSystem(mainObject, page);
        }


        private void RegisterItemIntoMetaTagSystem(FVRObject mainObject, ItemSpawnerV2.PageMode page)
        {
            if(mainObject.Category == FVRObject.ObjectCategory.Firearm)
            {
                IM.AddMetaTag(mainObject.TagSet.ToString(), TagType.Set, mainObject.ItemID, page);
                IM.AddMetaTag(mainObject.TagEra.ToString(), TagType.Era, mainObject.ItemID, page);
                IM.AddMetaTag(mainObject.TagFirearmSize.ToString(), TagType.Size, mainObject.ItemID, page);
                IM.AddMetaTag(mainObject.TagFirearmAction.ToString(), TagType.Action, mainObject.ItemID, page);
                IM.AddMetaTag(mainObject.TagFirearmRoundPower.ToString(), TagType.RoundClass, mainObject.ItemID, page);
                IM.AddMetaTag(mainObject.TagFirearmCountryOfOrigin.ToString(), TagType.CountryOfOrigin, mainObject.ItemID, page);
                IM.AddMetaTag(mainObject.TagFirearmFirstYear.ToString(), TagType.IntroductionYear, mainObject.ItemID, page);
                IM.AddMetaTag(mainObject.MagazineType.ToString(), TagType.MagazineType, mainObject.ItemID, page);

                if(mainObject.UsesRoundTypeFlag) 
                    IM.AddMetaTag(mainObject.RoundType.ToString(), TagType.Caliber, mainObject.ItemID, page);

                mainObject.TagFirearmFiringModes.ForEach(tag => 
                    IM.AddMetaTag(tag.ToString(), TagType.FiringMode, mainObject.ItemID, page));

                mainObject.TagFirearmFeedOption.ForEach(tag =>
                    IM.AddMetaTag(tag.ToString(), TagType.FeedOption, mainObject.ItemID, page));

                mainObject.TagFirearmMounts.ForEach(mode =>
                    IM.AddMetaTag(mode.ToString(), TagType.AttachmentMount, mainObject.ItemID, page));
            }

            else if(mainObject.Category == FVRObject.ObjectCategory.Attachment)
            {
                IM.AddMetaTag(mainObject.TagSet.ToString(), TagType.Set, mainObject.ItemID, page);
                IM.AddMetaTag(mainObject.TagEra.ToString(), TagType.Era, mainObject.ItemID, page);
                IM.AddMetaTag(mainObject.TagAttachmentFeature.ToString(), TagType.AttachmentFeature, mainObject.ItemID, page);
                IM.AddMetaTag(mainObject.TagAttachmentMount.ToString(), TagType.AttachmentMount, mainObject.ItemID, page);
            }
        }


        private ItemSpawnerID AddEntryToLegacySpawner(ItemSpawnerEntry entry)
        {
            if(!string.IsNullOrEmpty(entry.MainObjectID) && IM.OD.ContainsKey(entry.MainObjectID))
            {
                ItemSpawnerID itemSpawnerID = ConvertEntryToSpawnerID(entry);

                if (itemSpawnerID == null) return null;

                IM.CD[itemSpawnerID.Category].Add(itemSpawnerID);
                IM.SCD[itemSpawnerID.SubCategory].Add(itemSpawnerID);
                IM.Instance.SpawnerIDDic[itemSpawnerID.MainObject.ItemID] = itemSpawnerID;
                OtherLoader.SpawnerIDsByMainObject[itemSpawnerID.MainObject.ItemID] = itemSpawnerID;

                return itemSpawnerID;
            }

            return null;
        }

        private ItemSpawnerID ConvertEntryToSpawnerID(ItemSpawnerEntry entry)
        {
            OtherLogger.Log("Converting to legacy spawner: " + entry.MainObjectID, OtherLogger.LogType.Loading);

            List<ItemSpawnerID.ESubCategory> subcats = entry.EntryPath
                .Split('/')
                .Where(o => Enum.IsDefined(typeof(ItemSpawnerID.ESubCategory), o))
                .Select(o => (ItemSpawnerID.ESubCategory)Enum.Parse(typeof(ItemSpawnerID.ESubCategory), o))
                .ToList();

            OtherLogger.Log("Possible subcategories: " + subcats.Count(), OtherLogger.LogType.Loading);

            if (subcats.Count() == 0 ||
                subcats[0] == ItemSpawnerID.ESubCategory.None ||
                !IM.OD.ContainsKey(entry.MainObjectID)) return null;


            ItemSpawnerID itemSpawnerID = ScriptableObject.CreateInstance<ItemSpawnerID>();

            foreach(ItemSpawnerCategoryDefinitions.Category category in IM.CDefs.Categories)
            {
                if(category.Subcats.Any(o => o.Subcat == subcats[0]))
                {
                    itemSpawnerID.Category = category.Cat;
                    itemSpawnerID.SubCategory = subcats[0];
                }
            }

            
            itemSpawnerID.MainObject = IM.OD[entry.MainObjectID];
            itemSpawnerID.SecondObject = entry.SecondaryObjectIDs.Where(o => IM.OD.ContainsKey(o)).Select(o => IM.OD[o]).FirstOrDefault();
            itemSpawnerID.DisplayName = entry.DisplayName;
            itemSpawnerID.IsDisplayedInMainEntry = entry.IsDisplayedInMainEntry;
            itemSpawnerID.ItemID = entry.MainObjectID;
            itemSpawnerID.ModTags = entry.ModTags;
            itemSpawnerID.Secondaries_ByStringID = entry.SecondaryObjectIDs.Where(o => IM.OD.ContainsKey(o)).ToList();
            itemSpawnerID.Secondaries = new ItemSpawnerID[] { };
            itemSpawnerID.Sprite = entry.EntryIcon;
            itemSpawnerID.UsesLargeSpawnPad = entry.UsesLargeSpawnPad;
            itemSpawnerID.IsReward = entry.IsReward;

            return itemSpawnerID;
        }



    }

}