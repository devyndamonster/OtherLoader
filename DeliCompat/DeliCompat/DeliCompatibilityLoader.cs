using Deli;
using Deli.Runtime;
using Deli.Setup;
using Deli.VFS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.DeliCompat
{
    
    public class DeliCompatibilityLoader : DeliBehaviour
    {
        private void Awake()
        {
            Stages.Runtime += DuringRuntime;
        }

        private void DuringRuntime(RuntimeStage stage)
        {
            stage.DelayedReaders.Get<byte[]>();

            stage.RuntimeAssetLoaders[Source, "item"] = StartAssetLoadFirst;
            stage.RuntimeAssetLoaders[Source, "item_last"] = StartAssetLoadFirst;
            stage.RuntimeAssetLoaders[Source, "item_unordered"] = StartAssetLoadFirst;
        }

        public IEnumerator StartAssetLoadFirst(RuntimeStage stage, Mod mod, IHandle handle)
        {
            yield return null;
            OtherLogger.Log("Old mod found: " + mod.Resources.Path, OtherLogger.LogType.General);
            OtherLogger.Log("Alt path: " + handle.Path, OtherLogger.LogType.General);
            IFileHandle file = handle as IFileHandle;
            OtherLogger.Log("Another path: " + file.Directory, OtherLogger.LogType.General);
            
        }
    }

}
