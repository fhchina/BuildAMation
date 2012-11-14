// <copyright file="Win32Resource.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder
    {
        public object Build(C.Win32Resource resourceFile, out bool success)
        {
            string resourceFilePath = resourceFile.ResourceFile.AbsolutePath;
            if (!System.IO.File.Exists(resourceFilePath))
            {
                throw new Opus.Core.Exception(System.String.Format("Resource file '{0}' does not exist", resourceFilePath));
            }

            Opus.Core.StringArray inputFiles = new Opus.Core.StringArray();
            inputFiles.Add(resourceFilePath);

            Opus.Core.IModule resourceFileModule = resourceFile as Opus.Core.IModule;
            Opus.Core.BaseOptionCollection resourceFileOptions = resourceFileModule.Options;

            C.Win32ResourceCompilerOptionCollection compilerOptions = resourceFileOptions as C.Win32ResourceCompilerOptionCollection;

            Opus.Core.DependencyNode node = resourceFileModule.OwningNode;
            Opus.Core.Target target = node.Target;

            Opus.Core.StringArray commandLineBuilder = new Opus.Core.StringArray();
            Opus.Core.DirectoryCollection directoriesToCreate = null;
            if (compilerOptions is CommandLineProcessor.ICommandLineSupport)
            {
                CommandLineProcessor.ICommandLineSupport commandLineOption = compilerOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target);

                directoriesToCreate = commandLineOption.DirectoriesToCreate();
            }
            else
            {
                throw new Opus.Core.Exception("Compiler options does not support command line translation");
            }

            // add output path
            commandLineBuilder.Add(System.String.Format("/fo {0}", compilerOptions.CompiledResourceFilePath));

            // NEW STYLE
#if true
            Opus.Core.IToolset toolset = target.Toolset;
            Opus.Core.ITool compilerTool = toolset.Tool(typeof(C.IWinResourceCompilerTool));
#else
            C.Win32ResourceCompilerBase compilerInstance = C.Win32ResourceCompilerFactory.GetTargetInstance(target);
            Opus.Core.ITool compilerTool = compilerInstance as Opus.Core.ITool;
#endif

            string executablePath = compilerTool.Executable(target);

            string recipe = null;
            if (executablePath.Contains(" "))
            {
                recipe += System.String.Format("\"{0}\"", executablePath);
            }
            else
            {
                recipe += executablePath;
            }
            recipe += System.String.Format(" {0} $<", commandLineBuilder.ToString(' '));
            // replace target with $@
            recipe = recipe.Replace(resourceFileOptions.OutputPaths[C.OutputFileFlags.Win32CompiledResource], "$@");

            Opus.Core.StringArray recipes = new Opus.Core.StringArray();
            recipes.Add(recipe);

            string makeFilePath = MakeFileBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFilePath));

            MakeFile makeFile = new MakeFile(node, this.topLevelMakeFilePath);

            MakeFileRule rule = new MakeFileRule(resourceFileOptions.OutputPaths, C.OutputFileFlags.Win32CompiledResource, node.UniqueModuleName, directoriesToCreate, null, inputFiles, recipes);
            makeFile.RuleArray.Add(rule);

            using (System.IO.TextWriter makeFileWriter = new System.IO.StreamWriter(makeFilePath))
            {
                makeFile.Write(makeFileWriter);
            }

            MakeFileTargetDictionary targetDictionary = makeFile.ExportedTargets;
            MakeFileVariableDictionary variableDictionary = makeFile.ExportedVariables;
            Opus.Core.StringArray environmentPaths = null;
            if (compilerTool is Opus.Core.IToolEnvironmentPaths)
            {
                environmentPaths = (compilerTool as Opus.Core.IToolEnvironmentPaths).Paths(target);
            }
            System.Collections.Generic.Dictionary<string, Opus.Core.StringArray> environment = null;
            if (compilerTool is Opus.Core.IToolEnvironmentVariables)
            {
                environment = (compilerTool as Opus.Core.IToolEnvironmentVariables).Variables(target);
            }
            MakeFileData returnData = new MakeFileData(makeFilePath, targetDictionary, variableDictionary, environmentPaths, environment);
            success = true;
            return returnData;
        }
    }
}