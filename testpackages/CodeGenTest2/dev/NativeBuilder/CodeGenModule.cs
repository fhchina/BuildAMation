namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        public object Build(CodeGenTest2.CodeGenModule codeGenModule, out bool success)
        {
            Opus.Core.BaseModule codeGenModuleModule = codeGenModule as Opus.Core.BaseModule;
            Opus.Core.DependencyNode node = codeGenModuleModule.OwningNode;
            Opus.Core.Target target = node.Target;
            Opus.Core.BaseOptionCollection codeGenModuleOptions = codeGenModuleModule.Options;
            CodeGenTest2.CodeGenOptionCollection toolOptions = codeGenModuleOptions as CodeGenTest2.CodeGenOptionCollection;
            Opus.Core.ITool tool = target.Toolset.Tool(typeof(CodeGenTest2.ICodeGenTool));

            // dependency checking
            {
                Opus.Core.StringArray inputFiles = new Opus.Core.StringArray();
                inputFiles.Add(tool.Executable((Opus.Core.BaseTarget)target));
                Opus.Core.StringArray outputFiles = codeGenModuleOptions.OutputPaths.Paths;
                if (!RequiresBuilding(outputFiles, inputFiles))
                {
                    Opus.Core.Log.DebugMessage("'{0}' is up-to-date", node.UniqueModuleName);
                    success = true;
                    return null;
                }
            }

            var commandLineBuilder = new Opus.Core.StringArray();
            if (toolOptions is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = toolOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, null);

                Opus.Core.DirectoryCollection directoriesToCreate = commandLineOption.DirectoriesToCreate();
                foreach (string directoryPath in directoriesToCreate)
                {
                    NativeBuilder.MakeDirectory(directoryPath);
                }
            }
            else
            {
                throw new Opus.Core.Exception("CodeGen options does not support command line translation");
            }

            string hostApplication = null;
            if (target.HasPlatform(Opus.Core.EPlatform.OSX))
            {
                hostApplication = "mono";
            }
            int exitCode = CommandLineProcessor.Processor.Execute(node, tool, commandLineBuilder, hostApplication);
            success = (0 == exitCode);

            return null;
        }
    }
}