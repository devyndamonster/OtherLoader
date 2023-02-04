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
using OtherLoader.Core.Services;

namespace OtherLoader
{
    public class ItemLoader
    {

        private static BaseAssetLoader[] assetLoaders = new BaseAssetLoader[] 
        {
            new MechanicalAccuracyLoader(),
            new FVRObjectLoader(),
            new RoundDisplayDataLoader(),
            new CategoryDefinitionLoader(),
            new ItemSpawnerIdLoader(),
            new ItemSpawnerEntryLoader(),
            new HandlingGrabSetLoader(),
            new HandlingReleaseSetLoader(),
            new HandlingSlotSetLoader(),
            new BulletImpactSetLoader(),
            new AudioImpactSetLoader(),
            new TutorialBlockLoader(),
            new QuickbeltLoader()
        };

        private static IApplicationPathService _applicationPathService = new ApplicationPathService(Application.dataPath);


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

        private string BuildBundleID(string bundlePath, string bundleName)
        {
            string directoryPath = Path.GetDirectoryName(bundlePath) + "\\";
            return directoryPath + " : " + bundleName;
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

            string bundleID = BuildBundleID(file.FullName, file.Name);

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
            string bundleID = BuildBundleID(bundlePath, bundleName);
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
            string originalBundleName = bundleName.Replace("late_", "");
            string bundleID = BuildBundleID(bundlePath, originalBundleName);
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
            if (!Directory.Exists(_applicationPathService.MainLegacyDirectory)) Directory.CreateDirectory(_applicationPathService.MainLegacyDirectory);

            OtherLogger.Log("Plugins folder found (" + Paths.PluginPath + ")", OtherLogger.LogType.General);

            List<string> legacyPaths = Directory.GetDirectories(Paths.PluginPath, "LegacyVirtualObjects", SearchOption.AllDirectories).ToList();
            legacyPaths.Add(_applicationPathService.MainLegacyDirectory);

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

                    string bundleName = Path.GetFileName(bundlePath);
                    string bundleID = BuildBundleID(bundlePath, bundleName);

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

            yield return LoaderUtils.WaitUntilBundleCanLoad(bundleID);

            float loadingTime = Time.time;
            BeginActiveLoading(bundleID);

            AnvilCallback<AssetBundle> bundle = LoaderUtils.LoadAssetBundle(path);
            yield return bundle;

            yield return ApplyLoadedAssetBundleAsync(bundle, bundleID).TryCatch(exception => { HandleApplyBundleFailed(exception, bundleID); });
            
            FinishActiveLoading(bundle, bundleID, path, allowUnload, loadingTime);

            if(afterLoad != null)
            {
                yield return afterLoad;
            }
        }


        private IEnumerator ApplyLoadedAssetBundleAsync(AnvilCallback<AssetBundle> bundle, string bundleID)
        {
            foreach(BaseAssetLoader loader in assetLoaders)
            {
                IEnumerator loadEnumerator = loader.LoadAssetsFromBundle(bundle.Result, bundleID);
                yield return loadEnumerator.TryCatch(exception => { HandleLoadFromBundleFailed(exception, loader, bundleID); });
            }
        }


        private void HandleApplyBundleFailed(Exception exception, string bundleId)
        {
            OtherLogger.LogError("Failed to load mod (" + bundleId + ")");
            OtherLogger.LogError(exception.ToString());
            LoaderStatus.UpdateProgress(bundleId, 1);
            LoaderStatus.RemoveActiveLoader(bundleId, true);
        }

        private void HandleLoadFromBundleFailed(Exception exception, BaseAssetLoader loader, string bundleId)
        {
            OtherLogger.LogError("Failed to load assets in bundle (" + bundleId + ") with loader type (" + loader.GetType().ToString() + ")");
            OtherLogger.LogError(exception.ToString());
        }

        private void BeginActiveLoading(string bundleId)
        {
            LoaderStatus.AddActiveLoader(bundleId);
            LogLoadingStart(bundleId);
            LoaderStatus.UpdateProgress(bundleId, UnityEngine.Random.Range(.1f, .3f));
        }

        private void FinishActiveLoading(AnvilCallback<AssetBundle> bundle, string bundleId, string path, bool allowUnload, float loadingTime)
        {
            LogLoadingDone(bundleId, loadingTime);
            HandleOptimizeMemory(bundle, bundleId, allowUnload);

            OtherLoader.ManagedBundles.Add(bundleId, path);
            LoaderStatus.UpdateProgress(bundleId, 1);
            LoaderStatus.RemoveActiveLoader(bundleId, !(OtherLoader.OptimizeMemory.Value && allowUnload));
        }

        private void LogLoadingStart(string bundleId)
        {
            if (OtherLoader.LogLoading.Value)
            {
                OtherLogger.Log("Beginning async loading of asset bundle (" + bundleId + ")", OtherLogger.LogType.General);
            }
        }


        private void LogLoadingDone(string bundleId, float loadingTime)
        {
            if (OtherLoader.LogLoading.Value)
            {
                OtherLogger.Log($"[{(Time.time - loadingTime).ToString("0.000")} s] Completed loading bundle ({bundleId})", OtherLogger.LogType.General);
            }

            else
            {
                OtherLogger.Log($"[{(Time.time - loadingTime).ToString("0.000")} s] Completed loading bundle ({LoaderUtils.GetBundleNameFromUniqueID(bundleId)})", OtherLogger.LogType.General);
            }
        }


        private void HandleOptimizeMemory(AnvilCallback<AssetBundle> bundle, string bundleId, bool allowUnload)
        {
            if (allowUnload && OtherLoader.OptimizeMemory.Value)
            {
                OtherLogger.Log("Unloading asset bundle (Optimize Memory is true)", OtherLogger.LogType.Loading);
                bundle.Result.Unload(false);
            }
            else
            {
                AnvilManager.m_bundles.Add(bundleId, bundle);
            }
        }
    }

}