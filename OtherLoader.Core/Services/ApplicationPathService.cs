using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OtherLoader.Core.Services
{
    public class ApplicationPathService : IApplicationPathService
    {
        public string MainLegacyDirectory => _applicationPath.Replace("/h3vr_Data", "/LegacyVirtualObjects");

        public string OtherLoaderSaveDirectory => _applicationPath.Replace("/h3vr_Data", "/OtherLoader");
        
        public string UnlockedItemSaveDataPath => Path.Combine(OtherLoaderSaveDirectory, "UnlockedItems.json");

        private readonly string _applicationPath;

        public ApplicationPathService(string applicationPath)
        {
            _applicationPath = applicationPath;
        }

        public void InitializeApplicationPaths()
        {
            CreateFolder(MainLegacyDirectory);
            CreateFolder(OtherLoaderSaveDirectory);
        }

        private void CreateFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
