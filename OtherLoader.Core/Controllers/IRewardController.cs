using OtherLoader.Core.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Core.Controllers
{
    public interface IRewardController
    {
        public bool IsItemUnlocked(string mainObjectId);

        public bool ForceUnlockItem(string mainObjectId);

        public bool AutoUnlockItem(string mainObjectId);
    }
}
