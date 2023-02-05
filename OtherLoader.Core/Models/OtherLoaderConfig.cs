using BepInEx;
using BepInEx.Configuration;

namespace OtherLoader.Core.Models
{
    public class OtherLoaderConfig
    {
        public ConfigEntry<int> MaxActiveLoadersConfig { get; private set; }
        
        public ConfigEntry<bool> OptimizeMemory { get; private set; }
        
        public ConfigEntry<bool> EnableLogging { get; private set; }
        
        public ConfigEntry<bool> LogLoading { get; private set; }
        
        public ConfigEntry<bool> LogItemSpawner { get; private set; }
        
        public ConfigEntry<bool> LogMetaTagging { get; private set; }
        
        public ConfigEntry<bool> AddUnloadButton { get; private set; }
        
        public ConfigEntry<ItemUnlockMode> UnlockMode { get; private set; }

        public static OtherLoaderConfig LoadFromFile(BaseUnityPlugin plugin)
        {
            return new OtherLoaderConfig
            {
                OptimizeMemory = plugin.Config.Bind(
                    "General",
                    "OptimizeMemory",
                    false,
                    "When enabled, modded assets will be loaded on-demand instead of kept in memory. Can cause small hiccups when spawning modded guns for the first time. Useful if you are low on RAM"
                ),
                EnableLogging = plugin.Config.Bind(
                    "Logging",
                    "EnableLogging",
                    true,
                    "When enabled, OtherLoader will log more than just errors and warning to the output log"
                ),
                LogLoading = plugin.Config.Bind(
                    "Logging",
                    "LogLoading",
                    false,
                    "When enabled, OtherLoader will log additional useful information during the loading process. EnableLogging must be set to true for this to have an effect"
                ),
                LogItemSpawner = plugin.Config.Bind(
                    "Logging",
                    "LogItemSpawner",
                    false,
                    "When enabled, OtherLoader will log additional useful information about the item spawner. EnableLogging must be set to true for this to have an effect"
                ),
                LogMetaTagging = plugin.Config.Bind(
                    "Logging",
                    "LogMetaTagging",
                    false,
                    "When enabled, OtherLoader will log additional useful information about metadata. EnableLogging must be set to true for this to have an effect"
                ),
                MaxActiveLoadersConfig = plugin.Config.Bind(
                    "General",
                    "MaxActiveLoaders",
                    6,
                    "Sets the number of mods that can be loading at once. Values less than 1 will result in all mods being loaded at the same time"
                ),
                UnlockMode = plugin.Config.Bind(
                    "General",
                    "UnlockMode",
                    ItemUnlockMode.Normal,
                    "When set to Unlockathon, all items will start out locked, and you must unlock items by finding them in game"
                ),
            };  
        }
    }
}
