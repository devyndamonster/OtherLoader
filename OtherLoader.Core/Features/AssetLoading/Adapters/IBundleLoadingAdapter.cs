using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Core.Adapters
{
    public interface IBundleLoadingAdapter
    {
        public IEnumerator LoadAssetsFromAssetBundle(string bundlePath);

        public void AddManagedBundle(string bundlePath);

        public void AddLateManagedBundle(string bundlePath);

    }
}
