using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Services
{
    public class PathService : IPathService
    {
        public IEnumerable<string> GetParentPaths(string path)
        {
            var paths = path
                .Split('/')
                .Select(pathSegment =>
                    path.Substring(0, path.IndexOf(pathSegment) + pathSegment.Length));
                
            return paths.Take(paths.Count() - 1);
        }

        public string GetEndOfPath(string path)
        {
            return path.Split('/').Last();
        }

        public string GetParentPath(string path)
        {
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
            return HasParent(path) &&
                path.StartsWith(parentPath) &&
                path.Trim('/').Length > parentPath.Trim('/').Length;
        }

        public bool IsImmediateParentOf(string parentPath, string path)
        {
            return HasParent(path) &&
                GetParentPath(path).Trim('/') == parentPath.Trim('/');
        }
    }
}
