using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.UnitTests.Controllers
{
    public class LoadOrderControllerTests
    {
        [TestFixture]
        public class CanBundleBeginLoading
        {
            [Test]
            public void LoadFirstWontLoadWhileLoadFirstDependancyWaiting()
            {
                throw new NotImplementedException();
            }

            [Test]
            public void LoadFirstWontLoadWhileLoadFirstDependancyLoading()
            {
                throw new NotImplementedException();
            }

            [Test]
            public void LoadFirstWillLoadWhileLoadUnorderedDependancyWaiting()
            {
                throw new NotImplementedException();
            }

            //TODO more tests
        }
    }
}
