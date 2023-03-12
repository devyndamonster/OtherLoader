using FluentAssertions;
using NUnit.Framework;
using OtherLoader.Core.Features.MetaData.Models.Vanilla;
using OtherLoader.Core.Features.MetaData.Services;

namespace OtherLoader.UnitTests.Services
{
    public class SpawnerEntryDataServiceTests
    {
        [TestFixture]
        public class ConvertToSpawnerEntryData
        {
            [TestCase("G18", ItemCategory.Pistol, SubCategory.MachinePistol, "Firearms/MachinePistol/G18")]
            [TestCase("MF_Medical180", ItemCategory.MeatFortress, SubCategory.MF_Medic, "Firearms/MeatFortress/MF_Medic/MF_Medical180")]
            [TestCase("38TroundClip", ItemCategory.Clip, SubCategory.None, "Ammo/Clip/38TroundClip")]
            [TestCase("SpeedloaderR8", ItemCategory.Speedloader, SubCategory.None, "Ammo/Speedloader/SpeedloaderR8")]
            [TestCase("10x27mmPulsed", ItemCategory.Cartridge, SubCategory.None, "Ammo/Cartridge/10x27mmPulsed")]
            [TestCase("MagazineFiveSeven30rnd", ItemCategory.Magazine, SubCategory.None, "Ammo/Magazine/MagazineFiveSeven30rnd")]
            [TestCase("CustomCategoryItem", (ItemCategory)1234, (SubCategory)5678, "Firearms/ModdedCategory_1234/ModdedSubcategory_5678/CustomCategoryItem")]
            public void ItWillCreateCorrectPath(string mainObjectId, ItemCategory category, SubCategory subCategory, string expectedPath)
            {
                var spawnerId = new ItemSpawnerId
                {
                    ItemId = "ItemId",
                    MainObjectId = mainObjectId,
                    Category = category,
                    SubCategory = subCategory
                };

                var service = new SpawnerEntryDataService();

                var spawnerEntry = service.ConvertToSpawnerEntryData(spawnerId);

                spawnerEntry.Path.Should().Be(expectedPath);
            }

        }
    }
}
