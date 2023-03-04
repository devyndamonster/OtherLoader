using OtherLoader.Core.Adapters;
using OtherLoader.Core.Controllers;
using OtherLoader.Core.Enums;
using OtherLoader.Core.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OtherLoader.Core.Services
{
    public class AssetLoadingService : IAssetLoadingService
    {
        public event Action<object[]> OnAssetLoadComplete;

        private readonly ILoadOrderController _loadOrderController;
        private readonly IBundleLoadingAdapter _bundleLoadingAdapter;

        public AssetLoadingService(ILoadOrderController loadOrderController, IBundleLoadingAdapter bundleLoadingAdapter)
        {
            _loadOrderController = loadOrderController;
            _bundleLoadingAdapter = bundleLoadingAdapter;
        }
        
        public IEnumerable<IEnumerator> LoadDirectAssets(DirectLoadModData modData)
        {
            var loadCoroutines = new List<IEnumerator>();
            
            foreach(var loadOrderBundles in modData.BundlesByLoadOrder)
            {
                foreach (var bundleName in loadOrderBundles.Value)
                {
                    if (!string.IsNullOrEmpty(bundleName))
                    {
                        loadCoroutines.Add(LoadAssetBundle(modData.FolderPath, bundleName, loadOrderBundles.Key));
                    }
                }
            }
            
            return loadCoroutines;
        }

        public IEnumerable<IEnumerator> LoadLegacyAssets()
        {
            throw new NotImplementedException();
        }

        public IEnumerator RegisterAssetLoadFirstLate(FileSystemInfo handle)
        {
            throw new NotImplementedException();
        }

        public IEnumerator RegisterAssetLoadLastLate(FileSystemInfo handle)
        {
            throw new NotImplementedException();
        }

        public IEnumerator RegisterAssetLoadUnorderedLate(FileSystemInfo handle)
        {
            throw new NotImplementedException();
        }

        public IEnumerator StartAssetLoadData(FileSystemInfo handle)
        {
            throw new NotImplementedException();
        }

        public IEnumerator StartAssetLoadFirst(FileSystemInfo handle)
        {
            throw new NotImplementedException();
        }

        public IEnumerator StartAssetLoadLast(FileSystemInfo handle)
        {
            throw new NotImplementedException();
        }

        public IEnumerator StartAssetLoadUnordered(FileSystemInfo handle)
        {
            throw new NotImplementedException();
        }

         /* Steps of asset loading
         * 1. Register the asset for loading
         * 2. Have the asset wait to start loading untill it is allowed based on order
         * 3. Register that the asset is now actively being loaded
         * 4. Load the asset bundle
         * 5. Get the assets from the asset bundle an apply them to the game
         * 6. Register asset bundle as finished loading
         * 7. If the bundle has a late bundle, register that to load later
         */

        private IEnumerator LoadAssetBundle(string modId, string bundlePath, LoadOrderType loadOrder)
        {
            _loadOrderController.RegisterBundleForLoading(bundlePath);
            yield return null;

            while (!_loadOrderController.CanBundleBeginLoading(bundlePath))
            {
                yield return null;
            }

            _loadOrderController.RegisterBundleLoadingStarted(bundlePath);
            
            var resultCoroutine = new ResultCoroutine<object[]>(_bundleLoadingAdapter.LoadAssetsFromAssetBundle(""));
            yield return resultCoroutine;
            OnAssetLoadComplete?.Invoke(resultCoroutine.Result);

            _loadOrderController.RegisterBundleLoadingComplete(bundlePath);

            //TODO: Register bundle to load later

            yield return null;
        }
    }
}
