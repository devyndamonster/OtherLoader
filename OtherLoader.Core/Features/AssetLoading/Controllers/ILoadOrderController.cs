using OtherLoader.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Core.Controllers
{
    public interface ILoadOrderController
    {
        public void RegisterBundleForLoading(string bundlePath, string modId, LoadOrderType loadOrder);

        public bool CanBundleBeginLoading(string bundlePath);

        public void RegisterBundleLoadingStarted(string bundlePath);

        public void RegisterBundleLoadingComplete(string bundlePath);
    }
}
