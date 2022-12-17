using Moq;
using NUnit.Framework;
using OtherLoader.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.UnitTests
{
    
    public class Tests
    {
        [Test]
        public void TestTest()
        {
            Assert.IsTrue(true);
        }

        [Test]
        public void TestTestA()
        {
            var test = new Class1();
            Assert.IsTrue(test.GetTestStuff());
        }

    }
}
