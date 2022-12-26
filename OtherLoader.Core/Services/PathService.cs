using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Core.Services
{
    public class PathService : IPathService
    {
        public IEnumerable<string> GetParentPaths(string path)
        {
            if (!HasParent(path)) return new string[0];

            var parentPath = GetParentPath(path);
            var parentPaths = GetParentPaths(parentPath);
            
            return parentPaths.Union(new[] { parentPath });
        }

        public string GetEndOfPath(string path)
        {
            return path.Split('/').Last();
        }

        public string GetParentPath(string path)
        {
            if (!path.Contains("/")) return string.Empty;

            return path.Substring(0, path.LastIndexOf('/'));
        }

        public string GetRootPath(string path)
        {
            return path.Split('/').First();
        }

        public bool HasParent(string path)
        {
            return path.Contains('/');
        }
        
        public bool IsParentOf(string parentPath, string path)
        {
            if (!HasParent(path)) return false;

            var parent = GetParentPath(path);
            
            return 
                (parent == parentPath) ||
                (HasParent(parent) && IsParentOf(parentPath, parent));
        }

        public bool IsImmediateParentOf(string parentPath, string path)
        {
            return HasParent(path) && GetParentPath(path) == parentPath;
        }
    }
}
