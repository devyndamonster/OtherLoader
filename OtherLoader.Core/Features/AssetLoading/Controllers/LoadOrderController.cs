using OtherLoader.Core.Enums;
using OtherLoader.Core.Features.AssetLoading.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Core.Controllers
{
    public class LoadOrderController : ILoadOrderController
    {
        private Dictionary<string, BundleLoadData> _bundlePathToData;

        private Dictionary<string, ModLoadData> _modIdToData;

        public LoadOrderController()
        {
            _bundlePathToData = new Dictionary<string, BundleLoadData>();
            _modIdToData = new Dictionary<string, ModLoadData>();
        }

        public void RegisterBundleForLoading(string bundlePath, string modId, LoadOrderType loadOrder)
        {
            if (!_modIdToData.ContainsKey(modId))
            {
                _modIdToData[modId] = new ModLoadData
                {
                    ModId = modId,
                    BundlePaths = new List<string>(),
                };
            }

            _modIdToData[modId].BundlePaths.Add(bundlePath);

            _bundlePathToData[bundlePath] = new BundleLoadData
            {
                BundlePath = bundlePath,
                ModId = modId,
                LoadState = BundleLoadState.Waiting,
                LoadOrder = loadOrder,
            };
        }

        public void RegisterBundleLoadingStarted(string bundlePath)
        {
            _bundlePathToData[bundlePath].LoadState = BundleLoadState.Loading;
        }

        public void RegisterBundleLoadingComplete(string bundlePath)
        {
            _bundlePathToData[bundlePath].LoadState = BundleLoadState.Loaded;
        }

        //TODO: this should probably not iterate over everything every time it's checked
        public bool CanBundleBeginLoading(string bundlePath)
        {
            var bundleData = _bundlePathToData[bundlePath];
            var modData = _modIdToData[bundleData.ModId];

            IEnumerable<BundleLoadData> dependancies;

            if(bundleData.LoadOrder == LoadOrderType.LoadFirst || bundleData.LoadOrder == LoadOrderType.LoadUnordered)
            {
                dependancies = modData.BundlePaths
                    .Select(path => _bundlePathToData[path])
                    .Where(bundle => bundle.LoadOrder == LoadOrderType.LoadFirst)
                    .TakeWhile(bundle => bundle.BundlePath != bundlePath);
            }

            else
            {
                dependancies = modData.BundlePaths
                    .Select(path => _bundlePathToData[path])
                    .TakeWhile(bundle => bundle.BundlePath != bundlePath);
            }

            return dependancies.All(bundle => bundle.LoadState == BundleLoadState.Loaded);
        }
    }
}
