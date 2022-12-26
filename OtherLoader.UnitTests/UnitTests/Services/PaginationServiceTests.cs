using FluentAssertions;
using NUnit.Framework;
using OtherLoader.Core.Services;

namespace OtherLoader.UnitTests.Services
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

        [TestCase(20, 0, 0, false)]
        [TestCase(20, 1, 0, false)]
        [TestCase(20, 20, 0, false)]
        [TestCase(20, 21, 0, true)]
        [TestCase(20, 21, 1, false)]
        public void ItWillHaveNextPageInCorrectScenarios(int pageSize, int itemCount, int currentPage, bool expected)
        {
            var pageService = new PaginationService();

            var result = pageService.HasNextPage(pageSize, itemCount, currentPage);

            result.Should().Be(expected);
        }

        [TestCase(0, false)]
        [TestCase(1, true)]
        [TestCase(123, true)]
        public void ItWillHavePrevPageInCorrectScenarios(int currentPage, bool expected)
        {
            var pageService = new PaginationService();

            var result = pageService.HasPrevPage(currentPage);

            result.Should().Be(expected);
        }
    }
}
