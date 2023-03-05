using OtherLoader.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Core.Controllers
{
    public class LoadOrderController : ILoadOrderController
    {
        public bool CanBundleBeginLoading(string bundleName)
        {
            throw new NotImplementedException();
        }
        
        public void RegisterBundleForLoading(string bundleName, string modId, LoadOrderType loadOrder)
        {
            throw new NotImplementedException();
        }

        public void RegisterBundleLoadingComplete(string bundleName)
        {
            throw new NotImplementedException();
        }

        public void RegisterBundleLoadingStarted(string bundleName)
        {
            throw new NotImplementedException();
        }
    }
}
