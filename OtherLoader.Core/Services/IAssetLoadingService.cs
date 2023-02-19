using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace OtherLoader.Core.Services
{
    public interface IAssetLoadingService
    {

        /* Steps of asset loading
         * 1. Register the asset for loading
         * 2. Have the asset wait to start loading untill it is allowed based on order
         * 3. Register that the asset is now actively being loaded
         * 4. Load the asset bundle
         * 5. Get the assets from the asset bundle an apply them to the game
         * 6. Register asset bundle as finished loading
         */

        public IEnumerator StartAssetLoadFirst(FileSystemInfo handle);

        public IEnumerator StartAssetLoadUnordered(FileSystemInfo handle);

        public IEnumerator StartAssetLoadLast(FileSystemInfo handle);

        public IEnumerator StartAssetLoadData(FileSystemInfo handle);

        public IEnumerator RegisterAssetLoadFirstLate(FileSystemInfo handle);

        public IEnumerator RegisterAssetLoadUnorderedLate(FileSystemInfo handle);

        public IEnumerator RegisterAssetLoadLastLate(FileSystemInfo handle);

        public IEnumerable<IEnumerator> LoadDirectAssets(string folderPath, string guid, string[] dependancies, string[] loadFirst, string[] loadAny, string[] loadLast);

        public IEnumerable<IEnumerator> LoadLegacyAssets();

    }
}
