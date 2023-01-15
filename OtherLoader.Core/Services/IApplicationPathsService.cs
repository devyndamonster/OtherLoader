
namespace OtherLoader.Core.Services
{
    public interface IApplicationPathService
    {
        public string GetOtherloaderSaveDirectory();

        public void InitializeApplicationPaths();
    }
}
