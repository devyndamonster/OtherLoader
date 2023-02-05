using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OtherLoader.Core.Controllers
{
    public interface IAssetLoadingController
    {
        public IEnumerator StartAssetLoadFirst(FileSystemInfo handle);
        

    }
}
