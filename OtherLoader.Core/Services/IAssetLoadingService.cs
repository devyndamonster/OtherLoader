using OtherLoader.Core.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace OtherLoader.Core.Services
{
    public interface IAssetLoadingService
    {
        public event Action<object[]> OnAssetLoadComplete;
        
        public IEnumerator StartAssetLoadFirst(FileSystemInfo handle);

        public IEnumerator StartAssetLoadUnordered(FileSystemInfo handle);

        public IEnumerator StartAssetLoadLast(FileSystemInfo handle);

        public IEnumerator StartAssetLoadData(FileSystemInfo handle);

        public IEnumerator RegisterAssetLoadFirstLate(FileSystemInfo handle);

        public IEnumerator RegisterAssetLoadUnorderedLate(FileSystemInfo handle);

        public IEnumerator RegisterAssetLoadLastLate(FileSystemInfo handle);

        public IEnumerable<IEnumerator> LoadDirectAssets(DirectLoadModData modData);

        public IEnumerable<IEnumerator> LoadLegacyAssets();

    }
}
