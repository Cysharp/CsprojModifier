using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CsprojModifier.Editor
{
    public class CsprojModifierSettingsProvider : SettingsProvider
    {
        private static class Styles
        {
            public static readonly GUIStyle VerticalStyle;

            static Styles()
            {
                VerticalStyle = new GUIStyle(EditorStyles.inspectorFullWidthMargins);
                VerticalStyle.margin = new RectOffset(10, 10, 10, 10);
            }
        }
        public CsprojModifierSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }

        [SettingsProvider]
        public static SettingsProvider Create()
            => new CsprojModifierSettingsProvider("Project/Editor/C# Project Modifier", SettingsScope.Project, new []{ "Analyzer", "C#", "csproj", "Project", "Import" } /* TODO */);

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            Initialize();
            base.OnActivate(searchContext, rootElement);
        }

        public override void OnGUI(string searchContext)
        {
            using (new EditorGUILayout.VerticalScope(Styles.VerticalStyle))
            {
                foreach (var feature in CsprojModifierFeatureProvider.Features)
                {
                    feature.OnGUI();
                    GUILayout.Space(10);
                }
            }

            if (GUI.changed)
            {
                CsprojModifierSettings.Instance.Save();
            }
        }

        private void Initialize()
        {
            foreach (var feature in CsprojModifierFeatureProvider.Features)
            {
                feature.Initialize();
            }
        }
    }
}
