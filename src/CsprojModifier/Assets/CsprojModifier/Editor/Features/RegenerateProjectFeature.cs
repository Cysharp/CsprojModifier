using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Unity.CodeEditor;
using UnityEngine;

namespace CsprojModifier.Editor.Features
{
    public class RegenerateProjectFeature : ICsprojModifierFeature
    {
        public void Initialize()
        {
        }

        public void OnGUI()
        {
            GUILayout.Space(10);

            if (GUILayout.Button("Regenerate project files"))
            {
                if (CodeEditor.CurrentEditor.GetType().Name == "DefaultExternalCodeEditor")
                {
                    // SyncVS.Synchronizer.Sync(); (SyncVS is an internal class, so call it with Reflection)

                    var syncVsType = Type.GetType("UnityEditor.SyncVS, UnityEditor");
                    ThrowIfNull(syncVsType, "Type 'UnityEditor.SyncVS' is not found on the editor.");

                    var slnSynchronizerType = Type.GetType("UnityEditor.VisualStudioIntegration.SolutionSynchronizer, UnityEditor");
                    ThrowIfNull(slnSynchronizerType, "Type 'UnityEditor.VisualStudioIntegration.SolutionSynchronizer' is not found on the editor.");

                    var solutionSynchronizerField = syncVsType.GetField("Synchronizer", BindingFlags.Static | BindingFlags.NonPublic);
                    ThrowIfNull(solutionSynchronizerField, "Field 'Synchronizer' is not found in 'SolutionSynchronizer'.");

                    var syncMethod = slnSynchronizerType.GetMethod("Sync", BindingFlags.Instance | BindingFlags.Public);
                    ThrowIfNull(syncMethod, "Method 'Sync' is not found in 'Synchronizer'.");

                    syncMethod.Invoke(solutionSynchronizerField.GetValue(null), Array.Empty<object>());
                }
                else
                {
                    // HACK: Make it look like a dummy file has been added.
                    CodeEditor.CurrentEditor.SyncIfNeeded(new [] { "RegenerateProjectFeature.cs" }, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());
                }
            }
        }

        private void ThrowIfNull(object value, string message)
        {
            if (value == null)
            {
                throw new Exception(message);
            }
        }
    }
}
