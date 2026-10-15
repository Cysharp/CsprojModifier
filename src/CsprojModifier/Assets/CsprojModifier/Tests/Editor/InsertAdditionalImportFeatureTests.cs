using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CsprojModifier.Editor.Features;
using NUnit.Framework;

namespace CsprojModifier.Editor.Tests
{
    public class InsertAdditionalImportFeatureTests
    {
        [TestCase("", "", ImportProjectPosition.AppendContent)]
        [TestCase("", "", ImportProjectPosition.PrependContent)]
        [TestCase("", "http://schemas.microsoft.com/developer/msbuild/2003", ImportProjectPosition.AppendContent)]
        [TestCase("", "http://schemas.microsoft.com/developer/msbuild/2003", ImportProjectPosition.PrependContent)]
        [TestCase("http://schemas.microsoft.com/developer/msbuild/2003", "", ImportProjectPosition.AppendContent)]
        [TestCase("http://schemas.microsoft.com/developer/msbuild/2003", "", ImportProjectPosition.PrependContent)]
        [TestCase("http://schemas.microsoft.com/developer/msbuild/2003", "http://schemas.microsoft.com/developer/msbuild/2003", ImportProjectPosition.AppendContent)]
        [TestCase("http://schemas.microsoft.com/developer/msbuild/2003", "http://schemas.microsoft.com/developer/msbuild/2003", ImportProjectPosition.PrependContent)]
        public void InjectContentUsesDestinationProjectNamespace(string projectNamespace, string sourceNamespace, ImportProjectPosition position)
        {
            var directory = Path.Combine(Path.GetTempPath(), "CsprojModifier-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            var settings = CsprojModifierSettings.Instance;
            var originalImports = settings.AdditionalImports;
            try
            {
                var propsPath = Path.Combine(directory, "Additional.props");
                File.WriteAllText(propsPath,
                    "<Project xmlns=\"" + sourceNamespace + "\">" +
                    "<PropertyGroup Condition=\"'$(Configuration)' == 'Debug'\"><Nullable>enable</Nullable></PropertyGroup>" +
                    "<ItemGroup><AdditionalFiles Include=\"BannedSymbols.txt\" /></ItemGroup>" +
                    "<ProjectExtensions><Payload xmlns=\"urn:extension\">kept</Payload></ProjectExtensions>" +
                    "</Project>");
                settings.AdditionalImports = new System.Collections.Generic.List<ImportProjectItem>
                {
                    new ImportProjectItem { Path = propsPath, Position = position }
                };
                var input = "<Project xmlns=\"" + projectNamespace + "\"><PropertyGroup><Existing>kept</Existing></PropertyGroup></Project>";
                var output = new InsertAdditionalImportFeature().OnGeneratedCSProject(Path.Combine(directory, "Assembly-CSharp.csproj"), input);
                var project = XDocument.Parse(output).Root;
                XNamespace ns = projectNamespace;
                var injectedGroup = project.Elements(ns + "PropertyGroup").Single(x => x.Attribute("Condition") != null);
                Assert.That(injectedGroup.Attribute("Condition").Value, Is.EqualTo("'$(Configuration)' == 'Debug'"));
                Assert.That(injectedGroup.Element(ns + "Nullable").Value, Is.EqualTo("enable"));
                Assert.That(project.Element(ns + "ItemGroup").Element(ns + "AdditionalFiles").Attribute("Include").Value, Is.EqualTo("BannedSymbols.txt"));
                Assert.That(project.Element(ns + "ProjectExtensions").Element(XName.Get("Payload", "urn:extension")).Value, Is.EqualTo("kept"));
                var groups = project.Elements(ns + "PropertyGroup").ToArray();
                Assert.That(groups[position == ImportProjectPosition.PrependContent ? 1 : 0].Element(ns + "Existing").Value, Is.EqualTo("kept"));
            }
            finally
            {
                settings.AdditionalImports = originalImports;
                Directory.Delete(directory, true);
            }
        }
    }
}
