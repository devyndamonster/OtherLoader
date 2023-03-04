using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Core.Services
{
    public interface IPathService
    {
        public string GetRootPath(string path);

        public string GetParentPath(string path);

        public string GetEndOfPath(string path);

        public IEnumerable<string> GetParentPaths(string path);

        public bool HasParent(string path);

        public bool IsParentOf(string parentPath, string path);

        public bool IsImmediateParentOf(string parentPath, string path);
    }
}
