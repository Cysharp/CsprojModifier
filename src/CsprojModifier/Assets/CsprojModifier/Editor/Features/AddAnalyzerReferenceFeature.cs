using System;
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
        private IReadOnlyList<string> _analyzers = GetAnalyzers();

        public override void Initialize()
        {
            var settings = CsprojModifierSettings.Instance;
            // WORKAROUND: https://issuetracker.unity3d.com/issues/missingmethodexception-when-adding-elements-to-reorderablelist-with-string-type
            var addAnalyzerReferencesAdditionalProjects = new List<ValueTuple<string>>(settings.AddAnalyzerReferencesAdditionalProjects.Select(x => ValueTuple.Create(x)));
            _reorderableListAdditionalAddAnalyzerProjects = new ReorderableList(addAnalyzerReferencesAdditionalProjects, typeof(ValueTuple<string>), draggable: true, displayHeader: false, displayAddButton: true, displayRemoveButton: true)
            {
                drawNoneElementCallback = rect => EditorGUI.LabelField(rect, "Assembly-CSharp.csproj and Assembly-CSharp-Editor.csproj are always targeted."),
                drawElementCallback = ((rect, index, active, focused) =>
                {
                    using (var editScope = new EditorGUI.ChangeCheckScope())
                    {
                        var selectedItem = addAnalyzerReferencesAdditionalProjects[index];

                        rect.height -= 4;
                        rect.y += 2;

                        const int buttonBrowseWidth = 32;
                        const int controlGap = 4;

                        rect.width -= controlGap + buttonBrowseWidth;
                        selectedItem = ValueTuple.Create(EditorGUI.TextField(rect, selectedItem.Item1));

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
                                selectedItem = ValueTuple.Create(PathEx.MakeRelative(Application.dataPath, selectedFilePath));
                                GUI.changed = true;
                            }
                        }

                        if (editScope.changed)
                        {
                            addAnalyzerReferencesAdditionalProjects[index] = selectedItem;
                            settings.AddAnalyzerReferencesAdditionalProjects = addAnalyzerReferencesAdditionalProjects.Select(x => x.Item1).ToList();
                            settings.Save();
                        }
                    }

                }),
                onChangedCallback = (list) =>
                {
                    settings.AddAnalyzerReferencesAdditionalProjects = addAnalyzerReferencesAdditionalProjects.Select(x => x.Item1).ToList();
                    settings.Save();
                },
            };
        }

        public override string OnGeneratedCSProject(string path, string content)
        {
            var settings = CsprojModifierSettings.Instance;
            if (RoslynAnalyzerUnityEditorNativeSupport.HasRoslynAnalyzerIdeSupport) return content;
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
            EditorGUILayout.LabelField("Roslyn Analyzer", EditorStyles.boldLabel);
            DrawAnalyzerReferences();
        }

        private void DrawAnalyzerReferences()
        {
            var settings = CsprojModifierSettings.Instance;

            if (RoslynAnalyzerUnityEditorNativeSupport.HasRoslynAnalyzerIdeSupport)
            {
                EditorGUILayout.HelpBox("The current code editor has Roslyn Analyzer IDE support. Roslyn Analyzers are enabled by Unity Editor.", MessageType.Info);
                return;
            }

            settings.EnableAddAnalyzerReferences = EditorGUILayout.ToggleLeft("Add Roslyn Analyzer references to .csproj", settings.EnableAddAnalyzerReferences);
            if (settings.EnableAddAnalyzerReferences)
            {
                using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                {
                    foreach (var analyzer in _analyzers)
                    {
                        EditorGUILayout.LabelField(analyzer, EditorStyles.label);
                    }
                }
                EditorGUILayout.HelpBox("Analyzer must be labeled as 'RoslynAnalyzer'", MessageType.Info);

                if (GUILayout.Button("Rescan Roslyn Analyzers"))
                {
                    _analyzers = GetAnalyzers();
                }
                EditorGUILayout.LabelField("The project to be added for Roslyn Analyzer references.");
                _reorderableListAdditionalAddAnalyzerProjects.DoLayoutList();
            }
        }


        private static class RoslynAnalyzerUnityEditorNativeSupport
        {
            public static bool HasRoslynAnalyzerIdeSupport
            {
                get
                {

#if UNITY_2020_2_OR_NEWER && (HAS_ROSLYN_ANALZYER_SUPPORT_RIDER || HAS_ROSLYN_ANALZYER_SUPPORT_VSCODE || HAS_ROSLYN_ANALZYER_SUPPORT_VS)
                    // The editor extension for 'Rider', 'Visual Studio Code' and 'Visual Studio' has the functionality to add Roslyn analyzer references.
                    var codeEditorType = Unity.CodeEditor.CodeEditor.CurrentEditor.GetType();
                    if (codeEditorType.Name == "VSCodeScriptEditor" || codeEditorType.Name == "RiderScriptEditor" || codeEditorType.Name == "VisualStudioEditor")
                    {
                        return true;
                    }
#endif
                    return false;
                }
            }
        }
    }
}
