using Moq;
using NUnit.Framework;
using OtherLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OtherLoader.Tests
{
    [TestFixture]
    public class ItemSpawnerEntryTests
    {
        private ItemSpawnerEntrySampleBuilder _sampleBuilder = new ItemSpawnerEntrySampleBuilder();

        [Test]
        public void EntryIsACategoryEntry()
        {
            _sampleBuilder
                .Reset()
                .SetIsDisplayed(true)
                .SetDisplayName("Special Magazines");

            _sampleBuilder.SetCategory("Ammo/Magazines/SpecialMagazines/");
            Assert.That(_sampleBuilder.GetEntry().IsCategoryEntry());

            _sampleBuilder.SetCategory("Ammo/Magazines/SpecialMagazines");
            Assert.That(_sampleBuilder.GetEntry().IsCategoryEntry());
        }

        [Test]
        public void EntryIsNotACategoryEntry()
        {
            _sampleBuilder
                .Reset()
                .SetItem("Ammo/Magazines/SpecialMagazines/AnItem", "AnItem")
                .SetIsDisplayed(true)
                .SetSecondaryItems("FirstItem", "SecondItem")
                .SetSpawnWithItems("FirstItem");

            Assert.That(!_sampleBuilder.GetEntry().IsCategoryEntry());
        }




    }
}
