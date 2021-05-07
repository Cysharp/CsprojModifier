using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CsprojModifier.Editor.Internal
{
    internal static class PathEx
    {
        public static string GetFullPath(string path)
        {
            if (Path.IsPathRooted(path)) return path;
            return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Application.dataPath), path));
        }

        public static bool Equals(string pathA, string pathB)
        {
            return string.Equals(pathA.Replace("\\", "/"), pathB.Replace("\\", "/"), StringComparison.OrdinalIgnoreCase);
        }

        public static string MakeRelative(string basePath, string targetPath)
        {
            var basePathParts = basePath.Split('\\', '/');
            var targetPathParts = targetPath.Split('\\', '/');

            var targetPathFixed = targetPath;
            for (var i = 0; i < Math.Min(basePathParts.Length, targetPathParts.Length); i++)
            {
                var basePathPrefix = string.Join("/", basePathParts.Take(i + 1));
                var targetPathPrefix = string.Join("/", targetPathParts.Take(i + 1));

                if (basePathPrefix == targetPathPrefix)
                {
                    var pathPrefix = basePathPrefix;
                    var upperDirCount = (basePathParts.Length - i - 2); // excepts a filename

                    var sb = new StringBuilder();
                    for (var j = 0; j < upperDirCount; j++)
                    {
                        sb.Append("..");
                        sb.Append('/');
                    }
                    sb.Append(targetPath.Substring(pathPrefix.Length + 1));

                    targetPathFixed = sb.ToString();
                }
                else
                {
                    break;
                }
            }

            return targetPathFixed;
        }
    }
}
