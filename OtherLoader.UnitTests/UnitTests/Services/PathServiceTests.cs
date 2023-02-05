using FluentAssertions;
using NUnit.Framework;
using OtherLoader.Core.Services;
using System.Collections;

namespace OtherLoader.UnitTests.Services
{
    [TestFixture]
    [Category("PathService")]
    public class PathServiceTests
    {
        [TestCase("", "")]
        [TestCase("Firearms", "Firearms")]
        [TestCase("Firearms/Pistols", "Firearms")]
        [TestCase("Firearms/", "Firearms")]
        [TestCase("Firearms/ /Pistols", "Firearms")]
        [TestCase("Firearms///////", "Firearms")]
        public void ItWillGetCorrectRoot(string path, string expected)
        {
            var pathService = new PathService();

            var result = pathService.GetRootPath(path);

            result.Should().Be(expected);
        }

        [TestCase("", "")]
        [TestCase("Firearms", "")]
        [TestCase("Firearms/", "Firearms")]
        [TestCase("Firearms/Pistols", "Firearms")]
        [TestCase("Firearms/Pistols/Automatic", "Firearms/Pistols")]
        public void ItWillGetCorrectParentPath(string path, string expected)
        {
            var pathService = new PathService();

            var result = pathService.GetParentPath(path);

            result.Should().Be(expected);
        }

        [TestCase("", "")]
        [TestCase("Firearms", "Firearms")]
        [TestCase("Firearms/", "")]
        [TestCase("Firearms/Pistols", "Pistols")]
        public void ItWillGetCorrectEndOfPath(string path, string expected)
        {
            var pathService = new PathService();

            var result = pathService.GetEndOfPath(path);

            result.Should().Be(expected);
        }
        
        [TestCase("", false)]
        [TestCase("Firearms", false)]
        [TestCase("Firearms/", true)]
        [TestCase("Firearms/Pistols", true)]
        public void ItWillHaveParentForCorrectPaths(string path, bool expected)
        {
            var pathService = new PathService();

            var result = pathService.HasParent(path);

            result.Should().Be(expected);
        }
        
        [TestCase("Firearms", "", false)]
        [TestCase("Firearms", "Firearms", false)]
        [TestCase("Firearms", "Fire", false)]
        [TestCase("Firearms/", "Firearms", true)]
        [TestCase("Firearms/Pistols", "Firearms", true)]
        [TestCase("Firearms/Pistols/Revolvers", "Firearms", true)]
        [TestCase("Firearms/Pistols/Revolvers", "Pistols", false)]
        [TestCase("Firearms/Pistols/Revolvers", "Firearms/Pis", false)]
        [TestCase("Firearms/Pistols/Revolvers", "Firearms/Pistols", true)]
        public void ItWillBeParentOfCorrectPaths(string path, string parent, bool expected)
        {
            var pathService = new PathService();

            var result = pathService.IsParentOf(parent, path);
    
            result.Should().Be(expected);
        }

        [TestCase("Firearms", "", false)]
        [TestCase("Firearms", "Firearms", false)]
        [TestCase("Firearms", "Fire", false)]
        [TestCase("Firearms/", "Firearms", true)]
        [TestCase("Firearms/Pistols", "Firearms", true)]
        [TestCase("Firearms/Pistols/Revolvers", "Firearms", false)]
        [TestCase("Firearms/Pistols/Revolvers", "Pistols", false)]
        [TestCase("Firearms/Pistols/Revolvers", "Firearms/Pis", false)]
        [TestCase("Firearms/Pistols/Revolvers", "Firearms/Pistols", true)]
        public void ItWillBeImmediateParentOfCorrectPaths(string path, string parent, bool expected)
        {
            var pathService = new PathService();

            var result = pathService.IsImmediateParentOf(parent, path);

            result.Should().Be(expected);
        }

        [TestCase("Firearms", new string[] { })]
        [TestCase("Firearms/", new string[] { "Firearms" })]
        [TestCase("Firearms/Pistols/Revolvers", new string[] { "Firearms", "Firearms/Pistols" })]
        [TestCase("Firearms//Pistols", new string[] { "Firearms", "Firearms/" })]
        [TestCase("Firearms/Pistols/Pistols", new string[] { "Firearms", "Firearms/Pistols" })]
        [TestCase("Firearms/Pistols/Firearms/Pistols", new string[] { "Firearms", "Firearms/Pistols", "Firearms/Pistols/Firearms" })]
        public void ItWillGetCorrectParentPaths(string path, IEnumerable expectedParents)
        {
            var pathService = new PathService();
            
            var result = pathService.GetParentPaths(path);
            
            result.Should().ContainInOrder(expectedParents);
        }

    }
}
