using FluentAssertions;
using NUnit.Framework;
using OtherLoader.Core.Models;
using OtherLoader.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.UnitTests.Models
{
    [TestFixture]
    public class ResultCoroutineTests
    {
        [Test]
        public void WillReturnResult()
        {
            var expectedResult = new object[]
            {
                "Test Result"
            };

            var coroutine = new ResultCoroutine<object[]>(TestCoroutine());

            coroutine.ExecuteCoroutine();

            coroutine.Result.ShouldBeEquivalentTo(expectedResult);
        }

        
        [Test]
        public void ItWillRunSubroutines()
        {
            Assert.True(false);
        }
        

        private IEnumerator TestCoroutine()
        {
            yield return null;

            yield return new object[]
            {
                "Test Result"
            };
        }
    }
}
