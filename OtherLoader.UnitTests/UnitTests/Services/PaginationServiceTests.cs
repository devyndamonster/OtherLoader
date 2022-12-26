using FluentAssertions;
using NUnit.Framework;
using OtherLoader.Core.Services;

namespace OtherLoader.UnitTests.UnitTests.Services
{
    public class PaginationServiceTests
    {
        [TestCase(20, 0, 1)]
        [TestCase(20, 1, 1)]
        [TestCase(20, 20, 1)]
        [TestCase(20, 21, 2)]
        [TestCase(20, 50, 3)]
        public void ItWillReturnCorrectNumberOfPages(int pageSize, int itemCount, int expected)
        {
            var pageService = new PaginationService();

            var result = pageService.GetNumberOfPages(pageSize, itemCount);

            result.Should().Be(expected);
        }
    }
}
