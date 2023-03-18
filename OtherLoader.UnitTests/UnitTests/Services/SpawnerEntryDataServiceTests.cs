using FluentAssertions;
using NUnit.Framework;
using OtherLoader.Core.Features.MetaData.Models.Vanilla;
using OtherLoader.Core.Features.MetaData.Services;
using System.Collections.Generic;
using System.Linq;

namespace OtherLoader.UnitTests.Services
{
    public class SpawnerEntryDataServiceTests
    {
        [TestFixture]
        public class ConvertToSpawnerEntryData
        {
            [Test, TestCaseSource(nameof(CombinedPathTestData))]
            public void ItWillCreateCorrectPath(string mainObjectId, ObjectCategory mainObjectCategory, ItemCategory category, SubCategory subCategory, string expectedPath)
            {
                var spawnerId = new ItemSpawnerId
                {
                    ItemId = "ItemId",
                    MainObjectId = mainObjectId,
                    Category = category,
                    SubCategory = subCategory
                };

                var mainObject = new FVRObject
                {
                    ObjectId = mainObjectId,
                    ObjectCategory = mainObjectCategory
                };

                var service = new SpawnerEntryDataService();

                var spawnerEntry = service.ConvertToSpawnerEntryData(spawnerId, mainObject);

                spawnerEntry.Path.Should().Be(expectedPath);
            }

            private static IEnumerable<TestCaseData> CombinedPathTestData()
            {
                return FirearmsPageTestData()
                    .Concat(AmmoPageTestData());
            }

            private static IEnumerable<TestCaseData> FirearmsPageTestData()
            {
                yield return new TestCaseData("TestItem", ObjectCategory.Firearm, ItemCategory.Pistol, SubCategory.MachinePistol, "Firearms/MachinePistol/TestItem");
                yield return new TestCaseData("TestItem", ObjectCategory.Uncategorized, ItemCategory.Pistol, SubCategory.MachinePistol, "Firearms/MachinePistol/TestItem");
                yield return new TestCaseData("TestItem", ObjectCategory.Firearm, ItemCategory.MeatFortress, SubCategory.MF_Medic, "Firearms/MeatFortress/MF_Medic/TestItem");
                yield return new TestCaseData("CustomItem", ObjectCategory.Firearm, (ItemCategory)1234, (SubCategory)5678, "Firearms/ModdedCategory_1234/ModdedSubcategory_5678/CustomItem");
                yield return new TestCaseData("CustomItem", ObjectCategory.Uncategorized, (ItemCategory)1234, (SubCategory)5678, "Firearms/ModdedCategory_1234/ModdedSubcategory_5678/CustomItem");
            }

            private static IEnumerable<TestCaseData> AmmoPageTestData()
            {
                yield return new TestCaseData("TestItem", ObjectCategory.Uncategorized, ItemCategory.Clip, SubCategory.None, "Ammo/Clip/TestItem");
                yield return new TestCaseData("TestItem", ObjectCategory.Uncategorized, ItemCategory.Speedloader, SubCategory.None, "Ammo/Speedloader/TestItem");
                yield return new TestCaseData("TestItem", ObjectCategory.Uncategorized, ItemCategory.Cartridge, SubCategory.None, "Ammo/Cartridge/TestItem");
                yield return new TestCaseData("TestItem", ObjectCategory.Uncategorized, ItemCategory.Magazine, SubCategory.None, "Ammo/Magazine/TestItem");
            }
        }

        
    }
}
