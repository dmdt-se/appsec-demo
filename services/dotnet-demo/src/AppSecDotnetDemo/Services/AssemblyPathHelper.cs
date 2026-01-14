using System.Collections.Generic;
using System.IO;

namespace AppSecDotnetDemo.Services;

public static class AssemblyPathHelper
{
    public static string Resolve(string contentRootPath, string? configuredPath)
    {
        var candidates = new List<string>();

        if (!string.IsNullOrWhiteSpace(configuredPath))
        {
            candidates.Add(Path.IsPathRooted(configuredPath)
                ? configuredPath
                : Path.GetFullPath(Path.Combine(contentRootPath, configuredPath)));
        }

        candidates.Add(Path.Combine(contentRootPath, "assemblies"));
        candidates.Add(Path.GetFullPath(Path.Combine(contentRootPath, "..", "..", "assemblies")));

        foreach (var candidate in candidates)
        {
            if (Directory.Exists(candidate))
            {
                return candidate;
            }
        }

        return candidates[0];
    }
}
