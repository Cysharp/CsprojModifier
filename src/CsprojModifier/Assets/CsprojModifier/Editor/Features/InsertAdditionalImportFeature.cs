using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Linq;
using CsprojModifier.Editor.Internal;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CsprojModifier.Editor.Features
{
    public class InsertAdditionalImportFeature : CsprojModifierFeatureBase
    {
        private ReorderableList _reorderableListAdditionalImports;
        private ReorderableList _reorderableListAdditionalImportsAdditionalProjects;

        public override void Initialize()
        {
            var settings = CsprojModifierSettings.Instance;
            _reorderableListAdditionalImports = new ReorderableList(settings.AdditionalImports, typeof(ImportProjectItem), draggable: true, displayHeader: false, displayAddButton: true, displayRemoveButton: true)
            {
                drawElementCallback = ((rect, index, active, focused) =>
                {
                    var selectedItem = settings.AdditionalImports[index];

                    rect.height -= 4;
                    rect.y += 2;

                    const int buttonBrowseWidth = 32;
                    const int buttonPositionWidth = 96;
                    const int controlGap = 4;

                    rect.width -= controlGap + buttonBrowseWidth + controlGap + buttonPositionWidth;
                    selectedItem.Path = EditorGUI.TextField(rect, selectedItem.Path);

                    rect.x += rect.width + controlGap;
                    rect.width = buttonBrowseWidth;
                    if (GUI.Button(rect, "..."))
                    {
                        var selectedFilePath = EditorUtility.OpenFilePanelWithFilters(
                            "Add Additional Project",
                            Path.GetDirectoryName(Application.dataPath),
                            new[] { "MSBuild Project File (*.props;*.target)", "props,target", "All files", "*" }
                        );
                        if (!string.IsNullOrWhiteSpace(selectedFilePath))
                        {
                            selectedItem.Path = PathEx.MakeRelative(Application.dataPath, selectedFilePath);
                        }
                    }

                    rect.x += rect.width + controlGap;
                    rect.width = buttonPositionWidth;
                    selectedItem.Position = (ImportProjectPosition)EditorGUI.EnumPopup(rect, selectedItem.Position);
                }),
                onChangedCallback = (list) => settings.Save(),
            };


            _reorderableListAdditionalImportsAdditionalProjects = new ReorderableList(settings.AdditionalImportsAdditionalProjects, typeof(string), draggable: true, displayHeader: false, displayAddButton: true, displayRemoveButton: true)
            {
                drawNoneElementCallback = rect => EditorGUI.LabelField(rect, "Assembly-CSharp.csproj and Assembly-CSharp-Editor.csproj are always targeted."),
                drawElementCallback = ((rect, index, active, focused) =>
                {
                    var selectedItem = settings.AdditionalImportsAdditionalProjects[index];

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

                    settings.AdditionalImportsAdditionalProjects[index] = selectedItem;
                }),
                onChangedCallback = (list) => settings.Save(),
            };
        }

        public override void OnGUI()
        {
            EditorGUILayout.LabelField("Additional project imports", EditorStyles.boldLabel);
            _reorderableListAdditionalImports.DoLayoutList();
            EditorGUILayout.LabelField("The project to be added for Import.");
            _reorderableListAdditionalImportsAdditionalProjects.DoLayoutList();
        }

        public override string OnGeneratedCSProject(string path, string content)
        {
            var settings = CsprojModifierSettings.Instance;
            var canApply = path.EndsWith("Assembly-CSharp.csproj") ||
                           path.EndsWith("Assembly-CSharp-Editor.csproj") ||
                           settings.AdditionalImportsAdditionalProjects.Any(x => PathEx.Equals(PathEx.GetFullPath(x), path) || x == "*");


            if (settings.AdditionalImports.Any() && canApply)
            {

                var baseDir = Path.GetDirectoryName(path);
                var xDoc = XDocument.Parse(content);
                var nsMsbuild = (XNamespace)"http://schemas.microsoft.com/developer/msbuild/2003";
                var projectE = xDoc.Element(nsMsbuild + "Project");

                foreach (var target in settings.AdditionalImports)
                {
                    var hash = string.Concat(SHA256.Create().ComputeHash(File.ReadAllBytes(Path.GetFullPath(Path.Combine(baseDir, target.Path)))).Select(x => x.ToString("x2")));

                    if (target.Position == ImportProjectPosition.Append)
                    {
                        projectE.Add(new XComment($"{target.Path}:{hash}"));
                        projectE.Add(new XElement(nsMsbuild + "Import", new XAttribute("Project", target.Path)));
                    }
                    else if (target.Position == ImportProjectPosition.Prepend)
                    {
                        projectE.AddFirst(new XElement(nsMsbuild + "Import", new XAttribute("Project", target.Path)));
                        projectE.AddFirst(new XComment($"{target.Path}:{hash}"));
                    }
                }

                content = xDoc.ToString();
                return content;
            }

            return content;
        }

    }
}
