using Moq;
using NUnit.Framework;
using OtherLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Tests
{
    [TestFixture]
    public class ItemSpawnerEntryTests
    {
        [Test]
        public void IsEntryCategory()
        {
            ItemSpawnerEntry entry = new ItemSpawnerEntry();

            entry.MainObjectID = "";

            Assert.That(entry.IsCategoryEntry());
        }

        [Test]
        public void IsEntryNotCategory()
        {
            ItemSpawnerEntry entry = new ItemSpawnerEntry();

            entry.MainObjectID = "AnItemId";

            Assert.That(!entry.IsCategoryEntry());
        }

    }
}
