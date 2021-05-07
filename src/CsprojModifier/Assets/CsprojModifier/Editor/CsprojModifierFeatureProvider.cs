using System.Collections.Generic;
using System.Linq;
using CsprojModifier.Editor.Features;
using UnityEditor;

namespace CsprojModifier.Editor
{
    public class CsprojModifierFeatureProvider : AssetPostprocessor
    {
        private static readonly List<ICsprojModifierFeature> _features = new List<ICsprojModifierFeature>()
        {
            new InsertAdditionalImportFeature(),
            new AddAnalyzerReferenceFeature(),
            new RegenerateProjectFeature(),
        };

        public static List<ICsprojModifierFeature> Features => _features;

        private static bool OnPreGeneratingCSProjectFiles()
            => _features.OfType<ICsprojModifierGeneratedFileProcessor>().Aggregate(false, (r, x) => x.OnPreGeneratingCSProjectFiles() || r);
        private static string OnGeneratedCSProject(string path, string content)
            => _features.OfType<ICsprojModifierGeneratedFileProcessor>().Aggregate(content, (r, x) => x.OnGeneratedCSProject(path, r));

        private static void OnGeneratedCSProjectFiles()
        {
            foreach (var feature in _features.OfType<ICsprojModifierGeneratedFileProcessor>())
            {
                feature.OnGeneratedCSProjectFiles();
            }
        }

        private static string OnGeneratedSlnSolution(string path, string content)
            => _features.OfType<ICsprojModifierGeneratedFileProcessor>().Aggregate(content, (r, x) => x.OnGeneratedSlnSolution(path, r));

    }
}