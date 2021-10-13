using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CsprojModifier.Editor
{
    public class CsprojModifierSettings : ScriptableObject
    {
        private const string SettingsPath = "ProjectSettings/CsprojModifierSettings.json";

        private static CsprojModifierSettings _instance;
        public static CsprojModifierSettings Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                _instance = LoadOrNew();

                return _instance;
            }
        }

        #region Settings
        public List<ImportProjectItem> AdditionalImports;
        public List<string> AdditionalImportsAdditionalProjects;

        public bool EnableAddAnalyzerReferences;
        public List<string> AddAnalyzerReferencesAdditionalProjects;
        #endregion


        public CsprojModifierSettings()
        {
            AdditionalImports = new List<ImportProjectItem>();
            AdditionalImportsAdditionalProjects = new List<string>();
            AddAnalyzerReferencesAdditionalProjects = new List<string>();
        }

        private static CsprojModifierSettings LoadOrNew()
        {
            if (File.Exists(SettingsPath))
            {
                var instance = CreateInstance<CsprojModifierSettings>();
                JsonUtility.FromJsonOverwrite(File.ReadAllText(SettingsPath), instance);
                return instance;
            }
            else
            {
                var instance = CreateInstance<CsprojModifierSettings>();
                return instance;
            }
        }

        public void Save()
        {
            File.WriteAllText(SettingsPath, JsonUtility.ToJson(_instance));
        }
    }

    public enum ImportProjectPosition
    {
        Append,
        Prepend,
        AppendContent,
        PrependContent,
    }

    [Serializable]
    public class ImportProjectItem
    {
        public string Path;
        public ImportProjectPosition Position;
    }
}