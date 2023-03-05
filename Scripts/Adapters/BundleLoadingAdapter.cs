using OtherLoader.Core.Adapters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Scripts.Adapters
{
    public class BundleLoadingAdapter : IBundleLoadingAdapter
    {
        public void AddLateManagedBundle(string bundlePath)
        {
            throw new NotImplementedException();
        }

        public void AddManagedBundle(string bundlePath)
        {
            throw new NotImplementedException();
        }

        public IEnumerator LoadAssetsFromAssetBundle(string bundlePath)
        {
            throw new NotImplementedException();
        }
    }
}
