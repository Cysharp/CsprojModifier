using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsprojModifier.Editor.Features
{
    public interface ICsprojModifierFeature
    {
        void Initialize();
        void OnGUI();
    }

    public interface ICsprojModifierGeneratedFileProcessor
    {
        bool OnPreGeneratingCSProjectFiles();
        string OnGeneratedCSProject(string path, string content);
        void OnGeneratedCSProjectFiles();
        string OnGeneratedSlnSolution(string path, string content);
    }

    public abstract class CsprojModifierFeatureBase : ICsprojModifierFeature, ICsprojModifierGeneratedFileProcessor
    {
        public abstract void Initialize();

        public abstract void OnGUI();

        public virtual bool OnPreGeneratingCSProjectFiles()
            => false;

        public virtual string OnGeneratedCSProject(string path, string content)
            => content;

        public virtual void OnGeneratedCSProjectFiles()
        {
        }

        public virtual string OnGeneratedSlnSolution(string path, string content)
            => content;
    }
}
