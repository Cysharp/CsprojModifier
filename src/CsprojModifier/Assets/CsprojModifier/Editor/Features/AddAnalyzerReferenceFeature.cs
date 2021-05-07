using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using CsprojModifier.Editor.Internal;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CsprojModifier.Editor.Features
{
    public class AddAnalyzerReferenceFeature : CsprojModifierFeatureBase
    {
        private ReorderableList _reorderableListAdditionalAddAnalyzerProjects;

        public override void Initialize()
        {
            var settings = CsprojModifierSettings.Instance;
            _reorderableListAdditionalAddAnalyzerProjects = new ReorderableList(settings.AddAnalyzerReferencesAdditionalProjects, typeof(string), draggable: true, displayHeader: false, displayAddButton: true, displayRemoveButton: true)
            {
                drawNoneElementCallback = rect => EditorGUI.LabelField(rect, "Assembly-CSharp.csproj and Assembly-CSharp-Editor.csproj are always targeted."),
                drawElementCallback = ((rect, index, active, focused) =>
                {
                    var selectedItem = settings.AddAnalyzerReferencesAdditionalProjects[index];

                    rect.height -= 4;
                    rect.y += 2;

                    const int buttonBrowseWidth = 32;
                    const int controlGap = 4;

                    rect.width -= controlGap + buttonBrowseWidth;
                    selectedItem = EditorGUI.TextField(rect, selectedItem);

                    rect.x += rect.width + controlGap;
                    rect.width = buttonBrowseWidth;
                    if (GUI.Button(rect, "..."))
                    {
                        var selectedFilePath = EditorUtility.OpenFilePanelWithFilters(
                            "Add Additional Project",
                            Path.GetDirectoryName(Application.dataPath),
                            new[] { "C# Project File (*.csproj)", "csproj", "All files", "*" }
                        );
                        if (!string.IsNullOrWhiteSpace(selectedFilePath))
                        {
                            selectedItem = PathEx.MakeRelative(Application.dataPath, selectedFilePath);
                        }
                    }

                    settings.AddAnalyzerReferencesAdditionalProjects[index] = selectedItem;
                }),
                onChangedCallback = (list) => settings.Save(),
            };
        }

        public override string OnGeneratedCSProject(string path, string content)
        {
            var settings = CsprojModifierSettings.Instance;
            if (!settings.EnableAddAnalyzerReferences) return content;

            var canApply = path.EndsWith("Assembly-CSharp.csproj") ||
                           path.EndsWith("Assembly-CSharp-Editor.csproj") ||
                           (settings.AddAnalyzerReferencesAdditionalProjects?.Any(x => PathEx.Equals(PathEx.GetFullPath(x), path) || x == "*") ?? false);

            if (canApply)
            {
                var analyzers = GetAnalyzers();
                if (analyzers.Any())
                {
                    var xDoc = XDocument.Parse(content);
                    var nsMsbuild = (XNamespace)"http://schemas.microsoft.com/developer/msbuild/2003";
                    var projectE = xDoc.Element(nsMsbuild + "Project");

                    var baseDir = Path.GetDirectoryName(path);
                    var analyzersInCsproj = new HashSet<string>(projectE.Descendants(nsMsbuild + "Analyzer").Select(x => x.Attribute("Include")?.Value).Where(x => x != null));
                    projectE.Add(new XElement(nsMsbuild + "ItemGroup", analyzers.Where(x => !analyzersInCsproj.Contains(x)).Select(x => new XElement(nsMsbuild + "Analyzer", new XAttribute("Include", x)))));
                    content = xDoc.ToString();
                }

                return content;
            }

            return content;
        }

        private static IReadOnlyList<string> GetAnalyzers()
            => AssetDatabase.FindAssets("l:RoslynAnalyzer").Select(x => AssetDatabase.GUIDToAssetPath(x)).ToArray();

        public override void OnGUI()
        {
            EditorGUILayout.LabelField("Analyzer", EditorStyles.boldLabel);
            DrawAnalyzerReferences();
        }

        private void DrawAnalyzerReferences()
        {
            var settings = CsprojModifierSettings.Instance;
            settings.EnableAddAnalyzerReferences = EditorGUILayout.ToggleLeft("Add Analyzer references to .csproj", settings.EnableAddAnalyzerReferences);
            if (settings.EnableAddAnalyzerReferences)
            {
                using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                {
                    var analyzers = GetAnalyzers();
                    foreach (var analyzer in analyzers)
                    {
                        EditorGUILayout.LabelField(analyzer, EditorStyles.label);
                    }
                }
                EditorGUILayout.HelpBox("Analyzer must be tagged as 'RoslynAnalyzer'", MessageType.Info);

                EditorGUILayout.LabelField("The project to be added for analyzer references.");
                _reorderableListAdditionalAddAnalyzerProjects.DoLayoutList();
            }
        }

    }
}
