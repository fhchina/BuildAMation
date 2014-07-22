// <copyright file="TextFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XmlUtilities package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed partial class XcodeBuilder
    {
        public object
        Build(
            XmlUtilities.TextFileModule moduleToBuild,
            out bool success)
        {
            var node = moduleToBuild.OwningNode;
            var targetNode = node.ExternalDependents[0];
            var project = this.Workspace.GetProject(targetNode);

            var outputFileLoc = moduleToBuild.Locations[XmlUtilities.TextFileModule.OutputFile];
            var outputFilePath = outputFileLoc.GetSingleRawPath();

            var moduleName = node.ModuleName;
            var fileRef = project.FileReferences.Get(moduleName, PBXFileReference.EType.Text, outputFilePath, project.RootUri);
            var sourcesBuildPhase = project.SourceBuildPhases.Get("MiscSources", moduleName);
            var data = project.BuildFiles.Get(moduleName, fileRef, sourcesBuildPhase);
            if (null == data)
            {
                throw new Opus.Core.Exception("Build file not available");
            }

            var content = moduleToBuild.Content.ToString();
            string shellScriptName = "Writing text file for " + targetNode.UniqueModuleName;
            var shellScriptBuildPhase = project.ShellScriptBuildPhases.Get(shellScriptName, node.ModuleName);
            shellScriptBuildPhase.OutputPaths.Add(outputFilePath);
            foreach (var line in content.Split('\n'))
            {
                var escapedLine = line.Replace("\"", "\\\"");
                shellScriptBuildPhase.ShellScriptLines.Add(System.String.Format("echo \\\"{0}\\\" >> $SCRIPT_OUTPUT_FILE_0", escapedLine));
            }

            // is this a post action?
            {
                // because this is performed AFTER the application, we can directly add to the build phases
                var nativeTarget = targetNode.Data as PBXNativeTarget;
                nativeTarget.BuildPhases.Insert(0, shellScriptBuildPhase);
            }

            success = true;
            return data;
        }
    }
}