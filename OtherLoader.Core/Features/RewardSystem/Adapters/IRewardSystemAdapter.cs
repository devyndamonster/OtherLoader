using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Core.Adapters
{
    public interface IRewardSystemAdapter
    {
        public void InitializeFromSaveFile();

        public bool IsRewardUnlocked(string itemId);
    }
}
