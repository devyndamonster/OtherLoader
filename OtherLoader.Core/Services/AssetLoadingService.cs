using OtherLoader.Core.Enums;
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
        public IEnumerable<IEnumerator> LoadDirectAssets(string folderPath, string guid, string[] dependancies, string[] loadFirst, string[] loadAny, string[] loadLast)
        {
            var loadCoroutines = new List<IEnumerator>();
            
            foreach (string bundleFirst in loadFirst)
            {
                if (!string.IsNullOrEmpty(bundleFirst))
                {
                    loadCoroutines.Add(StartAssetLoadDirect(folderPath, bundleFirst, guid, dependancies, LoadOrderType.LoadFirst, false));
                }
            }

            foreach (string bundleAny in loadAny)
            {
                if (!string.IsNullOrEmpty(bundleAny))
                {
                    loadCoroutines.Add(StartAssetLoadDirect(folderPath, bundleAny, guid, dependancies, LoadOrderType.LoadUnordered, false));
                }
            }

            foreach (string bundleLast in loadLast)
            {
                if (!string.IsNullOrEmpty(bundleLast))
                {
                    loadCoroutines.Add(StartAssetLoadDirect(folderPath, bundleLast, guid, dependancies, LoadOrderType.LoadLast, false));
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
        
        private IEnumerator StartAssetLoadDirect(string folderPath, string bundleName, string guid, string[] dependancies, LoadOrderType loadOrder, bool allowUnload)
        {
            yield return null;
        }
    }
}
