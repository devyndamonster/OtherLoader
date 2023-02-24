using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Core.Controllers
{
    public interface ILoadOrderController
    {
        public void RegisterBundleForLoading(string bundleName);

        public bool CanBundleBeginLoading(string bundleName);

        public void RegisterBundleLoadingStarted(string bundleName);

        public void RegisterBundleLoadingComplete(string bundleName);
    }
}
