
namespace OtherLoader.Core.Services
{
    public interface IApplicationPathService
    {
        public string MainLegacyDirectory { get; }

        public string OtherLoaderSaveDirectory { get; }

        public string UnlockedItemSaveDataPath { get; }
        
        public void InitializeApplicationPaths();
    }
}
