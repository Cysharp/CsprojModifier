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

#if UNITY_2018_1_OR_NEWER && HAS_ROSLYN_ANALZYER_SUPPORT_RIDER_3_0_2_OR_NEWER
#else
        private static void OnGeneratedCSProjectFiles()
        {
            foreach (var feature in _features.OfType<ICsprojModifierGeneratedFileProcessor>())
            {
                feature.OnGeneratedCSProjectFiles();
            }
        }
#endif

        private static string OnGeneratedSlnSolution(string path, string content)
            => _features.OfType<ICsprojModifierGeneratedFileProcessor>().Aggregate(content, (r, x) => x.OnGeneratedSlnSolution(path, r));

    }
}