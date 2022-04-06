//https://www.bussmann.io/blog/using-stylecop-analyzers-with-unity
namespace Bussmann.RoslynAnalyzers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Csproj asset post processor.
    ///
    /// <para>Applies changes after csproj assets are generated.</para>
    /// </summary>
    public class CsprojAssetPostprocessor : AssetPostprocessor
    {
        private const string NugetFolder = "RoslynAnalyzers";

        /// <summary>
        /// Called on csproj files generated.
        /// </summary>
        public static void OnGeneratedCSProjectFiles()
        {
            try
            {
                string[] lines = GetCsprojLinesInSln();
                string currentDirectory = Directory.GetCurrentDirectory();

                string[] projectFiles = Directory
                    .GetFiles(currentDirectory, "*.csproj")
                    .Where(csprojFile => lines.Any(line => line
                        .Contains("\"" + Path.GetFileName(csprojFile) + "\""))).ToArray();

                foreach (string file in projectFiles)
                {
                    UpdateProjectFile(file);
                }
            }
            catch (Exception error)
            {
                Debug.LogError(error);
            }
        }

        /// <summary>
        /// Gets the post process order.
        /// </summary>
        ///
        /// <returns>The post process order.</returns>
        public override int GetPostprocessOrder()
        {
            return 20;
        }

        /// <summary>
        /// Gets the csproj lines in the current solution.
        /// </summary>
        ///
        /// <returns>The csproj text lines.</returns>
        private static string[] GetCsprojLinesInSln()
        {
            string projectDirectory = Directory.GetParent(Application.dataPath)?.FullName;
            string projectName = Path.GetFileName(projectDirectory);
            string slnFile = Path.GetFullPath($"{projectName}.sln");

            if (!File.Exists(slnFile))
            {
                return new string[0];
            }

            string slnAllText = File.ReadAllText(slnFile);

            string[] lines = slnAllText
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Where(a => a.StartsWith("Project(")).ToArray();

            return lines;
        }

        /// <summary>
        /// Updates a given project file.
        /// </summary>
        ///
        /// <param name="projectFile">The file to process.</param>
        private static void UpdateProjectFile(string projectFile)
        {
            XDocument document;

            try
            {
                document = XDocument.Load(projectFile);
            }
            catch (Exception)
            {
                Debug.LogError($"Failed to Load {projectFile}");

                return;
            }

            XElement projectContentElement = document.Root;

            if (projectContentElement != null)
            {
                XNamespace xmlns = projectContentElement.Name.NamespaceName;
                AddRoslynAnalyzers(projectContentElement, xmlns);
            }

            document.Save(projectFile);
        }

        /// <summary>
        /// Adds the roslyn analyzers to the csproj.
        /// </summary>
        ///
        /// <param name="projectContentElement">The project content element to append.</param>
        /// <param name="xmlns">The xml namespace.</param>
        private static void AddRoslynAnalyzers(XContainer projectContentElement, XNamespace xmlns)
        {
            string currentDirectory = Directory.GetCurrentDirectory();

            DirectoryInfo roslynAnalysersBaseDir = new DirectoryInfo(Path.Combine(currentDirectory, NugetFolder));

            if (!roslynAnalysersBaseDir.Exists)
            {
                Debug.LogError("The nuget package directory could not be found.");
                Debug.Log("Looked for " + roslynAnalysersBaseDir);
                return;
            }

            IEnumerable<string> relativePaths = roslynAnalysersBaseDir
                .GetFiles("*", SearchOption.AllDirectories)
                .Select(x => x.FullName.Substring(currentDirectory.Length + 1));

            XElement itemGroup = new XElement(xmlns + "ItemGroup");

            foreach (string file in relativePaths)
            {
                if (new FileInfo(file).Extension == ".dll")
                {
                    XElement reference = new XElement(xmlns + "Analyzer");
                    reference.Add(new XAttribute("Include", file));
                    itemGroup.Add(reference);
                }

                if (new FileInfo(file).Extension == ".json")
                {
                    XElement reference = new XElement(xmlns + "AdditionalFiles");
                    reference.Add(new XAttribute("Include", file));
                    itemGroup.Add(reference);
                }

                if (new FileInfo(file).Extension == ".ruleset")
                {
                    SetOrUpdateProperty(projectContentElement, xmlns, "CodeAnalysisRuleSet", existing => file);
                }
            }

            projectContentElement.Add(itemGroup);
        }

        /// <summary>
        /// Sets or updates a given property in a given <see cref="XElement"/>.
        /// </summary>
        ///
        /// <param name="root">The root element to update.</param>
        /// <param name="xmlns">The xml namespace.</param>
        /// <param name="name">The property name.</param>
        /// <param name="updater">The updater function.</param>
        private static void SetOrUpdateProperty(XContainer root, XNamespace xmlns, string name, Func<string, string> updater)
        {
            XElement element = root.Elements(xmlns + "PropertyGroup").Elements(xmlns + name).FirstOrDefault();

            if (element != null)
            {
                string result = updater(element.Value);

                if (result == element.Value)
                {
                    return;
                }

                int currentSubDirectoryCount = Regex.Matches(element.Value, "/").Count;
                int newSubDirectoryCount = Regex.Matches(result, "/").Count;

                if (currentSubDirectoryCount != 0 && currentSubDirectoryCount < newSubDirectoryCount)
                {
                    return;
                }

                element.SetValue(result);
            }
            else
            {
                AddProperty(root, xmlns, name, updater(string.Empty));
            }
        }

        /// <summary>
        /// Adds a property to the first property group without a condition.
        /// </summary>
        ///
        /// <param name="root">The root element to add to.</param>
        /// <param name="xmlns">The xml namespace.</param>
        /// <param name="name">The property name.</param>
        /// <param name="content">The property content.</param>
        private static void AddProperty(XContainer root, XNamespace xmlns, string name, string content)
        {
            XElement propertyGroup = root.Elements(xmlns + "PropertyGroup")
                .FirstOrDefault(element => !element.Attributes(xmlns + "Condition").Any());

            if (propertyGroup == null)
            {
                propertyGroup = new XElement(xmlns + "PropertyGroup");

                root.AddFirst(propertyGroup);
            }

            propertyGroup.Add(new XElement(xmlns + name, content));
        }
    }
}
